using Docker.DotNet;
using Docker.DotNet.Models;

namespace Unite.Orchestrator.Docker;

public class NetworksService
{
    private readonly DockerClient _client;


    public NetworksService(DockerClient client)
    {
        _client = client;
    }


    public async Task<NetworkResponse[]> FindByName(Func<string, bool> predicate)
    {
        var networks = await _client.Networks.ListNetworksAsync();

        return networks.Where(network => predicate(network.Name)).ToArray();
    }
}
