using System.Text.Json.Serialization;
using Unite.Analysis.Services.Rnasc.Models.Enums;

namespace Unite.Analysis.Services.Rnasc.Models;

public class Options
{
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
