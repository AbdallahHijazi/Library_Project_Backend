using LibraryProject;
using LibraryProjectDomain.Models.ReportModel;
using LibraryProjectRepository.Repositories.Reports;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace LibraryProjectAPI.Controllers.ReportsC
{
    [Route("api/[controller]")]
    [ApiController]
    public class ReportsController : ControllerBase
    {
        private readonly LibraryDbContext context;
        private readonly ReportsRepository repository;

        public ReportsController(LibraryDbContext context,
                                 ReportsRepository repository
                                 )
        {
            this.context = context;
            this.repository = repository;
        }

        [HttpGet("dashboard-stats")]
        public async Task<IActionResult> GetDashboardStats()
        {
            var todayUtc = DateTime.UtcNow.Date;

            var totalBooks = await context.Books.CountAsync();
            var totalMembers = await context.Members.CountAsync();

            var activeMembers = await context.Members.CountAsync(m => m.BorrowingsCount > 0);

            var borrowedBooks = await context.Borrowings
                        .CountAsync(br => br.IsActive == true);
            var overdueBooks = await context.Borrowings
                            .CountAsync(br =>br.ReturnDateActual == null && br.IsActive == true && br.ReturnDate< todayUtc);

            return Ok(new
            {
                totalBooks,
                totalMembers,
                activeMembers,
                borrowedBooks,
                overdueBooks
            });
        }

        [HttpGet("most-borrowed-books")]
        public async Task<IActionResult> GetMostBorrowedBooks([FromQuery] int limit = 10)
        {
            var data = await context.Borrowings
                            .AsNoTracking()
                            .Include(br => br.Book)
                            .Where(br => br.Id != null)
                            .GroupBy(br => new { br.BookId, br.Book.Title })
                            .Select(g => new
                            {
                                bookId = g.Key.BookId,
                                title = g.Key.Title,
                                borrowCount = g.Count()
                            })
                            .OrderByDescending(x => x.borrowCount)
                            .Take(limit)
                            .ToListAsync();

            return Ok(data);
        }

        [HttpGet("overdue-books")]
        public async Task<IActionResult> GetOverdueBooks()
        {
            var todayUtc = DateTime.UtcNow.Date;

            var overdue = await context.Borrowings
            .AsNoTracking()
            .Include(r => r.Book)
            .Include(r => r.Member)
            .Where(r => r.ReturnDateActual == null && r.ReturnDate < todayUtc && r.IsActive)
            .Select(r => new
            {
                id = r.Id,
                bookTitle = r.Book.Title,
                memberName = r.Member.FullName,
                dueDate = r.ReturnDate, 
                daysLate = EF.Functions.DateDiffDay(r.ReturnDate, todayUtc) 
            })
            .OrderByDescending(r => r.daysLate)
            .ToListAsync();

            return Ok(overdue);
        }
        [HttpGet("overdue-books/export-pdf")]
        public async Task<IActionResult> ExportOverdueBooksPdf()
        {
            var todayUtc = DateTime.UtcNow.Date;

            var overdueData = await context.Borrowings
                .AsNoTracking()
                .Where(r => r.IsActive)
                .Select(r => new OverdueBookReportItem
                {
                    BookTitle = r.Book.Title,
                    MemberName = r.Member.FullName,
                    DueDate = r.ReturnDate,
                    DaysLate = todayUtc > r.ReturnDate
                                                    ? EF.Functions.DateDiffDay(r.ReturnDate, todayUtc)
                                                    : 0,
                    DaysLeft = todayUtc <= r.ReturnDate
                                                    ? EF.Functions.DateDiffDay(todayUtc, r.ReturnDate)
                                                    : 0
                })
                .OrderByDescending(r => r.DaysLate)
                .ThenBy(x => x.DaysLeft)
                .ToListAsync();

            var pdfBytes = await repository.GenerateOverdueReportAsync(overdueData);

            if (pdfBytes.Length == 0)
                return StatusCode(500, "فشل في توليد تقرير PDF.");

            var fileName = $"تقرير_الكتب_المتأخرة_{DateTime.Now:yyyyMMdd_HHmmss}.pdf";
            return File(pdfBytes, "application/pdf", fileName);
        }

        [HttpGet("active-members")]
        public async Task<IActionResult> GetMostActiveMembers([FromQuery] int limit = 10)
        {
            var data = await context.Borrowings
            .AsNoTracking()
            .Include(br => br.Member)
            .GroupBy(br => new { br.MemberId, br.Member.FullName })
            .Select(g => new
            {
                memberId = g.Key.MemberId,
                memberName = g.Key.FullName,
                borrowCount = g.Count() 
            })
            .OrderByDescending(x => x.borrowCount) 
            .Take(limit) 
            .ToListAsync();

            return Ok(data);
        }
    }
}
