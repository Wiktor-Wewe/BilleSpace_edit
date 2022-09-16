using BilleSpace.Domain.Results;
using BilleSpace.Infrastructure.Entities;
using MediatR;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Logging;
using Microsoft.IdentityModel.Tokens;
using Microsoft.Extensions.Configuration;
using System;
using System.Collections.Generic;
using System.IdentityModel.Tokens.Jwt;
using System.Linq;
using System.Security.Claims;
using System.Text;
using System.Threading.Tasks;

namespace BilleSpace.Domain.CQRS
{
    public class LoginQuery : IRequest<Result<string>>
    {
        public string Email { get; set; }
        public string Password { get; set; }
    }

    public class LoginQueryHandler : IRequestHandler<LoginQuery, Result<string>>
    {
        private readonly UserManager<User> _userManager;
        private readonly ILogger<LoginQueryHandler> _logger;
        private readonly AuthenticationSettings _authenticationSettings;

        public LoginQueryHandler(UserManager<User> userManager, ILogger<LoginQueryHandler> logger, AuthenticationSettings authenticationSettings)
        {
            _userManager = userManager;
            _logger = logger;
            _logger.LogInformation($"[{DateTime.UtcNow}] Object {nameof(LoginQueryHandler)} has been created.");
            _authenticationSettings = authenticationSettings;
        }

        public async Task<Result<string>> Handle(LoginQuery request, CancellationToken cancellationToken)
        {
            var errors = new List<string>();
            var user = new User();
            try
            {
                user = await _userManager.FindByEmailAsync(request.Email.Normalize());
            }
            catch(Exception ex)
            {
                errors.Add(ex.Message);
                _logger.LogError($"[{DateTime.UtcNow}] {ex.Message}");
                return Result.BadRequest<string>(errors);
            }
            
            if (user == null)
            {
                errors.Add("Wrong Email");
                _logger.LogError($"[{DateTime.UtcNow}] Wrong Email");
                return Result.BadRequest<string>(errors);
            }

            if (!await _userManager.CheckPasswordAsync(user, request.Password))
            {
                errors.Add("Wrong password");
                _logger.LogError($"[{DateTime.UtcNow}] Wrong password");
                return Result.Forbidden<string>(errors);
            }

            //Generate token
            var claims = new List<Claim>()
            {
                new Claim(ClaimTypes.Name, user.Id),
                new Claim(ClaimTypes.Email, $"{user.Email}")
            };

            var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_authenticationSettings.JwtKey));
            var cred = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);
            var expires = DateTime.Now.AddDays(_authenticationSettings.JwtExpireDays);

            var token = new JwtSecurityToken(_authenticationSettings.JwtIssuer,
                _authenticationSettings.JwtIssuer,
                claims,
                expires: expires,
                signingCredentials: cred);

            var tokenHandler = new JwtSecurityTokenHandler();

            return Result.Ok(tokenHandler.WriteToken(token));
        }
    }
}
