# Differential Methylation Analysis
This analysis compares DNA methylation array data (CpG M-values) of two datasets with limma.  
Analysis data is processed by the [DM analysis service](https://github.com/dkfz-unite/unite-analysis-dm).

Endpoint: `POST api/analysis/dm`


## Model
[Analysis](./api-model-analysis.md) with the following `data`.

**`*`** - Required fields

**`datasets`*** - Datasets to analyse
- Type: _Array_
- Element type: [Dataset](./api-model-dataset.md)
- Limitations: Exactly two datasets with samples are required by the runner.
- Example: `[{...}, {...}]`

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
    "name": "IDH mutant vs wild type",
    "data": {
        "datasets": [
            {
                "id": "8b4e6a2c-2b4b-4e3e-9c6d-5a4f1d3b9e0f",
                "name": "IDH mutant",
                "order": 1,
                "domain": "materials",
                "criteria": {...}
            },
            {
                "id": "2e6ee2ff-d96d-422d-8a0d-a49a412baa5c",
                "name": "IDH wild type",
                "order": 2,
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
- The runner uses the Illumina EPIC array manifest and annotation (hg19).


## Input
The service downloads the IDAT files of each sample to a sample subdirectory of the working directory and writes:
- `metadata.tsv` - `sample_id` ([sample id](./api-model-sample-metadata.md#columns)), `path` (absolute path of the sample IDAT files without channel suffix) and `condition` (dataset `id`).
- `options.json` - analysis options.

The last condition in `metadata.tsv` (the last dataset) is the reference condition of the comparison.


## Results Metadata
`GET api/analysis/{id}/meta` returns `results_heatmap.tsv.gz` (the `file` parameter is ignored): gzip compressed TSV file with the runner results binned for a volcano plot.
Results are put to a 256 x 256 grid of log2 fold change and -log10 adjusted p-value. Each bin is a row with the following columns:
- `CpgId`, `Regulatory_Feature_Name`, `Phantom4_Enhancers`, `Phantom5_Enhancers`, `UCSC_RefGene_Name` - annotation of the first CpG in the bin.
- `logFc` - mean log2 fold change of the CpGs in the bin.
- `adj.P.Val` - mean adjusted p-value of the CpGs in the bin.
- `Count` - number of CpGs in the bin.


## Results Data
`GET api/analysis/{id}/data` returns `output.zip` with:
- `results.tsv` - limma results written by the runner (see the [runner](https://github.com/dkfz-unite/unite-analysis-dm) for columns).

After processing, the downloaded IDAT files are deleted. `metadata.tsv`, `options.json`, `results.tsv` and `results_heatmap.tsv.gz` stay in the working directory, but only `results.tsv` is archived.
