using Unite.Analysis.Services.KMeier.Models;
using Unite.Essentials.Tsv;

namespace Unite.Analysis.Services.KMeier;

public static class MetaLoader
{
    public static async Task PrepareMetadata(AnalysisContext context, string workingDirectoryPath)
    {
        var entries = Load(context).Filter();

        Validate(entries);

        var map = MetaMapper.Map(entries);

        var tsv = TsvWriter.Write(entries, map);

        File.WriteAllText(Path.Combine(workingDirectoryPath, "input.tsv"), tsv);

        await Task.CompletedTask;
    }


    private static Metadata[] Load(AnalysisContext context)
    {
        var metadataEntries = new List<Metadata>();

        foreach(var donor in context.Donors)
        {
            var metadataEntry = new Metadata
            {
                Id = context.GetSampleKey(donor.Key),
                DiagnisosDate = donor.Value.ClinicalData?.DiagnosisDate,
                VitalStatus = donor.Value.ClinicalData?.VitalStatus,
                VitalStatusChangeDate = donor.Value.ClinicalData?.VitalStatusChangeDate,
                VitalStatusChangeDay = donor.Value.ClinicalData?.VitalStatusChangeDay
            };

            metadataEntries.Add(metadataEntry);
        }

        return metadataEntries.ToArray();
    }

    private static Metadata[] Filter(this Metadata[] entries)
    {
        return entries.Where(entry => {
            
            if (entry.DiagnisosDate == null)
                return false;

            if (entry.VitalStatus == null)
                return false;

            if (entry.VitalStatusChangeDate == null && entry.VitalStatusChangeDay == null)
                return false;

            return true;

        }).ToArray();
    }

    private static void Validate (Metadata[] entries)
    {
        if (entries.Length < 2)
            throw new InvalidOperationException("The dataset requires at least two unique donors with survival time data.");
    }
}
