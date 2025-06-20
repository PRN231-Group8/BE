﻿namespace PRN231.ExploreNow.BusinessObject.Models.Response;

public class LocationResponse
{
	public Guid Id { get; set; }
	public string Name { get; set; }
	public string Description { get; set; }
	public string Address { get; set; }
	public double? Longitude { get; set; }
	public double? Latitude { get; set; }
	public string Status { get; set; }
	public double Temperature { get; set; }
	public List<PhotoResponseForLocation> Photos { get; set; }
}