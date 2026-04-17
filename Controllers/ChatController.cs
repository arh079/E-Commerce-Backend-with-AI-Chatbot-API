using ChatAPI.Data;
using ChatAPI.DTO;
using ChatAPI.Models;
using ChatAPI.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;
using System.Text.RegularExpressions;

namespace ChatAPI.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class ChatController : ControllerBase
    {
        private readonly IChatService chatService; 
        private readonly AppDbContext context;
        private readonly IAIService aIService;
        public ChatController(IAIService _aIService, IChatService _chatService , AppDbContext _context) 
        { 
            chatService = _chatService;
            context = _context;
            aIService = _aIService;
        }
        [Authorize]
        [HttpPost("Send")]
       
        public async Task<IActionResult> Send(SendMessageDto dto)
        {

            string userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (userId == null)
            {
                return Unauthorized();
            }
            if (dto == null)
            {
                return BadRequest("Message Data Is Required");
            }

            var reply = 
                await chatService.SendMessageAsync(userId, dto.ConversationId, dto.Message);

            var Cleanreply =  aIService.CleanAiResponse(reply);
            //var Cleanreply = Regex.Replace(Clean, @"(^\w\s\p{P})", "");

            return Ok(new{ Cleanreply });
        }
        [Authorize]
        [HttpGet("history/{conversationId}")] 
        public async Task<IActionResult> GetHistory(Guid conversationId)
        {
            string userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            var history = 
                await chatService.GetHistoryAsync(userId, conversationId);
            return Ok(history); 
        }
        [Authorize]
        [HttpPost("CreateConversation")]

        public async Task <IActionResult> CreateConversation()
        {
            string userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;

            var convId = await chatService.GetOrCreateConversation(userId);

            return Ok(new {convId});
        }

    }
}
