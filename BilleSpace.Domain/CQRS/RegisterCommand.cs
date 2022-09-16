using BilleSpace.Domain.Results;
using BilleSpace.Infrastructure.Entities;
using MediatR;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Logging;
using Microsoft.IdentityModel.Tokens;
using System;
using System.Collections.Generic;
using System.IdentityModel.Tokens.Jwt;
using System.Linq;
using System.Security.Claims;
using System.Text;
using System.Threading.Tasks;

namespace BilleSpace.Domain.CQRS
{
    public class RegisterCommand : IRequest<Result<string>> 
    {
        public string Username { get; set; }
        public string Email { get; set; }
        public string Password { get; set; }
    }

    public class RegisterCommandHandler : IRequestHandler<RegisterCommand, Result<string>>
    {
        private readonly UserManager<User> _userManager;
        private readonly ILogger<RegisterCommandHandler> _logger;
        private readonly AuthenticationSettings _authenticationSettings;

        public RegisterCommandHandler(UserManager<User> userManager, ILogger<RegisterCommandHandler> logger, AuthenticationSettings authenticationSettings)
        {
            _userManager = userManager;
            _logger = logger;
            _logger.LogInformation($"[{DateTime.UtcNow}] Object {nameof(RegisterCommandHandler)} has been created.");
            _authenticationSettings = authenticationSettings;
        }

        public async Task<Result<string>> Handle(RegisterCommand command, CancellationToken cancellationToken)
        {
            var errors = new List<string>();
            var user = new User()
            {
                UserName = command.Username,
                Email = command.Email,    
            };

            

            var emailResult = await _userManager.FindByEmailAsync(command.Email);

            if (emailResult != null)
            {
                errors.Add("User with this email already exists!");
                _logger.LogError($"[{DateTime.UtcNow}] User with this email already exists!");
                return Result.BadRequest<string>(errors);
            }

            var result = await _userManager.CreateAsync(user, command.Password);
            if (!result.Succeeded)
            {
                foreach(var error in result.Errors)
                {
                    errors.Add(error.Description);
                }
                _logger.LogError($"[{DateTime.UtcNow}] {result.Errors.ToString()}");
                return Result.BadRequest<string>(errors);
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
