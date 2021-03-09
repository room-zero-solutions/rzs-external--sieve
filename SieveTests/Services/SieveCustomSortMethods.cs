﻿using System.Linq;
using Sieve.Services;
using SieveTests.Entities;

namespace SieveTests.Services
{
    public class SieveCustomSortMethods : ISieveCustomSortMethods
    {
        public IQueryable<Post> Popularity(IQueryable<Post> source, bool useThenBy) => useThenBy
            ? ((IOrderedQueryable<Post>)source).ThenBy(p => p.LikeCount)
            : source.OrderBy(p => p.LikeCount)
                .ThenBy(p => p.CommentCount)
                .ThenBy(p => p.DateCreated);
    }
}
