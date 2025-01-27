using Docker.DotNet;

namespace Unite.Orchestrator.Docker;

public class DockerService
{
    public ImageService Images { get; }
    public ContainersService Containers { get; }
    public NetworksService Networks { get; }


    public DockerService(DockerClient client)
    {
        Images = new ImageService(client);
        Containers = new ContainersService(client);
        Networks = new NetworksService(client);
    }
}
