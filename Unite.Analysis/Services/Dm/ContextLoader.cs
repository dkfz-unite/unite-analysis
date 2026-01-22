using Microsoft.EntityFrameworkCore;
using Unite.Data.Context;
using Unite.Indices.Search.Services;

using DonorIndex = Unite.Indices.Entities.Donors.DonorIndex;
using ImageIndex = Unite.Indices.Entities.Images.ImageIndex;
using SpecimenIndex = Unite.Indices.Entities.Specimens.SpecimenIndex;

namespace Unite.Analysis.Services.Dm;

public class ContextLoader : SamplesContextLoader
{
    public ContextLoader(
        ISearchService<DonorIndex> donorsSearchService,
        ISearchService<ImageIndex> imagesSearchService,
        ISearchService<SpecimenIndex> specimensSearchService,
        IDbContextFactory<DomainDbContext> dbContextFactory) : base(donorsSearchService, imagesSearchService, specimensSearchService, dbContextFactory)
    {
    }
}
