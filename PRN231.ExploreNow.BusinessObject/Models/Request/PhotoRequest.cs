using System.ComponentModel.DataAnnotations;

namespace PRN231.ExploreNow.BusinessObject.Models.Request;

public class PhotoRequest
{
    [Required(ErrorMessage = "Photo URL is required")]
    public string Url { get; set; }

    public string Alt { get; set; }
}