using ChatAPI.DTO;
using ChatAPI.Models;
using Microsoft.AspNetCore.Identity;
using System.Net.Http;
using System.Text;
using System.Text.Json;
using static System.Net.Mime.MediaTypeNames;
using Microsoft.AspNetCore.Hosting;

namespace ChatAPI.Services
{
    public class AIService : IAIService
    {

        private readonly HttpClient httpClient;
        private readonly IConfiguration configuration;
        private readonly IWebHostEnvironment env;
        public AIService(HttpClient _httpClient, IConfiguration _configuration, IWebHostEnvironment _env)
        {
            httpClient = _httpClient;
            configuration = _configuration;
            env = _env;
        }
        private async Task<string> SendRequestAsync(object body)
        {
            var apiKey = configuration["OpenRouter:ApiKey"];

            httpClient.DefaultRequestHeaders.Clear();
            httpClient.DefaultRequestHeaders.Add("Authorization", $"Bearer {apiKey}");

            if (string.IsNullOrEmpty(apiKey))
            {
                throw new Exception("API KEY IS NULL");
            }

            if (Environment.GetEnvironmentVariable(env.EnvironmentName) == "Development")
            {
                httpClient.DefaultRequestHeaders.Add("HTTP-Referer", "http://localhost:4200");
            }
            else
            {
                httpClient.DefaultRequestHeaders.Add("HTTP-Referer", "http://chatbotapi.runasp.net");
            }

            httpClient.DefaultRequestHeaders.Add("X-Title", "ChatAPI");

            var json = JsonSerializer.Serialize(body);

            var response = await httpClient.PostAsync(
                "https://openrouter.ai/api/v1/chat/completions",
                new StringContent(json, Encoding.UTF8, "application/json"));

            if (!response.IsSuccessStatusCode)
            {
                var errorBody = await response.Content.ReadAsStringAsync();
                return $"Error: {response.StatusCode} - {errorBody}";
            }

            return await response.Content.ReadAsStringAsync();
        }

        public async Task<string> GetAIContentAsync(object body)
        {
            var responseText = await SendRequestAsync(body);

            using var doc = JsonDocument.Parse(responseText);

            if (!doc.RootElement.TryGetProperty("choices", out var choices))
            {
                throw new ApplicationException($"Unexpected response: {responseText}");
            }

            var content = choices[0]
                .GetProperty("message")
                .GetProperty("content")
                .GetString();

            return content;
        }
        public string CleanAiResponse(string message)
        {
                message = message.Replace("**", "")
                .Replace("*", "")
                .Replace("\\n", "")
                .Replace("#", "")
                .Replace("\\", "")
                .Replace("\\n", "");
            return message;
        }

        public async Task<string> GetResponseAsync(List<AIMessageDto> messages)
        {
            var systemMessage = new AIMessageDto
            {
                Role = "system",
                Content =
                $@" You are an assistant specialized only in e-commerce and selling electronic devices Like Phones and Laptops.
                      Your job is to:
             - Your Name is 'Mohsen' Or مُحسِن
             - Your Developer is 'Ahmed Reda'
             - Do NOT use Markdown or any formatting symbols such as \n,\\ **, *, or #. 
             - Do NOT include Symbols in the response Please. 
             - Respond using plain text only.
             - Do not answer any question outside the scope of e-commerce and electronic products
             - Never answer questions about politics, religion, sports, space, or general knowledge. 
             - Keep answers short and persuasive.
             - Speak Arabic if the user writes Arabic.
             - Speak in friendly professional tone.
             - Stay focused only on electronics and store-related topics.
             -If a question is unrelated, reply with: 'I am specialized only in e-commerce and electronic products.'"
            };

            messages.Insert(0, systemMessage);

            var formattedMessages = messages.Select(m => new
            {
                role = m.Role,
                content = m.Content

            }).ToList();

            var body =
                new
                {
                    model = "openrouter/free",
                    messages = formattedMessages
                };

            return await GetAIContentAsync(body);

        }
     
    public async Task<List<string>> ExtractProductNames(string message)
    {
            var prompt = $@"

             - Your Name is 'Mohsen' Or مُحسِن
             - Your Developer is 'Ahmed Reda'
             - Do NOT use Markdown or any formatting symbols such as \n,\\ **, *, or #. 
             - Do NOT include Symbols in the response Please. 
             - Respond using plain text only.
             - Extract product names from this message.
                  Return ONLY JSON like this:
                  {{ ""products"": [""name1"", ""name2""] }}

                Message:{message}
            ";

            var body = new
            {
                model = "openrouter/free",
                messages = new[]
                { 
                    new {
                    role = "user",
                    content = prompt }
                }
            };

            var content = await GetAIContentAsync(body);

            try
            {
                var result = JsonSerializer.Deserialize<ExtractResponseDto>(content);
                return result?.products ?? new List<string>();
            }
            catch
            {
                return new List<string>();
            }

        }
    }
}
