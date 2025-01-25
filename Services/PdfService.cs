using IronPdf;
using IronPdf.Rendering;
using System.Threading.Tasks;
using InvoiceData;

namespace GiddhTemplate.Services
{
    public class PdfService
    {
        private readonly PdfRendererConfigService _rendererConfig;
        private readonly RazorTemplateService _razorTemplateService;

        public PdfService()
        {
            _rendererConfig = new PdfRendererConfigService();
            _razorTemplateService = new RazorTemplateService();
        }

        public async Task<string> GeneratePdfAsync(Root request)
        {
            Console.WriteLine("PDF Generation Started ... 1");
            var renderer = _rendererConfig.GetConfiguredRenderer();
            string TemplateInitialPath = Path.Combine(Directory.GetCurrentDirectory(), "Templates", "Tally");

            Console.WriteLine("PDF Generation Started ... 2");

            // Load CSS from file
            string commonStyles = "";
            string headerStyles = "";
            string footerStyles = "";
            string bodyStyles = "";

            string commonStylesPath = Path.Combine(TemplateInitialPath, "Styles", "Styles.css");
            if (File.Exists(commonStylesPath))
            {
                commonStyles = File.ReadAllText(commonStylesPath);
            }
            Console.WriteLine("PDF Generation Started ... 3");
            string headerStylesPath = Path.Combine(TemplateInitialPath, "Styles", "Header.css");
            if (File.Exists(headerStylesPath))
            {
                headerStyles = File.ReadAllText(headerStylesPath);
            }
            Console.WriteLine("PDF Generation Started ... 4");
            string footerStylesPath = Path.Combine(TemplateInitialPath, "Styles", "Footer.css");
            if (File.Exists(footerStylesPath))
            {
                footerStyles = File.ReadAllText(footerStylesPath);
            }
            Console.WriteLine("PDF Generation Started ... 5");
            string bodyStylesPath = Path.Combine(TemplateInitialPath, "Styles", "Body.css");
            if (File.Exists(bodyStylesPath))
            {
                bodyStyles = File.ReadAllText(bodyStylesPath);
            }

            Console.WriteLine("PDF Generation Started ... 6");

            // Render Razor template to HTML string
            string headerTemplatePath = Path.Combine(TemplateInitialPath, "Header.cshtml");
            string footerTemplatePath = Path.Combine(TemplateInitialPath, "Footer.cshtml");
            string bodyTemplatePath = Path.Combine(TemplateInitialPath, "Body.cshtml");

            Console.WriteLine("PDF Generation Started ... 7");
            string header = await _razorTemplateService.RenderTemplateAsync(headerTemplatePath, request);
            Console.WriteLine("PDF Generation Started ... 7");
            string footer = await _razorTemplateService.RenderTemplateAsync(footerTemplatePath, request);
            Console.WriteLine("PDF Generation Started ... 8");
            string body = await _razorTemplateService.RenderTemplateAsync(bodyTemplatePath, request);
            Console.WriteLine("PDF Generation Started ... 9");

            // Header Code
            renderer.RenderingOptions.HtmlHeader = new HtmlHeaderFooter()
            {
                HtmlFragment = $"<style>{commonStyles}{headerStyles}</style>{header}",
                // Enable the dynamic height feature
                MaxHeight = HtmlHeaderFooter.FragmentHeight,
            };

            // Footer Code
            renderer.RenderingOptions.HtmlFooter = new HtmlHeaderFooter()
            {
                HtmlFragment = $"<style>{commonStyles}{footerStyles}</style>{footer}",
                // Enable the dynamic height feature
                MaxHeight = HtmlHeaderFooter.FragmentHeight,
            };

            // renderer.RenderingOptions.CustomCssUrl = commonStyles;

            Console.WriteLine("PDF Generation Started ... 10");

            PdfDocument pdf = renderer.RenderHtmlAsPdf($"<style>{commonStyles}{bodyStyles}</style>{body}");

            // Uncomment below line to get HTML string
            // Console.WriteLine("HTML"+ pdf.ToHtmlString());

            // Uncomment below line to save PDF file in local 
            // string rootPath = Path.Combine(Directory.GetCurrentDirectory(), "Downloads");
            // string filePath = Path.Combine(rootPath, "PDF_" + DateTimeOffset.Now.ToString("HHmmssfff") + ".pdf");
            // pdf.SaveAs(filePath);

            // Return Base64 string
            Console.WriteLine("PDF Generation Started ... 11");
            return Convert.ToBase64String(pdf.BinaryData);
        }
    }
}
