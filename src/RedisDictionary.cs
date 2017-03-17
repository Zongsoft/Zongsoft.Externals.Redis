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
using System.Collections;
using System.Collections.Generic;
using System.Linq;

namespace Zongsoft.Externals.Redis
{
	public class RedisDictionary : RedisObjectBase, IRedisDictionary, IDictionary, Zongsoft.Common.IAccumulator, Zongsoft.Runtime.Caching.ICache
	{
		#region 构造函数
		public RedisDictionary(string name, StackExchange.Redis.IDatabase database) : base(name, database)
		{
		}
		#endregion

		#region 公共属性
		public int Count
		{
			get
			{
				return (int)this.Database.HashLength(this.Name);
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
				return this.Database.HashGet(this.Name, key);
			}
			set
			{
				this.Database.HashSet(this.Name, key, value);
			}
		}

		public ICollection<string> Keys
		{
			get
			{
				return this.Database.HashKeys(this.Name).Cast<string>().ToArray();
			}
		}

		public ICollection<string> Values
		{
			get
			{
				return this.Database.HashValues(this.Name).Cast<string>().ToArray();
			}
		}
		#endregion

		#region 公共方法
		public void SetRange(IEnumerable<KeyValuePair<string, string>> items)
		{
			if(items == null)
				return;

			this.Database.HashSet(this.Name, items.Select(item => new StackExchange.Redis.HashEntry(item.Key, item.Value)).ToArray());
		}

		public bool TryAdd(string key, string value)
		{
			return this.Database.HashSet(this.Name, key, value, StackExchange.Redis.When.NotExists);
		}

		public IReadOnlyList<string> GetValues(params string[] keys)
		{
			return this.Database.HashGet(this.Name, keys.Cast<StackExchange.Redis.RedisValue>().ToArray()).Cast<string>().ToArray();
		}

		public IReadOnlyDictionary<string, string> GetAllEntries()
		{
			return this.Database.HashGetAll(this.Name).ToDictionary(entry => (string)entry.Name, entry => (string)entry.Value);
		}

		public long Increment(string key, int interval = 1)
		{
			return this.Database.HashIncrement(this.Name, key, interval);
		}

		public long Decrement(string key, int interval = 1)
		{
			return this.Database.HashDecrement(this.Name, key, interval);
		}

		public void Add(string key, string value)
		{
			if(string.IsNullOrWhiteSpace(key))
				throw new ArgumentNullException(nameof(key));

			if(value == null)
				throw new ArgumentNullException(nameof(value));

			if(!this.Database.HashSet(this.Name, key, value, StackExchange.Redis.When.NotExists))
				throw new RedisException($"The '{key}' key of entry is existed in the '{this.Name}' dictionary.");
		}

		public bool ContainsKey(string key)
		{
			return this.Database.HashExists(this.Name, key);
		}

		public bool Exists(string key)
		{
			return this.Database.HashExists(this.Name, key);
		}

		public bool Remove(string key)
		{
			return this.Database.HashDelete(this.Name, key);
		}

		public bool TryGetValue(string key, out string value)
		{
			value = this.Database.HashGet(this.Name, key);
			return value != null;
		}

		public void Clear()
		{
			this.Database.KeyDelete(this.Name);
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
			var entries = this.Database.HashGetAll(this.Name);

			if(entries != null && entries.Length > 0)
			{
				for(int i = 0; i < entries.Length && arrayIndex + i < array.Length; i++)
				{
					array[arrayIndex + i] = new KeyValuePair<string, string>(entries[i].Name, entries[i].Value);
				}
			}
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
				return this.Database.HashLength(this.Name);
			}
		}

		bool Zongsoft.Runtime.Caching.ICache.Rename(string key, string newKey)
		{
			throw new NotSupportedException("The cache container isn't supports the feature.");
		}

		TimeSpan? Zongsoft.Runtime.Caching.ICache.GetExpiry(string key)
		{
			throw new NotSupportedException("The cache container isn't supports the feature.");
		}

		void Zongsoft.Runtime.Caching.ICache.SetExpiry(string key, TimeSpan duration)
		{
			throw new NotSupportedException("The cache container isn't supports the feature.");
		}

		public object GetValue(string key)
		{
			return this.Database.HashGet(this.Name, key);
		}

		T Zongsoft.Runtime.Caching.ICache.GetValue<T>(string key)
		{
			return Utility.ConvertValue<T>(this.GetValue(key));
		}

		object Zongsoft.Runtime.Caching.ICache.GetValue(string key, Func<string, Runtime.Caching.CacheEntry> valueCreator)
		{
			if(string.IsNullOrWhiteSpace(key))
				throw new ArgumentNullException(nameof(key));

			var result = this.Database.HashGet(this.Name, key);

			if(valueCreator == null)
				return result.IsNull ? null : result.ToString();

			if(result.IsNull)
			{
				var entry = valueCreator(key);

				if(this.Database.HashSet(this.Name, key, this.GetStoredValue(entry.Value), StackExchange.Redis.When.NotExists))
					return entry.Value;

				//再次获取一遍指定的键值
				return this.Database.HashGet(this.Name, key);
			}

			return result.ToString();
		}

		public bool SetValue(string key, object value)
		{
			return ((Zongsoft.Runtime.Caching.ICache)this).SetValue(key, value, TimeSpan.Zero, false);
		}

		bool Zongsoft.Runtime.Caching.ICache.SetValue(string key, object value, TimeSpan duration, bool requiredNotExists = false)
		{
			if(duration > TimeSpan.Zero)
				throw new NotSupportedException("The cache container isn't supports the feature, the expires must be zero.");

			if(value == null)
				return this.Database.HashDelete(this.Name, key);

			if(requiredNotExists)
				return this.Database.HashSet(this.Name, key, this.GetStoredValue(value), StackExchange.Redis.When.NotExists);
			else
				return this.Database.HashSet(this.Name, key, this.GetStoredValue(value), StackExchange.Redis.When.Always);
		}

		bool Zongsoft.Runtime.Caching.ICache.SetValue(string key, object value, DateTime expires, bool requiredNotExists = false)
		{
			if(expires > DateTime.Now)
				throw new NotSupportedException("The cache container isn't supports the feature, the expires must be zero.");

			return ((Zongsoft.Runtime.Caching.ICache)this).SetValue(key, value, TimeSpan.Zero, requiredNotExists);
		}
		#endregion

		#region 遍历枚举
		public IEnumerator<KeyValuePair<string, string>> GetEnumerator()
		{
			return this.Database.HashScan(this.Name).Select(p => new KeyValuePair<string, string>(p.Name.ToString(), p.Value.ToString())).GetEnumerator();
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
				throw new ArgumentNullException(nameof(key));

			if(value == null)
				throw new ArgumentNullException(nameof(value));

			if(!this.Database.HashSet(this.Name, key.ToString(), this.GetStoredValue(value), StackExchange.Redis.When.NotExists))
				throw new RedisException($"The '{key}' key of entry is existed in the '{this.Name}' dictionary.");
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
				return this.Database.HashKeys(this.Name).Cast<string>().ToArray();
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
				return this.Database.HashValues(this.Name).Cast<string>().ToArray();
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
					this[key.ToString()] = Utility.GetStoredValue(value);
			}
		}

		void ICollection.CopyTo(Array array, int index)
		{
			var entries = this.Database.HashGetAll(this.Name);

			if(entries != null && entries.Length > 0)
			{
				for(int i = 0; i < entries.Length && index + i < array.Length; i++)
				{
					array.SetValue(new KeyValuePair<string, string>(entries[i].Name, entries[i].Value), index + i);
				}
			}
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
