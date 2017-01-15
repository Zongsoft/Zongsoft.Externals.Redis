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
		private readonly RedisServiceSettings _settings;
		private readonly Lazy<RedisSubscriber> _subscriber;
		private readonly Lazy<RedisSequence> _sequence;
		private IConnectionMultiplexer _redis;
		private IDatabase _database;
		private int _isDisposed;
		#endregion

		#region 构造函数
		public RedisService(Zongsoft.Options.IOptionProvider options)
		{
			var connectionString = "127.0.0.1";

			if(options != null)
			{
				var configuration = options.GetOptionObject("/Externals/Redis") as Configuration.IRedisConfiguration;

				if(configuration != null)
					connectionString = configuration.DefaultConnectionString;
			}

			_syncRoot = new object();
			_settings = new RedisServiceSettings(ConfigurationOptions.Parse(connectionString));
			_subscriber = new Lazy<RedisSubscriber>(() => new RedisSubscriber(this.Redis.GetSubscriber()), true);
			_sequence = new Lazy<RedisSequence>(() => new RedisSequence(this), true);
		}

		public RedisService(string connectionString)
		{
			if(string.IsNullOrWhiteSpace(connectionString))
				connectionString = "127.0.0.1";

			_syncRoot = new object();
			_settings = new RedisServiceSettings(ConfigurationOptions.Parse(connectionString));
			_subscriber = new Lazy<RedisSubscriber>(() => new RedisSubscriber(this.Redis.GetSubscriber()), true);
			_sequence = new Lazy<RedisSequence>(() => new RedisSequence(this), true);
		}
		#endregion

		#region 公共属性
		public long Count
		{
			get
			{
				return this.Redis.GetServer(this.Redis.GetCounters().EndPoint).DatabaseSize(this.DatabaseId);
			}
		}

		public string Name
		{
			get
			{
				string addresses = string.Empty;

				if(_settings.Addresses.Count > 1)
				{
					foreach(var endPoint in _settings.Addresses)
					{
						if(string.IsNullOrEmpty(addresses))
							addresses += ", ";

						addresses += endPoint.ToString();
					}

					addresses = "[" + addresses + "]";
				}
				else
				{
					addresses = _settings.Addresses.First().ToString();
				}

				if(_settings.UseTwemproxy)
					return $"(Twemproxy){addresses}#{this.DatabaseId}";
				else
					return addresses + "#" + this.DatabaseId;
			}
		}

		public bool IsDisposed
		{
			get
			{
				return _isDisposed != 0;
			}
		}

		public int DatabaseId
		{
			get
			{
				var database = _database;

				if(database == null)
					return _settings.DatabaseId;
				else
					return database.Database;
			}
		}

		public RedisServiceSettings Settings
		{
			get
			{
				return _settings;
			}
		}

		public RedisSubscriber Subscriber
		{
			get
			{
				return _subscriber.Value;
			}
		}

		public Common.ISequence Sequence
		{
			get
			{
				return _sequence.Value;
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
				if(_isDisposed != 0)
					throw new ObjectDisposedException(nameof(RedisService));

				if(_redis == null)
				{
					lock(_syncRoot)
					{
						if(_isDisposed != 0)
							throw new ObjectDisposedException(nameof(RedisService));

						if(_redis == null)
							_redis = ConnectionMultiplexer.Connect(_settings.InnerOptions);
					}
				}

				return _redis;
			}
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

		public string GetValue(string key)
		{
			if(string.IsNullOrWhiteSpace(key))
				throw new ArgumentNullException(nameof(key));

			return this.Database.StringGet(key);
		}

		public string[] GetValues(params string[] keys)
		{
			if(keys == null || keys.Length == 0)
				throw new ArgumentNullException(nameof(keys));

			return this.Database.StringGet(keys.Cast<RedisKey>().ToArray()).Cast<string>().ToArray();
		}

		public string ExchangeValue(string key, string value)
		{
			if(string.IsNullOrWhiteSpace(key))
				throw new ArgumentNullException(nameof(key));

			//更新指定键值对，并返回指定键的旧值
			return this.Database.StringGetSet(key, value);
		}

		public string ExchangeValue(string key, string value, TimeSpan duration)
		{
			if(string.IsNullOrWhiteSpace(key))
				throw new ArgumentNullException(nameof(key));

			//更新指定键值对，并返回指定键的旧值
			var result = this.Database.StringGetSet(key, value);

			//如果指定的有效期大于零，则设置它的有效期
			if(duration > TimeSpan.Zero)
				this.Database.KeyExpire(key, duration, CommandFlags.FireAndForget);

			return result;
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

		public object GetEntry(string key)
		{
			if(string.IsNullOrWhiteSpace(key))
				throw new ArgumentNullException(nameof(key));

			return this.GetEntry<object>(key, value => value);
		}

		public T GetEntry<T>(string key, Func<object, T> convert = null)
		{
			if(string.IsNullOrWhiteSpace(key))
				throw new ArgumentNullException(nameof(key));

			if(convert == null)
			{
				convert = value =>
				{
					if(value == null)
						return default(T);

					return Zongsoft.Common.Convert.ConvertValue<T>(value);
				};
			}

			var entryType = this.GetEntryType(key);

			switch(entryType)
			{
				case RedisEntryType.Dictionary:
					return convert(new RedisDictionary(key, this.Database));
				case RedisEntryType.List:
					return convert(new RedisQueue(key, this.Database));
				case RedisEntryType.Set:
				case RedisEntryType.SortedSet:
					return convert(new RedisHashset(key, this.Database));
				case RedisEntryType.String:
					return convert(this.GetValue(key));
			}

			return convert(default(T));
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

		public TimeSpan? GetEntryExpiry(string key)
		{
			if(string.IsNullOrWhiteSpace(key))
				throw new ArgumentNullException(nameof(key));

			return this.Database.KeyTimeToLive(key);
		}

		public bool SetEntryExpiry(string key, TimeSpan duration)
		{
			if(string.IsNullOrWhiteSpace(key))
				throw new ArgumentNullException(nameof(key));

			if(duration > TimeSpan.Zero)
				return this.Database.KeyExpire(key, duration);
			else
				return this.Database.KeyPersist(key);
		}

		public bool SetEntryExpiry(string key, DateTime expires)
		{
			if(string.IsNullOrWhiteSpace(key))
				throw new ArgumentNullException(nameof(key));

			if(expires.Year >= 2000)
				return this.Database.KeyExpire(key, expires);
			else
				return this.Database.KeyPersist(key);
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
			this.Redis.GetServer(this.Redis.GetCounters().EndPoint).FlushDatabase(this.DatabaseId);
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

			return this.Database.SetCombineAndStore(SetOperation.Intersect, destination, sets.Cast<RedisKey>().ToArray());
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
			return this.GetEntryExpiry(key);
		}

		void Zongsoft.Runtime.Caching.ICache.SetExpiry(string key, TimeSpan duration)
		{
			this.SetEntryExpiry(key, duration);
		}

		T Zongsoft.Runtime.Caching.ICache.GetValue<T>(string key)
		{
			return Utility.ConvertValue<T>(((Zongsoft.Runtime.Caching.ICache)this).GetValue(key));
		}

		object Zongsoft.Runtime.Caching.ICache.GetValue(string key)
		{
			if(string.IsNullOrWhiteSpace(key))
				throw new ArgumentNullException(nameof(key));

			return this.GetEntry(key);
		}

		object Zongsoft.Runtime.Caching.ICache.GetValue(string key, Func<string, Zongsoft.Runtime.Caching.CacheEntry> valueCreator)
		{
			if(string.IsNullOrWhiteSpace(key))
				throw new ArgumentNullException(nameof(key));

			if(valueCreator == null)
				return this.GetEntry(key);

			//获取指定的键值，即指定键对应的条目对象
			var result = this.GetEntry(key);

			//只有当指定的键不存在，才需要进行后续的设置操作
			if(result == null)
			{
				var entry = valueCreator(key);

				//当指定的键不存在则设置其新值
				if(this.SetValueCore(key, entry.Value, entry.Expiry ?? TimeSpan.Zero, true))
					return entry.Value;

				//再次获取一遍指定的键值
				return this.GetEntry(key);
			}

			return result;
		}

		bool Zongsoft.Runtime.Caching.ICache.SetValue(string key, object value)
		{
			if(string.IsNullOrWhiteSpace(key))
				throw new ArgumentNullException(nameof(key));

			return this.SetValueCore(key, value, TimeSpan.Zero);
		}

		bool Zongsoft.Runtime.Caching.ICache.SetValue(string key, object value, TimeSpan duration, bool requiredNotExists = false)
		{
			if(string.IsNullOrWhiteSpace(key))
				throw new ArgumentNullException(nameof(key));

			return this.SetValueCore(key, value, duration, requiredNotExists);
		}

		bool Zongsoft.Runtime.Caching.ICache.SetValue(string key, object value, DateTime expires, bool requiredNotExists = false)
		{
			if(string.IsNullOrWhiteSpace(key))
				throw new ArgumentNullException(nameof(key));

			return this.SetValueCore(key, value, expires - DateTime.Now, requiredNotExists);
		}

		private bool SetValueCore(string key, object value, TimeSpan duration, bool requiredNotExists = false)
		{
			if(string.IsNullOrWhiteSpace(key))
				throw new ArgumentNullException(nameof(key));

			if(value == null)
				return this.Database.KeyDelete(key);

			if(value is RedisObjectBase)
				return true;

			if(requiredNotExists && this.Database.KeyExists(key))
				return false;

			When condition = requiredNotExists ? When.NotExists : When.Always;

			Action<IBatch> complete = batch =>
			{
				if(duration > TimeSpan.Zero)
					batch.KeyExpireAsync(key, duration);
			};

			var set = value as ISet<string>;

			if(set != null)
				return this.Database.SetAdd(key, set.Cast<RedisValue>().ToArray()) > 0;

			if(this.Batch<DictionaryEntry>(value as IDictionary,
							(batch, entry) =>
							{
								if(entry.Key != null)
								{
									if(entry.Value == null)
										batch.HashDeleteAsync(key, entry.Key.ToString());
									else
										batch.HashSetAsync(key, entry.Key.ToString(), Utility.GetStoredValue(entry.Value), condition);
								}
							}, complete) ||
			   this.Batch<KeyValuePair<string, string>>(value as IDictionary<string, string>,
							(batch, entry) =>
							{
								if(entry.Key != null)
								{
									if(entry.Value == null)
										batch.HashDeleteAsync(key, entry.Key);
									else
										batch.HashSetAsync(key, entry.Key, entry.Value, condition);
								}
							}, complete) ||
			   this.Batch<KeyValuePair<string, object>>(value as IDictionary<string, object>,
							(batch, entry) =>
							{
								if(entry.Key != null)
								{
									if(entry.Value == null)
										batch.HashDeleteAsync(key, entry.Key);
									else
										batch.HashSetAsync(key, entry.Key, Utility.GetStoredValue(entry.Value), condition);
								}
							}, complete) ||
			   this.Batch<object>(value as Zongsoft.Collections.IQueue,
							(batch, item) =>
							{
								if(item != null)
									batch.ListRightPushAsync(key, Utility.GetStoredValue(item), condition);
							}, complete))
				return true;

			if(duration > TimeSpan.Zero)
				return this.Database.StringSet(key, value.ToString(), duration, condition);
			else
				return this.Database.StringSet(key, value.ToString(), null, condition);
		}

		private bool Batch<T>(IEnumerable items, Action<IBatch, T> iterator, Action<IBatch> complete = null)
		{
			if(items == null || iterator == null)
				return false;

			IBatch batch = null;

			foreach(var item in items)
			{
				if(item == null)
					continue;

				if(batch == null)
					batch = this.Database.CreateBatch();

				iterator(batch, (T)item);
			}

			if(batch != null)
			{
				if(complete != null)
					complete(batch);

				batch.Execute();
			}

			return batch != null;
		}
		#endregion

		#region 获取缓存
		public Zongsoft.Runtime.Caching.ICache GetCache(string name, bool createNotExists)
		{
			if(string.IsNullOrWhiteSpace(name))
				return this;

			//如果 createNotExists 参数为假(即当指定名称的字典不存在也无需创建一个)并且指定的字典名称不存在，则返回空(null)
			if(!createNotExists && !this.Exists(name))
				return null;

			//返回指定名称的字典对象
			return new RedisDictionary(name, this.Database);
		}
		#endregion

		#region 获取队列
		public Zongsoft.Collections.IQueue GetQueue(string name)
		{
			return this.GetEntry<RedisQueue>(name);
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
			var isDisposed = Interlocked.Exchange(ref _isDisposed, 1);

			if(isDisposed != 0)
				return;

			lock(_syncRoot)
			{
				_database = null;

				var redis = Interlocked.Exchange(ref _redis, null);

				if(redis != null)
					redis.Dispose();
			}
		}
		#endregion
	}
}
