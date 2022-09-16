using BilleSpace.Infrastructure;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using System.Security.Claims;

namespace BilleSpace.Authorization
{
    public class OnlyReceptionist : AuthorizeAttribute, IAuthorizationFilter
    {
        public async void OnAuthorization(AuthorizationFilterContext context)
        {
            var dbContext = context.HttpContext
            .RequestServices
            .GetService(typeof(BilleSpaceDbContext)) as BilleSpaceDbContext;

            var email = context.HttpContext.User.FindFirstValue(ClaimTypes.Email);

            var pass = dbContext.Receptionists.Any(rec => rec.UserEmail == email);

            if (!pass)
            {
                context.Result = new UnauthorizedResult();
            }
        }

    }
}
