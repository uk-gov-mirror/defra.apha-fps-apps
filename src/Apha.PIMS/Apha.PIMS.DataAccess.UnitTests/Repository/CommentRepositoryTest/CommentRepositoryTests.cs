using Apha.Common.Helpers.Repository;
using Apha.PIMS.Core.Entities;
using Apha.PIMS.Core.Pagination;
using Apha.PIMS.DataAccess.Data;
using Apha.PIMS.DataAccess.Repository;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Query;
using Moq;
using System.Linq.Expressions;

namespace Apha.PIMS.DataAccess.UnitTests.Repository.CommentRepositoryTest
{
    public class CommentRepositoryTests
    {
       
        private static CommentRepository CreateRepository(
            IEnumerable<Comment>? comments = null,
            IEnumerable<CommentTopic>? commentTopics = null,
            IEnumerable<ProjectRadTrackData>? projectRadTrackData = null)
        {
            var mockContext = RepositoryTestHelper.CreateMockDbContext<PimsDbContext>();

            var commentsMockSet = RepositoryTestHelper.CreateMockDbSet(comments ?? Enumerable.Empty<Comment>());
            RepositoryTestHelper.SetupDbSetOperations(commentsMockSet);
            RepositoryTestHelper.SetupSaveChanges(mockContext);
            mockContext.Setup(x => x.Comments).Returns(commentsMockSet.Object);

            
            var commentTopicsMockSet = RepositoryTestHelper.CreateMockDbSet(commentTopics ?? Enumerable.Empty<CommentTopic>());
            RepositoryTestHelper.SetupDbSetOperations(commentTopicsMockSet);
            mockContext.Setup(x => x.CommentTopics).Returns(commentTopicsMockSet.Object);

            
            var projectRadTrackDataMockSet = RepositoryTestHelper.CreateMockDbSet(projectRadTrackData ?? Enumerable.Empty<ProjectRadTrackData>());
            RepositoryTestHelper.SetupDbSetOperations(projectRadTrackDataMockSet);
            mockContext.Setup(x => x.ProjectRadTrackData).Returns(projectRadTrackDataMockSet.Object);

            return new CommentRepository(mockContext.Object);
        }

       
        private static (
            CommentRepository Repo,
            Mock<DbSet<Comment>> CommentsDbSet,
            Mock<PimsDbContext> Context)
            CreateRepositoryWithMocks(
                IEnumerable<Comment>? comments = null,
                IEnumerable<CommentTopic>? commentTopics = null,
                IEnumerable<ProjectRadTrackData>? projectRadTrackData = null)
        {
            var mockContext = RepositoryTestHelper.CreateMockDbContext<PimsDbContext>();

            var commentsMockSet = RepositoryTestHelper.CreateMockDbSet(comments ?? Enumerable.Empty<Comment>());
            RepositoryTestHelper.SetupDbSetOperations(commentsMockSet);
            RepositoryTestHelper.SetupSaveChanges(mockContext);
            mockContext.Setup(x => x.Comments).Returns(commentsMockSet.Object);

           
            var commentTopicsMockSet = RepositoryTestHelper.CreateMockDbSet(commentTopics ?? Enumerable.Empty<CommentTopic>());
            RepositoryTestHelper.SetupDbSetOperations(commentTopicsMockSet);
            mockContext.Setup(x => x.CommentTopics).Returns(commentTopicsMockSet.Object);

            
            var projectRadTrackDataMockSet = RepositoryTestHelper.CreateMockDbSet(projectRadTrackData ?? Enumerable.Empty<ProjectRadTrackData>());
            RepositoryTestHelper.SetupDbSetOperations(projectRadTrackDataMockSet);
            mockContext.Setup(x => x.ProjectRadTrackData).Returns(projectRadTrackDataMockSet.Object);

            var repo = new CommentRepository(mockContext.Object);
            return (repo, commentsMockSet, mockContext);
        }

        private static CommentRepository CreateRepositoryForForecastSpend(IEnumerable<ProjectRadTrackData> projectRadTrackData)
        {
            var mockContext = RepositoryTestHelper.CreateMockDbContext<PimsDbContext>();
            var queryable = projectRadTrackData.AsQueryable();

            var projectRadTrackDataMockSet = new Mock<DbSet<ProjectRadTrackData>>();
            projectRadTrackDataMockSet.As<IQueryable<ProjectRadTrackData>>().Setup(m => m.Provider)
                .Returns(new ForecastAsyncQueryProvider<ProjectRadTrackData>(queryable.Provider));
            projectRadTrackDataMockSet.As<IQueryable<ProjectRadTrackData>>().Setup(m => m.Expression).Returns(queryable.Expression);
            projectRadTrackDataMockSet.As<IQueryable<ProjectRadTrackData>>().Setup(m => m.ElementType).Returns(queryable.ElementType);
            projectRadTrackDataMockSet.As<IQueryable<ProjectRadTrackData>>().Setup(m => m.GetEnumerator()).Returns(() => queryable.GetEnumerator());
            projectRadTrackDataMockSet.As<IAsyncEnumerable<ProjectRadTrackData>>()
                .Setup(m => m.GetAsyncEnumerator(It.IsAny<CancellationToken>()))
                .Returns(() => new ForecastAsyncEnumerator<ProjectRadTrackData>(queryable.GetEnumerator()));

            mockContext.Setup(x => x.ProjectRadTrackData).Returns(projectRadTrackDataMockSet.Object);

            return new CommentRepository(mockContext.Object);
        }

        private sealed class ForecastAsyncQueryProvider<TEntity> : IAsyncQueryProvider
        {
            private readonly IQueryProvider _inner;

            public ForecastAsyncQueryProvider(IQueryProvider inner)
            {
                _inner = inner;
            }

