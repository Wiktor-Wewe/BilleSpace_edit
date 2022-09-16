using BilleSpace.Domain.Results;
using BilleSpace.Infrastructure;
using BilleSpace.Infrastructure.Entities;
using BilleSpace.Infrastructure.Models;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using System.Text.Json.Serialization;

namespace BilleSpace.Domain.CQRS
{
    public class ManageOfficeCommand : IRequest<Result<OfficeModel>>
    {
        [JsonIgnore]
        public Guid Id { get; set; }
        public string City { get; set; }
        public string Address { get; set; }
        public string PostCode { get; set; }
        public List<OfficeZoneModel> OfficeZones { get; set; }
        public List<ParkingZoneModel>? ParkingZones { get; set; }
        public string? OfficeMap { get; set; }

        [JsonIgnore]
        public string? AuthorEmail { get; set; }
    }

    public class ManageOfficeCommandHandler : IRequestHandler<ManageOfficeCommand, Result<OfficeModel>>
    {
        private readonly BilleSpaceDbContext _dbContext;
        private readonly ILogger<ManageOfficeCommandHandler> _logger;

        public ManageOfficeCommandHandler(BilleSpaceDbContext dbContext, ILogger<ManageOfficeCommandHandler> logger)
        {
            _dbContext = dbContext;
            _logger = logger;
            _logger.LogInformation($"[{DateTime.UtcNow}] Object {nameof(ManageOfficeCommandHandler)} has been created.");
        }

