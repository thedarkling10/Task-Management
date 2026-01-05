using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;
using TaskManagementApp.Models;

namespace TaskManagementApp.Services
{
    public class ProjectSummaryService : IProjectSummaryService
    {
        private readonly HttpClient _httpClient;
        private readonly string _apiKey;
        private readonly ILogger<ProjectSummaryService> _logger;



        public ProjectSummaryService(
            IConfiguration configuration,
            ILogger<ProjectSummaryService> logger)
        {
            _httpClient = new HttpClient();
            _apiKey = configuration["OpenAI:ApiKey"]
                ?? throw new ArgumentNullException("OpenAI:ApiKey missing");

            _logger = logger;

            _httpClient.BaseAddress = new Uri("https://api.openai.com/v1/");
            _httpClient.DefaultRequestHeaders.Authorization =
                new AuthenticationHeaderValue("Bearer", _apiKey);
            _httpClient.DefaultRequestHeaders.Accept
                .Add(new MediaTypeWithQualityHeaderValue("application/json"));
        }

        public async Task<string> GenerateSummaryAsync(Project project)
        {
            // CAZ SPECIAL: fără activitate
            if (project.Tasks == null || !project.Tasks.Any())
            {
                return "Nu există actualizări recente pentru acest proiect.";
            }

            var tasksText = string.Join("\n",
                project.Tasks.Select(t =>
                    $"- {t.Title} | Status: {t.Status} | Deadline: {t.EndDate:d}"
                )
            );

            var systemPrompt = """
            You are a project management assistant.
            Generate a concise project summary including:
            - Overall progress
            - Important deadlines
            - Current task statuses

            If there is no recent activity, say:
            "Nu există actualizări recente pentru acest proiect."
            """;

            var userPrompt = $"""
            Project title: {project.Title}

            Tasks:
            {tasksText}
            """;

            var requestBody = new
            {
                model = "gpt-4o-mini",
                messages = new[]
                {
                    new { role = "system", content = systemPrompt },
                    new { role = "user", content = userPrompt }
                },
                temperature = 0.2,
                max_tokens = 200
            };

            var content = new StringContent(
                JsonSerializer.Serialize(requestBody),
                Encoding.UTF8,
                "application/json");

            var response = await _httpClient.PostAsync("chat/completions", content);
            var responseText = await response.Content.ReadAsStringAsync();

            if (!response.IsSuccessStatusCode)
            {
                _logger.LogError("OpenAI error: {Response}", responseText);
                return $"Eroare OpenAI: {response.StatusCode} - {responseText}";
            }

            using var jsonDoc = JsonDocument.Parse(responseText);
            return jsonDoc.RootElement
                .GetProperty("choices")[0]
                .GetProperty("message")
                .GetProperty("content")
                .GetString()!;
        }

    }
}
