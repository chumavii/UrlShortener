using Microsoft.EntityFrameworkCore;
using System;
using System.Linq;
using UrlShortener.Data;
using UrlShortener.Models;
using Xunit;

namespace UrlShortener.Tests
{
    public class ApplicationDbContextTests
    {
        [Fact]
        public void OnModelCreating_ShouldConfigureUniqueIndexForOriginalUrl()
        {
            // Arrange
            var options = new DbContextOptionsBuilder<ApplicationDbContext>()
                .UseInMemoryDatabase(Guid.NewGuid().ToString())
                .Options;

            using var context = new ApplicationDbContext(options);

            // Act
            var entityType = context.Model.FindEntityType(typeof(UrlMapping));
            var index = entityType?.GetIndexes()
                .SingleOrDefault(i => i.Properties.Any(p => p.Name == nameof(UrlMapping.OriginalUrl)));

            // Assert
            Assert.NotNull(entityType);
            Assert.NotNull(index);
            Assert.True(index!.IsUnique);
        }

        [Fact]
        public void OnModelCreating_ShouldConfigureUniqueIndexForShortCode()
        {
            // Arrange
            var options = new DbContextOptionsBuilder<ApplicationDbContext>()
                .UseInMemoryDatabase(Guid.NewGuid().ToString())
                .Options;

            using var context = new ApplicationDbContext(options);

            // Act
            var entityType = context.Model.FindEntityType(typeof(UrlMapping));
            var index = entityType?.GetIndexes()
                .SingleOrDefault(i => i.Properties.Any(p => p.Name == nameof(UrlMapping.ShortCode)));

            // Assert
            Assert.NotNull(entityType);
            Assert.NotNull(index);
            Assert.True(index!.IsUnique);
        }
    }
}
