using LibraryProject;
using LibraryProjectDomain.DTOS.BookDTO;
using LibraryProjectDomain.DTOS.BorrowingDTO;
using LibraryProjectDomain.Models.BookModel;
using LibraryProjectDomain.Models.Sheard;
using LibraryProjectRepository.SheardRepository;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LibraryProjectRepository.Repositories.Books
{
    public class BookRepository : GinericRepository<Book>
    {
        private readonly LibraryDbContext context;

        public BookRepository(LibraryDbContext context) : base(context)
        {
            this.context = context;
        }
        public async Task<PagedResult<BookRead>> GetAllBooks(
                                                            int page = 1, int pageSize = 10,
                                                            string? category = null,
                                                            int? minCopies = null,
                                                            int? maxCopies = null,
                                                            bool exportToExcel = false)
        {
            if (page <= 0) page = 1;
            if (pageSize <= 0) pageSize = 10;
            const int MaxPageSize = 100;
            pageSize = Math.Min(pageSize, MaxPageSize);

            var query =  context.Books
                                .Include(b => b.Borrowings.Where(b => b.IsActive))     
                                .ThenInclude(b => b.Member)
                                .Where(b =>
                                    (string.IsNullOrWhiteSpace(category) || b.Category.Contains(category)) &&
                                    (!minCopies.HasValue || b.CopiesCount >= minCopies.Value) &&
                                    (!maxCopies.HasValue || b.CopiesCount <= maxCopies.Value))
                                .OrderBy(b => b.Title) 
                                .AsQueryable();

            var totalCount = await query.CountAsync();
            List<BookRead> books;

            if (exportToExcel)
            {
                books = await query
                            .Select(b => new BookRead
                            {
                                Id = b.Id,
                                Title = b.Title,
                                Author = b.Author,
                                Category = b.Category,
                                Year = b.Year,
                                CopiesCount = b.CopiesCount,
                                BorrowingReads = b.Borrowings.Where(br => br.IsActive).Select(br => new BorrowingRead
                                {
                                    Id = br.Id,
                                    MemberId = br.MemberId,
                                    BookId = br.BookId,
                                    BorrowDate = br.BorrowDate,
                                    ReturnDate = br.ReturnDate,
                                    IsActive = br.IsActive
                                }).ToList()
                            })
                            .ToListAsync();

                return new PagedResult<BookRead>
                {
                    Items = books,
                    Page = 1,
                    PageSize = totalCount,
                    TotalCount = totalCount
                };
            }
            else
            {
                books = await query
                            .Skip((page - 1) * pageSize)
                            .Take(pageSize)
                            
                            .Select(b => new BookRead
                            {
                                Id = b.Id,
                                Title = b.Title,
                                Author = b.Author,
                                Category = b.Category,
                                Year = b.Year,
                                CopiesCount = b.CopiesCount,
                                BorrowingReads = b.Borrowings.Where(br => br.IsActive).Select(br => new BorrowingRead
                                {
                                    Id = br.Id,
                                    MemberId = br.MemberId,
                                    BookId = br.BookId,
                                    BorrowDate = br.BorrowDate,
                                    ReturnDate = br.ReturnDate, 
                                    IsActive = br.IsActive
                                }).ToList()
                            })
                            .ToListAsync();
            }
            if (books == null)
            {
                return null;
            }
            var booksRead = new PagedResult<BookRead>
            {
                Items = books,
                Page = page,
                PageSize = pageSize,
                TotalCount = totalCount
            };

            return booksRead;
        }
        public async Task<BookRead> GetBookById(Guid id)
        {
            var book = await context.Books
                                     .Include(b => b.Borrowings.Where(b => b.IsActive))   
                                     .ThenInclude(b => b.Member)
                                    .FirstOrDefaultAsync(b => b.Id == id);
            if (book == null)
            {
                return null;
            }
            var bookRead = new BookRead
            {
                Id = book.Id,
                Title = book.Title,
                Author = book.Author,
                Category = book.Category,
                Year = book.Year,
                CopiesCount = book.CopiesCount,
                BorrowingReads = book.Borrowings.Select(br => new BorrowingRead
                {
                    Id = br.Id,
                    MemberId = br.MemberId,
                    BookId = br.BookId,
                    BorrowDate = br.BorrowDate,
                    ReturnDate = br.ReturnDate,
                    IsActive = br.IsActive
                }).ToList()
            };
            return bookRead;
        }
        public async Task<Book> CreateBook(BookOpartion create)
        {
            if (create == null)
                return null;

            var book = new Book
            {
                Title = create.Title,
                Author = create.Author,
                Category = create.Category,
                Year = create.Year,
                CopiesCount = create.CopiesCount
            };

            context.Books.Add(book);
            await context.SaveChangesAsync();
            return book;
        }
        public async Task<Book?> UpdateBook(Guid id, BookOpartion update)
        {
            var book = await context.Books
                               .FirstOrDefaultAsync(b => b.Id == id);
            if (book == null)
                return null;

            book.Title = update.Title;
            book.Author = update.Author;
            book.Category = update.Category;
            book.Year = update.Year;
            book.CopiesCount = update.CopiesCount;

            context.Books.Update(book);
            await context.SaveChangesAsync();

            return book;
        }
        public async Task<bool> DeleteBook(Guid bookId)
        {
            var book = await context.Books.FirstOrDefaultAsync(b => b.Id == bookId);
            if (book == null)
                return false;
            var hasActiveBorrowings = await context.Borrowings
                                                        .AnyAsync(b =>
                                                            b.BookId == bookId &&
                                                            b.IsActive);
            if (hasActiveBorrowings)
            {
                throw new InvalidOperationException("Cannot delete the book because it has active borrowings.");
            }
            context.Books.Remove(book);
            await context.SaveChangesAsync();
            return true;
        }
    }
}
