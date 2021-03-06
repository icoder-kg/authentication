using Identity.ClaimProviders;
using Identity.CustomValidator;
using Identity.Helper;
using Identity.Models;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace Identity
{
    public class Startup
    {
        public IConfiguration configuration { get; }

        public Startup(IConfiguration configuration)
        {
            this.configuration = configuration;
        }

        public void ConfigureServices(IServiceCollection services)
        {
            services.AddTransient<IAuthorizationHandler, ExpireDateExchangeHandler>();

            services.AddDbContext<AppIdentityDbContext>(opts =>
            {
                opts.UseSqlServer(configuration["ConnectionStrings:DefaultConnection"]);
            });
            services.AddAuthorization(opts =>
            {
                opts.AddPolicy("BishkekPolicy", policy =>
                {
                    policy.RequireClaim("city", "Bishkek");
                });
                opts.AddPolicy("ViolencePolicy", policy =>
                {
                    policy.RequireClaim("violence");
                });
                opts.AddPolicy("ExchangePolicy", policy =>
                {
                    policy.AddRequirements(new ExpireDateExchangeRequirement());
                });
            });

            services.AddAuthentication().AddFacebook(opts =>
            {
                opts.AppId = configuration["Facebook:AppId"];
                opts.AppSecret = configuration["Facebook:AppSecret"];
            }).AddGoogle(opts =>
            {
                opts.ClientId = configuration["Google:ClientId"];
                opts.ClientSecret = configuration["Google:ClientSecret"];
            }).AddMicrosoftAccount(opts =>
            {
                opts.ClientId = configuration["Microsoft:ClientId"];
                opts.ClientSecret = configuration["Microsoft:ClientSecret"];
            });

            services.AddRazorPages().AddRazorRuntimeCompilation();

            services.AddIdentity<AppUser, AppRole>(opts =>
            {
                //User Name validation
                opts.User.RequireUniqueEmail = true;
                opts.User.AllowedUserNameCharacters = "abcdefghijklmnopqrstuvwxyzABCDEFGHIJKLMNOPQRSTUVWXYZ0123456789-._";

                //Pasword validation
                opts.Password.RequiredLength = 8;
                opts.Password.RequireNonAlphanumeric = false;
                opts.Password.RequireLowercase = false;
                opts.Password.RequireUppercase = false;
                opts.Password.RequireDigit = false;
            }).AddPasswordValidator<CustomPasswordValidator>()
            .AddUserValidator<CustomUserValidator>()
            .AddErrorDescriber<CustomIdentityErrorDescriber>()
            .AddEntityFrameworkStores<AppIdentityDbContext>()
            .AddDefaultTokenProviders();

            services.AddMvc(option => option.EnableEndpointRouting = false);

            //Cookie
            CookieBuilder cookieBuilder = new CookieBuilder();
            cookieBuilder.Name = "Usman.KG";
            cookieBuilder.HttpOnly = false;
            cookieBuilder.SameSite = SameSiteMode.Lax;
            cookieBuilder.SecurePolicy = CookieSecurePolicy.SameAsRequest;

            services.ConfigureApplicationCookie(opts =>
            {
                opts.LoginPath = new PathString("/Home/SignIn");
                opts.Cookie = cookieBuilder;
                opts.SlidingExpiration = true;
                opts.ExpireTimeSpan = System.TimeSpan.FromDays(60);
                opts.AccessDeniedPath = new PathString("/Member/AccessDenied");
                opts.LogoutPath = new PathString("/Home/SignOut");
            });
            services.AddScoped<IClaimsTransformation, ClaimProvider>();
            services.AddMvc();
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
        {
            app.UseDeveloperExceptionPage();
            app.UseStatusCodePages();
            app.UseStaticFiles();
            app.UseAuthentication();
            app.UseMvcWithDefaultRoute();//Controller/Action/{id}
        }
    }
}