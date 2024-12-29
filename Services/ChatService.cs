using GraphlitClient;
using RAGaaSBlazor.Data;
using StrawberryShake;

namespace RAGaaSBlazor.Services
{
    public class ChatService
    {
        private readonly IConfiguration _configuration;
        //private readonly HttpClient _httpClient;
        private string SystemMessage = "You are an AI assistant that helps people find information about food.  For anything other than food, respond with 'I can only answer questions about food.'";
        private string url = "https://www.investors.com/research/swing-trading/powerful-setup-constellation-energy-ceg-stock/";
        public ChatService(IConfiguration configuration)
        {
            _configuration = configuration;
            
        }

        public async Task<Message> GetResponse(string prompt)
        {
            var response = string.Empty;

            HttpClient httpClient = new HttpClient();

            var client = new Graphlit(httpClient,
                                _configuration.GetSection("ClientSettings")["GRAPHLIT_ORGANIZATION_ID"],
                                _configuration.GetSection("ClientSettings")["GRAPHLIT_ENVIRONMENT_ID"],
                                _configuration.GetSection("ClientSettings")["GRAPHLIT_JWT_SECRET"],
                                _configuration.GetSection("ClientSettings")["GRAPHLIT_OWNER_ID"]);

            var id = await IngestUriAsync(client.Client, null, new Uri(url));

            response = await PromptConversationAsync(client.Client, prompt);

            return new Message(response, false);

        }

        private static async Task<string?> PromptConversationAsync(IGraphlitClient client, string prompt)
        {
            var result = await client.PromptConversation.ExecuteAsync(prompt, null, null, null, null, null, null);

            var response = result.Data?.PromptConversation;

            if (response == null)
            {
                Console.WriteLine($"Error:{result?.Errors[0]?.Message}");
                var res = await client.DeleteAllConversations.ExecuteAsync(null, true, null);
                if (res.IsSuccessResult())
                    Console.WriteLine("old conversations deleted, you are good to go next time around");
            }

            return response?.Message?.Message;
        }

        private static async Task<string?> IngestUriAsync(IGraphlitClient client, string name, Uri uri)
        {
            var result = await client.IngestUri.ExecuteAsync(name: name, uri: uri, null, true, null, null, null,null);

            result.EnsureNoErrors();

            var response = result.Data?.IngestUri;

            return response?.Id;
        }
    }
}
