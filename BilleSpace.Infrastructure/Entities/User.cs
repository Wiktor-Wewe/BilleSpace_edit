using Microsoft.AspNet.Identity;
using Microsoft.AspNetCore.Identity;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BilleSpace.Infrastructure.Entities
{
    public class User : IdentityUser
    {
        public List<Reservation> Reservations { get; set; }
    }
}
