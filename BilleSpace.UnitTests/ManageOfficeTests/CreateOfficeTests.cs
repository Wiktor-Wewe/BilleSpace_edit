using BilleSpace.Domain.CQRS;
using BilleSpace.Infrastructure;
using BilleSpace.Infrastructure.Entities;
using BilleSpace.Infrastructure.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Duende.IdentityServer.EntityFramework.Options;
using Moq;

namespace BilleSpace.UnitTests.ManageOfficeTests
{
    [Category("CreateOfficeTests")]
    public class CreateOfficeTests
    {
        private async Task<BilleSpaceDbContext> GetDatabaseContext()
        {
            var options = new DbContextOptionsBuilder<BilleSpaceDbContext>()
                .UseInMemoryDatabase("InMemoryDbToTest")
                .Options;
            var databaseContext = new BilleSpaceDbContext(options, Options.Create(new OperationalStoreOptions()));
            databaseContext.Database.EnsureCreated();

            return databaseContext;
        }

        [Test]
        public async Task CreateOffice_WithCorrectData_ReturnResultOKWithOfficeModel()
        {
            // Arrange
            City city = new City()
            {
                Name = "Olsztyn",
                Country = new Country()
                {
                    Id = Guid.NewGuid(),
                    Name = "Poland",
                    Symbol = "PL"
                }
            };

            var request = new ManageOfficeCommand
            {
                Id = Guid.Empty,
                City = city.Name,
                Address = "Olsztyn",
                PostCode = "Olsztyn",
                OfficeZones = new List<OfficeZoneModel> { new OfficeZoneModel
                {
                    Name = "OfficeZone",
                    Desks = 5,
                } },
                ParkingZones = new List<ParkingZoneModel>(),
                OfficeMap = null,
                AuthorEmail = "Author@wp.pl"
            };
            CancellationTokenSource cancellationTokenSource = new CancellationTokenSource();

            var context = await GetDatabaseContext();
            context.Offices.RemoveRange(context.Offices);
            context.Countries.Add(city.Country);
            context.Cities.Add(city);
            await context.SaveChangesAsync();

            Mock<ILogger<ManageOfficeCommandHandler>> mockLogger = new Mock<ILogger<ManageOfficeCommandHandler>>();

            var handler = new ManageOfficeCommandHandler(context, mockLogger.Object);

            // Act
            var result = await handler.Handle(request, cancellationTokenSource.Token);

            // Assert 
            Assert.That(result.Code, Is.EqualTo(200));
            Assert.That(result.Data.Address, Is.EqualTo("Olsztyn"));
            Assert.That(result.Errors, Is.Null);
        }

        [Test]
        public async Task CreateOffice_WithTwoSameOfficeZoneName_WithNotExistedCity_ReturnResultBadRequestWithErrorMessage()
        {
            // Arrange
            var request = new ManageOfficeCommand
            {
                Id = Guid.Empty,
                City = "NotExist",
                Address = "Olsztyn",
                PostCode = "Olsztyn",
                OfficeZones = new List<OfficeZoneModel> {
                    new OfficeZoneModel
                    {
                        Name = "OfficeZone",
                        Desks = 5,
                    },
                    new OfficeZoneModel
                    {
                        Name = "OfficeZone",
                        Desks = 10,
                    }
                },
                ParkingZones = new List<ParkingZoneModel>(),
                OfficeMap = null
            };
            CancellationTokenSource cancellationTokenSource = new CancellationTokenSource();

            var context = await GetDatabaseContext();
            context.Offices.RemoveRange(context.Offices);
            await context.SaveChangesAsync();

            Mock<ILogger<ManageOfficeCommandHandler>> mockLogger = new Mock<ILogger<ManageOfficeCommandHandler>>();

            var handler = new ManageOfficeCommandHandler(context, mockLogger.Object);

            // Act
            var result = await handler.Handle(request, cancellationTokenSource.Token);

            // Assert 
            Assert.That(result.Code, Is.EqualTo(400));
            Assert.That(result.Errors.Count, Is.EqualTo(2));
            Assert.That(result.Errors[0], Is.EqualTo($"[{DateTime.UtcNow}] Office Zones must have different name."));
            Assert.That(result.Errors[1], Is.EqualTo($"[{DateTime.UtcNow}] Can not find city {request.City}."));
        }

        [Test]
        public async Task CreateOffice_WithoutOfficeZoneName_ReturnResultBadRequestWithErrorMessage()
        {
            // Arrange
            City city = new City()
            {
                Name = "Olsztyn",
                Country = new Country()
                {
                    Id = Guid.NewGuid(),
                    Name = "Poland",
                    Symbol = "PL"
                }
            };

            var request = new ManageOfficeCommand
            {
                Id = Guid.Empty,
                City = "Olsztyn",
                Address = "Olsztyn",
                PostCode = "Olsztyn",
                OfficeZones = new List<OfficeZoneModel> {
                    new OfficeZoneModel
                    {
                        Name = "",
                        Desks = 5,
                    },
                    new OfficeZoneModel
                    {
                        Name = "OfficeZone",
                        Desks = 10,
                    }
                },
                ParkingZones = new List<ParkingZoneModel>(),
                OfficeMap = null
            };
            CancellationTokenSource cancellationTokenSource = new CancellationTokenSource();

            var context = await GetDatabaseContext();
            context.Offices.RemoveRange(context.Offices);
            context.Cities.Add(city);
            context.Countries.Add(city.Country);
            await context.SaveChangesAsync();

            Mock<ILogger<ManageOfficeCommandHandler>> mockLogger = new Mock<ILogger<ManageOfficeCommandHandler>>();

            var handler = new ManageOfficeCommandHandler(context, mockLogger.Object);

            // Act
            var result = await handler.Handle(request, cancellationTokenSource.Token);

            // Assert 
            Assert.That(result.Code, Is.EqualTo(400));
            Assert.That(result.Errors.Count, Is.EqualTo(1));
            Assert.That(result.Errors[0], Is.EqualTo($"[{DateTime.UtcNow}] OffizeZones can not be null."));
        }

