# Methylation PCA Analysis
This analysis performs principal component analysis (PCA) of DNA methylation array data (CpG M-values) of one dataset.  
Analysis data is processed by the [PCAM analysis service](https://github.com/dkfz-unite/unite-analysis-pcam).

Endpoint: `POST api/analysis/pcam`


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
**`pp`** - IDAT files preprocessing method ([minfi](https://bioconductor.org/packages/minfi)).
- Type: _String_
- Possible values: `"Illumina"`, `"SWAN"`, `"Quantile"`, `"Noob"`, `"Raw"`
- Default: `"Illumina"`

### Example
```jsonc
{
    "name": "Glioma methylation PCA",
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
            "pp": "Illumina"
        }
    }
}
```


## Dataset Requirements
- Samples of methylation array analysis with methylation sample resources in IDAT format: one red channel file (name contains `red`) and one green channel file (name contains `grn`). Samples without both files are skipped.
- The runner uses the Illumina EPIC array manifest.


## Input
The service downloads the IDAT files of each sample to a sample subdirectory of the working directory and writes:
- `metadata.tsv` - [sample metadata](./api-model-sample-metadata.md) with an additional `path` column (absolute path of the sample IDAT files without channel suffix).
- `options.json` - analysis options.


## Results Metadata
`GET api/analysis/{id}/meta?file={file}`:
- `file=metadata` - returns `metadata.tsv` ([sample metadata](./api-model-sample-metadata.md)).
- any other value or no value - returns `results.tsv`: PCA scores written by the runner, with the `Sample` column (`sample_id`) and principal component columns `PC1` ... `PCn` (up to 20 components).


## Results Data
`GET api/analysis/{id}/data` returns `output.zip` with:
- `metadata.tsv` - [input](#input) sample metadata.
- `results.tsv` - PCA scores.
- `options.json` - analysis options.

After processing, the downloaded IDAT files are deleted.
