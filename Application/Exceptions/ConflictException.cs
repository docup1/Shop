namespace Application.Exceptions;

/// <summary>Конфликт с текущим состоянием (например, уже занятое имя пользователя).</summary>
public class ConflictException(string message) : Exception(message);