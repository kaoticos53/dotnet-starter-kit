using Bogus;
using FluentAssertions;
using Xunit;

namespace Catalog.Domain.UnitTests.Tests.Common;

/// <summary>
/// Clase base para pruebas unitarias que proporciona funcionalidades comunes.
/// </summary>
public abstract class BaseTest
{
    protected static readonly Faker Faker = new Faker();
    
    /// <summary>
    /// Verifica que el método de validación arroje una excepción de validación con el mensaje esperado.
    /// </summary>
    /// <param name="action">Acción que debe arrojar la excepción.</param>
    /// <param name="expectedMessage">Mensaje de error esperado.</param>
    protected static void ShouldThrowValidationError(Action action, string expectedMessage)
    {
        // Arrange & Act
        var exception = Assert.Throws<ArgumentException>(action);
        
        // Assert
        exception.Should().NotBeNull();
        exception.Message.Should().Contain(expectedMessage);
    }
    
    /// <summary>
    /// Verifica que el método de validación no arroje ninguna excepción.
    /// </summary>
    /// <param name="action">Acción que no debe arrojar excepciones.</param>
    protected static void ShouldNotThrowValidationError(Action action)
    {
        // Arrange & Act
        var exception = Record.Exception(action);
        
        // Assert
        exception.Should().BeNull();
    }
}
