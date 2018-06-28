using Docms.Web.Config;
using Docms.Web.Data;
using Docms.Web.Models;
using Docms.Web.Services;
using Docms.Web.VisualizationSystem.Data;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

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

            services.AddTransient<IUserStore<ApplicationUser>, InMemoryUserStore>();
            services.AddTransient<IRoleStore<ApplicationRole>, InMemoryRoleStore>();

            services.AddIdentity<ApplicationUser, ApplicationRole>()
                .AddDefaultTokenProviders();

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
                app.UseExceptionHandler("/Home/Error");
            }

            app.UseStaticFiles();
            app.UseAuthentication();
            app.UseMvcWithDefaultRoute();
        }
    }
}
