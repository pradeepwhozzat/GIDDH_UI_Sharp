using IronPdf;
using IronPdf.Rendering;

namespace GiddhTemplate.Services
{
    public class PdfRendererConfigService
    {
        private ChromePdfRenderer? _cachedRenderer;
        
        public ChromePdfRenderer GetConfiguredRenderer()
        {
            if (_cachedRenderer == null) {
                // Initialize the renderer if it has not been cached yet
                IronPdf.License.LicenseKey = Environment.GetEnvironmentVariable("IRON_PDF_LICENSE_KEY");
                _cachedRenderer = new ChromePdfRenderer();
                
                // Set custom margin
                _cachedRenderer.RenderingOptions.MarginTop = 0;
                _cachedRenderer.RenderingOptions.MarginLeft = 0;
                _cachedRenderer.RenderingOptions.MarginRight = 0;
                _cachedRenderer.RenderingOptions.MarginBottom = 0;

                // Additional rendering options
                _cachedRenderer.RenderingOptions.PrintHtmlBackgrounds = true;
                _cachedRenderer.RenderingOptions.PaperSize = PdfPaperSize.A4;
                _cachedRenderer.RenderingOptions.PaperOrientation = PdfPaperOrientation.Portrait;

                // Choose screen or print CSS media
                _cachedRenderer.RenderingOptions.CssMediaType = PdfCssMediaType.Screen;
            }
            return _cachedRenderer;
        }
    }
}
