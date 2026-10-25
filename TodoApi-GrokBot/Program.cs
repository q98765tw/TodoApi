using Microsoft.EntityFrameworkCore;
using TodoApiGrokBot.Data;
using TodoApiGrokBot.Extensions;

var builder = WebApplication.CreateBuilder(args);

// Render 會用 PORT 環境變數指定 port
var port = Environment.GetEnvironmentVariable("PORT");
if (!string.IsNullOrEmpty(port))
    builder.WebHost.UseUrls($"http://0.0.0.0:{port}");

builder.Services
    .AddJwtAuth(builder.Configuration)
    .AddApplicationServices(builder.Configuration)
    .AddSwaggerWithJwt()
    .AddControllers();

var app = builder.Build();

// 啟動時自動套用 migration（整合測試用 "Testing" 環境，由測試自行建立 SQLite schema）
if (!app.Environment.IsEnvironment("Testing"))
{
    using var scope = app.Services.CreateScope();
    scope.ServiceProvider.GetRequiredService<AppDbContext>().Database.Migrate();
}

app.UseSwagger();
app.UseSwaggerUI();
app.UseDefaultFiles();
app.UseStaticFiles();
app.UseAuthentication();
app.UseAuthorization();
app.MapControllers();
app.Run();

// 讓測試專案的 WebApplicationFactory<Program> 可以參考
public partial class Program { }