            public IQueryable CreateQuery(Expression expression)
            {
                var elementType = expression.Type.GetGenericArguments().First();
                return (IQueryable)Activator.CreateInstance(typeof(ForecastAsyncEnumerable<>).MakeGenericType(elementType), expression)!;
            }

            public IQueryable<TElement> CreateQuery<TElement>(Expression expression)
            {
                return new ForecastAsyncEnumerable<TElement>(expression);
            }

            public object? Execute(Expression expression)
            {
                return _inner.Execute(expression);
            }

            public TResult Execute<TResult>(Expression expression)
            {
                return _inner.Execute<TResult>(expression);
            }

            public TResult ExecuteAsync<TResult>(Expression expression, CancellationToken cancellationToken = default)
            {
                var resultType = typeof(TResult).GetGenericArguments().First();
                var executeMethod = typeof(IQueryProvider)
                    .GetMethods()
                    .First(m => m.Name == nameof(IQueryProvider.Execute) && m.IsGenericMethodDefinition)
                    .MakeGenericMethod(resultType);

                var executionResult = executeMethod.Invoke(_inner, new object[] { expression });

                return (TResult)typeof(Task)
                    .GetMethod(nameof(Task.FromResult))!
                    .MakeGenericMethod(resultType)
                    .Invoke(null, new[] { executionResult })!;
            }
        }

        private sealed class ForecastAsyncEnumerable<T> : EnumerableQuery<T>, IAsyncEnumerable<T>, IQueryable<T>
        {
            public ForecastAsyncEnumerable(IEnumerable<T> enumerable)
                : base(enumerable)
            {
            }

            public ForecastAsyncEnumerable(Expression expression)
                : base(expression)
            {
            }

            public IAsyncEnumerator<T> GetAsyncEnumerator(CancellationToken cancellationToken = default)
            {
                return new ForecastAsyncEnumerator<T>(this.AsEnumerable().GetEnumerator());
            }

            IQueryProvider IQueryable.Provider => new ForecastAsyncQueryProvider<T>(this);
        }

        private sealed class ForecastAsyncEnumerator<T> : IAsyncEnumerator<T>
        {
            private readonly IEnumerator<T> _inner;

            public ForecastAsyncEnumerator(IEnumerator<T> inner)
            {
                _inner = inner;
            }

            public T Current => _inner.Current;

            public ValueTask DisposeAsync()
            {
                _inner.Dispose();
                return ValueTask.CompletedTask;
            }

            public ValueTask<bool> MoveNextAsync()
            {
                return ValueTask.FromResult(_inner.MoveNext());
            }
        }

        #region GetCommentsByProjectAsync — project filter

        [Fact]
        public async Task GetCommentsByProjectAsync_ReturnsAllMatchingComments_WhenProjectExists()
        {
            // Arrange
            var comments = new List<Comment>
            {
                new() { CommentNo = 1, Project = "PP001", Year = 2023, Topic = "Topic1" },
                new() { CommentNo = 2, Project = "PP001", Year = 2024, Topic = "Topic2" },
                new() { CommentNo = 3, Project = "PP002", Year = 2024, Topic = "Topic3" }
            };
            var repo = CreateRepository(comments: comments);
            var query = new PaginationParameters<string> { Page = 1, PageSize = 10 };

            // Act
            var result = await repo.GetCommentsByProjectAsync("PP001", null, query);

            // Assert
            Assert.NotNull(result);
            Assert.Equal(2, result.Data.Count);
            Assert.All(result.Data, c => Assert.Equal("PP001", c.Project));
        }

        [Fact]
        public async Task GetCommentsByProjectAsync_ReturnsEmpty_WhenNoCommentsMatchProject()
        {
            // Arrange
            var comments = new List<Comment>
            {
                new() { CommentNo = 1, Project = "PP002", Year = 2024, Topic = "Topic1" }
            };
            var repo = CreateRepository(comments: comments);
            var query = new PaginationParameters<string> { Page = 1, PageSize = 10 };

            // Act
            var result = await repo.GetCommentsByProjectAsync("PP001", null, query);

            // Assert
            Assert.NotNull(result);
            Assert.Empty(result.Data);
            Assert.Equal(0, result.PaginationData.TotalRecords);
        }

        [Fact]
        public async Task GetCommentsByProjectAsync_ReturnsEmpty_WhenCommentsDbSetIsEmpty()
        {
            // Arrange
            var repo = CreateRepository(comments: new List<Comment>());
            var query = new PaginationParameters<string> { Page = 1, PageSize = 10 };

            // Act
            var result = await repo.GetCommentsByProjectAsync("PP001", null, query);

            // Assert
            Assert.NotNull(result);
            Assert.Empty(result.Data);
        }

        [Fact]
        public async Task GetCommentsByProjectAsync_DoesNotReturnOtherProjects()
        {
            // Arrange
            var comments = new List<Comment>
            {
                new() { CommentNo = 1, Project = "PP001", Year = 2024, Topic = "A" },
                new() { CommentNo = 2, Project = "PP002", Year = 2024, Topic = "B" },
                new() { CommentNo = 3, Project = "PP003", Year = 2024, Topic = "C" }
            };
            var repo = CreateRepository(comments: comments);
            var query = new PaginationParameters<string> { Page = 1, PageSize = 10 };

            // Act
            var result = await repo.GetCommentsByProjectAsync("PP001", null, query);

            // Assert
            Assert.Single(result.Data);
            Assert.Equal("PP001", result.Data.First().Project);
        }

        #endregion

        #region GetCommentsByProjectAsync — year filter

