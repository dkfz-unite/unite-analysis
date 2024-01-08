# Analysis API
This API allows to create and manage analysis tasks.

## General
General API endpoints.

## GET: [api](http://localhost:5004/api) - [api/analysis](https://localhost/api/analysis)
Health check.

### Responses
`"2022-03-17T09:45:10.9359202Z"` - Current UTC date and time in JSON format, if service is up and running


## Tasks
API is accessible for authorized users only and requires `Authorization: Bearer [token]` to be set in request headers.

To read more about authorization visit [Identity Service](https://github.com/dkfz-unite/unite-identity) page.


## POST: [api/tasks/{type}](http://localhost:5004/api/tasks/{type}) - [api/analysis/tasks/dexp](https://localhost/api/analysis/tasks/{type})
Create analysis task.

### Body
Depending of the task type, the body is:
- `dexp` - [DESeq2](./api-model-deseq2.md) analysis data in `application/json` format.

### Resources
- `"key"` - unique key of created analysis task.

### Responses
- `200` - request was processed successfully
- `400` - request data didn't pass validation
- `401` - missing JWT token


## GET: [api/tasks/{key}](http://localhost:5004/api/tasks/{key}) - [api/analysis/tasks/{key}](https://localhost/api/analysis/tasks{key})
Get analysis task status by it's key.

### Resources
- `"status"` - task status type.

Task status types:
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


## GET: [api/tasks/{key}/results](http://localhost:5004/api/tasks/{key}/results) - [api/analysis/tasks/{key}/results](https://localhost/api/analysis/tasks/{key}/results)
Get analysis task results by it's key.

### Resources
Depending of the task type, the result is:
- `dexp` - [DESeq2](./api-model-deseq2-result.md) analysis results.

### Responses
- `200` - request was processed successfully
- `400` - request data didn't pass validation
- `401` - missing JWT token
- `404` - task with given key was not found


## GET: [api/tasks/{key}/data](http://localhost:5004/api/tasks/{key}/data) - [api/analysis/tasks/{key}/data](https://localhost/api/analysis/tasks/{key}/data)
Download analysis task results by it's key.

### Resources
Depending of the task type, the download data is:
- `dexp` - DESeq2 analysis data (the same as [result](./api-model-deseq2-result.md)).

### Responses
- `200` - request was processed successfully
- `400` - request data didn't pass validation
- `401` - missing JWT token
- `404` - task with given key was not found


## DELETE: [api/tasks/{key}](http://localhost:5004/api/tasks/{key}) - [api/analysis/tasks/{key}](https://localhost/api/analysis/tasks/{key})
Delete analysis task by it's key (if task was completed or failed).

### Responses
- `200` - request was processed successfully
- `400` - request data didn't pass validation
- `401` - missing JWT token
- `404` - task with given key was not found
