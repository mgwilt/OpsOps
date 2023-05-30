using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;
using OpsOps.Clients;
using OpsOps.Models;

namespace OpsOps.Data;

// an azure devops httpclient


public class AzurePipelinesService
{
    private readonly ILogger<AzurePipelinesService> _logger;
    private readonly IAzureDevOpsPipelinesHttpClient _azdoClient;

    public AzurePipelinesService(ILogger<AzurePipelinesService> logger, IAzureDevOpsPipelinesHttpClient azdoClient)
    {
        _logger = logger;
        _azdoClient = azdoClient;
    }

    public async Task<List<AzdoProject>> GetProjectsAsync()
    {
        return await _azdoClient.GetProjectsAsync();
    }
}