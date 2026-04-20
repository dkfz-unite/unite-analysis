using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Unite.Analysis.Models.Metadata;
using Unite.Analysis.Services;

namespace Unite.Analysis.Web.Controllers;

[Route("api/[controller]")]
[Authorize]
public class MetadataController : Controller
{
    public record MappingOption(string Label, string Value);
    public record MappingGroup(string Key, string Name, MappingOption[] Options, MappingGroup[] Children = null);
    
    [HttpGet("options")]
    public IActionResult GetOptions()
    {
        var mappings = new Mappings<SampleMetadata>();

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

    private static MappingOption[] GetMappingOptions(IEnumerable<Mapping<SampleMetadata, string>> mappings)
    {
        return mappings.Select(mapping => new MappingOption(mapping.Key, mapping.Name)).ToArray();
    }
}
