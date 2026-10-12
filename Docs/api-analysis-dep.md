# Differential Protein Expression Analysis (limma)
This analysis compares protein expression (mass spectrometry) data of two datasets with limma.  
Analysis data is processed by the [DEP analysis service](https://github.com/dkfz-unite/unite-analysis-dep).

Endpoint: `POST api/analysis/dep`


## Model
[Analysis](./api-model-analysis.md) with the following `data`.

**`*`** - Required fields

**`datasets`*** - Datasets to analyse
- Type: _Array_
- Element type: [Dataset](./api-model-dataset.md)
- Limitations: Exactly two conditions (dataset names) with samples are required by the runner.
- Example: `[{...}, {...}]`

**`options`** - Analysis options
- Type: _Object([Options](#analysis-options))_
- Example: `{...}`

### Analysis Options
Preprocessing options. The same options are used by [umapp](./api-analysis-umapp.md) and [cedp](./api-analysis-cedp.md) analyses.

**`normalization_log_offset`** - Offset of log2 transformation: `log2(x + offset)`.
- Type: _Number_
- Default: `0.1`

**`normalization_method`** - Normalization method applied after log2 transformation.
- Type: _String_
- Possible values: `"median"` (median centering), `"quantile"` (quantile normalization)
- Default: `"median"`

**`imputation_method`** - Missing values imputation method.
- Type: _String_
- Possible values: `"mindet"` (deterministic minimum), `"minprob"` (probabilistic minimum)
- Default: `"mindet"`

**`stratify_imputation_by_batch`** - Impute missing values separately for each batch.
- Type: _Boolean_
- Default: `false`

**`batch_correction_method`** - Batch correction method.
- Note: The DEP runner ignores this option; if all samples have a batch and there are at least two batches, the batch is added to the linear model instead.
- Type: _String_
- Possible values: `"combat"`, `"limma"`, `null`
- Default: `null`

**`min_non_missing_fraction`** - Minimum fraction of non-missing values a protein needs to be analysed.
- Type: _Number_
- Default: `0.7`

**`require_min_fraction_one_class`** - Apply `min_non_missing_fraction` per condition: a protein is kept if it has enough values in at least one condition.
- Type: _Boolean_
- Default: `true`

### Example
```jsonc
{
    "name": "Primary vs recurrent",
    "data": {
        "datasets": [
            {
                "id": "8b4e6a2c-2b4b-4e3e-9c6d-5a4f1d3b9e0f",
                "name": "Primary",
                "order": 1,
                "domain": "materials",
                "criteria": {...}
            },
            {
                "id": "2e6ee2ff-d96d-422d-8a0d-a49a412baa5c",
                "name": "Recurrent",
                "order": 2,
                "domain": "materials",
                "criteria": {...}
            }
        ],
        "options": {
            "normalization_log_offset": 0.1,
            "normalization_method": "median",
            "imputation_method": "mindet",
            "stratify_imputation_by_batch": false,
            "batch_correction_method": null,
            "min_non_missing_fraction": 0.7,
            "require_min_fraction_one_class": true
        }
    }
}
```


## Dataset Requirements
- Samples of mass spectrometry analysis with protein expression data. Samples without expression values are skipped.
- Datasets should have different names, because the dataset name is the condition label.
- If `batch_correction_method` is set and only some samples have a batch, a warning is produced during preparation (it is not stored with the analysis).


## Input
The service writes the following files to the working directory:
- `data.tsv` - raw protein expression values: `feature` (Ensembl protein id) and one column per sample (internal sample id).
- `metadata.tsv` - `sample` (internal sample id), `condition` (dataset `name`), `batch`, `donor` (donor id), `specimen` (specimen id), `specimen_type`.
- `annotations.tsv` - protein annotations: `id` (Ensembl protein id), `accession`, `symbol`, `gene` (gene symbol), `description`, `source`.
- `options.json` - analysis options.

The last condition in `metadata.tsv` (the last dataset) is the reference condition of the comparison.


## Results Metadata
`GET api/analysis/{id}/meta?file={file}` returns a file of the working directory:
- no `file` - `results.tsv`: limma results written by the runner (`feature`, `logFC`, `AveExpr`, `t`, `P.Value`, `adj.P.Val`, `B`, `contrast`; see the [runner](https://github.com/dkfz-unite/unite-analysis-dep)).
- `file` - the file with the given name, e.g. `annotations.tsv` (used by the portal), `metadata.tsv`, `data.tsv` or `options.json`.


## Results Data
`GET api/analysis/{id}/data` returns `results.zip` with:
- `data.tsv`, `metadata.tsv`, `annotations.tsv`, `options.json` - [input](#input) files.
- `results.tsv` - limma results.
