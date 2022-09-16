using BilleSpace.Infrastructure;
using BilleSpace.Infrastructure.Entities;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.VisualStudio.TestPlatform.TestHost;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http.Headers;
using System.Text;
using System.Threading.Tasks;

namespace BilleSpace.IntegrationTests
{
    public class Setup
    {
        protected readonly HttpClient _httpClient;

        protected Setup()
        {
            var webAppFactory = new WebApplicationFactory<Program>()
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
                            options.UseInMemoryDatabase("InMemoryIntegrationTest");
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
                                    Seeder(appContext);
                                }
                                catch (Exception ex)
                                {
                                    throw;
                                }
                            }
                        }

                        services.AddAuthentication("Test")
                            .AddScheme<AuthenticationSchemeOptions, TestAuthHandler>(
                            "Test", options => { });
                    });
                });

            _httpClient = webAppFactory.CreateDefaultClient();
        }

        protected void AuthenticateAsync()
        {
            _httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Test");
        }

        protected void Seeder(BilleSpaceDbContext appContext)
        {
            Country country = new Country() { Id = Guid.Parse("0581dee8-c905-4f2d-91ad-3adb279e394d"), Name = "Poland", Symbol = "PL" };
            appContext.Countries.Add(country);

            City city = new City() { Id = Guid.Parse("30bad866-aadf-4cac-9195-74b5874b6876"), Name = "Olsztyn", Country = country };
            appContext.Cities.Add(city);

            Receptionist receptionist = new Receptionist() { UserEmail = "Zdzisiek@wp.pl" };
            appContext.Receptionists.Add(receptionist);

            appContext.Offices.Add(new Office()
            {
                Id = Guid.Parse("e15ff775-3a89-4f0e-a037-78bf1c7b0d8c"),
                Address = "address",
                PostCode = "12-123",
                CityId = city.Id,
                City = city,
                AuthorEmail = "Zdzisiek@wp.pl",
                OfficeMapUrl = null,
                OfficeZones = new List<OfficeZone>() { new OfficeZone() { Id = Guid.Parse("b2a24a75-5b26-456a-aa1d-f02821be6d4a"), Name = "OfficeZone", Desks = 5 } },
                ParkingZones = new List<ParkingZone>() { new ParkingZone() { Id = Guid.Parse("c0988a2e-24f5-4a82-b4c2-3619f338a2eb"), Name = "ParkingZone", Spaces = 15 } },
            });

            appContext.SaveChanges();
        }
    }
}
