namespace Unite.Analysis.Services.KMeier;

public class AnalysisContext
{
    /// <summary>
    /// Type of the sample: 1 - donor, 2 - image, 3 - specimen.
    /// </summary>
    public byte SampleType { get; }

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


    public AnalysisContext(byte sampleType)
    {
        SampleType = sampleType;

        Donors = [];
        Images = [];
        Specimens = [];
    }


    public string GetSampleKey(int id)
    {
        if (SampleType == 1)
        {
            var donor = Donors[id];
            return $"{donor.ReferenceId}";
        }
        else if (SampleType == 2)
        {
            var donor = Donors[id];
            var image = Images.Values.First(image => image.DonorId == donor.Id);
            return $"{donor.ReferenceId}_{image.ReferenceId}";
        }
        else if (SampleType == 3)
        {
            var donor = Donors[id];
            var specimen = Specimens.Values.First(specimen => specimen.DonorId == donor.Id);
            return $"{donor.ReferenceId}_{specimen.ReferenceId}";
        }
        else
        {
            throw new InvalidOperationException("Invalid sample type.");
        }
    }
}