        public async Task<Result<OfficeModel>> Handle(ManageOfficeCommand request, CancellationToken cancellationToken)
        {
            // Required properties
            var isAdding = request.Id == Guid.Empty;
            Office? office = null;

            var city = await _dbContext.Cities
                .Include(x => x.Country)
                .FirstOrDefaultAsync(x => x.Name == request.City, cancellationToken);

            var officeByAddress = await _dbContext.Offices
                .Include(x => x.City)
                .FirstOrDefaultAsync(x => x.Address == request.Address, cancellationToken);

            // Declare if we want add or edit office
            if (isAdding)
            {
                office = new Office();
            }
            else
            {
                office = await _dbContext.Offices
                    .Include(x => x.OfficeZones)
                    .Include(x => x.ParkingZones)
                    .FirstOrDefaultAsync(x => x.Id == request.Id, cancellationToken);

                // Return error message if user is not office creator
                if (office.AuthorEmail != request.AuthorEmail)
                {
                    _logger.LogError($"[{DateTime.UtcNow}] User with {request.AuthorEmail} Email can not edit this office!");
                    return Result.Forbidden<OfficeModel>(new List<string>() { $"[{DateTime.UtcNow}] User with {request.AuthorEmail} Identifier can not edit this office!" });
                }
            }

            // Validation
            List<string> errorMessages = new List<string>();

            // OfficeZone
            if (request.OfficeZones.Count != request.OfficeZones.DistinctBy(x => x.Name).Count())
            {
                errorMessages.Add($"[{DateTime.UtcNow}] Office Zones must have different name.");
            }

            if (request.OfficeZones.Any(x => string.IsNullOrEmpty(x.Name)))
            {
                errorMessages.Add($"[{DateTime.UtcNow}] OffizeZones can not be null.");
            }

            // Address
            if (officeByAddress != null && officeByAddress != office && officeByAddress.City.Name == request.City)
            {
                errorMessages.Add($"[{DateTime.UtcNow}] Address {officeByAddress.Address} already taken.");
            }

            // City
            if (city == null)
            {
                errorMessages.Add($"[{DateTime.UtcNow}] Can not find city {request.City}.");
            }

            // If any error happened return Result BadRequest with error messages
            if (errorMessages.Count > 0)
            {
                _logger.LogError(string.Join(Environment.NewLine, errorMessages));
                return Result.BadRequest<OfficeModel>(errorMessages);
            }

            // Setup for officeZone property
            if (!isAdding)
            {
                // Office Zone - Delete if not exist in payload, but exist in office
                var officeZonesToDelete = office.OfficeZones
                    .Where(x => request.OfficeZones.All(y => y.Name != x.Name))
                    .ToList();

                _logger.LogInformation($"[{DateTime.UtcNow}] Deleted {officeZonesToDelete.Count} officeZones from office with id: {office.Id}.");
                _dbContext.OfficeZones.RemoveRange(officeZonesToDelete);
                office.OfficeZones.RemoveAll(x => x == officeZonesToDelete.FirstOrDefault(y => y.Id == x.Id));

                // Office Zone - Edit office zones
                foreach (var oldZone in office.OfficeZones)
                {
                    _logger.LogInformation($"[{DateTime.UtcNow}] Changes in officeZone with {oldZone.Id} Id.");
                    var officeZoneToEdit = request.OfficeZones.FirstOrDefault(x => x.Name == oldZone.Name);
                    oldZone.Desks = officeZoneToEdit.Desks;
                }
            }

            // Office Zone - Add office zone if exist in payload, but not exist in office
            List<OfficeZoneModel>? officeZonesToCreate = null;
            if (isAdding)
            {
                officeZonesToCreate = request.OfficeZones;
            }
            else
            {
                officeZonesToCreate = request.OfficeZones
                    .Where(x => office.OfficeZones.All(y => y.Name != x.Name))
                    .ToList();
            }

            // Map OfficeZoneModel to OfficeZone
            _logger.LogInformation($"[{DateTime.UtcNow}] Created {officeZonesToCreate.Count} officeZone(s) for office with {office.Id} Id.");
            office.OfficeZones
                .AddRange(officeZonesToCreate
                .Select(x => new OfficeZone { Id = x.Id, Name = x.Name, Desks = x.Desks }));


            // Setup for parkingZone property
            if (!isAdding)
            {
                // Parking Zone - Delete if not exist in payload, but exist in office
                var parkingZonesToDelete = office.ParkingZones
                    .Where(x => request.ParkingZones.All(y => y.Name != x.Name))
                    .ToList();

                _logger.LogInformation($"[{DateTime.UtcNow}] Deleted {parkingZonesToDelete.Count} parkingZones from office with id: {office.Id}.");
                _dbContext.ParkingZones.RemoveRange(parkingZonesToDelete);
                office.ParkingZones.RemoveAll(x => x == parkingZonesToDelete.FirstOrDefault(y => y.Id == x.Id));

                // Parking Zone - Edit if exist in both payload and in office
                foreach (var oldZone in office.ParkingZones)
                {
                    _logger.LogInformation($"[{DateTime.UtcNow}] Changes in parkingZone with {oldZone.Id} Id.");
                    var parkingZoneToEdit = request.ParkingZones.FirstOrDefault(x => x.Name == oldZone.Name);
                    oldZone.Spaces = parkingZoneToEdit.Spaces;
                }
            }            

            // Parking Zone - Add if exist in payload, but not exist in office
            List<ParkingZoneModel>? parkingZonesToCreate = null;
            if (isAdding)
            {
                parkingZonesToCreate = request.ParkingZones;
            }
            else
            {
                parkingZonesToCreate = request.ParkingZones
                    .Where(x => office.ParkingZones.All(y => y.Name != x.Name))
                    .ToList();
            }

            // Map ParkingZoneModel to ParkingZone
            _logger.LogInformation($"[{DateTime.UtcNow}] Created {parkingZonesToCreate.Count} parkingZone(s) for office with {office.Id} Id.");
            office.ParkingZones.AddRange(parkingZonesToCreate.Select(x => new ParkingZone { Id = x.Id, Name = x.Name, Spaces = x.Spaces }));


            // Overwrite other properties with current data 
            office.CityId = city.Id;
            office.Address = request.Address;
            office.PostCode = request.PostCode;
            office.OfficeMapUrl = request.OfficeMap;
            office.AuthorEmail = request.AuthorEmail;

            // Save new office or edited changes
            if (isAdding)
            {
                
                _logger.LogInformation($"[{DateTime.UtcNow}] Office created by user with {request.AuthorEmail} email.");
                await _dbContext.Offices.AddAsync(office, cancellationToken);
            }
            else
            {
                _logger.LogInformation($"[{DateTime.UtcNow}] Office changed by user with {request.AuthorEmail} email.");
            }

            try
            {
                await _dbContext.SaveChangesAsync(cancellationToken);
            }
            catch (Exception ex)
            {
                _logger.LogError($"[{DateTime.UtcNow}] Something goes wrong during saving changes in database.");
                _logger.LogError($"[{DateTime.UtcNow}] {ex.Message}");
                return Result.BadRequest<OfficeModel>(new List<string>() { $"Error occurred while saving changes to database." });
            }

            // Create OfficeModel to response
            var data = OfficeModel.ToViewModel(office);

            return Result.Ok(data);
        }
    }
}
