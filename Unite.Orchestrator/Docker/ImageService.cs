using Docker.DotNet;
using Docker.DotNet.Models;

namespace Unite.Orchestrator.Docker;

public class ImageService
{
    private readonly DockerClient _client;


    public ImageService(DockerClient client)
    {
        _client = client;
    }


    public async Task<ImagesListResponse[]> FindByName(Func<string, bool> predicate)
    {
        var images = await _client.Images.ListImagesAsync(new ImagesListParameters());

        return images.Where(image => image.RepoTags.Any(tag => predicate(tag))).ToArray();
    }
}
