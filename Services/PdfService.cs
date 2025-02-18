using PuppeteerSharp;
using System;
using System.IO;
using System.Threading.Tasks;
using System.Linq;
using System.Text;
using InvoiceData;
using PuppeteerSharp.Media;

namespace GiddhTemplate.Services
{
    public class PdfService
    {
        private readonly RazorTemplateService _razorTemplateService;
        private readonly Lazy<(string Common, string Header, string Footer, string Body, string BackgroundStyles)> _styles;
        private string _openSansFontCSS = ""; // Cache the Open Sans CSS
        private string _openRobotoFontCSS = ""; // Cache the Roboto CSS
        public class TemplateResult // Or a struct if appropriate
        {
            public bool RepeatHeaderFooter { get; set; }
            public string? Header { get; set; }
            public string? Body { get; set; }  
            public string? Footer { get; set; }
        }
        public PdfService()
        {
            _razorTemplateService = new RazorTemplateService();
            string templatePath = Path.Combine(Directory.GetCurrentDirectory(), "Templates", "Tally");
            _styles = new Lazy<(string, string, string, string, string)>(() => LoadStyles(templatePath));
        }

        private string LoadFileContent(string filePath)
        {
            return File.Exists(filePath) ? File.ReadAllText(filePath) : string.Empty;
        }

        private (string Common, string Header, string Footer, string Body, string Background) LoadStyles(string basePath)
        {
            return (
                LoadFileContent(Path.Combine(basePath, "Styles", "Styles.css")),
                LoadFileContent(Path.Combine(basePath, "Styles", "Header.css")),
                LoadFileContent(Path.Combine(basePath, "Styles", "Footer.css")),
                LoadFileContent(Path.Combine(basePath, "Styles", "Body.css")),
                LoadFileContent(Path.Combine(basePath, "Styles", "Background.css"))
            );
        }

        private async Task<string> RenderTemplate(string templatePath, Root request)
        {
            return await _razorTemplateService.RenderTemplateAsync(templatePath, request);
        }

        private TemplateResult CreatePdfDocument(string header, string body, string footer, string commonStyles, string headerStyles, string footerStyles, string bodyStyles, Root request, string backgroundStyles)
        {
            // Dynamic Theme
            var themeCSS = new StringBuilder();
            if (request?.Theme?.Font?.Family == "Open Sans")
            {
                themeCSS.Append(LoadOpenSansFontCSS());
            }
            else if (request?.Theme?.Font?.Family == "Roboto")
            {
                themeCSS.Append(LoadRobotoFontCSS());
            }

            themeCSS.Append("html, body {");
            themeCSS.Append($"--font-family: \"{request?.Theme?.Font?.Family}\";");
            themeCSS.Append($"--font-size-default: {request?.Theme?.Font?.FontSizeDefault}px;");
            themeCSS.Append($"--font-size-large: {request?.Theme?.Font?.FontSizeDefault + 4}px;");
            themeCSS.Append($"--font-size-small: {request?.Theme?.Font?.FontSizeSmall}px;");
            themeCSS.Append($"--font-size-medium: {request?.Theme?.Font?.FontSizeMedium}px;");
            themeCSS.Append($"--color-primary: {request?.Theme?.PrimaryColor};");
            themeCSS.Append($"--color-secondary: {request?.Theme?.SecondaryColor};");
            themeCSS.Append("}");

            var allStyles = $"{commonStyles}{headerStyles}{bodyStyles}{footerStyles}{themeCSS.ToString()}"; // Combine all styles
            if (request?.TemplateType?.ToUpper() == "TALLY" && request?.ShowSectionsInline == true)
            {
                Console.WriteLine($"<html> <head> <style>{allStyles}</style></head> <body> <div class='main-wrapper' style='display: flex; flex-direction: column; height: -webkit-fill-available;'>{header}{body}{footer}</div> </body> </html>");
                return new TemplateResult
                {
                    RepeatHeaderFooter = true,
                    Body = $"<html> <head> <style>{allStyles}</style></head> <body> <div class='main-wrapper' style='display: flex; flex-direction: column; height: -webkit-fill-available;'>{header}{body}{footer}</div> </body> </html>"
                };
            }
            else
            {
                return new TemplateResult
                {
                    RepeatHeaderFooter = false,
                    Header = $"<style>{allStyles}{backgroundStyles}</style>{header}",
                    Body = $"<style>{allStyles}{backgroundStyles}</style>{body}",
                    Footer = $"<style>{allStyles}{backgroundStyles}</style>{footer}"
                };
            }
        }

