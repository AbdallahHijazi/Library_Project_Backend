using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LibraryProjectDomain.DTOS.BorrowingDTO
{
    public class BorrowingRead
    {
        public Guid Id { get; set; }
        public Guid MemberId { get; set; }
        public Guid BookId { get; set; }
        public DateTime BorrowDate { get; set; }
        public DateTime? ReturnDate { get; set; }
        public DateTime? ReturnDateActual { get; set; }
        public bool IsActive { get; set; }

    }
}
