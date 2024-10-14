using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using PRN231.ExploreNow.Services.Interfaces;
using PRN231.ExploreNow.BusinessObject.Models.Response;
using CloudinaryDotNet;
using Microsoft.AspNetCore.Authorization;
using PRN231.ExploreNow.BusinessObject.Enums;

namespace PRN231.ExploreNow.API.Controllers
{
    [Route("api/user/avatar")]
    [ApiController]
    public class UploadImageController : ControllerBase
    {
        private IUserService _userService;

        public UploadImageController(IUserService userService)
        {
            _userService = userService;
        }

        [HttpPost]
        [Authorize(Roles = StaticUserRoles.CUSTOMER)]
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
