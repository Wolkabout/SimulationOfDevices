using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;
using SimulationOfDevices.Services.Common.Services;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;

namespace SimulationOfDevices.Services.Common.Authentication
{
    public class JwtTokenGenerator : IJwtTokenGenerator
    {
        private readonly IDateTimeProvider _dateTimeProvider;
        private readonly JwtSettings _jwtSettings;

        public JwtTokenGenerator(IDateTimeProvider dateTimeProvider, IOptions<JwtSettings> jwtOptions)
        {
            _dateTimeProvider = dateTimeProvider;
            _jwtSettings = jwtOptions.Value;
        }


        public string GenerateToken(string deviceId, string firstName, string lastName)
        {

            var signingCredentials = new SigningCredentials(
                new SymmetricSecurityKey(
                    Encoding.UTF8.GetBytes(_jwtSettings.Secret)),
                    SecurityAlgorithms.HmacSha256);

            var claims = new[]
            {
                new Claim(JwtRegisteredClaimNames.Sub, deviceId), //The "sub" (subject) claim identifies the principal that is the subject of the JWT.
                new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString()), //The "jti" (JWT ID) claim provides a unique identifier for the JWT.             
            };

            var securityToken = new JwtSecurityToken(
                issuer: _jwtSettings.Issuer,
                audience: _jwtSettings.Audience,
                expires: _dateTimeProvider.DateTimeUtcNow.AddDays(_jwtSettings.ExpiryDays),
                claims: claims,
                signingCredentials: signingCredentials
                );

            return new JwtSecurityTokenHandler().WriteToken(securityToken);
        }
    }
}
