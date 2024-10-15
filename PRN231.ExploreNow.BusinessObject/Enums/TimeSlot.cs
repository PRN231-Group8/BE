using PRN231.ExploreNow.BusinessObject.OtherObjects;
using System.Text.Json.Serialization;

namespace PRN231.ExploreNow.BusinessObject.Enums
{
	public class TimeSlot
	{
		[JsonConverter(typeof(TimeSpanConverter))]
		public TimeSpan StartTime { get; set; }

		[JsonConverter(typeof(TimeSpanConverter))]
		public TimeSpan EndTime { get; set; }
	}
}
