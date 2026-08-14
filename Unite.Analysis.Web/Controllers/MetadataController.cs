using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Unite.Analysis.Models.Metadata;
using Unite.Analysis.Services;
using Unite.Data.Context;

namespace Unite.Analysis.Web.Controllers;

[Route("api/[controller]")]
[Authorize]
public class MetadataController : Controller
{
    private readonly IDbContextFactory<DomainDbContext> _dbContextFactory;

    public record MappingOption(string Label, string Value);
    public record MappingGroup(string Key, string Name, MappingOption[] Options, MappingGroup[] Children = null);
    

    public MetadataController(IDbContextFactory<DomainDbContext> dbContextFactory)
    {
        _dbContextFactory = dbContextFactory;
    }


    [HttpGet("options")]
    public IActionResult GetOptions()
    {
        var mappings = new MetadataMappings<SampleMetadata>();

        string[] options =
        [
            ..mappings.Donor.Select(mapping => mapping.Key),
            ..mappings.Image.Select(mapping => mapping.Key),
            ..mappings.ImageMr.Select(mapping => mapping.Key),
            ..mappings.Specimen.Select(mapping => mapping.Key),
            ..mappings.SpecimenMaterial.Select(mapping => mapping.Key),
            ..mappings.SpecimenLine.Select(mapping => mapping.Key),
            ..mappings.SpecimenOrganoid.Select(mapping => mapping.Key),
            ..mappings.SpecimenXenograft.Select(mapping => mapping.Key)
        ];

        return Ok(options);

        // MappingGroup[] groups =
        // [
        //     new ("donor", "Donor", GetMappingOptions(mappings.Donor)),

        //     new ("image", "Image", GetMappingOptions(mappings.Image), [
        //             new ("mr", "MR", GetMappingOptions(mappings.ImageMr))
        //         ]),

        //     new("specimen", "Specimen", GetMappingOptions(mappings.Specimen), [
        //             new ("material", "Material", GetMappingOptions(mappings.SpecimenMaterial)),
        //             new ("line", "Cell Line", GetMappingOptions(mappings.SpecimenLine)),
        //             new ("organoid", "Organoid", GetMappingOptions(mappings.SpecimenOrganoid)),
        //             new ("xenograft", "Xenograft", GetMappingOptions(mappings.SpecimenXenograft))
        //         ])
        // ];

        // return Ok(groups);
    }

    [HttpGet("values")]
    public IActionResult GetValues(string key)
    {
        var mappings = new DataMappings();

        var mapping = mappings.All.FirstOrDefault(mapping => mapping.Key == key);

        if (mapping != null)
        {
            using var dbContext = _dbContextFactory.CreateDbContext();

            return Ok(mapping.Autocomplete(dbContext));
        }
        else
        {
            return Ok(Array.Empty<string>());
        }
    }
   

    private static MappingOption[] GetMappingOptions(IEnumerable<Mapping<SampleMetadata, string>> mappings)
    {
        return mappings.Select(mapping => new MappingOption(mapping.Key, mapping.Name)).ToArray();
    }
}
