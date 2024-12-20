using System.Runtime.InteropServices;
using Docker.DotNet;
using Docker.DotNet.Models;
using Unite.Analysis.Configuration.Options;
using Unite.Analysis.Web.Services.Cache;

namespace Unite.Analysis.Web.Services;

public class ScellViewerService
{
    private static readonly string _imageNameDeploy = "ghcr.io/dkfz-unite/unite-analysis-scell-view:latest";
    private static readonly string _imageNameBuild = "unite.analysis.scell.view";
    private static readonly string _containerName = "unite.analysis.scell.view";
    private static readonly string _containerAlias = "view.scell.analysis.unite.net";

    private readonly IAnalysisOptions _options;

    
    public ScellViewerService(IAnalysisOptions options)
    {
        _options = options;
    }
    

    public async Task<string> Spawn(string key, bool local)
    {
        if (ContainersCache.TryGet(key, out var record))
            return $"{record.RemoteUrl}|{record.LocalUrl}";

        using var client = CreateClient();

        var number = await GetNumber(client, _containerName);
        var dataPath = await GetDataPath(client, _options.DataPath);
        var certsPath = await GetCertificatesPath(client);
        var network = await GetNetwork(client, "unite");
        var instanceName = GetName(number);
        var instanceAlias = GetAlias(number);
        var instancePort = GetPort(number);

        var parameters = new CreateContainerParameters
        {
            Image = await GetImage(client, local),
            Name = instanceName,
            Env =
            [
                $"ASPNETCORE_ENVIRONMENT=Release", 
                $"FILE_PATH={key}/result.data.h5ad"
            ],
            HostConfig = new HostConfig
            {
                PortBindings = new Dictionary<string, IList<PortBinding>>
                { 
                    { "443/tcp", [new PortBinding() { HostIP = "127.0.0.1", HostPort = $"{instancePort}" }] },
                    // { "80/tcp", [new PortBinding() { HostIP = "127.0.0.1", HostPort = $"{instancePort + 1}" }] }
                },
                Binds =
                [
                    $"{dataPath}:/data/:rw",
                    $"{certsPath}:/https/:ro"
                ],
                NetworkMode = network.ID,
                AutoRemove = true,
                PublishAllPorts = true
            },
            NetworkingConfig = new NetworkingConfig
            {
                EndpointsConfig = new Dictionary<string, EndpointSettings> { { network.Name, new() { Aliases = [instanceName, instanceAlias] } } }
            }
        };

        var container = await client.Containers.CreateContainerAsync(parameters);

        var success = await client.Containers.StartContainerAsync(container.ID, new ContainerStartParameters());

        if (success)
        {
            record = new CacheRecord
            {
                Name = instanceName,
                LastActive = DateTime.UtcNow,
                LocalUrl = $"https://localhost:{instancePort}",
                RemoteUrl = $"https://{instanceAlias}"
            };

            ContainersCache.Add(key, record);
            
            return string.Join(" | ", record.RemoteUrl, record.LocalUrl);;
        }
        else
        {
            return null;
        }
    }

    public async Task<bool> Ping(string key, bool local)
    {
        if (!ContainersCache.TryGet(key, out var record))
            return false;

        try
        {
            var handler = new HttpClientHandler
            { 
                UseProxy = false, 
                ClientCertificateOptions = ClientCertificateOption.Manual,
                ServerCertificateCustomValidationCallback = (message, cert, chain, errors) => true
            };

            using var client = new HttpClient(handler);

            var respone = await client.GetAsync(local ? record.LocalUrl : record.RemoteUrl);

            return respone.IsSuccessStatusCode;;
        }
        catch
        {
            return false;
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
        return $"{_containerName}{number:00}";
    }

    private static string GetAlias(int number)
    {
        var parts = _containerAlias.Split('.');
        parts[0] += $"{number:00}";
        return string.Join('.', parts);
    }

    private static int GetPort(int number)
    {
        return int.Parse($"543{number:00}");
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

    private static async Task<string> GetCertificatesPath(DockerClient client)
    {
        var parameters = new ContainersListParameters() { All = true };
        var containers = await client.Containers.ListContainersAsync(parameters);
        var container = containers.FirstOrDefault(container => container.Names.Any(name => name.Contains("unite.portal")));

        if (container == null)
            return null;

        var mount = container.Mounts.FirstOrDefault(mount => mount.Destination == "/https");

        if (mount == null)
            return null;

        return mount.Source["/host_mnt".Length..];
    }

    private static async Task<int> GetNumber(DockerClient client, string name)
    {
        var parameters = new ContainersListParameters() { All = true };
        var containers = await client.Containers.ListContainersAsync(parameters);

        var actualNumbers = containers
            .Where(container => container.Names.Any(cintainerName => cintainerName.Contains(name)))
            .Select(container => container.Names.First(cintainerName => cintainerName.Contains(name)))
            .Select(cintainerName => cintainerName.TrimStart('/'))
            .Select(cintainerName => int.Parse(cintainerName[(name.Length+1)..]))
            .ToArray();

        var cachedNumbers = ContainersCache.Records
            .Select(record => int.Parse(record.Value.Name[name.Length..]));

        var mergedNumbers = actualNumbers.Concat(cachedNumbers).Distinct();

        var number = Enumerable.Range(1, 99).Except(mergedNumbers).First();

        return number;
    }

    private static async Task<NetworkResponse> GetNetwork(DockerClient client, string name)
    {
        var networks = await client.Networks.ListNetworksAsync();

        var network = networks.FirstOrDefault(n => n.Name == name);

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
