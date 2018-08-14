using Docms.Domain.Documents;
using Docms.Infrastructure;
using Docms.Infrastructure.Files;
using Docms.Infrastructure.Repositories;
using Docms.Web.Api.Serialization;
using Docms.Web.Application.Identity;
using Docms.Web.Application.Queries;
using Docms.Web.Application.Queries.DocumentHistories;
using Docms.Web.Application.Queries.Documents;
using IdentityServer4.Models;
using MediatR;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IdentityModel.Tokens.Jwt;
using VisualizationSystem.Infrastructure;

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
            services.AddCustomDbContext(Configuration)
                .AddCustomIdentity(Configuration)
                .AddCustomAuthentication(Configuration);
            services.AddMvc()
                .AddJsonOptions(options =>
                {
                    options.SerializerSettings.TypeNameHandling = TypeNameHandling.Objects;
                    options.SerializerSettings.SerializationBinder = new DocmsJsonTypeBinder();
                });

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
            app.UseIdentityServer();
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
            services.AddDbContext<VisualizationSystemContext>(options =>
                options.UseSqlite(configuration.GetConnectionString("VisualizationSystemConnection")));
            return services;
        }
        public static IServiceCollection AddCustomIdentity(this IServiceCollection services, IConfiguration configuration)
        {
            services.AddTransient<IUserStore<ApplicationUser>, ApplicationUserStore>();
            services.AddTransient<IRoleStore<ApplicationRole>, ApplicationRoleStore>();

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
                .AddInMemoryClients(new List<Client>()
                {
                    new Client()
                    {
                        ClientId = "docms-client",
                        ClientSecrets = new List<Secret>()
                        {
                            new Secret("docms-client-secret".Sha256())
                        },
                        AllowedScopes = { "docmsapi" },
                        AllowedGrantTypes = GrantTypes.ResourceOwnerPassword,
                        AllowAccessTokensViaBrowser = true,
                    }
                })
                .AddInMemoryApiResources(new List<ApiResource>()
                {
                    new ApiResource("docmsapi", "文書管理システム API")
                    {
                        Scopes = new List<Scope>() {
                            new Scope("docmsapi", "文書管理システム API")
                        },
                        ApiSecrets = new List<Secret>()
                        {
                            new Secret("docmsapi-secret".Sha256())
                        }
                    }
                })
                .AddAspNetIdentity<ApplicationUser>();

            return services;
        }

        public static IServiceCollection AddCustomAuthentication(this IServiceCollection services, IConfiguration configuration)
        {
            // prevent from mapping "sub" claim to nameidentifier.
            JwtSecurityTokenHandler.DefaultInboundClaimTypeMap.Remove("sub");

            var identityUrl = configuration.GetValue<string>("IdentityUrl");

            services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
                .AddJwtBearer(options =>
                {
                    options.Authority = identityUrl;
                    options.RequireHttpsMetadata = false;
                    options.Audience = "docmsapi";
                });

            return services;
        }


        public static IServiceCollection RegisterServices(this IServiceCollection services, IConfiguration configuration)
        {
            services.AddScoped<IDocumentRepository, DocumentRepository>();
            services.AddTransient<DocumentsQueries>();
            services.AddTransient<DocumentHistoriesQueries>();
            services.AddSingleton<IFileStorage>(sv => new LocalFileStorage("App_Data/flies"));
            return services;
        }

        public static IServiceCollection RegisterMediators(this IServiceCollection services, IConfiguration configuration)
        {
            services.AddMediatR();
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
                serviceScope.ServiceProvider
                    .GetService<VisualizationSystemContext>()
                    .Database
                    .EnsureCreated();
            }
        }
    }
}
