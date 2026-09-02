namespace Application.Exceptions;

/// <summary>Запрошенная сущность не найдена (или скрыта политикой доступа).</summary>
public class NotFoundException(string message) : Exception(message);