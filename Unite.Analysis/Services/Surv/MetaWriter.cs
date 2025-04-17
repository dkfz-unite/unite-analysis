using Unite.Analysis.Services.Surv.Models.Context;
using Unite.Analysis.Services.Surv.Models.Criteria;
using Unite.Essentials.Tsv;

namespace Unite.Analysis.Services.Surv;

public static class MetaWriter
{
    public const string InputFileName = "input.tsv";

    public static async Task Write(AnalysisContext analysisContext, Options analysisOptions, string directoryPath)
    {
        var records = MetaLoader.Load(analysisContext, analysisOptions);
        var recordsMap = MetaMapper.Map(records);

        var filePath = Path.Combine(directoryPath, InputFileName);
        var fileContent = TsvWriter.Write(records, recordsMap);

        await File.WriteAllTextAsync(filePath, fileContent);
    }
}
