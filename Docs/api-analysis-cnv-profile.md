# CNV Profiles Analysis
This analysis builds a grid of copy number gains and losses per chromosome arm for the samples of one dataset, with optional sample metadata tracks.  
The analysis is performed by the analysis service itself (no runner service).

Endpoint: `POST api/analysis/cnv-profile`


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
**`event_threshold`** - Minimum gain or loss fraction of the chromosome arm to report it as an event.
- Note: An arm is a gain if its gain fraction is greater than the threshold, otherwise a loss if its loss fraction is greater than the threshold.
- Type: _Number_
- Default: `0.8`

**`track_property`** - [Sample metadata](./api-model-sample-metadata.md#columns) properties to add as sample tracks.
- Note: Property keys are available at [api/metadata/options](./api.md#get-apimetadataoptions).
- Type: _Array_
- Default: `[]`
- Example: `["donor_sex", "specimen_tumor_type"]`

**`track_property_value`** - Track values to include.
- Note: If set, a track is added to a sample only if its value is one of these [values](./api-model-sample-metadata.md#values).
- Type: _Array_
- Default: `[]` (all values)
- Example: `["Female", "Primary"]`

### Example
```jsonc
{
    "name": "Glioma CNV profiles",
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
            "event_threshold": 0.8,
            "track_property": ["donor_sex", "specimen_tumor_type"],
            "track_property_value": []
        }
    }
}
```


## Dataset Requirements
- Samples of WGS, WES or methylation array analysis.
- Gains and losses are taken from the CNV profiles (per chromosome arm) of the samples. Samples without CNV profiles have no events.


## Results Metadata
`GET api/analysis/{id}/meta?file={file}` returns a file of the analysis directory:
- no `file` - `output/result.json`, created during preparation.
- `file` - the file with the given path relative to the analysis directory, e.g. `input/options.json`.

```jsonc
{
    "samples": [
        {
            "id": 101, // Internal sample id
            "donorId": 15, // Internal donor id
            "specimenId": 37, // Internal specimen id
            "tracks": { "donor_sex": "Female", "specimen_tumor_type": "Primary" }
        }
    ],
    "regions": [ // Arms of all chromosomes except MT
        { "id": "Chr1P", "chromosome": "1", "arm": "P" },
        { "id": "Chr1Q", "chromosome": "1", "arm": "Q" }
    ],
    "observations": [ // Only gains and losses
        { "sampleId": 101, "regionId": "Chr1P", "event": "Gain" }
    ]
}
```


## Results Data
`GET api/analysis/{id}/data` returns `analysis.zip` with:
- `input/options.json` - analysis options.
- `output/result.json` - [results](#results-metadata).
