using IronPdf;
using IronPdf.Rendering;
using System;
using System.IO;
using System.Threading.Tasks;
using System.Linq;
using System.Text;
using InvoiceData;

namespace GiddhTemplate.Services
{
    public class PdfService
    {
        private readonly PdfRendererConfigService _rendererConfig;
        private readonly RazorTemplateService _razorTemplateService;
        private readonly Lazy<(string Common, string Header, string Footer, string Body, string BackgroundStyles)> _styles;

        public PdfService()
        {
            _rendererConfig = new PdfRendererConfigService();
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

        private HtmlHeaderFooter CreateHtmlHeaderFooter(string styles, string content)
        {
            return new HtmlHeaderFooter
            {
                HtmlFragment = $"<style>{styles}</style>{content}",
                MaxHeight = HtmlHeaderFooter.FragmentHeight
            };
        }

        private PdfDocument CreatePdfDocument(string header, string body, string footer, string commonStyles, string headerStyles, string footerStyles, string bodyStyles, ChromePdfRenderer renderer, Root request, string backgroundStyles)
        {
            // Dynamic Theme
            var themeCSS = new StringBuilder();
            if (request?.Theme?.Font?.Family == "Open Sans")
            {
                themeCSS.Append(LoadOpenSansFontCSS());
            }
            else if (request?.Theme?.Font?.Family == "Roboto")
            {
                Console.WriteLine("LoadRobotoFontCSS");
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
                return renderer.RenderHtmlAsPdf($"<style>{allStyles}</style><div style='display: flex; flex-direction: column; height: -webkit-fill-available;'>{header}{body}{footer}</div>");
            }
            else
            {
                renderer.RenderingOptions.HtmlHeader = CreateHtmlHeaderFooter(allStyles, header);
                renderer.RenderingOptions.HtmlFooter = CreateHtmlHeaderFooter(allStyles, footer);
                return renderer.RenderHtmlAsPdf($"<style>{allStyles}{backgroundStyles}</style>{body}");
            }
        }

        private string _openSansFontCSS = ""; // Cache the Open Sans CSS
        private string _openRobotoFontCSS = ""; // Cache the Open Sans CSS

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
            return _openSansFontCSS;
        }
        private string LoadRobotoFontCSS()
        {
            if (_openRobotoFontCSS != null) // Load only once
            {
                Console.WriteLine("_openRobotoFontCSS");
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
            return _openRobotoFontCSS;
        }

        private void GenerateLocalPdfFile(PdfDocument pdf, Root request)
        {
            if (pdf == null) throw new ArgumentNullException(nameof(pdf));

            string rootPath = Path.Combine(Directory.GetCurrentDirectory(), "Downloads");
            Directory.CreateDirectory(rootPath);

            string pdfName = $"{(string.IsNullOrWhiteSpace(request?.PdfRename) ? "PDF" : request.PdfRename)} {DateTimeOffset.Now:HHmmssfff}.pdf";
            string filePath = Path.Combine(rootPath, pdfName);

            pdf.SaveAs(filePath);
            Console.WriteLine($"PDF Downloaded, Please check -> {pdfName}");
        }

        public async Task<string> GeneratePdfAsync(Root request)
        {
            Console.WriteLine("PDF Generation Started ...");

            var renderer = _rendererConfig.GetConfiguredRenderer();
            string templatePath = Path.Combine(Directory.GetCurrentDirectory(), "Templates", "Tally");
            var (commonStyles, headerStyles, footerStyles, bodyStyles, BackgroundStyles) = _styles.Value;

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


            PdfDocument pdf = CreatePdfDocument(header, body, footer, commonStyles, headerStyles, footerStyles, bodyStyles, renderer, request, BackgroundStyles);

            // Add Page Number in Footer
            var pageNumber = new HtmlHeaderFooter { HtmlFragment = "<center style='font-size: 14px'>({page})</center>" };
            pdf.AddHtmlFooters(pageNumber, 1, Enumerable.Range(1, pdf.PageCount - 1)); // Pages 2 and up

            // Uncomment below line to save PDF file in local 
            // GenerateLocalPdfFile(pdf, request);

            return Convert.ToBase64String(pdf.BinaryData);
        }
    }
}