using BilleSpace.Domain.Results;
using BilleSpace.Infrastructure;
using BilleSpace.Infrastructure.Models;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace BilleSpace.Domain.CQRS
{
    public class LoadReservationsQuery : IRequest<Result<List<ReservationModel>>>
    {
    }

    public class LoadReservationsQueryHandler : IRequestHandler<LoadReservationsQuery, Result<List<ReservationModel>>>
    {
        private readonly BilleSpaceDbContext _context;
        private readonly ILogger<LoadReservationsQueryHandler> _logger;

        public LoadReservationsQueryHandler(BilleSpaceDbContext context, ILogger<LoadReservationsQueryHandler> logger)
        {
            _context = context;
            _logger = logger;
            _logger.LogInformation($"[{DateTime.UtcNow}] Object {nameof(LoadReservationsQueryHandler)} has been created.");
        }

        public async Task<Result<List<ReservationModel>>> Handle(LoadReservationsQuery request, CancellationToken cancellationToken)
        {
            var data = _context.Reservations
                .Include(y => y.Office)
                    .ThenInclude(y => y.City)
                        .ThenInclude(y => y.Country)
                .Include(y => y.OfficeZone)
                .Include(y => y.ParkingZone)
                .Select(z => new ReservationModel()
                {
                    Id = z.Id,
                    Date = z.Date,
                    OfficeDesk = z.OfficeDesk,
                    ParkingSpace = z.ParkingSpace,
                    Office = new OfficeModel()
                    {
                        Id = z.OfficeId,
                        Address = z.Office.Address,
                        PostCode = z.Office.PostCode,
                        City = new CityModel()
                        {
                            Id = z.Office.CityId,
                            Name = z.Office.City.Name,
                            Country = new CountryModel()
                            {
                                Id = z.Office.City.CountryId,
                                Name = z.Office.City.Country.Name,
                                Symbol = z.Office.City.Country.Symbol
                            }
                        },
                        OfficeZones = z.Office.OfficeZones.Select(off => new OfficeZoneModel()
                        {
                            Id = off.Id,
                            Name = off.Name,
                            Desks = off.Desks
                        }),
                        ParkingZones = z.Office.ParkingZones.Select(par => new ParkingZoneModel()
                        {
                            Id = par.Id,
                            Name = par.Name,
                            Spaces = par.Spaces
                        }),
                        OfficeMapUrl = z.Office.OfficeMapUrl,
                    },
                    OfficeZone = new OfficeZoneModel()
                    {
                        Id = z.OfficeZoneId,
                        Name = z.OfficeZone.Name,
                        Desks = z.OfficeZone.Desks
                    },
                    ParkingZone = z.ParkingZoneId != null ? new ParkingZoneModel()
                    {
                        Id = z.ParkingZone.Id,
                        Name = z.ParkingZone.Name,
                        Spaces = z.ParkingZone.Spaces
                    } : null,
                })
                .AsNoTracking();

            List<ReservationModel>? response = null;
            var errors = new List<string>();
            try
            {
                response = await data.ToListAsync(cancellationToken);
            }
            catch (Exception ex)
            {
                errors.Add(ex.Message);
                _logger.LogError(ex.Message);
                return Result.BadRequest<List<ReservationModel>>(errors);
            }

            return Result.Ok(response);
        }
    }
}
