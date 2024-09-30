using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Text;
using System.Threading.Tasks;

namespace PRN231.ExploreNow.Validations.Interface
{
    public interface ITokenValidator
    {
        ClaimsPrincipal ValidateToken(string token);
    }
}
