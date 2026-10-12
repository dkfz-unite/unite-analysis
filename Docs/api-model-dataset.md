# Dataset Model
Includes the information about the dataset.
A dataset is a set of search criteria; its content is resolved when the analysis is prepared, with the data visible to the user who created the analysis.

```jsonc
{
    "id": "8b4e6a2c-2b4b-4e3e-9c6d-5a4f1d3b9e0f",
    "name": "Old",
    "order": 1,
    "domain": "donors",
    "criteria": {
        "donor": { "age": { "value": { "from": 70 } } }
    }
}
```

## Fields
**`id`*** - Dataset unique identifier.
- Note: [deg](./api-analysis-deg.md) and [dm](./api-analysis-dm.md) analyses use it as the condition label of the dataset samples.
- Type: _String_
- Example: `"8b4e6a2c-2b4b-4e3e-9c6d-5a4f1d3b9e0f"`

**`name`** - Dataset name.
- Note: [surv](./api-analysis-surv.md) and [dep](./api-analysis-dep.md) analyses use it as the label of the dataset (curve or condition).
- Type: _String_
- Example: `"Old"`

**`order`** - Dataset order.
- Note: [deg](./api-analysis-deg.md) analysis sorts datasets by order. Other analyses use datasets in the order of the array.
- Type: _Number_
- Example: `1`

**`domain`*** - Dataset domain.
- Note: Domain defines which data the search criteria select.
- Type: _String_
- Possible values: [Dataset domains](#dataset-domains)
- Example: `"donors"`

**`criteria`*** - Dataset search criteria.
- Note: Criteria is used to filter the dataset data in given domain.
- Type: [Search criteria](https://github.com/dkfz-unite/unite-indices/blob/main/Docs/search-criteria.md)
- Example: `{...}`

The user scope of the search (`userId`, `isRoot`) is set by the service from the identity of the user who creates the analysis. Values sent in the request are ignored.

### Dataset Domains
Dataset domains can be of the following types:
- `donors` - Donors data.
- `mrs` - MR images data.
- `cts` - CT images data.
- `materials` - Donor derived materials data.
- `lines` - Cell lines data.
- `organoids` - Organoids data.
- `xenografts` - Xenografts data.
- `genes` - Genes data.
- `sms` - Simple mutations data.
- `cnvs` - Copy number variants data.
- `svs` - Structural variants data.

Analyses support only the following domains:
- [surv](./api-analysis-surv.md) - `donors`.
- [deg](./api-analysis-deg.md) - `donors`, `mrs`, `materials`, `lines`, `organoids`, `xenografts`.
- Other analyses - `donors`, `mrs`, `cts`, `materials`, `lines`, `organoids`, `xenografts`.

### Sample Selection
Except for [surv](./api-analysis-surv.md), analyses work with omics samples. For each donor, image or specimen in the dataset, **one** sample of the data type required by the analysis is selected:
- `donors` - a sample of any specimen of the donor.
- `mrs`, `cts` - a sample of any specimen of the image donor. Every image in the dataset has to resolve to its own sample, otherwise the analysis fails.
- `materials`, `lines`, `organoids`, `xenografts` - a sample of the specimen.

If there are several suitable samples, all analyses except [deg](./api-analysis-deg.md) prefer samples in the following order: primary tumor material, recurrent tumor material, metastasis material, other tumor material, normal material, other material, cell line, organoid, xenograft. Samples that have a matched sample are preferred as well (they have half of the rank of their group).
[deg](./api-analysis-deg.md) takes any suitable sample.
Donors and specimens without suitable samples are skipped.

##
**`*`** - Required fields
