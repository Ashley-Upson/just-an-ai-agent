//using cCoder.Security.Api;
//using cCoder.Security.Data.EF.MSSQL;
//using cCoder.Security.Data.EF.Interfaces;
using Microsoft.EntityFrameworkCore;
using JustAnAiAgent.Data;

var config = new ConfigurationBuilder()
    .AddEnvironmentVariables()
    .Build();

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllersWithViews();

//builder.Services.AddSecurityApi((services, securityConfig) =>
//{
//    securityConfig.AddMSSQLModelProvider(services, builder.Configuration.GetConnectionString("SSO"));
//    securityConfig.UsePasswordHasherHashing(services);
//});

builder.Services.AddDbContext<JustAnAiAgentDbContext>(options => options.UseSqlServer(config.GetConnectionString("JustAnAiAgent")));

var app = builder.Build();

using var scope = app.Services.CreateScope();

//scope.ServiceProvider
//    .GetRequiredService<ISecurityDbContextFactory>()
//    .CreateDbContext()
//    .Migrate();

// Configure the HTTP request pipeline.
app.UseHttpsRedirection();
app.UseRouting();
app.UseAuthorization();
app.MapStaticAssets();
app.UseSession();

app.MapControllerRoute(name: "default", pattern: "{controller=Home}/{action=Index}/{id?}")
    .WithStaticAssets();

await app.RunAsync();