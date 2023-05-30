
using System.Net.Http.Headers;
using System.Text.Json;
using OpsOps.Models;

namespace OpsOps.Clients;

public interface IAzureDevOpsPipelinesHttpClient
{
    Task<List<AzdoProject>> GetProjectsAsync();
}

public class AzureDevOpsPipelinesHttpClient : IAzureDevOpsPipelinesHttpClient
{
    private readonly ILogger<AzureDevOpsPipelinesHttpClient> _logger;
    private readonly HttpClient _httpClient;

    public AzureDevOpsPipelinesHttpClient(IConfiguration configuration, 
        ILogger<AzureDevOpsPipelinesHttpClient> logger, 
        HttpClient httpClient)
    {
        _logger = logger;
        _httpClient = httpClient;

        var credentials = Convert.ToBase64String(System.Text.ASCIIEncoding.ASCII.GetBytes(string.Format("{0}:{1}", "", configuration["azdo:pat"])));

        _httpClient.DefaultRequestHeaders.Accept.Clear();
        _httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Basic", credentials);    
        _httpClient.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));    
        _httpClient.BaseAddress = new Uri($"https://dev.azure.com/{configuration["azdo:org"]}/");
    }

    public async Task<List<AzdoProject>> GetProjectsAsync()
    {
        var projects = new List<AzdoProject>();
        var response = await _httpClient.GetAsync($"_apis/projects?api-version=6.0");

        if (response.IsSuccessStatusCode)
        {
            var content = await response.Content.ReadAsStringAsync();
            var json = JsonDocument.Parse(content);
            var values = json.RootElement.GetProperty("value").EnumerateArray();

            foreach (var value in values)
            {
                var project = new AzdoProject();
                project.Id = value.TryGetProperty("id", out var id) ? id.GetString() ?? "" : "";
                project.Name = value.TryGetProperty("name", out var name) ? name.GetString() ?? "" : "";
                project.Description = value.TryGetProperty("description", out var description) ? description.GetString() ?? "" : "";
                project.Url = value.TryGetProperty("url", out var url) ? url.GetString() ?? "" : "";
                project.State = value.TryGetProperty("state", out var state) ? state.GetString() ?? "" : "";
                project.DefaultTeamImageUrl = value.TryGetProperty("defaultTeamImageUrl", out var defaultTeamImageUrl) ? defaultTeamImageUrl.GetString() ?? "" : "";
                project.Links = value.TryGetProperty("_links", out var links) ? links.GetString() ?? "" : "";
                projects.Add(project);
            }
        }

        // log section with a header explaining what is coming next and then all of the projects
        _logger.LogInformation("Projects:");
        foreach (var project in projects)
        {
            _logger.LogInformation($"  {project.Id} {project.Name}");
        }

        return projects;
    }

    public async Task<List<AzdoPipeline>> GetYamlPipelinesAsync(string projectId)
    {
        var pipelines = new List<AzdoPipeline>();
        var response = await _httpClient.GetAsync($"{projectId}/_apis/pipelines?api-version=6.0");

        if (response.IsSuccessStatusCode)
        {
            var content = await response.Content.ReadAsStringAsync();
            var json = JsonDocument.Parse(content);
            var values = json.RootElement.GetProperty("value").EnumerateArray();

            foreach (var value in values)
            {
                if (value.TryGetProperty("configuration", out var configuration) &&
                    configuration.TryGetProperty("type", out var type) && type.GetString() == "yaml")
                {
                    var pipeline = new AzdoPipeline();
                    pipeline.Id = value.TryGetProperty("id", out var id) ? id.GetString() ?? "" : "";
                    pipeline.Name = value.TryGetProperty("name", out var name) ? name.GetString() ?? "" : "";
                    pipeline.ConfigurationPath = configuration.GetProperty("path").GetString() ?? "";
                    pipeline.Url = value.TryGetProperty("url", out var url) ? url.GetString() ?? "" : "";
                    pipelines.Add(pipeline);
                }
            }
        }

        return pipelines;
    }

    public async Task<List<AzdoClassicPipeline>> GetClassicPipelinesAsync(string projectId)
    {
        var pipelines = new List<AzdoClassicPipeline>();
        var response = await _httpClient.GetAsync($"{projectId}/_apis/pipelines?api-version=6.0");

        if (response.IsSuccessStatusCode)
        {
            var content = await response.Content.ReadAsStringAsync();
            var json = JsonDocument.Parse(content);
            var values = json.RootElement.GetProperty("value").EnumerateArray();

            foreach (var value in values)
            {
                if (value.TryGetProperty("configuration", out var configuration) &&
                    configuration.TryGetProperty("type", out var type) && type.GetString() == "designer")
                {
                    var pipeline = new AzdoClassicPipeline();
                    pipeline.Id = value.TryGetProperty("id", out var id) ? id.GetString() ?? "" : "";
                    pipeline.Name = value.TryGetProperty("name", out var name) ? name.GetString() ?? "" : "";
                    pipeline.Url = value.TryGetProperty("url", out var url) ? url.GetString() ?? "" : "";
                    pipelines.Add(pipeline);
                }
            }
        }

        return pipelines;
    }

    public async Task<AzdoRepository> GetRepositoryByPipelineIdAsync(string projectId, string pipelineId)
    {
        var response = await _httpClient.GetAsync($"{projectId}/_apis/pipelines/{pipelineId}?api-version=6.0");

        if (!response.IsSuccessStatusCode) return null;

        var content = await response.Content.ReadAsStringAsync();
        var json = JsonDocument.Parse(content);

        if (json.RootElement.TryGetProperty("repository", out var repository))
        {
            var azdoRepository = new AzdoRepository();
            azdoRepository.Id = repository.TryGetProperty("id", out var id) ? id.GetString() : "";
            azdoRepository.Name = repository.TryGetProperty("name", out var name) ? name.GetString() : "";
            
            if (json.RootElement.TryGetProperty("configuration", out var configuration) &&
                configuration.TryGetProperty("path", out var path))
            {
                azdoRepository.ConfigurationPath = path.GetString();
            }

            return azdoRepository;
        }

        return null;
    }
    
    public async Task<string> GetYamlFileContentsByPipelineIdAsync(string projectId, string pipelineId)
    {
        var repository = await GetRepositoryByPipelineIdAsync(projectId, pipelineId);

        if (repository == null || string.IsNullOrEmpty(repository.ConfigurationPath)) return string.Empty;

        var response = await _httpClient.GetAsync($"{projectId}/_apis/git/repositories/{repository.Name}/items?scopePath={repository.ConfigurationPath}&api-version=6.0");

        if (!response.IsSuccessStatusCode) return string.Empty;

        var content = await response.Content.ReadAsStringAsync();

        return content;
    }
}

