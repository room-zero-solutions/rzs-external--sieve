﻿using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using RzsSieve.Exceptions;
using RzsSieve.Models;
using RzsSieve.Services;
using RzsSieveUnitTests.Entities;
using RzsSieveUnitTests.Services;

namespace RzsSieveUnitTests
{
    [TestClass]
    public class General
    {
        private readonly SieveProcessor _processor;
        private readonly IQueryable<Post> _posts;
        private readonly IQueryable<Comment> _comments;

        public General()
        {
            _processor = new ApplicationSieveProcessor(new SieveOptionsAccessor(),
                new SieveCustomSortMethods(),
                new SieveCustomFilterMethods());

            _posts = new List<Post>
            {
                new Post() {
                    Id = 0,
                    Title = "A",
                    LikeCount = 100,
                    IsDraft = true,
                    CategoryId = null,
                    TopComment = new Comment { Id = 0, Text = "A1" },
                    FeaturedComment = new Comment { Id = 4, Text = "A2" },
                    Created = DateTime.Parse("2020-01-01")
                },
                new Post() {
                    Id = 1,
                    Title = "B",
                    LikeCount = 50,
                    IsDraft = false,
                    CategoryId = 1,
                    TopComment = new Comment { Id = 3, Text = "B1" },
                    FeaturedComment = new Comment { Id = 5, Text = "B2" },
                    Created = DateTime.Parse("2020-01-01 11:00:00")
                },
                new Post() {
                    Id = 2,
                    Title = "C",
                    LikeCount = 0,
                    CategoryId = 1,
                    TopComment = new Comment { Id = 2, Text = "C1" },
                    FeaturedComment = new Comment { Id = 6, Text = "C2" },
                    Created = DateTime.Parse("2020-01-02")
                },
                new Post() {
                    Id = 3,
                    Title = "D",
                    LikeCount = 3,
                    IsDraft = true,
                    CategoryId = 2,
                    TopComment = new Comment { Id = 1, Text = "D1" },
                    FeaturedComment = new Comment { Id = 7, Text = "D2" },
                    Created = DateTime.Parse("2020-01-02 13:22:22")
                },
            }.AsQueryable();

            _comments = new List<Comment>
            {
                new Comment() {
                    Id = 0,
                    DateCreated = DateTimeOffset.UtcNow.AddDays(-20),
                    Text = "This is an old comment."
                },
                new Comment() {
                    Id = 1,
                    DateCreated = DateTimeOffset.UtcNow.AddDays(-1),
                    Text = "This is a fairly new comment."
                },
                new Comment() {
                    Id = 2,
                    DateCreated = DateTimeOffset.UtcNow,
                    Text = "This is a brand new comment. (Text in braces)"
                },
            }.AsQueryable();
        }
        
        [TestMethod]
        public void DateEqualsMatchesDateTimeWithOrWithoutTimeComponent()
        {
            var model = new SieveModel()
            {
                Filters = "Created#=2020-01-01"
            };

            var result = _processor.Apply(model, _posts);

            Assert.AreEqual(result.ElementAt(0).Id, 0);
            Assert.AreEqual(result.ElementAt(1).Id, 1);
            Assert.IsTrue(result.Count() == 2);
        }

        [TestMethod]
        public void DateGreaterThanOrEqualMatchesDateTimeWithOrWithoutTimeComponent()
        {
            var model = new SieveModel()
            {
                Filters = "Created#>=2020-01-02"
            };

            var result = _processor.Apply(model, _posts);

            Assert.AreEqual(result.ElementAt(0).Id, 2);
            Assert.AreEqual(result.ElementAt(1).Id, 3);
            Assert.IsTrue(result.Count() == 2);
        }
        
        [TestMethod]
        public void DateLessThanOrEqualMatchesDateTimeWithOrWithoutTimeComponent()
        {
            var model = new SieveModel()
            {
                Filters = "Created#<=2020-01-01"
            };

            var result = _processor.Apply(model, _posts);

            Assert.AreEqual(result.ElementAt(0).Id, 0);
            Assert.AreEqual(result.ElementAt(1).Id, 1);
            Assert.IsTrue(result.Count() == 2);
        }

        [TestMethod]
        public void DateGreaterThanMatchesDateTimeWithOrWithoutTimeComponent()
        {
            var model = new SieveModel()
            {
                Filters = "Created#>2020-01-01"
            };

            var result = _processor.Apply(model, _posts);

            Assert.AreEqual(result.ElementAt(0).Id, 2);
            Assert.AreEqual(result.ElementAt(1).Id, 3);
            Assert.IsTrue(result.Count() == 2);
        }

