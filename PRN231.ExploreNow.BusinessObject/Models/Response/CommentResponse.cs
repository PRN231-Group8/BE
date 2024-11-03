using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PRN231.ExploreNow.BusinessObject.Models.Response
{
	public class CommentResponse
	{
		public Guid Id { get; set; }
		public string Content { get; set; }
		public Guid PostId { get; set; }
		public DateTime CreatedDate { get; set; }
		public string UserId { get; set; }
	}
}
