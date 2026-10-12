# Analysis API
Allows to create and manage analysis tasks, to load and download their results, and to start single cell dataset viewers.

> [!Note]
> API is accessible for authorized users only and requires `JWT` token as `Authorization` header (read more about [Identity Service](https://github.com/dkfz-unite/unite-identity)).

API is **proxied** to main API and can be accessed at [[host]/api/analysis](http://localhost/api/analysis) (**without** `api` prefix).

Analyses run asynchronously: creating an analysis queues a task, which is then prepared (input data is collected and written to the shared analysis data directory) and processed (the method runs, usually in a separate runner service). Results are available when the task status is `Processed`.

## Overview
- get:[api](#get-api) - health check.
- post:[api/analysis/{type}](#post-apianalysistype) - create or restart analysis.
- get:[api/analysis/scell/models](#get-apianalysisscellmodels) - get available cell type annotation models.
- put:[api/analysis/{id}/status](#put-apianalysisidstatus) - get analysis status.
- get:[api/analysis/{id}/meta](#get-apianalysisidmeta) - load analysis result file.
- get:[api/analysis/{id}/data](#get-apianalysisiddata) - download analysis results archive.
- delete:[api/analysis/{id}](#delete-apianalysisid) - delete analysis.
- post:[api/analyses](#post-apianalyses) - get user analyses list.
- delete:[api/analyses](#delete-apianalysesuseridid) - delete all user analyses.
- get:[api/metadata/options](#get-apimetadataoptions) - get sample metadata properties.
- get:[api/metadata/values](#get-apimetadatavalueskeykey) - get values of sample metadata property.
- post:[api/viewer/scell](#post-apiviewerscellidid) - start single cell dataset viewer.
- put:[api/viewer/scell](#put-apiviewerscellidid) - keep single cell dataset viewer alive.


## GET: [api](http://localhost:5004/api)
Health check.

### Responses
`"2022-03-17T09:45:10.9359202Z"` - Current UTC date and time in JSON format, if service is up and running


## POST: [api/analysis/{type}](http://localhost:5004/api/analysis/{type})
Create analysis of given type, or restart an existing analysis.

### Parameters
- `type` - analysis type.

#### Analysis types
- `surv` - [Survival curve estimation (Kaplan-Meier)](./api-analysis-surv.md).
- `dm` - [Differential methylation](./api-analysis-dm.md).
- `pcam` - [Principal component analysis of methylation data](./api-analysis-pcam.md).
- `deg` - [Bulk RNA differential gene expression (DESeq2)](./api-analysis-deg.md).
- `gaf` - [Gene alteration frequency](./api-analysis-gaf.md).
- `dep` - [Differential protein expression (limma)](./api-analysis-dep.md).
- `umapp` - [UMAP of protein expression profiles](./api-analysis-umapp.md).
- `cedp` - [Protein expression distribution per condition](./api-analysis-cedp.md).
- `scell` - [Single cell RNA dataset creation](./api-analysis-scell.md).
- `cnv-profile` - [CNV profiles grid](./api-analysis-cnv-profile.md).

> [!Note]
> Gene expression distribution per condition (CEDG) is planned but not implemented yet. There is no endpoint for it.

### Body
[Analysis](./api-model-analysis.md) with method specific `data`:
- [surv](./api-analysis-surv.md#model)
- [dm](./api-analysis-dm.md#model)
- [pcam](./api-analysis-pcam.md#model)
- [deg](./api-analysis-deg.md#model)
- [gaf](./api-analysis-gaf.md#model)
- [dep](./api-analysis-dep.md#model)
- [umapp](./api-analysis-umapp.md#model)
- [cedp](./api-analysis-cedp.md#model)
- [scell](./api-analysis-scell.md#model)
- [cnv-profile](./api-analysis-cnv-profile.md#model)

If the body has no `id`, a new analysis is created and queued.
If the body has the `id` of an existing analysis with status `Processed` or `Failed`, the analysis is restarted: its working directory is deleted, its record is updated with the request data and its task is queued again. Processing uses the data stored in the task when the analysis was created.

### Resources
- `"id"` - unique id of created analysis (string).

### Responses
- `200` - request was processed successfully
- `400` - request data didn't pass validation, or the analysis to restart is still in progress
- `401` - missing JWT token
- `404` - analysis to restart was not found


## GET: [api/analysis/scell/models](http://localhost:5004/api/analysis/scell/models)
Get the list of available cell type annotation models.
The list is loaded from the [CellTypist](https://www.celltypist.org) models registry (`https://celltypist.cog.sanger.ac.uk/models/models.json`) and returned as is. Model names can be used as `model` option of the [scell](./api-analysis-scell.md#analysis-options) analysis.

### Resources
CellTypist models registry JSON.

### Responses
- `200` - request was processed successfully
- `400` - models registry could not be loaded
- `401` - missing JWT token


## PUT: [api/analysis/{id}/status](http://localhost:5004/api/analysis/{id}/status)
Get analysis status. Also copies the status and comment of the task to the analysis record.

### Parameters
- `id` - analysis id.

### Resources
```jsonc
{
    "status": "Processed", // Analysis task status
    "comment": null // Task comment, e.g. "Failed due to service reboot" (omitted if empty)
}
```

#### Task status types:
- `Preparing` - analysis is being prepared.
- `Prepared` - analysis is prepared and waits for processing.
- `Processing` - analysis is being processed.
- `Processed` - analysis is processed, results are available.
- `Failed` - analysis failed at one of the steps. Failed analyses are not retried automatically, they can be [restarted](#post-apianalysistype).

A newly created analysis has no status until preparation starts. While the task has no status, the endpoint returns `500`.

### Responses
- `200` - request was processed successfully
- `401` - missing JWT token
- `404` - task with given id was not found
- `500` - task has no status yet


## GET: [api/analysis/{id}/meta](http://localhost:5004/api/analysis/{id}/meta?file={file})
Load analysis result file for visualization.

### Parameters
- `id` - analysis id.
- `file` - (optional) file to load. Supported only by some analysis types, see the method pages.

### Resources
Content of the result file. The file and its format depend on the analysis type:
- [surv](./api-analysis-surv.md#results-metadata)
- [dm](./api-analysis-dm.md#results-metadata)
- [pcam](./api-analysis-pcam.md#results-metadata)
- [deg](./api-analysis-deg.md#results-metadata)
- [gaf](./api-analysis-gaf.md#results-metadata)
- [dep](./api-analysis-dep.md#results-metadata)
- [umapp](./api-analysis-umapp.md#results-metadata)
- [cedp](./api-analysis-cedp.md#results-metadata)
- [scell](./api-analysis-scell.md#results-metadata)
- [cnv-profile](./api-analysis-cnv-profile.md#results-metadata)

### Responses
- `200` - request was processed successfully
- `400` - analysis type is not supported
- `401` - missing JWT token
- `404` - task with given id was not found
- `500` - result file is not available (e.g. analysis is not processed yet)


## GET: [api/analysis/{id}/data](http://localhost:5004/api/analysis/{id}/data)
Download analysis results archive (zip).

### Parameters
- `id` - analysis id.

### Resources
Zip archive. Its name and content depend on the analysis type:
- [surv](./api-analysis-surv.md#results-data)
- [dm](./api-analysis-dm.md#results-data)
- [pcam](./api-analysis-pcam.md#results-data)
- [deg](./api-analysis-deg.md#results-data)
- [gaf](./api-analysis-gaf.md#results-data)
- [dep](./api-analysis-dep.md#results-data)
- [umapp](./api-analysis-umapp.md#results-data)
- [cedp](./api-analysis-cedp.md#results-data)
- [scell](./api-analysis-scell.md#results-data)
- [cnv-profile](./api-analysis-cnv-profile.md#results-data)

### Responses
- `200` - request was processed successfully
- `400` - analysis type is not supported
- `401` - missing JWT token
- `404` - task with given id was not found
- `500` - archive is not available (e.g. analysis is not processed yet)


## DELETE: [api/analysis/{id}](http://localhost:5004/api/analysis/{id})
Delete analysis: its task, its record and its working directory with all data and results.
Only analyses with status `Processed` or `Failed` can be deleted.

### Parameters
- `id` - analysis id.

### Responses
- `200` - request was processed successfully
- `400` - analysis is still in progress
- `401` - missing JWT token
- `404` - task with given id was not found


## POST: [api/analyses](http://localhost:5004/api/analyses)
Get user analyses list.

### Body
#### json - application/json
```jsonc
{
  "userId": "user@example.com" // User id stored with the analyses (the portal uses the account email)
}
```

### Resources
Array of analysis records:
```jsonc
[
    {
        "id": "66f1c0f0e4b0a1a2b3c4d5e6", // Analysis id
        "userId": "user@example.com",
        "name": "Old vs young",
        "description": "Survival of donors older and younger than 70",
        "date": "2024-09-23T10:00:00.000Z",
        "status": "Processed", // Last status, updated by the status endpoint
        "type": "surv",
        "data": "{...}" // Analysis data (datasets and options) as JSON string
    }
]
```
Empty fields are omitted.

### Responses
- `200` - request was processed successfully
- `401` - missing JWT token


## DELETE: [api/analyses?userId={id}](http://localhost:5004/api/analyses?userId={id})
Delete all analyses of the user (tasks, records and working directories) regardless of their status.

### Parameters
- `userId` - user id stored with the analyses (the portal uses the account email).

### Responses
- `200` - request was processed successfully
- `401` - missing JWT token


## GET: [api/metadata/options](http://localhost:5004/api/metadata/options)
Get the keys of sample metadata properties (e.g. `donor_sex`, `specimen_tumor_type`).
The keys can be used as `condition_property` ([cedp](./api-analysis-cedp.md#analysis-options)), `class_property` ([umapp](./api-analysis-umapp.md#analysis-options)) and `track_property` ([cnv-profile](./api-analysis-cnv-profile.md#analysis-options)) options. See [sample metadata](./api-model-sample-metadata.md#columns).

### Resources
Array of property keys.

### Responses
- `200` - request was processed successfully
- `401` - missing JWT token


## GET: [api/metadata/values?key={key}](http://localhost:5004/api/metadata/values?key={key})
Get up to 50 most frequent values of sample metadata property in the database (for autocomplete).

### Parameters
- `key` - property key. Supported keys are donor, image and specimen properties except ages, KPS, MR volumes, organoid and xenograft implanted cells numbers and xenograft survival days.

### Resources
Array of values as strings. Empty array if the key is not supported.

### Responses
- `200` - request was processed successfully
- `401` - missing JWT token


## POST: [api/viewer/scell?id={id}](http://localhost:5004/api/viewer/scell?id={id})
Start a [CELLxGENE](https://cellxgene.cziscience.com) viewer for the dataset created by [scell](./api-analysis-scell.md) analysis.
The service starts a viewer container for the analysis (or reuses the running one) which serves the analysis `result.data.h5ad` file at `/viewer/cxg{number}/`. Viewers are removed after `UNITE_ANALYSIS_IDLE_TIMEOUT` minutes without activity; analysis results are not affected.

### Parameters
- `id` - analysis id.

### Resources
- `"number"` - two digit viewer number (e.g. `01`), used in the viewer path `/viewer/cxg01/`.

### Responses
- `200` - request was processed successfully


## PUT: [api/viewer/scell?id={id}](http://localhost:5004/api/viewer/scell?id={id})
Mark the viewer of the analysis as active, so it is not removed as idle.

### Parameters
- `id` - analysis id.

### Resources
- `true` - viewer is running and was marked as active.
- `false` - there is no running viewer for the analysis.

### Responses
- `200` - request was processed successfully