        private string LoadOpenSansFontCSS()
        {
            if (_openSansFontCSS != null) // Load only once
            {
                string fontPath = Path.Combine(Directory.GetCurrentDirectory(), "Templates", "Tally", "Styles", "Fonts", "OpenSans");
                _openSansFontCSS = $@"
                        @font-face {{ font-family: 'Open Sans'; src: url('{fontPath}/OpenSans-Light.ttf') format('truetype'); font-weight: 200; font-style: normal; unicode-range: U+0020-007E, U+00A0-00FF; }}
                        @font-face {{ font-family: 'Open Sans'; src: url('{fontPath}/OpenSans-LightItalic.ttf') format('truetype'); font-weight: 200; font-style: italic; unicode-range: U+0020-007E, U+00A0-00FF; }}
                        @font-face {{ font-family: 'Open Sans'; src: url('{fontPath}/OpenSans-Regular.ttf') format('truetype'); font-weight: 400; font-style: normal; unicode-range: U+0020-007E, U+00A0-00FF; }}
                        @font-face {{ font-family: 'Open Sans'; src: url('{fontPath}/OpenSans-Italic.ttf') format('truetype'); font-weight: 400; font-style: italic; unicode-range: U+0020-007E, U+00A0-00FF; }}
                        @font-face {{ font-family: 'Open Sans'; src: url('{fontPath}/OpenSans-Medium.ttf') format('truetype'); font-weight: 500; font-style: normal; unicode-range: U+0020-007E, U+00A0-00FF; }}
                        @font-face {{ font-family: 'Open Sans'; src: url('{fontPath}/OpenSans-MediumItalic.ttf') format('truetype'); font-weight: 500; font-style: italic; unicode-range: U+0020-007E, U+00A0-00FF; }}
                        @font-face {{ font-family: 'Open Sans'; src: url('{fontPath}/OpenSans-Bold.ttf') format('truetype'); font-weight: 700; font-style: normal; unicode-range: U+0020-007E, U+00A0-00FF; }}
                        @font-face {{ font-family: 'Open Sans'; src: url('{fontPath}/OpenSans-BoldItalic.ttf') format('truetype'); font-weight: 700; font-style: italic; unicode-range: U+0020-007E, U+00A0-00FF; }}
                    ";
            }
            return _openSansFontCSS ?? string.Empty;
        }
        private string LoadRobotoFontCSS()
        {
            if (_openRobotoFontCSS != null) // Load only once
            {
                string fontName = "Roboto";
                string fontPath = Path.Combine(Directory.GetCurrentDirectory(), "Templates", "Tally", "Styles", "Fonts", "Roboto");
                _openRobotoFontCSS = $@"
                        @font-face {{ font-family: {fontName}; src: url('{fontPath}/{fontName}-Light.ttf') format('truetype'); font-weight: 200; font-style: normal; unicode-range: U+0020-007E, U+00A0-00FF; }}
                        @font-face {{ font-family: {fontName}; src: url('{fontPath}/{fontName}-LightItalic.ttf') format('truetype'); font-weight: 200; font-style: italic; unicode-range: U+0020-007E, U+00A0-00FF; }}
                        @font-face {{ font-family: {fontName}; src: url('{fontPath}/{fontName}-Regular.ttf') format('truetype'); font-weight: 400; font-style: normal; unicode-range: U+0020-007E, U+00A0-00FF; }}
                        @font-face {{ font-family: {fontName}; src: url('{fontPath}/{fontName}-Italic.ttf') format('truetype'); font-weight: 400; font-style: italic; unicode-range: U+0020-007E, U+00A0-00FF; }}
                        @font-face {{ font-family: {fontName}; src: url('{fontPath}/{fontName}-Medium.ttf') format('truetype'); font-weight: 500; font-style: normal; unicode-range: U+0020-007E, U+00A0-00FF; }}
                        @font-face {{ font-family: {fontName}; src: url('{fontPath}/{fontName}-MediumItalic.ttf') format('truetype'); font-weight: 500; font-style: italic; unicode-range: U+0020-007E, U+00A0-00FF; }}
                        @font-face {{ font-family: {fontName}; src: url('{fontPath}/{fontName}-Bold.ttf') format('truetype'); font-weight: 700; font-style: normal; unicode-range: U+0020-007E, U+00A0-00FF; }}
                        @font-face {{ font-family: {fontName}; src: url('{fontPath}/{fontName}-BoldItalic.ttf') format('truetype'); font-weight: 700; font-style: italic; unicode-range: U+0020-007E, U+00A0-00FF; }}
                    ";
            }
            return _openRobotoFontCSS ?? string.Empty;
        }

        private string GetFileNameWithPath(Root request)
        {
            string rootPath = Path.Combine(Directory.GetCurrentDirectory(), "Downloads");
            Directory.CreateDirectory(rootPath);

            string pdfName = $"{(string.IsNullOrWhiteSpace(request?.PdfRename) ? "PDF" : request.PdfRename)} {DateTimeOffset.Now:HHmmssfff}.pdf";
            return Path.Combine(rootPath, pdfName);
        }

