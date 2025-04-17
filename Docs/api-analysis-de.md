# Bulk RNA Differential Expression Analysis (DESeq2)
This analysis uses available data to perform DESEq2 differential expression analysis on bulk RNA data.  
Analysis data is processed by the [DE analysis service](https://github.com/dkfz-unite/unite-analysis-de).


## Model
Includes information about the datasets to analyse.

**`*`** - Required fields

**`datasets`*** - Datasets to analyse
- Type: _Array_
- Element type: [Dataset](./api-model-dataset.md)
- Example: `[{...}, {...}]`

### Example
```jsonc
{
    "datasets": [
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


## Results Metadata
The analysis has no metadata.


## Results Data
Analysis produces a tsv file with differential expression information.  
The file can can be used for further analysis or visualization.
