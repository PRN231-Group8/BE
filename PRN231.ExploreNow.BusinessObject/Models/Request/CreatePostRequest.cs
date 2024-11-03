using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Http;
using PRN231.ExploreNow.BusinessObject.Enums;

namespace PRN231.ExploreNow.BusinessObject.Models.Request
{
	public class CreatePostRequest
	{
		public string Content { get; set; }
		public List<IFormFile> Photos { get; set; } = new List<IFormFile>();

	}
}