        [Fact]
        public async Task GetCommentsByProjectAsync_FiltersByYear_WhenYearIsProvided()
        {
            // Arrange
            var comments = new List<Comment>
            {
                new() { CommentNo = 1, Project = "PP001", Year = 2022, Topic = "Topic1" },
                new() { CommentNo = 2, Project = "PP001", Year = 2023, Topic = "Topic2" },
                new() { CommentNo = 3, Project = "PP001", Year = 2024, Topic = "Topic3" }
            };
            var repo = CreateRepository(comments: comments);
            var query = new PaginationParameters<string> { Page = 1, PageSize = 10 };

            // Act
            var result = await repo.GetCommentsByProjectAsync("PP001", 2023, query);

            // Assert
            Assert.Single(result.Data);
            Assert.Equal((short)2023, result.Data.First().Year);
            Assert.Equal(2, result.Data.First().CommentNo);
        }

        [Fact]
        public async Task GetCommentsByProjectAsync_ReturnsAllYears_WhenYearIsNull()
        {
            // Arrange
            var comments = new List<Comment>
            {
                new() { CommentNo = 1, Project = "PP001", Year = 2022, Topic = "Topic1" },
                new() { CommentNo = 2, Project = "PP001", Year = 2023, Topic = "Topic2" },
                new() { CommentNo = 3, Project = "PP001", Year = 2024, Topic = "Topic3" }
            };
            var repo = CreateRepository(comments: comments);
            var query = new PaginationParameters<string> { Page = 1, PageSize = 10 };

            // Act
            var result = await repo.GetCommentsByProjectAsync("PP001", null, query);

            // Assert
            Assert.Equal(3, result.Data.Count);
            Assert.Equal((short)2024, result.Data.First().Year);
            Assert.Equal((short)2022, result.Data.Last().Year);
        }

        [Fact]
        public async Task GetCommentsByProjectAsync_ReturnsEmpty_WhenYearMatchesNoComments()
        {
            // Arrange
            var comments = new List<Comment>
            {
                new() { CommentNo = 1, Project = "PP001", Year = 2022, Topic = "Topic1" },
                new() { CommentNo = 2, Project = "PP001", Year = 2023, Topic = "Topic2" }
            };
            var repo = CreateRepository(comments: comments);
            var query = new PaginationParameters<string> { Page = 1, PageSize = 10 };

            // Act
            var result = await repo.GetCommentsByProjectAsync("PP001", 2099, query);

            // Assert
            Assert.Empty(result.Data);
        }

        [Fact]
        public async Task GetCommentsByProjectAsync_ReturnsMultipleComments_WhenMultipleMatchProjectAndYear()
        {
            // Arrange
            var comments = new List<Comment>
            {
                new() { CommentNo = 1, Project = "PP001", Year = 2024, Topic = "Topic1" },
                new() { CommentNo = 2, Project = "PP001", Year = 2024, Topic = "Topic2" },
                new() { CommentNo = 3, Project = "PP001", Year = 2023, Topic = "Topic3" }
            };
            var repo = CreateRepository(comments: comments);
            var query = new PaginationParameters<string> { Page = 1, PageSize = 10 };

            // Act
            var result = await repo.GetCommentsByProjectAsync("PP001", 2024, query);

            // Assert
            Assert.Equal(2, result.Data.Count);
            Assert.All(result.Data, c => Assert.Equal((short)2024, c.Year));
        }

        #endregion

        #region GetCommentsByProjectAsync — topic filter

        
        //   GetCommentsByProjectAsync in Phase 4 (ICommentRepository / CommentRepository update)

        [Fact]
        public async Task GetCommentsByProjectAsync_FiltersByTopic_WhenTopicIsProvided()
        {
            // Arrange
            var comments = new List<Comment>
            {
                new() { CommentNo = 1, Project = "PP001", Year = 2024, Topic = "Safety"   },
                new() { CommentNo = 2, Project = "PP001", Year = 2024, Topic = "Finance"  },
                new() { CommentNo = 3, Project = "PP001", Year = 2024, Topic = "Safety"   },
                new() { CommentNo = 4, Project = "PP001", Year = 2024, Topic = "Planning" }
            };
            var repo = CreateRepository(comments: comments);
            var query = new PaginationParameters<string> { Page = 1, PageSize = 10 };

            // Act
            var result = await repo.GetCommentsByProjectAsync("PP001", null, query, topic: "Safety");

            // Assert
            Assert.Equal(2, result.Data.Count);
            Assert.All(result.Data, c => Assert.Equal("Safety", c.Topic));
        }

        [Fact]
        public async Task GetCommentsByProjectAsync_ReturnsAllTopics_WhenTopicIsNull()
        {
            // Arrange
            var comments = new List<Comment>
            {
                new() { CommentNo = 1, Project = "PP001", Year = 2024, Topic = "Safety"  },
                new() { CommentNo = 2, Project = "PP001", Year = 2024, Topic = "Finance" },
                new() { CommentNo = 3, Project = "PP001", Year = 2024, Topic = "Safety"  }
            };
            var repo = CreateRepository(comments: comments);
            var query = new PaginationParameters<string> { Page = 1, PageSize = 10 };

            // Act — pass topic: null (default) — all records should be returned
            var result = await repo.GetCommentsByProjectAsync("PP001", null, query, topic: null);

            // Assert
            Assert.Equal(3, result.Data.Count);
        }

        [Fact]
        public async Task GetCommentsByProjectAsync_ReturnsEmpty_WhenTopicMatchesNoComments()
        {
            // Arrange
            var comments = new List<Comment>
            {
                new() { CommentNo = 1, Project = "PP001", Year = 2024, Topic = "Safety"  },
                new() { CommentNo = 2, Project = "PP001", Year = 2024, Topic = "Finance" }
            };
            var repo = CreateRepository(comments: comments);
            var query = new PaginationParameters<string> { Page = 1, PageSize = 10 };

            // Act
            var result = await repo.GetCommentsByProjectAsync("PP001", null, query, topic: "Engineering");

            // Assert
            Assert.Empty(result.Data);
            Assert.Equal(0, result.PaginationData.TotalRecords);
        }

