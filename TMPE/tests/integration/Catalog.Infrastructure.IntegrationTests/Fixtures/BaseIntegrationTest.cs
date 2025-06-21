using Xunit;
using FSH.Starter.WebApi.Catalog.Infrastructure.Persistence;

namespace Catalog.Infrastructure.IntegrationTests.Fixtures;

/// <summary>
/// Clase base para pruebas de integración que proporciona acceso a la base de datos de prueba.
/// </summary>
[CollectionDefinition("Database collection")]
public class DatabaseCollection : ICollectionFixture<TestDatabaseFixture>
{
    // Esta clase no necesita código. Se utiliza para definir la colección de pruebas.
}

/// <summary>
/// Clase base para pruebas de integración que requieren acceso a la base de datos.
/// </summary>
[Collection("Database collection")]
public abstract class BaseIntegrationTest : IClassFixture<TestDatabaseFixture>, IAsyncLifetime
{
    protected TestDatabaseFixture Fixture { get; }
    protected CatalogDbContext Context => Fixture.Context;

    protected BaseIntegrationTest(TestDatabaseFixture fixture)
    {
        Fixture = fixture;
    }

    public virtual async Task InitializeAsync()
    {
        // Reiniciar la base de datos antes de cada prueba
        await Fixture.ResetDatabaseAsync();
    }

    public virtual Task DisposeAsync()
    {
        // No es necesario limpiar aquí ya que TestDatabaseFixture maneja la limpieza
        return Task.CompletedTask;
    }
}
