using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;
using PRN231.ExploreNow.Validations.Interface;
using System;
using System.Collections.Generic;
using System.IdentityModel.Tokens.Jwt;
using System.Linq;
using System.Security.Claims;
using System.Text;
using System.Threading.Tasks;

namespace PRN231.ExploreNow.Validations
{
    public class TokenValidator : ITokenValidator
    {
        private readonly TokenValidationParameters _tokenValidationParameters;
        public TokenValidator(IOptions<JwtBearerOptions> jwtOptions)
        {
            _tokenValidationParameters = jwtOptions.Value.TokenValidationParameters;
        }

        public ClaimsPrincipal ValidateToken(string token)
        {
            var tokenHandler = new JwtSecurityTokenHandler();
            try
            {
                var principal = tokenHandler.ValidateToken(token, _tokenValidationParameters, out var validatedToken);
                return principal;
            }
            catch
            {
                return null;
            }
        }
    }
}
