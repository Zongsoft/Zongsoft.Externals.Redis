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
using System.Collections;
using System.Collections.Generic;

namespace Zongsoft.Externals.Redis
{
	public class RedisDictionary : RedisObjectBase, IRedisDictionary, IDictionary, Zongsoft.Common.IAccumulator, Zongsoft.Runtime.Caching.ICache
	{
		#region 构造函数
		public RedisDictionary(string name, Zongsoft.Collections.ObjectPool<ServiceStack.Redis.IRedisClient> redisPool) : base(name, redisPool)
		{
		}
		#endregion

		#region 公共属性
		public int Count
		{
			get
			{
				var redis = this.Redis;

				try
				{
					return (int)redis.GetHashCount(this.Name);
				}
				finally
				{
					this.RedisPool.Release(redis);
				}
			}
		}

		bool ICollection<KeyValuePair<string, string>>.IsReadOnly
		{
			get
			{
				return false;
			}
		}

		public string this[string key]
		{
			get
			{
				var redis = this.Redis;

				try
				{
					return redis.GetValueFromHash(this.Name, key);
				}
				finally
				{
					this.RedisPool.Release(redis);
				}
			}
			set
			{
				var redis = this.Redis;

				try
				{
					redis.SetEntryInHash(this.Name, key, value);
				}
				finally
				{
					this.RedisPool.Release(redis);
				}
			}
		}

		public ICollection<string> Keys
		{
			get
			{
				var redis = this.Redis;

				try
				{
					return redis.GetHashKeys(this.Name);
				}
				finally
				{
					this.RedisPool.Release(redis);
				}
			}
		}

		public ICollection<string> Values
		{
			get
			{
				var redis = this.Redis;

				try
				{
					return redis.GetHashValues(this.Name);
				}
				finally
				{
					this.RedisPool.Release(redis);
				}
			}
		}
		#endregion

		#region 公共方法
		public void SetRange(IEnumerable<KeyValuePair<string, string>> items)
		{
			if(items == null)
				return;

			var redis = this.Redis;

			try
			{
				redis.SetRangeInHash(this.Name, items);
			}
			finally
			{
				this.RedisPool.Release(redis);
			}
		}

		public bool TryAdd(string key, string value)
		{
			var redis = this.Redis;

			try
			{
				return redis.SetEntryInHashIfNotExists(this.Name, key, value);
			}
			finally
			{
				this.RedisPool.Release(redis);
			}
		}

		public IReadOnlyList<string> GetValues(params string[] keys)
		{
			var redis = this.Redis;

			try
			{
				return redis.GetValuesFromHash(this.Name, keys);
			}
			finally
			{
				this.RedisPool.Release(redis);
			}
		}

		public IReadOnlyDictionary<string, string> GetAllEntries()
		{
			var redis = this.Redis;

			try
			{
				return redis.GetAllEntriesFromHash(this.Name);
			}
			finally
			{
				this.RedisPool.Release(redis);
			}
		}

		public long Increment(string key, int interval = 1)
		{
			var redis = this.Redis;

			try
			{
				return redis.IncrementValueInHash(this.Name, key, interval);
			}
			finally
			{
				this.RedisPool.Release(redis);
			}
		}

		public long Decrement(string key, int interval = 1)
		{
			var redis = this.Redis;

			try
			{
				return redis.IncrementValueInHash(this.Name, key, -interval);
			}
			finally
			{
				this.RedisPool.Release(redis);
			}
		}

		public void Add(string key, string value)
		{
			var redis = this.Redis;

			try
			{
				if(!redis.SetEntryInHashIfNotExists(this.Name, key, value))
					throw new RedisException(string.Format("The '{1}' key of entry is existed in the '{0}' dictionary.", this.Name, key));
			}
			finally
			{
				this.RedisPool.Release(redis);
			}
		}

