using Microsoft.EntityFrameworkCore;
using Unite.Analysis.Models;
using Unite.Analysis.Services.CnvProfile.Models;
using Unite.Data.Context;
using Unite.Data.Entities.Omics.Analysis.Enums;
using Unite.Indices.Entities.Donors;
using Unite.Indices.Search.Services;
using ImageIndex = Unite.Indices.Entities.Images.ImageIndex;
using SpecimenIndex = Unite.Indices.Entities.Specimens.SpecimenIndex;

namespace Unite.Analysis.Services.CnvProfile;

public class ContextLoader : GenericSamplesContextLoader<AnalysisContext>
{
    public ContextLoader(ISearchService<DonorIndex> donorsSearchService, 
        ISearchService<ImageIndex> imagesSearchService, 
        ISearchService<SpecimenIndex> specimensSearchService, 
        IDbContextFactory<DomainDbContext> dbContextFactory) : base(donorsSearchService, imagesSearchService, specimensSearchService, dbContextFactory)
    {
    }

    protected override AnalysisContext BuildContext(SampleType sampleType)
    {
        return new AnalysisContext(sampleType);
    }

    public override Task<AnalysisContext> LoadDatasetData(DatasetCriteria model, params AnalysisType[] analysisTypes)
    {
        return base.LoadDatasetData(model, analysisTypes);
    }
}