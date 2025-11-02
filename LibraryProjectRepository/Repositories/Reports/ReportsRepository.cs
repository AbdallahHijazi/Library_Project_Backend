using LibraryProjectDomain.Models.ReportModel;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Net;
using System.Text;
using Wkhtmltopdf.NetCore;
using Wkhtmltopdf.NetCore;
using Options = Wkhtmltopdf.NetCore.Options;

namespace LibraryProjectRepository.Repositories.Reports
{
    public class ReportsRepository
    {
        private readonly ILogger<ReportsRepository> logger;
        private readonly IHtmlToPdf htmlToPdf;

        public ReportsRepository(ILogger<ReportsRepository> logger, IHtmlToPdf htmlToPdf)
        {
            this.logger = logger;
            this.htmlToPdf = htmlToPdf;
        }
        public async Task<byte[]> GenerateOverdueReportAsync(IEnumerable<OverdueBookReportItem> overdueData)
        {
            var html = new StringBuilder();
            html.AppendLine("<!DOCTYPE html>");
            html.AppendLine("<html lang='ar' dir='rtl'><head>");
            html.AppendLine("<meta charset='UTF-8'>");
            html.AppendLine("<meta http-equiv='X-UA-Compatible' content='IE=edge'/>");
            html.AppendLine("<style>");
            html.AppendLine("@font-face{ font-family:'Amiri'; src:url('fonts/Amiri-Regular.ttf') format('truetype'); }");
            html.AppendLine("body { font-family: 'Amiri','Arial',sans-serif; direction: rtl; font-size: 12px; }");
            html.AppendLine("h1 { color: #dc3545; text-align: right; margin: 0 0 8px; }");
            html.AppendLine("p.meta { margin: 4px 0 12px; }");
            html.AppendLine("table { width: 100%; border-collapse: collapse; margin-top: 8px; text-align: right; }");
            html.AppendLine("th, td { border: 1px solid #dee2e6; padding: 8px; }");
            html.AppendLine("th { background-color: #f8f9fa; }");
            html.AppendLine(".late-days { color: #dc3545; font-weight: bold; }");
            html.AppendLine("</style></head><body>");

            html.AppendLine("<h1>تقرير سجلات الاستعارة </h1>");
            html.AppendLine($"<p class='meta'>تاريخ التقرير: {DateTime.Now:yyyy-MM-dd}</p>");

            html.AppendLine("<table><thead><tr>");
            html.AppendLine("<th>الكتاب</th><th>العضو</th><th>تاريخ الاستحقاق</th><th>أيام التأخير</th>");
            html.AppendLine("</tr></thead><tbody>");

            foreach (var item in overdueData)
            {
                var due = item.DueDate.HasValue
                    ? item.DueDate.Value.ToString("yyyy-MM-dd", CultureInfo.InvariantCulture)
                    : "—";
                var days = item.DaysLate ?? 0;

                html.AppendLine("<tr>");
                html.AppendLine($"<td>{WebUtility.HtmlEncode(item.BookTitle)}</td>");
                html.AppendLine($"<td>{WebUtility.HtmlEncode(item.MemberName)}</td>");
                html.AppendLine($"<td>{due}</td>");
                html.AppendLine($"<td class='late-days'>{days} يوم</td>");
                html.AppendLine("</tr>");
            }

            html.AppendLine("</tbody></table>");
            html.AppendLine("</body></html>");

            try
            {
                var baseUrl = "file:///" + Path.Combine(Directory.GetCurrentDirectory(), "wwwroot").Replace("\\", "/") + "/";
                var bytes = await htmlToPdf.RenderAsync(html.ToString(), baseUrl: baseUrl, landscape: false);
                return bytes;
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "Failed to generate PDF report using Playwright.");
                return Array.Empty<byte>();
            }
        }
    }
    public interface IHtmlToPdf
    {
        Task<byte[]> RenderAsync(string html, string? baseUrl = null, bool landscape = false);
    }
}




