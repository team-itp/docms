using System;
using Docms.Web.Data;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.AspNetCore.TestHost;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace Docms.Web.Tests.Utils
{
    static class TestUtils
    {
        public static WebApplicationFactory<Startup> CreateWebApplicationFactory()
        {
            var factory = new WebApplicationFactory<Startup>();
            return factory.WithWebHostBuilder(builder =>
            {
                builder.ConfigureServices(services =>
                {
                    // Create a new service provider.
                    var serviceProvider = new ServiceCollection()
                        .AddEntityFrameworkInMemoryDatabase()
                        .BuildServiceProvider();

                    // Add a database context (ApplicationDbContext) using an in-memory 
                    // database for testing.
                    services.AddDbContext<DocmsDbContext>(options =>
                    {
                        options.UseInMemoryDatabase("InMemoryDb");
                        options.UseInternalServiceProvider(serviceProvider);
                    });

                    // Build the service provider.
                    var sp = services.BuildServiceProvider();

                    // Create a scope to obtain a reference to the database
                    // context (ApplicationDbContext).
                    using (var scope = sp.CreateScope())
                    {
                        var scopedServices = scope.ServiceProvider;
                        var db = scopedServices.GetRequiredService<DocmsDbContext>();
                        var logger = scopedServices
                            .GetRequiredService<ILogger<WebApplicationFactory<Startup>>>();

                        // Ensure the database is created.
                        db.Database.EnsureCreated();

                        try
                        {
                            // Seed the database with test data.
                            InitializeDbForTests(db);
                        }
                        catch (Exception ex)
                        {
                            logger.LogError(ex, $"An error occurred seeding the " +
                                "database with test messages. Error: {ex.Message}");
                        }
                    }
                });
            });
        }

        public static void InitializeDbForTests(DocmsDbContext db)
        {
        }
    }
}