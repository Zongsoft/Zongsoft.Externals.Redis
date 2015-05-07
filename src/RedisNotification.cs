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
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using ServiceStack.Redis;

namespace Zongsoft.Externals.Redis
{
	public class RedisNotification : MarshalByRefObject, Zongsoft.Common.IDisposableObject
	{
		#region 事件定义
		public event EventHandler<Zongsoft.Common.DisposedEventArgs> Disposed;
		public event EventHandler<RedisNotificationChannelEventArgs> Subscribed;
		public event EventHandler<RedisNotificationChannelEventArgs> Unsubscribed;
		public event EventHandler<RedisNotificationChannelMessageEventArgs> Notified;
		#endregion

		#region 成员字段
		private RedisClient _redis;
		private IRedisSubscription _redisSubscription;
		#endregion

		#region 构造函数
		public RedisNotification()
		{
		}

		public RedisNotification(RedisClient redis)
		{
			if(redis == null)
				throw new ArgumentNullException("redis");

			_redis = redis;
			_redisSubscription = new ServiceStack.Redis.RedisSubscription(_redis);
			_redisSubscription.OnMessage = this.OnNotified;
			_redisSubscription.OnSubscribe = this.OnSubscribed;
			_redisSubscription.OnUnSubscribe = this.OnUnsubscribed;
		}
		#endregion

		#region 公共属性
		public RedisClient Proxy
		{
			get
			{
				return _redis;
			}
			set
			{
				if(value == null)
					throw new ArgumentNullException();

				if(object.ReferenceEquals(_redis, value))
					return;

				_redis = value;

				var subscription = System.Threading.Interlocked.Exchange(ref _redisSubscription, new ServiceStack.Redis.RedisSubscription(_redis)
				{
					OnMessage = this.OnNotified,
					OnSubscribe = this.OnSubscribed,
					OnUnSubscribe = this.OnUnsubscribed,
				});

				if(subscription != null)
				{
					subscription.OnMessage = null;
					subscription.OnSubscribe = null;
					subscription.OnUnSubscribe = null;
				}
			}
		}

		public bool IsDisposed
		{
			get
			{
				return _redis == null;
			}
		}
		#endregion

		#region 公共方法
		public void Subscribe(params string[] channels)
		{
			var redis = this.Proxy;

			if(redis == null)
				throw new ObjectDisposedException(this.GetType().FullName);

			_redisSubscription.SubscribeToChannels(channels);
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
		protected virtual void OnNotified(string channel, string message)
		{
			var handler = this.Notified;

			if(handler != null)
				handler(this, new RedisNotificationChannelMessageEventArgs(channel, message));
		}

		protected virtual void OnSubscribed(string channel)
		{
			var handler = this.Subscribed;

			if(handler != null)
				handler(this, new RedisNotificationChannelEventArgs(channel));
		}

		protected virtual void OnUnsubscribed(string channel)
		{
			var handler = this.Unsubscribed;

			if(handler != null)
				handler(this, new RedisNotificationChannelEventArgs(channel));
		}
		#endregion

		#region 处置方法
		public void Dispose()
		{
			this.Dispose(true);
			GC.SuppressFinalize(this);

			var disposed = this.Disposed;

			if(disposed != null)
				disposed(this, new Common.DisposedEventArgs(true));
		}

		protected virtual void Dispose(bool disposing)
		{
			var redis = System.Threading.Interlocked.Exchange(ref _redis, null);

			if(redis != null)
				redis.Dispose();

			var redisSubscription = System.Threading.Interlocked.Exchange(ref _redisSubscription, null);

			if(redisSubscription != null)
			{
				redisSubscription.OnMessage = null;
				redisSubscription.OnSubscribe = null;
				redisSubscription.OnUnSubscribe = null;

				redisSubscription.Dispose();
			}
		}
		#endregion
	}
}
