/*
 * Authors:
 *   钟峰(Popeye Zhong) <zongsoft@gmail.com>
 *
 * Copyright (C) 2014 Zongsoft Corporation <http://www.zongsoft.com>
 *
 * This file is part of Zongsoft.CoreLibrary.
 *
 * Zongsoft.CoreLibrary is free software; you can redistribute it and/or
 * modify it under the terms of the GNU Lesser General Public
 * License as published by the Free Software Foundation; either
 * version 2.1 of the License, or (at your option) any later version.
 *
 * Zongsoft.CoreLibrary is distributed in the hope that it will be useful,
 * but WITHOUT ANY WARRANTY; without even the implied warranty of
 * MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the GNU
 * Lesser General Public License for more details.
 *
 * The above copyright notice and this permission notice shall be
 * included in all copies or substantial portions of the Software.
 *
 * You should have received a copy of the GNU Lesser General Public
 * License along with Zongsoft.CoreLibrary; if not, write to the Free Software
 * Foundation, Inc., 51 Franklin Street, Fifth Floor, Boston, MA 02110-1301 USA
 */

using System;
using System.Net;
using System.Collections.Generic;
using System.Collections.Concurrent;

using ServiceStack.Redis;

namespace Zongsoft.Externals.Redis
{
	public class RedisService : IRedisService, IDisposable, Zongsoft.Collections.IQueueProvider
	{
		#region 成员字段
		private ServiceStack.Redis.RedisClient _redis;
		private IPEndPoint _address;
		private string _password;
		private int _databaseId;
		private int _isDisposed;
		private TimeSpan _timeout;
		private ConcurrentDictionary<int, Zongsoft.Collections.ObjectCache<object>> _caches;
		#endregion

		#region 构造函数
		public RedisService()
		{
			_address = new IPEndPoint(IPAddress.Loopback, 6379);
		}

		public RedisService(IPEndPoint address, string password = null, int databaseId = 0)
		{
			_address = address;
			_password = password;
			_databaseId = databaseId;
		}

		public RedisService(Configuration.RedisConfigurationElement config)
		{
			if(config == null)
			{
				_address = new IPEndPoint(IPAddress.Loopback, 6379);
				return;
			}

			_address = Zongsoft.Communication.IPEndPointConverter.Parse(config.Address);
			_password = config.Password;
			_databaseId = config.DbIndex;
			_timeout = config.Timeout;
		}
		#endregion

		#region 公共属性
		public IPEndPoint Address
		{
			get
			{
				return _address;
			}
			set
			{
				if(_address == value)
					return;

				if(_redis != null)
					throw new InvalidOperationException();

				_address = value;
			}
		}

		public string Password
		{
			set
			{
				if(_redis != null)
					throw new InvalidOperationException();

				_password = value;
			}
		}

		public int DatabaseId
		{
			get
			{
				return _databaseId;
			}
			set
			{
				if(_databaseId != value)
					_databaseId = Math.Abs(value);
			}
		}

		public TimeSpan Timeout
		{
			get
			{
				return _timeout;
			}
			set
			{
				_timeout = value;
			}
		}

		public bool IsDisposed
		{
			get
			{
				return _isDisposed != 0;
			}
		}
		#endregion

		#region 保护属性
		protected ServiceStack.Redis.IRedisClient RedisClient
		{
			get
			{
				if(_isDisposed != 0)
					throw new ObjectDisposedException("Redis");

				if(_redis == null)
				{
					if(_isDisposed != 0)
						throw new ObjectDisposedException("Redis");

					System.Threading.Interlocked.CompareExchange(ref _redis, new ServiceStack.Redis.RedisClient(_address.Address.ToString(), _address.Port, _password, _databaseId), null);

					if(_timeout > TimeSpan.Zero)
					{
						_redis.ConnectTimeout = (int)_timeout.TotalMilliseconds;
						_redis.RetryTimeout = (int)_timeout.TotalMilliseconds;
						_redis.SendTimeout = (int)_timeout.TotalMilliseconds;
					}
				}

				return _redis;
			}
		}
		#endregion

		#region 获取集合
		public IRedisHashset GetHashset(string name)
		{
			return this.GetCacheEntry(name, RedisEntryType.Set, (key, redis) => new RedisSet(key, redis));
		}

