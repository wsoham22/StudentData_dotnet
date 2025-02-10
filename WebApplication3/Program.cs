using Microsoft.Data.SqlClient;
using System.Data;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.EntityFrameworkCore;
using MyAspNetCoreApp.Data;
using NLog;
using NLog.Extensions.Logging;
using NLog.Web;

var builder = WebApplication.CreateBuilder(args);

// Set up NLog configuration from appsettings.json or nlog.config
LogManager.LoadConfiguration("nlog.config");

try
{
    builder.Services.AddControllersWithViews();
    //builder.Services.AddScoped<IStudentRepository, StudentRepository>();
    builder.Services.AddMemoryCache();
    builder.Services.AddDbContext<ApplicationDbContext>(options =>
        options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection")));
    builder.Services.AddScoped<IDbConnection>(db =>
        new SqlConnection(builder.Configuration.GetConnectionString("DefaultConnection")));
}
catch (Exception ex)
{
    var logger = LogManager.GetCurrentClassLogger();
    logger.Error(ex, "Error occurred during service configuration");
    throw; 
}

var app = builder.Build();

try
{
    app.Urls.Add("http://0.0.0.0:8081");
    if (!app.Environment.IsDevelopment())
    {
        app.UseExceptionHandler("/Home/Error");
        app.UseHsts();
    }

    app.UseHttpsRedirection();
    app.UseStaticFiles();

    app.UseRouting();

    app.UseAuthorization();

    app.MapControllerRoute(
        name: "default",
        pattern: "{controller=Students}/{action=Index}/{id?}");

    app.Run();
}
catch (Exception ex)
{
    var logger = LogManager.GetCurrentClassLogger();
    logger.Fatal(ex, "Error occurred during app startup");
    throw;  // Ensure the app doesn't run in an erroneous state
}
