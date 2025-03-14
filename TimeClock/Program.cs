using Microsoft.EntityFrameworkCore;
using TimeClock.Models;
using TimeClock.Services;

class Program
{
    static async Task Main(string[] args)
    {
        var builder = WebApplication.CreateBuilder(args);

        builder.Services.AddDbContext<ApplicationDbContext>(
            dbContextoptions =>
            {
                dbContextoptions.UseSqlite(builder.Configuration["ConnectionStrings:SQLLiteConnection"]);
            }
            );

        //builder.Services.AddDbContext<ApplicationDbContext>(
        //    dbContextoptions =>
        //    {
        //        dbContextoptions.UseMySQL(builder.Configuration["ConnectionStrings:MySQLConnection"]);
        //    }
        //    );

        //builder.Services.AddDbContext<ApplicationDbContext>(
        //    dbContextoptions =>
        //    {
        //        dbContextoptions.UseSqlServer(builder.Configuration["ConnectionStrings:MSSQLConnection"]);
        //    }
        //    );



        // Add services to the container.
        builder.Services.AddControllersWithViews();
        builder.Services.AddDistributedMemoryCache(); // Required for session state
        builder.Services.AddSession(); // Enables session support
        builder.Services.AddMemoryCache();

        builder.Services.AddScoped<IDataAccessService, DataAccessService>();


        var app = builder.Build();

        // Configure the HTTP request pipeline.
        if (!app.Environment.IsDevelopment())
        {
            app.UseExceptionHandler("/Home/Error");
            // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
            app.UseHsts();
        }

        app.UseHttpsRedirection();
        app.UseStaticFiles();

        app.UseRouting();

        app.UseAuthorization();
        app.UseSession();

        app.MapControllerRoute(
            name: "default",
            pattern: "{controller=Home}/{action=Index}/{id?}");

        app.Run();


    }
}


