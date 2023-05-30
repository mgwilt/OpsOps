namespace OpsOps.Models;

public class AzdoSecrets
{
    public string? Org { get; set; }
    public string? Pat { get; set; }
}

public class AzdoProject
{
    public string? Id { get; set; }
    public string? Name { get; set; }
    public string? Description { get; set; }
    public string? Url { get; set; }
    public string? State { get; set; }
    public string? Revision { get; set; }
    public string? Visibility { get; set; }
    public string? LastUpdateTime { get; set; }
    public string? DefaultTeamImageUrl { get; set; }
    public string? Links { get; set; }
}

public class AzdoClassicPipeline
{
    public string? Id { get; set; }
    public string? Name { get; set; }
    public string? Url { get; set; }
}

public class AzdoPipeline
{
    public string? Id { get; set; }
    public string? Name { get; set; }
    public string? ConfigurationPath { get; set; }
    public string? Url { get; set; }
}

public class AzdoRepository
{
    public string? Id { get; set; }
    public string? Name { get; set; }
    public string? ConfigurationPath { get; set; }
}