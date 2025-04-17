# DEXP Analysis Task Result
Includes the information about differential expression analysis task result.

Result of the analysis task is a TSV file with the following columns:
- `geneId` - internal gene ID.
- `geneStableId` - stable gene ID from [Ensembl](https://www.ensembl.org/index.html) database.
- `geneSymbol` - well known gene name from [Ensembl](https://www.ensembl.org/index.html) database.
- `log2FoldChange` - log2 fold change.
- `pValueAdjusted` - adjusted p-value.

```tsv
geneId	geneStableId	geneSymbol	log2FoldChange	pValueAdjusted
1126	ENSG00000139618	ACAP3	-0.0000000000     1.0000000000
```
