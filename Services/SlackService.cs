using System.Text;
using System;
using System.Net.Http;
using System.Threading.Tasks;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json;

namespace GiddhTemplate.Services
{
    public class SlackService : ISlackService
    {
        private readonly string _slackWebhookUrl;

        public SlackService(HttpClient httpClient)
        {
            _slackWebhookUrl = "https://flow.sokt.io/func/scriCC1OrB0A";
        }

        public async Task SendErrorAlertAsync(string url, string environment, string error, string stackTrace)
        {
            try
            {
                // Create key-value pairs instead of JSON string
                var keyValuePairs = new Dictionary<string, string>
                {
                    { "url", url },
                    { "env", environment },
                    { "error", error },
                    { "errorStackTrace", stackTrace }
                };

                Console.WriteLine($"Sending error data: {string.Join(", ", keyValuePairs.Select(kv => $"{kv.Key}={kv.Value}"))}");
                using var _httpClient = new HttpClient { Timeout = TimeSpan.FromSeconds(10) };

                var json = JsonSerializer.Serialize(keyValuePairs);
                var content = new StringContent(json, Encoding.UTF8, "application/json");

                var response = await _httpClient.PostAsync(_slackWebhookUrl, content);

                if (!response.IsSuccessStatusCode)
                {
                    var errorContent = await response.Content.ReadAsStringAsync();
                    throw new Exception($"Failed to send error alert: {response.StatusCode} - {errorContent}");
                }
            }
            catch (Exception ex)
            {
                // Log the exception but don't throw to avoid breaking the main flow
                Console.WriteLine($"Error while sending alter on slack", ex.StackTrace);
            }
        }
    }
}