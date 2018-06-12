using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.DependencyInjection;
using Swashbuckle.AspNetCore.Swagger;
using System.Collections.Generic;

namespace Docms.Web.Swagger
{
    public static class SwaggerExtension
    {
        public static void AddSwashbuckle(this IServiceCollection services)
        {
            services.AddSwaggerGen(c =>
            {
                c.SwaggerDoc("v1", new Info
                {
                    Version = "v1",
                    Title = "DocMS API",
                    Description = "Document Management System for small companies Web API",
                    TermsOfService = "None"
                });

                c.AddSecurityDefinition("Bearer", new OAuth2Scheme()
                {
                    Description = "OAuth2 クライアント認可フロー",
                    Flow = "implicit",
                    Scopes = new Dictionary<string, string>() {
                        { "docmsapi", "try out the api" }
                    },
                    TokenUrl = "/connect/token",
                    AuthorizationUrl = "/connect/authorize",
                });

                c.DocumentFilter<SecurityRequirementsDocumentFilter>();
            });
        }

        public static void UseSwashbuckle(this IApplicationBuilder app)
        {
            // Enable middleware to serve generated Swagger as a JSON endpoint.
            app.UseSwagger();

            // Enable middleware to serve swagger-ui (HTML, JS, CSS, etc.), 
            // specifying the Swagger JSON endpoint.
            app.UseSwaggerUI(c =>
            {
                c.SwaggerEndpoint("/swagger/v1/swagger.json", "DocMS API v1");
                c.OAuthClientId("swagger");
                c.OAuthClientSecret("swagger-client-secret");
            });
        }
    }
}
