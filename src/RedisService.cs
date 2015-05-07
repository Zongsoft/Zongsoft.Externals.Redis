/*
 * Authors:
 *   钟峰(Popeye Zhong) <zongsoft@gmail.com>
 *
 * Copyright (C) 2014-2015 Zongsoft Corporation <http://www.zongsoft.com>
 *
 * This file is part of Zongsoft.Externals.Redis.
 *
 * Zongsoft.Externals.Redis is free software; you can redistribute it and/or
 * modify it under the terms of the GNU Lesser General Public
 * License as published by the Free Software Foundation; either
 * version 2.1 of the License, or (at your option) any later version.
 *
 * Zongsoft.Externals.Redis is distributed in the hope that it will be useful,
 * but WITHOUT ANY WARRANTY; without even the implied warranty of
 * MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the GNU
 * Lesser General Public License for more details.
 *
 * The above copyright notice and this permission notice shall be
 * included in all copies or substantial portions of the Software.
 *
 * You should have received a copy of the GNU Lesser General Public
 * License along with Zongsoft.Externals.Redis; if not, write to the Free Software
 * Foundation, Inc., 51 Franklin Street, Fifth Floor, Boston, MA 02110-1301 USA
 */

using System;
using System.Net;
using System.Collections.Generic;
using System.Collections.Concurrent;
using System.Threading;

using ServiceStack.Redis;

namespace Zongsoft.Externals.Redis
{
	public class RedisService : MarshalByRefObject, IRedisService, IDisposable, Zongsoft.Collections.IQueueProvider, Zongsoft.Runtime.Caching.ICache, Zongsoft.Runtime.Caching.ICacheProvider
	{
		#region 成员字段
		private RedisServiceSettings _settings;
		private Zongsoft.Collections.ObjectPool<IRedisClient> _redisPool;
		private ConcurrentDictionary<int, Zongsoft.Collections.ObjectCache<RedisObjectBase>> _caches;
		#endregion

		#region 构造函数
		public RedisService()
		{
			_settings = new RedisServiceSettings();
			_redisPool = RedisPoolManager.GetRedisPool(_settings.Address, _settings.PoolSize, this.CreateProxy);
		}

		public RedisService(IPEndPoint address, string password = null, int databaseId = 0)
		{
			_settings = new RedisServiceSettings(address, password, databaseId);
			_redisPool = RedisPoolManager.GetRedisPool(_settings.Address, _settings.PoolSize, this.CreateProxy);
		}

		public RedisService(RedisServiceSettings settings)
		{
			if(settings == null)
				throw new ArgumentNullException("settings");

			_settings = settings;
			_redisPool = RedisPoolManager.GetRedisPool(_settings.Address, _settings.PoolSize, this.CreateProxy);
		}

		public RedisService(Configuration.RedisConfigurationElement config)
		{
			if(config == null)
			{
				_settings = new RedisServiceSettings();
			}
			else
			{
				var address = Zongsoft.Communication.IPEndPointConverter.Parse(config.Address);

				if(address.Port == 0)
					address.Port = 6379;

				_settings = new RedisServiceSettings(address, config.Password, config.DatabaseId)
				{
					PoolSize = config.PoolSize,
					Timeout = config.Timeout,
				};
			}

			_redisPool = RedisPoolManager.GetRedisPool(_settings.Address, _settings.PoolSize, this.CreateProxy);
		}
		#endregion

		#region 公共属性
		public RedisServiceSettings Settings
		{
			get
			{
				return _settings;
			}
			set
			{
				if(value == null)
					throw new ArgumentNullException();

				_settings = value;
			}
		}

		public bool IsDisposed
		{
			get
			{
				return _redisPool == null;
			}
		}

		protected ServiceStack.Redis.IRedisClient Proxy
		{
			get
			{
				var redisPool = _redisPool;

				if(redisPool == null)
					throw new ObjectDisposedException("RedisPool");

				return redisPool.GetObject();
			}
		}
		#endregion

		#region 获取集合
		public IRedisHashset GetHashset(string name)
		{
			return this.GetCacheEntry(name, RedisEntryType.Set, key => new RedisHashset(key, _redisPool));
		}

		public IRedisDictionary GetDictionary(string name)
		{
			return this.GetCacheEntry(name, RedisEntryType.Dictionary, key => new RedisDictionary(key, _redisPool));
		}

		public IRedisQueue GetQueue(string name)
		{
			return this.GetCacheEntry(name, RedisEntryType.List, key => new RedisQueue(key, _redisPool));
		}
		#endregion

