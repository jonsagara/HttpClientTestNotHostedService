using System.Net.Http;
using System.Threading.Tasks;

namespace HttpClientTestNotHostedService
{
    public class TestService
    {
        private readonly HttpClient _httpClient;

        public TestService(IHttpClientFactory httpClientFactory)
        {
            _httpClient = httpClientFactory.CreateClient();
        }

        public async Task<string> GetMicrosoftAsync()
        {
            using (var requestMsg = new HttpRequestMessage(HttpMethod.Get, "https://www.microsoft.com"))
            {
                using (var responseMsg = await _httpClient.SendAsync(requestMsg))
                {
                    return await responseMsg.Content.ReadAsStringAsync();
                }
            }
        }
    }
}
