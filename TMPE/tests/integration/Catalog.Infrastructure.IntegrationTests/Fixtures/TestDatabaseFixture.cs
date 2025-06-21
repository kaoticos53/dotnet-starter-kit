using System;
using System.Data.Common;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using FSH.Framework.Core.Persistence;
using FSH.Framework.Infrastructure.Persistence;
using FSH.Framework.Infrastructure.Tenant;
using FSH.Starter.WebApi.Catalog.Infrastructure.Persistence;
using Finbuckle.MultiTenant;
using Finbuckle.MultiTenant.Abstractions;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Moq;
using Npgsql;
using Respawn;

namespace Catalog.Infrastructure.IntegrationTests.Fixtures;

/// <summary>
/// Fixture para pruebas de integración que gestiona el ciclo de vida de la base de datos de prueba.
/// </summary>
public class TestDatabaseFixture : IAsyncLifetime, IDisposable
{
    private readonly string _connectionString;
    private NpgsqlConnection? _dbConnection;
    private readonly ILoggerFactory _loggerFactory;
    private readonly IMultiTenantContextAccessor<FshTenantInfo> _multiTenantContextAccessor;
    private readonly Mock<IPublisher> _mockPublisher;
    private readonly IOptions<DatabaseOptions> _databaseOptions;
    private readonly IServiceProvider _serviceProvider;
    private readonly FshTenantInfo _tenantInfo;
    private Respawner? _respawner;
    private bool _disposed;

    public CatalogDbContext? Context { get; private set; }

