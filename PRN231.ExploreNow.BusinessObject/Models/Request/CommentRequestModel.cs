using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PRN231.ExploreNow.BusinessObject.Models.Request
{
	public class CommentRequest
	{
		public string Content { get; set; }
		public Guid PostId { get; set; }
	}
}
