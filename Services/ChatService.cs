using ChatAPI.Data;
using ChatAPI.DTO;
using ChatAPI.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace ChatAPI.Services
{
    public class ChatService : IChatService
    {
        private readonly UserManager<ApplicationUser> userManager;
        private readonly AppDbContext context;
        private readonly IAIService aiService;
        public ChatService(AppDbContext _context, IAIService _aiService, UserManager<ApplicationUser> userManager)
        {
            context = _context;
            aiService = _aiService;
            this.userManager = userManager;
        }

        public async Task<string> SendMessageAsync(string userId, Guid conversationId, string message)
        {

            var conversation =
                await context.Conversations
                .Include(c => c.Messages)
                .FirstOrDefaultAsync(c => c.Id == conversationId && c.UserId == userId);
            if (conversation == null)
            {
                throw new Exception("Conversation not found");
            }

            //Save User Message
            var userMessage = new Message
            {
                Id = Guid.NewGuid(),
                ConversationId = conversationId,
                Role = "user",
                Content = message,
                CreatedAt = DateTime.UtcNow

            };

            context.Messages.Add(userMessage);
            await context.SaveChangesAsync();

            if (message.ToLower().Contains("compare") || message.Contains("قارن"))
            {
                var compareResult = await HandleComparisonWithAI(message);

                // save bot response
                context.Messages.Add(new Message
                {
                    Id = Guid.NewGuid(),
                    ConversationId = conversationId,
                    Role = "assistant",
                    Content = compareResult,
                    CreatedAt = DateTime.UtcNow
                });

                await context.SaveChangesAsync();

                return compareResult;
            }

            //Get Last 10 Messages
            var history =
                context.Messages
                .Where(m => m.ConversationId == conversationId)
                .OrderByDescending(m => m.CreatedAt)
                .Take(10)
                .OrderBy(m => m.CreatedAt)
                .Select(m => new AIMessageDto
                {
                    Role = m.Role,
                    Content = m.Content
                }).ToList();


            //Add the Last One to History
            history.Add(new AIMessageDto
            {
                Role = "user",
                Content = message
            });

            var aiReply = await aiService.GetResponseAsync(history);


            var botMessage = new Message
            {
                Id = Guid.NewGuid(),
                ConversationId = (Guid)conversationId,
                Role = "assistant",
                Content = aiReply,
                CreatedAt = DateTime.UtcNow
            };

            context.Messages.Add(botMessage);

            conversation.LastUpdatedAt = DateTime.UtcNow;

            await context.SaveChangesAsync();

            return (aiReply);
        }


        public async Task<List<MessageDto>> GetHistoryAsync(string userId, Guid conversationId)
        {
            return await context.Messages
                .Where(m => m.ConversationId == conversationId && m.Conversation.UserId == userId)
                .OrderBy(m => m.CreatedAt)
                .Select(m => new MessageDto { Role = m.Role, Content = m.Content })
                .ToListAsync();
        }

        public async Task<Guid> GetOrCreateConversation(string userId)
        {
            var conversation = await context.Conversations.FirstOrDefaultAsync(c => c.UserId == userId);
            if (conversation != null)
                return conversation.Id;

            var newConversation = new Conversation
            {
                Id = Guid.NewGuid(),
                UserId = userId,
                CreatedAt = DateTime.UtcNow,
                LastUpdatedAt = DateTime.UtcNow
            };
            context.Conversations.Add(newConversation);
            await context.SaveChangesAsync();
            return newConversation.Id;
        }

        private async Task<string> HandleComparisonWithAI(string message)
        {
            var names = await aiService.ExtractProductNames(message);

            if (names.Count < 2)
                return "number of products is lower than 2!";

            var products = await context.Products
                .Where(p => names.Any(n => p.ProductName.ToLower().Contains(n.ToLower())))
                .ToListAsync();

            if (products.Count < 2)
                return ("NotFound!");

            var result = "Comparison:";

            foreach (var p in products)
            {
                result += $"{p.ProductName} Price: {p.Price} Description: {p.Description}";
            }

            var cheapest = products.OrderBy(p => p.Price).First();

            result += $"Cheapest: {cheapest.ProductName}";

            return result;
        }
    }
}
