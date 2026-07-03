using Unite.Analysis.Services.CnvProfile.Models.Output;
using Unite.Data.Context.Repositories;
using Unite.Data.Entities.Omics.Analysis.Dna.Cnv;
using Unite.Data.Entities.Omics.Enums;
using Unite.Essentials.Extensions;
using ChromosomeArm = Unite.Data.Entities.Omics.Enums.ChromosomeArm;

namespace Unite.Analysis.Services.CnvProfile;

public class ProcessingService
{
    private readonly CnvProfilesRepository _cnvProfilesRepository;

    private readonly Dictionary<Chromosome, ChromosomeArm[]> _chromosomeArmMap = new()
    {
        { Chromosome.Chr1, [ChromosomeArm.P, ChromosomeArm.Q] },
        { Chromosome.Chr2, [ChromosomeArm.P, ChromosomeArm.Q] },
        { Chromosome.Chr3, [ChromosomeArm.P, ChromosomeArm.Q] },
        { Chromosome.Chr4, [ChromosomeArm.P, ChromosomeArm.Q] },
        { Chromosome.Chr5, [ChromosomeArm.P, ChromosomeArm.Q] },
        { Chromosome.Chr6, [ChromosomeArm.P, ChromosomeArm.Q] },
        { Chromosome.Chr7, [ChromosomeArm.P, ChromosomeArm.Q] },
        { Chromosome.Chr8, [ChromosomeArm.P, ChromosomeArm.Q] },
        { Chromosome.Chr9, [ChromosomeArm.P, ChromosomeArm.Q] },
        { Chromosome.Chr10, [ChromosomeArm.P, ChromosomeArm.Q] },
        { Chromosome.Chr11, [ChromosomeArm.P, ChromosomeArm.Q] },
        { Chromosome.Chr12, [ChromosomeArm.P, ChromosomeArm.Q] },
        { Chromosome.Chr13, [ChromosomeArm.P, ChromosomeArm.Q] },
        { Chromosome.Chr14, [ChromosomeArm.P, ChromosomeArm.Q] },
        { Chromosome.Chr15, [ChromosomeArm.P, ChromosomeArm.Q] },
        { Chromosome.Chr16, [ChromosomeArm.P, ChromosomeArm.Q] },
        { Chromosome.Chr17, [ChromosomeArm.P, ChromosomeArm.Q] },
        { Chromosome.Chr18, [ChromosomeArm.P, ChromosomeArm.Q] },
        { Chromosome.Chr19, [ChromosomeArm.P, ChromosomeArm.Q] },
        { Chromosome.Chr20, [ChromosomeArm.P, ChromosomeArm.Q] },
        { Chromosome.Chr21, [ChromosomeArm.P, ChromosomeArm.Q] },
        { Chromosome.Chr22, [ChromosomeArm.P, ChromosomeArm.Q] },
        { Chromosome.ChrX, [ChromosomeArm.P, ChromosomeArm.Q] },
        { Chromosome.ChrY, [ChromosomeArm.P, ChromosomeArm.Q] }
    };
    
    public ProcessingService(CnvProfilesRepository cnvProfilesRepository)
    {
        _cnvProfilesRepository = cnvProfilesRepository;
    }
    
    public async Task<ResultMatrix> ProcessData(SamplesContext context, Models.Criteria.Options options)
    {
        Console.WriteLine($"CNVP Analysis");
        Console.WriteLine($"Related Donors: {context.Donors.Keys.Count}");
        Console.WriteLine($"Related Samples: {context.OmicsSamples.Keys.Count}");
        Console.WriteLine($"Related Specimens: {context.Specimens.Keys.Count}");
        
        var sampleIds = context.OmicsSamples.Keys.ToArray();
        var cnvProfiles = await _cnvProfilesRepository.GetRelatedProfiles(sampleIds);
        
        Console.WriteLine($"Related CNVPs: {cnvProfiles.Length}");
        
        var armsCount = GetArmsCount();

        var model = new ResultMatrix
        {
            DnaRegions = new Models.Output.DnaRegion[armsCount],
            Samples = new Sample[sampleIds.Length],
            Observations = new List<Observation>()
        };
        
        int k = 0;
        foreach (var mapEntry in _chromosomeArmMap)
        {
            var chromosome = mapEntry.Key;
            foreach (var arm in mapEntry.Value)
            {
                model.DnaRegions[k] = new DnaRegion
                {
                    Id = GetDnaRegionId(mapEntry, arm),
                    Chromosome = chromosome,
                    Arm = arm
                };

                k++;
            }
        }

        for (int i = 0; i < sampleIds.Length; i++)
        {
            var sampleId = sampleIds[i];
            
            foreach (var mapEntry in _chromosomeArmMap)
            {
                foreach (var chromosomeArm in mapEntry.Value)
                {
                    var cnvProfile = cnvProfiles.FirstOrDefault(x => x.SampleId == sampleId && x.ChromosomeId == mapEntry.Key && x.ChromosomeArmId == chromosomeArm);
                    var observationEvent = GetEvent(cnvProfile, options.EventThreshold);

                    if (observationEvent != Event.Neutral)
                    {
                        var dnaRegionId = GetDnaRegionId(mapEntry, chromosomeArm);
                        model.Observations.Add(new Observation{ DnaRegionId = dnaRegionId, SampleId = sampleId, Event = observationEvent });
                    }
                }
            }

            var sample = context.OmicsSamples[sampleId];
            var specimen = sample.Specimen;

            var tumorFamily = specimen.TumorClassification?.Family?.Name;
            if (String.IsNullOrWhiteSpace(tumorFamily))
            {
                tumorFamily = "Undefined";
            }
            
            model.Samples[i] = new Sample
            {
                Id = sampleId, 
                TumorType = tumorFamily,
                DonorId = specimen.DonorId
            };
        }

        return model;
    }

    private static string GetDnaRegionId(KeyValuePair<Chromosome, ChromosomeArm[]> mapEntry, ChromosomeArm arm)
    {
        return mapEntry.Key.ToDefinitionString() + arm.ToDefinitionString();
    }

    private Event GetEvent(Profile cnvProfile, double eventThreshold = 0.8)
    {
        if (cnvProfile != null)
        {
            if(cnvProfile.Gain > eventThreshold)
                return Event.Gain;
        
            if(cnvProfile.Loss > eventThreshold)
                return Event.Loss;
        }

        return Event.Neutral;
    }

    private int GetArmsCount()
    {
        return _chromosomeArmMap.Values.Sum(arms => arms.Length);
    }
}