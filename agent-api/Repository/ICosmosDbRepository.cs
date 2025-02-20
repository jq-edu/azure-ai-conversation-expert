namespace AgentApi.Repository
{
    public interface ICosmosDbRepository
    {
        Task<IEnumerable<T>> GetItems<T>(string query, IDictionary<string, object> parameters) where T : class;
        Task<T> InsertAsync<T>(T item) where T : class;
    }
}