var builder = WebApplication.CreateBuilder(args);
builder.Services.AddControllers();

builder.WebHost.ConfigureKestrel(options =>
{
    options.ListenAnyIP(1300);
});
var app = builder.Build();

app.MapControllers();
app.Run();