using Infrastructure.DataBase;
using Microsoft.Extensions.Logging.Abstractions;
using Microsoft.Extensions.Options;
using Tests.Application;

namespace Tests.Repositories;

public class AdminSeederTests
{
    private readonly ServiceTestHarness _harness = new();

    private AdminSeeder CreateSeeder(string userName = "admin", string password = "admin1234")
        => new(
            _harness.Users,
            _harness.Uow,
            _harness.PasswordHasher,
            new OptionsWrapper<AdminSeedOptions>(new AdminSeedOptions
            {
                UserName = userName,
                Password = password
            }),
            NullLogger<AdminSeeder>.Instance);

    [Fact]
    public async Task Seed_MissingAdmin_CreatesAdminWithAdminRole()
    {
        await CreateSeeder().SeedAsync();

        var admin = Assert.Single(_harness.Users.Items);
        Assert.Equal("admin", admin.UserName);
        Assert.True(admin.IsAdmin);
        Assert.Equal(1, _harness.Uow.SaveCount);
    }

    [Fact]
    public async Task Seed_MissingAdmin_HashesConfiguredPassword()
    {
        await CreateSeeder(password: "secret123").SeedAsync();

        var admin = Assert.Single(_harness.Users.Items);
        Assert.Equal("hash:secret123", admin.PasswordHash);
    }

    [Fact]
    public async Task Seed_ExistingAdminByUserName_DoesNotDuplicateOrReset()
    {
        var existing = _harness.AddUser("admin", isAdmin: true);

        await CreateSeeder().SeedAsync();

        Assert.Single(_harness.Users.Items);
        Assert.Equal(existing.PasswordHash, Assert.Single(_harness.Users.Items).PasswordHash);
        Assert.Equal(0, _harness.Uow.SaveCount);
    }

    [Fact]
    public async Task Seed_ExistingNonAdminWithSameName_DoesNotPromote()
    {
        var existing = _harness.AddUser("admin", isAdmin: false);

        await CreateSeeder().SeedAsync();

        Assert.Single(_harness.Users.Items);
        Assert.Equal(existing.Id, Assert.Single(_harness.Users.Items).Id);
        Assert.Equal(0, _harness.Uow.SaveCount);
    }

    [Fact]
    public async Task Seed_EmptyUserName_ThrowsInvalidOperation()
        => await Assert.ThrowsAsync<InvalidOperationException>(
            () => CreateSeeder(userName: "  ").SeedAsync());

    [Fact]
    public async Task Seed_EmptyPassword_ThrowsInvalidOperation()
        => await Assert.ThrowsAsync<InvalidOperationException>(
            () => CreateSeeder(password: "").SeedAsync());
}
