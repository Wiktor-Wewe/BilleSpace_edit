using BilleSpace.Infrastructure.Entities;
using Duende.IdentityServer.EntityFramework.Options;
using Microsoft.AspNetCore.ApiAuthorization.IdentityServer;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;

namespace BilleSpace.Infrastructure
{
    public class BilleSpaceDbContext : ApiAuthorizationDbContext<User>
    {
        public BilleSpaceDbContext(DbContextOptions options, IOptions<OperationalStoreOptions> operationalStoreOptions)
            : base(options, operationalStoreOptions)
        {
        }

        public DbSet<City> Cities { get; set; }
        public DbSet<Country> Countries { get; set; }
        public DbSet<Office> Offices { get; set; }
        public DbSet<OfficeZone> OfficeZones { get; set; }
        public DbSet<ParkingZone> ParkingZones { get; set; }
        public DbSet<Reservation> Reservations { get; set; }
        public DbSet<Receptionist> Receptionists { get; set; }
        public DbSet<User> Users { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            // Office
            modelBuilder.Entity<Office>()
                .Property(x => x.CityId)
                .IsRequired();

            modelBuilder.Entity<Office>()
                .Property(x => x.Address)
                .IsRequired();

            modelBuilder.Entity<Office>()
                .Property(x => x.PostCode)
                .IsRequired();

            base.OnModelCreating(modelBuilder);
        }
    }
}
