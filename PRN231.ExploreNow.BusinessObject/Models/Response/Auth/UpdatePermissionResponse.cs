using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PRN231.ExploreNow.BusinessObject.Models.Response.Auth
{
    public class UpdatePermissionResponse
    {
        [Required(ErrorMessage = "UserName is required")]
        public string UserName { get; set; }
    }
}
