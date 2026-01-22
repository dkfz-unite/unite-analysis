using Unite.Data.Entities.Donors;
using Unite.Data.Entities.Images;
using Unite.Data.Entities.Specimens;
using Unite.Data.Entities.Specimens.Enums;

namespace Unite.Analysis.Services;

public class SamplesContext
{
    /// <summary>
    /// Type of the sample: 1 - donor, 2 - image, 3 - specimen.
    /// </summary>
    public byte SampleType { get; set; }

    /// <summary>
    /// Donors cached per donor identifier.
    /// </summary>
    public Dictionary<int, Donor> Donors { get; set; }

    /// <summary>
    /// Images cached per image identifier.
    /// </summary> 
    public Dictionary<int, Image> Images { get; set; }

    /// <summary>
    /// Specimens cached per specimen identifier.
    /// </summary>
    public Dictionary<int, Specimen> Specimens { get; set; }

    /// <summary>
    /// Omics samples cached per sample identifier.
    /// </summary>
    public Dictionary<int, Data.Entities.Omics.Analysis.Sample> OmicsSamples { get; set; }


    public SamplesContext(byte sampleType)
    {
        SampleType = sampleType;

        Donors = [];
        Images = [];
        Specimens = [];
        OmicsSamples = [];
    }


    public string GetSampleKey(int id)
    {
        var donor = GetSampleDonor(id);
        var specimen = GetSampleSpecimen(id);

        if (SampleType == 1)
            return $"{donor.ReferenceId}";
        else
            return $"{donor.ReferenceId}-{specimen.ReferenceId}";
    }

    public Donor GetSampleDonor(int id)
    {
        var sample = OmicsSamples[id];
        var specimen = Specimens[sample.SpecimenId];
        var donor = Donors[specimen.DonorId];

        return donor;
    }

    public Image GetSampleImage(int id)
    {
        var sample = OmicsSamples[id];
        var specimen = Specimens[sample.SpecimenId];
        var image = Images.Values.FirstOrDefault(image => image.DonorId == specimen.DonorId);

        return image;
    }

    public Specimen GetSampleSpecimen(int id)
    {
        var sample = OmicsSamples[id];
        var specimen = Specimens[sample.SpecimenId];

        return specimen;
    }

    public void RemoveSample(params int[] ids)
    {
        foreach (var id in ids)
        {
            OmicsSamples.Remove(id);
        }
    }

    public int GetSampleRelevance(int id)
    {
        var sample = OmicsSamples[id];
        var specimen = Specimens[sample.SpecimenId];

        var score = (specimen.TypeId == SpecimenType.Material && specimen.TumorTypeId == TumorType.Primary) ? 1 :
                    (specimen.TypeId == SpecimenType.Material && specimen.TumorTypeId == TumorType.Recurrent) ? 2 :
                    (specimen.TypeId == SpecimenType.Material && specimen.TumorTypeId == TumorType.Metastasis) ? 3 :
                    (specimen.TypeId == SpecimenType.Material && specimen.ConditionId == Condition.Tumor) ? 4 :
                    (specimen.TypeId == SpecimenType.Material && specimen.ConditionId == Condition.Normal) ? 5 :
                    specimen.TypeId == SpecimenType.Material ? 6 :
                    specimen.TypeId == SpecimenType.Line ? 10 :
                    specimen.TypeId == SpecimenType.Organoid ? 20 :
                    specimen.TypeId == SpecimenType.Xenograft ? 30 : 40;

        var multiplier = sample.MatchedSampleId != null ? 1 : 2;

        return score * multiplier;
    }
}
