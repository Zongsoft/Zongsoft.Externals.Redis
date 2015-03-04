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
	public class RedisService : MarshalByRefObject, IRedisService, IDisposable, Zongsoft.Collections.IQueueProvider
	{
		#region 成员字段
		private Zongsoft.Common.IObjectReference<ServiceStack.Redis.IRedisClient> _redisReference;
		private IPEndPoint _address;
		private string _password;
		private int _databaseId;
		private TimeSpan _timeout;
		private ConcurrentDictionary<int, Zongsoft.Collections.ObjectCache<object>> _caches;
		#endregion

		#region 构造函数
		public RedisService()
		{
			_address = new IPEndPoint(IPAddress.Loopback, 6379);
			_redisReference = new Zongsoft.Common.ObjectReference<ServiceStack.Redis.IRedisClient>(this.CreateProxy);
		}

		public RedisService(IPEndPoint address, string password = null, int databaseId = 0)
		{
			_address = address;
			_password = password;
			_databaseId = databaseId;
			_redisReference = new Zongsoft.Common.ObjectReference<ServiceStack.Redis.IRedisClient>(this.CreateProxy);
		}

		public RedisService(Configuration.RedisConfigurationElement config)
		{
			if(config == null)
			{
				_address = new IPEndPoint(IPAddress.Loopback, 6379);
				return;
			}

			_address = Zongsoft.Communication.IPEndPointConverter.Parse(config.Address);

			if(_address.Port == 0)
				_address.Port = 6379;

			_password = config.Password;
			_databaseId = config.DatabaseId;
			_timeout = config.Timeout;
			_redisReference = new Zongsoft.Common.ObjectReference<ServiceStack.Redis.IRedisClient>(this.CreateProxy);
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

				_address = value;
			}
		}

		public string Password
		{
			set
			{
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
				return _redisReference == null;
			}
		}

		public ServiceStack.Redis.IRedisClient Proxy
		{
			get
			{
				return _redisReference.Target;
			}
		}
		#endregion

		#region 获取集合
		public IRedisHashset GetHashset(string name)
		{
			return this.GetCacheEntry(name, RedisEntryType.Set, (key, redis) => new RedisHashset(key, redis));
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
			var redis = this.Proxy;

			return redis.GetValue(key);
		}

		public IEnumerable<string> GetValues(params string[] keys)
		{
			if(keys == null || keys.Length == 0)
				throw new ArgumentNullException("keys");

			//获取或创建Redis客户端代理对象
			var redis = this.Proxy;

			return redis.GetValues(System.Linq.Enumerable.ToList(keys));
		}

		public string ExchangeValue(string key, string value)
		{
			if(string.IsNullOrWhiteSpace(key))
				throw new ArgumentNullException("key");

			//获取或创建Redis客户端代理对象
			var redis = this.Proxy;

			return redis.GetAndSetEntry(key, value);
		}

		public string ExchangeValue(string key, string value, DateTime expires)
		{
			if(string.IsNullOrWhiteSpace(key))
				throw new ArgumentNullException("key");

			//获取或创建Redis客户端代理对象
			var redis = this.Proxy;

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
			var redis = this.Proxy;

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
			var redis = this.Proxy;

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
			var redis = this.Proxy;

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
			var redis = this.Proxy;

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
			var redis = this.Proxy;

			return redis.GetTimeToLive(key);
		}

		public bool SetEntryExpire(string key, DateTime expires)
		{
			if(string.IsNullOrWhiteSpace(key))
				throw new ArgumentNullException("key");

			//获取或创建Redis客户端代理对象
			var redis = this.Proxy;

			return redis.ExpireEntryAt(key, expires);
		}

		public bool SetEntryExpire(string key, TimeSpan duration)
		{
			if(string.IsNullOrWhiteSpace(key))
				throw new ArgumentNullException("key");

			//获取或创建Redis客户端代理对象
			var redis = this.Proxy;

			return redis.ExpireEntryIn(key, duration);
		}

		public void Rename(string oldKey, string newKey)
		{
			if(string.IsNullOrWhiteSpace(oldKey))
				throw new ArgumentNullException("oldKey");

			if(string.IsNullOrWhiteSpace(newKey))
				throw new ArgumentNullException("newKey");

			//获取或创建Redis客户端代理对象
			var redis = this.Proxy;

			redis.RenameKey(oldKey, newKey);
		}

		public void Clear()
		{
			//获取或创建Redis客户端代理对象
			var redis = this.Proxy;

			redis.DeleteAll<object>();
		}

		public bool Remove(string key)
		{
			if(string.IsNullOrWhiteSpace(key))
				throw new ArgumentNullException("key");

			//获取或创建Redis客户端代理对象
			var redis = this.Proxy;

			return redis.Remove(key);
		}

		public void RemoveRange(params string[] keys)
		{
			if(keys == null || keys.Length == 0)
				throw new ArgumentNullException("keys");

			//获取或创建Redis客户端代理对象
			var redis = this.Proxy;

			redis.RemoveAll(keys);
		}

		public bool Contains(string key)
		{
			if(string.IsNullOrWhiteSpace(key))
				throw new ArgumentNullException("key");

			//获取或创建Redis客户端代理对象
			var redis = this.Proxy;

			return redis.ContainsKey(key);
		}

		public long Increment(string key, int interval = 1)
		{
			if(string.IsNullOrWhiteSpace(key))
				throw new ArgumentNullException("key");

			//获取或创建Redis客户端代理对象
			var redis = this.Proxy;

			if(interval < 1)
			{
				var text = redis.GetValue(key);
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
			var redis = this.Proxy;

			if(interval < 1)
			{
				var text = redis.GetValue(key);
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
			var redis = this.Proxy;

			return redis.GetIntersectFromSets(sets);
		}

		public void SetIntersect(string destination, params string[] sets)
		{
			if(sets == null)
				throw new ArgumentNullException("sets");

			//获取或创建Redis客户端代理对象
			var redis = this.Proxy;

			redis.StoreIntersectFromSets(destination, sets);
		}

		public HashSet<string> GetUnion(params string[] sets)
		{
			if(sets == null)
				throw new ArgumentNullException("sets");

			//获取或创建Redis客户端代理对象
			var redis = this.Proxy;

			return redis.GetUnionFromSets(sets);
		}

		public void SetUnion(string destination, params string[] sets)
		{
			if(sets == null)
				throw new ArgumentNullException("sets");

			//获取或创建Redis客户端代理对象
			var redis = this.Proxy;

			redis.StoreUnionFromSets(destination, sets);
		}

		/// <summary>
		/// 刷新当前Redis服务，当与远程Redis服务器连接中断后可使用该方法手动连接。
		/// </summary>
		/// <param name="timeout">刷新的超时，如果为零则表示无超时限制。</param>
		public virtual void Refresh(TimeSpan timeout)
		{
			var reference = _redisReference;

			if(reference == null)
				throw new ObjectDisposedException("Redis");

			reference.Invalidate();
		}

		#region 虚拟方法
		protected virtual ServiceStack.Redis.IRedisClient CreateProxy()
		{
			if(_redisReference == null)
				throw new ObjectDisposedException("Redis");

			var redis = new ServiceStack.Redis.RedisClient(_address.Address.ToString(), _address.Port, _password, _databaseId);

			if(_timeout > TimeSpan.Zero)
			{
				redis.ConnectTimeout = (int)_timeout.TotalMilliseconds;
				redis.RetryTimeout = (int)_timeout.TotalMilliseconds;
				redis.SendTimeout = (int)_timeout.TotalMilliseconds;
			}

			return redis;
		}
		#endregion

		#region 私有方法
		private T GetCacheEntry<T>(string name, RedisEntryType entryType, Func<string, Zongsoft.Common.IObjectReference<ServiceStack.Redis.IRedisClient>, T> createThunk)
		{
			if(string.IsNullOrWhiteSpace(name))
				throw new ArgumentNullException("name");

			//获取或创建Redis客户端代理对象
			var redis = this.Proxy;

			//获取指定名称的条目类型
			var storedEntryType = this.GetEntryType(name);

			if(storedEntryType != RedisEntryType.None && storedEntryType != entryType)
				throw new RedisException("The specified name entry is invalid entry.");

			//获取当前数据库的缓存器
			var cache = this.GetCache();

			return (T)cache.Get(name, key => createThunk(key, _redisReference));
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
			var caches = _caches;

			if(caches != null)
				caches.Clear();

			var redisReference = _redisReference as IDisposable;

			if(redisReference != null)
				redisReference.Dispose();

			_redisReference = null;
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
