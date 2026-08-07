using Unite.Analysis.Models.Metadata;
using Unite.Analysis.Services.CnvProfile.Models.Output;
using Unite.Data.Context.Repositories;
using Unite.Data.Entities.Omics.Analysis.Dna.Cnv;
using Unite.Data.Entities.Omics.Analysis.Dna.Cnv.Enums;
using Unite.Data.Entities.Omics.Enums;
using Unite.Essentials.Extensions;

namespace Unite.Analysis.Services.CnvProfile;

public class ProcessingService
{
    private readonly CnvProfilesRepository _cnvProfilesRepository;
    
    public ProcessingService(CnvProfilesRepository cnvProfilesRepository)
    {
        _cnvProfilesRepository = cnvProfilesRepository;
    }
    
    public async Task<GridData> ProcessData(SamplesContext context, Models.Criteria.Options options)
    {
        var gridSamples = new List<GridSample>();
        var gridObservations = new List<GridObservation>();
        var gridRegions = Enum.GetValues<Chromosome>()
            .Where(chromosome => chromosome != Chromosome.ChrMT)
            .SelectMany(chromosome => new GridRegion[] { new(chromosome, ChromosomeArm.P), new(chromosome, ChromosomeArm.Q) })
            .ToArray();

        var mappings = new MetadataMappings<SampleMetadata>();
        var metadata = SampleMetadataLoader.Load(context);
        var sampleIds = context.OmicsSamples.Keys.ToArray();
        var cnvProfiles = await _cnvProfilesRepository.GetRelatedProfiles(sampleIds);

        foreach (var sampleId in sampleIds)
        {
            foreach (var region in gridRegions)
            {
                var cnvProfile = cnvProfiles.FirstOrDefault(profile =>
                    profile.SampleId == sampleId &&
                    profile.ChromosomeId == region.Chromosome &&
                    profile.ChromosomeArmId == region.Arm
                );

                var observationEvent = GetEvent(cnvProfile, options.EventThreshold);
                if (observationEvent != CnvType.Neutral)
                    gridObservations.Add(new GridObservation(sampleId, region.Id, observationEvent));
            }

            var donor = context.GetSampleDonor(sampleId);
            var specimen = context.GetSampleSpecimen(sampleId);
            var sample = context.OmicsSamples[sampleId];
            var tracks = new Dictionary<string, string>();

            foreach (var propertyKey in options.TrackProperty)
            {
                var sampleMetadata = metadata.FirstOrDefault(entry => entry.Key == sampleId);
                var propertyMapping = mappings.All.FirstOrDefault(mapping => mapping.Key == propertyKey);
                var propertyGetter = propertyMapping?.Expression.Compile();
                var propertyValue = propertyGetter?.Invoke(sampleMetadata);

                if (options.TrackPropertyValue.IsNotEmpty())
                    if (!options.TrackPropertyValue.Contains(propertyValue))
                        continue;
                
                tracks.Add(propertyKey, propertyValue);
            }

            gridSamples.Add(new GridSample(sampleId, donor.Id, specimen.Id, tracks));
        }

        return new GridData(gridSamples.ToArray(), gridRegions.ToArray(), gridObservations.ToArray());
    }

    private static CnvType GetEvent(Profile cnvProfile, double eventThreshold = 0.8)
    {
        if (cnvProfile != null)
        {
            if(cnvProfile.Gain > eventThreshold)
                return CnvType.Gain;
        
            if(cnvProfile.Loss > eventThreshold)
                return CnvType.Loss;
        }

        return CnvType.Neutral;
    }
}
