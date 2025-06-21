using System;
using FSH.Framework.Core.Exceptions;
using FSH.Starter.WebApi.Catalog.Domain.Exceptions;
using FluentAssertions;
using Xunit;

namespace FSH.Starter.WebApi.Catalog.Domain.Tests.Domain.Exceptions;

public class DomainExceptionTests
{
    public class ProductNotFoundExceptionTests
    {
        [Fact]
        public void Constructor_WithProductId_SetsMessage()
        {
            // Arrange
            var productId = Guid.NewGuid();
            var expectedMessage = $"product with id {productId} not found";

            // Act
            var exception = new ProductNotFoundException(productId);

            // Assert
            exception.Should().NotBeNull();
            exception.Should().BeAssignableTo<NotFoundException>();
            exception.Message.Should().Be(expectedMessage);
        }

        [Fact]
        public void Constructor_WithEmptyGuid_SetsMessage()
        {
            // Arrange
            var productId = Guid.Empty;
            var expectedMessage = "product with id 00000000-0000-0000-0000-000000000000 not found";

            // Act
            var exception = new ProductNotFoundException(productId);

            // Assert
            exception.Message.Should().Be(expectedMessage);
        }
    }


    public class BrandNotFoundExceptionTests
    {
        [Fact]
        public void Constructor_WithBrandId_SetsMessage()
        {
            // Arrange
            var brandId = Guid.NewGuid();
            var expectedMessage = $"Brand with id {brandId} not found";

            // Act
            var exception = new BrandNotFoundException(brandId);

            // Assert
            exception.Should().NotBeNull();
            exception.Should().BeAssignableTo<NotFoundException>();
            exception.Message.Should().Be(expectedMessage);
        }

        [Fact]
        public void Constructor_WithEmptyGuid_SetsMessage()
        {
            // Arrange
            var brandId = Guid.Empty;
            var expectedMessage = "Brand with id 00000000-0000-0000-0000-000000000000 not found";

            // Act
            var exception = new BrandNotFoundException(brandId);

            // Assert
            exception.Message.Should().Be(expectedMessage);
        }
    }
}
