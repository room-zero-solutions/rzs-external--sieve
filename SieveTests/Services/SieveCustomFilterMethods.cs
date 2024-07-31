using System;
using System.Linq;
using System.Linq.Expressions;
using RzsSieve.Services;
using RzsSieveTests.Entities;

namespace RzsSieveTests.Services
{
    public class SieveCustomFilterMethods : ISieveCustomFilterMethods
    {
        public IQueryable<Post> IsNew(IQueryable<Post> source, string op, string[] values)
            => source.Where(p => p.LikeCount < 100 && p.CommentCount < 5);
    }
}
