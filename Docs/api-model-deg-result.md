# DEG Analysis Result
Includes the information about [differential gene expression analysis](./api-analysis-deg.md) result.

Result of the analysis (`results_final.tsv`) is a TSV file with the following columns:
- `geneId` - internal gene ID.
- `geneStableId` - stable gene ID from [Ensembl](https://www.ensembl.org/index.html) database.
- `geneSymbol` - gene symbol from [Ensembl](https://www.ensembl.org/index.html) database (stable ID if the gene has no symbol).
- `log2FoldChange` - log2 fold change, rounded to 3 decimals.
- `pValueAdjusted` - adjusted p-value, rounded to 3 decimals.

```tsv
geneId	geneStableId	geneSymbol	log2FoldChange	pValueAdjusted
1126	ENSG00000139618	BRCA2	-0.512	0.003
```
