# Sample Metadata
Several analyses write metadata of the analysed samples to a TSV file:
- [pcam](./api-analysis-pcam.md) - `metadata.tsv` (with an additional `path` column).
- [umapp](./api-analysis-umapp.md) - `annotations.tsv` (with an additional `sample_key` column).
- [cedp](./api-analysis-cedp.md) - `input/samples.tsv` (with an additional `sample_key` column).
- [scell](./api-analysis-scell.md) - `metadata.tsv` (input of the runner).

The same property keys are used by the [metadata](./api.md#get-apimetadataoptions) endpoints and by the `condition_property`, `class_property` and `track_property` analysis options.

## Columns
A column is written only if at least one sample has a value for it. Missing values of written columns are `Unknown`.

**Sample**
- `sample_key` - internal sample id (only where noted above).
- `sample_id` - readable sample id: `{donor}` for `donors` datasets, `{donor}-{type}-{specimen}` for other datasets, where `{donor}` and `{specimen}` are the donor and specimen ids from the source data and `{type}` is `mat` (material), `lne` (cell line), `org` (organoid), `xen` (xenograft) or `spe`.

**Donor**
- `donor_id`, `donor_age`, `donor_sex`, `donor_diagnosis`, `donor_diagnosis_primary_site`, `donor_diagnosis_localization`, `donor_vital_status`, `donor_progression_status`, `donor_steroids_reactive`, `donor_kps`.

**Image** (MR images)
- `image_id`, `image_type`, `mr_whole_tumor`, `mr_contrast_enhancing`, `mr_non_contrast_enhancing`.

**Specimen**
- `specimen_id`, `specimen_type`, `specimen_category`, `specimen_tumor_type`, `specimen_tumor_grade`, `specimen_tumor_superfamily`, `specimen_tumor_family`, `specimen_tumor_class`, `specimen_tumor_subclass`, `specimen_idh_status`, `specimen_tert_status`, `specimen_mgmt_status`.
- Material: `material_fixation_type`, `material_source`.
- Cell line: `line_cells_species`, `line_cells_type`, `line_cells_culture_type`.
- Organoid: `organoid_medium`, `organoid_implanted_cells_number`, `organoid_tumorigenicity`.
- Xenograft: `xenograft_mouse_strain`, `xenograft_group_size`, `xenograft_implant_type`, `xenograft_implant_location`, `xenograft_implanted_cells_number`, `xenograft_tumorigenicity`, `xenograft_tumor_growth_form`, `xenograft_survival_days`.

## Values
- Boolean values are `Yes`/`No`; IDH and TERT status are `Mutant`/`Wild Type`; MGMT status is `Methylated`/`Unmethylated`.
- Numeric values (`donor_age`, `donor_kps`, MR volumes, implanted cells numbers, xenograft group size) are written as ranges calculated from the values of the analysed samples, e.g. `<14`, `15-28`, `43-56`, `>57`.
- `xenograft_survival_days` is written as `{from}-{to}` range.

```tsv
sample_id	donor_id	donor_age	donor_sex	specimen_id	specimen_type	specimen_category	specimen_tumor_type
D001-mat-S001	D001	43-56	Female	S001	Material	Tumor	Primary
D002-mat-S002	D002	>57	Male	S002	Material	Tumor	Recurrent
```
