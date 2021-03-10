using System;
using RzsSieve.Attributes;
using RzsSieveUnitTests.Abstractions.Entity;

namespace RzsSieveUnitTests.Entities
{
    public class BaseEntity : IBaseEntity
    {
        public int Id { get; set; }

        [Sieve(CanFilter = true, CanSort = true)]
        public DateTimeOffset DateCreated { get; set; } = DateTimeOffset.UtcNow;
    }
}
