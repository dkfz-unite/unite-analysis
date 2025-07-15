using System.IO.Compression;
using Unite.Analysis.Helpers;
using Unite.Analysis.Services.Dm.Models.Data;
using Unite.Essentials.Tsv;

namespace Unite.Analysis.Services.Dm;

public class OutputWriter
{
    public const string ResultsFileArchiveName = "results.tsv.gz"; // Not used, left for reference.
    public const string ResultsFileName = "results.tsv";
    public const string ResultsHeatmapFileArchiveName = "results_heatmap.tsv.gz";
    public const string ResultsHeatmapFileName = "results_heatmap.tsv"; // Not used, left for reference.
    public const string ArchiveFileName = "output.zip";


    public static async Task ProcessOutput(string path)
    {
        if (Directory.Exists(path))
        {
            foreach (var subPath in Directory.GetDirectories(path))
            {
                Directory.Delete(subPath, true);
            }
        }

        await BinResults(path);
    }

    public static async Task ArchiveOutput(string path)
    {
        using var archiveStream = new FileStream(Path.Combine(path, ArchiveFileName), FileMode.CreateNew);
        using var archive = new ZipArchive(archiveStream, ZipArchiveMode.Create, false);

        archive.CreateEntryFromFile(Path.Combine(path, ResultsFileName), ResultsFileName);

        await Task.CompletedTask;
    }


    private static async Task BinResults(string path)
    {
        var records = await ReadResults(path, ResultsFileName);

        var bins = BinResultRecords(records);

        await WriteResultsHeatmap(path, ResultsHeatmapFileArchiveName, bins);

    }

    private static async Task<ResultRecord[]> ReadResults(string path, string fileName)
    {
        var filePath = Path.Combine(path, ResultsFileName);
        var tsv = await File.ReadAllTextAsync(Path.Combine(filePath));
        var map = new ClassMap<ResultRecord>().AutoMap();
        return TsvReader.Read(tsv, map).ToArray();
    }

    private static Dictionary<(int, int), ResultRecordBin> BinResultRecords(ResultRecord[] records, int logFcBinsCount = 256, int adjPValBinsCount = 256)
    {
        var maxLogFc = records.Max(x => x.Log2FoldChange);
        var minLogFc = records.Min(x => x.Log2FoldChange);
        var maxAdjPVal = records.Max(x => -Math.Log10(x.PValueAdjusted));
        var minAdjPVal = records.Min(x => -Math.Log10(x.PValueAdjusted));

        var xStep = (maxLogFc - minLogFc) / logFcBinsCount;
        var yStep = (maxAdjPVal - minAdjPVal) / adjPValBinsCount;

        var grid = new Dictionary<(int, int), ResultRecordBin>();

        foreach (var data in records)
        {
            var x = data.Log2FoldChange;
            var y = -Math.Log10(data.PValueAdjusted);
            var xi = (int)((x - minLogFc) / xStep);
            var yi = (int)((y - minAdjPVal) / yStep);

            var xIndex = Math.Min(xi, logFcBinsCount - 1);
            var yIndex = Math.Min(yi, adjPValBinsCount - 1);

            var key = (xIndex, yIndex);

            if (!grid.TryGetValue(key, out var cell))
            {
                cell = new ResultRecordBin
                {
                    CpgId = data.CpgId,
                    RegulatoryFeatureName = data.RegulatoryFeatureName,
                    Phantom4Enhancers = data.Phantom4Enhancers,
                    Phantom5Enhancers = data.Phantom5Enhancers,
                    UcscRefGeneName = data.UcscRefGeneName
                };

                grid[key] = cell;
            }

            cell.Count++;
            cell.Log2FoldChange += (data.Log2FoldChange - cell.Log2FoldChange) / cell.Count;
            cell.PValueAdjusted += (data.PValueAdjusted - cell.PValueAdjusted) / cell.Count;
        }

        return grid;
    }

    public static async Task WriteResultsHeatmap(string path, string fileName, Dictionary<(int, int), ResultRecordBin> grid)
    {
        string filePath = Path.Combine(path, fileName);

        var entries = grid.Values.ToArray();
        var map = new ClassMap<ResultRecordBin>().AutoMap();
        var tsv = TsvWriter.Write(entries, map);

        using var compressedFileStream = new FileStream(filePath, FileMode.Create, FileAccess.Write);
        using var compressionStream = new GZipStream(compressedFileStream, CompressionMode.Compress);
        using var writer = new StreamWriter(compressionStream);

        await writer.WriteAsync(tsv);
    }
}
