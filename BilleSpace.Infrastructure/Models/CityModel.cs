namespace BilleSpace.Infrastructure.Models
{
    public class CityModel : BaseModel
    {
        public string Name { get; set; }
        public CountryModel Country { get; set; }
    }
}