		#region 公共方法
		public RedisNotification CreateNotification()
		{
			return new RedisNotification(this.CreateProxy());
		}

		public object GetEntry(string key)
		{
			var entryType = this.GetEntryType(key);

			switch(entryType)
			{
				case RedisEntryType.Dictionary:
					return this.GetDictionary(key);
				case RedisEntryType.List:
					return this.GetQueue(key);
				case RedisEntryType.Set:
				case RedisEntryType.SortedSet:
					return this.GetHashset(key);
				case RedisEntryType.String:
					return this.GetValue(key);
			}

			return null;
		}

		public string GetValue(string key)
		{
			if(string.IsNullOrWhiteSpace(key))
				throw new ArgumentNullException("key");

			//获取或创建Redis客户端代理对象
			var redis = this.Proxy;

			try
			{
				return redis.GetValue(key);
			}
			finally
			{
				_redisPool.Release(redis);
			}
		}

		public IEnumerable<string> GetValues(params string[] keys)
		{
			if(keys == null || keys.Length == 0)
				throw new ArgumentNullException("keys");

			//获取或创建Redis客户端代理对象
			var redis = this.Proxy;

			try
			{
				return redis.GetValues(System.Linq.Enumerable.ToList(keys));
			}
			finally
			{
				_redisPool.Release(redis);
			}
		}

		public string ExchangeValue(string key, string value)
		{
			if(string.IsNullOrWhiteSpace(key))
				throw new ArgumentNullException("key");

			//获取或创建Redis客户端代理对象
			var redis = this.Proxy;

			try
			{
				return redis.GetAndSetEntry(key, value);
			}
			finally
			{
				_redisPool.Release(redis);
			}
		}

		public string ExchangeValue(string key, string value, DateTime expires)
		{
			if(string.IsNullOrWhiteSpace(key))
				throw new ArgumentNullException("key");

			//获取或创建Redis客户端代理对象
			var redis = this.Proxy;

			//创建一个Redis事务
			var transaction = redis.CreateTransaction();

			try
			{
				string result = null;

				transaction.QueueCommand(proxy => proxy.GetAndSetEntry(key, value), s => result = s);
				transaction.QueueCommand(proxy => proxy.ExpireEntryAt(key, expires));

				transaction.Commit();

				return result;
			}
			finally
			{
				if(transaction != null)
					transaction.Dispose();

				_redisPool.Release(redis);
			}
		}

		public string ExchangeValue(string key, string value, TimeSpan duration)
		{
			if(string.IsNullOrWhiteSpace(key))
				throw new ArgumentNullException("key");

			//获取或创建Redis客户端代理对象
			var redis = this.Proxy;

			//创建一个Redis事务
			var transaction = redis.CreateTransaction();

			try
			{
				string result = null;

				transaction.QueueCommand(proxy => proxy.GetAndSetEntry(key, value), s => result = s);
				transaction.QueueCommand(proxy => proxy.ExpireEntryIn(key, duration));

				transaction.Commit();

				return result;
			}
			finally
			{
				if(transaction != null)
					transaction.Dispose();

				_redisPool.Release(redis);
			}
		}

		public bool SetValue(string key, string value)
		{
			return this.SetValue(key, value, TimeSpan.Zero);
		}

		public bool SetValue(string key, string value, DateTime expires, bool requiredNotExists = false)
		{
			if(string.IsNullOrWhiteSpace(key))
				throw new ArgumentNullException("key");

			//获取或创建Redis客户端代理对象
			var redis = this.Proxy;

			try
			{
				if(requiredNotExists)
				{
					bool result = false;

					using(var transaction = redis.CreateTransaction())
					{
						transaction.QueueCommand(proxy => proxy.SetEntryIfNotExists(key, value), b => result = b);
						transaction.QueueCommand(proxy => proxy.ExpireEntryAt(key, expires));

						transaction.Commit();
					}

					return result;
				}

				return redis.Set(key, value, expires);
			}
			finally
			{
				_redisPool.Release(redis);
			}
		}

		public bool SetValue(string key, string value, TimeSpan duration, bool requiredNotExists = false)
		{
			if(string.IsNullOrWhiteSpace(key))
				throw new ArgumentNullException("key");

			//获取或创建Redis客户端代理对象
			var redis = this.Proxy;

			try
			{
				if(requiredNotExists)
				{
					bool result = false;

					using(var transaction = redis.CreateTransaction())
					{
						transaction.QueueCommand(proxy => proxy.SetEntryIfNotExists(key, value), b => result = b);
						transaction.QueueCommand(proxy => redis.ExpireEntryIn(key, duration));

						transaction.Commit();
					}

					return result;
				}

				return redis.Set(key, value, duration);
			}
			finally
			{
				_redisPool.Release(redis);
			}
		}