        [Fact]
        public async Task GetCommentsByProjectAsync_FiltersByTopicAndYear_WhenBothAreProvided()
        {
            // Arrange
            var comments = new List<Comment>
            {
                new() { CommentNo = 1, Project = "PP001", Year = 2023, Topic = "Safety"  },
                new() { CommentNo = 2, Project = "PP001", Year = 2024, Topic = "Safety"  },
                new() { CommentNo = 3, Project = "PP001", Year = 2024, Topic = "Finance" },
                new() { CommentNo = 4, Project = "PP001", Year = 2023, Topic = "Finance" }
            };
            var repo = CreateRepository(comments: comments);
            var query = new PaginationParameters<string> { Page = 1, PageSize = 10 };

            // Act
            var result = await repo.GetCommentsByProjectAsync("PP001", 2024, query, topic: "Safety");

            // Assert
            Assert.Single(result.Data);
            Assert.Equal(2, result.Data.First().CommentNo);
            Assert.Equal((short)2024, result.Data.First().Year);
            Assert.Equal("Safety", result.Data.First().Topic);
        }

        [Fact]
        public async Task GetCommentsByProjectAsync_ReturnsAllTopics_WhenTopicIsEmptyString()
        {
            // Arrange — empty string is treated as "no filter" by IsNullOrEmpty guard in repository
            var comments = new List<Comment>
            {
                new() { CommentNo = 1, Project = "PP001", Year = 2024, Topic = "Safety"  },
                new() { CommentNo = 2, Project = "PP001", Year = 2024, Topic = "Finance" }
            };
            var repo = CreateRepository(comments: comments);
            var query = new PaginationParameters<string> { Page = 1, PageSize = 10 };

            // Act
            var result = await repo.GetCommentsByProjectAsync("PP001", null, query, topic: string.Empty);

            // Assert — empty string must not filter (IsNullOrEmpty returns true)
            Assert.Equal(2, result.Data.Count);
        }

        #endregion

        #region GetCommentsByProjectAsync — ApplySorting

        [Theory]
        [InlineData("CommentNo", false)]
        [InlineData("CommentNo", true)]
        [InlineData("topic", false)]
        [InlineData("topic", true)]
        [InlineData("year", false)]
        [InlineData("year", true)]
        [InlineData("MadeBy", false)]
        [InlineData("MadeBy", true)]
        [InlineData("project", false)]
        [InlineData("project", true)]
        public async Task GetCommentsByProjectAsync_WithSorting_ReturnsSortedResults(
            string sortBy, bool descending)
        {
            // Arrange — source list is intentionally unsorted to prove sorting takes effect
            var comments = new List<Comment>
            {
                new() { CommentNo = 2, Project = "PP001", Year = 2023, Topic = "Beta",  MadeBy = "Bob"     },
                new() { CommentNo = 1, Project = "PP001", Year = 2022, Topic = "Alpha", MadeBy = "Alice"   },
                new() { CommentNo = 3, Project = "PP001", Year = 2024, Topic = "Gamma", MadeBy = "Charlie" }
            };
            var repo = CreateRepository(comments: comments);
            var query = new PaginationParameters<string>
            {
                Page = 1,
                PageSize = 10,
                SortBy = sortBy,
                Descending = descending
            };

            // Act
            var result = await repo.GetCommentsByProjectAsync("PP001", null, query);

            // Assert
            Assert.Equal(3, result.Data.Count);
            var first = result.Data.First();

            switch (sortBy.ToLower())
            {
                case "commentno":
                    Assert.Equal(descending ? 3 : 1, first.CommentNo);
                    break;
                case "topic":
                    Assert.Equal(descending ? "Gamma" : "Alpha", first.Topic);
                    break;
                case "year":
                    Assert.Equal(descending ? (short)2024 : (short)2022, first.Year);
                    break;
                case "madeby":
                    Assert.Equal(descending ? "Charlie" : "Alice", first.MadeBy);
                    break;
                case "project":
                    // All results share the same project value; sorting by project is a no-op —
                    // verify the code path runs without error and all records are returned.
                    Assert.Equal(3, result.Data.Count);
                    break;
            }
        }

        [Theory]
        [InlineData(false)]
        [InlineData(true)]
        public async Task GetCommentsByProjectAsync_SortByDateEntered_ReturnsSortedResults(bool descending)
        {
            // Arrange
            var comments = new List<Comment>
            {
                new() { CommentNo = 2, Project = "PP001", Year = 2023, Topic = "Beta",  DateEntered = new DateTime(2023, 6,  1) },
                new() { CommentNo = 1, Project = "PP001", Year = 2022, Topic = "Alpha", DateEntered = new DateTime(2022, 1,  1) },
                new() { CommentNo = 3, Project = "PP001", Year = 2024, Topic = "Gamma", DateEntered = new DateTime(2024, 12, 1) }
            };
            var repo = CreateRepository(comments: comments);
            var query = new PaginationParameters<string>
            {
                Page = 1,
                PageSize = 10,
                SortBy = "DateEntered",
                Descending = descending
            };

            // Act
            var result = await repo.GetCommentsByProjectAsync("PP001", null, query);

            // Assert
            Assert.Equal(3, result.Data.Count);
            if (descending)
                Assert.Equal(new DateTime(2024, 12, 1), result.Data.First().DateEntered);
            else
                Assert.Equal(new DateTime(2022, 1, 1), result.Data.First().DateEntered);
        }

