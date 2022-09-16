using BilleSpace.Domain.Results;
using BilleSpace.Infrastructure;
using BilleSpace.Infrastructure.Entities;
using BilleSpace.Infrastructure.Models;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace BilleSpace.Domain.CQRS
{
    public class LoadOfficeQuery : IRequest<Result<OfficeModel>>
    {
        public Guid Id { get; set; }

        public LoadOfficeQuery(Guid id)
        {
            Id = id;
        }
    }

    public class LoadOfficeQueryHandler : IRequestHandler<LoadOfficeQuery, Result<OfficeModel>>
    {
        private readonly BilleSpaceDbContext _dbContext;
        private readonly ILogger<LoadOfficeQueryHandler> _logger;

        public LoadOfficeQueryHandler(BilleSpaceDbContext dbContext, ILogger<LoadOfficeQueryHandler> logger)
        {
            _dbContext = dbContext;
            _logger = logger;
            _logger.LogInformation($"[{DateTime.UtcNow}] Object '{nameof(LoadOfficeQueryHandler)}' has been created.");
        }

        public async Task<Result<OfficeModel>> Handle(LoadOfficeQuery request, CancellationToken cancellationToken)
        {
            var officeQuery = _dbContext.Offices
                .Include(cit => cit.City)
                    .ThenInclude(cou => cou.Country)
                .Include(off => off.OfficeZones)
                .Include(par => par.ParkingZones)
                .Select(off => new OfficeModel()
                {
                    Address = off.Address,
                    Id = off.Id,
                    OfficeMapUrl = off.OfficeMapUrl,
                    PostCode = off.PostCode,
                    City = new CityModel()
                    {
                        Country = new CountryModel()
                        {
                            Id = off.City.Country.Id,
                            Name = off.City.Country.Name,
                            Symbol = off.City.Country.Symbol
                        },
                        Id = off.City.Id,
                        Name = off.City.Name,
                    },
                    OfficeZones = off.OfficeZones.Select(offz => new OfficeZoneModel()
                    {
                        Desks = offz.Desks,
                        Id = offz.Id,
                        Name = offz.Name
                    }),
                    ParkingZones = off.ParkingZones.Select(par => new ParkingZoneModel()
                    {
                        Id = par.Id,
                        Name = par.Name,
                        Spaces = par.Spaces,
                    })
                })
                .AsNoTracking();

            OfficeModel? office = null;
            var errors = new List<string>();
            try
            {
                office = await officeQuery
                .FirstOrDefaultAsync(off => off.Id == request.Id, cancellationToken);
            }
            catch (Exception e)
            {
                errors.Add(e.Message);
                _logger.LogError(String.Join(Environment.NewLine, errors));
                return Result.BadRequest<OfficeModel>(errors);
            }

            if(office == null)
            {
                return Result.NotFound<OfficeModel>(request.Id);
            }

            return Result.Ok(office);
        }

    }
}
