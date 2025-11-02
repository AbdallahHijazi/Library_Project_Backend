using LibraryProject;
using LibraryProjectDomain.DTOS.BookDTO;
using LibraryProjectDomain.Models.BookModel;
using LibraryProjectRepository.Repositories.Books;
using LibraryProjectRepository.SheardRepository;
using LibraryProjectSecurity;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Mvc;
using OfficeOpenXml;
using System.Text.Json;

namespace LibraryProjectAPI.Controllers.BookC
{
    [Route("api/[controller]")]
    [ApiController]
    public class BookController : ControllerBase
    {
        private readonly LibraryDbContext context;
        private readonly IRepository<Book> irepository;
        private readonly BookRepository repository;

        public BookController(LibraryDbContext context,
                              IRepository<Book> irepository,
                              BookRepository repository)
        {
            this.context = context;
            this.irepository = irepository;
            this.repository = repository;
        }
        //[Permission("Book.get")]
        [HttpGet]
        public async Task<IActionResult> GetBooks([FromQuery] int page = 1,
                                                  [FromQuery] int pageSize = 10,
                                                  [FromQuery] string? category = null,
                                                  [FromQuery] int? minCopies = null,
                                                  [FromQuery] int? maxCopies = null,
                                                  [FromQuery] bool exportToExcel = false)
        {
            var books = await repository.GetAllBooks(page, pageSize, category, minCopies, maxCopies, exportToExcel);
            if (exportToExcel)
            {
                using var package = new ExcelPackage();
                var sheet = package.Workbook.Worksheets.Add("Books");

                sheet.Cells[1, 1].Value = "Title";
                sheet.Cells[1, 2].Value = "Author";
                sheet.Cells[1, 3].Value = "Category";
                sheet.Cells[1, 4].Value = "Year";
                sheet.Cells[1, 5].Value = "CopiesCount";

                for (int i = 0; i < books.Items.Count; i++)
                {
                    var b = books.Items[i];
                    sheet.Cells[i + 2, 1].Value = b.Title;
                    sheet.Cells[i + 2, 2].Value = b.Author;
                    sheet.Cells[i + 2, 3].Value = b.Category;
                    sheet.Cells[i + 2, 4].Value = b.Year.ToString("yyyy-MM-dd");
                    sheet.Cells[i + 2, 5].Value = b.CopiesCount;
                }

                var stream = new MemoryStream(package.GetAsByteArray());
                string excelName = $"Books_{DateTime.Now:yyyyMMddHHmmss}.xlsx";

                return File(stream, "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet", excelName);
            }
            if (books == null)
            {
                return NotFound("There are no books in the database.");
            }
            return Ok(books);
        }

        //[Permission("Book.get")]
        [HttpGet("{id}", Name = "GetBook")]
        public async Task<IActionResult> GetBookById(Guid id)
        {
            var book = await repository.GetBookById(id);
            if (book == null)
            {
              return  NotFound("There is no book with this name");
            }
            return Ok(book);
        }

        //[Permission("Book.Create")]
        [HttpPost]
        public async Task<IActionResult> CreateBook(BookOpartion create)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);
            var newBook = await repository.CreateBook(create);
            return CreatedAtAction(nameof(GetBookById), new { id = newBook.Id }, newBook);
        }

        [Permission("Book.Update")]
        [HttpPut("{id}")]
        public async Task<IActionResult> UpdateBook(Guid id,BookOpartion update)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            var book = await repository.UpdateBook(id, update);
            if (book == null)
                return NotFound("This book cannot be modified because it does not exist");

            return Ok(book);
        }

        //[Permission("Book.Delete")]
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteBook(Guid id)
        {
            try
            {
                var book = await repository.DeleteBook(id);
                if (!book)
                    return NotFound("There is no book with this name or it has been deleted previously");
                return NoContent();
            }
            catch (InvalidOperationException ex)
            {
                if (ex.Message.Contains("active borrowings"))
                {
                    return Conflict(ex.Message); 
                }
                return StatusCode(500, "An unexpected error occurred during deletion.");
            }
        }

    }
}
