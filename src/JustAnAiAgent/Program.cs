using cCoder.Security.Api;
using JustAnAiAgent.AI;
using cCoder.Security.Data.EF.MSSQL;
using cCoder.Security.Data.EF.Interfaces;

var config = new ConfigurationBuilder()
    .AddEnvironmentVariables()
    .Build();

if (config.GetValue<bool>("AI.RunOllama"))
{
    AIModelHost aiHost = new(config.GetValue<string>("AI.OllamaExe"), config.GetValue<string>("AI.FallbackModel"));
    aiHost.Start();
}

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllersWithViews();

builder.Services.AddSecurityApi((services, securityConfig) =>
{
    securityConfig.AddMSSQLModelProvider(services, builder.Configuration.GetConnectionString("SSO"));
    securityConfig.UsePasswordHasherHashing(services);
});

var app = builder.Build();

using var scope = app.Services.CreateScope();

scope.ServiceProvider
    .GetRequiredService<ISecurityDbContextFactory>()
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