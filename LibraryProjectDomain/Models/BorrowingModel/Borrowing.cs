using LibraryProjectDomain.Models.BookModel;
using LibraryProjectDomain.Models.MembersModel;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LibraryProjectDomain.Models.BorrowingModel
{
    public class Borrowing
    {
        public Borrowing()
        {
            Id = Guid.NewGuid();
        }
        public Guid Id { get; set; }

        public Guid MemberId { get; set; }
        public Member Member { get; set; }
        public Guid BookId { get; set; }
        public Book Book { get; set; }
        public bool IsActive { get; set; }
        public DateTime BorrowDate { get; set; } = DateTime.Now;
        public DateTime? ReturnDate { get; set; } 
        public DateTime? ReturnDateActual { get; set; }
    }
}
