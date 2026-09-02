using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using Application.Services;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Presentation.Controllers;
using Tests.Application;

namespace Tests.Presentation;

/// <summary>
/// Собирает сервисы на in-memory фейках и инстанцирует контроллеры с claim'ом sub
/// (моделирует аутентифицированный запрос). Ошибки и маршрутизацию контроллеры
/// не обрабатывают — они проходят через GlobalExceptionMiddleware, который покрыт
/// отдельными тестами.
/// </summary>
internal sealed class ControllerTestContext
{
    /// <summary>Фиксированное «сейчас» — сессии, созданные тестами, считаются активными.</summary>
    public static readonly DateTimeOffset Now = new(2026, 1, 15, 12, 0, 0, TimeSpan.Zero);

    public ControllerTestContext()
    {
        Harness = new ServiceTestHarness(Now);
    }

    public ServiceTestHarness Harness { get; }

    public AuthController AuthController(Guid? callerId = null)
        => WithUser(new AuthController(new AuthService(
                Harness.Users, Harness.Sessions, Harness.PasswordHasher, Harness.Tokens, Harness.Uow,
                TimeSpan.FromDays(30)), new UserService(Harness.Users, Harness.Uow)), callerId);

    public OrdersController OrdersController(Guid? callerId = null)
        => WithUser(new OrdersController(
            new OrderService(Harness.Users, Harness.Orders, Harness.Uow)), callerId);

    public UsersController UsersController(Guid? callerId = null)
        => WithUser(new UsersController(new UserService(Harness.Users, Harness.Uow)), callerId);

    private static T WithUser<T>(T controller, Guid? callerId) where T : ControllerBase
    {
        controller.ControllerContext = new ControllerContext
        {
            HttpContext = new DefaultHttpContext
            {
                User = callerId.HasValue
                    ? new ClaimsPrincipal(new ClaimsIdentity([new Claim(JwtRegisteredClaimNames.Sub, callerId.Value.ToString())], "Test"))
                    : new ClaimsPrincipal(new ClaimsIdentity())
            }
        };
        return controller;
    }
}