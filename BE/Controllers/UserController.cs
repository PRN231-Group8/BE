using FluentValidation.Results;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using PRN231.ExploreNow.BusinessObject.Models.Request;
using PRN231.ExploreNow.Services.Interfaces;
using PRN231.ExploreNow.Validations.Profile;
using PRN231.ExploreNow.Validations.User;
using PRN231.ExploreNow.BusinessObject.Models.Response;
using Microsoft.AspNetCore.Authorization;
using PRN231.ExploreNow.BusinessObject.Enums;

namespace PRN231.ExploreNow.API.Controllers
{
    [Route("api/users")]
    [ApiController]
    public class UserController : ControllerBase
    {
        private readonly IUserService _userService;
        private readonly ProfileValidation _validation;

        public UserController(IUserService userService, ProfileValidation validation)
        {
            _userService = userService;
            _validation = validation;
        }

        [Authorize]
        [HttpPut("/{id}/profile")]
        public async Task<IActionResult> UpdateUserProfile(UserProfileRequestModel model, string id)
        {
            try
            {
                ValidationResult validationResult = _validation.Validate(model);
                if (!validationResult.IsValid)
                {
                    var errors = validationResult.Errors.Select(e => (object)new
                    {
                        e.PropertyName,
                        e.ErrorMessage
                    }).ToList();
                    return BadRequest(new BaseResponse<object>
                    {
                        IsSucceed = false,
                        Results = errors,
                        Message = "An error occur when input profile"
                    });
                }
                var profile = await _userService.UpdateUserProfile(id, model);
                return Ok(new BaseResponse<object>
                {
                    IsSucceed = true,
                    Result = profile,
                    Message = "Profile updated successfully"
                });

            }
            catch (Exception ex)
            {
                return BadRequest(new BaseResponse<object> {IsSucceed = false ,Result = ex.Message, Message = "There is something wrong" });
            }
        }

        [HttpPost("/image")]
        [Authorize]
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
