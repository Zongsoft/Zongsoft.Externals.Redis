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
using System.Net;
using System.Collections.Generic;

namespace Zongsoft.Externals.Redis
{
	[Serializable]
	public class RedisServiceSettings
	{
		#region 成员字段
		private IPEndPoint _address;
		private string _password;
		private int _databaseId;
		private TimeSpan _timeout;
		private int _poolSize;
		#endregion

		#region 构造函数
		public RedisServiceSettings()
		{
			_address = new IPEndPoint(IPAddress.Loopback, 6379);
		}

		public RedisServiceSettings(IPEndPoint address, string password, int databaseId = 0)
		{
			if(address == null)
				throw new ArgumentNullException("address");

			_address = address;
			_password = password;
			_databaseId = Math.Abs(databaseId);
		}
		#endregion

		#region 公共属性
		public IPEndPoint Address
		{
			get
			{
				return _address;
			}
			set
			{
				if(value == null)
					throw new ArgumentNullException();

				if(_address != value)
					_address = value;
			}
		}

		public string Password
		{
			get
			{
				return _password;
			}
			set
			{
				_password = value;
			}
		}

		public int DatabaseId
		{
			get
			{
				return _databaseId;
			}
			set
			{
				if(_databaseId != value)
					_databaseId = Math.Abs(value);
			}
		}

		public TimeSpan Timeout
		{
			get
			{
				return _timeout;
			}
			set
			{
				_timeout = value;
			}
		}

		public int PoolSize
		{
			get
			{
				return _poolSize;
			}
			set
			{
				_poolSize = Math.Max(value, 16);
			}
		}
		#endregion
	}
}
