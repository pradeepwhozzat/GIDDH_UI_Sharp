using GiddhTemplate.Services;

var builder = WebApplication.CreateBuilder(args);
builder.Services.AddControllers();
builder.Services.AddSingleton<PdfRendererConfigService>();
builder.WebHost.ConfigureKestrel(options => {
    options.ListenAnyIP(5000);
});
var app = builder.Build();
// Eager initialization of the ChromePdfRenderer through PdfRendererConfigService
var rendererConfigService = app.Services.GetRequiredService<PdfRendererConfigService>();
rendererConfigService.GetConfiguredRenderer();
app.MapControllers();
app.Run();
