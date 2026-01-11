using TaskManagementApp.Models;

namespace TaskManagementApp.Services
{
    public interface IProjectSummaryService
    {
        Task<string> GenerateSummaryAsync(Project project);
    }
}