        [Test]
        public async Task CreateOffice_WithExistedAddress_ReturnResultBadRequestWithErrorMessage()
        {
            // Arrange
            var office = new Office()
            {
                City = new City { Name = "Olsztyn", Country = new Country { Name = "Poland", Symbol = "PL" } },
                Address = "Olsztyn",
                PostCode = "Olsztyn",
                OfficeZones = new List<OfficeZone> {
                    new OfficeZone
                    {
                        Name = "OfficeZone",
                        Desks = 5,
                    },
                },
                ParkingZones = new List<ParkingZone>(),
                OfficeMapUrl = null,
                AuthorEmail = "Author@wp.pl"
            };

            var request = new ManageOfficeCommand
            {
                Id = Guid.Empty,
                City = "Olsztyn",
                Address = "Olsztyn",
                PostCode = "Olsztyn",
                OfficeZones = new List<OfficeZoneModel> {
                    new OfficeZoneModel
                    {
                        Name = "OfficeZone",
                        Desks = 5,
                    },
                },
                ParkingZones = new List<ParkingZoneModel>(),
                OfficeMap = null,
                AuthorEmail = "Author"
            };
            CancellationTokenSource cancellationTokenSource = new CancellationTokenSource();

            var context = await GetDatabaseContext();
            context.Offices.RemoveRange(context.Offices);
            context.Offices.Add(office);
            await context.SaveChangesAsync();

            Mock<ILogger<ManageOfficeCommandHandler>> mockLogger = new Mock<ILogger<ManageOfficeCommandHandler>>();

            var handler = new ManageOfficeCommandHandler(context, mockLogger.Object);

            // Act
            var result = await handler.Handle(request, cancellationTokenSource.Token);

            // Assert 
            Assert.That(result.Code, Is.EqualTo(400));
            Assert.That(result.Errors.Count, Is.EqualTo(1));
            Assert.That(result.Errors[0], Is.EqualTo($"[{DateTime.UtcNow}] Address {request.Address} already taken."));
        }

        [Test]
        public async Task CreateOffice_WithWrongCity_ReturnResultBadRequestWithErrorMessage()
        {
            // Arrange
            var request = new ManageOfficeCommand
            {
                Id = Guid.Empty,
                City = "NotExisted",
                Address = "NotExisted",
                PostCode = "Olsztyn",
                OfficeZones = new List<OfficeZoneModel> {
                    new OfficeZoneModel
                    {
                        Name = "OfficeZone",
                        Desks = 5,
                    },
                },
                ParkingZones = new List<ParkingZoneModel>(),
                OfficeMap = null
            };
            CancellationTokenSource cancellationTokenSource = new CancellationTokenSource();

            var context = await GetDatabaseContext();
            context.Offices.RemoveRange(context.Offices);
            await context.SaveChangesAsync();

            Mock<ILogger<ManageOfficeCommandHandler>> mockLogger = new Mock<ILogger<ManageOfficeCommandHandler>>();

            var handler = new ManageOfficeCommandHandler(context, mockLogger.Object);

            // Act
            var result = await handler.Handle(request, cancellationTokenSource.Token);

            // Assert 
            Assert.That(result.Code, Is.EqualTo(400));
            Assert.That(result.Errors.Count, Is.EqualTo(1));
            Assert.That(result.Errors[0], Is.EqualTo($"[{DateTime.UtcNow}] Can not find city {request.City}."));
        }

        [Test]
        public async Task CreateOffice_CancellationToken_ThrowOperationCanceledException()
        {
            // Arrange
            var request = new ManageOfficeCommand
            {
                Id = Guid.Empty,
                City = "NotExisted",
                Address = "NotExisted",
                PostCode = "Olsztyn",
                OfficeZones = new List<OfficeZoneModel> {
                    new OfficeZoneModel
                    {
                        Name = "OfficeZone",
                        Desks = 5,
                    },
                },
                ParkingZones = new List<ParkingZoneModel>(),
                OfficeMap = null
            };
            CancellationTokenSource cancellationTokenSource = new CancellationTokenSource();
            cancellationTokenSource.Cancel();

            var context = await GetDatabaseContext();
            context.Offices.RemoveRange(context.Offices);
            await context.SaveChangesAsync();

            Mock<ILogger<ManageOfficeCommandHandler>> mockLogger = new Mock<ILogger<ManageOfficeCommandHandler>>();

            var handler = new ManageOfficeCommandHandler(context, mockLogger.Object);

            // Act

            // Assert 
            Assert.ThrowsAsync(Is.TypeOf<OperationCanceledException>()
                .And.Message.EqualTo("The operation was canceled."), async delegate { await handler.Handle(request, cancellationTokenSource.Token); });
        }
    }
}
