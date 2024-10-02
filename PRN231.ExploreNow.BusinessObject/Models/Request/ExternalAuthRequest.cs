using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PRN231.ExploreNow.BusinessObject.Models.Request
{
	public class ExternalAuthRequest
	{
		public string? Provider { get; set; }
		public string? IdToken { get; set; }
	}
}
