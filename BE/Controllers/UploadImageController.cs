using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using PRN231.ExploreNow.Services.Interfaces;
using PRN231.ExploreNow.BusinessObject.Models.Response;
using CloudinaryDotNet;

namespace PRN231.ExploreNow.API.Controllers
{
    [Route("api/UploadImage")]
    [ApiController]
    public class UploadImageController : ControllerBase
    {
        private IUserService _userService;

        public UploadImageController(IUserService userService)
        {
            _userService = userService;
        }

        [HttpPost]
        public async Task<IActionResult> UploadImage(IFormFile file)
        {
            try
            {
                var imageUrl = new { Url = _userService.SaveImage(file).Result };
                return Ok(new BaseResponse<object> { IsSucceed = true, Message = "Update image successfully", Result = imageUrl });
            }
            catch (Exception ex) 
            {
                return BadRequest(ex.Message);
            }
        }
    }
}
