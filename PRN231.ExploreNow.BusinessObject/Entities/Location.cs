using PRN231.ExploreNow.BusinessObject.Enums;

namespace PRN231.ExploreNow.BusinessObject.Entities;

public class Location : BaseEntity
{
	public string Name { get; set; }
	public string Description { get; set; }
	public string Address { get; set; }
	public WeatherStatus Status { get; set; }
	public double Temperature { get; set; }
	public ICollection<Photo> Photos { get; set; }
	public ICollection<TourTimestamp> TourTimestamps { get; set; }
}