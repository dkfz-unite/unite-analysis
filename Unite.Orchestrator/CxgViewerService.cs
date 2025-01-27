using Docker.DotNet.Models;
using Unite.Orchestrator.Configuration.Options;
using Unite.Orchestrator.Docker;
using Unite.Orchestrator.Docker.Cache;
using Unite.Orchestrator.Docker.Extensions;

namespace Unite.Orchestrator;

public class CxgViewerService
{
    private readonly IOrchestratorOptions _orchestratorOptions;
    private readonly ICxgViewerOptions _cxgViewerOptions;
    private readonly DockerService _dockerService;

    public CxgViewerService(
        IOrchestratorOptions orchestratorOptions, 
        ICxgViewerOptions cxgViewerOptions, 
        DockerService dockerService)
    {
        _orchestratorOptions = orchestratorOptions;
        _cxgViewerOptions = cxgViewerOptions;
        _dockerService = dockerService;
    }


    public async Task<string> Spawn(string key)
    {
        if (ContainerRecords.TryGet(key, out var record))
        {
            record.LastActive = DateTime.UtcNow;

            return $"{record.Number:00}";
        }        
        
        var number = await GetNumber(_cxgViewerOptions.Name, _cxgViewerOptions.Ports);
        var port = await GetPort(number, _cxgViewerOptions.Ports);
        var image = await GetImage(_cxgViewerOptions.Image);
        var name = await GetName(_cxgViewerOptions.Name, number);
        var alias = await GetAlias(_cxgViewerOptions.Alias, number);
        var path = await GetPath("unite.analysis", _orchestratorOptions.DataPath);
        var network = await GetNetwork("unite");
        
        var parameters = new CreateContainerParameters()
            .SetImage(image)
            .SetName(name)
            .SetLabel("com.docker.compose.project", "unite")
            .SetVariable("URL_PREFIX", $"/viewer/cxg{number:00}")
            .SetVariable("FILE_PATH", $"{key}/result.data.h5ad")
            .BindPort("80", $"{port}")
            .BindVolume("/app/data/", path)
            .SetNetwork(network, "unite", name, alias);
        
        var container = await _dockerService.Containers.Create(parameters);

        var success = await _dockerService.Containers.Start(container.ID);

        if (success)
        {
            ContainerRecords.Add(key, new ContainerRecord
            {
                Id = container.ID,
                Number = number,
                LastActive = DateTime.UtcNow
            });
            
            return $"{number:00}";
        }
        else
        {
            return null;
        }
    }


    private async Task<string> GetNetwork(string networkName)
    {
        var networks = await _dockerService.Networks.FindByName(name => name == networkName);
        var network = networks.FirstOrDefault();

        if (network == null)
            throw new Exception($"Network '{networkName}' not found");

        return network.ID;
    }

    private async Task<string> GetImage(string imageName)
    {
        var images = await _dockerService.Images.FindByName(name => name == imageName);
        var image = images.FirstOrDefault();

        if (image == null)
            throw new Exception($"Image '{imageName}' not found");

        return image.ID;
    }

    private async Task<string> GetPath(string containerName, string mountPath)
    {
        var containers = await _dockerService.Containers.FindByName(name => name.Contains(containerName));
        var container = containers.FirstOrDefault();

        if (container == null)
            return mountPath;

        var mount = container.Mounts.FirstOrDefault(mount => mount.Destination == mountPath);

        if (mount == null)
            return mountPath;

        return mount.Source["/host_mnt".Length..];
    }

    private async Task<int> GetNumber(string containerName, int[] containerPorts)
    {
        var cntainerShortName = containerName.Replace("{n}", "");
        var containers = await _dockerService.Containers.FindByName(name => name.Contains(cntainerShortName));

        var actualNumbers = containers
            .Select(container => container.Names.First(name => name.Contains(cntainerShortName)))
            .Select(name => name.TrimStart('/').Replace(cntainerShortName, ""))
            .Select(name => int.Parse(name))
            .ToArray();

        var cachedNumbers = ContainerRecords.Records
            .Select(record => record.Value.Number)
            .ToArray();

        var mergedNumbers = actualNumbers.Concat(cachedNumbers).ToArray();

        var maxNumber = containerPorts[1] - containerPorts[0] + 1;
        var number = Enumerable.Range(1, maxNumber).Except(mergedNumbers).FirstOrDefault();

        return number;    
    }

    private async Task<int> GetPort(int containerNumber, int[] containerPorts)
    {
        var port = containerPorts[0] + containerNumber - 1;

        return await Task.FromResult(port);
    }

    private async Task<string> GetName(string containerName, int containerNumber)
    {
        var name = containerName.Replace("{n}", $"{containerNumber:00}");

        return await Task.FromResult(name);
    }

    private async Task<string> GetAlias(string containerAlias, int containerNumber)
    {
        var alias = containerAlias.Replace("{n}", $"{containerNumber:00}");

        return await Task.FromResult(alias);
    }
}
