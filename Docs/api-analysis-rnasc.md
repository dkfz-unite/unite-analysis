# Single Cell RNA Dataset Creation Analysis
This analysis uses available data to create a single cell RNA dataset for one or many combined samples.  
Analysis data is processed by the [single cell analysis service](https://github.com/dkfz-unite/unite-analysis-sc).


## Model
Includes information about the datasets to analyse and analysis options.

**`*`** - Required fields

**`datasets`*** - Datasets to analyse
- Type: _Array_
- Element type: [Dataset](./api-model-dataset.md)
- Limitations: For single cell RNA analysis, only one dataset is allowed.
- Example: `[{...}, {...}]`

**`options`*** - Analysis options
- Type: _Object([Options](#analysis-options))_
- Example: `{...}`

### Analysis Options
Options defining which steps of the analysis should be performed.

**`qc`** - Calculate quality control metrics.
- Type: _Boolean_
- Default: `true`

**`sparse`** - Make the data sparse.
- Note: It's highly recommended to make the data sparse as this significantly reduces the size of the data and calculation speed, but amost has no effect on the results.
- Type: _Boolean_
- Default: `true`

**`pp`** - Preprocessing method.
- Type: _String_
- Possible values: `"default"`, `"seurat"`, `"zheng17"`
- Default: `"default"`

**`pca`** - Perform principal component analysis.
- Note: It's recommended to perform PCA before calculating neighbors and clustering.
- Type: _Boolean_
- Default: `true`

**`neighbors`** - Calculate neighbors.
- Note: It's recommended to calculate neighbors before clustering.
- Type: _Boolean_
- Default: `true`

**`clustering`** - Clustering method.
- Type: _String_
- Possible values: `"louvain"`, `"leiden"`
- Default: `"louvain"`

**`embedding`** - Embedding methods.
- Type: _Array_
- Possible values: `"umap"`, `"tsne"`
- Default: `["umap"]`

### Example
```jsonc
{
    "datasets": [
        {
            "key": "8b4e6a2c-2b4b-4e3e-9c6d-5a4f1d3b9e0f",
            "order": 1,
            "domain": "Donors",
            "criteria": {
                "donor": { "project": "Project1" }
            }
        }
    ],
    "options": {
        "qc": true,
        "sparse": true,
        "pp": "default",
        "pca": true,
        "neighbors": true,
        "clustering": "louvain",
        "embedding": ["umap"],
    }
}
```


## Results Metadata
Results metadata is a file containing general statistics about the resulting dataset, such as:
- Number of cells
- Number of genes
- Cells type list
- Diagnoses list
- etc.


## Results Data
Analysis produces a single cell RNA dataset file in [AnnData](https://anndata.readthedocs.io/en/latest/) format.  
The file can be used fo further analysis and visualisation by the [CELLxGENE](https://cellxgene.cziscience.com) portal.
