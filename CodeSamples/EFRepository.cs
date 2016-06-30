using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Entity;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using EntityFramework.Extensions;

using AJZ.FCL;

namespace AJZ.FCL.Data
{
    /// <summary>
    /// Repository base class
    /// </summary>
    /// <typeparam name="TEntity">The type of underlying entity in this repository</typeparam>
    public class EFRepository<TEntity> : IRepository<TEntity> where TEntity : class
    {
        /// <summary>
        /// 数据会话操作接口
        /// </summary>
        public ISession Session
        {
            get;
            set;
        }

        /// <summary>
        /// EF的数据会话
        /// </summary>
        public EntityFrameworkSession EFSession
        {
            get 
            { 
                return this.Session as EntityFrameworkSession; 
            }
        }  
        
        /// <summary>
        /// 实体上下文中实体集
        /// </summary>
        private DbSet<TEntity> Set
        {
            get 
            { 
                return this.EFSession.DbContext.Set<TEntity>(); 
            }
        }

        public EFRepository()
        {
            
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="item"></param>
        public virtual void Add(TEntity item)
        {
            if (item != null)
            {
                this.Set.Add(item);
                if(!this.EFSession.IsTransaction)
                {
                    this.EFSession.DbContext.SaveChanges();
                }
            }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="item"></param>
        public virtual void Add(IEnumerable<TEntity> itemList)
        {
            if (itemList != null)
            {
                this.Set.AddRange(itemList);
                if (!this.EFSession.IsTransaction)
                {
                    this.EFSession.DbContext.SaveChanges();
                }
            }
        }
        /// <summary>
        /// 删除实体
        /// </summary>
        /// <param name="item"></param>
        public virtual void Remove(TEntity item)
        {
            if (this.EFSession.DbContext.Entry(item).State == EntityState.Detached)
            {
                this.Set.Attach(item);
            }
            this.Set.Remove(item);
            if (!this.EFSession.IsTransaction)
            {
                this.EFSession.DbContext.SaveChanges();
            }
        }

        /// <summary>
        /// 条件删除实体
        /// </summary>
        /// <param name="expression"></param>
        public virtual void Remove(Expression<Func<TEntity, bool>> expression)
        {
            if (expression != null)
            {
                this.Set.Where<TEntity>(expression).Delete();
                if (!this.EFSession.IsTransaction)
                {
                    this.EFSession.DbContext.SaveChanges();
                }
            }
        }

        /// <summary>
        /// 更新
        /// </summary>
        /// <param name="updateExpression">更新表达式</param>
        /// <param name="filterExpression">条件表达式</param>
        public virtual int Update(Expression<Func<TEntity, TEntity>> updateExpression, Expression<Func<TEntity, bool>> filterExpression)
        {
            int affected = this.Set.Where(filterExpression).Update(updateExpression);
            if (!this.EFSession.IsTransaction)
            {
                this.EFSession.DbContext.SaveChanges();
            }

            return affected;
        }

        ///// <summary>
        ///// <see cref="IRepository{TEntity}"/>
        ///// </summary>
        ///// <param name="item"><see cref="IRepository{TEntity}"/></param>
        //public virtual void Modify<T>(T item) where T : Entity
        //{
        //    GetDbSet<T>().Attach(item);
        //    _dbContext.Entry(item).State = EntityState.Modified;
        //}

        //#endregion



        /// <summary>
        /// 根据键获取单个实体
        /// </summary>
        /// <param name="keyValues"></param>
        /// <returns></returns>
        public virtual TEntity Get(object keyValue)
        {
            if (keyValue != null)
            {
                return this.Set.Find(keyValue);
            }
            else
            {
                return default(TEntity);
            }
        }

        /// <summary>
        /// 根据键获取多个实体
        /// </summary>
        /// <param name="keyValues"></param>
        /// <returns></returns>
        public virtual TEntity Get(params object[] keyValues)
        {
            if (keyValues != null)
            {
                return this.Set.Find(keyValues);
            }
            else
            {
                return default(TEntity);
            }
        }

        /// <summary>
        /// 根据条件获取唯一实体
        /// </summary>
        /// <param name="expression">条件</param>
        /// <returns></returns>
        public virtual TEntity Single(Expression<Func<TEntity, bool>> expression)
        {
            if (expression != null)
            {
                return this.Set.SingleOrDefault(expression);
            }
            return default(TEntity);
        }

        /// <summary>
        /// 根据条件获取第一个实体
        /// </summary>
        /// <param name="expression">条件</param>
        /// <param name="orderBy">排序</param>
        /// <returns></returns>
        public virtual TEntity First(Expression<Func<TEntity, bool>> expression, Func<IQueryable<TEntity>, IOrderedQueryable<TEntity>> orderBy)
        {
            if (expression == null)
            {
                return default(TEntity);
            }

            return ( orderBy == null ) ? this.Set.FirstOrDefault(expression) : orderBy(this.Set).FirstOrDefault(expression);

        }

        /// <summary>
        /// 返回记录数
        /// </summary>
        /// <param name="expression">条件表达式</param>
        /// <returns></returns>
        public virtual long Count(Expression<Func<TEntity, bool>> expression)
        {
            return this.Set.Where(expression).Count();
        }

        /// <summary>
        /// 判断实体是否存在
        /// </summary>
        /// <param name="expression">条件表达式</param>
        /// <returns></returns>
        public virtual bool Exists(Expression<Func<TEntity, bool>> expression)
        {
            return this.Set.Any(expression);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        public IEnumerable<TEntity> GetAll()
        {
            return this.Set;
        }

        /// <summary>
        /// 获取过滤结果
        /// </summary>
        /// <param name="filter">条件表达式</param>
        /// <param name="orderBy">排序表达式</param>
        /// <returns></returns>
        public virtual IEnumerable<TEntity> GetList(Expression<Func<TEntity, bool>> filter, Func<IQueryable<TEntity>, IOrderedQueryable<TEntity>> orderBy = null)
        {
            return (orderBy == null) ? this.Set.Where(filter) : orderBy(this.Set.Where(filter));
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="searchParam"></param>
        /// <param name="expression"></param>
        /// <param name="orderBy"></param>
        /// <returns></returns>
        public IPagedList<TEntity> GetPagedList(SearchBase search, Expression<Func<TEntity, bool>> expression, Func<IQueryable<TEntity>, IOrderedQueryable<TEntity>> orderBy)
        {
            return this.GetPagedList(search.Pager.CurrentPageIndex, search.Pager.PageSize, expression, orderBy);
        }

        /// <summary>
        /// 根据条件表达式获取满足条件的所有类型为T的数据
        /// </summary>
        /// <param name="pageIndex">页码</param>
        /// <param name="pageSize">每页数据量</param>
        /// <param name="expression">条件表达式</param>
        /// <param name="orderBy">排序表达式 如: Func<IQueryable<TEntity>, IOrderedQueryable<TEntity>> orderBy = o => o.OrderBy(g => g.SKU).OrderByDescending(g => g.SKU)</param>
        /// <returns>返回满足条件的所有类型为T的数据和分页信息</returns>
        public IPagedList<TEntity> GetPagedList(int pageIndex, int pageSize, Expression<Func<TEntity, bool>> expression, Func<IQueryable<TEntity>, IOrderedQueryable<TEntity>> orderBy)
        {
            var query = this.Set.Where(expression);
            if (orderBy != null)
            {
                orderBy(query);
            }

            PagedList<TEntity> pagedList = new PagedList<TEntity>(pageIndex, pageSize);
            pagedList.TotalItemCount = query.Count();

            int skipCount = 0;
            if (pageIndex > 1)
            {
                skipCount = pageSize * pageIndex - pageSize;
            }
            pagedList.Lists = query.Skip(skipCount).Take(pageSize).ToList();

            return pagedList;
        }

        ///// <summary>
        ///// 
        ///// </summary>
        ///// <param name="searchParam"></param>
        ///// <param name="expression"></param>
        ///// <param name="orderBy"></param>
        ///// <returns></returns>
        //public virtual IEnumerable<TEntity> GetPaged(SearchBase search, Expression<Func<TEntity, bool>> expression, Func<IQueryable<TEntity>, IOrderedQueryable<TEntity>> orderBy)
        //{
        //    return this.GetPaged(search.Pager.CurrentPageIndex, search.Pager.PageSize, expression, orderBy);
        //}

        ///// <summary>
        ///// <see cref="IRepository{TEntity}"/>
        ///// </summary>
        ///// <param name="pageIndex"></param>
        ///// <param name="pageCount"></param>
        ///// <param name="expression"></param>
        ///// <param name="orderBy"></param>
        ///// <returns></returns>
        //public virtual IEnumerable<TEntity> GetPaged(int pageIndex, int pageCount, Expression<Func<TEntity, bool>> expression, Func<IQueryable<TEntity>, IOrderedQueryable<TEntity>> orderBy)
        //{
        //    if (orderBy == null)
        //    {
        //        return this.Set.Skip(pageCount * (pageIndex - 1))
        //                        .Take(pageCount);
        //    }

        //    return orderBy(this.Set)
        //                .Skip(pageCount * (pageIndex - 1))
        //                .Take(pageCount);
        //}

        #region BulkInsert

        //public virtual void BulkInsert<T>(IEnumerable<T> entities, int? batchSize = null)
        //{
        //    _dbContext.BulkInsert(entities, batchSize);
        //}
        //public virtual void BulkInsert<T>(IEnumerable<T> entities, BulkInsertOptions options)
        //{
        //    _dbContext.BulkInsert(entities, options);
        //}

        //public virtual void BulkInsert<T>(IEnumerable<T> entities, SqlBulkCopyOptions sqlBulkCopyOptions, int? batchSize = null)
        //{
        //    _dbContext.BulkInsert(entities, sqlBulkCopyOptions, batchSize);
        //}
        //public virtual void BulkInsert<T>(IEnumerable<T> entities, IDbTransaction transaction, SqlBulkCopyOptions sqlBulkCopyOptions = SqlBulkCopyOptions.Default, int? batchSize = null)
        //{
        //    _dbContext.BulkInsert(entities, transaction, sqlBulkCopyOptions, batchSize);
        //}

        #endregion

        #region IDisposable Members

        /// <summary>
        /// <see cref="M:System.IDisposable.Dispose"/>
        /// </summary>
        public void Dispose()
        {
            if (this.EFSession != null)
            {
                this.EFSession.Dispose();
            }
        }

        #endregion

        #region GetQuery


        /// <summary>
        /// 获取IQueryable
        /// </summary>
        /// <returns>IQueryable</returns>
        public IQueryable<TEntity> GetQuery()
        {
            return GetQuery(null);
        }

        /// <summary>
        /// 根据条件获取IQueryable
        /// </summary>
        /// <param name="predicate">查询条件表达式</param>
        /// <returns>IQueryable</returns>
        public IQueryable<TEntity> GetQuery(Expression<Func<TEntity, bool>> predicate)
        {
            return GetQuery(predicate, null);
        }

        /// <summary>
        /// 根据条件获取列表
        /// </summary>
        /// <param name="predicate">查询条件表达式</param>
        /// <param name="orderBy">排序</param>
        /// <returns>实体列表</returns>
        public IQueryable<TEntity> GetQuery(Expression<Func<TEntity, bool>> predicate, Func<IQueryable<TEntity>, IOrderedQueryable<TEntity>> orderBy)
        {
            if (orderBy != null)
            {
                return orderBy(predicate == null ? this.Set : this.Set.Where(predicate));
            }
            return ( predicate == null ) ? this.Set : this.Set.Where(predicate);
        }


        ///// <summary>
        ///// 获取IQueryable
        ///// </summary>
        ///// <returns>IQueryable</returns>
        //public IQueryable<T> GetQuery<T>() where T : Entity
        //{
        //    return GetQuery<T>(null);
        //}

        ///// <summary>
        ///// 根据条件获取列表
        ///// </summary>
        ///// <param name="predicate">查询条件表达式</param>
        ///// <returns>实体列表</returns>
        //public IQueryable<T> GetQuery<T>(Expression<Func<T, bool>> predicate) where T : Entity
        //{
        //    return GetQuery(predicate, null);
        //}

        ///// <summary>
        ///// 根据条件获取列表
        ///// </summary>
        ///// <param name="predicate">查询条件表达式</param>
        ///// <param name="orderBy">排序</param>
        ///// <returns>实体列表</returns>
        //public IQueryable<T> GetQuery<TEntity>(Expression<Func<T, bool>> predicate, Func<IQueryable<T>, IOrderedQueryable<T>> orderBy) where T : Entity
        //{
        //    var set = this.Set;

        //    if (orderBy != null)
        //    {
        //        return orderBy(predicate == null ? set : this.Set.Where(predicate));
        //    }
        //    return predicate == null ? set : set.Where(predicate);
        //}

        //#endregion

        //#region DbSet

        //private DbSet<TEntity> DbSet
        //{
        //    get { return _dbContext.Set<TEntity>(); }
        //}

        //private DbSet<T> GetDbSet<T>() where T : Entity
        //{
        //    return _dbContext.Set<T>();
        //}

        #endregion

        #region

        /*
        /// <summary>
        /// 执行sql语句（DDL/DML）
        /// </summary>
        /// <param name="sql"></param>
        /// <param name="parameters"></param>
        /// <returns></returns>
        public int ExecuteCommand(string sql, params object[] parameters)
        {
            return _dbContext.Database.ExecuteSqlCommand(sql, parameters);
        }

        public IEnumerable<TEntity> ExecuteQuery(string sql, params object[] parameters)
        {

            return DbSet.SqlQuery(sql, parameters);
        }

        /// <summary>
        /// 返回dataset(更新于20140418)
        /// </summary>
        /// <param name="sql"></param>
        /// <param name="parameters"></param>
        /// <returns></returns>
        public DataSet SqlQueryForDataSet(string sql, SqlParameter[] parameters)
        {
            SqlConnection conn = new SqlConnection();
            conn.ConnectionString = _dbContext.Database.Connection.ConnectionString;
            if (conn.State != ConnectionState.Open)
            {
                conn.Open();
            }
            SqlCommand cmd = new SqlCommand();
            cmd.Connection = conn;
            cmd.CommandText = sql;

            if (parameters.Length > 0)
            {
                foreach (var item in parameters)
                {
                    cmd.Parameters.Add(item);
                }
            }
            SqlDataAdapter adapter = new SqlDataAdapter(cmd);
            DataSet dataSet = new DataSet();
            adapter.Fill(dataSet);
            return dataSet;
        }
        */

        #endregion

    }
}
