namespace BaretStoreWebAPI.DAL
{
    public interface ICrud<T>
    {
        Task<IEnumerable<T>> GetAll();
        Task<IEnumerable<T>> GetByName(string name);
        Task<T> GetById(int id);
        Task<T> Insert(T obj);
        Task<T> Update(T obj);
        Task Delete(int id);
    }
}
