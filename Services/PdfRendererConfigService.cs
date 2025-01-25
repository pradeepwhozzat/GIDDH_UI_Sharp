using IronPdf;
using IronPdf.Rendering;

namespace GiddhTemplate.Services
{
    public class PdfRendererConfigService
    {
        public ChromePdfRenderer GetConfiguredRenderer()
        {
            IronPdf.License.LicenseKey = Environment.GetEnvironmentVariable("IRON_PDF_LICENSE_KEY");
            ChromePdfRenderer renderer = new ChromePdfRenderer();
            
            // Set custom margin
            renderer.RenderingOptions.MarginTop = 0;
            renderer.RenderingOptions.MarginLeft = 0;
            renderer.RenderingOptions.MarginRight = 0;
            renderer.RenderingOptions.MarginBottom = 0;

            // Additional rendering options
            renderer.RenderingOptions.PrintHtmlBackgrounds = true;
            renderer.RenderingOptions.PaperSize = PdfPaperSize.A4;
            renderer.RenderingOptions.PaperOrientation = PdfPaperOrientation.Portrait;

            // Choose screen or print CSS media
            renderer.RenderingOptions.CssMediaType = PdfCssMediaType.Print;

            return renderer;
        }
    }
}