		public IRedisDictionary GetDictionary(string name)
		{
			return this.GetCacheEntry(name, RedisEntryType.Dictionary, (key, redis) => new RedisDictionary(key, redis));
		}

		public IRedisQueue GetQueue(string name)
		{
			return this.GetCacheEntry(name, RedisEntryType.List, (key, redis) => new RedisQueue(key, redis));
		}
		#endregion

		public string GetValue(string key)
		{
			if(string.IsNullOrWhiteSpace(key))
				throw new ArgumentNullException("key");

			//获取或创建Redis客户端代理对象
			var redis = this.RedisClient;

			return redis.GetEntry(key);
		}

		public IEnumerable<string> GetValues(params string[] keys)
		{
			if(keys == null || keys.Length == 0)
				throw new ArgumentNullException("keys");

			//获取或创建Redis客户端代理对象
			var redis = this.RedisClient;

			return redis.GetValues(System.Linq.Enumerable.ToList(keys));
		}

		public string ExchangeValue(string key, string value)
		{
			if(string.IsNullOrWhiteSpace(key))
				throw new ArgumentNullException("key");

			//获取或创建Redis客户端代理对象
			var redis = this.RedisClient;

			return redis.GetAndSetEntry(key, value);
		}

		public string ExchangeValue(string key, string value, DateTime expires)
		{
			if(string.IsNullOrWhiteSpace(key))
				throw new ArgumentNullException("key");

			//获取或创建Redis客户端代理对象
			var redis = this.RedisClient;

			using(var transaction = redis.CreateTransaction())
			{
				string result = null;

				transaction.QueueCommand(proxy => proxy.GetAndSetEntry(key, value), s => result = s);
				transaction.QueueCommand(proxy => proxy.ExpireEntryAt(key, expires));

				transaction.Commit();

				return result;
			}
		}

