namespace Infrastructure.DataBase;

/// <summary>
/// Параметры автоматического seed-администратора. Читаются из секции "Seed:Admin"
/// конфигурации. Логин проверяется на уникальность при первом запуске.
/// </summary>
public sealed class AdminSeedOptions
{
    public const string SectionName = "Seed:Admin";

    public string UserName { get; set; } = "admin";

    public string Password { get; set; } = "admin1234";
}
