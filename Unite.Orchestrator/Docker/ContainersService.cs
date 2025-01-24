using Docker.DotNet;
using Docker.DotNet.Models;

namespace Unite.Orchestrator.Docker;

public class ContainersService
{
    private readonly DockerClient _client;


    public ContainersService(DockerClient client)
    {
        _client = client;
    }


    public async Task<ContainerListResponse[]> FindByName(Func<string, bool> predicate, ContainersListParameters parameters = null)
    {
        var listParameters = parameters ?? new ContainersListParameters() { All = true };
        var containers = await _client.Containers.ListContainersAsync(listParameters);

        return containers.Where(container => container.Names.Any(name => predicate(name))).ToArray();
    }

    public async Task<CreateContainerResponse> Create(CreateContainerParameters parameters)
    {
        return await _client.Containers.CreateContainerAsync(parameters);
    }

    public async Task<bool> Start(string id, ContainerStartParameters parameters = null)
    {
        var startParameters = parameters ?? new ContainerStartParameters();

        return await _client.Containers.StartContainerAsync(id, startParameters);
    }

    public async Task Remove(string id, ContainerRemoveParameters parameters = null)
    {
        var killParameters = parameters ?? new ContainerRemoveParameters() { Force = true };

        await _client.Containers.RemoveContainerAsync(id, killParameters);
    }
}
