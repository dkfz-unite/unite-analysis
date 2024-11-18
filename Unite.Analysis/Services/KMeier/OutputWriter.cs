using System.IO.Compression;
using System.Text.Json;
using Unite.Analysis.Services.KMeier.Models.Output;
using Unite.Essentials.Tsv;

namespace Unite.Analysis.Services.KMeier;

public class OutputWriter
{
    public const string ResultFileName = "result.tsv";
    public const string RankFileName = "logrank_test.tsv";
    public const string CensoredFileName = "censored.tsv";

    public const string OutputFileName = "output.json";
    public const string ArchiveFileName = "output.zip";


    public static async Task ProcessOutput(string path)
    {
        var rankTsv = await File.ReadAllTextAsync(Path.Combine(path, RankFileName));
        var rankRecords = TsvReader.Read<RankRecord>(rankTsv).ToArray();

        var resultTsv = await File.ReadAllTextAsync(Path.Combine(path, ResultFileName));
        var resultRecords = TsvReader.Read<ResultRecord>(resultTsv).ToArray();

        var rank = new Rank
        {
            Chi2 = rankRecords[0].Chi2,
            P = rankRecords[0].P
        };

        var curves = resultRecords.GroupBy(x => x.DatasetId).ToDictionary
        (
            group => group.Key,
            group => new Curve
            {
                Time = group.Select(record => record.Time).ToArray(),
                SurvivalProb = group.Select(record => record.SurvivalProb).ToArray(),
                ConfIntLower = group.Select(record => record.ConfIntLower).ToArray(),
                ConfIntUpper = group.Select(record => record.ConfIntUpper).ToArray()
            }
        );

        var result = new Output
        {
            Rank = rank,
            Curves = curves
        };

        var outputJson = JsonSerializer.Serialize(result);
        await File.WriteAllTextAsync(Path.Combine(path, OutputFileName), outputJson);
    }

    public static async Task ArchiveOutput(string path)
    {
        using var archiveStream = new FileStream(Path.Combine(path, ArchiveFileName), FileMode.CreateNew);
        using var archive = new ZipArchive(archiveStream, ZipArchiveMode.Create, false);

        archive.CreateEntryFromFile(Path.Combine(path, ResultFileName), ResultFileName);
        archive.CreateEntryFromFile(Path.Combine(path, RankFileName), RankFileName);
        archive.CreateEntryFromFile(Path.Combine(path, CensoredFileName), CensoredFileName);

        File.Delete(Path.Combine(path, ResultFileName));
        File.Delete(Path.Combine(path, RankFileName));
        File.Delete(Path.Combine(path, CensoredFileName));

        await Task.CompletedTask;
    }
}
