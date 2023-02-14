﻿using System.Linq.Expressions;

namespace EasMe.EntityFrameworkCore.Abstract
{
    public interface IEntityRepository<T>
    where T : class, IEntity, new()
    {
        IQueryable<T> Get(Expression<Func<T, bool>>? filter = null);
        List<T> GetList(Expression<Func<T, bool>>? filter = null);
        bool Any(Expression<Func<T, bool>> filter);
        bool Any();
        T? GetFirstOrDefault(Expression<Func<T, bool>> filter);
        T? GetFirst(Expression<Func<T, bool>> filter);
        T GetSingle(Expression<Func<T, bool>> filter);
        T? GetSingleOrDefault(Expression<Func<T, bool>> filter);
        T GetSingle();
        T? GetFirst();
        void Add(T entity);
        void AddRange(IEnumerable<T> entities);
        void Update(T entity);
        void Update(T entity, Action<T> updateAction);
        void UpdateRange(IEnumerable<T> entities);
        //int UpdateWhere(Expression<Func<T, bool>> filter, Action<T> updateAction);
        //int DeleteWhere(Expression<Func<T, bool>> filter);
        //bool UpdateWhereSingle(Expression<Func<T, bool>> filter, Action<T> updateAction);
        //bool DeleteWhereSingle(Expression<Func<T, bool>> filter);
        void Delete(T entity);
        void Delete(int id);
        void DeleteRange(IEnumerable<T> entities);
        T? Find(int id);
        int Count(Expression<Func<T, bool>> filter);
        int Count();
        //bool Save();
        //Task<bool> SaveAsync();

    }
}
