using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.IdentityModel.Tokens;
using Microsoft.AspNetCore.Mvc;
using System.Text;
using Microsoft.AspNetCore.HttpOverrides;

using TodoApi.Models;
using TodoApi.Data;
using Microsoft.AspNetCore.DataProtection;

namespace TodoApi
{
    public class Startup
    {
        public Startup(IConfiguration configuration)
        {
            Configuration = configuration;
        }

        // Get the appsetting.json file
        public IConfiguration Configuration { get; }

        // This method gets called by the runtime.
        // Use this method to add services to the container.
        public void ConfigureServices(IServiceCollection services)
        {
            // Creates the database
            services.AddEntityFrameworkSqlite()
                .AddDbContext<TodoDbContext>(options =>
                    //options.UseSqlite(Configuration.GetConnectionString("TodoDatabase")));
                    options.UseInMemoryDatabase("TodoList"));

            // Use Identity inside the database
            services.AddIdentity<User, IdentityRole>()
                .AddEntityFrameworkStores<TodoDbContext>()
                .AddDefaultTokenProviders();

            services.Configure<IdentityOptions>(options =>
            {
                // Password settings
                options.Password.RequireDigit = true;
                options.Password.RequiredLength = 8;
                options.Password.RequireNonAlphanumeric = false;
                options.Password.RequireUppercase = true;
                options.Password.RequireLowercase = false;
                options.Password.RequiredUniqueChars = 6;

                // Lockout settings
                options.Lockout.DefaultLockoutTimeSpan = TimeSpan.FromMinutes(30);
                options.Lockout.MaxFailedAccessAttempts = 10;
                options.Lockout.AllowedForNewUsers = true;

                // User settings
                options.User.RequireUniqueEmail = true;
            });

            services.ConfigureApplicationCookie(options =>
            {
                // Cookie settings
                options.Cookie.HttpOnly = true;
                options.Cookie.Expiration = TimeSpan.FromDays(150);
                options.LoginPath = "/login"; // If the LoginPath is not set here, ASP.NET Core will default to /Account/Login
                options.LogoutPath = "/logout"; // If the LogoutPath is not set here, ASP.NET Core will default to /Account/Logout
                options.AccessDeniedPath = "/Account/AccessDenied"; // If the AccessDeniedPath is not set here, ASP.NET Core will default to /Account/AccessDenied
                options.SlidingExpiration = true;

                // Avoid redirection to login page for unlogged users
                options.Events.OnRedirectToLogin = context =>
                {
                    context.Response.StatusCode = 401;
                    return Task.CompletedTask;
                };
            });

            // Use Cookie OR JWT tokens to identify the user
            // By default it is cookie to change it use
            // [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
            services.AddAuthentication()
              .AddCookie(options => options.SlidingExpiration = true)
              .AddJwtBearer(options =>
              {
                  options.RequireHttpsMetadata = false;
                  options.SaveToken = true;

                  options.TokenValidationParameters = new TokenValidationParameters()
                  {
                      // Grab the data from the appsetting.json file
                      IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(Configuration["Token:JwtKey"])),
                      ValidIssuer = Configuration["Token:JwtIssuer"],
                      ValidAudience = Configuration["Token:JwtIssuer"],

                      // If you want to allow a certain amount of clock drift, set that here:
                      ClockSkew = TimeSpan.Zero
                  };

                  // Log the authentication events
                  /*
                  options.Events = new JwtBearerEvents
                  {
                      OnAuthenticationFailed = context =>
                      {
                          Console.WriteLine("OnAuthenticationFailed: " +
                              context.Exception.Message);
                          return Task.CompletedTask;
                      },
                      OnTokenValidated = context =>
                      {
                          Console.WriteLine("OnTokenValidated: " +
                              context.SecurityToken);
                          return Task.CompletedTask;
                      }
                  };
                  */
              });

            /*
            services.AddDataProtection()
                .ProtectKeysWithCertificate("thumbprint");
            */

            services.AddMvc();/* options =>
                // Prevent CSRF attacks
                // Every request will be blocked
                // unless it includes a valid antiforgery token.
                options.Filters.Add(new ValidateAntiForgeryTokenAttribute()));
                */
        }

        // This method gets called by the runtime.  
        // Use this method to configure the HTTP request pipeline. 
        public void Configure(IApplicationBuilder app,
                            IHostingEnvironment env,
                            TodoDbContext dbContext)
        {
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
                app.UseBrowserLink();
                app.UseDatabaseErrorPage();
            }

            // For reverse proxy server
            app.UseForwardedHeaders(new ForwardedHeadersOptions
            {
                ForwardedHeaders = ForwardedHeaders.XForwardedFor | ForwardedHeaders.XForwardedProto
            });

            // Authentification Identity and JWT middleware
            app.UseAuthentication();

            app.UseMvc();

            dbContext.Database.EnsureCreated();
        }
    }
}
