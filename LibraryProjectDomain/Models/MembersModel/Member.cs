using LibraryProjectDomain.Models.BorrowingModel;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LibraryProjectDomain.Models.MembersModel
{
    public class Member
    {
        public Member()
        {
            Id = Guid.NewGuid();
            Borrowings = new List<Borrowing>();
            BorrowingsCount = 0;
        }
        public Guid Id { get; set; }
        public string FullName { get; set; } = string.Empty;
        [EmailAddress]
        public string Email { get; set; } = string.Empty;
        [Phone]
        public string PhoneNumber { get; set; } = string.Empty;
        public DateTime RegistrationDate { get; set; } = DateTime.Now;
        public ICollection<Borrowing> Borrowings { get; set; }
        public int BorrowingsCount { get; set; }
    }
}
