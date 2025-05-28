using System.IO.Compression;
using Unite.Analysis.Services.Dm.Models.Data;
using Unite.Essentials.Extensions;
using Unite.Essentials.Tsv;

namespace Unite.Analysis.Services.Dm;

public class OutputWriter
{
    public const string ResultDataFileName = "results.csv.gz";
    public const string ReducedResultDataFileName = "results_reduced.csv.gz";
    public const string AnnotationDataFileName = "results_annotated.tsv";
    public const string ReducedPoints = "reduced_points.tsv";
    public const string CompressedReducedPoints = "reduced_points.tsv.gz";
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

        await Task.CompletedTask;
    }

    public static async Task ArchiveOutput(string path)
    {
        using var archiveStream = new FileStream(Path.Combine(path, ArchiveFileName), FileMode.CreateNew);
        using var archive = new ZipArchive(archiveStream, ZipArchiveMode.Create, false);

        archive.CreateEntryFromFile(Path.Combine(path, ResultDataFileName), ResultDataFileName);
        archive.CreateEntryFromFile(Path.Combine(path, ReducedResultDataFileName), ReducedResultDataFileName);
        archive.CreateEntryFromFile(Path.Combine(path, AnnotationDataFileName), AnnotationDataFileName);
        archive.CreateEntryFromFile(Path.Combine(path, ReducedPoints), ReducedPoints);
        await Task.CompletedTask;
    }

    public static async Task ReducePoints(string path, string resultFileName)
    {
        var filePath = Path.Join(path, resultFileName);
        var tsvRaw = await File.ReadAllTextAsync(filePath);
        var mapRaw = new ClassMap<Resultdata>()
                .Map(x => x.Log2FoldChange, "logFC")
                .Map(x => x.PValueAdjusted, "adj.P.Val")
                .Map(x => x.CpgId, "CpgId")
                .Map(x => x.RegulatoryFeatureName, "Regulatory_Feature_Name")
                .Map(x => x.Phantom4Enhancers, "Phantom4_Enhancers")
                .Map(x => x.Phantom5Enhancers, "Phantom5_Enhancers")
                .Map(x => x.UcscRefGeneName, "UCSC_RefGene_Name");
                
        var dataRaw = TsvReader.Read(tsvRaw, mapRaw);
        var timer = System.Diagnostics.Stopwatch.StartNew();

        int logFcGrid = 256, adjPValGrid = 256;
        var maxLogFc = dataRaw.Max(x => x.Log2FoldChange);
        var minLogFc = dataRaw.Min(x => x.Log2FoldChange);
        var maxAdjPVal = dataRaw.Max(x => -Math.Log10(x.PValueAdjusted));
        var minAdjPVal = dataRaw.Min(x => -Math.Log10(x.PValueAdjusted));

        double xStep = (maxLogFc - minLogFc) / logFcGrid;
        double yStep = (maxAdjPVal - minAdjPVal) / adjPValGrid;

        var grid = new Dictionary<(int, int), GridCell>();

        foreach (var data in dataRaw)
        {
            double x = data.Log2FoldChange;
            double y = -Math.Log10(data.PValueAdjusted);

            int xi = (int)((x - minLogFc) / xStep);
            int yi = (int)((y - minAdjPVal) / yStep);

            xi = Math.Min(xi, logFcGrid - 1);
            yi = Math.Min(yi, adjPValGrid - 1);

            var key = (xi, yi);
            if (!grid.TryGetValue(key, out var cell))
            {
                cell = new GridCell();
                grid[key] = cell;
            }
            cell.Count++;
            cell.Log2FoldChange += data.Log2FoldChange;
            cell.PValueAdjusted += data.PValueAdjusted;
            cell.CpgId = cell.CpgId ?? data.CpgId;
            cell.Regulatory_Feature_Name = cell.Regulatory_Feature_Name ?? data.RegulatoryFeatureName;
            cell.Phantom4_Enhancer =  cell.Phantom4_Enhancer ?? data.Phantom4Enhancers ;
            cell.Phantom5_Enhancer = cell.Phantom5_Enhancer ?? data.Phantom5Enhancers;
            cell.UCSC_RefGene_Name = cell.UCSC_RefGene_Name ?? data.UcscRefGeneName;
        }
        timer.Stop();
        Console.WriteLine(timer.Elapsed);
        await WriteMatrix(grid, path);
    }

    public static async Task WriteMatrix(Dictionary<(int, int), GridCell> grid, string path)
    {
        string filePath = Path.Combine(path, ReducedPoints);

        var entries = Load(grid);
        var map = ResultMapper.Map(entries);
        var tsv = TsvWriter.Write(entries, map);
        await File.WriteAllTextAsync(filePath, tsv);
    }

    public static Resultdata[] Load(Dictionary<(int, int), GridCell> grid)
    {
        List<Resultdata> resultdataList = new List<Resultdata>();

        foreach (var gridData in grid)
        {
            var resultData = new Resultdata();
            var cell = gridData.Value;
            if (cell.Count > 0)
            {
                resultData.Log2FoldChange = cell.Log2FoldChange / cell.Count;
                resultData.PValueAdjusted = cell.PValueAdjusted / cell.Count;
                resultData.Count = cell.Count;
                resultData.CpgId = cell.CpgId;
                resultData.RegulatoryFeatureName = cell.Regulatory_Feature_Name;
                resultData.Phantom4Enhancers = cell.Phantom4_Enhancer;
                resultData.Phantom5Enhancers = cell.Phantom5_Enhancer;
                resultData.UcscRefGeneName = cell.UCSC_RefGene_Name;
                resultdataList.Add(resultData);
            }
        }
        return resultdataList.ToArrayOrNull();
    }
}