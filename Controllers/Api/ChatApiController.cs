using Asp.Versioning;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Website_QLPT.Data;

namespace Website_QLPT.Controllers.Api
{
    [ApiVersion("1.0")]
    [Route("api/v{version:apiVersion}/chat")]
    [ApiController]
    [Authorize(AuthenticationSchemes = "Bearer,Identity.Application")]
    public class ChatApiController : ControllerBase
    {
        private readonly ApplicationDbContext _context;

        public ChatApiController(ApplicationDbContext context)
        {
            _context = context;
        }

        [HttpGet("history/{receiverEmail}")]
        public async Task<IActionResult> GetHistory(string receiverEmail)
        {
            var userEmail = User.Identity?.Name;
            if (string.IsNullOrEmpty(userEmail)) return Unauthorized();

            var messagesRaw = await _context.ChatMessages
                .Where(m => 
                    (m.SenderEmail == userEmail && m.ReceiverEmail == receiverEmail) ||
                    (m.SenderEmail == receiverEmail && m.ReceiverEmail == userEmail))
                .OrderBy(m => m.SentAt)
                .Take(50)
                .ToListAsync();

            var messages = messagesRaw.Select(m => new
                {
                    m.Id,
                    m.SenderEmail,
                    m.ReceiverEmail,
                    m.Content,
                    SentAt = m.SentAt.ToString("dd/MM/yyyy HH:mm")
                })
                .ToList();

            return Ok(messages);
        }
    }
}
