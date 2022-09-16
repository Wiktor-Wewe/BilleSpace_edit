using BilleSpace.Domain.CQRS;
using BilleSpace.Infrastructure;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.VisualStudio.TestPlatform.TestHost;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace BilleSpace.IntegrationTests.E2E_Office_Tests
{
    public class IntegrationTest
    {
        protected readonly HttpClient TestClient;

        public IntegrationTest()
        {
            var appFactor = new WebApplicationFactory<Program>()
                .WithWebHostBuilder(builder =>
                {
                    builder.ConfigureServices(services =>
                    {
                        var dbContext = services.SingleOrDefault(d => d.ServiceType == typeof(DbContextOptions<BilleSpaceDbContext>));

                        if (dbContext != null)
                        {
                            services.Remove(dbContext);
                        }
                        var serviceProvider = new ServiceCollection().AddEntityFrameworkInMemoryDatabase().BuildServiceProvider();

                        services.AddDbContext<BilleSpaceDbContext>(options =>
                        {
                            options.UseInMemoryDatabase("InMemoryEmployeeTest");
                            options.UseInternalServiceProvider(serviceProvider);
                        });
                        var sp = services.BuildServiceProvider();

                        using (var scope = sp.CreateScope())
                        {
                            using (var appContext = scope.ServiceProvider.GetRequiredService<BilleSpaceDbContext>())
                            {
                                try
                                {
                                    appContext.Database.EnsureCreated();
                                }
                                catch (Exception ex)
                                {
                                    throw;
                                }
                            }
                        }
                    });
                });
            TestClient = appFactor.CreateClient();
        }

        protected async Task Register()
        {
            TestClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", await RegisterGetToken());
        }

        protected async Task Login()
        {
            TestClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", await LoginGetToken());
        }

        private async Task<string> LoginGetToken()
        {
            Regex rx = new Regex(@"[a-zA-Z0-9_-]+[.][a-zA-Z0-9_-]+[.][a-zA-Z0-9_-]+", RegexOptions.Compiled | RegexOptions.IgnoreCase);

            var response = await TestClient.PostAsJsonAsync("api/users/login", new LoginQuery()
            {
                Email = "test@wp.pl",
                Password = "ZAQ!2wsx"
            });

            string registerResponse = await response.Content.ReadAsStringAsync();

            MatchCollection matches = rx.Matches(registerResponse);

            return matches[0].ToString();
        }

        private async Task<string> RegisterGetToken()
        {
            Regex rx = new Regex(@"[a-zA-Z0-9_-]+[.][a-zA-Z0-9_-]+[.][a-zA-Z0-9_-]+", RegexOptions.Compiled | RegexOptions.IgnoreCase);

            var response = await TestClient.PostAsJsonAsync("api/users", new RegisterCommand
            {
                Username = "test",
                Email = "test@wp.pl",
                Password = "ZAQ!2wsx"
            });

            var registerResponse = await response.Content.ReadAsStringAsync();

            MatchCollection matches = rx.Matches(registerResponse);

            return matches[0].ToString();
        }

    }
}
