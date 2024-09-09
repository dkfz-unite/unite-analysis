# Analysis API
Allows to create and manage analysis tasks.

> [!Note]
> API is accessible for authorized users only and requires `JWT` token as `Authorization` header (read more about [Identity Service](https://github.com/dkfz-unite/unite-identity)).

API is **proxied** to main API and can be accessed at [[host]/api/analysis](http://localhost/api/analysis) (**without** `api` prefix).

## Overview
- get:[api](#get-api) - health check.
- post:[api/task/{type}](#post-apitasktype) - create analysis task.
- get:[api/task/{key}/status](#get-apitaskkeystatus) - get analysis task status.
- get:[api/task/{key}/meta](#get-apitaskkeymeta) - get analysis task results metadata.
- get:[api/task/{key}/data](#get-apitaskkeydata) - download analysis task results data.
- delete:[api/task/{key}](#delete-apitaskkey) - delete analysis task data.


## GET: [api](http://localhost:5004/api)
Health check.

### Responses
`"2022-03-17T09:45:10.9359202Z"` - Current UTC date and time in JSON format, if service is up and running


## POST: [api/task/{type}](http://localhost:5004/api/task/{type})
Create analysis task of given type.

### Parameters
- `type` - task type.

#### Task types:
- `rna-de` - [Bulk RNA differential expression analysis](./api-model-rna_de.md).
- `rnasc` - [Single cell RNA analysis](./api-model-rnasc.md).

### Body
Depends on the task type:
- [rna-de](./api-analysis-rna_de.md#model)
- [rnasc](./api-analysis-rnasc.md#model)

### Resources
- `"key"` - unique key of created analysis task.

### Responses
- `200` - request was processed successfully
- `400` - request data didn't pass validation
- `401` - missing JWT token


## GET: [api/task/{key}/status](http://localhost:5004/api/task/{key}/status)
Get analysis task status.

### Parameters
- `key` - task key.

### Resources
- `"status"` - task status type.

#### Task status types:
- `Preparing` - task is being prepared.
- `Prepared` - task is prepared.
- `Processing` - task is being processed.
- `Processed` - task is processed.
- `Failed` - task failed at one of the steps.

### Responses
- `200` - request was processed successfully
- `400` - request data didn't pass validation
- `401` - missing JWT token
- `404` - task with given key was not found


## GET: [api/task/{key}/meta](http://localhost:5004/api/task/{key}/meta)
Get analysis task results metadata.

### Parameters
- `key` - task key.

### Resources
Depends on the task type:
- [rna-de](./api-analysis-rna_de.md#results-metadata)
- [rnasc](./api-analysis-rnasc.md#results-metadata)

### Responses
- `200` - request was processed successfully
- `400` - request data didn't pass validation
- `401` - missing JWT token
- `404` - task with given key was not found


## GET: [api/task/{key}/data](http://localhost:5004/api/task/{key}/data)
Download analysis task results data

### Parameters
- `key` - task key.

### Resources
A file (or archive) to download. Depends on the task type:
- [rna-de](./api-analysis-rna_de.md#results-data)
- [rnasc](./api-analysis-rnasc.md#results-data)

### Responses
- `200` - request was processed successfully
- `400` - request data didn't pass validation
- `401` - missing JWT token
- `404` - task with given key was not found


## DELETE: [api/task/{key}](http://localhost:5004/api/task/{key})
Delete analysis task and all it's related data.

### Parameters
- `key` - task key.

### Responses
- `200` - request was processed successfully
- `400` - request data didn't pass validation
- `401` - missing JWT token
- `404` - task with given key was not found