		public RedisEntryType GetEntryType(string key)
		{
			if(string.IsNullOrWhiteSpace(key))
				throw new ArgumentNullException("key");

			//获取或创建Redis客户端代理对象
			var redis = this.Proxy;

			//设置默认返回值
			var entryType = RedisKeyType.None;

			try
			{
				//获取Redis条目类型
				entryType = redis.GetEntryType(key);
			}
			finally
			{
				_redisPool.Release(redis);
			}

			switch(entryType)
			{
				case RedisKeyType.Hash:
					return RedisEntryType.Dictionary;
				case RedisKeyType.List:
					return RedisEntryType.List;
				case RedisKeyType.Set:
					return RedisEntryType.Set;
				case RedisKeyType.SortedSet:
					return RedisEntryType.SortedSet;
				case RedisKeyType.String:
					return RedisEntryType.String;
			}

			return RedisEntryType.None;
		}

		public TimeSpan GetEntryExpire(string key)
		{
			if(string.IsNullOrWhiteSpace(key))
				throw new ArgumentNullException("key");

			//获取或创建Redis客户端代理对象
			var redis = this.Proxy;

			try
			{
				return redis.GetTimeToLive(key);
			}
			finally
			{
				_redisPool.Release(redis);
			}
		}

		public bool SetEntryExpire(string key, DateTime expires)
		{
			if(string.IsNullOrWhiteSpace(key))
				throw new ArgumentNullException("key");

			//获取或创建Redis客户端代理对象
			var redis = this.Proxy;

			try
			{
				return redis.ExpireEntryAt(key, expires);
			}
			finally
			{
				_redisPool.Release(redis);
			}
		}

		public bool SetEntryExpire(string key, TimeSpan duration)
		{
			if(string.IsNullOrWhiteSpace(key))
				throw new ArgumentNullException("key");

			//获取或创建Redis客户端代理对象
			var redis = this.Proxy;

			try
			{
				return redis.ExpireEntryIn(key, duration);
			}
			finally
			{
				_redisPool.Release(redis);
			}
		}

		public void Rename(string oldKey, string newKey)
		{
			if(string.IsNullOrWhiteSpace(oldKey))
				throw new ArgumentNullException("oldKey");

			if(string.IsNullOrWhiteSpace(newKey))
				throw new ArgumentNullException("newKey");

			//获取或创建Redis客户端代理对象
			var redis = this.Proxy;

			try
			{
				redis.RenameKey(oldKey, newKey);
			}
			finally
			{
				_redisPool.Release(redis);
			}
		}

		public void Clear()
		{
			//获取或创建Redis客户端代理对象
			var redis = this.Proxy;

			try
			{
				redis.DeleteAll<object>();
			}
			finally
			{
				_redisPool.Release(redis);
			}
		}

		public bool Remove(string key)
		{
			if(string.IsNullOrWhiteSpace(key))
				throw new ArgumentNullException("key");

			//获取或创建Redis客户端代理对象
			var redis = this.Proxy;

			try
			{
				return redis.Remove(key);
			}
			finally
			{
				_redisPool.Release(redis);
			}
		}

		public void RemoveRange(params string[] keys)
		{
			if(keys == null || keys.Length == 0)
				throw new ArgumentNullException("keys");

			//获取或创建Redis客户端代理对象
			var redis = this.Proxy;

			try
			{
				redis.RemoveAll(keys);
			}
			finally
			{
				_redisPool.Release(redis);
			}
		}

		public bool Contains(string key)
		{
			if(string.IsNullOrWhiteSpace(key))
				throw new ArgumentNullException("key");

			//获取或创建Redis客户端代理对象
			var redis = this.Proxy;

			try
			{
				return redis.ContainsKey(key);
			}
			finally
			{
				_redisPool.Release(redis);
			}
		}

		public bool Exists(string key)
		{
			if(string.IsNullOrWhiteSpace(key))
				throw new ArgumentNullException("key");

			//获取或创建Redis客户端代理对象
			var redis = this.Proxy;

			try
			{
				return redis.ContainsKey(key);
			}
			finally
			{
				_redisPool.Release(redis);
			}
		}

