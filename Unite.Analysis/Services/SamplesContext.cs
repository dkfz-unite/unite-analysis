using Unite.Data.Entities.Donors;
using Unite.Data.Entities.Images;
using Unite.Data.Entities.Specimens;
using Unite.Data.Entities.Specimens.Enums;
using Unite.Data.Entities.Specimens.Materials.Enums;

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
        var image = Images[specimen.DonorId];

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

        return (specimen.TypeId == SpecimenType.Material && specimen.Material.TumorTypeId == TumorType.Primary) ? 1 :
               (specimen.TypeId == SpecimenType.Material && specimen.Material.TumorTypeId == TumorType.Recurrent) ? 2 :
               (specimen.TypeId == SpecimenType.Material && specimen.Material.TumorTypeId == TumorType.Metastasis) ? 3 :
               (specimen.TypeId == SpecimenType.Material && specimen.Material.TypeId == MaterialType.Tumor) ? 4 :
               (specimen.TypeId == SpecimenType.Material && specimen.Material.TypeId == MaterialType.Normal) ? 5 :
               specimen.TypeId == SpecimenType.Material ? 6 :
               specimen.TypeId == SpecimenType.Line ? 10 :
               specimen.TypeId == SpecimenType.Organoid ? 20 :
               specimen.TypeId == SpecimenType.Xenograft ? 30 : 40;
    }
}
