using System;

namespace RzsSieveUnitTests.Abstractions.Entity
{
    public interface IBaseEntity
    {
        int Id { get; set; }
        DateTimeOffset DateCreated { get; set; }
    }
}
