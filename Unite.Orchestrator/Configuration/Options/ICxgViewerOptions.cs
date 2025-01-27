namespace Unite.Orchestrator.Configuration.Options;

public interface ICxgViewerOptions
{
    string Image { get; }
    string Name { get; }
    string Alias { get; }
    int[] Ports { get; }
}
