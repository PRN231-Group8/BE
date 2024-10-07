using System.ComponentModel.DataAnnotations;
using PRN231.ExploreNow.BusinessObject.Enums;

namespace PRN231.ExploreNow.BusinessObject.Models.Request;

public class LocationsRequest
{
    public string Name { get; set; }
    public string Description { get; set; }
    public string Address { get; set; }
    public WeatherStatus Status { get; set; }
    public double Temperature { get; set; }
    public List<PhotoRequest> Photos { get; set; } = new List<PhotoRequest>();
}