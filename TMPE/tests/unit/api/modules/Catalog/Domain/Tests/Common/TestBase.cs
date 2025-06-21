using System;
using AutoFixture;
using AutoFixture.AutoMoq;
using AutoFixture.Xunit2;
using Xunit.Abstractions;

namespace Catalog.Domain.Tests.Common;

/// <summary>
/// Base class for all test fixtures in the domain layer.
/// Provides common test infrastructure like AutoFixture.
/// </summary>
public abstract class TestBase : IDisposable
{
    protected readonly ITestOutputHelper Output;
    private readonly Lazy<IFixture> _fixtureLazy;
    
    protected IFixture Fixture => _fixtureLazy.Value;
    
    protected TestBase(ITestOutputHelper output)
    {
        Output = output ?? throw new ArgumentNullException(nameof(output));
        _fixtureLazy = new Lazy<IFixture>(CreateAndConfigureFixture);
    }
    
    protected virtual IFixture CreateAndConfigureFixture()
    {
        var fixture = new Fixture()
            .Customize(new AutoMoqCustomization { ConfigureMembers = true });
            
        // Configure any default customizations here
        
        return fixture;
    }
    
    public virtual void Dispose()
    {
        // Clean up test resources if needed
    }
}

/// <summary>
/// AutoMoqData attribute that can be used for parameterized tests with AutoFixture and Moq
/// </summary>
public class AutoMoqDataAttribute : AutoDataAttribute
{
    public AutoMoqDataAttribute() : base(CreateFixture) { }
    
    private static IFixture CreateFixture()
    {
        var fixture = new Fixture()
            .Customize(new AutoMoqCustomization { ConfigureMembers = true });
            
        // Configure any default customizations here
        
        return fixture;
    }
}
