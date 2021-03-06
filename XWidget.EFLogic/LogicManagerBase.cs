﻿using Microsoft.EntityFrameworkCore;
using System;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;

namespace XWidget.EFLogic {
    /// <summary>
    /// 邏輯管理器
    /// </summary>
    public abstract class LogicManagerBase<TContext> where TContext : DbContext {
        internal DynamicLogicMapBuilder<TContext> MapBuilder { get; set; }

        /// <summary>
        /// DI服務提供者
        /// </summary>
        public IServiceProvider ServiceProvider { get; internal set; }

        /// <summary>
        /// 資料庫上下文
        /// </summary>
        public TContext Database { get; internal set; }

        /// <summary>
        /// 動態邏輯對應
        /// </summary>
        public DynamicLogicMap<TContext> DynamicLogicMap {
            get {
                return new DynamicLogicMap<TContext>() {
                    Maps = MapBuilder.Maps
                };
            }
        }

        /// <summary>
        /// 邏輯管理器建構子
        /// </summary>
        /// <param name="database">資料庫上下文</param>
        public LogicManagerBase(TContext database) {
            Database = database;
        }

        /// <summary>
        /// 取得動態類型操作邏輯
        /// </summary>
        /// <typeparam name="TEntity">實例類型</typeparam>
        /// <typeparam name="TId">唯一識別號類型</typeparam>
        /// <returns>操作邏輯</returns>
        public LogicBase<TContext, TEntity, TId> GetLogicByType<TEntity, TId>()
            where TEntity : class {
            return (LogicBase<TContext, TEntity, TId>)GetLogicByType(typeof(TEntity));
        }

        internal object GetLogicByType(Type type) {
            var prop = this.GetType()
                .GetProperties()
                .SingleOrDefault(
                    x =>
                        (
                            x.PropertyType.IsGenericType &&
                            x.PropertyType.GetGenericTypeDefinition() == typeof(LogicBase<,,>) &&
                            x.PropertyType.GenericTypeArguments[1] == type
                        ) || (
                            x.PropertyType.BaseType != null &&
                            x.PropertyType.BaseType.IsGenericType &&
                            x.PropertyType.BaseType.GetGenericTypeDefinition() == typeof(LogicBase<,,>) &&
                            x.PropertyType.BaseType.GenericTypeArguments[1] == type
                        )
                );

            if (prop == null) {
                return DynamicLogicMap.GetLogicByType(this, type);
            }

            return prop.GetValue(this);
        }

        /// <summary>
        /// 透過唯一識別號取得指定物件實例
        /// </summary>
        /// <typeparam name="T">實例類型</typeparam>
        /// <param name="id">唯一識別號</param>
        /// <param name="parameters">參數</param>
        /// <returns>物件實例</returns>
        public async Task<T> GetAsync<T>(object id, object[] parameters = null) where T : class {
            var targetLogic = (dynamic)GetLogicByType(typeof(T));

            return await ((dynamic)targetLogic).GetAsync(id, parameters);
        }

        /// <summary>
        /// 透過唯一識別號取得指定物件實例
        /// </summary>
        /// <param name="type">實例類型</param>
        /// <param name="id">唯一識別號</param>
        /// <param name="parameters">參數</param>
        /// <returns>物件實例</returns>
        public async Task<object> GetAsync(Type type, object id, object[] parameters = null) {
            var targetLogic = (dynamic)GetLogicByType(type);

            return await ((dynamic)targetLogic).GetAsync(id, parameters);
        }

        /// <summary>
        /// 加入新的物件實例
        /// </summary>
        /// <param name="entity">物件實例</param>
        /// <param name="parameters">參數</param>
        /// <returns>加入後的物件</returns>
        public async Task<T> CreateAsync<T>(T entity, object[] parameters = null) where T : class {
            var targetLogic = (dynamic)GetLogicByType(typeof(T));

            return await ((dynamic)targetLogic).CreateAsync(entity, parameters);
        }

        /// <summary>
        /// 更新指定的物件實例
        /// </summary>
        /// <param name="entity">物件實例</param>
        /// <param name="parameters">參數</param>
        /// <returns>加入後的物件</returns>
        public async Task<T> UpdateAsync<T>(T entity, object[] parameters = null) where T : class {
            var targetLogic = (dynamic)GetLogicByType(typeof(T));

            return await ((dynamic)targetLogic).UpdateAsync(entity, parameters);
        }

        /// <summary>
        /// 刪除指定的物件
        /// </summary>
        /// <param name="id">唯一識別號</param>
        /// <param name="parameters">參數</param>
        public async Task DeleteAsync<T>(object id, object[] parameters = null) where T : class {
            var targetLogic = (dynamic)GetLogicByType(typeof(T));

            await targetLogic.DeleteAsync(id, parameters);
        }

        /// <summary>
        /// 刪除指定的物件
        /// </summary>
        /// <param name="type">實例類型</param>
        /// <param name="id">唯一識別號</param>
        /// <param name="parameters">參數</param>
        public async Task DeleteAsync(Type type, object id, object[] parameters = null) {
            var targetLogic = (dynamic)GetLogicByType(type);

            await targetLogic.DeleteAsync(id, parameters);
        }

        /// <summary>
        /// 取得指定實例唯一識別號值
        /// </summary>
        /// <param name="entity">物件實例</param>
        /// <returns>唯一識別號</returns>
        public async Task<object> GetEntityIdentity(object entity) {
            var entryType = entity.GetType();
            dynamic logic = GetLogicByType(entryType);
            return entryType.GetProperty((string)logic.IdentityPropertyName).GetValue(entity);
        }

        /// <summary>
        /// 取得指定實例唯一識別號屬性
        /// </summary>
        /// <param name="entity">物件實例</param>
        /// <returns>唯一識別號屬性</returns>
        public async Task<PropertyInfo> GetEntityIdentityProperty(object entity) {
            var entryType = entity.GetType();
            dynamic logic = GetLogicByType(entryType);
            return entryType.GetProperty((string)logic.IdentityPropertyName);
        }


        /// <summary>
        /// 取得直接關聯物件鏈中所有物件包含自身，如類型A有一對多的B類型屬性，則使用其中B類型物鍵取得直接關聯物件鏈則為[B,A]
        /// </summary>
        /// <typeparam name="TEntity">實例類型</typeparam>
        /// <param name="id">物件唯一識別號</param>
        /// <param name="parameters">參數</param>
        /// <returns>直接關聯物件鏈物件陣列</returns>
        public async Task<object[]> GetDirectChain<TEntity>(object id, params object[] parameters)
            where TEntity : class {
            return await GetDirectChain(typeof(TEntity), await GetAsync<TEntity>(id, parameters), parameters);
        }

        /// <summary>
        /// 取得直接關聯物件鏈中所有物件包含自身，如類型A有一對多的B類型屬性，則使用其中B類型物鍵取得直接關聯物件鏈則為[B,A]
        /// </summary>
        /// <param name="type">實例類型</param>
        /// <param name="entity">物件實例</param>
        /// <param name="parameters">參數</param>
        /// <returns>直接關聯物件鏈物件陣列</returns>
        public async Task<object[]> GetDirectChain(Type type, object entity, params object[] parameters) {
            var logic = (dynamic)GetLogicByType(type);
            return await logic.GetDirectChain(entity, parameters);
        }
    }
}