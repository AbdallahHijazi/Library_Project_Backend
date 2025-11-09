using LibraryProject;
using LibraryProjectDomain.DTOS.BookDTO;
using LibraryProjectDomain.DTOS.BorrowingDTO;
using LibraryProjectDomain.DTOS.MemberDTO;
using LibraryProjectDomain.Models.BookModel;
using LibraryProjectDomain.Models.BorrowingModel;
using LibraryProjectDomain.Models.MembersModel;
using LibraryProjectRepository.SheardRepository;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LibraryProjectRepository.Repositories.Members
{
    public class MemberRepository : GinericRepository<Member>
    {
        private readonly LibraryDbContext context;

        public MemberRepository(LibraryDbContext context) : base(context)
        {
            this.context = context;
        }
        public async Task<List<MemberRead>> GetAllMembersWithBorrowings()
        {
            var members = await context.Members
                                            .Include(m => m.Borrowings.Where(b => b.IsActive))
                                            .ThenInclude(b => b.Book)
                                            .ToListAsync();
            if (members==null)
                return null;
            var membersRead = members.Select(m => new MemberRead
            {
                Id = m.Id,
                FullName = m.FullName,
                Email = m.Email,
                PhoneNumber = m.PhoneNumber,
                RegistrationDate = m.RegistrationDate,
                BorrowingsCount = m.BorrowingsCount,
                Borrowings = m.Borrowings.Select(b => new BorrowingRead
                {
                    Id = b.Id,
                    MemberId = b.MemberId,
                    BookId = b.BookId,
                    BorrowDate = b.BorrowDate,
                    ReturnDate = b.ReturnDate,
                    IsActive = b.IsActive
                }).ToList()
            }).ToList();
            return membersRead;
        }
        public async Task<MemberRead?> GetMemberWithBorrowings(Guid id)
        {
            var member= await context.Members
                                        .Include(m => m.Borrowings.Where(b => b.IsActive))
                                        .ThenInclude(b => b.Book)
                                        .FirstOrDefaultAsync(m => m.Id == id);
            if (member == null)
                return null;
            var membersRead = new MemberRead
            {
                Id = member.Id,
                FullName = member.FullName,
                Email = member.Email,
                PhoneNumber = member.PhoneNumber,
                RegistrationDate = member.RegistrationDate,
                BorrowingsCount = member.BorrowingsCount,
                Borrowings = member.Borrowings.Select(b => new BorrowingRead
                {
                    Id = b.Id,
                    MemberId = b.MemberId,
                    BookId = b.BookId,
                    BorrowDate = b.BorrowDate,
                    ReturnDate = b.ReturnDate,
                    IsActive = b.IsActive
                }).ToList()
            };
            return membersRead;
        }
        public async Task<Member> AddMember(MemberOpartion create)
        {
            if (create == null)
                return null;
            var member = new Member
            {
                FullName = create.FullName,
                Email = create.Email,
                PhoneNumber = create.PhoneNumber,
                RegistrationDate = DateTime.Now
            }; 
            context.Members.Add(member);
            await context.SaveChangesAsync();
            return member;
        }
        public async Task<Member?> UpdateMember(Guid id, MemberOpartion update)
        {
            var member = await context.Members
                               .FirstOrDefaultAsync(b => b.Id == id);
            if (member == null)
                return null;
                member.FullName = update.FullName;
                member.Email = update.Email;
                member.PhoneNumber = update.PhoneNumber;
                member.RegistrationDate = DateTime.Now;

            context.Members.Update(member);
            await context.SaveChangesAsync();

            return member;
        }
        
        public async Task<bool> DeleteMember(Guid memberId)
        {
            var member = await context.Members
                .Include(m => m.Borrowings) 
                .FirstOrDefaultAsync(m => m.Id == memberId);

            if (member == null)
                return false;

            if (member.Borrowings != null && member.Borrowings.Any())
            {
                throw new InvalidOperationException("لا يمكن حذف العضو لأنه يمتلك استعارات حالية/مفتوحة مرتبطة به.");
            }

            var tx = await context.Database.BeginTransactionAsync();
            try
            {
                context.Members.Remove(member);
                await context.SaveChangesAsync();
                await tx.CommitAsync();

                return true;
            }
            catch (Exception)
            {
                await tx.RollbackAsync();
                throw;
            }
        }
    }
}
