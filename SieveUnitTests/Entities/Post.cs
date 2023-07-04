﻿using System;
using RzsSieve.Attributes;
using RzsSieveUnitTests.Abstractions.Entity;

namespace RzsSieveUnitTests.Entities
{
    public class Post : BaseEntity, IPost
    {
        [Sieve(CanFilter = true, CanSort = true)]
        public string Title { get; set; } = Guid.NewGuid().ToString().Replace("-", string.Empty).Substring(0, 8);

        [Sieve(CanFilter = true, CanSort = true)]
        public int LikeCount { get; set; } = new Random().Next(0, 1000);

        [Sieve(CanFilter = true, CanSort = true)]
        public int CommentCount { get; set; } = new Random().Next(0, 1000);

        [Sieve(CanFilter = true, CanSort = true)]
        public int? CategoryId { get; set; } = new Random().Next(0, 4);

        [Sieve(CanFilter = true, CanSort = true)]
        public bool IsDraft { get; set; }

        public string ThisHasNoAttribute { get; set; }

        public string ThisHasNoAttributeButIsAccessible { get; set; }

        public int OnlySortableViaFluentApi { get; set; }

        public Comment TopComment { get; set; }
        public Comment FeaturedComment { get; set; }

        [Sieve(CanFilter = true, CanSort = true)]
        public DateTime? Created { get; set; }
    }
}
