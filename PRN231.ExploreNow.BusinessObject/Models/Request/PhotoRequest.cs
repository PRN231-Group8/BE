using System.ComponentModel.DataAnnotations;

namespace PRN231.ExploreNow.BusinessObject.Models.Request;

public class PhotoRequest
{
    public string Url { get; set; }

    public string Alt { get; set; }
}