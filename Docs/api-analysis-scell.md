# Single Cell RNA Dataset Creation Analysis
This analysis combines single cell (or single nucleus) RNA expression data of the samples of one dataset into one [AnnData](https://anndata.readthedocs.io/en/latest/) dataset and optionally processes it (quality control, preprocessing, PCA, neighbors, clustering, embeddings, cell type annotation).  
Analysis data is processed by the [SCELL analysis service](https://github.com/dkfz-unite/unite-analysis-scell).

Endpoint: `POST api/analysis/scell`

The resulting dataset can be viewed with the [single cell dataset viewer](./api.md#post-apiviewerscellidid).


## Model
[Analysis](./api-model-analysis.md) with the following `data`.

**`*`** - Required fields

**`datasets`*** - Datasets to analyse
- Type: _Array_
- Element type: [Dataset](./api-model-dataset.md)
- Limitations: Exactly one dataset.
- Example: `[{...}]`

**`annotations`** - Custom cell annotations.
- Note: Content of a TSV file. If set, it is written to `annotations.tsv` and the `meta` option is set to `true`.
- Type: _String_
- Example: `"..."`

**`options`** - Analysis options
- Type: _Object([Options](#analysis-options))_
- Example: `{...}`

### Analysis Options
Options defining which steps of the analysis should be performed.

**`meta`** - Use custom cell annotations.
- Note: Set by the service if `annotations` are provided.
- Type: _Boolean_
- Default: `false`

**`model`** - Cell type annotation model.
- Note: Name of a [CellTypist](https://www.celltypist.org) model, see [api/analysis/scell/models](./api.md#get-apianalysisscellmodels).
- Type: _String_
- Default: `null`

**`qc`** - Calculate quality control metrics.
- Type: _Boolean_
- Default: `false`

**`sparse`** - Make the data sparse.
- Note: It's highly recommended to make the data sparse as this significantly reduces the size of the data and calculation speed, but almost has no effect on the results.
- Type: _Boolean_
- Default: `true`

**`pp`** - Preprocessing method.
- Type: _String_
- Possible values: `"default"`, `"seurat"`, `"zheng17"`
- Default: `"default"`

**`genes`** - Minimum number of genes expressed in a cell.
- Type: _Number_
- Default: `5`

**`cells`** - Minimum number of cells where a gene is expressed.
- Type: _Number_
- Default: `25`

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
    "name": "Glioma single cell dataset",
    "data": {
        "datasets": [
            {
                "id": "8b4e6a2c-2b4b-4e3e-9c6d-5a4f1d3b9e0f",
                "name": "Glioma",
                "order": 1,
                "domain": "donors",
                "criteria": {
                    "donor": { "project": { "value": ["Project 1"] } }
                }
            }
        ],
        "options": {
            "qc": true,
            "sparse": true,
            "pp": "default",
            "genes": 5,
            "cells": 25,
            "pca": true,
            "neighbors": true,
            "clustering": "louvain",
            "embedding": ["umap"]
        }
    }
}
```


## Dataset Requirements
- Samples of single cell or single nucleus RNA sequencing analysis with single cell expression resources `barcodes.tsv.gz`, `features.tsv.gz` (TSV, gzip) and `matrix.mtx.gz` (MTX, gzip).
- Samples without all three files are skipped. If no sample has them, the analysis fails.


## Input
The service downloads the resources of each sample to a sample subdirectory of the working directory and writes:
- `metadata.tsv` - [sample metadata](./api-model-sample-metadata.md).
- `annotations.tsv` - custom cell annotations (only if provided).
- `options.json` - analysis options.


## Results Metadata
`GET api/analysis/{id}/meta` returns `result.meta.json` written by the runner (the `file` parameter is ignored).


## Results Data
`GET api/analysis/{id}/data` returns `output.zip` with:
- `result.data.h5ad` - dataset in [AnnData](https://anndata.readthedocs.io/en/latest/) format. It can be used for further analysis and visualisation, e.g. with [CELLxGENE](https://cellxgene.cziscience.com).
- `result.meta.json` - result metadata.

After processing, the downloaded sample resources are deleted.
