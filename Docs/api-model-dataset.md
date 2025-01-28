# Dataset Model
Includes the information about the dataset.

```jsonc
{
    "id": "8b4e6a2c-2b4b-4e3e-9c6d-5a4f1d3b9e0f",
    "order": 1,
    "domain": "Donors",
    "criteria": {
        "donor": { "age": { "from": 70 } }
    }
}
```

## Fields
**`id`*** - Dataset unique identifier.
- Note: Id is used to identify the dataset in the analysis task.
- Type: _String_
- Example: `"8b4e6a2c-2b4b-4e3e-9c6d-5a4f1d3b9e0f"`

**`order`** - Dataset order.
- Note: Order is used to sort the datasets in the analysis task (sometimes order of datasets matters).
- Type: _Number_
- Example: `1`

**`domain`*** - Dataset domain.
- Note: Domain is used to understand, which data to include in the dataset according to search criteria.
- Type: _String_
- Possible values: [Dataset domains](#dataset-domains)
- Example: `"Donors"`

**`criteria`*** - Dataset search criteria.
- Note: Criteria is used to filter the dataset data in given domain.
- Type: [Search criteria](https://github.com/dkfz-unite/unite-indices/blob/main/Docs/search-criteria.md)
- Example: `{...}`

### Dataset Domains
Dataset domains can be of the following types:
- `Donors` - Donors data.
- `Mris` - MRI images data.
- `Cts` - CT images data.
- `Materials` - Dall donor derived materials data.
- `Lines` - Cell lines data.
- `Organoids` - Organoids data.
- `Xenografts` - Xenografts data.
- `Genes` - Genes data.
- `Ssms` - Simple somatic mutations data.
- `Cnvs` - Copy number variants data.
- `Svs` - Structural variants data.


##
**`*`** - Required fields
