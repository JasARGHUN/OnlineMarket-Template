using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.UI;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.HttpsPolicy;
using Microsoft.EntityFrameworkCore;
using OnlineMarket.DataAccess.Data;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using OnlineMarket.DataAccess.Repository.IRepository;
using OnlineMarket.DataAccess.Repository;
using Microsoft.AspNetCore.Identity.UI.Services;
using OnlineMarket.Utility;
using ReflectionIT.Mvc.Paging;
using Stripe;

namespace OnlineMarket
{
    public class Startup
    {
        public Startup(IConfiguration configuration)
        {
            Configuration = configuration;
        }

        public IConfiguration Configuration { get; }

        public void ConfigureServices(IServiceCollection services)
        {
            services.AddDbContext<ApplicationDbContext>(options =>
                options.UseSqlServer(
                    Configuration.GetConnectionString("DefaultConnection")));

            services.AddIdentity<IdentityUser, IdentityRole>()
                .AddDefaultTokenProviders()
                .AddEntityFrameworkStores<ApplicationDbContext>();

            // These email processing services handle and send a confirmation email. To change go to OnlineMarket.Utility/EmailOptions and EmailSender.
            // For API's change go to OnlineMarket/appsettings.json
            services.AddSingleton<IEmailSender, EmailSender>();
            services.Configure<EmailOptions>(Configuration);

            // Stripe services
            // For API's change go to OnlineMarket/appsettings.json
            services.Configure<StripeSettings>(Configuration.GetSection("Stripe"));

            services.AddScoped<IUnitOfWork, UnitOfWork>();

            services.AddControllersWithViews().AddRazorRuntimeCompilation();
            services.AddRazorPages();

            // Access level handler service in Identity, views located on Areas/Identity/Pages/Account
            services.ConfigureApplicationCookie(options =>
            {
                options.LoginPath = $"/Identity/Account/Login";
                options.LogoutPath = $"/Identity/Account/Logout";
                options.AccessDeniedPath = $"/Identity/Account/AccessDenied";
            });

            // Setting and Configuration make in this area OnlineMarket/Areas/Identity/Pages/Account/ExternalLogin.cshtml and ExternalLogin.cshtml.cs
            services.AddAuthentication().AddFacebook(options =>
            {
                // This is where you change the ID and Secret to your Facebook API ID and secret.
                options.AppId = "383519965985250";
                options.AppSecret = "dfabdcd017cfd140220b2838d06130f3";
            });
            services.AddAuthentication().AddGoogle(options =>
            {
                // This is where you change the ID and Secret to your Google(OAuth) API ID and secret.
                options.ClientId = "749187772104-p3l4om0og4l7e1o5a5534816t5l5r6br.apps.googleusercontent.com";
                options.ClientSecret = "GooxuRd4CBVtkZzV7UXtt287";
            });

            services.AddSession(options =>
            {
                options.IdleTimeout = TimeSpan.FromMinutes(50);
                options.Cookie.HttpOnly = true;
                options.Cookie.IsEssential = true;
            });

            services.AddMvc();
            services.AddPaging(options => {
                options.ViewName = "Bootstrap4";
                options.HtmlIndicatorDown = " <span>&darr;</span>";
                options.HtmlIndicatorUp = " <span>&uarr;</span>";
            });
        }

        //SendGrid Login:testWP-03@yandex.ru Password:qwertyuiop1234567890

        public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
        {
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
                app.UseDatabaseErrorPage();
            }
            else
            {
                app.UseExceptionHandler("/Home/Error");
                app.UseHsts();
            }
            app.UseHttpsRedirection();
            app.UseStaticFiles();

            app.UseRouting();

            // Stripe middleware
            StripeConfiguration.ApiKey = Configuration.GetSection("Stripe")["SecretKey"];

            app.UseSession();

            app.UseAuthentication();
            app.UseAuthorization();

            app.UseEndpoints(endpoints =>
            {
                endpoints.MapControllerRoute(
                    name: "default",
                    pattern: "{area=Customer}/{controller=Home}/{action=Index}/{id?}");
                endpoints.MapRazorPages();
            });
        }
    }
}
