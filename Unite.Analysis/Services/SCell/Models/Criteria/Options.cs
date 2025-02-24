using System.Text.Json.Serialization;
using Unite.Analysis.Services.SCell.Models.Criteria.Enums;

namespace Unite.Analysis.Services.SCell.Models.Criteria;

public class Options
{
    /// <summary>
    /// Custom annotations.
    /// </summary>
    [JsonPropertyName("meta")]
    public bool Meta { get; set; } = false;

    /// <summary>
    /// Cell types annotation model.
    /// </summary>
    [JsonPropertyName("model")]
    public string Model { get; set; } = null;

    /// <summary>
    /// Calculate quality control metrics.
    /// </summary>
    [JsonPropertyName("qc")]
    public bool QC { get; set; } = false;

    /// <summary>
    /// Make data sparse.
    /// </summary> 
    [JsonPropertyName("sparse")]
    public bool Sparse { get; set; } = true;

    /// <summary>
    /// Pre processing type.
    /// </summary>
    [JsonPropertyName("pp")]
    public PreprocessingType PP { get; set; } = PreprocessingType.Default;

    /// <summary>
    /// Minimum number of genes to be expressed in a cell.
    /// </summary>
    [JsonPropertyName("genes")]
    public int Genes { get; set; } = 5;

    /// <summary>
    /// Minimum number of cells where gene is expressed.
    /// </summary>
    [JsonPropertyName("cells")]
    public int Cells { get; set; } = 25;

    /// <summary>
    /// Perform principal component analysis.
    /// </summary>
    [JsonPropertyName("pca")]
    public bool PCA { get; set; } = true;

    /// <summary>
    /// Calculate t-distributed stochastic neighbor embedding.
    /// </summary>
    [JsonPropertyName("neighbors")]
    public bool Neighbors { get; set; } = true;

    /// <summary>
    /// Perform clustering.
    /// </summary>
    [JsonPropertyName("clustering")]
    public ClusteringType Clustering { get; set; } = ClusteringType.Louvain;

    /// <summary>
    /// Calculate different types of embeddings.
    /// </summary>
    [JsonPropertyName("embedding")]
    public EmbeddingType[] Embedding { get; set; } = [EmbeddingType.Umap];
}