		public long Increment(string key, int interval = 1)
		{
			if(string.IsNullOrWhiteSpace(key))
				throw new ArgumentNullException("key");

			//获取或创建Redis客户端代理对象
			var redis = this.Proxy;

			try
			{
				if(interval == 1)
					return redis.IncrementValue(key);
				else
					return redis.IncrementValueBy(key, interval);
			}
			finally
			{
				_redisPool.Release(redis);
			}
		}

		public long Decrement(string key, int interval = 1)
		{
			if(string.IsNullOrWhiteSpace(key))
				throw new ArgumentNullException("key");

			//获取或创建Redis客户端代理对象
			var redis = this.Proxy;

			try
			{
				if(interval == 1)
					return redis.DecrementValue(key);
				else
					return redis.DecrementValueBy(key, interval);
			}
			finally
			{
				_redisPool.Release(redis);
			}
		}

		public HashSet<string> GetIntersect(params string[] sets)
		{
			if(sets == null)
				throw new ArgumentNullException("sets");

			//获取或创建Redis客户端代理对象
			var redis = this.Proxy;

			try
			{
				return redis.GetIntersectFromSets(sets);
			}
			finally
			{
				_redisPool.Release(redis);
			}
		}

		public void SetIntersect(string destination, params string[] sets)
		{
			if(sets == null)
				throw new ArgumentNullException("sets");

			//获取或创建Redis客户端代理对象
			var redis = this.Proxy;

			try
			{
				redis.StoreIntersectFromSets(destination, sets);
			}
			finally
			{
				_redisPool.Release(redis);
			}
		}

		public HashSet<string> GetUnion(params string[] sets)
		{
			if(sets == null)
				throw new ArgumentNullException("sets");

			//获取或创建Redis客户端代理对象
			var redis = this.Proxy;

			try
			{
				return redis.GetUnionFromSets(sets);
			}
			finally
			{
				_redisPool.Release(redis);
			}
		}

		public void SetUnion(string destination, params string[] sets)
		{
			if(sets == null)
				throw new ArgumentNullException("sets");

			//获取或创建Redis客户端代理对象
			var redis = this.Proxy;

			try
			{
				redis.StoreUnionFromSets(destination, sets);
			}
			finally
			{
				_redisPool.Release(redis);
			}
		}

		public int Notifiy(string channel, string message)
		{
			if(string.IsNullOrWhiteSpace(channel))
				throw new ArgumentNullException("channel");

			if(string.IsNullOrEmpty(message))
				return 0;

			//获取或创建Redis客户端代理对象
			var redis = this.Proxy;

			try
			{
				return (int)redis.PublishMessage(channel, message);
			}
			finally
			{
				_redisPool.Release(redis);
			}
		}
		#endregion

		#region 虚拟方法
		protected virtual ServiceStack.Redis.RedisClient CreateProxy()
		{
			var redis = new ServiceStack.Redis.RedisClient(_settings.Address.Address.ToString(), _settings.Address.Port, _settings.Password, _settings.DatabaseId);

			if(_settings.Timeout > TimeSpan.Zero)
			{
				redis.ConnectTimeout = (int)_settings.Timeout.TotalMilliseconds;
				redis.RetryTimeout = (int)_settings.Timeout.TotalMilliseconds;
				redis.SendTimeout = (int)_settings.Timeout.TotalMilliseconds;
			}

			return redis;
		}
		#endregion

		#region 私有方法
		private T GetCacheEntry<T>(string name, RedisEntryType entryType, Func<string, T> createThunk) where T : RedisObjectBase
		{
			if(string.IsNullOrWhiteSpace(name))
				throw new ArgumentNullException("name");

			//获取指定名称的条目类型
			var storedEntryType = this.GetEntryType(name);

			if(storedEntryType != RedisEntryType.None && storedEntryType != entryType)
				throw new RedisException("The specified name entry is invalid entry.");

			//确保缓存容器创建完成
			if(_caches == null)
				System.Threading.Interlocked.CompareExchange(ref _caches, new ConcurrentDictionary<int, Zongsoft.Collections.ObjectCache<RedisObjectBase>>(), null);

			//获取当前数据库的缓存器
			var cache = _caches.GetOrAdd(_settings.DatabaseId, new Collections.ObjectCache<RedisObjectBase>());

			return (T)cache.Get(name, key =>
			{
				var redisObject = createThunk(key);

				redisObject.Disposed += (_, __) =>
				{
					cache.Remove(key);
				};

				return redisObject;
			});
		}
		#endregion

