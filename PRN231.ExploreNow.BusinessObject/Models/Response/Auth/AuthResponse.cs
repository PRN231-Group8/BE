using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PRN231.ExploreNow.BusinessObject.Models.Response.Auth
{
    public class AuthResponse
    {
        public bool IsSucceed { get; set; }
        public string? Token { get; set; }
        public string? Role { get; set; }
        public string? UserId { get; set; }
        public string? Email { get; set; }
    }
}
