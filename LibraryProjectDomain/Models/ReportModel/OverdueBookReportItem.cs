using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LibraryProjectDomain.Models.ReportModel
{
    public class OverdueBookReportItem
    {
        public string BookTitle { get; set; }
        public string MemberName { get; set; }
        public DateTime? DueDate { get; set; }
        public int? DaysLate { get; set; }
        public int? DaysLeft { get; set; }
    }
}
