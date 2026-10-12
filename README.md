# Analysis Service

Shared architecture, development workflows, and agent instructions:
[UNITE project documentation](https://github.com/dkfz-unite/.github/blob/main/README.md).

## General
Analysis service provides the following functionality:
- [Analysis web API](Docs/api.md) - REST API for data analysis and analysis tasks.
- Analysis preparation service - service for gathering required data and preparing it for the analysis.
- Analysis execution service - service for executing analysis tasks.
- Single cell dataset viewers - on-demand Cell-x-Gene containers for single cell analysis results.

### Documentation
- [API overview](Docs/api.md) - endpoints, statuses, results.
- [Analysis request](Docs/api-model-analysis.md), [dataset](Docs/api-model-dataset.md) and [sample metadata](Docs/api-model-sample-metadata.md) models.
- Analysis types: [surv](Docs/api-analysis-surv.md), [dm](Docs/api-analysis-dm.md), [pcam](Docs/api-analysis-pcam.md), [deg](Docs/api-analysis-deg.md), [gaf](Docs/api-analysis-gaf.md), [dep](Docs/api-analysis-dep.md), [umapp](Docs/api-analysis-umapp.md), [cedp](Docs/api-analysis-cedp.md), [scell](Docs/api-analysis-scell.md), [cnv-profile](Docs/api-analysis-cnv-profile.md). Gene expression distribution per condition (CEDG) is planned but not implemented yet.

Analyses follow asynchronous **Prepare** and **Process** phases:

1. The frontend checks method eligibility, opens an options dialog, and submits
   dataset search criteria and options. The service creates a record in MongoDB
   `user-data.analyses` and a SQL task with `AnalysisTypeId` set.
2. Prepare evaluates dataset criteria through search, resolves relevant samples,
   loads SQL values or downloads required resources, and writes formatted input
   files into a shared volume directory named by the analysis key. The task
   moves from `null` through `Preparing` to `Prepared`.
3. Process takes `Prepared` tasks and performs method-specific processing,
   usually through a separate R/Python container using
   [unite-commands](https://github.com/dkfz-unite/unite-commands). Some methods
   compute internally. Outputs, archives, and cleanup are method-specific; a
   successful task becomes `Processed`.
4. The portal retrieves result data and visualizes it in the browser. Analyses
   and their remaining files stay available until the user deletes them.

The handlers currently allow **10 active jobs per phase**, separately from each
external runner's process limit. A busy command runner returns HTTP `501`;
Analysis restores the task to `Prepared` for another attempt. Preparation or
computation errors become `Failed` and require manual restart. This is different
from the submission pipeline's automatic retry of internal processing errors.

File contracts are agreed per method; there is no universal filename/schema
contract. Preserve the preparation/runner/frontend agreement and include
supporting sample metadata in downloads where available.

Scell creates an AnnData/H5AD dataset from downloaded single-cell resources.
Results are viewed through a local Cell-x-Gene fork in an on-demand Docker
container per dataset, proxied into a portal iframe. Idle viewer containers are
removed separately from retained analysis results.

## Dependencies
- [SQL](https://github.com/dkfz-unite/unite-env/tree/main/programs/postgresql) - domain SQL data and analysis tasks.
- [MongoDB](https://github.com/dkfz-unite/unite-env/tree/main/programs/mongodb) - analysis records in `user-data.analyses`.
- [Elasticsearch](https://github.com/dkfz-unite/unite-env/tree/main/programs/elasticsearch) - Elasticsearch server with indices of domain data.
- Analysis runners (`unite-analysis-<method>`) - services which run the analysis scripts, see [Configuration](#configuration).
- Docker - the service starts single cell viewer containers through the Docker socket.

### Library dependencies

`Unite.Analysis` references the distinct `Unite.Data.Context`,
`Unite.Indices.Search`, and `Unite.Essentials.Tsv` NuGet packages.
`Unite.Analysis.Web` adds `Unite.Cache` for Mongo analysis records. Search brings
the separate index context/models packages. Inspect `.csproj` files for current
versions; a local library edit does not automatically update a package consumer.

## Access
Environment|Address|Port
-----------|-------|----
Host|http://localhost:5004|5004
Docker|http://analysis.unite.net|80

## Configuration
To configure the application, change environment variables in either docker or [launchSettings.json](Unite.Analysis.Web/Properties/launchSettings.json) file (if running locally):

- `ASPNETCORE_ENVIRONMENT` - ASP.NET environment (`Release`).
- `UNITE_API_KEY` - shared 32 characters key, used to validate JWT tokens of requests and to sign the tokens the service uses to download sample resources.
- `UNITE_ELASTIC_HOST` - Elasticsearch service host (`http://es.unite.net:9200`).
- `UNITE_ELASTIC_USER` - Elasticsearch service user.
- `UNITE_ELASTIC_PASSWORD` - Elasticsearch service password.
- `UNITE_SQL_HOST` - SQL server host (`pgsql.unite.net`).
- `UNITE_SQL_PORT` - SQL server port (`5432`).
- `UNITE_SQL_USER` - SQL server user.
- `UNITE_SQL_PASSWORD` - SQL server password.
- `UNITE_MONGO_HOST` - MongoDB host (`mongo.unite.net`).
- `UNITE_MONGO_PORT` - MongoDB port (`27017`).
- `UNITE_MONGO_USER` - MongoDB user.
- `UNITE_MONGO_PASSWORD` - MongoDB password.
- `UNITE_ANALYSIS_DATA_PATH` - path to the analysis data directory, shared with the runners (`/mnt/data`). A leading `~/` is resolved to the user home directory.
- `UNITE_ANALYSIS_DATA_HOST` - (optional) host which replaces the host of sample resource URLs when resources are downloaded (DM, PCAM and Scell), e.g. `http://localhost:5400` for local development. Not set by default.
- `UNITE_ANALYSIS_IDLE_TIMEOUT` - minutes after which an inactive Cell-x-Gene viewer is removed (`30`); viewer removal does not delete analysis results.

Runner hosts (each runner exposes `POST /api/run?key={analysis id}`):
- `UNITE_ANALYSIS_SURV_HOST` - survival estimation runner host (`http://surv.analysis.unite.net`).
- `UNITE_ANALYSIS_DM_HOST` - differential methylation runner host (`http://dm.analysis.unite.net`).
- `UNITE_ANALYSIS_PCAM_HOST` - methylation PCA runner host (`http://pcam.analysis.unite.net`).
- `UNITE_ANALYSIS_DEG_HOST` - bulk RNA differential expression runner host (`http://deg.analysis.unite.net`).
- `UNITE_ANALYSIS_DEP_HOST` - differential protein expression runner host (`http://dep.analysis.unite.net`).
- `UNITE_ANALYSIS_UMAPP_HOST` - protein expression UMAP runner host (`http://umapp.analysis.unite.net`).
- `UNITE_ANALYSIS_CEDP_HOST` - protein expression distribution runner host (`http://cedp.analysis.unite.net`).
- `UNITE_ANALYSIS_SCELL_HOST` - single cell dataset creation runner host (`http://scell.analysis.unite.net`).

GAF and CNV profile analyses are computed by the service itself and have no runner.

Cell-x-Gene viewer (single cell datasets):
- `UNITE_VIEWER_CXG_IMAGE` - viewer image (`ghcr.io/dkfz-unite/unite-cellxgene:latest`). The image has to be available locally.
- `UNITE_VIEWER_CXG_NAME` - viewer container name, `{n}` is replaced with the two digit viewer number (`unite.viewer.cxg{n}`).
- `UNITE_VIEWER_CXG_ALIAS` - viewer network alias, `{n}` is replaced with the two digit viewer number (`cxg{n}.viewer.unite.net`).
- `UNITE_VIEWER_CXG_PORTS` - range of host ports for viewers in the format `start-end` (`54301-54399`); the range also limits the number of viewers.

Viewers are started through the Docker socket (`/var/run/docker.sock`) in the `unite` network, and mount the host source of the service data directory.

> [!NOTE]
> For local development purposes we recommend to use **default** values.

## Installation

### Docker Compose
The easiest way to install the application is to use docker-compose:
- [Environment configuration and installation scripts](https://github.com/dkfz-unite/unite-env)
- [Analysis service configuration and installation scripts](https://github.com/dkfz-unite/unite-env/tree/main/applications/unite-analysis)

### Docker
The image of the service is available in our [registry](https://github.com/dkfz-unite/unite-analysis/pkgs/container/unite-analysis).

[Dockerfile](./Dockerfile) is used to build an image of the application. The build restores UNITE NuGet packages from GitHub Packages, so a GitHub user and a token with `read:packages` scope are required. To build an image run the following command:
```bash
docker build \
--build-arg USER=[github_user] \
--build-arg TOKEN=[github_token] \
-t unite.analysis:latest .
```

All application components should run in the same docker network. To create common docker network (if not yet available) run the following command:
```bash
docker network create unite
```

To run application in docker run the following command:
```bash
docker run \
--name unite.analysis \
--restart unless-stopped \
--net unite \
--net-alias analysis.unite.net \
-p 127.0.0.1:5004:80 \
-v /var/run/docker.sock:/var/run/docker.sock \
-v unite.analysis.data:/mnt/data:rw \
-e ASPNETCORE_ENVIRONMENT=Release \
-e UNITE_API_KEY=[api_key] \
-e UNITE_ELASTIC_HOST=http://es.unite.net:9200 \
-e UNITE_ELASTIC_USER=[elasticsearch_user] \
-e UNITE_ELASTIC_PASSWORD=[elasticsearch_password] \
-e UNITE_SQL_HOST=pgsql.unite.net \
-e UNITE_SQL_PORT=5432 \
-e UNITE_SQL_USER=[sql_user] \
-e UNITE_SQL_PASSWORD=[sql_password] \
-e UNITE_MONGO_HOST=mongo.unite.net \
-e UNITE_MONGO_PORT=27017 \
-e UNITE_MONGO_USER=[mongo_user] \
-e UNITE_MONGO_PASSWORD=[mongo_password] \
-e UNITE_ANALYSIS_DATA_PATH=/mnt/data \
-e UNITE_ANALYSIS_IDLE_TIMEOUT=30 \
-e UNITE_ANALYSIS_SURV_HOST=http://surv.analysis.unite.net \
-e UNITE_ANALYSIS_DM_HOST=http://dm.analysis.unite.net \
-e UNITE_ANALYSIS_PCAM_HOST=http://pcam.analysis.unite.net \
-e UNITE_ANALYSIS_DEG_HOST=http://deg.analysis.unite.net \
-e UNITE_ANALYSIS_DEP_HOST=http://dep.analysis.unite.net \
-e UNITE_ANALYSIS_UMAPP_HOST=http://umapp.analysis.unite.net \
-e UNITE_ANALYSIS_CEDP_HOST=http://cedp.analysis.unite.net \
-e UNITE_ANALYSIS_SCELL_HOST=http://scell.analysis.unite.net \
-e UNITE_VIEWER_CXG_IMAGE=ghcr.io/dkfz-unite/unite-cellxgene:latest \
-e UNITE_VIEWER_CXG_NAME=unite.viewer.cxg{n} \
-e UNITE_VIEWER_CXG_ALIAS=cxg{n}.viewer.unite.net \
-e UNITE_VIEWER_CXG_PORTS=54301-54399 \
-d \
unite.analysis:latest
```

The volume `unite.analysis.data` has to be mounted to `/mnt/data` in the runner containers as well.

App will be available at http://localhost:5004.
