using IronPdf;
using IronPdf.Rendering;
using System;

namespace GiddhTemplate.Services
{
    public class PdfRendererConfigService
    {
        private readonly Lazy<ChromePdfRenderer> _cachedRenderer;

        public PdfRendererConfigService()
        {
            _cachedRenderer = new Lazy<ChromePdfRenderer>(() =>
            {
                License.LicenseKey = Environment.GetEnvironmentVariable("IRON_PDF_LICENSE_KEY");
                // Add Logger
                IronPdf.Logging.Logger.LoggingMode = IronPdf.Logging.Logger.LoggingModes.All;
                IronPdf.Logging.Logger.LogFilePath = "Defaultimg.log";


                var renderer = new ChromePdfRenderer();

                // Set custom margin
                renderer.RenderingOptions.MarginTop = 8;
                renderer.RenderingOptions.MarginLeft = 0;
                renderer.RenderingOptions.MarginRight = 0;
                renderer.RenderingOptions.MarginBottom = 8;

                // Additional rendering options
                renderer.RenderingOptions.PrintHtmlBackgrounds = true;
                renderer.RenderingOptions.PaperSize = PdfPaperSize.A4;
                renderer.RenderingOptions.PaperOrientation = PdfPaperOrientation.Portrait;

                // Choose screen or print CSS media
                renderer.RenderingOptions.CssMediaType = PdfCssMediaType.Print;

                PdfDocument pdf = renderer.RenderHtmlAsPdf("<h1>Warmup the Chrome Renderer !</h1>");

                return renderer;
            });
        }

        public ChromePdfRenderer GetConfiguredRenderer() => _cachedRenderer.Value;
    }
}