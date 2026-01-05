using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;
using TaskManagementApp.Models;

namespace TaskManagementApp.Services
{
    public class GoogleProjectSummaryService : IProjectSummaryService
    {
        private readonly HttpClient _httpClient;
        private readonly string _apiKey;
        private readonly ILogger<ProjectSummaryService> _logger;

        // URL-ul de bază pentru Google Gemini API
        private const string BaseUrl = "https://generativelanguage.googleapis.com/v1beta/models/";
        private const string ModelName = "gemini-2.5-flash-lite";

        public GoogleProjectSummaryService(
            IConfiguration configuration,
            ILogger<ProjectSummaryService> logger)
        {
            _httpClient = new HttpClient();
            _apiKey = configuration["GoogleAI:ApiKey"]
                ?? throw new ArgumentNullException("GoogleAI:ApiKey is not configured in appsettings.json");

            _logger = logger;

            _httpClient.DefaultRequestHeaders.Accept
                .Add(new MediaTypeWithQualityHeaderValue("application/json"));
        }

        public async Task<string> GenerateSummaryAsync(Project project)
        {
            // CAZ SPECIAL: fara activitate
            if (project.Tasks == null || !project.Tasks.Any())
            {
                return "There are no recent updates for the project";
            }

            try
            {
                var tasksText = string.Join("\n",
                    project.Tasks.Select(t =>
                        $"- {t.Title} | Status: {t.Status} | Deadline: {t.EndDate:d}"
                    )
                );

                // Construim Prompt-ul combinand instructiunile de sistem cu datele utilizatorului
                var prompt = $@"You are a project management assistant. 
Generate a concise project summary in English including:
- Overall progress
- Important deadlines
- Current task statuses

Project title: {project.Title}
Tasks:
{tasksText}

If there is no recent activity, respond with: 'No recent activity on this project.'";

                // Construim obiectul de request conform structurii Google AI
                var requestBody = new GoogleAiRequest
                {
                    Contents = new List<GoogleAiContent>
                    {
                        new GoogleAiContent
                        {
                            Parts = new List<GoogleAiPart>
                            {
                                new GoogleAiPart { Text = prompt }
                            }
                        }
                    },
                    GenerationConfig = new GoogleAiGenerationConfig
                    {
                        Temperature = 0.2,
                        MaxOutputTokens = 400 // Suficient pentru un rezumat
                    }
                };

                var jsonContent = JsonSerializer.Serialize(requestBody, new JsonSerializerOptions
                {
                    PropertyNamingPolicy = JsonNamingPolicy.CamelCase
                });

                var content = new StringContent(jsonContent, Encoding.UTF8, "application/json");

                // URL-ul conține cheia API ca parametru
                var requestUrl = $"{BaseUrl}{ModelName}:generateContent?key={_apiKey}";

                _logger.LogInformation("Seding Summary request to Gemini API for project: {ProjectTitle}", project.Title);

                var response = await _httpClient.PostAsync(requestUrl, content);
                var responseContent = await response.Content.ReadAsStringAsync();

                if (!response.IsSuccessStatusCode)
                {
                    var errorDetail = await response.Content.ReadAsStringAsync(); // Citeste detaliile erorii
                    _logger.LogError("Error Gemini API: {StatusCode} - {Content}", response.StatusCode, errorDetail);
                    return $"Error API ({response.StatusCode}): {errorDetail}";
                }

                // Deserializam raspunsul
                var googleResponse = JsonSerializer.Deserialize<GoogleAiResponse>(responseContent, new JsonSerializerOptions
                {
                    PropertyNameCaseInsensitive = true
                });

                // Extragem textul (Candidates[0] -> Content -> Parts[0] -> Text)
                var assistantMessage = googleResponse?.Candidates?.FirstOrDefault()?.Content?.Parts?.FirstOrDefault()?.Text;

                if (string.IsNullOrEmpty(assistantMessage))
                {
                    return "The AI has returned an empty answer.";
                }

                return CleanResponse(assistantMessage);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error in service ProjectSummaryService");
                return "AI Generating Summary has encountered an eroor.";
            }
        }

        private string CleanResponse(string text)
        {
            // Eliminam eventualele tag-uri de cod daca Gemini decide sa le puna
            return text.Replace("```", "").Replace("markdown", "").Trim();
        }
    }

    public class GoogleAiRequest
    {
        [JsonPropertyName("contents")]
        public List<GoogleAiContent> Contents { get; set; } = new();
        [JsonPropertyName("generationConfig")]
        public GoogleAiGenerationConfig? GenerationConfig { get; set; }
    }

    public class GoogleAiContent
    {
        [JsonPropertyName("parts")]
        public List<GoogleAiPart> Parts { get; set; } = new();
    }

    public class GoogleAiPart
    {
        [JsonPropertyName("text")]
        public string Text { get; set; } = string.Empty;
    }

    public class GoogleAiGenerationConfig
    {
        [JsonPropertyName("temperature")]
        public double Temperature { get; set; } = 0.7;
        [JsonPropertyName("maxOutputTokens")]
        public int MaxOutputTokens { get; set; } = 1024;
    }

    public class GoogleAiResponse
    {
        [JsonPropertyName("candidates")]
        public List<GoogleAiCandidate>? Candidates { get; set; }
    }

    public class GoogleAiCandidate
    {
        [JsonPropertyName("content")]
        public GoogleAiContent? Content { get; set; }
    }
}