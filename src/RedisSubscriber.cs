/*
 * Authors:
 *   钟峰(Popeye Zhong) <9555843@qq.com>
 *
 * Copyright (C) 2014-2016 Zongsoft Corporation <http://www.zongsoft.com>
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

namespace Zongsoft.Externals.Redis
{
	public class RedisSubscriber : MarshalByRefObject, Zongsoft.Common.IDisposableObject
	{
		#region 事件定义
		public event EventHandler<Zongsoft.Common.DisposedEventArgs> Disposed;
		public event EventHandler<RedisChannelEventArgs> Subscribed;
		public event EventHandler<RedisChannelEventArgs> Unsubscribed;
		public event EventHandler<RedisChannelMessageEventArgs> Received;
		#endregion

		#region 成员字段
		private StackExchange.Redis.ISubscriber _subscriber;
		#endregion

		#region 构造函数
		public RedisSubscriber(StackExchange.Redis.ISubscriber subscriber)
		{
			if(subscriber == null)
				throw new ArgumentNullException(nameof(subscriber));

			_subscriber = subscriber;
		}
		#endregion

		#region 公共属性
		public bool IsDisposed
		{
			get
			{
				return _subscriber == null;
			}
		}

		public StackExchange.Redis.ISubscriber Subscriber
		{
			get
			{
				var subscriber = _subscriber;

				if(subscriber == null)
					throw new ObjectDisposedException(nameof(RedisSubscriber));

				return subscriber;
			}
		}
		#endregion

		#region 公共方法
		public void Subscribe(params string[] channels)
		{
			foreach(var channel in channels)
			{
				this.Subscriber.Subscribe(channel, (ch, message) =>
				{
					this.OnReceived(ch, message);
				});

				this.OnSubscribed(channel);
			}
		}

		public void Unsubscribe(params string[] channels)
		{
			var redis = this.Proxy;

			if(redis == null)
				throw new ObjectDisposedException(this.GetType().FullName);

			_redisSubscription.UnSubscribeFromChannels(channels);
		}

		public void UnsubscribeAll()
		{
			_redisSubscription.UnSubscribeFromAllChannels();
		}

		public long Publish(string channel, string message)
		{
			var redis = this.Proxy;

			if(redis == null)
				throw new ObjectDisposedException(this.GetType().FullName);

			if(string.IsNullOrWhiteSpace(channel))
				throw new ArgumentNullException("channel");

			if(string.IsNullOrEmpty(message))
				return 0;

			return redis.PublishMessage(channel, message);
		}
		#endregion

		#region 事件处理
		protected virtual void OnReceived(string channel, string message)
		{
			var handler = this.Received;

			if(handler != null)
				handler(this, new RedisChannelMessageEventArgs(channel, message));
		}

		protected virtual void OnSubscribed(string channel)
		{
			var handler = this.Subscribed;

			if(handler != null)
				handler(this, new RedisChannelEventArgs(channel));
		}

		protected virtual void OnUnsubscribed(string channel)
		{
			var handler = this.Unsubscribed;

			if(handler != null)
				handler(this, new RedisChannelEventArgs(channel));
		}
		#endregion

		#region 处置方法
		public void Dispose()
		{
			this.Dispose(true);
			GC.SuppressFinalize(this);

			//激发“Disposed”事件
			this.Disposed?.Invoke(this, new Common.DisposedEventArgs(true));
		}

		protected virtual void Dispose(bool disposing)
		{
			var subscriber = System.Threading.Interlocked.Exchange(ref _subscriber, null);

			if(subscriber != null)
				subscriber.UnsubscribeAll(StackExchange.Redis.CommandFlags.FireAndForget);
		}
		#endregion
	}
}
