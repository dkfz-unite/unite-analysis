using System.Runtime.InteropServices;
using Docker.DotNet;

namespace Unite.Orchestrator.Docker;

public static class ClientService
{
    public static DockerClient CreateClient()
    {
        var socket = RuntimeInformation.IsOSPlatform(OSPlatform.Windows) ? "npipe://./pipe/docker_engine" :
                     RuntimeInformation.IsOSPlatform(OSPlatform.Linux) ? "unix:///var/run/docker.sock" :
                     RuntimeInformation.IsOSPlatform(OSPlatform.OSX) ? "unix:///var/run/docker.sock" :
                     throw new PlatformNotSupportedException();

        var config = new DockerClientConfiguration(new Uri(socket));

        var client = config.CreateClient();

        return client;
    }
}
