using Microsoft.Playwright;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LibraryProjectRepository.Repositories.Reports
{
    public class PlaywrightHtmlToPdf : IHtmlToPdf
    {
        public async Task<byte[]> RenderAsync(string html, string? baseUrl = null, bool landscape = false)
        {
            using var playwright = await Playwright.CreateAsync();
            await using var browser = await playwright.Chromium.LaunchAsync(new() { Headless = true });

            var context = await browser.NewContextAsync(new()
            {
                Locale = "ar",
                BaseURL = baseUrl
            });
            var page = await context.NewPageAsync();

            await page.SetContentAsync(html, new()
            {
                WaitUntil = WaitUntilState.NetworkIdle
            });

            var pdf = await page.PdfAsync(new()
            {
                Format = "A4",
                Landscape = landscape,
                PrintBackground = true,
                Margin = new() { Top = "15mm", Right = "12mm", Bottom = "15mm", Left = "12mm" }
            });

            return pdf;
        }
    }
}
