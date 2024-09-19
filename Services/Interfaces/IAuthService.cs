using Domain.DTO.Auth;

namespace Services.Interfaces
{
    public interface IAuthService
    {
        Task<AuthServiceResponseDto> SeedRolesAsync();
    }
}
