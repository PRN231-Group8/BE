using Microsoft.AspNetCore.Http;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PRN231.ExploreNow.BusinessObject.Models.Request
{
	public class UpdatePhotoRequest
	{
		public Guid PhotoId { get; set; }
		public Guid PostId { get; set; }
		public IFormFile File { get; set; }
	}
}
