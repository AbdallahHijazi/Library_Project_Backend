using DocumentFormat.OpenXml.Office2010.Excel;
using LibraryProject;
using LibraryProjectDomain.DTOS.BorrowingDTO;
using LibraryProjectDomain.DTOS.MemberDTO;
using LibraryProjectDomain.Models.BorrowingModel;
using LibraryProjectDomain.Models.MembersModel;
using LibraryProjectDomain.Models.Sheard;
using LibraryProjectRepository.SheardRepository;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LibraryProjectRepository.Repositories.Borrowings
{
    public class BorrowingRepository : GinericRepository<Borrowing>
    {
        private readonly LibraryDbContext context;

        public BorrowingRepository(LibraryDbContext context) : base(context)
        {
            this.context = context;
        }
        public async Task<PagedResult<BorrowingReadExtended>> GetAllBorrowings(
                                                         int page = 1,
                                                         int pageSize = 10,
                                                         string? memberName = null,
                                                         DateTime? returnDate = null,
                                                         bool exportToExcel = false)
        {
            if (page <= 0) page = 1;
            if (pageSize <= 0) pageSize = 10;
            const int MaxPageSize = 100;
            pageSize = Math.Min(pageSize, MaxPageSize);

            var query = context.Borrowings
                               .Include(b => b.Member)
                               .Include(b => b.Book)
                               .Where(b => b.IsActive)
                               .AsQueryable();

            if (!string.IsNullOrWhiteSpace(memberName))
                query = query.Where(b => b.Member.FullName.Contains(memberName));

            if (returnDate.HasValue)
                query = query.Where(b => b.ReturnDate.HasValue && b.ReturnDate.Value.Date == returnDate.Value.Date);

            var totalCount = await query.CountAsync();

            List<BorrowingReadExtended> borrowings;

            if (exportToExcel)
            {
                borrowings = await query
                    .Select(b => new BorrowingReadExtended
                    {
                        Id = b.Id,
                        MemberId = b.MemberId,
                        MemberName = b.Member.FullName,
                        BookId = b.BookId,
                        BookTitle = b.Book.Title,
                        BorrowDate = b.BorrowDate,
                        ReturnDate = b.ReturnDate
                    })
                    .ToListAsync();

                return new PagedResult<BorrowingReadExtended>
                {
                    Items = borrowings,
                    Page = 1,
                    PageSize = totalCount,
                    TotalCount = totalCount,
                   
                };
            }
            else
            {
                borrowings = await query
                    .OrderBy(b => b.BorrowDate)
                    .Skip((page - 1) * pageSize)
                    .Take(pageSize)
                    .Select(b => new BorrowingReadExtended
                    {
                        Id = b.Id,
                        MemberId = b.MemberId,
                        MemberName = b.Member.FullName,
                        BookId = b.BookId,
                        BookTitle = b.Book.Title,
                        BorrowDate = b.BorrowDate,
                        ReturnDate = b.ReturnDate,
                        IsActive = b.IsActive
                    })
                    .ToListAsync();
            }

            return new PagedResult<BorrowingReadExtended>
            {
                Items = borrowings,
                Page = page,
                PageSize = pageSize,
                TotalCount = totalCount
            };
        }
        public async Task<BorrowingRead?> GetBorrowing(Guid id)
        {
            var borrowing = await context.Borrowings
                                            .Include(b => b.Member)
                                            .Include(b => b.Book)
                                            .Where(b => b.IsActive)
                                            .FirstOrDefaultAsync(b => b.Id == id);
            if (borrowing.IsActive == false)
                return null;
            var borrowingRead = new BorrowingRead
            {
                Id = borrowing.Id,
                MemberId = borrowing.MemberId,
                BookId = borrowing.BookId,
                BorrowDate = borrowing.BorrowDate,
                ReturnDate = borrowing.ReturnDate,
                IsActive = borrowing.IsActive,
                ReturnDateActual =borrowing.ReturnDateActual
            };
            return borrowingRead;
        }
        public async Task<Borrowing> AddBorrowing(BorrowingOperation create)
        {
            if (create == null)
                return null;

            DateTime finalBorrowDate = create.BorrowDate ?? DateTime.Now;
            DateTime finalReturnDate = create.ReturnDate ?? finalBorrowDate.AddDays(10);

            if (finalReturnDate < finalBorrowDate)
            {
                throw new ArgumentException("The return date cannot be before the borrowing date.");
            }

            var tx = await context.Database.BeginTransactionAsync();
            try
            {
                var book = await context.Books.FirstOrDefaultAsync(b => b.Id == create.BookId);
                if (book == null)
                    throw new ArgumentException("This book is not available");

                if (create.IsActive == true)
                {
                    if (book.CopiesCount <= 0)
                        throw new InvalidOperationException("This book cannot be borrowed because it is currently unavailable");

                    var hasActiveBorrow = await context.Borrowings
                                                        .AnyAsync(
                                                            b => b.MemberId == create.MemberId
                                                              && b.BookId == create.BookId
                                                              && b.ReturnDateActual == null 
                                                        );
                    if (hasActiveBorrow)
                        throw new InvalidOperationException("You cannot borrow the same book before returning it");

                    var activeCount = await context.Borrowings
                                            .CountAsync(b => b.MemberId == create.MemberId && b.ReturnDateActual == null && b.IsActive == true);
                    if (activeCount >= 2)
                        throw new InvalidOperationException("You may not borrow more than two books at a time.");
                }


                var borrowing = new Borrowing
                {
                    MemberId = create.MemberId,
                    BookId = create.BookId,
                    BorrowDate = finalBorrowDate,
                    ReturnDate = finalReturnDate,
                    IsActive = create.IsActive
                };

                if (borrowing.IsActive == true)
                {
                    book.CopiesCount -= 1;
                }

                context.Borrowings.Add(borrowing);
                await context.SaveChangesAsync();

                var member = await context.Members.FindAsync(create.MemberId);
                if (member != null && borrowing.IsActive == true)
                {
                    member.BorrowingsCount++;
                    await context.SaveChangesAsync();
                }
                await tx.CommitAsync();
                return borrowing;
            }
            catch (Exception)
            {
                await tx.RollbackAsync();
                throw;
            }
        }
        public async Task<Borrowing?> UpdateBorrowing(Guid id, BorrowingOperation update)
        {
            var borrowing = await context.Borrowings
                               .FirstOrDefaultAsync(b => b.Id == id);
            if (borrowing == null)
                return null;
            borrowing.MemberId = update.MemberId;
            borrowing.BookId = update.BookId;
            borrowing.BorrowDate = DateTime.Now;
            borrowing.ReturnDate = update.ReturnDate;
            borrowing.IsActive = update.IsActive;

            context.Borrowings.Update(borrowing);
            await context.SaveChangesAsync();

            return borrowing;
        }
        public async Task<bool> DeleteBorrowing(Guid borrowingId)
        {
            var tx = await context.Database.BeginTransactionAsync();
            try
            {
                var borrowing = await context.Borrowings
                .FirstOrDefaultAsync(b => b.Id == borrowingId);

                if (borrowing == null)
                    return false;

                var book = await context.Books.FirstOrDefaultAsync(b => b.Id == borrowing.BookId);
                if (book != null)
                {
                    book.CopiesCount += 1;
                }

                borrowing.IsActive = false;
                context.Borrowings.Update(borrowing);

                var member = await context.Members.FindAsync(borrowing.MemberId);
                if (member != null)
                {
                    member.BorrowingsCount--;
                }

                await context.SaveChangesAsync();
                await tx.CommitAsync();
                borrowing.ReturnDateActual = DateTime.UtcNow;
                return true;
            }
            catch (Exception)
            {
                await tx.RollbackAsync();
                throw;
            }
        }
        public async Task<List<BorrowingReadExtended>> GetAllBorrowingsEnd()
        {
            var query = context.Borrowings
                .Include(b => b.Member)
                .Include(b => b.Book)
                .Where(b => b.IsActive == false) 
                .OrderBy(b => b.BorrowDate)
                .AsQueryable();
            var borrowings = await query
                .Select(b => new BorrowingReadExtended
                {
                    Id = b.Id,
                    MemberId = b.MemberId,
                    MemberName = b.Member.FullName,
                    BookId = b.BookId,
                    BookTitle = b.Book.Title,
                    BorrowDate = b.BorrowDate,
                    ReturnDate = b.ReturnDate,
                    ReturnDateActual = b.ReturnDateActual,
                    IsActive = b.IsActive
                })
                .ToListAsync();
            return borrowings;
        }
    }
}
