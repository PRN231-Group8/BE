using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PRN231.ExploreNow.BusinessObject.Models.Request
{
    public class UserRequestModel
    {
            public string? FirstName { get; set; }
            public string? LastName { get; set; }
            public DateTime? Dob { get; set; }
            public string? Gender { get; set; }
            public string? Address { get; set; }
        public DateTime CreatedDate { get; set; }
        public string? CreatedBy { get; set; }
        public string? LastUpdatedBy { get; set; }
        public DateTime? LastUpdatedDate { get; set; }
        public string? AvatarPath { get; set; }
        public string? VerifyToken { get; set; }
        public DateTime? VerifyTokenExpires { get; set; }
        public bool isActived { get; set; } = false;
    }
}
