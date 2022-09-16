using BilleSpace.Domain.Results;
using BilleSpace.Infrastructure;
using BilleSpace.Infrastructure.Entities;
using BilleSpace.Infrastructure.Models;
using MediatR;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using System.Security.Claims;
using System.Text.Json.Serialization;

namespace BilleSpace.Domain.CQRS
{
    public class ManageReservationCommand : IRequest<Result<ReservationModel>>
    {

        [JsonIgnore]
        public Guid Id { get; set; }
        public Guid OfficeId { get; set; }
        public Guid OfficeZoneId { get; set; }
        public Guid? ParkingZoneId { get; set; }
        public DateTime Date { get; set; }
        public string OfficeDesk { get; set; }
        public string? ParkingSpace { get; set; }
        public string UserEmail { get; set; }
    }

    public class MakeReservationCommandHandler : IRequestHandler<ManageReservationCommand, Result<ReservationModel>>
    {
        private readonly BilleSpaceDbContext _dbContext;
        private readonly ILogger<MakeReservationCommandHandler> _logger;

        public MakeReservationCommandHandler(BilleSpaceDbContext dbContext, ILogger<MakeReservationCommandHandler> logger)
        {
            _dbContext = dbContext;
            _logger = logger;
            _logger.LogInformation($"[{DateTime.UtcNow}] Object '{nameof(MakeReservationCommandHandler)}' has been created.");
        }

