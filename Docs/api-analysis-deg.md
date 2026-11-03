# Bulk RNA Differential Expression Analysis (DESeq2)
This analysis performs DESeq2 differential gene expression analysis of bulk RNA data between datasets.  
Analysis data is processed by the [DEG analysis service](https://github.com/dkfz-unite/unite-analysis-deg).

Endpoint: `POST api/analysis/deg`


## Model
[Analysis](./api-model-analysis.md) with the following `data`.

**`*`** - Required fields

**`datasets`*** - Datasets to analyse
- Type: _Array_
- Element type: [Dataset](./api-model-dataset.md)
- Limitations: Two datasets are expected (the number is not validated by the service). Supported domains: `donors`, `mrs`, `materials`, `lines`, `organoids`, `xenografts`.
- Example: `[{...}, {...}]`

The analysis has no options.

### Example
```jsonc
{
    "name": "Old vs young",
    "data": {
        "datasets": [
            {
                "id": "8b4e6a2c-2b4b-4e3e-9c6d-5a4f1d3b9e0f",
                "name": "Old",
                "order": 1,
                "domain": "donors",
                "criteria": {
                    "donor": { "age": { "value": { "from": 70 } } }
                }
            },
            {
                "id": "2e6ee2ff-d96d-422d-8a0d-a49a412baa5c",
                "name": "Young",
                "order": 2,
                "domain": "donors",
                "criteria": {
                    "donor": { "age": { "value": { "to": 69 } } }
                }
            }
        ]
    }
}
```


## Dataset Requirements
- Samples with bulk gene expression data (raw read counts). For each donor, image or specimen one sample with gene expression data is taken.
- Datasets should not overlap: the same donor, image or specimen in two datasets of the same domain produces duplicate sample rows in `metadata.tsv`.
- Only genes with a value in every sample and with more than 10 reads in more than half of the samples are analysed.


## Input
The service writes the following files to the working directory:
- `data.tsv` - raw read counts: `gene_id` (Ensembl gene id) and one column per sample. Sample columns are named `{Domain}_{id}`, where `{Domain}` is the dataset domain (`Donors`, `Mrs`, `Materials`, `Lines`, `Organoids`, `Xenografts`) and `{id}` is the internal id of the donor, image or specimen.
- `metadata.tsv` - `sample_id` (sample column name) and `condition` (dataset `id`).

The runner compares the conditions with DESeq2 (`design = ~ condition`) and writes `results.tsv` (see the [runner](https://github.com/dkfz-unite/unite-analysis-deg) for details).


## Results Metadata
`GET api/analysis/{id}/meta` returns `results_final.tsv` (the `file` parameter is ignored): [DEG result](./api-model-deg-result.md) with gene annotations, created by the service from the runner results.


## Results Data
`GET api/analysis/{id}/data` returns `results.zip` with the following files:
- `data.tsv` - [input](#input) read counts.
- `metadata.tsv` - [input](#input) sample conditions.
- `results.tsv` - DESeq2 results written by the runner.
