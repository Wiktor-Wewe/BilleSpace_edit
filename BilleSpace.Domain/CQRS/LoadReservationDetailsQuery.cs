using BilleSpace.Domain.Results;
using BilleSpace.Infrastructure;
using BilleSpace.Infrastructure.Models;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace BilleSpace.Domain.CQRS
{
    public class LoadReservationDetailsQuery : IRequest<Result<ReservationModel>>
    {
        public Guid Id { get; set; }

        public LoadReservationDetailsQuery(Guid id)
        {
            Id = id;
        }
    }

    public class LoadReservationDetailsQueryHandler : IRequestHandler<LoadReservationDetailsQuery, Result<ReservationModel>>
    {
        private readonly BilleSpaceDbContext _context;
        private readonly ILogger<LoadReservationDetailsQueryHandler> _logger;

        public LoadReservationDetailsQueryHandler(BilleSpaceDbContext context, ILogger<LoadReservationDetailsQueryHandler> logger)
        {
            _context = context;
            _logger = logger;
            _logger.LogInformation($"[{DateTime.UtcNow}] Object {nameof(LoadReservationDetailsQueryHandler)} has been created.");
        }

        public async Task<Result<ReservationModel>> Handle(LoadReservationDetailsQuery request, CancellationToken cancellationToken)
        {
            var data = _context.Reservations
                .Include(x => x.Office)
                    .ThenInclude(x => x.City)
                        .ThenInclude(x => x.Country)
                .Include(x => x.OfficeZone)
                .Include(x => x.ParkingZone)
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

            ReservationModel? response = null;
            var errors = new List<string>();

            try
            {
                response = await data.FirstOrDefaultAsync(x => x.Id == request.Id);
            }
            catch (Exception ex)
            {
                errors.Add(ex.Message);
                _logger.LogError(ex.Message);
                return Result.BadRequest<ReservationModel>(errors);
            }

            if (response == null)
            {
                _logger.LogError($"Can't find reservation with id: {request.Id}");
                return Result.NotFound<ReservationModel>(request.Id);
            }

            return Result.Ok(response);
        }
    }
}
