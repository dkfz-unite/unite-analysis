# Survival Curve Estimation Analysis (Kaplan-Meier)
This analysis estimates Kaplan-Meier survival (or progression free) curves for each dataset and compares them with the log-rank test.  
Analysis data is processed by the [SURV analysis service](https://github.com/dkfz-unite/unite-analysis-surv).

Endpoint: `POST api/analysis/surv`


## Model
[Analysis](./api-model-analysis.md) with the following `data`.

**`*`** - Required fields

**`datasets`*** - Datasets to analyse
- Type: _Array_
- Element type: [Dataset](./api-model-dataset.md)
- Limitations: Only `donors` domain is supported.
- Example: `[{...}, {...}]`

**`options`** - Analysis options
- Type: _Object([Options](#analysis-options))_
- Example: `{...}`

### Analysis Options
**`progression`** - Use disease progression data instead of survival data.
- Type: _Boolean_
- Default: `false`

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
        ],
        "options": {
            "progression": false
        }
    }
}
```


## Dataset Requirements
- Donors need clinical data with:
  - status: vital status (event - donor is dead), or progression status if `progression` is `true`;
  - time: status change day, or enrollment date and status change date.
- Donors without status or time are skipped.
- At least two such donors are required in all datasets together, otherwise the analysis fails.


## Input
The service writes `input.tsv` for the runner, one row per donor:
- `dataset_id` - dataset `name`.
- `donor_id` - donor id from the source data.
- `enrolment_date` - enrollment date (`yyyy-MM-dd`).
- `status` - `true` if the event happened (death or progression), otherwise `false`.
- `status_change_date` - status change date (`yyyy-MM-dd`).
- `status_change_day` - days from enrollment to status change.


## Results Metadata
`GET api/analysis/{id}/meta` returns `output.json` (the `file` parameter is ignored), created by the service from the runner results:
```jsonc
{
    "rank": { // Log-rank test, values rounded to 2 decimals (null if not available)
        "chi2": 4.21,
        "p": 0.04
    },
    "curves": { // Survival curves per dataset name (null if not available)
        "Old": {
            "time": [0, 120, 365],
            "survival_prob": [1, 0.9, 0.75],
            "conf_int_lower": [1, 0.8, 0.6],
            "conf_int_upper": [1, 0.98, 0.9]
        }
    }
}
```


## Results Data
`GET api/analysis/{id}/data` returns `output.zip` with the following files (each one only if it exists):
- `input.tsv` - [input](#input) data.
- `result.tsv` - curves data (`time`, `survival_prob`, `conf_int_lower`, `conf_int_upper`, `dataset_id`).
- `logrank_test.tsv` - log-rank test results (`chi2`, `p`).
- `censored.tsv` - censored data written by the runner.

The archived files are deleted from the working directory; `output.json` and `output.zip` stay.
