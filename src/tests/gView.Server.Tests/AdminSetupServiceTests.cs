using gView.Framework.Core.Exceptions;
using gView.Server.Services.Security;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging.Abstractions;

namespace gView.Server.Tests;

public class AdminSetupServiceTests
{
    private class FakeLoginStore : IManagerLoginStore
    {
        public List<(string username, string password)> Created { get; } = new();

        public bool HasManagerLogin() => Created.Count > 0;

        public void CreateFirstManagerLogin(string username, string password) => Created.Add((username, password));
    }

    private static AdminSetupService Create(Dictionary<string, string?>? config = null)
        => new(
            new ConfigurationBuilder().AddInMemoryCollection(config ?? new()).Build(),
            new LoginThrottleService(3, 3, TimeSpan.FromMinutes(5), TimeSpan.FromMinutes(15)),
            NullLoggerFactory.Instance);

    [Fact]
    public void Initialize_WithEnvironment_CreatesAdmin()
    {
        var store = new FakeLoginStore();
        var setup = Create(new()
        {
            [AdminSetupService.AdminUserKey] = "admin",
            [AdminSetupService.AdminPasswordKey] = "Correct-Horse-42"
        });

        setup.Initialize(store);

        Assert.Single(store.Created);
        Assert.Null(setup.CurrentSetupCode);
        Assert.False(setup.IsSetupRequired(store));
    }

    [Fact]
    public void Initialize_WithoutEnvironment_CreatesSetupCodeButNoAdmin()
    {
        var store = new FakeLoginStore();
        var setup = Create();

        setup.Initialize(store);

        Assert.Empty(store.Created);
        Assert.NotNull(setup.CurrentSetupCode);
        Assert.True(setup.IsSetupRequired(store));
    }

    [Fact]
    public void CreateFirstAdmin_WithSetupCode_CreatesAdminAndConsumesCode()
    {
        var store = new FakeLoginStore();
        var setup = Create();
        setup.Initialize(store);
        var code = setup.CurrentSetupCode!;
        var formatted = $"{code[..4]}-{code[4..8]}-{code[8..]}".ToLowerInvariant();

        setup.CreateFirstAdmin(store, "1.1.1.1", formatted, "admin", "pw", "pw");

        Assert.Single(store.Created);
        Assert.Null(setup.CurrentSetupCode);
        Assert.Throws<MapServerException>(() => setup.CreateFirstAdmin(store, "1.1.1.1", code, "evil", "pw", "pw"));
    }

    [Fact]
    public void CreateFirstAdmin_WrongCode_Throws()
    {
        var store = new FakeLoginStore();
        var setup = Create();
        setup.Initialize(store);

        var ex = Assert.Throws<MapServerException>(() => setup.CreateFirstAdmin(store, "1.1.1.1", "WRONG", "admin", "pw", "pw"));

        Assert.Equal("Invalid setup code", ex.Message);
        Assert.Empty(store.Created);
    }

    [Fact]
    public void CreateFirstAdmin_PasswordsDoNotMatch_Throws()
    {
        var store = new FakeLoginStore();
        var setup = Create();
        setup.Initialize(store);

        Assert.Throws<MapServerException>(() => setup.CreateFirstAdmin(store, "1.1.1.1", setup.CurrentSetupCode!, "admin", "pw", "other"));
        Assert.Empty(store.Created);
    }

    [Fact]
    public void CreateFirstAdmin_TooManyWrongCodes_BlocksEvenTheRightCode()
    {
        var store = new FakeLoginStore();
        var setup = Create();
        setup.Initialize(store);

        for (int i = 0; i < 3; i++)
        {
            Assert.Throws<MapServerException>(() => setup.CreateFirstAdmin(store, "6.6.6.6", "WRONG", "admin", "pw", "pw"));
        }

        Assert.Throws<MapServerException>(() => setup.CreateFirstAdmin(store, "6.6.6.6", setup.CurrentSetupCode!, "admin", "pw", "pw"));
        Assert.Empty(store.Created);
    }
}
