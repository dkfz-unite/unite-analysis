# Gene Alteration Frequency Analysis
This analysis finds the genes most frequently affected by simple mutations (SMs) in the samples of one dataset and prepares the data for an alteration grid (oncogrid).  
The analysis is performed by the analysis service itself (no runner service).

Endpoint: `POST api/analysis/gaf`


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
**`genes`** - Maximum number of genes (the most frequently affected genes are taken).
- Type: _Number_
- Default: `30`

**`sm`** - Impacts of the most severe effect of SM variants to include.
- Type: _Array_
- Possible values: `"High"`, `"Moderate"`, `"Low"`, `"Unknown"`
- Default: `["High", "Moderate"]`

**`donors`** - Number of donors.
- Note: The option is stored, but not used by the analysis.
- Type: _Number_
- Default: `100`

### Example
```jsonc
{
    "name": "Glioma oncogrid",
    "data": {
        "datasets": [
            {
                "id": "8b4e6a2c-2b4b-4e3e-9c6d-5a4f1d3b9e0f",
                "name": "Glioma",
                "order": 1,
                "domain": "donors",
                "criteria": {...}
            }
        ],
        "options": {
            "genes": 30,
            "sm": ["High", "Moderate"]
        }
    }
}
```


## Dataset Requirements
- Samples of WGS or WES analysis with simple mutations.
- Only variants which affect a gene transcript (not up- or downstream) and whose most severe effect has one of the selected impacts are counted.
- Genes are ranked by the number of samples with such variants.


## Results Metadata
`GET api/analysis/{id}/meta` returns `result.json` (the `file` parameter is ignored), created during preparation:
```jsonc
{
    "genes": [
        {
            "id": "1126", // Internal gene id
            "symbol": "IDH1",
            "biotype": "protein_coding",
            "chromosome": "2",
            "strand": false
        }
    ],
    "donors": [
        {
            "id": "15", // Internal donor id
            "displayId": "D001", // Donor id from the source data
            "diagnosis": "Glioblastoma",
            "primarySite": "Brain",
            "localization": "Frontal lobe",
            "sex": "Female",
            "age": 54,
            "vitalStatus": false,
            "vitalStatusChangeDay": 400,
            "progressionStatus": true,
            "progressionStatusChangeDay": 200,
            "steroidsReactive": null,
            "kps": 80
        }
    ],
    "observations": [ // One entry per variant, donor and gene
        {
            "id": 1234567, // Internal variant id
            "donorId": 15,
            "geneId": 1126,
            "position": "2:208248388", // Chromosome and start position
            "type": "SNV",
            "change": "C > T", // Reference and alternate allele ("-" if empty)
            "impact": "Moderate",
            "effect": "missense_variant" // Most severe effect type
        }
    ]
}
```


## Results Data
`GET api/analysis/{id}/data` returns `output.zip` with:
- `result.json` - [results](#results-metadata).
- `options.json` - analysis options.
