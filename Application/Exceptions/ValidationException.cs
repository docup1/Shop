namespace Application.Exceptions;

/// <summary>Домен/запрос не валиден: неверные входные данные, нарушение бизнес-правил.</summary>
public class ValidationException(string message, string? propertyName = null) : Exception(message)
{
    public string? PropertyName { get; } = propertyName;
}