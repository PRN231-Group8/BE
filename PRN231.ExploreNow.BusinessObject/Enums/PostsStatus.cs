using System.Text.Json.Serialization;

namespace PRN231.ExploreNow.BusinessObject.Enums
{
	[JsonConverter(typeof(JsonStringEnumConverter))]
	public enum PostsStatus
	{
		Pending,
		Approved,
		Rejected
	}
}