        public async Task<Result<ReservationModel>> Handle(ManageReservationCommand request, CancellationToken cancellationToken)
        {
            var isAdding = request.Id == Guid.Empty;
            Reservation? reservation = null;

            var office = await GetOfficeAsync(request.OfficeId);
            var officeZone = await GetOfficeZoneAsync(request.OfficeZoneId);

            ParkingZone? parkingZone = null;
            if (request.ParkingZoneId != null)
            {
                parkingZone = await GetParkingZoneAsync(request.ParkingZoneId);
            }

            var errorMessages = new List<string>();
            if (office == null)
            {
                errorMessages.Add($"[{DateTime.UtcNow}] Office with id: {request.OfficeId} does not exist.");
            }
            if (officeZone == null)
            {
                errorMessages.Add($"[{DateTime.UtcNow}] OfficeZone with id: {request.OfficeZoneId} does not exist.");
            }
            if (parkingZone == null && request.ParkingZoneId != null)
            {
                errorMessages.Add($"[{DateTime.UtcNow}] ParkingZone with id: {request.ParkingZoneId} does not exist.");
            }
            if (errorMessages.Count > 0)
            {
                _logger.LogError(String.Join(Environment.NewLine, errorMessages));
                return Result.BadRequest<ReservationModel>(errorMessages);
            }


            if (isAdding)
            {
                reservation = new Reservation()
                {
                    Date = request.Date,
                    OfficeId = request.OfficeId,
                    Office = office,
                    OfficeZoneId = request.OfficeZoneId,
                    OfficeZone = officeZone,
                    ParkingZone = parkingZone,
                    UserEmail = request.UserEmail,
                    OfficeDesk = request.OfficeDesk,
                    ParkingSpace = request.ParkingSpace
                };

                if(request.ParkingZoneId != null)
                {
                    reservation.ParkingZoneId = request.ParkingZoneId;
                }

                var reservationFromDb = await _dbContext.Reservations
                    .FirstOrDefaultAsync(res => res.Date.Date == reservation.Date.Date &&
                                  res.OfficeId == reservation.OfficeId &&
                                  res.OfficeZoneId == reservation.OfficeZoneId &&
                                  res.ParkingZoneId == reservation.ParkingZoneId &&
                                  res.ParkingSpace == reservation.ParkingSpace ||
                                  res.OfficeDesk == reservation.OfficeDesk,
                                  cancellationToken);

                if (reservationFromDb != null)
                {
                    return Result.BadRequest<ReservationModel>(new List<string>() { "This seat is already reserved." });
                }

                try
                {
                    await _dbContext.Reservations.AddAsync(reservation, cancellationToken);
                    await _dbContext.SaveChangesAsync(cancellationToken);
                    _logger.LogInformation($"[{DateTime.UtcNow}] Add new reservation.");
                }
                catch (Exception e)
                {
                    _logger.LogError($"[{DateTime.UtcNow}] Can't add new reservation:");
                    _logger.LogError(e.Message);
                    return Result.BadRequest<ReservationModel>(new List<string>() { "Can't add new reservation." });
                }
            }
            else
            {
                reservation = await _dbContext.Reservations.FirstOrDefaultAsync(res => res.Id == request.Id);

                if (reservation == null)
                {
                    return Result.BadRequest<ReservationModel>(new List<string>() { $"Reservation with id: {request.Id} does not exist." });
                }

                reservation.Date = request.Date;
                reservation.OfficeId = request.OfficeId;
                reservation.Office = office;
                reservation.OfficeZoneId = request.OfficeZoneId;
                reservation.OfficeZone = officeZone;
                reservation.ParkingZoneId = request.OfficeZoneId;
                reservation.ParkingZone = parkingZone;
                reservation.UserEmail = request.UserEmail;

                try
                {
                    _dbContext.Reservations.Update(reservation);
                    await _dbContext.SaveChangesAsync(cancellationToken);
                    _logger.LogInformation($"[{DateTime.UtcNow}] Update reservation (id: {request.Id}).");
                }
                catch (Exception e)
                {
                    _logger.LogError($"[{DateTime.UtcNow}] Can't edit reservation (id: {request.Id}):");
                    _logger.LogError(e.Message);
                    return Result.BadRequest<ReservationModel>(new List<string>() { "Can't edit reservation." });
                }

            }

            var model = new ReservationModel()
            {
                Id = reservation.Id,
                Date = reservation.Date,
                Office = new OfficeModel()
                {
                    Id = reservation.OfficeId,
                    Address = reservation.Office.Address,
                    PostCode = reservation.Office.PostCode,
                    OfficeMapUrl = reservation.Office.OfficeMapUrl,
                    City = new CityModel()
                    {
                        Id = reservation.Office.CityId,
                        Name = reservation.Office.City.Name,
                        Country = new CountryModel()
                        {
                            Id = reservation.Office.City.CountryId,
                            Name = reservation.Office.City.Country.Name,
                            Symbol = reservation.Office.City.Country.Symbol
                        }
                    },
                    OfficeZones = reservation.Office.OfficeZones.Select(x => new OfficeZoneModel()
                    {
                        Id = x.Id,
                        Name = x.Name,
                        Desks = x.Desks
                    }),
                    ParkingZones = reservation.Office.ParkingZones.Select(x => new ParkingZoneModel()
                    {
                        Id = x.Id,
                        Name = x.Name,
                        Spaces = x.Spaces
                    }),
                },
                OfficeZone = new OfficeZoneModel()
                {
                    Id = reservation.OfficeZoneId,
                    Name = reservation.OfficeZone.Name,
                    Desks = reservation.OfficeZone.Desks
                },
                OfficeDesk = reservation.OfficeDesk,
                ParkingZone = reservation.ParkingZoneId != null ? new ParkingZoneModel()
                {
                    Id = reservation.ParkingZone.Id,
                    Name = reservation.ParkingZone.Name,
                    Spaces = reservation.ParkingZone.Spaces
                } : null,
                ParkingSpace = reservation.ParkingSpace
            };

            return Result.Ok(model);
        }

        private async Task<Office> GetOfficeAsync(Guid OfficeId)
        {
            return await _dbContext.Offices
                .Include(cit => cit.City)
                    .ThenInclude(cou => cou.Country)
                .FirstOrDefaultAsync(off => off.Id == OfficeId);
        }

        private async Task<OfficeZone> GetOfficeZoneAsync(Guid OfficeZoneId)
        {
            return await _dbContext.OfficeZones
                .FirstOrDefaultAsync(off => off.Id == OfficeZoneId);
        }

        private async Task<ParkingZone> GetParkingZoneAsync(Guid? ParkingZoneId)
        {
            return await _dbContext.ParkingZones
                .FirstOrDefaultAsync(par => par.Id == ParkingZoneId);
        }
    }
}
