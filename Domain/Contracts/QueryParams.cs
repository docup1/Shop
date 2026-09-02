namespace Domain.Contracts;

/// <summary>
/// Параметры cursor-based пагинации. <see cref="Cursor"/> — Id последнего элемента
/// предыдущей страницы (null для первой страницы), pageSize ограничен 1..100.
/// </summary>
public sealed record QueryParams(string? Cursor = null, int PageSize = 20);