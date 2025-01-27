namespace Unite.Orchestrator.Configuration.Options;

public interface IOrchestratorOptions
{
    int IdleTimeout { get; }
    string DataPath { get; }
}
