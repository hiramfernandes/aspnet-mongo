using Telegram.Bot.Types;

namespace Purchases.Domain.Contracts;

public interface IRemoteFileManager
{
    public Task<TGFile> GetFile(string fileId);
    public Task DownloadFile(string filePath, Stream stream, CancellationToken cancellationToken);
}
