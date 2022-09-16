using BilleSpace.Domain.Results;
using BilleSpace.Infrastructure;
using BilleSpace.Infrastructure.Entities;
using BilleSpace.Infrastructure.Models;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace BilleSpace.Domain.CQRS
{
    public class LoadOfficesQuery : IRequest<Result<List<OfficeModel>>>
    {

    }

    public class LoadOfficesQueryHandler : IRequestHandler<LoadOfficesQuery, Result<List<OfficeModel>>>
    {
        private readonly BilleSpaceDbContext _dbContext;
        private readonly ILogger<LoadOfficesQueryHandler> _logger;

        public LoadOfficesQueryHandler(BilleSpaceDbContext dbContext, ILogger<LoadOfficesQueryHandler> logger)
        {
            _dbContext = dbContext;
            _logger = logger;
            _logger.LogInformation($"[{DateTime.UtcNow}] Object '{nameof(LoadOfficesQueryHandler)}' has been created.");
        }

        public async Task<Result<List<OfficeModel>>> Handle(LoadOfficesQuery request, CancellationToken cancellationToken)
        {

            var officesQuery = _dbContext.Offices
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

            List<OfficeModel>? offices = null;
            var errors = new List<string>();
            try
            {
                offices = await officesQuery
                .ToListAsync(cancellationToken);
            }
            catch (Exception e)
            {
                errors.Add(e.Message);
                _logger.LogError(String.Join(Environment.NewLine, errors));
                return Result.BadRequest<List<OfficeModel>>(errors);
            }

            if (offices == null)
            {
                errors.Add("Can't find any office");
                _logger.LogError(String.Join(Environment.NewLine, errors));
                return Result.Forbidden<List<OfficeModel>>(errors);
            }

            return Result.Ok(offices);
        }
    }
}
