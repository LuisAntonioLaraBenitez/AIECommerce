using AIChallenge.Models;

namespace AIChallenge.Repositories;

public interface IEcommerceRepository
{
    Task<AppData> ReadAsync(CancellationToken cancellationToken = default);

    Task WriteAsync(AppData data, CancellationToken cancellationToken = default);
}
