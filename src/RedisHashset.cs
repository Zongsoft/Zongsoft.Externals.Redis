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
using System.Collections.Generic;

namespace Zongsoft.Externals.Redis
{
	public class RedisHashset : RedisObjectBase, IRedisHashset
	{
		#region 构造函数
		public RedisHashset(string name, ServiceStack.Redis.IRedisClient redis) : base(name, redis)
		{
		}

		public RedisHashset(string name, Zongsoft.Common.IObjectReference<ServiceStack.Redis.IRedisClient> redisReference) : base(name, redisReference)
		{
		}
		#endregion

		public int Count
		{
			get
			{
				return (int)this.Redis.GetSetCount(this.Name);
			}
		}

		public bool IsReadOnly
		{
			get
			{
				return false;
			}
		}

		public HashSet<string> GetExcept(params string[] other)
		{
			return this.Redis.GetDifferencesFromSet(this.Name, other);
		}

		public void SetExcept(string destination, params string[] other)
		{
			this.Redis.StoreDifferencesFromSet(destination, this.Name, other);
		}

		public HashSet<string> GetIntersect(params string[] other)
		{
			var sets = new string[other.Length + 1];
			sets[0] = this.Name;
			Array.Copy(other, 0, sets, 1, other.Length);

			return this.Redis.GetIntersectFromSets(sets);
		}

		public void SetIntersect(string destination, params string[] other)
		{
			var sets = new string[other.Length + 1];
			sets[0] = this.Name;
			Array.Copy(other, 0, sets, 1, other.Length);

			this.Redis.StoreIntersectFromSets(destination, sets);
		}

		public HashSet<string> GetUnion(params string[] other)
		{
			var sets = new string[other.Length + 1];
			sets[0] = this.Name;
			Array.Copy(other, 0, sets, 1, other.Length);

			return this.Redis.GetUnionFromSets(sets);
		}

		public void SetUnion(string destination, params string[] other)
		{
			var sets = new string[other.Length + 1];
			sets[0] = this.Name;
			Array.Copy(other, 0, sets, 1, other.Length);

			this.Redis.StoreUnionFromSets(destination, sets);
		}

		public HashSet<string> GetRandomValues(int count)
		{
			var result = new HashSet<string>();

			for(int i = 0; i < Math.Max(1, count); i++)
			{
				result.Add(this.Redis.GetRandomItemFromSet(this.Name));
			}

			return result;
		}

		public bool Move(string destination, string item)
		{
			this.Redis.MoveBetweenSets(this.Name, destination, item);
			return true;
		}

		public void RemoveRange(params string[] items)
		{
			using(var transaction = this.Redis.CreateTransaction())
			{
				foreach(var item in items)
				{
					transaction.QueueCommand(proxy => proxy.RemoveEntryFromHash(this.Name, item));
				}

				transaction.Commit();
			}
		}

		public bool Remove(string item)
		{
			this.Redis.RemoveItemFromSet(this.Name, item);
			return true;
		}

		public void Add(string item)
		{
			this.Redis.AddItemToSet(this.Name, item);
		}

		public void AddRange(params string[] items)
		{
			this.Redis.AddRangeToSet(this.Name, System.Linq.Enumerable.ToList(items));
		}

		public void Clear()
		{
			this.Redis.Remove(this.Name);
		}

		public bool Contains(string item)
		{
			return this.Redis.SetContainsItem(this.Name, item);
		}

		void ICollection<string>.CopyTo(string[] array, int arrayIndex)
		{
			throw new NotImplementedException();
		}

		public IEnumerator<string> GetEnumerator()
		{
			var items = this.Redis.GetAllItemsFromSet(this.Name);

			foreach(var item in items)
			{
				yield return item;
			}
		}

		System.Collections.IEnumerator System.Collections.IEnumerable.GetEnumerator()
		{
			return this.GetEnumerator();
		}
	}
}
