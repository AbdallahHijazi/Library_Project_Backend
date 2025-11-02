using LibraryProject;
using LibraryProjectDomain.DTOS.BorrowingDTO;
using LibraryProjectDomain.Models.BorrowingModel;
using LibraryProjectRepository.Repositories.Borrowings;
using LibraryProjectRepository.SheardRepository;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using OfficeOpenXml;

namespace LibraryProjectAPI.Controllers.BorrowingC
{
    [Route("api/[controller]")]
    [ApiController]
    public class BorrowingController : ControllerBase
    {
        private readonly LibraryDbContext context;
        private readonly IRepository<Borrowing> irepository;
        private readonly BorrowingRepository repository;

        public BorrowingController(LibraryDbContext context,
                                   IRepository<Borrowing> irepository,
                                   BorrowingRepository repository)
        {
            this.context = context;
            this.irepository = irepository;
            this.repository = repository;
        }
        
        [HttpGet]
        public async Task<IActionResult> GetBorrowings(
                                                     [FromQuery] int page = 1,
                                                     [FromQuery] int pageSize = 10,
                                                     [FromQuery] string? memberName = null,
                                                     [FromQuery] DateTime? returnDate = null,
                                                     [FromQuery] bool exportToExcel = false)
        {
            var borrowings = await repository.GetAllBorrowings(page, pageSize, memberName, returnDate, exportToExcel);

            if (exportToExcel)
            {
                using var package = new ExcelPackage();
                var sheet = package.Workbook.Worksheets.Add("Borrowings");

                sheet.Cells[1, 1].Value = "Member Name";
                sheet.Cells[1, 2].Value = "Book Title";
                sheet.Cells[1, 3].Value = "Borrow Date";
                sheet.Cells[1, 4].Value = "Return Date";

                for (int i = 0; i < borrowings.Items.Count; i++)
                {
                    var b = borrowings.Items[i];
                    sheet.Cells[i + 2, 1].Value = b.MemberName;
                    sheet.Cells[i + 2, 2].Value = b.BookTitle;
                    sheet.Cells[i + 2, 3].Value = b.BorrowDate.ToString("yyyy-MM-dd");
                    sheet.Cells[i + 2, 4].Value = b.ReturnDate?.ToString("yyyy-MM-dd");
                }

                var stream = new MemoryStream(package.GetAsByteArray());
                string excelName = $"Borrowings_{DateTime.Now:yyyyMMddHHmmss}.xlsx";

                return File(stream, "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet", excelName);
            }

            if (borrowings == null || !borrowings.Items.Any())
                return NotFound("No borrowings found.");

            return Ok(borrowings);
        }

        [HttpGet("{id}")]
        public async Task<IActionResult> GetBorrowing(Guid id)
        {
            var borrowing = await repository.GetBorrowing(id);
            if (borrowing == null)
                return NotFound($"Borrowing not found with id {id}");

            return Ok(borrowing);
        }
        [HttpGet("end")]
        public async Task<IActionResult> GetBorrowing()
        {
            var borrowings = await repository.GetAllBorrowingsEnd();
            if (borrowings == null)
                return NotFound($"Borrowing not found with ");

            return Ok(borrowings);
        }
        [HttpPost]
        public async Task<IActionResult> CreateBorrowing(BorrowingOperation create)
        {
            try
            {
                if (!ModelState.IsValid)
                    return ValidationProblem(ModelState);

                var newBorrowing = await repository.AddBorrowing(create);
                if (newBorrowing == null)
                    return BadRequest(new { message = "بيانات غير صالحة." });
                await context.SaveChangesAsync();

                return CreatedAtAction(nameof(GetBorrowing), new { id = newBorrowing.Id }, newBorrowing);
            }
            catch (InvalidOperationException ex)
            {
                return BadRequest(new { message = ex.Message });
            }
            catch (ArgumentException ex)
            {
                return NotFound(new { message = ex.Message });
            }
            catch (Exception)
            {
                return StatusCode(500, new { message = "حدث خطأ غير متوقع." });
            }
        }

        [HttpPut("{id}")]
        public async Task<IActionResult> UpdateBorrowing(Guid id, BorrowingOperation update)
        {
            if (!ModelState.IsValid)
                return ValidationProblem(ModelState);
            try
            {
                var borrowing = await repository.UpdateBorrowing(id, update);
                await context.SaveChangesAsync();

                return Ok(borrowing);
            }
            catch (ArgumentException)
            {
                return NotFound($"Borrowing not found with id {id}");
            }
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteBorrowing(Guid id)
        {
            var borrowing = await repository.DeleteBorrowing(id);
            if (!borrowing)
                return NotFound($"Borrowing not found with id {id}");

            await context.SaveChangesAsync();
            return NoContent();
        }
    }
}
