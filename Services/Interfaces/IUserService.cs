using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PRN231.ExploreNow.Services.Interfaces
{
    public interface IUserService
    {
        Task<bool> VerifyEmailTokenAsync(string email, string token);

    }
}