		public bool ContainsKey(string key)
		{
			var redis = this.Redis;

			try
			{
				return redis.HashContainsEntry(this.Name, key);
			}
			finally
			{
				this.RedisPool.Release(redis);
			}
		}

		public bool Exists(string key)
		{
			var redis = this.Redis;

			try
			{
				return redis.HashContainsEntry(this.Name, key);
			}
			finally
			{
				this.RedisPool.Release(redis);
			}
		}

		public bool Remove(string key)
		{
			var redis = this.Redis;

			try
			{
				return redis.RemoveEntryFromHash(this.Name, key);
			}
			finally
			{
				this.RedisPool.Release(redis);
			}
		}

		public bool TryGetValue(string key, out string value)
		{
			var redis = this.Redis;

			try
			{
				value = redis.GetValueFromHash(this.Name, key);
				return value != null;
			}
			finally
			{
				this.RedisPool.Release(redis);
			}
		}

		public void Clear()
		{
			var redis = this.Redis;

			try
			{
				redis.Remove(this.Name);
			}
			finally
			{
				this.RedisPool.Release(redis);
			}
		}
		#endregion

		#region 显式实现
		void ICollection<KeyValuePair<string, string>>.Add(KeyValuePair<string, string> item)
		{
			this.Add(item.Key, item.Value);
		}

		bool ICollection<KeyValuePair<string, string>>.Contains(KeyValuePair<string, string> item)
		{
			if(item.Key == null)
				return false;

			return this.ContainsKey(item.Key);
		}

		void ICollection<KeyValuePair<string, string>>.CopyTo(KeyValuePair<string, string>[] array, int arrayIndex)
		{
			throw new NotImplementedException();
		}

