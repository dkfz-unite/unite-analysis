using Microsoft.EntityFrameworkCore;
using Unite.Data.Context;
using Unite.Indices.Entities.Donors;
using Unite.Indices.Search.Services;
using ImageIndex = Unite.Indices.Entities.Images.ImageIndex;
using SpecimenIndex = Unite.Indices.Entities.Specimens.SpecimenIndex;

namespace Unite.Analysis.Services;

public class SamplesContextLoader : GenericSamplesContextLoader<SamplesContext>
{
    public SamplesContextLoader(ISearchService<DonorIndex> donorsSearchService, 
        ISearchService<ImageIndex> imagesSearchService, 
        ISearchService<SpecimenIndex> specimensSearchService, 
        IDbContextFactory<DomainDbContext> dbContextFactory) : base(donorsSearchService, imagesSearchService, specimensSearchService, dbContextFactory)
    {
    }

    protected override SamplesContext BuildContext(SampleType sampleType)
    {
        return new SamplesContext(sampleType);
    }
}
