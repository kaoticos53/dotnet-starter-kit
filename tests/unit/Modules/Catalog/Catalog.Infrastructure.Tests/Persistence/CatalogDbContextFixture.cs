using Finbuckle.MultiTenant;
using FSH.Framework.Infrastructure.Tenant;
using FSH.Starter.WebApi.Catalog.Infrastructure.Persistence;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Moq;
using Shared.Constants;
using Xunit;

namespace FSH.Starter.WebApi.Catalog.Infrastructure.Tests.Persistence;

public class CatalogDbContextFixture : IDisposable
{
    private const string InMemoryConnectionString = "DataSource=:memory:";
    private readonly SqliteConnection _connection;
    private readonly DbContextOptions<CatalogDbContext> _dbContextOptions;
    private readonly IMultiTenantContextAccessor<FshTenantInfo> _multiTenantContextAccessor;
    private readonly IPublisher _publisher;
    private readonly IOptions<DatabaseOptions> _databaseOptions;

    public CatalogDbContextFixture()
    {
        _connection = new SqliteConnection(InMemoryConnectionString);
        _connection.Open();

        _dbContextOptions = new DbContextOptionsBuilder<CatalogDbContext>()
            .UseSqlite(_connection)
            .EnableSensitiveDataLogging()
            .Options;

        // Mock the required dependencies
        var mockMultiTenantContextAccessor = new Mock<IMultiTenantContextAccessor<FshTenantInfo>>();
        var mockMultiTenantContext = new Mock<ITenantInfo>();
        mockMultiTenantContext.Setup(c => c.Id).Returns("test-tenant");
        mockMultiTenantContextAccessor.Setup(a => a.MultiTenantContext).Returns(new MultiTenantContext<FshTenantInfo>()
        {
            TenantInfo = new FshTenantInfo { Id = "test-tenant", Identifier = "test-tenant" }
        });

        _multiTenantContextAccessor = mockMultiTenantContextAccessor.Object;
        _publisher = Mock.Of<IPublisher>();
        _databaseOptions = Options.Create(new DatabaseOptions { ConnectionString = InMemoryConnectionString });

        using var context = CreateContext();
        context.Database.EnsureCreated();
    }

    public CatalogDbContext CreateContext()
    {
        var loggerFactory = LoggerFactory.Create(builder =>
        {
            builder.AddConsole();
            builder.SetMinimumLevel(LogLevel.Debug);
        });

        var context = new CatalogDbContext(
            _multiTenantContextAccessor,
            _dbContextOptions,
            _publisher,
            _databaseOptions);
            
        return context;
    }

    public void Dispose()
    {
        _connection?.Dispose();
        GC.SuppressFinalize(this);
    }

    public static CatalogDbContext CreateInMemoryContext()
    {
        var options = new DbContextOptionsBuilder<CatalogDbContext>()
            .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
            .Options;

        var loggerFactory = LoggerFactory.Create(builder =>
        {
            builder.AddConsole();
            builder.SetMinimumLevel(LogLevel.Debug);
        });

        return new CatalogDbContext(options, loggerFactory);
    }
}

[CollectionDefinition("CatalogDbContextTests")]
public class CatalogDbContextCollection : ICollectionFixture<CatalogDbContextFixture>
{
    // This class has no code, and is never created. Its purpose is simply
    // to be the place to apply [CollectionDefinition] and all the
    // ICollectionFixture<> interfaces.
}
