using BilleSpace.Domain.CQRS;
using BilleSpace.Infrastructure;
using BilleSpace.Infrastructure.Entities;
using BilleSpace.Infrastructure.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Duende.IdentityServer.EntityFramework.Options;
using Moq;

namespace BilleSpace.UnitTests.ReservationTests
{
    [Category("DeleteReservationTests")]
    public class DeleteReservationTests
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
        public async Task DeleteReservationTest_WithCorrectData_ReturnResultOk()
        {
            //Arrange
            Reservation reservation = new Reservation()
            {
                Id = Guid.Parse("b7f4bc32-fb72-4e5a-9059-6fcd578ebcef"),
                OfficeId = Guid.Empty,
                Date = DateTime.UtcNow,
                UserEmail = "somemail@wp.pl",
                ParkingZoneId = Guid.Empty,
                OfficeZoneId = Guid.Empty,
                Office = new Office()
                {
                    Address = String.Empty,
                    AuthorEmail = "somemail@wp.pl",
                    City = new City()
                    {
                        CountryId = Guid.Empty,
                        Country = new Country()
                        {
                            Id = Guid.Empty,
                            Name = "asd",
                            Symbol = "asd"
                        },
                        Id = Guid.Empty,
                        Name = "yes"
                    },
                    CityId = Guid.Empty,
                    Id = Guid.Empty,
                    OfficeMapUrl = String.Empty,
                    PostCode = String.Empty,
                    OfficeZones = new List<OfficeZone>()
                    {
                        new OfficeZone()
                        {
                            Desks = 12,
                            Id = Guid.Empty,
                            Name = "as"
                        }
                    },
                    ParkingZones = new List<ParkingZone>()
                    {
                        new ParkingZone()
                        {
                            Id = Guid.Empty,
                            Name = String.Empty,
                            Spaces = 12
                        }
                    }
                },
                OfficeDesk = String.Empty,
                OfficeZone = new OfficeZone()
                {
                    Desks = 12,
                    Id = Guid.Empty,
                    Name = "as"
                },
                ParkingSpace = String.Empty,
                ParkingZone = new ParkingZone()
                {
                    Id = Guid.Empty,
                    Name = "as",
                    Spaces = 12
                }
            };

            var request = new DeleteReservationCommand(Guid.Parse("b7f4bc32-fb72-4e5a-9059-6fcd578ebcef"))
            {

            };

            CancellationTokenSource cancellationTokenSource = new CancellationTokenSource();

            var context = await GetDatabaseContext();
            context.Reservations.RemoveRange(context.Reservations);
            context.Reservations.Add(reservation);
            await context.SaveChangesAsync();

            Mock<ILogger<DeleteReservationCommandHandler>> mockLogger = new Mock<ILogger<DeleteReservationCommandHandler>>();

            var handler = new DeleteReservationCommandHandler(context, mockLogger.Object);

            // Act
            var result = await handler.Handle(request, cancellationTokenSource.Token);

            // Assert 
            Assert.That(result.Code, Is.EqualTo(200));
            Assert.That(result.Errors, Is.Null);
        }

        [Test]
        public async Task DeleteReservationTest_WhenReservationNotFoundInDb_ReturnResultNotFound()
        {
            //Arrange
            var request = new DeleteReservationCommand(Guid.Parse("b7f4bc32-fb72-4e5a-9059-6fcd578ebcef"))
            {

            };

            CancellationTokenSource cancellationTokenSource = new CancellationTokenSource();

            var context = await GetDatabaseContext();
            context.Reservations.RemoveRange(context.Reservations);
            await context.SaveChangesAsync();

            Mock<ILogger<DeleteReservationCommandHandler>> mockLogger = new Mock<ILogger<DeleteReservationCommandHandler>>();

            var handler = new DeleteReservationCommandHandler(context, mockLogger.Object);

            // Act
            var result = await handler.Handle(request, cancellationTokenSource.Token);

            // Assert 
            Assert.That(result.Code, Is.EqualTo(404));
            Assert.That(result.Errors[0], Is.EqualTo("There is no object with id: b7f4bc32-fb72-4e5a-9059-6fcd578ebcef"));
        }

    }
}
