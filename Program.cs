using GiddhTemplate.Services;

public class Program
{
    public static async Task Main(string[] args)
    {
        var builder = WebApplication.CreateBuilder(args);
        builder.Services.AddControllers();
        builder.Services.AddSingleton<PdfService>();
        builder.WebHost.ConfigureKestrel(options =>
        {
            options.ListenAnyIP(5000);
        });
        var app = builder.Build();
        await PdfService.GetBrowserAsync();
        app.MapControllers();
        await app.RunAsync();
    }
}
