using Microsoft.Extensions.Options;
using RzsSieve.Models;

namespace RzsSieveUnitTests.Services
{
    public class SieveOptionsAccessor : IOptions<SieveOptions>
    {
        public SieveOptions Value { get; }

        public SieveOptionsAccessor()
        {
            Value = new SieveOptions()
            {
                ThrowExceptions = true
            };
        }
    }
}
