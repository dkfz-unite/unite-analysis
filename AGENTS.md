# UNITE repository instructions

Before changing this repository, read the shared UNITE instructions and
relevant project documentation maintained in
[dkfz-unite/.github](https://github.com/dkfz-unite/.github/blob/main/README.md).

1. Prefer the local shared checkout: `../unite-meta/AGENTS.md`, relative to this
   repository root. A sibling `.github/AGENTS.md` or a user-provided path is also
   valid. Resolve its document links relative to that shared checkout.
2. Read the shared `AGENTS.md`, project overview, repository map, and topic
   documents relevant to the task, following the shared README's task index.
3. If no shared checkout is accessible, use the GitHub versions beginning with
   [shared AGENTS.md](https://github.com/dkfz-unite/.github/blob/main/AGENTS.md).
   If those are unavailable too, report the missing context and proceed only
   with what local documentation/code establishes; do not invent shared rules.
4. Read this repository's README and relevant local docs; inspect Git status.
   More specific instructions here supplement the shared guidance.

Keep common rules in `.github`; keep component details in this repository.

## Analysis

Status: reviewed with the owner (2026-10-12).

`unite-analysis` runs downstream analyses of saved datasets: it records each
analysis, prepares input data from the search engine and SQL, runs the method
internally or in a separate runner container, packages results, and starts
Cell-x-Gene viewers for single-cell datasets. It does not ingest data or serve
search. The full workflow (Prepare/Process, statuses, runner capacity, file
contracts per method, viewer lifecycle) is in
[Analysis pipeline](../unite-meta/docs/analysis-pipeline.md); read it first.

### Projects

| Project | Contents |
| --- | --- |
| `Unite.Analysis` | One folder per method in `Services/` (`Surv`, `Deg`, `Dm`, `Pcam`, `Dep`, `Umapp`, `Cedp`, `Gaf`, `CnvProfile`, `Scell`), each with an `AnalysisService<TModel>` and its loaders/writers; shared dataset loading (`SamplesContextLoader*`, `SampleMetadata*`); request models (`Models/`) |
| `Unite.Analysis.Web` | Controllers (`AnalysisController` per method, `AnalysesController` list/delete-all, `ViewerController`), task service, preparing/processing/monitoring handlers and workers, Mongo repository for analysis records, options |
| `Unite.Orchestrator` | Docker client and `CxgViewerService`: starts, reuses and removes Cell-x-Gene viewer containers |

There are no test projects.

### Build and run

```sh
dotnet build Unite.Analysis.sln
dotnet run --project Unite.Analysis.Web --launch-profile http
```

- Local URL `http://localhost:5004`; Docker `analysis.unite.net`. Needs
  PostgreSQL, MongoDB, Elasticsearch, the runner containers of the methods being
  tested, the shared analysis data directory (`UNITE_ANALYSIS_DATA_PATH`,
  `/mnt/data` in Docker, volume `unite.analysis.data` shared with the runners),
  and access to the Docker socket for viewers (mounted in unite-env).
- To test locally, stop the `unite.analysis` container first; see
  [Running a service locally](../unite-meta/docs/development-validation.md#running-a-service-locally-instead-of-its-container).
  The preparing and processing workers start too and pick up queued analyses.
- Runners are separate repositories (`unite-analysis-<method>`) built around
  `unite-commands`; see the runner contracts in the analysis pipeline document.

### Configuration and API

Environment variables are in the [README](README.md) (runner hosts are defined
in `Configuration/Options/AnalysisOptions.cs`); the API is in [Docs/api.md](Docs/api.md)
with method pages. Developer-specific notes:

- `Docs/` has one page per method (`api-analysis-<method>.md`) and shared
  models; update the method page with every change to its options or results.
- The user scope of an analysis comes from the token: `RunTask` copies the user
  ID and root flag into every dataset's `DatasetCriteria`, and the loaders call
  `Stats(PersonalSearchCriteria)`. Never take them from the request.

### Adding or changing an analysis method

Methods are not uniform yet. The goal is one template with a ready API that
delivers requested inputs (e.g. gene expressions plus sample metadata) and
fixed places for results and visualizations, so new analyses integrate with
little code; prefer changes that move toward it. Follow the
[intended structure](../unite-meta/docs/analysis-pipeline.md#intended-structure-for-analyses)
(inputs in `input/`, results in `output/`, cleanup and archiving only after
success, busy runner means retry later) and use the most recent methods, such
as CEDP, as the reference rather than older ones.

| Change | Location |
| --- | --- |
| New method | `AnalysisTaskType` (`unite-data`) and migration; `Unite.Analysis/Services/<Method>/` (service with `Prepare`/`Process`, loaders, writers, `Load`/`Download`/`Delete`); request model; endpoint in `AnalysisController`; branches in the preparing/processing handlers, `Load`/`Download`/`Delete`, and `AnalysesController` delete-all; runner host option; runner repository and unite-env application; portal `analysis/components/<method>` and `AnalysisButton`; docs |
| Input/output contract | The method's service, the runner script and the portal result parser together; see "File contracts" in the analysis pipeline document |
| Dataset selection | `SamplesContextLoader*` (shared), or the method's own loader (DEG, survival) |
| Viewer behavior | `Unite.Orchestrator/CxgViewerService.cs`, `ViewerController`, `DockerMonitoringHandler` |

### Shared libraries

`Unite.Data.Context` 12.10.14 (latest 12.10.33), `Unite.Indices.Search` 13.10.19,
`Unite.Cache` 1.0.3 (latest 1.3.0), Docker.DotNet for the orchestrator.

### CI

`docker-images.yml` builds amd64 and arm64 images under one manifest on pushes
to `main`.

### Known issues

Defects are tracked in the private Jira; see the shared
[Track defects](../unite-meta/AGENTS.md#track-defects).
