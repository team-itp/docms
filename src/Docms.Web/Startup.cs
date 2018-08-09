using Docms.Domain.Documents;
using Docms.Infrastructure.Files;
using Docms.Infrastructure.Repositories;
using Docms.Infrastructure;
using Docms.Web.Application.Queries.Documents;
using Docms.Web.Application.Queries;
using MediatR;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using System;
using Microsoft.Extensions.DependencyInjection;
using Docms.Web.Application.DomainEventHandlers.DocumentCreated;
using Docms.Web.Application.Commands;

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
            services.AddCustomDbContext(Configuration);
            services.AddMvc();

            services.RegisterServices(Configuration);
            services.RegisterMediators(Configuration);
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
                app.UseExceptionHandler("/error");
            }

            app.UseCustomDbContext();
            app.UseStatusCodePagesWithRedirects("~/error/{0}");
            app.UseStaticFiles();
            app.UseMvcWithDefaultRoute();
        }
    }

    public static class CustomServiceCollectionExtension
    {
        public static IServiceCollection AddCustomDbContext(this IServiceCollection services, IConfiguration configuration)
        {
            services.AddDbContext<DocmsQueriesContext>(options =>
                options.UseSqlite(configuration.GetConnectionString("DocmsQueriesConnection")));
            services.AddDbContext<DocmsContext>(options =>
                options.UseSqlite(configuration.GetConnectionString("DocmsConnection")));
            return services;
        }

        public static IServiceCollection RegisterServices(this IServiceCollection services, IConfiguration configuration)
        {
            services.AddScoped<IDocumentRepository, DocumentRepository>();
            services.AddScoped<DocumentsQueries>();
            services.AddSingleton<IFileStorage>(sv => new LocalFileStorage("App_Data/flies"));
            return services;
        }

        public static IServiceCollection RegisterMediators(this IServiceCollection services, IConfiguration configuration)
        {
            services.AddMediatR();
            services.AddMediatR(typeof(CreateDocumentCommandHandler));
            services.AddMediatR(typeof(UpdateQueriesEventHandler));
            return services;
        }
    }

    public static class CustomAppExtension
    {
        public static void UseCustomDbContext(this IApplicationBuilder app)
        {
            if (!System.IO.Directory.Exists("App_Data"))
            {
                System.IO.Directory.CreateDirectory("App_Data");
            }
            using (var serviceScope = app.ApplicationServices
                .GetRequiredService<IServiceScopeFactory>()
                .CreateScope())
            {
                serviceScope.ServiceProvider
                    .GetService<DocmsContext>()
                    .Database
                    .EnsureCreated();
                serviceScope.ServiceProvider
                    .GetService<DocmsQueriesContext>()
                    .Database
                    .EnsureCreated();
            }
        }
    }
}
