using LibraryProjectDomain.DTOS.BorrowingDTO;
using LibraryProjectDomain.Models.BorrowingModel;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LibraryProjectDomain.DTOS.BookDTO
{
    public class BookRead
    {
        public Guid Id { get; set; }
        public string Title { get; set; } = string.Empty;
        public string Author { get; set; } = string.Empty;
        public string Category { get; set; } = string.Empty;
        public DateTime Year { get; set; }
        public int CopiesCount { get; set; }
        public ICollection<BorrowingRead> BorrowingReads { get; set; }
    }
}
