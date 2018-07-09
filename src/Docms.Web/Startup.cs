using Docms.Web.Config;
using Docms.Web.Data;
using Docms.Web.Models;
using Docms.Web.Services;
using Docms.Web.VisualizationSystem.Data;
using Docms.Web.VisualizationSystem.Services;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using System;

namespace Docms.Web
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
            services.AddDbContext<DocmsDbContext>(options =>
                options.UseSqlite(Configuration.GetConnectionString("DocmsConnection")));

            services.AddDbContext<VisualizationSystemDBContext>(options =>
                options.UseSqlite(Configuration.GetConnectionString("VisualizationSystemConnection")));

            services.AddTransient<IUserStore<ApplicationUser>, VisualizationSystemUserStore>();
            services.AddTransient<IRoleStore<ApplicationRole>, InMemoryRoleStore>();
            services.AddTransient<DocumentsService>();
            services.AddTransient<TagsService, VisualizationSystemTagsService>();

            services.AddIdentity<ApplicationUser, ApplicationRole>();
            services.ConfigureApplicationCookie(options =>
            {
                options.Cookie.HttpOnly = true;
                options.ExpireTimeSpan = TimeSpan.FromMinutes(30);
                options.LoginPath = "/account/login";
                options.AccessDeniedPath = "/account/accessdenied";
                options.SlidingExpiration = true;
            });

            services.AddIdentityServer()
                .AddDeveloperSigningCredential()
                .AddInMemoryClients(IdentityConfig.GetAllClients())
                .AddInMemoryApiResources(IdentityConfig.GetAllResources())
                .AddAspNetIdentity<ApplicationUser>();

            services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
                .AddJwtBearer(options =>
                {
                    options.Authority = "http://localhost:51693/";
                    options.Audience = "docmsapi";
                    options.RequireHttpsMetadata = false;
                });

            services.Configure<StorageSettings>(Configuration.GetSection("StorageSettings"));
            services.AddTransient<IStorageService, LocalStorageService>();

            services.AddMvc();
        }

        public void Configure(IApplicationBuilder app, IHostingEnvironment env)
        {
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
                app.UseDatabaseErrorPage();
            }
            else
            {
                app.UseExceptionHandler("/Shared/Error");
            }

            app.UseStaticFiles();
            app.UseIdentityServer();
            app.UseMvcWithDefaultRoute();
        }
    }
}