		public string ExchangeValue(string key, string value, TimeSpan duration)
		{
			if(string.IsNullOrWhiteSpace(key))
				throw new ArgumentNullException("key");

			//获取或创建Redis客户端代理对象
			var redis = this.RedisClient;

			using(var transaction = redis.CreateTransaction())
			{
				string result = null;

				transaction.QueueCommand(proxy => proxy.GetAndSetEntry(key, value), s => result = s);
				transaction.QueueCommand(proxy => proxy.ExpireEntryIn(key, duration));

				transaction.Commit();

				return result;
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
			var redis = this.RedisClient;

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

		public bool SetValue(string key, string value, TimeSpan duration, bool requiredNotExists = false)
		{
			if(string.IsNullOrWhiteSpace(key))
				throw new ArgumentNullException("key");

			//获取或创建Redis客户端代理对象
			var redis = this.RedisClient;

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

		public RedisEntryType GetEntryType(string key)
		{
			if(string.IsNullOrWhiteSpace(key))
				throw new ArgumentNullException("key");

			//获取或创建Redis客户端代理对象
			var redis = this.RedisClient;

			//获取Redis条目类型
			var entryType = redis.GetEntryType(key);

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
			var redis = this.RedisClient;

			return redis.GetTimeToLive(key);
		}

		public bool SetEntryExpire(string key, DateTime expires)
		{
			if(string.IsNullOrWhiteSpace(key))
				throw new ArgumentNullException("key");

			//获取或创建Redis客户端代理对象
			var redis = this.RedisClient;

			return redis.ExpireEntryAt(key, expires);
		}

		public bool SetEntryExpire(string key, TimeSpan duration)
		{
			if(string.IsNullOrWhiteSpace(key))
				throw new ArgumentNullException("key");

			//获取或创建Redis客户端代理对象
			var redis = this.RedisClient;

			return redis.ExpireEntryIn(key, duration);
		}

		public void Rename(string oldKey, string newKey)
		{
			if(string.IsNullOrWhiteSpace(oldKey))
				throw new ArgumentNullException("oldKey");

			if(string.IsNullOrWhiteSpace(newKey))
				throw new ArgumentNullException("newKey");

			//获取或创建Redis客户端代理对象
			var redis = this.RedisClient;

			redis.RenameKey(oldKey, newKey);
		}

		public void Clear()
		{
			//获取或创建Redis客户端代理对象
			var redis = this.RedisClient;

			redis.DeleteAll<object>();
		}

		public bool Remove(string key)
		{
			if(string.IsNullOrWhiteSpace(key))
				throw new ArgumentNullException("key");

			//获取或创建Redis客户端代理对象
			var redis = this.RedisClient;

			return redis.Remove(key);
		}

		public void RemoveRange(params string[] keys)
		{
			if(keys == null || keys.Length == 0)
				throw new ArgumentNullException("keys");

			//获取或创建Redis客户端代理对象
			var redis = this.RedisClient;

			redis.RemoveAll(keys);
		}

		public bool Contains(string key)
		{
			if(string.IsNullOrWhiteSpace(key))
				throw new ArgumentNullException("key");

			//获取或创建Redis客户端代理对象
			var redis = this.RedisClient;

			return redis.ContainsKey(key);
		}

		public long Increment(string key, int interval = 1)
		{
			if(string.IsNullOrWhiteSpace(key))
				throw new ArgumentNullException("key");

			//获取或创建Redis客户端代理对象
			var redis = this.RedisClient;

			if(interval < 1)
			{
				var text = redis.GetEntry(key);
				long result;

				if(long.TryParse(text, out result))
					return result;
				else
					return -1;
			}

			return redis.IncrementValueBy(key, interval);
		}

		public long Decrement(string key, int interval = 1)
		{
			if(string.IsNullOrWhiteSpace(key))
				throw new ArgumentNullException("key");

			//获取或创建Redis客户端代理对象
			var redis = this.RedisClient;

			if(interval < 1)
			{
				var text = redis.GetEntry(key);
				long result;

				if(long.TryParse(text, out result))
					return result;
				else
					return -1;
			}

			return redis.DecrementValueBy(key, interval);
		}

		public HashSet<string> GetIntersect(params string[] sets)
		{
			if(sets == null)
				throw new ArgumentNullException("sets");

			//获取或创建Redis客户端代理对象
			var redis = this.RedisClient;

			return redis.GetIntersectFromSets(sets);
		}

		public void SetIntersect(string destination, params string[] sets)
		{
			if(sets == null)
				throw new ArgumentNullException("sets");

			//获取或创建Redis客户端代理对象
			var redis = this.RedisClient;

			redis.StoreIntersectFromSets(destination, sets);
		}

		public HashSet<string> GetUnion(params string[] sets)
		{
			if(sets == null)
				throw new ArgumentNullException("sets");

			//获取或创建Redis客户端代理对象
			var redis = this.RedisClient;

			return redis.GetUnionFromSets(sets);
		}

		public void SetUnion(string destination, params string[] sets)
		{
			if(sets == null)
				throw new ArgumentNullException("sets");

			//获取或创建Redis客户端代理对象
			var redis = this.RedisClient;

			redis.StoreUnionFromSets(destination, sets);
		}

		#region 私有方法
		private T GetCacheEntry<T>(string name, RedisEntryType entryType, Func<string, ServiceStack.Redis.IRedisClient, T> createThunk)
		{
			if(string.IsNullOrWhiteSpace(name))
				throw new ArgumentNullException("name");

			//获取或创建Redis客户端代理对象
			var redis = this.RedisClient;

			//获取指定名称的条目类型
			var storedEntryType = this.GetEntryType(name);

			if(storedEntryType != RedisEntryType.None || storedEntryType != entryType)
				throw new RedisException("The specified name entry is invalid entry.");

			//获取当前数据库的缓存器
			var cache = this.GetCache();

			return (T)cache.Get(name, key => createThunk(key, redis));
		}

		private Zongsoft.Collections.ObjectCache<object> GetCache()
		{
			if(_caches == null)
				System.Threading.Interlocked.CompareExchange(ref _caches, new ConcurrentDictionary<int, Zongsoft.Collections.ObjectCache<object>>(), null);

			return _caches.GetOrAdd(this.DatabaseId, new Collections.ObjectCache<object>());
		}
		#endregion

		#region 处置方法
		public void Dispose()
		{
			var isDisposed = System.Threading.Interlocked.Exchange(ref _isDisposed, 1);

			if(isDisposed != 0)
				return;

			var redis = System.Threading.Interlocked.Exchange(ref _redis, null);

			if(redis != null)
				redis.Dispose();
		}
		#endregion

		#region 获取队列
		Collections.IQueue Collections.IQueueProvider.GetQueue(string name)
		{
			return this.GetQueue(name);
		}
		#endregion
	}
}
