# Protein Expression Distribution per Condition Analysis
This analysis compares the expression of one gene or protein (mass spectrometry data) between sample conditions of one dataset. Conditions are values of a sample metadata property.  
Analysis data is processed by the [CEDP analysis service](https://github.com/dkfz-unite/unite-analysis-cedp).

Endpoint: `POST api/analysis/cedp`

> [!Note]
> The same analysis for gene expression data (CEDG) is planned but not implemented yet.


## Model
[Analysis](./api-model-analysis.md) with the following `data`.

**`*`** - Required fields

**`datasets`*** - Datasets to analyse
- Type: _Array_
- Element type: [Dataset](./api-model-dataset.md)
- Limitations: Exactly one dataset.
- Example: `[{...}]`

**`options`*** - Analysis options
- Type: _Object([Options](#analysis-options))_
- Example: `{...}`

### Analysis Options
Preprocessing options of [DEP analysis](./api-analysis-dep.md#analysis-options): `normalization_log_offset`, `normalization_method`, `imputation_method`, `stratify_imputation_by_batch`, `batch_correction_method`, `min_non_missing_fraction`, `require_min_fraction_one_class` (with the same defaults), and the following options.

**`feature_type`** - Type of the analysed feature.
- Type: _String_
- Possible values: `"gene"`, `"protein"`
- Default: `"gene"`

**`feature_name`*** - Symbol of the gene or protein to analyse.
- Note: The service resolves it to the Ensembl gene or protein id and writes it to the `feature` option of `options.json`.
- Type: _String_
- Example: `"EGFR"`

**`condition_property`** - [Sample metadata](./api-model-sample-metadata.md#columns) property which defines the sample condition.
- Note: Property keys are available at [api/metadata/options](./api.md#get-apimetadataoptions). If not set or unknown, the conditions in `metadata.tsv` are empty.
- Type: _String_
- Example: `"specimen_tumor_type"`

**`condition_value`** - Conditions to include.
- Note: If set, only samples with one of these values of `condition_property` are analysed. Values are compared with the [sample metadata values](./api-model-sample-metadata.md#values).
- Type: _Array_
- Default: `null` (all samples)
- Example: `["Primary", "Recurrent"]`

**`model_type`** - Model used to compare the conditions.
- Type: _String_
- Possible values: `"lm"` (least squares), `"rfit"` (rank based)
- Default: `"lm"`

**`feature`** - Ensembl id of the analysed feature.
- Note: Set by the service from `feature_name`; a value in the request is overwritten.
- Type: _String_

### Example
```jsonc
{
    "name": "EGFR in primary and recurrent tumors",
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
            "feature_type": "protein",
            "feature_name": "EGFR",
            "condition_property": "specimen_tumor_type",
            "condition_value": ["Primary", "Recurrent"],
            "model_type": "lm"
        }
    }
}
```


## Dataset Requirements
- Samples of mass spectrometry analysis with protein expression data. Samples without expression values are skipped.
- At least one sample has to have expression data of the requested gene (any of its proteins) or protein, otherwise the analysis fails.
- If `batch_correction_method` is set and only some samples have a batch, a warning is produced during preparation (it is not stored with the analysis).


## Input
The service writes the following files to the `input` directory of the analysis:
- `data.tsv` - raw expression values: `feature` (Ensembl gene id if `feature_type` is `gene`, Ensembl protein id if `protein`) and one column per sample (internal sample id). With `gene`, the values of one protein per gene and sample are written.
- `metadata.tsv` - `sample` (internal sample id), `condition` (value of `condition_property`), `batch`, `donor` (donor id), `specimen` (specimen id), `specimen_type`.
- `samples.tsv` - [sample metadata](./api-model-sample-metadata.md) with `sample_key` (internal sample id).
- `options.json` - analysis options.

The runner writes its results to the `output` directory. The runner is not part of this repository.


## Results Metadata
`GET api/analysis/{id}/meta?file={file}` returns a file of the analysis directory:
- no `file` - `output/values.tsv` written by the runner.
- `file` - the file with the given path relative to the analysis directory, e.g. `output/contrasts.tsv` or `input/samples.tsv` (both used by the portal).


## Results Data
`GET api/analysis/{id}/data` returns `analysis.zip` with all files of the `input` and `output` directories (paths `input/...` and `output/...`).
