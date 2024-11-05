using PRN231.ExploreNow.BusinessObject.Enums;

namespace PRN231.ExploreNow.BusinessObject.Models.Request;
public class LocationCreateRequest
{
	public string Name { get; set; }
	public string Description { get; set; }
	public string Address { get; set; }
	public WeatherStatus Status { get; set; }
	public double Temperature { get; set; }
	public double? Longitude { get; set; }
	public double? Latitude { get; set; }
}