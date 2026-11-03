# Analysis Model
Common request body of all [analysis types](./api.md#analysis-types).
Method specific fields are in `data`, see the page of each analysis type.

```jsonc
{
    "id": null,
    "userId": "user@example.com",
    "type": "surv",
    "name": "Old vs young",
    "description": "Survival of donors older and younger than 70",
    "date": "2024-09-23T10:00:00.000Z",
    "data": {
        "datasets": [{...}, {...}],
        "options": {...}
    }
}
```

## Fields
**`id`** - Analysis id.
- Note: Leave empty to create a new analysis. Set to the id of an existing analysis with status `Processed` or `Failed` to restart it.
- Type: _String_
- Example: `"66f1c0f0e4b0a1a2b3c4d5e6"`

**`userId`** - Id of the user, who owns the analysis.
- Note: Stored with the analysis record and used to [list](./api.md#post-apianalyses) and [delete](./api.md#delete-apianalysesuseridid) user analyses. The portal uses the account email.
- Type: _String_
- Example: `"user@example.com"`

**`type`** - Analysis type.
- Note: Stored with the analysis record only. The analysis type is defined by the endpoint.
- Type: _String_
- Example: `"surv"`

**`name`** - Analysis name.
- Type: _String_
- Example: `"Old vs young"`

**`description`** - Analysis description.
- Type: _String_
- Example: `"Survival of donors older and younger than 70"`

**`date`** - Analysis date.
- Note: Stored with the analysis record only.
- Type: _String_
- Example: `"2024-09-23T10:00:00.000Z"`

**`data`*** - Analysis data.
- Type: _Object_
- Example: `{...}`

`status` and `comment` fields are stored as well, but are overwritten by the [status](./api.md#put-apianalysisidstatus) endpoint.

### Analysis Data
**`datasets`*** - Datasets to analyse.
- Type: _Array_
- Element type: [Dataset](./api-model-dataset.md)
- Limitations: Number of datasets depends on the analysis type.
- Example: `[{...}, {...}]`

**`options`** - Analysis options.
- Note: Not used by [deg](./api-analysis-deg.md) analysis. Omitted options take their default values.
- Type: _Object_
- Example: `{...}`

`data.id` is set by the service to the analysis id; a value in the request is ignored when an analysis is created.

## Validation
The request is checked only for the correct format (e.g. allowed option values).
Dataset requirements (number of datasets, available data) are checked when the analysis is prepared or processed; if they are not met, the analysis status becomes `Failed`.

##
**`*`** - Required fields
