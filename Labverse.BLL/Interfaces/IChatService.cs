namespace Labverse.BLL.Interfaces;

public interface IChatService
{
    Task<string> AskAsync(string message, CancellationToken ct = default);
}
