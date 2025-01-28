using IronPdf;
using IronPdf.Rendering;
using System.IO;
using System.Threading.Tasks;
using InvoiceData;

namespace GiddhTemplate.Services
{
    public class PdfService
    {
        private readonly PdfRendererConfigService _rendererConfig;
        private readonly RazorTemplateService _razorTemplateService;
        private readonly Lazy<(string Common, string Header, string Footer, string Body)> _styles;

        public PdfService()
        {
            _rendererConfig = new PdfRendererConfigService();
            _razorTemplateService = new RazorTemplateService();
            
            string templatePath = Path.Combine(Directory.GetCurrentDirectory(), "Templates", "Tally");
            _styles = new Lazy<(string, string, string, string)>(() => LoadStyles(templatePath));
        }

        private string LoadFileContent(string filePath)
        {
            return File.Exists(filePath) ? File.ReadAllText(filePath) : string.Empty;
        }

        private (string Common, string Header, string Footer, string Body) LoadStyles(string basePath)
        {
            return (
                Common: LoadFileContent(Path.Combine(basePath, "Styles", "Styles.css")),
                Header: LoadFileContent(Path.Combine(basePath, "Styles", "Header.css")),
                Footer: LoadFileContent(Path.Combine(basePath, "Styles", "Footer.css")),
                Body: LoadFileContent(Path.Combine(basePath, "Styles", "Body.css"))
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

        public async Task<string> GeneratePdfAsync(Root request)
        {
            Console.WriteLine("PDF Generation Started ...");

            var renderer = _rendererConfig.GetConfiguredRenderer();
            string templatePath = Path.Combine(Directory.GetCurrentDirectory(), "Templates", "Tally");
            var (commonStyles, headerStyles, footerStyles, bodyStyles) = _styles.Value;

            string header = await RenderTemplate(Path.Combine(templatePath, "Header.cshtml"), request);
            string footer = await RenderTemplate(Path.Combine(templatePath, "Footer.cshtml"), request);
            string body = await RenderTemplate(Path.Combine(templatePath, "Body.cshtml"), request);
            // Console.WriteLine("Header HTML:" + $"<style>{commonStyles}{headerStyles}</style>{header}");
            // Console.WriteLine("Header HTML:" + $"<style>{commonStyles}{bodyStyles}{footerStyles}</style>{body}");
            // Console.WriteLine("Header HTML:" + $"<style>{commonStyles}{footerStyles}</style>{footer}");


            renderer.RenderingOptions.HtmlHeader = CreateHtmlHeaderFooter($"{commonStyles}{headerStyles}", header);
            // renderer.RenderingOptions.HtmlFooter = CreateHtmlHeaderFooter($"{commonStyles}{footerStyles}", footer);
            PdfDocument pdf = renderer.RenderHtmlAsPdf($"<style>{commonStyles}{bodyStyles}{footerStyles}</style>{body}");

            // Uncomment below line to save PDF file in local 
            // string rootPath = Path.Combine(Directory.GetCurrentDirectory(), "Downloads");
            // string filePath = Path.Combine(rootPath, "PDF_" + DateTimeOffset.Now.ToString("HHmmssfff") + ".pdf");
            // pdf.SaveAs(filePath);
            // Console.WriteLine("PDF Downloaded, Please check !");

            return Convert.ToBase64String(pdf.BinaryData);
        }
    }
}
