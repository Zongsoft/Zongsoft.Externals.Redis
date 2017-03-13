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
using System.IO;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using StackExchange.Redis;

namespace Zongsoft.Externals.Redis
{
	public class RedisQueue : RedisObjectBase, IRedisQueue
	{
		#region 事件定义
		public event EventHandler<Zongsoft.Collections.DequeuedEventArgs> Dequeued;
		public event EventHandler<Zongsoft.Collections.EnqueuedEventArgs> Enqueued;
		#endregion

		#region 构造函数
		public RedisQueue(string name, StackExchange.Redis.IDatabase database) : base(name, database)
		{
		}
		#endregion

		#region 公共属性
		public int Capacity
		{
			get
			{
				return 0;
			}
		}

		public int Count
		{
			get
			{
				return (int)this.Database.ListLength(this.Name);
			}
		}

		bool ICollection.IsSynchronized
		{
			get
			{
				return false;
			}
		}

		private readonly object _syncRoot = new object();
		object ICollection.SyncRoot
		{
			get
			{
				return _syncRoot;
			}
		}
		#endregion

		#region 公共方法
		public void Clear()
		{
			this.Database.KeyDelete(this.Name);
		}

		public object Dequeue()
		{
			var result = this.Database.ListLeftPop(this.Name);

			if(result.IsNull)
				return null;

			//激发“Dequeued”事件
			this.OnDequeued(new Zongsoft.Collections.DequeuedEventArgs(result.ToString(), false, Collections.CollectionRemovedReason.Remove));

			return result.ToString();
		}

		public IEnumerable Dequeue(int length)
		{
			if(length < 1)
				throw new ArgumentOutOfRangeException("length");

			var count = Math.Min(length, this.Count);

			for(int i = 0; i < count; i++)
			{
				var result = this.Database.ListLeftPop(this.Name);

				//如果Redis队列返回值为空则表示队列已空
				if(result.IsNull)
					break;

				//激发“Dequeued”事件
				this.OnDequeued(new Zongsoft.Collections.DequeuedEventArgs(result.ToString(), false, Collections.CollectionRemovedReason.Remove));

				yield return result.ToString();
			}
		}

		public void Enqueue(string value)
		{
			if(value == null)
				throw new ArgumentNullException(nameof(value));

			if(this.Database.ListRightPush(this.Name, value) > 0)
				this.OnEnqueued(new Zongsoft.Collections.EnqueuedEventArgs(value, false));
		}

		public void Enqueue(object value, object settings = null)
		{
			if(value == null)
				throw new ArgumentNullException(nameof(value));

			if(this.Database.ListRightPush(this.Name, this.GetStoredValue(value)) > 0)
				this.OnEnqueued(new Zongsoft.Collections.EnqueuedEventArgs(value, false));
		}

		public int EnqueueMany<T>(IEnumerable<T> values, object settings = null)
		{
			if(values == null)
				throw new ArgumentNullException("values");

			var items = values.Select(p => this.GetStoredValue(p)).ToArray();
			var result = (int)this.Database.ListRightPush(this.Name, items);

			//激发“Enqueued”事件
			if(result > 0)
				this.OnEnqueued(new Zongsoft.Collections.EnqueuedEventArgs(values, true));

			return result;
		}

		public IEnumerable Peek(int length)
		{
			if(length < 1)
				throw new ArgumentOutOfRangeException("length");

			return this.Database.ListRange(this.Name, 0, length - 1).Cast<string>();
		}

		public object Peek()
		{
			return this.Database.ListGetByIndex(this.Name, 0);
		}

		public IEnumerable Take(int index, int length)
		{
			if(length > 0)
				return this.Database.ListRange(this.Name, index, index + length - 1).Cast<string>();
			else
				return this.Database.ListRange(this.Name, index).Cast<string>();
		}

		public object Take(int index)
		{
			return this.Database.ListGetByIndex(this.Name, index);
		}

		public void CopyTo(Array array, int index)
		{
			if(index < 0)
				throw new ArgumentOutOfRangeException("index");

			for(int i = index; i < array.Length; i++)
			{
				var item = this.Database.ListGetByIndex(this.Name, i - index);

				if(item.IsNull)
					break;

				array.SetValue((string)item, i);
			}
		}
		#endregion

		#region 激发事件
		protected virtual void OnDequeued(Zongsoft.Collections.DequeuedEventArgs args)
		{
			var dequeued = this.Dequeued;

			if(dequeued != null)
				dequeued(this, args);
		}

		protected virtual void OnEnqueued(Zongsoft.Collections.EnqueuedEventArgs args)
		{
			var enqueued = this.Enqueued;

			if(enqueued != null)
				enqueued(this, args);
		}
		#endregion

		#region 遍历枚举
		public System.Collections.IEnumerator GetEnumerator()
		{
			var count = this.Database.ListLength(this.Name);

			for(var i = 0; i < count; i++)
			{
				var result = this.Database.ListGetByIndex(this.Name, i);

				if(result.IsNull)
					yield break;

				yield return result.ToString();
			}
		}
		#endregion
	}
}
