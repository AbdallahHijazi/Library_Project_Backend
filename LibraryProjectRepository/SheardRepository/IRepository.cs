using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LibraryProjectRepository.SheardRepository
{
    public interface IRepository<T>
    {
        T Add(T entity);
        T Delete(T entity);
        IList<T> GetAll();
        T GetById(Guid id);
        T Update(T entity);
        void SaveChange();
    }
}
