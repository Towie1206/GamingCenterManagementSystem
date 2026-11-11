using System.Collections.Generic;

namespace GamingCenterManagementSystem.OOP
{
    // Generic Interface chuẩn mực
    public interface IRepository<T>
    {
        List<T> GetAll();
        T? GetById(int id);
        bool Add(T entity);
        bool Update(T entity);
        bool Delete(int id);
    }
}
