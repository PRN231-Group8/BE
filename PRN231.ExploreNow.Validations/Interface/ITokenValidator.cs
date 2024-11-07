using System.Security.Claims;

namespace PRN231.ExploreNow.Validations.Interface
{
	public interface ITokenValidator
	{
		ClaimsPrincipal ValidateToken(string token);
	}
}
