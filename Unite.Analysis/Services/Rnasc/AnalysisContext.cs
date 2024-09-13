namespace Unite.Analysis.Services.Rnasc;

public class AnalysisContext
{
    /// <summary>
    /// Type of the sample: 1 - donor, 2 - image, 3 - specimen.
    /// </summary>
    public byte SampleType { get; set; }

    /// <summary>
    /// Donors cached per donor identifier.
    /// </summary>
    public Dictionary<int, Data.Entities.Donors.Donor> Donors { get; set; }

    /// <summary>
    /// Images cached per image identifier.
    /// </summary> 
    public Dictionary<int, Data.Entities.Images.Image> Images { get; set; }

    /// <summary>
    /// Specimens cached per specimen identifier.
    /// </summary>
    public Dictionary<int, Data.Entities.Specimens.Specimen> Specimens { get; set; }

    /// <summary>
    /// Samples cached per sample identifier.
    /// </summary>
    public Dictionary<int, Data.Entities.Genome.Analysis.Sample> Samples { get; set; }


    public AnalysisContext(byte sampleType)
    {
        SampleType = sampleType;

        Donors = [];
        Images = [];
        Specimens = [];
        Samples = [];
    }


    public string GetSampleKey(int id)
    {
        if (SampleType == 1)
        {
            var donor = GetSampleDonor(id);
            var specimen = GetSampleSpecimen(id);
            return $"{donor.ReferenceId}-{specimen.ReferenceId}";
        }
        else if (SampleType == 2)
        {
            var image = GetSampleImage(id);
            var specimen = GetSampleSpecimen(id);
            return $"{image.ReferenceId}-{specimen.ReferenceId}";
        }
        else if (SampleType == 3)
        {
            var specimen = GetSampleSpecimen(id);
            return $"{specimen.ReferenceId}";
        }
        else
        {
            throw new InvalidOperationException("Invalid sample type.");
        }
    }

    public Data.Entities.Donors.Donor GetSampleDonor(int id)
    {
        var sample = Samples[id];
        var specimen = Specimens[sample.SpecimenId];
        var donor = Donors[specimen.DonorId];

        return donor;
    }

    public Data.Entities.Images.Image GetSampleImage(int id)
    {
        var sample = Samples[id];
        var specimen = Specimens[sample.SpecimenId];
        var image = Images[specimen.DonorId];

        return image;
    }

    public Data.Entities.Specimens.Specimen GetSampleSpecimen(int id)
    {
        var sample = Samples[id];
        var specimen = Specimens[sample.SpecimenId];

        return specimen;
    }

    public Data.Entities.Donors.Donor GetDonor(Data.Entities.Genome.Analysis.Sample sample)
    {
        var specimen = Specimens[sample.SpecimenId];
        var donor = Donors[specimen.DonorId];

        return donor;
    }
}