using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Data;
using System.Transactions;
using System.Data.Entity;
using System.Data.Entity.Core.Objects;
using System.Data.Entity.Core;


namespace AJZ.FCL.Data
{
    /// <summary>
    /// 实体框架数据会话
    /// </summary>
    public class EntityFrameworkSession : ISession 
    {
        private bool _flag;

        /// <summary>
        /// 是否分布式事务
        /// </summary>
        private bool Distributed
        {
            set;
            get;
        }
        
        private TransactionScope Scope
        {
            get;
            set;
        }


        public DbContext DbContext
        { 
            set; 
            get; 
        }
        
        /// <summary>
        /// 数据库连接对象
        /// </summary>
        public IDbConnection Connection
        {
            private set;
            get;
        }

        /// <summary>
        /// 数据库事务对象
        /// </summary>
        public IDbTransaction Transaction
        {
            private set;
            get;
        }

        /// <summary>
        /// 是否处于事务过程中
        /// </summary>
        public bool IsTransaction
        {
            get 
            {
                return this._flag;
            }
        }

        /// <summary>
        /// 构造器
        /// </summary>
        /// <param name="dbContext">具体的实体上下文</param>
        public EntityFrameworkSession(DbContext dbContext)
        {
            this.DbContext = dbContext;
        }


        //public EntityFrameworkSession()
        //{
        //    this.DbContext = new RSFBaseEntities();
        //}

        /// <summary>
        /// 开始事务
        /// </summary>
        /// <param name="isolation"></param>
        /// <returns></returns>
        public IDbTransaction BeginTransaction(System.Data.IsolationLevel isolation = System.Data.IsolationLevel.ReadCommitted)
        {
            this._flag = true;
            return this.Transaction;
        }

        /// <summary>
        /// 开始分布式事务
        /// </summary>
        /// <param name="isolation"></param>
        /// <returns></returns>
        public TransactionScope BeginDistributedTransaction(System.Transactions.IsolationLevel isolation = System.Transactions.IsolationLevel.ReadCommitted)
        {
            this.Distributed = true;
            this.Scope = new TransactionScope();
            return this.Scope;
        }

        /// <summary>
        /// 提交事务
        /// </summary>
        public void CommitTransaction()
        {
            this.DbContext.SaveChanges();
        }

        /// <summary>
        /// 提交分布式事务
        /// </summary>
        public void CommitDistributedTransaction()
        {
            this.Scope.Complete();
        }

        /// <summary>
        /// 回滚事务
        /// </summary>
        public void RollbackTransaction()
        {
             
        }

        /// <summary>
        /// 资源释放
        /// </summary>
        public void Dispose()
        {
            this.DbContext.Dispose();

            if (this.Distributed)
            {
                this.Scope.Dispose();
            }
        }
    }
}