    public TestDatabaseFixture()
    {
        try
        {
            Console.WriteLine("=== Initializing TestDatabaseFixture ===");

            // 1. Configure logging first
            _loggerFactory = LoggerFactory.Create(builder =>
            {
                builder
                    .AddConsole()
                    .AddDebug()
                    .SetMinimumLevel(LogLevel.Debug);
            });

            // 2. Load configuration
            var configuration = new ConfigurationBuilder()
                .SetBasePath(Directory.GetCurrentDirectory())
                .AddJsonFile("appsettings.json", optional: false, reloadOnChange: true)
                .AddJsonFile($"appsettings.{Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT") ?? "Development"}.json", optional: true)
                .AddEnvironmentVariables()
                .Build();

            // 3. Initialize connection string
            _connectionString = configuration.GetConnectionString("DefaultConnection") ?? 
                throw new InvalidOperationException("Connection string 'DefaultConnection' not found in configuration");
            
            // 4. Setup database options
            var databaseOptions = new DatabaseOptions
            {
                ConnectionString = _connectionString,
                Provider = "Npgsql"
            };
            
            _databaseOptions = Options.Create(databaseOptions);
            
            // 5. Initialize mock multi-tenant context accessor
            var mockAccessor = new Mock<IMultiTenantContextAccessor<FshTenantInfo>>(MockBehavior.Strict);
            var mockMultiTenantContext = new Mock<IMultiTenantContext<FshTenantInfo>>(MockBehavior.Strict);
            
            // Setup mock tenant info
            _tenantInfo = new FshTenantInfo(
                id: "test-tenant",
                name: "Test Tenant",
                connectionString: _connectionString,
                adminEmail: "test@test.com");
            
            // Setup mock multi-tenant context
            mockMultiTenantContext.Setup(x => x.TenantInfo).Returns(_tenantInfo);
            mockAccessor.Setup(x => x.MultiTenantContext).Returns(mockMultiTenantContext.Object);
            _multiTenantContextAccessor = mockAccessor.Object;
            
            // 6. Create database connection
            _dbConnection = new NpgsqlConnection(_connectionString);
            
            // 7. Create service collection and register services
            var services = new ServiceCollection();
            
            // Add logging
            services.AddSingleton(_loggerFactory);
            services.AddLogging(configure => configure
                .AddConsole()
                .AddDebug()
                .SetMinimumLevel(LogLevel.Debug));
                
            // Register the mock for both generic and non-generic interfaces
            services.AddScoped<IMultiTenantContextAccessor<FshTenantInfo>>(_ => mockAccessor.Object);
            services.AddScoped<IMultiTenantContextAccessor>(_ => mockAccessor.As<IMultiTenantContextAccessor>().Object);
            
            // Register other required services
            _mockPublisher = new Mock<IPublisher>(MockBehavior.Loose);
            _mockPublisher
                .Setup(p => p.Publish(It.IsAny<object>(), It.IsAny<CancellationToken>()))
                .Returns(Task.CompletedTask);
                
            services.AddScoped<IPublisher>(_ => _mockPublisher.Object);
            services.AddScoped(_ => _databaseOptions);
            
            // Build the service provider
            _serviceProvider = services.BuildServiceProvider();
            
            Console.WriteLine("[SUCCESS] TestDatabaseFixture initialized successfully");
        }
        catch (Exception ex)
        {
            Console.WriteLine($"[ERROR] Failed to initialize TestDatabaseFixture: {ex}");
            if (ex.InnerException != null)
            {
                Console.WriteLine($"[INNER] {ex.InnerException.GetType().Name}: {ex.InnerException.Message}");
            }
            Dispose();
            throw;
        }
    }

    public async Task InitializeAsync()
    {
        if (_disposed)
            throw new ObjectDisposedException(nameof(TestDatabaseFixture));
            
        Console.WriteLine("\n=== Starting test database initialization ===");
        
        try
        {
            if (_dbConnection == null)
            {
                throw new InvalidOperationException("Database connection is not initialized");
            }

            // 1. Open database connection
            Console.WriteLine("\n[INIT] Opening database connection...");
            await _dbConnection.OpenAsync();
            Console.WriteLine("[OK] Database connection opened successfully");

            // 2. Initialize Respawner
            _respawner = await Respawner.CreateAsync(_dbConnection, new RespawnerOptions
            {
                TablesToIgnore = new[] { new Respawn.Graph.Table("__EFMigrationsHistory") },
                SchemasToExclude = new[] { "public" }
            });
            
            // 3. Verify tenant info
            Console.WriteLine("\n[VERIFY] Verifying tenant info...");
            if (_tenantInfo == null)
            {
                throw new InvalidOperationException("Tenant info is not initialized");
            }
            Console.WriteLine($"[OK] Tenant Info: Id={_tenantInfo.Id}, Name={_tenantInfo.Name}");

            // 4. Initialize DbContext
            Console.WriteLine("\n[INIT] Initializing CatalogDbContext...");
            
            var optionsBuilder = new DbContextOptionsBuilder<CatalogDbContext>();
            optionsBuilder.UseNpgsql(_connectionString);
            
            Context = new CatalogDbContext(
                _multiTenantContextAccessor,
                optionsBuilder.Options,
                _mockPublisher.Object,
                _databaseOptions);
                
            // Ensure database is created
            await Context.Database.EnsureCreatedAsync();
            
            Console.WriteLine("[OK] CatalogDbContext initialized successfully");
            Console.WriteLine("\n[SUCCESS] Test database initialization completed successfully");
        }
        catch (Exception ex)
        {
            Console.WriteLine($"[ERROR] Error during database initialization: {ex}");
            if (ex.InnerException != null)
            {
                Console.WriteLine($"[INNER] {ex.InnerException.GetType().Name}: {ex.InnerException.Message}");
            }
            await DisposeAsync();
            throw;
        }
    }
    
    public async Task ResetDatabaseAsync()
    {
        if (_disposed)
            throw new ObjectDisposedException(nameof(TestDatabaseFixture));
            
        if (_respawner != null && _dbConnection != null)
        {
            try
            {
                await _respawner.ResetAsync(_dbConnection);
                Console.WriteLine("[OK] Test database reset completed successfully");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[ERROR] Error resetting test database: {ex.Message}");
                throw;
            }
        }
    }
    
    public async Task DisposeAsync()
    {
        await DisposeAsyncCore();
        GC.SuppressFinalize(this);
    }
    
    protected virtual async ValueTask DisposeAsyncCore()
    {
        if (!_disposed)
        {
            if (Context != null)
            {
                await Context.DisposeAsync();
                Context = null;
            }
            
            if (_dbConnection != null)
            {
                await _dbConnection.DisposeAsync();
                _dbConnection = null;
            }
            
            _loggerFactory?.Dispose();
            _disposed = true;
        }
    }
    
    public void Dispose()
    {
        Dispose(true);
        GC.SuppressFinalize(this);
    }
    
    protected virtual void Dispose(bool disposing)
    {
        if (!_disposed)
        {
            if (disposing)
            {
                // Dispose managed resources
                Context?.Dispose();
                _dbConnection?.Dispose();
                _loggerFactory?.Dispose();
            }

            // Free unmanaged resources
            _dbConnection = null;
            Context = null;
            _disposed = true;
        }
    }
    
    ~TestDatabaseFixture()
    {
        Dispose(false);
    }
}