        [Fact]
        public async Task GetCommentsByProjectAsync_WithNullSortBy_ReturnsResultsInDefaultOrder()
        {
            // Arrange
            var comments = new List<Comment>
            {
                new() { CommentNo = 3, Project = "PP001", Year = 2024, Topic = "Gamma" },
                new() { CommentNo = 1, Project = "PP001", Year = 2022, Topic = "Alpha" },
                new() { CommentNo = 2, Project = "PP001", Year = 2023, Topic = "Beta"  }
            };
            var repo = CreateRepository(comments: comments);
            var query = new PaginationParameters<string> { Page = 1, PageSize = 10, SortBy = null };

            // Act
            var result = await repo.GetCommentsByProjectAsync("PP001", null, query);

            // Assert
            Assert.Equal(3, result.Data.Count);
        }

        [Fact]
        public async Task GetCommentsByProjectAsync_WithEmptySortBy_ReturnsResultsInDefaultOrder()
        {
            // Arrange
            var comments = new List<Comment>
            {
                new() { CommentNo = 1, Project = "PP001", Year = 2023, Topic = "Alpha" },
                new() { CommentNo = 2, Project = "PP001", Year = 2024, Topic = "Beta"  }
            };
            var repo = CreateRepository(comments: comments);
            var query = new PaginationParameters<string> { Page = 1, PageSize = 10, SortBy = string.Empty };

            // Act
            var result = await repo.GetCommentsByProjectAsync("PP001", null, query);

            // Assert
            Assert.Equal(2, result.Data.Count);
        }

        [Fact]
        public async Task GetCommentsByProjectAsync_WithInvalidSortBy_ReturnsResultsInDefaultOrder()
        {
            // Arrange
            var comments = new List<Comment>
            {
                new() { CommentNo = 1, Project = "PP001", Year = 2023, Topic = "Alpha" },
                new() { CommentNo = 2, Project = "PP001", Year = 2024, Topic = "Beta"  }
            };
            var repo = CreateRepository(comments: comments);
            var query = new PaginationParameters<string> { Page = 1, PageSize = 10, SortBy = "invalid_field" };

            // Act
            var result = await repo.GetCommentsByProjectAsync("PP001", null, query);

            // Assert
            Assert.Equal(2, result.Data.Count);
        }

        #endregion

        #region GetCommentsByProjectAsync — ApplyPaging

        [Fact]
        public async Task GetCommentsByProjectAsync_WithPaging_ReturnsCorrectPage()
        {
            // Arrange
            var comments = Enumerable.Range(1, 5)
                .Select(i => new Comment { CommentNo = i, Project = "PP001", Year = (short)(2020 + i), Topic = $"Topic{i}" })
                .ToList();
            var repo = CreateRepository(comments: comments);
            var query = new PaginationParameters<string> { Page = 2, PageSize = 2 };

            // Act
            var result = await repo.GetCommentsByProjectAsync("PP001", null, query);

            // Assert
            Assert.Equal(2, result.Data.Count);
            Assert.Equal(5, result.PaginationData.TotalRecords);
            Assert.Equal(2, result.PaginationData.PageNumber);
            Assert.Equal(2, result.PaginationData.PageSize);
            Assert.Equal(3, result.PaginationData.TotalPages);
        }

        [Fact]
        public async Task GetCommentsByProjectAsync_ReturnsCorrectPaginationMetadata()
        {
            // Arrange
            var comments = new List<Comment>
            {
                new() { CommentNo = 1, Project = "PP001", Year = 2023, Topic = "Topic1" },
                new() { CommentNo = 2, Project = "PP001", Year = 2024, Topic = "Topic2" },
                new() { CommentNo = 3, Project = "PP001", Year = 2024, Topic = "Topic3" }
            };
            var repo = CreateRepository(comments: comments);
            var query = new PaginationParameters<string> { Page = 1, PageSize = 10 };

            // Act
            var result = await repo.GetCommentsByProjectAsync("PP001", null, query);

            // Assert
            Assert.Equal(3, result.PaginationData.TotalRecords);
            Assert.Equal(1, result.PaginationData.PageNumber);
            Assert.Equal(10, result.PaginationData.PageSize);
            Assert.Equal(1, result.PaginationData.TotalPages);
        }

        #endregion

        #region GetByIdAsync

        [Fact]
        public async Task GetByIdAsync_ReturnsComment_WhenCommentNoExists()
        {
            // Arrange
            var comments = new List<Comment>
            {
                new() { CommentNo = 1, Project = "PP001", Year = 2023, Topic = "Topic A", CommentText = "Text1", MadeBy = "User1", DateEntered = new DateTime(2023, 6, 1) },
                new() { CommentNo = 2, Project = "PP001", Year = 2024, Topic = "Topic B", CommentText = "Text2", MadeBy = "User2", DateEntered = new DateTime(2024, 1, 1) }
            };
            var repo = CreateRepository(comments: comments);

            // Act
            var result = await repo.GetByIdAsync(1);

            // Assert
            Assert.NotNull(result);
            Assert.Equal(1, result.CommentNo);
            Assert.Equal("PP001", result.Project);
            Assert.Equal((short)2023, result.Year);
            Assert.Equal("Topic A", result.Topic);
            Assert.Equal("Text1", result.CommentText);
            Assert.Equal("User1", result.MadeBy);
            Assert.Equal(new DateTime(2023, 6, 1), result.DateEntered);
        }

        [Fact]
        public async Task GetByIdAsync_ReturnsNull_WhenCommentNoDoesNotExist()
        {
            // Arrange
            var comments = new List<Comment>
            {
                new() { CommentNo = 1, Project = "PP001", Year = 2023, Topic = "Topic A" }
            };
            var repo = CreateRepository(comments: comments);

            // Act
            var result = await repo.GetByIdAsync(999);

            // Assert
            Assert.Null(result);
        }