        [TestMethod]
        public void DateLessThanMatchesDateTimeWithOrWithoutTimeComponent()
        {
            var model = new SieveModel()
            {
                Filters = "Created#<2020-01-02"
            };

            var result = _processor.Apply(model, _posts);

            Assert.AreEqual(result.ElementAt(0).Id, 0);
            Assert.AreEqual(result.ElementAt(1).Id, 1);
            Assert.IsTrue(result.Count() == 2);
        }

        [TestMethod]
        public void ContainsCanBeCaseInsensitive()
        {
            var model = new SieveModel()
            {
                Filters = "Title@=*a"
            };

            var result = _processor.Apply(model, _posts);

            Assert.AreEqual(result.First().Id, 0);
            Assert.IsTrue(result.Count() == 1);
        }

        [TestMethod]
        public void NotEqualsCanBeCaseInsensitive()
        {
            var model = new SieveModel()
            {
                Filters = "Title!=*a"
            };

            var result = _processor.Apply(model, _posts);

            Assert.AreEqual(result.First().Id, 1);
            Assert.IsTrue(result.Count() == 3);
        }

        [TestMethod]
        public void ContainsIsCaseSensitive()
        {
            var model = new SieveModel()
            {
                Filters = "Title@=a",
            };

            var result = _processor.Apply(model, _posts);

            Assert.IsTrue(result.Count() == 0);
        }

        [TestMethod]
        public void NotContainsWorks()
        {
            var model = new SieveModel()
            {
                Filters = "Title!@=D",
            };

            var result = _processor.Apply(model, _posts);

            Assert.IsTrue(result.Count() == 3);
        }

        [TestMethod]
        public void CanFilterBools()
        {
            var model = new SieveModel()
            {
                Filters = "IsDraft==false"
            };

            var result = _processor.Apply(model, _posts);

            Assert.IsTrue(result.Count() == 2);
        }

        [TestMethod]
        public void CanSortBools()
        {
            var model = new SieveModel()
            {
                Sorts = "-IsDraft"
            };

            var result = _processor.Apply(model, _posts);

            Assert.AreEqual(result.First().Id, 0);
        }

        [TestMethod]
        public void CanFilterNullableInts()
        {
            var model = new SieveModel()
            {
                Filters = "CategoryId==1"
            };

            var result = _processor.Apply(model, _posts);

            Assert.IsTrue(result.Count() == 2);
        }

        [TestMethod]
        public void EqualsDoesntFailWithNonStringTypes()
        {
            var model = new SieveModel()
            {
                Filters = "LikeCount==50",
            };

            Console.WriteLine(model.GetFiltersParsed()[0].Values);
            Console.WriteLine(model.GetFiltersParsed()[0].Operator);
            Console.WriteLine(model.GetFiltersParsed()[0].OperatorParsed);

            var result = _processor.Apply(model, _posts);

            Assert.AreEqual(result.First().Id, 1);
            Assert.IsTrue(result.Count() == 1);
        }

        [TestMethod]
        public void CustomFiltersWork()
        {
            var model = new SieveModel()
            {
                Filters = "Isnew",
            };

            var result = _processor.Apply(model, _posts);

            Assert.IsFalse(result.Any(p => p.Id == 0));
            Assert.IsTrue(result.Count() == 3);
        }

        [TestMethod]
        public void CustomGenericFiltersWork()
        {
            var model = new SieveModel()
            {
                Filters = "Latest",
            };

            var result = _processor.Apply(model, _comments);

            Assert.IsFalse(result.Any(p => p.Id == 0));
            Assert.IsTrue(result.Count() == 2);
        }

        [TestMethod]
        public void CustomFiltersWithOperatorsWork()
        {
            var model = new SieveModel()
            {
                Filters = "HasInTitle==A",
            };

            var result = _processor.Apply(model, _posts);

            Assert.IsTrue(result.Any(p => p.Id == 0));
            Assert.IsTrue(result.Count() == 1);
        }

        [TestMethod]
        public void CustomFiltersMixedWithUsualWork1()
        {
            var model = new SieveModel()
            {
                Filters = "Isnew,CategoryId==2",
            };

            var result = _processor.Apply(model, _posts);

            Assert.IsTrue(result.Any(p => p.Id == 3));
            Assert.IsTrue(result.Count() == 1);
        }

