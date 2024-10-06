using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using EffiHR.Application.Interfaces;
using EffiHR.Application.Data;
using Microsoft.Data.SqlClient;


namespace EffiHR.Application.Repositories
{
    internal class GenericRepository
    {
        private readonly DataContext  _context;

        public GenericRepository(DataContext context)
        {
            _context = context;
        }

        public IQueryable<T> GetAll<T>() where T : class
        {
            return _context.Set<T>();
        }


        public async Task AddAsync<T>(T entity) where T : class
        {
            await _context.Set<T>().AddAsync(entity);
        }


        public async Task UpdateAsync<T>(T entity) where T : class
        {
            var dbSet = _context.Set<T>();
            var entityEntry = _context.Entry(entity);

            if (entityEntry.State == EntityState.Detached)
            {
                dbSet.Attach(entity);
            }

            entityEntry.State = EntityState.Modified;
        }

        public async Task DeleteAsync<T>(Expression<Func<T, bool>> predicate = null) where T : class
        {
            var dbSet = _context.Set<T>();
            List<T> entities;

            if (predicate == null)
            {
                entities = await dbSet.ToListAsync();
            }
            else
            {
                entities = await dbSet.Where(predicate).ToListAsync();
            }

            if (entities.Any())
            {
                dbSet.RemoveRange(entities);
            }
        }


        public async Task SaveChangesAsync()
        {
            await _context.SaveChangesAsync();
        }

        public async Task<bool> ExistsAsync<T>(Guid id) where T : class
        {
            return await _context.Set<T>().FindAsync(id) != null;
        }

        public async Task<bool> SaveAsync()
        {
            return await _context.SaveChangesAsync() > 0;
        }

        public IQueryable<T> Get<T>(Expression<Func<T, bool>> predicate) where T : class
        {
            return _context.Set<T>().Where(predicate);
        }

        public IQueryable<T> GetAllAsync<T>() where T : class
        {
            return _context.Set<T>();
        }


        public async Task<T> GetOneAsync<T>(Expression<Func<T, bool>> predicate) where T : class
        {
            return await _context.Set<T>().FirstOrDefaultAsync(predicate);
        }

        public IQueryable<T> GetAsync<T>(Expression<Func<T, bool>> predicate) where T : class
        {
            return _context.Set<T>().Where(predicate);
        }

        public async Task<int> ExecProcAsync(string storedProcedureName, params SqlParameter[] parameters)
        {
            using (var command = _context.Database.GetDbConnection().CreateCommand())
            {
                command.CommandText = storedProcedureName;
                command.CommandType = CommandType.StoredProcedure;

                // Add parameters to the command
                command.Parameters.AddRange(parameters);

                if (command.Connection.State != ConnectionState.Open)
                {
                    await command.Connection.OpenAsync();
                }

                return await command.ExecuteNonQueryAsync();
            }
        }


    }

}
