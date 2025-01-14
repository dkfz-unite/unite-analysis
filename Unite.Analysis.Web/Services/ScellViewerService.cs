using System.Runtime.InteropServices;
using Docker.DotNet;
using Docker.DotNet.Models;
using Unite.Analysis.Configuration.Options;
using Unite.Analysis.Web.Services.Cache;

namespace Unite.Analysis.Web.Services;

public class ScellViewerService
{
    private static readonly string _imageNameDeploy = "ghcr.io/dkfz-unite/unite-cellxgene:latest";
    private static readonly string _imageNameBuild = "unite-cellxgene";
    private static readonly string _containerName = "unite.analysis.scell.view{n}";
    private static readonly string _containerAlias = "view{n}.scell.analysis.unite.net";
    private static readonly string _containerPort = "543{n}";

    private readonly IAnalysisOptions _options;

    
    public ScellViewerService(IAnalysisOptions options)
    {
        _options = options;
    }
    

    public async Task<string> Spawn(string key, bool local)
    {
        if (ContainersCache.TryGet(key, out var record))
            return $"{record.Number:00}";

        using var client = CreateClient();

        var network = await GetNetwork(client, "unite");
        var path = await GetDataPath(client, _options.DataPath);
        var number = await GetNumber(client, _containerName.Replace("{n}", ""));
        var name = GetName(number);
        var alias = GetAlias(number);
        var port = GetPort(number);

        var parameters = new CreateContainerParameters
        {
            Image = await GetImage(client, local),
            Name = name,
            Env =
            [
                $"URL_PREFIX=/viewer/cxg{number:00}",
                $"FILE_PATH={key}/result.data.h5ad"
            ],
            HostConfig = new HostConfig
            {
                PortBindings = new Dictionary<string, IList<PortBinding>>
                { 
                    { "80/tcp", [new PortBinding() { HostIP = "127.0.0.1", HostPort = $"{port}" }] }
                },
                Binds =
                [
                    $"{path}:/app/data/:rw"
                ],
                NetworkMode = network.ID,
                AutoRemove = true,
                PublishAllPorts = true
            },
            NetworkingConfig = new NetworkingConfig
            {
                EndpointsConfig = new Dictionary<string, EndpointSettings> { { network.Name, new() { Aliases = [name, alias] } } }
            }
        };

        var container = await client.Containers.CreateContainerAsync(parameters);

        var success = await client.Containers.StartContainerAsync(container.ID, new ContainerStartParameters());

        if (success)
        {
            record = new CacheRecord
            {
                Name = name,
                Number = number,
                LastActive = DateTime.UtcNow
            };

            ContainersCache.Add(key, record);
            
            return $"{number:00}";
        }
        else
        {
            return null;
        }
    }

    public async Task Update(string key)
    {
        if (ContainersCache.TryGet(key, out var record))
            record.LastActive = DateTime.UtcNow;

        await Task.CompletedTask;
    }

    public async Task Kill(string key)
    {
        if (!ContainersCache.TryGet(key, out var record))
            return;

        using var client = CreateClient();

        var parameters = new ContainerRemoveParameters() { Force = true };

        await client.Containers.RemoveContainerAsync(record.Name, parameters);

        ContainersCache.Remove(key);
    }
    

    private static string GetName(int number)
    {
        return _containerName.Replace("{n}", $"{number:00}");
    }

    private static string GetAlias(int number)
    {
        return _containerAlias.Replace("{n}", $"{number:00}");
    }

    private static string GetPort(int number)
    {
        return _containerPort.Replace("{n}", $"{number:00}");
    }

    private static async Task<string> GetImage(DockerClient client, bool local)
    {
        var images = await client.Images.ListImagesAsync(new ImagesListParameters() { All = true });

        var buildImage = images.FirstOrDefault(image => image.RepoTags.Any(tag => tag.Contains(_imageNameBuild)));

        var deployImage = images.FirstOrDefault(image => image.RepoTags.Any(tag => tag.Contains(_imageNameDeploy)));

        if (local)
            return buildImage?.ID ?? deployImage?.ID;
        else
            return deployImage?.ID ?? buildImage?.ID;
    }

    private static async Task<string> GetDataPath(DockerClient client, string path)
    {
        var parameters = new ContainersListParameters() { All = true };
        var containers = await client.Containers.ListContainersAsync(parameters);
        var container = containers.FirstOrDefault(container => container.Names.Any(name => name.Contains("unite.analysis")));

        if (container == null)
            return path;

        var mount = container.Mounts.FirstOrDefault(mount => mount.Destination == path);

        if (mount == null)
            return path;

        return mount.Source["/host_mnt".Length..];
    }

    private static async Task<int> GetNumber(DockerClient client, string containerName)
    {
        var parameters = new ContainersListParameters() { All = true };
        var containers = await client.Containers.ListContainersAsync(parameters);

        var actualNumbers = containers
            .Where(container => container.Names.Any(name => name.Contains(containerName)))
            .Select(container => container.Names.First(name => name.Contains(containerName)))
            .Select(name => name.TrimStart('/'))
            .Select(name => int.Parse(name[(containerName.Length+1)..]))
            .ToArray();

        var cachedNumbers = ContainersCache.Records
            .Select(record => record.Value.Number);

        var mergedNumbers = actualNumbers.Concat(cachedNumbers).Distinct();

        var number = Enumerable.Range(1, 99).Except(mergedNumbers).First();

        return number;
    }

    private static async Task<NetworkResponse> GetNetwork(DockerClient client, string networkName)
    {
        var networks = await client.Networks.ListNetworksAsync();

        var network = networks.FirstOrDefault(network => network.Name == networkName);

        return network;
    }

    private static DockerClient CreateClient()
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
