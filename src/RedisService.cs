/*
 * Authors:
 *   钟峰(Popeye Zhong) <9555843@qq.com>
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
using System.Linq;
using System.Collections;
using System.Collections.Generic;
using System.Collections.Concurrent;
using System.Threading;

using StackExchange.Redis;

namespace Zongsoft.Externals.Redis
{
	public class RedisService : MarshalByRefObject, IRedisService, IDisposable,
	                            Zongsoft.Common.IAccumulator,
	                            Zongsoft.Collections.IQueueProvider,
	                            Zongsoft.Runtime.Caching.ICache,
	                            Zongsoft.Runtime.Caching.ICacheProvider
	{
		#region 成员字段
		private readonly object _syncRoot;
		private readonly ConfigurationOptions _setting;
		private ConcurrentDictionary<int, Zongsoft.Collections.ObjectCache<RedisObjectBase>> _caches;
		private IConnectionMultiplexer _redis;
		private IDatabase _database;
		private RedisSubscriber _subscriber;
		private bool _isDisposed;
		#endregion

		#region 构造函数
		public RedisService()
		{
			_setting = ConfigurationOptions.Parse("127.0.0.1");
			_syncRoot = new object();
		}

		public RedisService(string connectionString)
		{
			if(string.IsNullOrWhiteSpace(connectionString))
				connectionString = "127.0.0.1";

			_setting = ConfigurationOptions.Parse(connectionString);
			_syncRoot = new object();
		}
		#endregion

		#region 公共属性
		public long Count
		{
			get
			{
				return -1;
			}
		}

		public string Name
		{
			get
			{
				string addresses = string.Empty;

				if(_setting.EndPoints.Count > 1)
				{
					foreach(var endPoint in _setting.EndPoints)
					{
						if(string.IsNullOrEmpty(addresses))
							addresses += ", ";

						addresses += endPoint.ToString();
					}

					addresses = "[" + addresses + "]";
				}
				else
				{
					addresses = _setting.EndPoints[0].ToString();
				}

				var databaseId = _setting.DefaultDatabase ?? 0;

				if(_database != null)
					databaseId = _database.Database;

				if(_setting.Proxy == Proxy.None)
					return addresses + "#" + databaseId;
				else
					return $"({_setting.Proxy}){addresses}#{databaseId}";
			}
		}

		public bool IsDisposed
		{
			get
			{
				return _isDisposed;
			}
		}

		public StackExchange.Redis.ConfigurationOptions Settings
		{
			get
			{
				return _setting;
			}
		}

		protected RedisSubscriber Subscriber
		{
			get
			{
				if(_subscriber == null)
					_subscriber = new RedisSubscriber(this.Redis.GetSubscriber());

				return _subscriber;
			}
		}

		protected StackExchange.Redis.IDatabase Database
		{
			get
			{
				if(_database == null)
					_database = this.Redis.GetDatabase();

				return _database;
			}
		}

		protected StackExchange.Redis.IConnectionMultiplexer Redis
		{
			get
			{
				if(_isDisposed)
					throw new ObjectDisposedException(nameof(RedisService));

				if(_redis == null)
				{
					lock(_syncRoot)
					{
						if(_isDisposed)
							throw new ObjectDisposedException(nameof(RedisService));

						if(_redis == null)
							_redis = ConnectionMultiplexer.Connect(_setting);
					}
				}

				return _redis;
			}
		}
		#endregion

		#region 获取集合
		public IRedisDictionary GetDictionary(string name)
		{
			return this.GetCacheEntry(name, RedisEntryType.Dictionary, (key, first) => new RedisDictionary(key, _redisPool));
		}

		public IRedisDictionary GetDictionary(string name, IDictionary<string, string> items)
		{
			return this.GetCacheEntry(name, RedisEntryType.Dictionary, (key, first) =>
			{
				var result = new RedisDictionary(key, _redisPool);

				if(first && items != null)
				{
					foreach(var item in items)
						result.TryAdd(item.Key, item.Value);
				}

				return result;
			});
		}

		public IRedisHashset GetHashset(string name)
		{
			return this.GetCacheEntry(name, RedisEntryType.Set, (key, first) => new RedisHashset(key, _redisPool));
		}

		public IRedisQueue GetQueue(string name)
		{
			return this.GetCacheEntry(name, RedisEntryType.List, (key, first) => new RedisQueue(key, _redisPool));
		}
		#endregion

		#region 公共方法
		public bool Use(int databaseId)
		{
			if(databaseId < 0)
				return false;

			var database = this.Redis.GetDatabase(databaseId);

			if(database != null)
				_database = database;

			return database != null;
		}

		public RedisSubscriber CreateSubscriber()
		{
			this.Redis.GetSubscriber();
			return new RedisSubscriber(this.CreateProxy());
		}

		public object GetEntry(string key)
		{
			if(string.IsNullOrWhiteSpace(key))
				throw new ArgumentNullException(nameof(key));

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
				throw new ArgumentNullException(nameof(key));

			return this.Database.StringGet(key);
		}

		public IEnumerable<string> GetValues(params string[] keys)
		{
			if(keys == null || keys.Length == 0)
				throw new ArgumentNullException(nameof(keys));

			return this.Database.StringGet(keys.Cast<RedisKey>().ToArray()).Cast<string>();
		}

		public string ExchangeValue(string key, string value)
		{
			if(string.IsNullOrWhiteSpace(key))
				throw new ArgumentNullException(nameof(key));

			return this.Database.StringGetSet(key, value);
		}

		public string ExchangeValue(string key, string value, TimeSpan duration)
		{
			if(string.IsNullOrWhiteSpace(key))
				throw new ArgumentNullException(nameof(key));

			//尝试获取指定键的条目值
			string result = this.Database.StringGet(key);

			//如果指定的键存在则返回它
			if(result != null)
				return result;

			//设置指定的键值对并附加指定的有效期
			return this.Database.StringSet(key, value, duration, When.NotExists) ? value : null;
		}

		public bool SetValue(string key, string value)
		{
			if(string.IsNullOrWhiteSpace(key))
				throw new ArgumentNullException(nameof(key));

			return this.Database.StringSet(key, value);
		}

		public bool SetValue(string key, string value, TimeSpan duration, bool requiredNotExists = false)
		{
			if(string.IsNullOrWhiteSpace(key))
				throw new ArgumentNullException(nameof(key));

			return this.Database.StringSet(key, value, duration, requiredNotExists ? When.NotExists : When.Always);
		}

		public RedisEntryType GetEntryType(string key)
		{
			if(string.IsNullOrWhiteSpace(key))
				throw new ArgumentNullException(nameof(key));

			//获取Redis条目类型
			var entryType = this.Database.KeyType(key);

			switch(entryType)
			{
				case RedisType.Hash:
					return RedisEntryType.Dictionary;
				case RedisType.List:
					return RedisEntryType.List;
				case RedisType.Set:
					return RedisEntryType.Set;
				case RedisType.SortedSet:
					return RedisEntryType.SortedSet;
				case RedisType.String:
					return RedisEntryType.String;
			}

			return RedisEntryType.None;
		}

		public TimeSpan? GetEntryExpire(string key)
		{
			if(string.IsNullOrWhiteSpace(key))
				throw new ArgumentNullException(nameof(key));

			return this.Database.StringGetWithExpiry(key).Expiry;
		}

		public bool SetEntryExpire(string key, TimeSpan duration)
		{
			if(string.IsNullOrWhiteSpace(key))
				throw new ArgumentNullException(nameof(key));

			return this.Database.KeyExpire(key, duration);
		}

		public bool SetEntryExpire(string key, DateTime expires)
		{
			if(string.IsNullOrWhiteSpace(key))
				throw new ArgumentNullException(nameof(key));

			return this.Database.KeyExpire(key, expires);
		}

		public bool Rename(string oldKey, string newKey)
		{
			if(string.IsNullOrWhiteSpace(oldKey))
				throw new ArgumentNullException(nameof(oldKey));

			if(string.IsNullOrWhiteSpace(newKey))
				throw new ArgumentNullException(nameof(newKey));

			return this.Database.KeyRename(oldKey, newKey, When.Exists);
		}

		public void Clear()
		{
			this.Redis.GetServer().FlushDatabaseAsync(this.Database.Database);

			//获取或创建Redis客户端代理对象
			var redis = this.Redis;

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
				throw new ArgumentNullException(nameof(key));

			return this.Database.KeyDelete(key);
		}

		public void RemoveMany(params string[] keys)
		{
			if(keys == null || keys.Length == 0)
				throw new ArgumentNullException(nameof(keys));

			this.Database.KeyDelete(keys.Cast<RedisKey>().ToArray());
		}

		public bool Contains(string key)
		{
			if(string.IsNullOrWhiteSpace(key))
				throw new ArgumentNullException(nameof(key));

			return this.Database.KeyExists(key);
		}

		public bool Exists(string key)
		{
			if(string.IsNullOrWhiteSpace(key))
				throw new ArgumentNullException(nameof(key));

			return this.Database.KeyExists(key);
		}

		public long Increment(string key, int interval = 1)
		{
			if(string.IsNullOrWhiteSpace(key))
				throw new ArgumentNullException(nameof(key));

			return this.Database.StringIncrement(key, interval);
		}

		public long Decrement(string key, int interval = 1)
		{
			if(string.IsNullOrWhiteSpace(key))
				throw new ArgumentNullException(nameof(key));

			return this.Database.StringDecrement(key, interval);
		}

		public HashSet<string> GetIntersect(params string[] sets)
		{
			if(sets == null)
				throw new ArgumentNullException(nameof(sets));

			return new HashSet<string>(this.Database.SetCombine(SetOperation.Intersect, sets.Cast<RedisKey>().ToArray()).Cast<string>());
		}

		public long SetIntersect(string destination, params string[] sets)
		{
			if(sets == null)
				throw new ArgumentNullException(nameof(sets));

			return this.Database.SetCombineAndStore(SetOperation.Intersect, destination, sets.Cast<string>().ToArray());
		}

		public HashSet<string> GetUnion(params string[] sets)
		{
			if(sets == null)
				throw new ArgumentNullException(nameof(sets));

			return new HashSet<string>(this.Database.SetCombine(SetOperation.Union, sets.Cast<RedisKey>().ToArray()).Cast<string>());
		}

		public long SetUnion(string destination, params string[] sets)
		{
			if(sets == null)
				throw new ArgumentNullException(nameof(sets));

			return this.Database.SetCombineAndStore(SetOperation.Union, destination, sets.Cast<RedisKey>().ToArray());
		}

		public long Publish(string channel, string message)
		{
			if(string.IsNullOrWhiteSpace(channel))
				throw new ArgumentNullException(nameof(channel));

			if(string.IsNullOrEmpty(message))
				return 0;

			return this.Redis.GetSubscriber().Publish(channel, message);
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
		private T GetCacheEntry<T>(string name, RedisEntryType entryType, Func<string, bool, T> createThunk) where T : RedisObjectBase
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
				var redisObject = createThunk(key, storedEntryType == RedisEntryType.None);

				redisObject.Disposed += (_, __) =>
				{
					cache.Remove(key);
				};

				return redisObject;
			});
		}
		#endregion

		#region 缓存接口
		event EventHandler<Runtime.Caching.CacheChangedEventArgs> Runtime.Caching.ICache.Changed
		{
			add
			{
				throw new NotImplementedException();
			}
			remove
			{
				throw new NotImplementedException();
			}
		}

		TimeSpan? Zongsoft.Runtime.Caching.ICache.GetExpiry(string key)
		{
			return this.GetEntryExpire(key);
		}

		void Zongsoft.Runtime.Caching.ICache.SetExpiry(string key, TimeSpan duration)
		{
			this.SetEntryExpire(key, duration);
		}

		T Zongsoft.Runtime.Caching.ICache.GetValue<T>(string key)
		{
			return Utility.ConvertValue<T>(((Zongsoft.Runtime.Caching.ICache)this).GetValue(key));
		}

		object Zongsoft.Runtime.Caching.ICache.GetValue(string key)
		{
			if(string.IsNullOrWhiteSpace(key))
				throw new ArgumentNullException(nameof(key));

			var creator = ((Zongsoft.Runtime.Caching.ICache)this).Creator;

			if(creator == null)
				return this.GetEntry(key);

			return ((Zongsoft.Runtime.Caching.ICache)this).GetValue(key, _ =>
			{
				TimeSpan duration;
				return new Tuple<object, TimeSpan>(creator.Create(null, _, out duration), duration);
			});
		}

		object Zongsoft.Runtime.Caching.ICache.GetValue(string key, Func<string, Zongsoft.Runtime.Caching.CacheEntry> valueCreator)
		{
			if(string.IsNullOrWhiteSpace(key))
				throw new ArgumentNullException(nameof(key));

			if(valueCreator == null)
				return this.GetEntry(key);

			var result = valueCreator(key);

			if(result.Value == null)
			{
				this.Database.KeyDelete(key);
				return null;
			}

			var text = result.Value.ToString();

			if(this.Database.StringSet(key, text, result.Expiry, When.NotExists))
				return text;

			return null;
		}

		bool Zongsoft.Runtime.Caching.ICache.SetValue(string key, object value)
		{
			if(string.IsNullOrWhiteSpace(key))
				throw new ArgumentNullException(nameof(key));

			if(value == null)
				return this.Database.KeyDelete(key);
			else
				return this.Database.StringSet(key, value.ToString());
		}

		bool Zongsoft.Runtime.Caching.ICache.SetValue(string key, object value, TimeSpan duration, bool requiredNotExists = false)
		{
			if(string.IsNullOrWhiteSpace(key))
				throw new ArgumentNullException(nameof(key));

			if(value == null)
				return this.Database.KeyDelete(key);
			else
				return this.Database.StringSet(key, value.ToString(), duration, requiredNotExists ? When.NotExists : When.Always);

			if(value == null)
				return this.Remove(key);

			if(value is RedisObjectBase)
				return false;

			IEnumerable<KeyValuePair<string, string>> dictionary;

			if(this.TryGetDictionary(value, out dictionary))
			{
				if(requiredNotExists && this.Exists(key))
					return false;

				var redisDictionary = this.GetDictionary(key);

				redisDictionary.SetRange(dictionary);

				if(duration > TimeSpan.Zero)
					this.SetEntryExpire(key, duration);

				return true;
			}

			ICollection<string> collection;

			if(this.TryGetCollection(value, out collection) && collection.Count > 0)
			{
				if(requiredNotExists && this.Exists(key))
					return false;

				var redisHashset = this.GetHashset(key);
				string[] values = collection as string[];

				if(values == null)
				{
					int index = 0;
					values = new string[collection.Count];

					foreach(var item in collection)
						values[index++] = item;
				}

				redisHashset.AddRange(values);

				if(duration > TimeSpan.Zero)
					this.SetEntryExpire(key, duration);

				return true;
			}

			return this.SetValue(key, Utility.GetStoreString(value), duration, requiredNotExists);
		}

		bool Zongsoft.Runtime.Caching.ICache.SetValue(string key, object value, DateTime expires, bool requiredNotExists = false)
		{
			if(string.IsNullOrWhiteSpace(key))
				throw new ArgumentNullException("key");

			if(value == null)
				return this.Remove(key);

			if(value is RedisObjectBase)
				return false;

			IEnumerable<KeyValuePair<string, string>> dictionary;

			if(this.TryGetDictionary(value, out dictionary))
			{
				if(requiredNotExists && this.Exists(key))
					return false;

				var redisDictionary = this.GetDictionary(key);

				redisDictionary.SetRange(dictionary);

				if(expires > DateTime.Now)
					this.SetEntryExpire(key, expires);

				return true;
			}

			ICollection<string> collection;

			if(this.TryGetCollection(value, out collection) && collection.Count > 0)
			{
				if(requiredNotExists && this.Exists(key))
					return false;

				var redisHashset = this.GetHashset(key);
				string[] values = collection as string[];

				if(values == null)
				{
					int index = 0;
					values = new string[collection.Count];

					foreach(var item in collection)
						values[index++] = item;
				}

				redisHashset.AddRange(values);

				if(expires > DateTime.Now)
					this.SetEntryExpire(key, expires);

				return true;
			}

			return this.SetValue(key, Utility.GetStoreString(value), (expires - DateTime.Now), requiredNotExists);
		}

		private bool TryGetDictionary(object value, out IEnumerable<KeyValuePair<string, string>> result)
		{
			result = null;

			if(value == null)
				return false;

			if(value is IEnumerable<KeyValuePair<string, string>>)
			{
				result = (IEnumerable<KeyValuePair<string, string>>)value;
				return true;
			}

			var dictionary = value as IDictionary;

			if(dictionary != null && dictionary.Count > 0)
			{
				var items = new List<KeyValuePair<string, string>>(dictionary.Count);

				foreach(var entryKey in dictionary.Keys)
				{
					if(entryKey != null && dictionary[entryKey] != null)
						items.Add(new KeyValuePair<string, string>(entryKey.ToString(), dictionary[entryKey].ToString()));
				}

				result = items;
				return true;
			}

			return false;
		}

		private bool TryGetCollection(object value, out ICollection<string> result)
		{
			result = null;

			if(value == null)
				return false;

			if(value is ICollection<string>)
			{
				result = (ICollection<string>)value;
				return true;
			}

			var collection = value as ICollection;

			if(collection != null && collection.Count > 0)
			{
				var items = new List<string>(collection.Count);

				foreach(var item in collection)
				{
					if(item != null)
						items.Add(item.ToString());
				}

				result = items;
				return true;
			}

			return false;
		}
		#endregion

		#region 获取缓存
		Zongsoft.Runtime.Caching.ICache Zongsoft.Runtime.Caching.ICacheProvider.GetCache(string name, bool createNotExists = false)
		{
			if(string.IsNullOrWhiteSpace(name))
				return this;

			if(!createNotExists)
			{
				if(!this.Exists(name.Trim()))
					return null;
			}

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
			if(_isDisposed)
				return;

			_isDisposed = true;
			_database = null;

			var redis = Interlocked.Exchange(ref _redis, null);

			if(redis != null)
				redis.Dispose();

			var caches = _caches;

			if(caches != null)
				caches.Clear();
		}
		#endregion
	}
}