		#region 缓存接口
		long Zongsoft.Runtime.Caching.ICache.Count
		{
			get
			{
				throw new NotSupportedException("The cache container isn't supports the feature.");
			}
		}

		Zongsoft.Runtime.Caching.ICacheCreator Zongsoft.Runtime.Caching.ICache.Creator
		{
			get;
			set;
		}

		TimeSpan? Zongsoft.Runtime.Caching.ICache.GetDuration(string key)
		{
			var duration = this.GetEntryExpire(key);

			if(duration == TimeSpan.Zero)
				return null;

			return duration;
		}

		void Zongsoft.Runtime.Caching.ICache.SetDuration(string key, TimeSpan duration)
		{
			this.SetEntryExpire(key, duration);
		}

		object Zongsoft.Runtime.Caching.ICache.GetValue(string key)
		{
			var creator = ((Zongsoft.Runtime.Caching.ICache)this).Creator;

			if(creator == null)
				return this.GetEntry(key);

			return ((Zongsoft.Runtime.Caching.ICache)this).GetValue(key, _ =>
			{
				TimeSpan duration;
				return new Tuple<object, TimeSpan>(creator.Create(null, _, out duration), duration);
			});
		}

		object Zongsoft.Runtime.Caching.ICache.GetValue(string key, Func<string, Tuple<object, TimeSpan>> valueCreator)
		{
			if(string.IsNullOrWhiteSpace(key))
				throw new ArgumentNullException("key");

			if(valueCreator == null)
				return this.GetEntry(key);

			var redis = this.Proxy;

			try
			{
				var result = valueCreator(key);

				if(result == null && result.Item1 == null)
				{
					redis.Remove(key);
					return null;
				}

				if(redis.SetEntryIfNotExists(key, result.Item1.ToString()))
				{
					if(result.Item2 > TimeSpan.Zero)
						redis.ExpireEntryIn(key, result.Item2);

					return result.Item1;
				}
			}
			finally
			{
				_redisPool.Release(redis);
			}

			return this.GetEntry(key);
		}

		bool Zongsoft.Runtime.Caching.ICache.SetValue(string key, object value)
		{
			if(value == null)
				return this.Remove(key);
			else
				return this.SetValue(key, value.ToString());
		}

		bool Zongsoft.Runtime.Caching.ICache.SetValue(string key, object value, bool requiredNotExists, TimeSpan? duration = null)
		{
			if(string.IsNullOrWhiteSpace(key))
				throw new ArgumentNullException("key");

			if(value == null)
				return this.Remove(key);

			//获取或创建Redis客户端代理对象
			var redis = this.Proxy;

			try
			{
				if(requiredNotExists)
				{
					bool result = false;

					using(var transaction = redis.CreateTransaction())
					{
						transaction.QueueCommand(proxy => proxy.SetEntryIfNotExists(key, value.ToString()), _ => result = _);

						if(duration.HasValue && duration.Value > TimeSpan.Zero)
							transaction.QueueCommand(proxy => redis.ExpireEntryIn(key, duration.Value));

						transaction.Commit();
					}

					return result;
				}

				if(duration.HasValue && duration.Value > TimeSpan.Zero)
					return redis.Set(key, value, duration.Value);
				else
					return redis.Set(key, value);
			}
			finally
			{
				_redisPool.Release(redis);
			}
		}

		bool Zongsoft.Runtime.Caching.ICache.SetValue(string key, object value, TimeSpan duration, bool requiredNotExists = false)
		{
			return ((Zongsoft.Runtime.Caching.ICache)this).SetValue(key, value, requiredNotExists, duration);
		}
		#endregion

		#region 获取缓存
		Zongsoft.Runtime.Caching.ICache Zongsoft.Runtime.Caching.ICacheProvider.GetCache(string name)
		{
			if(string.IsNullOrWhiteSpace(name))
				return this;

			return this.GetDictionary(name.Trim()) as Zongsoft.Runtime.Caching.ICache;
		}
		#endregion

		#region 获取队列
		Zongsoft.Collections.IQueue Zongsoft.Collections.IQueueProvider.GetQueue(string name)
		{
			return this.GetQueue(name);
		}
		#endregion

		#region 处置方法
		public void Dispose()
		{
			this.Dispose(true);
			GC.SuppressFinalize(this);
		}

		protected virtual void Dispose(bool disposing)
		{
			var pool = Interlocked.Exchange(ref _redisPool, null);

			if(pool != null)
				pool.Dispose();

			var caches = _caches;

			if(caches != null)
				caches.Clear();
		}
		#endregion
	}
}
