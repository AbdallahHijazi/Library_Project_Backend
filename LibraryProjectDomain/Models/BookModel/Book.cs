using LibraryProjectDomain.Models.BorrowingModel;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LibraryProjectDomain.Models.BookModel
{
    public class Book
    {
        public Book()
        {
            Id = Guid.NewGuid();
        }
        public Guid Id { get; set; }
        public string Title { get; set; } = string.Empty;
        public string Author { get; set; } = string.Empty;
        public string Category { get; set; } = string.Empty;
        public DateTime Year { get; set; }
        public int CopiesCount { get; set; }
        public ICollection<Borrowing> Borrowings { get; set; }

    }
}