        public async Task<string> GeneratePdfAsync(Root request)
        {
            Console.WriteLine("PDF Generation Started ...");
            // Console.WriteLine("First : " + DateTime.Now.ToString("HH:mm:ss.fff"));

            using var browser = await Puppeteer.LaunchAsync(new LaunchOptions
            {
                Headless = true,
                ExecutablePath = "/Applications/Google Chrome.app/Contents/MacOS/Google Chrome" // Example path for regular Chrome.  Correct this!
            });
            using var page = await browser.NewPageAsync();

            // Console.WriteLine("Get RendererConfig " + DateTime.Now.ToString("HH:mm:ss.fff"));

            string templatePath = Path.Combine(Directory.GetCurrentDirectory(), "Templates", "Tally");
            var (commonStyles, headerStyles, footerStyles, bodyStyles, BackgroundStyles) = _styles.Value;
            // Console.WriteLine("Get Styles " + DateTime.Now.ToString("HH:mm:ss"));

            // Run template rendering in parallel
            var renderTasks = new[]
            {
                RenderTemplate(Path.Combine(templatePath, "Header.cshtml"), request),
                RenderTemplate(Path.Combine(templatePath, "Footer.cshtml"), request),
                RenderTemplate(Path.Combine(templatePath, "Body.cshtml"), request)
            };

            await Task.WhenAll(renderTasks);

            string header = renderTasks[0].Result;
            string footer = renderTasks[1].Result;
            string body = renderTasks[2].Result;

            // Console.WriteLine("Get Templates " + DateTime.Now.ToString("HH:mm:ss.fff"));

            TemplateResult template = CreatePdfDocument(header, body, footer, commonStyles, headerStyles, footerStyles, bodyStyles, request, BackgroundStyles);
            // Console.WriteLine("Check - "+template);
            // Console.WriteLine("Get CreatePdfDocument " + DateTime.Now.ToString("HH:mm:ss.fff"));
            
            await page.SetContentAsync(template.Body);

            // Define the PDF options with header and footer
            var pdfOptions = new PdfOptions
            {
                 // 1. Format: Paper size and orientation
                Format = PaperFormat.A4, // Common formats: A4, Letter, Legal, etc.  See PaperFormat enum
                // Orientation:
                Landscape = false, // Default is portrait. Set to true for landscape

                // 2. Margins
                MarginOptions = new MarginOptions
                {
                    Top = "5mm", // String values are also accepted: "1in", "2cm", etc.
                    Bottom = "5mm",
                    Left = "5mm",
                    Right = "5mm"
                },

                // 4. Backgrounds
                PrintBackground = true, // Include background colors and images in the PDF

                // 5. Page Ranges (for multi-page PDFs)
                // PageRanges = "1-5", // Print only pages 1 through 5 (or any range you specify)

                // 6. Scale
                // Scale = 1.0, // Adjust the scaling of the PDF content.  1.0 is normal size.

                // 7. PreferCSSPageSize
                // PreferCSSPageSize = false, // Whether to use the page size defined in CSS

                // 8. EmulateMediaType
                // EmulateMediaType = MediaType.Print, // Emulate 'print' media type.  Useful for styling.

                // 9. OmitBackground
                // OmitBackground = false, // Whether to omit the background

                // 10. Title (PDF metadata)
                // Title = "Giddh Invoice",

                // 11. Author (PDF metadata)
                // Author = "Divyanshu",

                // 12. Keywords (PDF metadata)
                // Keywords = "pdf, generation, puppeteer",

                // 13. Producer (PDF metadata)
                // Producer = "My PDF Generator",

                // 14. Creator (PDF metadata)
                // Creator = "My Application",

                // 15. Create a tagged PDF (Accessibility)
                // Tagged = false, // Set to true to create a tagged PDF for better accessibility

                // 16. Format as PDF/A (Archiving)
                // PdfA = PdfAConformance.PDF_A_1a, // Or other PDF/A conformance levels

                // 17. Page ranges to print
                // PageRanges = "1-3, 5", // Example: Print pages 1, 2, 3, and 5

                // 18. Outline (Bookmarks)
                // This is more complex and usually done by manipulating the DOM before printing.

                // 19. Compression
                // PuppeteerSharp itself doesn't directly offer compression options.  You might need to use a separate PDF library after generating the PDF if you need to compress it.

                // 20. Watermarks
                // Watermarks are usually added by manipulating the HTML content before generating the PDF.

                // 21. Encryption/Security
                // PuppeteerSharp doesn't directly support PDF encryption. You'll need to use a separate PDF library after generating the PDF.
            };

            if (template.RepeatHeaderFooter == true) {
                // Console.WriteLine(template);
                Console.WriteLine($"template.RepeatHeaderFooter -> ");
                pdfOptions.DisplayHeaderFooter = true;
                pdfOptions.HeaderTemplate = template.Header;
                pdfOptions.FooterTemplate = template.Footer;
            }

            // Set EmulateMediaType (important for CSS)
            await page.EmulateMediaTypeAsync(MediaType.Print);
            
            // Uncomment below line to save PDF file in local 
            string pdfName = GetFileNameWithPath(request);
            Console.WriteLine($"PDF Downloaded, Please check -> {pdfName}");
            // Console.WriteLine("Options  " + pdfOptions);

            await page.PdfAsync(pdfName, pdfOptions);

            var pdfBytes = await page.PdfDataAsync(pdfOptions);
            // Console.WriteLine("pdfBytes " + DateTime.Now.ToString("HH:mm:ss.fff"));
            return Convert.ToBase64String(pdfBytes);
        }
    }
}
