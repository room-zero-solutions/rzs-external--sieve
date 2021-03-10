using Microsoft.Extensions.Options;
using RzsSieve.Models;
using RzsSieve.Services;
using RzsSieveTests.Entities;

namespace RzsSieveTests.Services
{
    public class ApplicationSieveProcessor : SieveProcessor
    {
        public ApplicationSieveProcessor(IOptions<SieveOptions> options, ISieveCustomSortMethods customSortMethods, ISieveCustomFilterMethods customFilterMethods) : base(options, customSortMethods, customFilterMethods)
        {
        }

        protected override SievePropertyMapper MapProperties(SievePropertyMapper mapper)
        {
            mapper.Property<Post>(p => p.Title)
                .CanSort()
                .CanFilter()
                .HasName("CustomTitleName");

            return mapper;
        }
    }
}
