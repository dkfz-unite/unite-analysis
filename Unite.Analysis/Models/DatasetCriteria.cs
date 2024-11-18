using Unite.Analysis.Models.Enums;
using Unite.Indices.Search.Services.Filters.Criteria;

namespace Unite.Analysis.Models;

public record DatasetCriteria
{
    /// <summary>
    /// Dataset key. Identifies the dataset in the analysis data.
    /// </summary>
    public string Key { get; set; }

    /// <summary>
    /// Dataset name. The name of the dataset in the analysis data.
    /// </summary>
    public string Name { get; set; }

    /// <summary>
    /// Dataset order. The order of the dataset in the analysis data.
    /// </summary>
    public int Order { get; set; }

    /// <summary>
    /// Dataset type. Used to find dataset data.
    /// </summary> 
    public DatasetDomain Domain { get; set; }

    /// <summary>
    /// Dataset criteria. Used to filter dataset data.
    /// </summary>
    public SearchCriteria Criteria { get; set; }
}
