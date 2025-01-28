# Kaplan Meier Survival Curve Estimation Analysis
This analysis uses available data to perform Kaplan-Meier survival curve estimation analysis.  
Analysis data is processed by the [Kaplan-Meier analysis service](https://github.com/dkfz-unite/unite-analysis-kmeier).


## Model
Includes information about the datasets to analyse and analysis options.

**`*`** - Required fields

**`datasets`*** - Datasets to analyse
- Type: _Array_
- Element type: [Dataset](./api-model-dataset.md)
- Example: `[{...}, {...}]`

**`options`*** - Analysis options
- Type: _Object([Options](#analysis-options))_
- Example: `{...}`

### Analysis Options
Options defining how the analysis should be performed.

**`progression`*** - Use disease progression data insterad of survival data.
- Type: _Boolean_
- Default: `false`

### Example
```jsonc
{
    "datasets": [
        {
            "id": "8b4e6a2c-2b4b-4e3e-9c6d-5a4f1d3b9e0f",
            "order": 1,
            "domain": "Donors",
            "criteria": {
                "donor": { "age": { "from": 70 } }
            }
        },
        {
            "id": "2e6ee2ff-d96d-422d-8a0d-a49a412baa5c",
            "order": 2,
            "domain": "Donors",
            "criteria": {
                "donor": { "age": { "to": 69 } }
            }
        }
    ],
    "options": {
        "progression": false
    }
}
```


## Results Metadata
The analysis has no metadata.


## Results Data
Analysis produces a tsv file with survival curve estimation results.  
The file can can be used for further analysis or visualization.
