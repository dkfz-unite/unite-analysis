using Docker.DotNet.Models;

namespace Unite.Orchestrator.Docker.Extensions;

public static class CreateContainerExtensions
{
    public static CreateContainerParameters SetName(this CreateContainerParameters options, string name)
    {
        options.Name = name;

        return options;
    }

    public static CreateContainerParameters SetImage(this CreateContainerParameters options, string name)
    {
        options.Image = name;

        return options;
    }

    public static CreateContainerParameters SetLabel(this CreateContainerParameters options, string key, string value)
    {
        options.Labels ??= new Dictionary<string, string>();
        options.Labels[key] = value;

        return options;
    }

    public static CreateContainerParameters SetVariable(this CreateContainerParameters options, string key, string value)
    {
        options.Env ??= new List<string>();
        options.Env.Add($"{key}={value}");

        return options;
    }

    public static CreateContainerParameters SetNetwork(this CreateContainerParameters options, string id, string name, params string[] aliases)
    {
        options.HostConfig ??= new HostConfig();
        options.HostConfig.NetworkMode = id;

        options.NetworkingConfig ??= new NetworkingConfig();
        options.NetworkingConfig.EndpointsConfig ??= new Dictionary<string, EndpointSettings>();
        options.NetworkingConfig.EndpointsConfig[name] = new EndpointSettings();
        options.NetworkingConfig.EndpointsConfig[name].Aliases = aliases;

        return options;
    }

    public static CreateContainerParameters BindPort(this CreateContainerParameters options, string targetPort, string hostPort, string hostIp = "127.0.0.1")
    {
        options.HostConfig ??= new HostConfig();
        options.HostConfig.PortBindings ??= new Dictionary<string, IList<PortBinding>>();
        options.HostConfig.PortBindings[$"{targetPort}/tcp"] = new List<PortBinding> { new() { HostIP = hostIp, HostPort = hostPort } };

        return options;
    }

    public static CreateContainerParameters BindVolume(this CreateContainerParameters options, string target, string source, string mode = "rw")
    {
        options.HostConfig ??= new HostConfig();
        options.HostConfig.Binds ??= new List<string>();
        options.HostConfig.Binds.Add($"{source}:{target}:{mode}");

        return options;
    }
}
