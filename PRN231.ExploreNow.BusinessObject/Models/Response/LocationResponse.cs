namespace PRN231.ExploreNow.BusinessObject.Models.Response;

public class LocationResponse
{
	public Guid Id { get; set; }
	public string Name { get; set; }
	public string Description { get; set; }
	public AddressInfoResponse Address { get; set; }
	public string Status { get; set; }
	public double Temperature { get; set; }
	public List<PhotoResponse> Photos { get; set; }
}