using Unite.Analysis.Services.CnvProfile.Models.Output;
using Unite.Data.Context.Repositories;
using Unite.Data.Entities.Omics.Analysis.Dna.Cnv;
using Unite.Data.Entities.Omics.Enums;
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
    
    public async Task<SampleRecords> ProcessData(SamplesContext context, Models.Criteria.Options options)
    {
        var sampleIds = context.OmicsSamples.Keys.ToArray();
        var cnvProfiles = await _cnvProfilesRepository.GetRelatedProfiles(sampleIds);

        var records = new SampleRecords(sampleIds.Length);
        var armsCount = GetArmsCount();
        
        foreach (var sampleId in sampleIds)
        {
            var record = new SampleRecord(armsCount);

            int i = 0;
            foreach (var mapEntry in _chromosomeArmMap)
            {
                foreach (var chromosomeArm in mapEntry.Value)
                {
                    var cnvProfile = cnvProfiles.FirstOrDefault(x => x.SampleId == sampleId && x.ChromosomeId == mapEntry.Key && x.ChromosomeArmId == chromosomeArm);
                    record.Events[i] = GetEvent(cnvProfile);
                    ++i;
                }
            }
        }

        return records;
    }

    private Event GetEvent(Profile cnvProfile)
    {
        if (cnvProfile != null)
        {
            if(cnvProfile.Gain > 0.8)
                return Event.Gain;
        
            if(cnvProfile.Loss > 0.8)
                return Event.Loss;
        }

        return Event.Neutral;
    }

    private int GetArmsCount()
    {
        return _chromosomeArmMap.Values.Sum(arms => arms.Length);
    }
}