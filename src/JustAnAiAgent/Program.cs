using cCoder.Security.Api;
using cCoder.Security.Data.EF.MSSQL;
using cCoder.Security.Data.EF.Interfaces;
using Microsoft.EntityFrameworkCore;
using JustAnAiAgent.Data;
using JustAnAiAgent.Data.Interfaces;
using JustAnAiAgent.Api;
using JustAnAiAgent.Services;

var config = new ConfigurationBuilder()
    .AddEnvironmentVariables()
    .SetBasePath(Directory.GetCurrentDirectory())
    .AddJsonFile("appsettings.json", optional: false, reloadOnChange: true)
    .Build();

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllersWithViews();

builder.Services.AddSecurityApi((services, securityConfig) =>
{
    securityConfig.AddMSSQLModelProvider(services, config.GetConnectionString("SSO"));
    securityConfig.UsePasswordHasherHashing(services);
});

builder.Services.AddDbContext<JustAnAiAgentDbContext>(options =>
{
    options.UseSqlServer(config.GetConnectionString("JustAnAiAgent"));
});

builder.Services.AddDataServices();
builder.Services.AddStandardServices();
builder.Services.AddMcpTools();
builder.Services.AddJustAnAiAgentApi();

builder.Services.AddMvc(options =>
{
    options.EnableEndpointRouting = false;
});

builder.Services.AddEndpointsApiExplorer();

var app = builder.Build();

using var scope = app.Services.CreateScope();

scope.ServiceProvider
    .GetRequiredService<ISecurityDbContextFactory>()
    .CreateDbContext()
    .Migrate();

scope.ServiceProvider
    .GetRequiredService<IJustAnAiAgentDbContextFactory>()
    .CreateDbContext()
    .Migrate();

// Configure the HTTP request pipeline.
app.UseHttpsRedirection();
app.UseRouting();
app.UseAuthorization();
app.MapStaticAssets();
app.UseSession();

app.MapControllerRoute(name: "default", pattern: "{controller=Home}/{action=Index}/{id?}")
    .WithStaticAssets();

await app.RunAsync();