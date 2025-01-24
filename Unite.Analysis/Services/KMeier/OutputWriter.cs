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
        var result = new Output();

        var resultPath = Path.Combine(path, ResultFileName);
        var hasResult = File.Exists(resultPath);

        if (hasResult)
        {
            var resultTsv = await File.ReadAllTextAsync(resultPath);
            var resultRecords = TsvReader.Read<ResultRecord>(resultTsv).ToArray();
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

            result.Curves = curves;
        }


        var rankPath = Path.Combine(path, RankFileName);
        var hasRank = File.Exists(rankPath);

        if (hasRank)
        {
            var rankTsv = await File.ReadAllTextAsync(rankPath);
            var rankRecords = TsvReader.Read<RankRecord>(rankTsv).ToArray();
            var rank = new Rank
            {
                Chi2 = rankRecords[0].Chi2,
                P = rankRecords[0].P
            };

            result.Rank = rank;
        }


        var outputJson = JsonSerializer.Serialize(result);

        await File.WriteAllTextAsync(Path.Combine(path, OutputFileName), outputJson);
    }

    public static async Task ArchiveOutput(string path)
    {
        using var archiveStream = new FileStream(Path.Combine(path, ArchiveFileName), FileMode.CreateNew);
        using var archive = new ZipArchive(archiveStream, ZipArchiveMode.Create, false);

        var resultPath = Path.Combine(path, ResultFileName);
        if (File.Exists(resultPath))
        {
            archive.CreateEntryFromFile(resultPath, ResultFileName);
            File.Delete(resultPath);
        }

        var rankPath = Path.Combine(path, RankFileName);
        if (File.Exists(rankPath))
        {
            archive.CreateEntryFromFile(rankPath, RankFileName);
            File.Delete(rankPath);
        }

        var censoredPath = Path.Combine(path, CensoredFileName);
        if (File.Exists(censoredPath))
        {
            archive.CreateEntryFromFile(censoredPath, CensoredFileName);
            File.Delete(censoredPath);
        }

        await Task.CompletedTask;
    }
}
