# DEXP Analysis Task Model
Includes the information about differential expression analysis task data.

Indentifies which datasets in which order should be analysed. More about the task you can read [here](https://github.com/dkfz-unite/unite-analysis-deseq2).
```jsonc
{
    "cohorts": [
        {
            "key": "8b4e6a2c-2b4b-4e3e-9c6d-5a4f1d3b9e0f",
            "order": 1,
            "domain": "Donors",
            "criteria": {
                "donor": { "age": { "from": 70 } }
            }
        },
        {
            "key": "2e6ee2ff-d96d-422d-8a0d-a49a412baa5c",
            "order": 2,
            "domain": "Donors",
            "criteria": {
                "donor": { "age": { "to": 69 } }
            }
        }
    ],
}
```

## Fields
**`cohorts`*** - Datasets to analyse
- Type: _Array_
- Element type: [Dataset model](./api-model-dataset.md)
- Example: `[{...}, {...}]`


##
**`*`** - Required fields
