using Ardalis.Specification;
using FSH.Framework.Core.Domain.Contracts;
using FSH.Framework.Core.Persistence;
using FSH.Starter.WebApi.Catalog.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Xunit;

namespace FSH.Starter.WebApi.Catalog.Infrastructure.Tests.Persistence;

public abstract class RepositoryTestBase<TEntity, TId> : IClassFixture<CatalogDbContextFixture>
    where TEntity : class, IAggregateRoot
    where TId : notnull
{
    protected readonly CatalogDbContextFixture Fixture;
    protected readonly CatalogDbContext Context;
    protected readonly IRepository<TEntity> Repository;
    protected readonly IReadRepository<TEntity> ReadRepository;

    protected RepositoryTestBase(CatalogDbContextFixture fixture)
    {
        Fixture = fixture;
        Context = Fixture.CreateContext();
        Repository = new CatalogRepository<TEntity>(Context);
        ReadRepository = Repository; // CatalogRepository implements both interfaces
        
        // Clear the database before each test
        ClearDatabase();
    }

    protected virtual void ClearDatabase()
    {
        var entries = Context.ChangeTracker.Entries()
            .Where(e => e.State != EntityState.Detached)
            .ToList();

        foreach (var entry in entries)
        {
            entry.State = EntityState.Detached;
        }

        Context.Database.EnsureDeleted();
        Context.Database.EnsureCreated();
    }

    protected void DetachAllEntities()
    {
        var entries = Context.ChangeTracker.Entries()
            .Where(e => e.State != EntityState.Detached)
            .ToList();

        foreach (var entry in entries)
        {
            entry.State = EntityState.Detached;
        }
    }

    protected void SaveChanges()
    {
        try
        {
            Context.SaveChanges();
        }
        catch (DbUpdateConcurrencyException ex)
        {
            // Handle concurrency conflicts if needed
            throw;
        }
    }

    protected async Task SaveChangesAsync()
    {
        try
        {
            await Context.SaveChangesAsync();
        }
        catch (DbUpdateConcurrencyException ex)
        {
            // Handle concurrency conflicts if needed
            throw;
        }
    }
}