        [Fact]
        public async Task GetByIdAsync_ReturnsNull_WhenCommentsDbSetIsEmpty()
        {
            // Arrange
            var repo = CreateRepository(comments: new List<Comment>());

            // Act
            var result = await repo.GetByIdAsync(1);

            // Assert
            Assert.Null(result);
        }

        [Theory]
        [InlineData(0)]
        [InlineData(999)]
        public async Task GetByIdAsync_ReturnsNull_WhenIdDoesNotMatch(int commentNo)
        {
            // Arrange
            var comments = new List<Comment>
            {
                new() { CommentNo = 1, Project = "PP001", Year = 2024, Topic = "Topic" }
            };
            var repo = CreateRepository(comments: comments);

            // Act
            var result = await repo.GetByIdAsync(commentNo);

            // Assert
            Assert.Null(result);
        }

        #endregion

        #region ExistsAsync

        
        //   and excludeCommentNo update-path parameter (mirrors ix_tblcomments unique constraint)

        [Fact]
        public async Task ExistsAsync_ReturnsTrue_WhenMatchingRecordExists()
        {
            // Arrange
            var comments = new List<Comment>
            {
                new() { CommentNo = 1, Project = "PP001", Year = 2024, Topic = "Safety" },
                new() { CommentNo = 2, Project = "PP001", Year = 2024, Topic = "Finance" }
            };
            var repo = CreateRepository(comments: comments);

            // Act
            var result = await repo.ExistsAsync("PP001", (short)2024, "Safety");

            // Assert
            Assert.True(result);
        }

        [Fact]
        public async Task ExistsAsync_ReturnsFalse_WhenNoMatchingRecordExists()
        {
            // Arrange
            var comments = new List<Comment>
            {
                new() { CommentNo = 1, Project = "PP001", Year = 2024, Topic = "Safety" }
            };
            var repo = CreateRepository(comments: comments);

            // Act
            var result = await repo.ExistsAsync("PP001", (short)2024, "Finance");

            // Assert
            Assert.False(result);
        }

        [Fact]
        public async Task ExistsAsync_ReturnsFalse_WhenCommentsDbSetIsEmpty()
        {
            // Arrange
            var repo = CreateRepository(comments: new List<Comment>());

            // Act
            var result = await repo.ExistsAsync("PP001", (short)2024, "Safety");

            // Assert
            Assert.False(result);
        }

        [Fact]
        public async Task ExistsAsync_ReturnsFalse_WhenProjectDoesNotMatch()
        {
            // Arrange
            var comments = new List<Comment>
            {
                new() { CommentNo = 1, Project = "PP002", Year = 2024, Topic = "Safety" }
            };
            var repo = CreateRepository(comments: comments);

            // Act
            var result = await repo.ExistsAsync("PP001", (short)2024, "Safety");

            // Assert
            Assert.False(result);
        }

        [Fact]
        public async Task ExistsAsync_ReturnsFalse_WhenYearDoesNotMatch()
        {
            // Arrange
            var comments = new List<Comment>
            {
                new() { CommentNo = 1, Project = "PP001", Year = 2023, Topic = "Safety" }
            };
            var repo = CreateRepository(comments: comments);

            // Act
            var result = await repo.ExistsAsync("PP001", (short)2024, "Safety");

            // Assert
            Assert.False(result);
        }

        [Fact]
        public async Task ExistsAsync_ReturnsFalse_WhenExcludeCommentNoMatchesOnlyRecord()
        {
            // Arrange — update-path: the only matching record is the one being updated; must not report duplicate
            var comments = new List<Comment>
            {
                new() { CommentNo = 5, Project = "PP001", Year = 2024, Topic = "Safety" }
            };
            var repo = CreateRepository(comments: comments);

            // Act — exclude the record being updated (CommentNo = 5)
            var result = await repo.ExistsAsync("PP001", (short)2024, "Safety", excludeCommentNo: 5);

            // Assert
            Assert.False(result);
        }

        [Fact]
        public async Task ExistsAsync_ReturnsTrue_WhenAnotherMatchingRecordExistsAfterExclusion()
        {
            // Arrange — update-path: there are two records with the same key; excluding one still leaves another
            var comments = new List<Comment>
            {
                new() { CommentNo = 5, Project = "PP001", Year = 2024, Topic = "Safety" },
                new() { CommentNo = 7, Project = "PP001", Year = 2024, Topic = "Safety" }
            };
            var repo = CreateRepository(comments: comments);

            // Act — exclude CommentNo=5; CommentNo=7 is still a duplicate
            var result = await repo.ExistsAsync("PP001", (short)2024, "Safety", excludeCommentNo: 5);

            // Assert
            Assert.True(result);
        }

        [Fact]
        public async Task ExistsAsync_ReturnsTrue_WhenExcludeCommentNoDoesNotMatchAnyRecord()
        {
            // Arrange — excludeCommentNo is set but doesn't match any of the existing records
            var comments = new List<Comment>
            {
                new() { CommentNo = 1, Project = "PP001", Year = 2024, Topic = "Safety" }
            };
            var repo = CreateRepository(comments: comments);

            // Act
            var result = await repo.ExistsAsync("PP001", (short)2024, "Safety", excludeCommentNo: 999);

            // Assert
            Assert.True(result);
        }

        [Fact]
        public async Task ExistsAsync_WithoutExcludeCommentNo_ReturnsTrue_WhenRecordExists()
        {
            // Arrange — verify default null path (no excludeCommentNo)
            var comments = new List<Comment>
            {
                new() { CommentNo = 3, Project = "PP002", Year = 2022, Topic = "Planning" }
            };
            var repo = CreateRepository(comments: comments);

            // Act
            var result = await repo.ExistsAsync("PP002", (short)2022, "Planning");

            // Assert
            Assert.True(result);
        }

        #endregion

        #region AddAsync

