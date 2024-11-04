using PRN231.ExploreNow.BusinessObject.Enums;

namespace PRN231.ExploreNow.BusinessObject.Models.Request
{
	public class TourRequestModel
	{
		public DateTime? StartDate { get; set; }
		public DateTime? EndDate { get; set; }
		public TourStatus Status { get; set; }
		public string Title { get; set; }
		public string Description { get; set; }
		public List<Guid>? TourMoods { get; set; }
		public List<Guid>? LocationInTours { get; set; }
	}
}
