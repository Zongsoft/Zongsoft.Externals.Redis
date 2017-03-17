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

using StackExchange.Redis;

namespace Zongsoft.Externals.Redis
{
	public class RedisHashset : RedisObjectBase, IRedisHashset, ICollection, IList, ICollection<string>
	{
		#region 私有变量
		private readonly object _syncRoot;
		#endregion

		#region 构造函数
		public RedisHashset(string name, StackExchange.Redis.IDatabase database) : base(name, database)
		{
			_syncRoot = new object();
		}
		#endregion

		#region 公共属性
		public int Count
		{
			get
			{
				return (int)this.Database.SetLength(this.Name);
			}
		}

		public bool IsReadOnly
		{
			get
			{
				return false;
			}
		}
		#endregion

		#region 公共方法
		public HashSet<string> GetExcept(params string[] other)
		{
			return new HashSet<string>(this.Database.SetCombine(SetOperation.Difference, this.GetRedisKeys(other)).Cast<string>());
		}

		public long SetExcept(string destination, params string[] other)
		{
			return this.Database.SetCombineAndStore(SetOperation.Difference, destination, this.GetRedisKeys(other));
		}

		public HashSet<string> GetIntersect(params string[] other)
		{
			return new HashSet<string>(this.Database.SetCombine(SetOperation.Intersect, this.GetRedisKeys(other)).Cast<string>());
		}

		public long SetIntersect(string destination, params string[] other)
		{
			return this.Database.SetCombineAndStore(SetOperation.Intersect, destination, this.GetRedisKeys(other));
		}

		public HashSet<string> GetUnion(params string[] other)
		{
			return new HashSet<string>(this.Database.SetCombine(SetOperation.Union, this.GetRedisKeys(other)).Cast<string>());
		}

		public long SetUnion(string destination, params string[] other)
		{
			return this.Database.SetCombineAndStore(SetOperation.Union, destination, this.GetRedisKeys(other));
		}

		public HashSet<string> GetRandomValues(int count)
		{
			return new HashSet<string>(this.Database.SetRandomMembers(this.Name, count).Cast<string>());
		}

		public bool Move(string destination, string item)
		{
			return this.Database.SetMove(this.Name, destination, item);
		}

		public int RemoveRange(params string[] items)
		{
			return (int)this.Database.SetRemove(this.Name, items.Cast<RedisValue>().ToArray());
		}

		public bool Remove(string item)
		{
			return this.Database.SetRemove(this.Name, item);
		}

		public void Add(string item)
		{
			this.Database.SetAdd(this.Name, item);
		}

		public int AddRange(IEnumerable<string> items)
		{
			if(items == null)
				return 0;

			return (int)this.Database.SetAdd(this.Name, items.Cast<RedisValue>().ToArray());
		}

		public int AddRange(params string[] items)
		{
			return this.AddRange((IEnumerable<string>)items);
		}

		public void Clear()
		{
			this.Database.KeyDelete(this.Name);
		}

		public bool Contains(string item)
		{
			return this.Database.SetContains(this.Name, item);
		}
		#endregion

		#region 显式实现
		bool ICollection.IsSynchronized
		{
			get
			{
				return true;
			}
		}

		object ICollection.SyncRoot
		{
			get
			{
				return _syncRoot;
			}
		}

		void ICollection.CopyTo(Array array, int index)
		{
			var items = this.Database.SetMembers(this.Name);

			if(items != null && items.Length > 0)
				Array.Copy(items, 0, array, index, array.Length - index);
		}

		void ICollection<string>.CopyTo(string[] array, int index)
		{
			var items = this.Database.SetMembers(this.Name);

			if(items != null && items.Length > 0)
				Array.Copy(items, 0, array, index, array.Length - index);
		}

		int IList.Add(object value)
		{
			if(value == null)
				throw new ArgumentNullException(nameof(value));

			this.Database.SetAdd(this.Name, this.GetStoredValue(value));

			return -1;
		}

		bool IList.Contains(object value)
		{
			if(value == null)
				return false;

			return this.Contains(this.GetStoredValue(value));
		}

		int IList.IndexOf(object value)
		{
			throw new NotSupportedException();
		}

		void IList.Insert(int index, object value)
		{
			throw new NotSupportedException();
		}

		void IList.Remove(object value)
		{
			if(value == null)
				return;

			this.Database.SetRemove(this.Name, this.GetStoredValue(value));
		}

		void IList.RemoveAt(int index)
		{
			throw new NotSupportedException();
		}

		bool IList.IsFixedSize
		{
			get
			{
				return false;
			}
		}

		object IList.this[int index]
		{
			get
			{
				throw new NotSupportedException();
			}
			set
			{
				throw new NotSupportedException();
			}
		}
		#endregion

		#region 遍历枚举
		public IEnumerator<string> GetEnumerator()
		{
			return this.Database.SetScan(this.Name).Select(p => p.ToString()).GetEnumerator();
		}

		IEnumerator IEnumerable.GetEnumerator()
		{
			return this.GetEnumerator();
		}
		#endregion

		#region 私有方法
		private RedisKey[] GetRedisKeys(string[] keys)
		{
			if(keys == null || keys.Length == 0)
				throw new ArgumentNullException(nameof(keys));

			var result = new RedisKey[keys.Length + 1];
			result[0] = this.Name;
			Array.Copy(keys, 0, result, 1, keys.Length);
			return result;
		}
		#endregion
	}
}
