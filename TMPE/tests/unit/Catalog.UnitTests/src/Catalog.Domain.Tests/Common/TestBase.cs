using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using AutoFixture;
using AutoFixture.AutoMoq;
using AutoFixture.Xunit2;
using Bogus;
using FluentAssertions;
using Xunit;
using Xunit.Abstractions;

namespace FSH.Starter.WebApi.Catalog.Domain.Tests.Common;

/// <summary>
/// Clase base para todas las pruebas unitarias que proporciona funcionalidades comunes.
/// </summary>
[SuppressMessage("Design", "CA1051:No declarar campos de instancia visibles", Justification = "Necesario para la herencia en pruebas")]
[SuppressMessage("Design", "CA1062:Validar argumentos de métodos de utilidad", Justification = "Validación manejada por AutoFixture")]
[SuppressMessage("Design", "CA1001:Types that own disposable fields should be disposable", Justification = "Base class implements IDisposable")]
public abstract class TestBase : IDisposable
{
    private bool _disposed;
    
    /// <summary>
    /// Obtiene el ayudante de salida de prueba.
    /// </summary>
    protected ITestOutputHelper? Output { get; }
    
    private IFixture? _fixture;
    
    /// <summary>
    /// Obtiene la instancia de AutoFixture para la generación de datos de prueba.
    /// </summary>
    protected IFixture Fixture => _fixture ??= CreateAndConfigureFixture();
    
    /// <summary>
    /// Obtiene la instancia de Faker para la generación de datos aleatorios.
    /// </summary>
    protected Faker Faker { get; }

    /// <summary>
    /// Inicializa una nueva instancia de la clase <see cref="TestBase"/>.
    /// </summary>
    /// <param name="output">El ayudante de salida de prueba.</param>
    protected TestBase(ITestOutputHelper? output = null)
    {
        Output = output;
        Faker = new Faker();
        // Fixture se inicializará de forma perezosa cuando se acceda por primera vez
    }
    
    private static Fixture CreateAndConfigureFixture()
    {
        var fixture = new Fixture();
        
        // Configuración común de AutoFixture
        fixture.Behaviors.OfType<ThrowingRecursionBehavior>().ToList()
            .ForEach(b => fixture.Behaviors.Remove(b));
        fixture.Behaviors.Add(new OmitOnRecursionBehavior());
        
        return fixture;
    }
    
    /// <summary>
    /// Libera los recursos utilizados por la instancia actual de la clase <see cref="TestBase"/>.
    /// </summary>
    public void Dispose()
    {
        Dispose(true);
        GC.SuppressFinalize(this);
    }
    
    /// <summary>
    /// Libera los recursos no administrados utilizados por la clase <see cref="TestBase"/> y, opcionalmente, libera los recursos administrados.
    /// </summary>
    /// <param name="disposing">Es true para liberar tanto recursos administrados como no administrados; es false para liberar únicamente recursos no administrados.</param>
    protected virtual void Dispose(bool disposing)
    {
        if (!_disposed)
        {
            if (disposing)
            {
                // Liberar recursos administrados aquí
            }
            
            _disposed = true;
        }
    }
    
    /// <summary>
    /// Escribe una línea en la salida de prueba.
    /// </summary>
    /// <param name="message">Mensaje a escribir.</param>
    protected void WriteLine(string message)
    {
        if (Output != null)
        {
            Output.WriteLine(message);
        }
    }
    
    /// <summary>
    /// Escribe una línea con formato en la salida de prueba.
    /// </summary>
    /// <param name="format">Cadena de formato compuesto.</param>
    /// <param name="args">Matriz de objetos que contiene cero o más objetos a los que se va a dar formato.</param>
    protected void WriteLine(string format, params object[] args)
    {
        if (Output != null)
        {
            Output.WriteLine(format, args);
        }
    }

    // Fixture property is now implemented above with lazy initialization
    
    /// <summary>
    /// Configures the fixture with test-specific customizations.
    /// This method is called once per test and can be overridden by derived classes.
    /// </summary>
    /// <param name="fixture">The fixture to configure.</param>
    protected virtual void ConfigureFixture(IFixture fixture)
    {
        // Default implementation does nothing
    }
    
    /// <summary>
    /// Crea una instancia de un tipo anónimo con propiedades inicializadas.
    /// </summary>
    /// <summary>
    /// Crea una instancia anónima del tipo especificado.
    /// </summary>
    /// <typeparam name="T">Tipo de la instancia a crear.</typeparam>
    /// <returns>Una nueva instancia del tipo especificado.</returns>
    protected T CreateAnonymous<T>()
    {
        return Fixture.Create<T>();
    }
    
    /// <summary>
    /// Crea una colección de instancias anónimas.
    /// </summary>
    /// <typeparam name="T">Tipo de las instancias a crear.</typeparam>
    /// <param name="count">Número de instancias a crear. El valor predeterminado es 3.</param>
    /// <returns>Una colección de instancias del tipo especificado.</returns>
    protected IEnumerable<T> CreateManyAnonymous<T>(int count = 3)
    {
        return Fixture.CreateMany<T>(count);
    }
}

/// <summary>
/// Atributo personalizado para pruebas con AutoData que configura AutoFixture con soporte para Moq.
/// </summary>
[AttributeUsage(AttributeTargets.Method)]
[SuppressMessage("Design", "CA1019:Definir descriptores de acceso para los argumentos de atributo", Justification = "No es necesario para este caso de uso")]
public sealed class AutoMoqDataAttribute : AutoDataAttribute
{
    /// <summary>
    /// Inicializa una nueva instancia de la clase <see cref="AutoMoqDataAttribute"/>.
    /// </summary>
    public AutoMoqDataAttribute()
        : base(() => new Fixture().Customize(new AutoMoqCustomization()))
    {
    }
}

/// <summary>
/// Atributo personalizado para pruebas con datos en línea que utiliza AutoMoq.
/// </summary>
[AttributeUsage(AttributeTargets.Method, AllowMultiple = true)]
[SuppressMessage("Design", "CA1019:Definir descriptores de acceso para los argumentos de atributo", Justification = "No es necesario para este caso de uso")]
public sealed class InlineAutoMoqDataAttribute : InlineAutoDataAttribute
{
    /// <summary>
    /// Inicializa una nueva instancia de la clase <see cref="InlineAutoMoqDataAttribute"/>.
    /// </summary>
    /// <param name="values">Valores en línea para la prueba.</param>
    public InlineAutoMoqDataAttribute(params object[] values)
        : base(new AutoMoqDataAttribute(), values)
    {
    }
}
