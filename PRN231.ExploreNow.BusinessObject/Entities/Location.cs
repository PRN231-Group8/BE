using PRN231.ExploreNow.BusinessObject.Enums;
using PRN231.ExploreNow.BusinessObject.OtherObjects;

namespace PRN231.ExploreNow.BusinessObject.Entities;

public class Location : BaseEntity
{
	public string Name { get; set; }
	public string Description { get; set; }
	public AddressInfo Address { get; set; }
	public WeatherStatus Status { get; set; }
	public double Temperature { get; set; }
	public ICollection<Photo> Photos { get; set; } = new List<Photo>();
	public ICollection<TourTimestamp> TourTimestamps { get; set; }
}