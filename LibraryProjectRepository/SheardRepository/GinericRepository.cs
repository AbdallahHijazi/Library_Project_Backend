using LibraryProject;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LibraryProjectRepository.SheardRepository
{
    public class GinericRepository<T> : IRepository<T> where T : class
    {
        private readonly LibraryDbContext context;

        public GinericRepository(LibraryDbContext context)
        {
            this.context = context;
        }
        public T Add(T entity)
        {
            var newEntity = context.Add(entity);
            return newEntity.Entity;
        }

        public T Delete(T entity)
        {
            if (entity == null)
            {
                return null;
            }
            context.Remove(entity);
            return entity;
        }

        public IList<T> GetAll()
        {
            var entity = context.Set<T>().ToList();
            return entity;
        }

        public T GetById(Guid id)
        {
            var entity = context.Set<T>().Find(id);
            if (entity == null)
            {
                return null;
            }

            return entity;
        }

        public void SaveChange()
        {
            context.SaveChanges();
        }

        public T Update(T entity)
        {
            var item = context.Update(entity);
            return item.Entity;
        }
    }
}
