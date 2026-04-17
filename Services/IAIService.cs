using ChatAPI.DTO;

namespace ChatAPI.Services
{
    public interface IAIService
    {
        Task<string> GetResponseAsync(List<AIMessageDto> messages);
        string CleanAiResponse(string message);
        Task<List<string>> ExtractProductNames(string message);
    }
}
