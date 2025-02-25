using System.Net.Http.Headers;
using System.Text;
using LudereAI.Application.Interfaces.Services;
using LudereAI.Domain.Models.Configs;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Microsoft.Identity.Client;
using Newtonsoft.Json;
using Task = System.Threading.Tasks.Task;

namespace LudereAI.Infrastructure.Services;

public class EmailService : IEmailService
{
    private readonly ILogger<IEmailService> _logger;
    private readonly EmailConfig _config;

    public EmailService(ILogger<IEmailService> logger, IOptions<EmailConfig> config)
    {
        _logger = logger;
        _config = config.Value;
    }

        /// <summary>
        /// Sends an email using Microsoft Graph API
        /// </summary>
        /// <param name="to">Recipient email address</param>
        /// <param name="subject">Email subject</param>
        /// <param name="body">HTML email body content</param>
        /// <returns>A task representing the asynchronous operation</returns>
        public async Task SendEmail(string to, string subject, string body)
        {
            try
            {
                // Get the access token
                var accessToken = await GetAccessTokenAsync();

                using var httpClient = new HttpClient();
                // Set the authorization header
                httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", accessToken);

                // Prepare the email message payload
                var emailPayload = new
                {
                    Message = new
                    {
                        Subject = subject,
                        Body = new
                        {
                            ContentType = "HTML",
                            Content = body
                        },
                        ToRecipients = new[]
                        {
                            new { EmailAddress = new { Address = to } }
                        }
                    },
                    SaveToSentItems = true
                };

                var jsonPayload = JsonConvert.SerializeObject(emailPayload);
                HttpContent content = new StringContent(jsonPayload, Encoding.UTF8, "application/json");

                // The endpoint to send mail from the configured sender address
                var requestUrl = $"https://graph.microsoft.com/v1.0/users/{_config.From}/sendMail";

                var response = await httpClient.PostAsync(requestUrl, content);
                    
                if (!response.IsSuccessStatusCode)
                {
                    var errorContent = await response.Content.ReadAsStringAsync();
                    _logger.LogError("Graph API returned error: {StatusCode} - {Content}", 
                        response.StatusCode, errorContent);
                    throw new HttpRequestException($"Graph API error: {response.StatusCode}");
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to send email to {Recipient} with subject '{Subject}'", to, subject);
                throw;
            }
        }
    
    /// <summary>
    /// Gets an access token for Microsoft Graph API
    /// </summary>
    /// <returns>The access token string</returns>
    private async Task<string> GetAccessTokenAsync()
    {
        try
        {
            // Create a confidential client application
            var app = ConfidentialClientApplicationBuilder.Create(_config.ClientId)
                .WithClientSecret(_config.ClientSecret)
                .WithAuthority(new Uri($"https://login.microsoftonline.com/{_config.TenantId}"))
                .Build();

            // Use the .default scope to request all configured permissions
            var scopes = new string[] { "https://graph.microsoft.com/.default" };

            // Acquire the token
            var result = await app.AcquireTokenForClient(scopes).ExecuteAsync();
            return result.AccessToken;
        }
        catch (MsalException ex)
        {
            _logger.LogError(ex, "Error acquiring Microsoft Graph API token");
            throw;
        }
    }
}