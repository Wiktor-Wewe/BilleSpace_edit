namespace BilleSpace.Infrastructure.Entities
{
    public class City
    {
        public Guid Id { get; set; }
        public string Name { get; set; }

        public Guid CountryId { get; set; }
        public Country Country { get; set; }
    }
}
