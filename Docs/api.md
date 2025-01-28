# Analysis API
Allows to create and manage analysis tasks.

> [!Note]
> API is accessible for authorized users only and requires `JWT` token as `Authorization` header (read more about [Identity Service](https://github.com/dkfz-unite/unite-identity)).

API is **proxied** to main API and can be accessed at [[host]/api/analysis](http://localhost/api/analysis) (**without** `api` prefix).

## Overview
- get:[api](#get-api) - health check.
- post:[api/analysis/{type}](#post-apianalysistype) - create analysis.
- put:[api/analysis/{id}/status](#put-apianalysisidstatus) - get analysis status.
- get:[api/analysis/{id}/meta](#get-apianalysisidmeta) - get analysis results metadata.
- get:[api/analysis/{id}/data](#get-apianalysisiddata) - download analysis results data.
- delete:[api/analysis/{id}](#delete-apianalysisid) - delete analysis data.
- post:[api/analyses](#post-apianalyses) - get user analyses list.
- delete:[api/analyses](#delete-apianalysesuseridid) - delete all user analyses.


## GET: [api](http://localhost:5004/api)
Health check.

### Responses
`"2022-03-17T09:45:10.9359202Z"` - Current UTC date and time in JSON format, if service is up and running


## POST: [api/analysis/{type}](http://localhost:5004/api/analysis/{type})
Create analysis of given type.

### Parameters
- `type` - analysis type.

#### Task types:
- `deseq2` - [Bulk RNA differential expression analysis](./api-analysis-deseq2.md).
- `rnasc` - [Single cell RNA analysis](./api-analysis-rnasc.md).
- `kmeier` - [Kaplan-Meier survival analysis](./api-analysis-kmeier.md).

### Body
Depends on the analysis type:
- [deseq2](./api-analysis-deseq2.md#model)
- [rnasc](./api-analysis-rnasc.md#model)
- [kmeier](./api-analysis-kmeier.md#model)

### Resources
- `"id"` - unique id of created analysis.

### Responses
- `200` - request was processed successfully
- `400` - request data didn't pass validation
- `401` - missing JWT token


## PUT: [api/analysis/{id}/status](http://localhost:5004/api/analysis/{id}/status)
Get analysis status. Also synchronises status with the actual task status.

### Parameters
- `id` - analysis id.

### Resources
- `"status"` - analysis status type.

#### Task status types:
- `Preparing` - analysis is being prepared.
- `Prepared` - analysis is prepared.
- `Processing` - analysis is being processed.
- `Processed` - analysis is processed.
- `Failed` - analysis failed at one of the steps.

### Responses
- `200` - request was processed successfully
- `400` - request data didn't pass validation
- `401` - missing JWT token
- `404` - task with given key was not found


## GET: [api/analysis/{id}/meta](http://localhost:5004/api/analysis/{id}/meta)
Get analysis results metadata.

### Parameters
- `id` - analysis id.

### Resources
Depends on the analysis type:
- [deseq2](./api-analysis-deseq2.md#results-metadata)
- [rnasc](./api-analysis-rnasc.md#results-metadata)
- [kmeier](./api-analysis-kmeier.md#results-metadata)

### Responses
- `200` - request was processed successfully
- `400` - request data didn't pass validation
- `401` - missing JWT token
- `404` - task with given key was not found


## GET: [api/analysis/{id}/data](http://localhost:5004/api/analysis/{id}/data)
Download analysis results data.

### Parameters
- `id` - analysis id.

### Resources
A file (or archive) to download. Depends on the analysis type:
- [rna-de](./api-analysis-deseq2.md#results-data)
- [rnasc](./api-analysis-rnasc.md#results-data)
- [kmeier](./api-analysis-kmeier.md#results-data)

### Responses
- `200` - request was processed successfully
- `400` - request data didn't pass validation
- `401` - missing JWT token
- `404` - task with given key was not found


## DELETE: [api/analysis/{id}](http://localhost:5004/api/analysis/{id})
Delete analysis and all it's related data.

### Parameters
- `id` - analysis id.

### Responses
- `200` - request was processed successfully
- `400` - request data didn't pass validation
- `401` - missing JWT token
- `404` - task with given key was not found


## POST: [api/analyses](http://localhost:5004/api/analyses)
Get user analyses list.

### Body
#### json - applications/json
```jsonc
{
  "userId": "email" // user email
}
```

### Resources
- `"analyses"` - list of user analyses in JSON format.

### Responses
- `200` - request was processed successfully
- `400` - request data didn't pass validation
- `401` - missing JWT token


## DELETE: [api/analyses?userId={id}](http://localhost:5004/api/analyses?userId={id})
Delete all user analyses.

### Parameters
- `userId` - unique user id (currently email).

### Responses
- `200` - request was processed successfully
- `400` - request data didn't pass validation
- `401` - missing JWT token