		bool ICollection<KeyValuePair<string, string>>.Remove(KeyValuePair<string, string> item)
		{
			if(item.Key == null)
				return false;

			return this.Remove(item.Key);
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

		long Zongsoft.Runtime.Caching.ICache.Count
		{
			get
			{
				var redis = this.Redis;

				try
				{
					return redis.GetHashCount(this.Name);
				}
				finally
				{
					this.RedisPool.Release(redis);
				}
			}
		}

		Zongsoft.Runtime.Caching.ICacheCreator Zongsoft.Runtime.Caching.ICache.Creator
		{
			get;
			set;
		}

		bool Zongsoft.Runtime.Caching.ICache.Rename(string key, string newKey)
		{
			throw new NotSupportedException("The cache container isn't supports the feature.");
		}

		TimeSpan? Zongsoft.Runtime.Caching.ICache.GetDuration(string key)
		{
			throw new NotSupportedException("The cache container isn't supports the feature.");
		}

		void Zongsoft.Runtime.Caching.ICache.SetDuration(string key, TimeSpan duration)
		{
			throw new NotSupportedException("The cache container isn't supports the feature.");
		}

		public object GetValue(string key)
		{
			var creator = ((Zongsoft.Runtime.Caching.ICache)this).Creator;

			if(creator == null)
				return ((Zongsoft.Runtime.Caching.ICache)this).GetValue(key, null);
			else
				return ((Zongsoft.Runtime.Caching.ICache)this).GetValue(key, _ =>
				{
					TimeSpan duration;
					return new Tuple<object, TimeSpan>(creator.Create(this.Name, _, out duration), duration);
				});
		}

		object Zongsoft.Runtime.Caching.ICache.GetValue(string key, Func<string, Tuple<object, TimeSpan>> valueCreator)
		{
			var redis = this.Redis;

			try
			{
				if(valueCreator == null)
					return redis.GetValueFromHash(this.Name, key);

				var result = valueCreator(key);

				if(result == null && result.Item1 == null)
				{
					redis.RemoveEntryFromHash(this.Name, key);
					return null;
				}

				if(redis.SetEntryInHashIfNotExists(this.Name, key, result.Item1.ToString()))
					return result.Item1;

				return redis.GetValueFromHash(this.Name, key);
			}
			finally
			{
				this.RedisPool.Release(redis);
			}
		}

		public bool SetValue(string key, object value)
		{
			return ((Zongsoft.Runtime.Caching.ICache)this).SetValue(key, value, TimeSpan.Zero, false);
		}

		bool Zongsoft.Runtime.Caching.ICache.SetValue(string key, object value, TimeSpan duration, bool requiredNotExists = false)
		{
			if(duration > TimeSpan.Zero)
				throw new NotSupportedException("The cache container isn't supports the feature.");

			var redis = this.Redis;

			try
			{
				if(value == null)
					return redis.RemoveEntryFromHash(this.Name, key);

				if(requiredNotExists)
					return redis.SetEntryInHashIfNotExists(this.Name, key, value.ToString());
				else
					return redis.SetEntryInHash(this.Name, key, value.ToString());
			}
			finally
			{
				this.RedisPool.Release(redis);
			}
		}
		#endregion

		#region 遍历枚举
		public IEnumerator<KeyValuePair<string, string>> GetEnumerator()
		{
			Dictionary<string, string> entries;
			var redis = this.Redis;

			try
			{
				entries = redis.GetAllEntriesFromHash(this.Name);
			}
			finally
			{
				this.RedisPool.Release(redis);
			}

			foreach(var entry in entries)
			{
				yield return entry;
			}
		}

		IDictionaryEnumerator IDictionary.GetEnumerator()
		{
			throw new NotSupportedException();
		}

		System.Collections.IEnumerator System.Collections.IEnumerable.GetEnumerator()
		{
			return this.GetEnumerator();
		}
		#endregion

		#region 字典接口
		void IDictionary.Add(object key, object value)
		{
			if(key == null)
				throw new ArgumentNullException("key");

			if(value == null)
				return;

			this.Add(key.ToString(), value.ToString());
		}

		void IDictionary.Clear()
		{
			this.Clear();
		}

		bool IDictionary.Contains(object key)
		{
			if(key == null)
				throw new ArgumentNullException("key");

			return this.ContainsKey(key.ToString());
		}

		bool IDictionary.IsFixedSize
		{
			get
			{
				return false;
			}
		}

		bool IDictionary.IsReadOnly
		{
			get
			{
				return false;
			}
		}

		ICollection IDictionary.Keys
		{
			get
			{
				var entries = this.GetAllEntries();

				if(entries == null)
					return null;

				var keys = new string[entries.Count];
				var index = 0;

				foreach(var key in entries.Keys)
				{
					keys[index++] = key;
				}

				return keys;
			}
		}

		void IDictionary.Remove(object key)
		{
			if(key == null)
				return;

			this.Remove(key.ToString());
		}

		ICollection IDictionary.Values
		{
			get
			{
				var entries = this.GetAllEntries();

				if(entries == null)
					return null;

				var values = new string[entries.Count];
				var index = 0;

				foreach(var value in entries.Values)
				{
					values[index++] = value;
				}

				return values;
			}
		}

		object IDictionary.this[object key]
		{
			get
			{
				if(key == null)
					return null;

				return this[key.ToString()];
			}
			set
			{
				if(key == null)
					throw new ArgumentNullException("key");

				if(value == null)
					this.Remove(key.ToString());
				else
					this[key.ToString()] = value.ToString();
			}
		}

		void ICollection.CopyTo(Array array, int index)
		{
			throw new NotImplementedException();
		}

		int ICollection.Count
		{
			get
			{
				return this.Count;
			}
		}

		bool ICollection.IsSynchronized
		{
			get
			{
				return false;
			}
		}

		private object _syncRoot;

		object ICollection.SyncRoot
		{
			get
			{
				if(_syncRoot == null)
					System.Threading.Interlocked.CompareExchange(ref _syncRoot, new object(), null);

				return _syncRoot;
			}
		}
		#endregion
	}
}
