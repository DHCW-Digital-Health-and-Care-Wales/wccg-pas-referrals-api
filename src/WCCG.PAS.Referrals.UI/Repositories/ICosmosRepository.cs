namespace WCCG.PAS.Referrals.UI.Repositories;

public interface ICosmosRepository<T>
{
    public Task<bool> UpsertAsync(T item);
    public Task<IEnumerable<T>> GetAllAsync();
    public Task<T> GetByIdAsync(string id);
}
