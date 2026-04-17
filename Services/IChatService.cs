using ChatAPI.DTO;
using ChatAPI.Models;

namespace ChatAPI.Services
{
    public interface IChatService
    {
        Task<string> SendMessageAsync(string userId, Guid conversationId, string message);
        Task<List<MessageDto>> GetHistoryAsync(string userId, Guid conversationId);
        Task<Guid> GetOrCreateConversation(string userId);
    }
}