        [Fact]
        public async Task AddAsync_AddsEntityAndReturnsIt()
        {
            // Arrange
            var (repo, _, _) = CreateRepositoryWithMocks();
            var entity = new Comment
            {
                CommentNo = 1,
                Project = "PP001",
                Year = 2024,
                Topic = "New Topic",
                CommentText = "Comment text",
                MadeBy = "User1",
                DateEntered = new DateTime(2024, 1, 1)
            };

            // Act
            var result = await repo.AddAsync(entity);

            // Assert
            Assert.NotNull(result);
            Assert.Same(entity, result);
            Assert.Equal(1, result.CommentNo);
            Assert.Equal("PP001", result.Project);
            Assert.Equal("New Topic", result.Topic);
        }

        [Fact]
        public async Task AddAsync_CallsDbSetAdd()
        {
            // Arrange
            var (repo, commentsDbSet, _) = CreateRepositoryWithMocks();
            var entity = new Comment { CommentNo = 1, Project = "PP001", Year = 2024, Topic = "Topic" };

            // Act
            await repo.AddAsync(entity);

            // Assert
            commentsDbSet.Verify(x => x.Add(entity), Times.Once);
        }

        [Fact]
        public async Task AddAsync_CallsSaveChangesAsync()
        {
            // Arrange
            var (repo, _, mockContext) = CreateRepositoryWithMocks();
            var entity = new Comment { CommentNo = 1, Project = "PP001", Year = 2024, Topic = "Topic" };

            // Act
            await repo.AddAsync(entity);

            // Assert
            RepositoryTestHelper.VerifySaveChanges(mockContext, times: 1);
        }

        #endregion

        #region UpdateAsync

        [Fact]
        public async Task UpdateAsync_ReturnsEntity()
        {
            // Arrange
            var (repo, _, _) = CreateRepositoryWithMocks();
            var entity = new Comment
            {
                CommentNo = 1,
                Project = "PP001",
                Year = 2024,
                Topic = "Updated Topic",
                CommentText = "Updated text",
                MadeBy = "User1"
            };

            // Act
            var result = await repo.UpdateAsync(entity);

            // Assert
            Assert.NotNull(result);
            Assert.Same(entity, result);
            Assert.Equal("Updated Topic", result.Topic);
        }

        [Fact]
        public async Task UpdateAsync_CallsDbSetUpdate()
        {
            // Arrange
            var (repo, commentsDbSet, _) = CreateRepositoryWithMocks();
            var entity = new Comment { CommentNo = 1, Project = "PP001", Year = 2024, Topic = "Topic" };

            // Act
            await repo.UpdateAsync(entity);

            // Assert
            commentsDbSet.Verify(x => x.Update(entity), Times.Once);
        }

        [Fact]
        public async Task UpdateAsync_CallsSaveChangesAsync()
        {
            // Arrange
            var (repo, _, mockContext) = CreateRepositoryWithMocks();
            var entity = new Comment { CommentNo = 1, Project = "PP001", Year = 2024, Topic = "Topic" };

            // Act
            await repo.UpdateAsync(entity);

            // Assert
            RepositoryTestHelper.VerifySaveChanges(mockContext, times: 1);
        }

        #endregion

        #region DeleteAsync

        [Fact]
        public async Task DeleteAsync_ReturnsTrue_WhenEntityFound()
        {
            // Arrange
            var entity = new Comment { CommentNo = 1, Project = "PP001", Year = 2024, Topic = "Topic" };
            var (repo, commentsDbSet, _) = CreateRepositoryWithMocks();
            commentsDbSet
                .Setup(x => x.FindAsync(It.IsAny<object[]>()))
                .Returns(new ValueTask<Comment?>(entity));

            // Act
            var result = await repo.DeleteAsync(1);

            // Assert
            Assert.True(result);
        }

        [Fact]
        public async Task DeleteAsync_CallsDbSetRemove_WhenEntityFound()
        {
            // Arrange
            var entity = new Comment { CommentNo = 1, Project = "PP001", Year = 2024, Topic = "Topic" };
            var (repo, commentsDbSet, _) = CreateRepositoryWithMocks();
            commentsDbSet
                .Setup(x => x.FindAsync(It.IsAny<object[]>()))
                .Returns(new ValueTask<Comment?>(entity));

            // Act
            await repo.DeleteAsync(1);

            // Assert
            commentsDbSet.Verify(x => x.Remove(entity), Times.Once);
        }

        [Fact]
        public async Task DeleteAsync_CallsSaveChangesAsync_WhenEntityFound()
        {
            // Arrange
            var entity = new Comment { CommentNo = 1, Project = "PP001", Year = 2024, Topic = "Topic" };
            var (repo, commentsDbSet, mockContext) = CreateRepositoryWithMocks();
            commentsDbSet
                .Setup(x => x.FindAsync(It.IsAny<object[]>()))
                .Returns(new ValueTask<Comment?>(entity));

            // Act
            await repo.DeleteAsync(1);

            // Assert
            RepositoryTestHelper.VerifySaveChanges(mockContext, times: 1);
        }

        [Fact]
        public async Task DeleteAsync_ReturnsFalse_WhenEntityNotFound()
        {
            // Arrange
            var (repo, commentsDbSet, _) = CreateRepositoryWithMocks();
            commentsDbSet
                .Setup(x => x.FindAsync(It.IsAny<object[]>()))
                .Returns(new ValueTask<Comment?>((Comment?)null));

            // Act
            var result = await repo.DeleteAsync(999);

            // Assert
            Assert.False(result);
        }

