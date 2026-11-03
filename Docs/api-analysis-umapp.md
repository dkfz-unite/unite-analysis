# Protein Expression UMAP Analysis
This analysis builds a UMAP embedding of protein expression (mass spectrometry) profiles of the samples of one dataset.  
Analysis data is processed by the [UMAPP analysis service](https://github.com/dkfz-unite/unite-analysis-umapp).

Endpoint: `POST api/analysis/umapp`


## Model
[Analysis](./api-model-analysis.md) with the following `data`.

**`*`** - Required fields

**`datasets`*** - Datasets to analyse
- Type: _Array_
- Element type: [Dataset](./api-model-dataset.md)
- Limitations: Exactly one dataset.
- Example: `[{...}]`

**`options`** - Analysis options
- Type: _Object([Options](#analysis-options))_
- Example: `{...}`

### Analysis Options
Preprocessing options of [DEP analysis](./api-analysis-dep.md#analysis-options): `normalization_log_offset`, `normalization_method`, `imputation_method`, `stratify_imputation_by_batch`, `batch_correction_method`, `min_non_missing_fraction`, `require_min_fraction_one_class` (with the same defaults), and the following options.

**`class_property`** - [Sample metadata](./api-model-sample-metadata.md#columns) property used as the sample class (condition).
- Note: Used only if `require_min_fraction_one_class` is `true`; otherwise the condition is empty. Property keys are available at [api/metadata/options](./api.md#get-apimetadataoptions).
- Type: _String_
- Default: `null`
- Example: `"specimen_tumor_type"`

**`feature_selection_method`** - Feature selection method.
- Type: _String_
- Possible values: `"variance"`
- Default: `"variance"`

**`feature_selection_n_features`** - Number of features to select.
- Type: _Number_
- Default: `1000`

**`umap_n_neighbors`** - UMAP number of neighbors.
- Type: _Number_
- Default: `15`

**`umap_metric`** - UMAP distance metric.
- Type: _String_
- Possible values: `"euclidean"`, `"manhattan"`
- Default: `"euclidean"`

**`umap_random_state`** - UMAP random state.
- Type: _Number_
- Default: `null`

**`umap_min_dist`** - UMAP minimum distance.
- Type: _Number_
- Default: `0.1`

**`umap_n_principal_components`** - Number of principal components.
- Type: _Number_
- Default: `50`

### Example
```jsonc
{
    "name": "Glioma proteome UMAP",
    "data": {
        "datasets": [
            {
                "id": "8b4e6a2c-2b4b-4e3e-9c6d-5a4f1d3b9e0f",
                "name": "Glioma",
                "order": 1,
                "domain": "materials",
                "criteria": {...}
            }
        ],
        "options": {
            "class_property": "specimen_tumor_type",
            "feature_selection_method": "variance",
            "feature_selection_n_features": 1000,
            "umap_n_neighbors": 15,
            "umap_metric": "euclidean",
            "umap_min_dist": 0.1,
            "umap_n_principal_components": 50
        }
    }
}
```


## Dataset Requirements
- Samples of mass spectrometry analysis with protein expression data. Samples without expression values are skipped.
- If `batch_correction_method` is set and only some samples have a batch, a warning is produced during preparation (it is not stored with the analysis).


## Input
The service writes the following files to the working directory:
- `data.tsv` - raw protein expression values: `feature` (Ensembl protein id) and one column per sample (internal sample id).
- `metadata.tsv` - `sample` (internal sample id), `condition` (value of `class_property`), `batch`, `donor` (donor id), `specimen` (specimen id), `specimen_type`.
- `annotations.tsv` - [sample metadata](./api-model-sample-metadata.md) with `sample_key` (internal sample id).
- `options.json` - analysis options.

The runner and its output format are not part of this repository.


## Results Metadata
`GET api/analysis/{id}/meta?file={file}` returns a file of the working directory:
- no `file` - `results.tsv` written by the runner.
- `file` - the file with the given name, e.g. `annotations.tsv` (used by the portal), `metadata.tsv`, `data.tsv` or `options.json`.


## Results Data
`GET api/analysis/{id}/data` returns `results.zip` with:
- `data.tsv`, `metadata.tsv`, `annotations.tsv`, `options.json` - [input](#input) files.
- `results.tsv` - runner results.
