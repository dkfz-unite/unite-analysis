# Analysis Service

## General
Analysis service provides the following functionality:
- [Analysis web API](Docs/api.md) - REST API for data analysis and analysis tasks.
- Analysis preparation service - service for gathering required data and preparing it for the analysis.
- Analysis execution service - service for executing analysis tasks.

All analyses are 2-step process:
- prepare - getting the data from available sources and preparing it for the analysis.
- execute - executing the analysis on the prepared data.

Both steps are asynchronous and may take significant time to complete. Therefore, each step is running in **10** threads in parallel.

General alogrithm looks as foolowing:
1. User creates analysis task (e.g. DESeq2 analysis).
2. Analysis **preparing** backgroud service prepare the data and puts it to the **shared** directory (This step is optional).
    - **10** different analyses can be prepared at the same time.
    - If there are more than **10** analysis preparation tasks, the rest will be queued.
3. Analysis **processing** background service executes the analysis with prepared data from **shared** directory.
    - Analysis can be performed either by the service itself or remotely (e.g. DESeq2 analysis is performed remotely).
    - **10** different analyses can be executed at the same time.
    - If there are more than **10** analysis execution tasks, the rest will be queued.
4. User can get analysis results from the **shared** directory as soon as the analysis is completed.

There are different ways of scalability here:
- Increase number of threads for analysis preparation and execution (will require more resources from the host machine).
- Scale analysis service to multiple instances (Requires additional configuration, but resources are used more efficiently).
- Scale remote analysis execution services (e.g. DESeq2 analysis can be performed by multiple instances of DESeq2 analysis service).

## Dependencies
- [SQL](https://github.com/dkfz-unite/unite-environment/tree/main/programs/postgresql) - SQL server with domain data and user identity data.
- [Elasticsearch](https://github.com/dkfz-unite/unite-environment/tree/main/programs/elasticsearch) - Elasticsearch server with indices of domain data.

## Access
Environment|Address|Port
-----------|-------|----
Host|http://localhost:5004|5004
Docker|http://analysis.unite.net|80

## Configuration
To configure the application, change environment variables in either docker or [launchSettings.json](https://github.com/dkfz-unite/unite-analysis/blob/main/Unite.Analysis.Web/Properties/launchSettings.json) file (if running locally):

- `ASPNETCORE_ENVIRONMENT` - ASP.NET environment (`Release`).
- `UNITE_API_KEY` - API key for decription of JWT token and user authorization.
- `UNITE_ELASTIC_HOST` - Elasticsearch service host (`http://es.unite.net:9200`).
- `UNITE_ELASTIC_USER` - Elasticsearch service user.
- `UNITE_ELASTIC_PASSWORD` - Elasticsearch service password.
- `UNITE_SQL_HOST` - SQL server host (`sql.unite.net`).
- `UNITE_SQL_PORT` - SQL server port (`5432`).
- `UNITE_SQL_USER` - SQL server user.
- `UNITE_SQL_PASSWORD` - SQL server password.
- `UNITE_ANALYSIS_DATA_PATH` - Path to analysis data directory (`/mnt/data`).
- `UNITE_ANALYSIS_DESEQ2_URL` - Path to the service for DESeq2 analysis (`http://deseq2.analysis.unite.net`).
- `UNITE_ANALYSIS_SCELL_URL` - Path to the service for scRNA dataset creation analysis (`http://scell.analysis.unite.net`).
- `UNITE_ANALYSIS_KMEIER_URL` - Path to the service for Kaplan-Meier survival curve estimation analysis (`http://kmeier.analysis.unite.net`).

> [!NOTE]
> For local development purposes we recommend to use **default** values.

## Installation

### Docker Compose
The easiest way to install the application is to use docker-compose:
- Environment configuration and installation scripts: https://github.com/dkfz-unite/unite-environment
- Analysis service configuration and installation scripts: https://github.com/dkfz-unite/unite-environment/tree/main/applications/unite-analysis

### Docker
The image of the service is available in our [registry](https://github.com/dkfz-unite/unite-analysis/pkgs/container/unite-analysis).

[Dockerfile](./Dockerfile) is used to build an image of the application. To build an image run the following command:
```
docker build -t unite.analysis:latest .
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
-e ASPNETCORE_ENVIRONMENT=Release \
-e UNITE_API_KEY=[api_key] \
-e UNITE_ELASTIC_HOST=http://es.unite.net:9200 \
-e UNITE_ELASTIC_USER=[elasticsearch_user] \
-e UNITE_ELASTIC_PASSWORD=[elasticsearch_password] \
-e UNITE_SQL_HOST=sql.unite.net \
-e UNITE_SQL_PORT=5432 \
-e UNITE_SQL_USER=[sql_user] \
-e UNITE_SQL_PASSWORD=[sql_password] \
-e UNITE_ANALYSIS_DATA_PATH=/mnt/data \
-e UNITE_ANALYSIS_DESEQ2_URL=http://deseq2.analysis.unite.net \
-e UNITE_ANALYSIS_SCELL_URL=http://scell.analysis.unite.net \
-e UNITE_ANALYSIS_KMEIER_URL=http://kmeier.analysis.unite.net \
-d \
unite.analysis:latest
```

App will be available at http://localhost:5004.