        [TestMethod]
        public void CustomFiltersMixedWithUsualWork2()
        {
            var model = new SieveModel()
            {
                Filters = "CategoryId==2,Isnew",
            };

            var result = _processor.Apply(model, _posts);

            Assert.IsTrue(result.Any(p => p.Id == 3));
            Assert.IsTrue(result.Count() == 1);
        }

        [TestMethod]
        public void CustomFiltersOnDifferentSourcesCanShareName()
        {
            var postModel = new SieveModel()
            {
                Filters = "CategoryId==2,Isnew",
            };

            var postResult = _processor.Apply(postModel, _posts);

            Assert.IsTrue(postResult.Any(p => p.Id == 3));
            Assert.AreEqual(1, postResult.Count());

            var commentModel = new SieveModel()
            {
                Filters = "Isnew",
            };

            var commentResult = _processor.Apply(commentModel, _comments);

            Assert.IsTrue(commentResult.Any(c => c.Id == 2));
            Assert.AreEqual(2, commentResult.Count());
        }

        [TestMethod]
        public void CustomSortsWork()
        {
            var model = new SieveModel()
            {
                Sorts = "Popularity",
            };

            var result = _processor.Apply(model, _posts);

            Assert.IsFalse(result.First().Id == 0);
        }

        [TestMethod]
        public void CustomGenericSortsWork()
        {
            var model = new SieveModel()
            {
                Sorts = "Oldest",
            };

            var result = _processor.Apply(model, _posts);

            Assert.IsTrue(result.Last().Id == 0);
        }

        [TestMethod]
        public void MethodNotFoundExceptionWork()
        {
            var model = new SieveModel()
            {
                Filters = "does not exist",
            };

            Assert.ThrowsException<SieveMethodNotFoundException>(() => _processor.Apply(model, _posts));
        }

        [TestMethod]
        public void IncompatibleMethodExceptionsWork()
        {
            var model = new SieveModel()
            {
                Filters = "TestComment",
            };

            Assert.ThrowsException<SieveIncompatibleMethodException>(() => _processor.Apply(model, _posts));
        }

        [TestMethod]
        public void OrNameFilteringWorks()
        {
            var model = new SieveModel()
            {
                Filters = "(Title|LikeCount)==3",
            };

            var result = _processor.Apply(model, _posts);
            var entry = result.FirstOrDefault();
            var resultCount = result.Count();

            Assert.IsNotNull(entry);
            Assert.AreEqual(1, resultCount);
            Assert.AreEqual(3, entry.Id);
        }

        [DataTestMethod]
        [DataRow("CategoryId==1,(CategoryId|LikeCount)==50")]
        [DataRow("(CategoryId|LikeCount)==50,CategoryId==1")]
        public void CombinedAndOrFilterIndependentOfOrder(string filter)
        {
            var model = new SieveModel()
            {
                Filters = filter,
            };

            var result = _processor.Apply(model, _posts);
            var entry = result.FirstOrDefault();
            var resultCount = result.Count();

            Assert.IsNotNull(entry);
            Assert.AreEqual(1, resultCount);
        }

        [TestMethod]
        public void CombinedAndOrWithSpaceFilteringWorks()
        {
            var model = new SieveModel()
            {
                Filters = "Title==D, (Title|LikeCount)==3",
            };

            var result = _processor.Apply(model, _posts);
            var entry = result.FirstOrDefault();
            var resultCount = result.Count();

            Assert.IsNotNull(entry);
            Assert.AreEqual(1, resultCount);
            Assert.AreEqual(3, entry.Id);
        }

        [TestMethod]
        public void OrValueFilteringWorks()
        {
            var model = new SieveModel()
            {
                Filters = "Title==C|D",
            };

            var result = _processor.Apply(model, _posts);
            Assert.AreEqual(2, result.Count());
            Assert.IsTrue(result.Any(p => p.Id == 2));
            Assert.IsTrue(result.Any(p => p.Id == 3));
        }

        [TestMethod]
        public void OrValueFilteringWorks2()
        {
            var model = new SieveModel()
            {
                Filters = "Text@=(|)",
            };

            var result = _processor.Apply(model, _comments);
            Assert.AreEqual(1, result.Count());
            Assert.AreEqual(2, result.FirstOrDefault().Id);
        }

