namespace Crypton.WebUIOld.HttpMessageHandlers;

public sealed record ProblemDetails(string Title, int Status, Dictionary<string, IEnumerable<string>> Errors);