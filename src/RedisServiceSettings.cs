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
using System.Net;
using System.Collections.Generic;

namespace Zongsoft.Externals.Redis
{
	[Serializable]
	public class RedisServiceSettings
	{
		#region 成员字段
		private StackExchange.Redis.ConfigurationOptions _options;
		#endregion

		#region 构造函数
		public RedisServiceSettings(StackExchange.Redis.ConfigurationOptions options)
		{
			if(options == null)
				throw new ArgumentNullException(nameof(options));

			_options = options;
		}
		#endregion

		#region 公共属性
		public IList<EndPoint> Addresses
		{
			get
			{
				return _options.EndPoints;
			}
		}

		public string Password
		{
			get
			{
				return _options.Password;
			}
		}

		public int DatabaseId
		{
			get
			{
				return _options.DefaultDatabase ?? 0;
			}
		}

		public int ConnectionRetries
		{
			get
			{
				return _options.ConnectRetry;
			}
		}

		public TimeSpan ConnectionTimeout
		{
			get
			{
				return TimeSpan.FromMilliseconds(_options.ConnectTimeout);
			}
		}

		public TimeSpan OperationTimeout
		{
			get
			{
				return TimeSpan.FromMilliseconds(_options.ResponseTimeout);
			}
		}

		public bool UseTwemproxy
		{
			get
			{
				return _options.Proxy == StackExchange.Redis.Proxy.Twemproxy;
			}
		}

		public bool AdvancedModeEnabled
		{
			get
			{
				return _options.AllowAdmin;
			}
		}

		public bool DnsEnabled
		{
			get
			{
				return _options.ResolveDns;
			}
		}

		public bool SslEnabled
		{
			get
			{
				return _options.Ssl;
			}
		}

		public string SslHost
		{
			get
			{
				return _options.SslHost;
			}
		}

		public string ClientName
		{
			get
			{
				return _options.ClientName;
			}
		}

		public string ServiceName
		{
			get
			{
				return _options.ServiceName;
			}
		}
		#endregion

		#region 内部属性
		internal StackExchange.Redis.ConfigurationOptions InnerOptions
		{
			get
			{
				return _options;
			}
		}
		#endregion

		#region 重写方法
		public override string ToString()
		{
			return _options.ToString();
		}
		#endregion
	}
}
