using PRN231.ExploreNow.BusinessObject.Enums;

namespace PRN231.ExploreNow.BusinessObject.Models.Request
{
	public class PostsRequest
	{
        public bool IsRecommended { get; set; }
        public string Content { get; set; }
		public PostsStatus? Status { get; set; }
		public bool? RemoveAllComments { get; set; }
		public List<string>? CommentsToRemove { get; set; }
		public bool? RemoveAllPhotos { get; set; }
		public List<string>? PhotosToRemove { get; set; }
	}
}
