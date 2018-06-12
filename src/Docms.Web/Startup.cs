using Docms.Web.Docs;
using Docms.Web.IdentityServer;
using Docms.Web.Infrastructure;
using Docms.Web.Swagger;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.SpaServices.Webpack;
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

        // This method gets called by the runtime. Use this method to add services to the container.
        public void ConfigureServices(IServiceCollection services)
        {
            services.AddIdentityServer4();
            services.AddSwashbuckle();

            services.AddAuthentication()
                .AddCookie(options =>
                {
                    options.LoginPath = "/account/login";
                })
                .AddJwtBearer(options =>
                {
                    options.Audience = "docmsapi";
                    options.Authority = "http://localhost:5000/";
                    options.RequireHttpsMetadata = false;
                });

            // アプリケーション用DIの設定
            services.AddSingleton<IFileStorage, LocalFileStorage>();
            services.AddSingleton<IDocumentsRepository, DocmsContextDocumentsRepository>();
            services.AddSingleton<ITagsRepository, DocmsContextTagsRepository>();

            services.AddMvc();
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IHostingEnvironment env)
        {
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
                app.UseWebpackDevMiddleware(new WebpackDevMiddlewareOptions
                {
                    HotModuleReplacement = true,
                    ReactHotModuleReplacement = true
                });
            }
            else
            {
                app.UseExceptionHandler("/Home/Error");
            }

            app.UseIdentityServer4();
            app.UseSwashbuckle();

            app.UseStaticFiles();

            app.UseMvc(routes =>
            {
                routes.MapRoute(
                    name: "default",
                    template: "{controller=Home}/{action=Index}/{id?}");

                routes.MapSpaFallbackRoute(
                    name: "spa-fallback",
                    defaults: new { controller = "Home", action = "Index" });
            });
        }
    }
}
