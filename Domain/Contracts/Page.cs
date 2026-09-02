namespace Domain.Contracts;

/// <summary>
/// Страница результатов cursor-based пагинации. <see cref="NextCursor"/> — Id последнего
/// элемента страницы, переданный в следующий запрос как QueryParams.Cursor.
/// null означает, что данных больше нет.
/// </summary>
public sealed record Page<T>(IReadOnlyCollection<T> Items, string? NextCursor);