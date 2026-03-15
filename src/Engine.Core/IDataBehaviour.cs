namespace Engine.Core;

public interface IDataBehaviour<T> : IBehaviour
{
    Task<T> GetDataAsync(CancellationToken ct = default);

    Task SetDataAsync(T data, CancellationToken ct = default);
}