        [Fact]
        public async Task DeleteAsync_DoesNotCallRemove_WhenEntityNotFound()
        {
            // Arrange
            var (repo, commentsDbSet, _) = CreateRepositoryWithMocks();
            commentsDbSet
                .Setup(x => x.FindAsync(It.IsAny<object[]>()))
                .Returns(new ValueTask<Comment?>((Comment?)null));

            // Act
            await repo.DeleteAsync(999);

            // Assert
            commentsDbSet.Verify(x => x.Remove(It.IsAny<Comment>()), Times.Never);
        }

        [Fact]
        public async Task DeleteAsync_DoesNotCallSaveChangesAsync_WhenEntityNotFound()
        {
            // Arrange
            var (repo, commentsDbSet, mockContext) = CreateRepositoryWithMocks();
            commentsDbSet
                .Setup(x => x.FindAsync(It.IsAny<object[]>()))
                .Returns(new ValueTask<Comment?>((Comment?)null));

            // Act
            await repo.DeleteAsync(999);

            // Assert
            RepositoryTestHelper.VerifySaveChanges(mockContext, times: 0);
        }

        #endregion

        #region GetCommentTopicsAsync

        
        //   Topic combo-box RowSource on the Comments form (tlkpcommenttopics → DbSet<CommentTopic>)

        [Fact]
        public async Task GetCommentTopicsAsync_ReturnsAllTopics_WhenTopicsExist()
        {
            // Arrange
            var topics = new List<CommentTopic>
            {
                new() { Topic = "Safety"    },
                new() { Topic = "Finance"   },
                new() { Topic = "Planning"  },
                new() { Topic = "Engineering" }
            };
            var repo = CreateRepository(commentTopics: topics);

            // Act
            var result = await repo.GetCommentTopicsAsync();

            // Assert
            Assert.NotNull(result);
            var list = result.ToList();
            Assert.Equal(4, list.Count);
        }

        [Fact]
        public async Task GetCommentTopicsAsync_ReturnsEmpty_WhenTopicsDbSetIsEmpty()
        {
            // Arrange
            var repo = CreateRepository(commentTopics: new List<CommentTopic>());

            // Act
            var result = await repo.GetCommentTopicsAsync();

            // Assert
            Assert.NotNull(result);
            Assert.Empty(result);
        }

        [Fact]
        public async Task GetCommentTopicsAsync_ReturnsCorrectTopicValues()
        {
            // Arrange
            var topics = new List<CommentTopic>
            {
                new() { Topic = "Safety"   },
                new() { Topic = "Finance"  }
            };
            var repo = CreateRepository(commentTopics: topics);

            // Act
            var result = await repo.GetCommentTopicsAsync();

            // Assert
            var list = result.ToList();
            Assert.Contains(list, t => t.Topic == "Safety");
            Assert.Contains(list, t => t.Topic == "Finance");
        }

        [Fact]
        public async Task GetCommentTopicsAsync_ReturnsSingleTopic_WhenOnlyOneTopicExists()
        {
            // Arrange
            var topics = new List<CommentTopic>
            {
                new() { Topic = "Safety" }
            };
            var repo = CreateRepository(commentTopics: topics);

            // Act
            var result = await repo.GetCommentTopicsAsync();

            // Assert
            Assert.Single(result);
            Assert.Equal("Safety", result.First().Topic);
        }

        #endregion

        #region GetForecastSpendByProjectAsync

        [Fact]
        public async Task GetForecastSpendByProjectAsync_ReturnsForecastSpend_WhenProjectExists()
        {
            // Arrange
            var radTrackData = new List<ProjectRadTrackData>
            {
                new() { Parentproject = "PP001", Pcforecastspend = 1234.56 },
                new() { Parentproject = "PP002", Pcforecastspend = 4321.00 }
            };
            var repo = CreateRepositoryForForecastSpend(radTrackData);

            // Act
            var result = await repo.GetForecastSpendByProjectAsync("PP001");

            // Assert
            Assert.Equal(1234.56, result);
        }

        [Fact]
        public async Task GetForecastSpendByProjectAsync_ReturnsNull_WhenProjectDoesNotExist()
        {
            // Arrange
            var radTrackData = new List<ProjectRadTrackData>
            {
                new() { Parentproject = "PP002", Pcforecastspend = 4321.00 }
            };
            var repo = CreateRepositoryForForecastSpend(radTrackData);

            // Act
            var result = await repo.GetForecastSpendByProjectAsync("PP001");

            // Assert
            Assert.Null(result);
        }

        #endregion

        #region UpdateForecastSpendByProjectAsync

        [Fact]
        public async Task UpdateForecastSpendByProjectAsync_UpdatesAndReturnsForecastSpend_WhenProjectExists()
        {
            // Arrange
            var radTrackData = new List<ProjectRadTrackData>
            {
                new() { Parentproject = "PP001", Pcforecastspend = 100.00 }
            };
            var (repo, _, mockContext) = CreateRepositoryWithMocks(projectRadTrackData: radTrackData);

            // Act
            var result = await repo.UpdateForecastSpendByProjectAsync("PP001", 2500.75);

            // Assert
            Assert.Equal(2500.75, result);
            Assert.Equal(2500.75, radTrackData[0].Pcforecastspend);
            RepositoryTestHelper.VerifySaveChanges(mockContext, times: 1);
        }

        [Fact]
        public async Task UpdateForecastSpendByProjectAsync_ReturnsNullAndDoesNotSave_WhenProjectDoesNotExist()
        {
            // Arrange
            var radTrackData = new List<ProjectRadTrackData>
            {
                new() { Parentproject = "PP002", Pcforecastspend = 100.00 }
            };
            var (repo, _, mockContext) = CreateRepositoryWithMocks(projectRadTrackData: radTrackData);

            // Act
            var result = await repo.UpdateForecastSpendByProjectAsync("PP001", 2500.75);

            // Assert
            Assert.Null(result);
            RepositoryTestHelper.VerifySaveChanges(mockContext, times: 0);
        }

        #endregion
    }
}
