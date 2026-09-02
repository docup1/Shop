namespace Application.Exceptions;

/// <summary>Аутентификация не пройдена или недостаточно прав.</summary>
public class UnauthorizedException(string message) : Exception(message);