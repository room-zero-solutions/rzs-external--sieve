using RzsSieve.Attributes;
using RzsSieveUnitTests.Abstractions.Entity;

namespace RzsSieveUnitTests.Entities
{
    public class Comment : BaseEntity, IComment
    {
        [Sieve(CanFilter = true)]
        public string Text { get; set; }
    }
}
