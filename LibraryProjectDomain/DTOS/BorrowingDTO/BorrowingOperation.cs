using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LibraryProjectDomain.DTOS.BorrowingDTO
{
    public class BorrowingOperation
    {
        [Required]
        public Guid MemberId { get; set; }

        [Required]
        public Guid BookId { get; set; }
        public DateTime? BorrowDate { get; set; }
        public DateTime? ReturnDate { get; set; }
        public bool IsActive { get; set; }= true;
    }
}