        [TestMethod]
        public void NestedFilteringWorks()
        {
            var model = new SieveModel()
            {
                Filters = "TopComment.Text!@=A",
            };

            var result = _processor.Apply(model, _posts);
            Assert.AreEqual(3, result.Count());
            var posts = result.ToList();
            Assert.IsTrue(posts[0].TopComment.Text.Contains("B"));
            Assert.IsTrue(posts[1].TopComment.Text.Contains("C"));
            Assert.IsTrue(posts[2].TopComment.Text.Contains("D"));
        }

        [TestMethod]
        public void NestedSortingWorks()
        {
            var model = new SieveModel()
            {
                Sorts = "TopComment.Id",
            };

            var result = _processor.Apply(model, _posts);
            Assert.AreEqual(4, result.Count());
            var posts = result.ToList();
            Assert.AreEqual(posts[0].Id, 0);
            Assert.AreEqual(posts[1].Id, 3);
            Assert.AreEqual(posts[2].Id, 2);
            Assert.AreEqual(posts[3].Id, 1);
        }

        [TestMethod]
        public void NestedFilteringWithIdenticTypesWorks()
        {
            var model = new SieveModel()
            {
                Filters = "(topc|featc)@=*2",
            };

            var result = _processor.Apply(model, _posts);
            Assert.AreEqual(4, result.Count());

            model = new SieveModel()
            {
                Filters = "(topc|featc)@=*B",
            };

            result = _processor.Apply(model, _posts);
            Assert.AreEqual(1, result.Count());
        }

        [TestMethod]
        public void FilteringNullsWorks()
        {
            var posts = new List<Post>
            {
                new Post() {
                    Id = 1,
                    Title = null,
                    LikeCount = 0,
                    IsDraft = false,
                    CategoryId = null,
                    TopComment = null,
                    FeaturedComment = null
                },
            }.AsQueryable();

            var model = new SieveModel()
            {
                Filters = "FeaturedComment.Text!@=Some value",
            };

            var result = _processor.Apply(model, posts);
            Assert.AreEqual(0, result.Count());
        }

        [TestMethod]
        public void SortingNullsWorks()
        {
            var posts = new List<Post>
            {
                new Post() {
                    Id = 1,
                    Title = null,
                    LikeCount = 0,
                    IsDraft = false,
                    CategoryId = null,
                    TopComment =  new Comment { Id = 1 },
                    FeaturedComment = null
                },
                new Post() {
                    Id = 2,
                    Title = null,
                    LikeCount = 0,
                    IsDraft = false,
                    CategoryId = null,
                    TopComment = null,
                    FeaturedComment = null
                },
            }.AsQueryable();

            var model = new SieveModel()
            {
                Sorts = "TopComment.Id",
            };

            var result = _processor.Apply(model, posts);
            Assert.AreEqual(2, result.Count());
            var sortedPosts = result.ToList();
            Assert.AreEqual(sortedPosts[0].Id, 2);
            Assert.AreEqual(sortedPosts[1].Id, 1);
        }

        [TestMethod]
        public void FilteringOnNullWorks()
        {
            var posts = new List<Post>
            {
                new Post() {
                    Id = 1,
                    Title = null,
                    LikeCount = 0,
                    IsDraft = false,
                    CategoryId = null,
                    TopComment = null,
                    FeaturedComment = null
                },
                new Post() {
                    Id = 2,
                    Title = null,
                    LikeCount = 0,
                    IsDraft = false,
                    CategoryId = null,
                    TopComment = null,
                    FeaturedComment = new Comment {  Id = 1, Text = null }
                },
            }.AsQueryable();

            var model = new SieveModel()
            {
                Filters = "FeaturedComment.Text==null",
            };

            var result = _processor.Apply(model, posts);
            Assert.AreEqual(1, result.Count());
            var filteredPosts = result.ToList();
            Assert.AreEqual(filteredPosts[0].Id, 2);
        }

        [TestMethod]
        public void BaseDefinedPropertyMappingSortingWorks_WithCustomName()
        {
            var model = new SieveModel()
            {
                Sorts = "-CreateDate"
            };

            var result = _processor.Apply(model, _posts);
            Assert.AreEqual(4, result.Count());

            var posts = result.ToList();
            Assert.AreEqual(posts[0].Id, 3);
            Assert.AreEqual(posts[1].Id, 2);
            Assert.AreEqual(posts[2].Id, 1);
            Assert.AreEqual(posts[3].Id, 0);
        }
    }
}
