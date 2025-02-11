using RazorLight;

namespace GiddhTemplate.Services
{
    public class RazorTemplateService
    {
        private readonly RazorLightEngine _engine;

        public RazorTemplateService()
        {
            _engine = new RazorLightEngineBuilder()
                .UseEmbeddedResourcesProject(typeof(RazorTemplateService)) // For embedded templates
                .UseFileSystemProject(Directory.GetCurrentDirectory())    // For file-based templates
                .UseMemoryCachingProvider()
                .Build();
        }

        public async Task<string> RenderTemplateAsync<T>(string templatePath, T model)
        {
            if (!File.Exists(templatePath))
            {
                throw new FileNotFoundException($"Template file not found: {templatePath}");
            }

            string templateContent = await File.ReadAllTextAsync(templatePath);
            return await _engine.CompileRenderStringAsync(templatePath, templateContent, model);
        }
    }
}
