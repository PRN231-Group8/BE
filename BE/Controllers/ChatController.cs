using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using PRN231.ExploreNow.BusinessObject.Models.Response;
using PRN231.ExploreNow.Services.Interfaces;

namespace PRN231.ExploreNow.API.Controllers
{
    [ApiController]
    [Route("api/chat-box")]
    public class ChatController : ControllerBase
    {
        private readonly IChatRoomService _chatRoomService;
        private readonly IChatMessageService _chatMessageService;

        public ChatController(IChatRoomService chatRoomService, IChatMessageService chatMessageService)
        {
            _chatRoomService = chatRoomService;
            _chatMessageService = chatMessageService;
        }

        [HttpGet("rooms/{id}/messages")]
        [Authorize]
        [ProducesResponseType(typeof(BaseResponse<IEnumerable<ChatMessageResponse>>), 200)]
        public async Task<IActionResult> GetChatMessages(Guid id)
        {
            try
            {
                var messages = await _chatMessageService.GetChatMessagesAsync(id);
                return Ok(new BaseResponse<IEnumerable<ChatMessageResponse>>
                {
                    IsSucceed = true,
                    Result = messages,
                    Message = "Chat messages retrieved successfully."
                });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new BaseResponse<IEnumerable<ChatMessageResponse>>
                {
                    IsSucceed = false,
                    Message = $"An error occurred while retrieving chat messages: {ex.Message}"
                });
            }
        }

        [HttpGet("messages/unread")]
        [Authorize]
        [ProducesResponseType(typeof(BaseResponse<int>), 200)]
        public async Task<IActionResult> GetUnreadMessageCount()
        {
            try
            {
                var count = await _chatMessageService.GetUnreadMessageCountAsync();
                return Ok(new BaseResponse<int>
                {
                    IsSucceed = true,
                    Result = count,
                    Message = "Unread message count retrieved successfully."
                });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new BaseResponse<int>
                {
                    IsSucceed = false,
                    Message = $"An error occurred while retrieving unread message count: {ex.Message}"
                });
            }
        }

        [HttpGet("rooms/{id}")]
        [Authorize]
        [ProducesResponseType(typeof(BaseResponse<ChatRoomResponse>), 200)]
        public async Task<IActionResult> GetChatRoom(Guid id)
        {
            try
            {
                var chatRoom = await _chatRoomService.GetChatRoomDetailsAsync(id);
                return Ok(new BaseResponse<ChatRoomResponse>
                {
                    IsSucceed = true,
                    Result = chatRoom,
                    Message = "Chat room details retrieved successfully."
                });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new BaseResponse<ChatRoomResponse>
                {
                    IsSucceed = false,
                    Message = $"An error occurred while retrieving chat room details: {ex.Message}"
                });
            }
        }

        [HttpGet("rooms/pending")]
        [Authorize(Roles = "MODERATOR")]
        [ProducesResponseType(typeof(BaseResponse<ChatRoomResponse>), 200)]
        public async Task<IActionResult> GetPendingChatRoom()
        {
            try
            {
                var chatPendingRoom = await _chatRoomService.GetPendingChatsAsync();
                return Ok(new BaseResponse<IEnumerable<ChatRoomResponse>>
                {
                    IsSucceed = true,
                    Result = chatPendingRoom,
                    Message = "Chat pending room details retrieved successfully."
                });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new BaseResponse<ChatRoomResponse>
                {
                    IsSucceed = false,
                    Message = $"An error occurred while retrieving chat room details: {ex.Message}"
                });
            }
        }

        [HttpGet("rooms/active")]
        [ProducesResponseType(typeof(BaseResponse<IEnumerable<ChatRoomResponse>>), 200)]
        [Authorize(Roles = "MODERATOR")]
        public async Task<IActionResult> GetActiveChats()
        {
            try
            {
                var activeChats = await _chatRoomService.GetActiveChatsAsync();
                return Ok(new BaseResponse<IEnumerable<ChatRoomResponse>>
                {
                    IsSucceed = true,
                    Result = activeChats,
                    Message = "Active chat rooms retrieved successfully."
                });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new BaseResponse<IEnumerable<ChatRoomResponse>>
                {
                    IsSucceed = false,
                    Message = $"An error occurred while retrieving active chat rooms: {ex.Message}"
                });
            }
        }

        [HttpGet("rooms")]
        [Authorize(Roles = "CUSTOMER")]
        [ProducesResponseType(typeof(BaseResponse<ChatRoomResponse>), 200)]
        public async Task<IActionResult> GetCurrentRoom()
        {
            try
            {
                var currentRoom = await _chatRoomService.GetCurrentRoomAsync();

                return Ok(new BaseResponse<ChatRoomResponse>
                {
                    IsSucceed = true,
                    Result = currentRoom,
                    Message = "Current chat room retrieved successfully"
                });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new BaseResponse<ChatRoomResponse>
                {
                    IsSucceed = false,
                    Message = $"An error occurred while retrieving current chat room: {ex.Message}"
                });
            }
        }
    }
}
