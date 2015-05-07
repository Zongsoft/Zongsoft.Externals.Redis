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

using Zongsoft.Options;
using Zongsoft.Options.Configuration;

namespace Zongsoft.Externals.Redis.Configuration
{
	public class RedisConfigurationElement : OptionConfigurationElement
	{
		#region 常量定义
		private const string XML_NAME_ATTRIBUTE = "name";
		private const string XML_ADDRESS_ATTRIBUTE = "address";
		private const string XML_DATABASEID_ATTRIBUTE = "databaseId";
		private const string XML_PASSWORD_ATTRIBUTE = "password";
		private const string XML_TIMEOUT_ATTRIBUTE = "timeout";
		private const string XML_POOLSIZE_ATTRIBUTE = "poolSize";
		#endregion

		#region 公共属性
		[OptionConfigurationProperty(XML_NAME_ATTRIBUTE, Behavior = OptionConfigurationPropertyBehavior.IsKey)]
		public string Name
		{
			get
			{
				return (string)this[XML_NAME_ATTRIBUTE];
			}
			set
			{
				if(string.IsNullOrWhiteSpace(value))
					throw new ArgumentNullException();

				this[XML_NAME_ATTRIBUTE] = value;
			}
		}

		[OptionConfigurationProperty(XML_ADDRESS_ATTRIBUTE, DefaultValue = "127.0.0.1", Behavior = OptionConfigurationPropertyBehavior.IsRequired)]
		public string Address
		{
			get
			{
				return (string)this[XML_ADDRESS_ATTRIBUTE];
			}
			set
			{
				if(string.IsNullOrWhiteSpace(value))
					throw new ArgumentNullException();

				this[XML_ADDRESS_ATTRIBUTE] = value;
			}
		}

		[OptionConfigurationProperty(XML_DATABASEID_ATTRIBUTE, DefaultValue = 0)]
		public int DatabaseId
		{
			get
			{
				return (int)this[XML_DATABASEID_ATTRIBUTE];
			}
			set
			{
				if(value < 0)
					throw new ArgumentOutOfRangeException();

				this[XML_DATABASEID_ATTRIBUTE] = value;
			}
		}

		[OptionConfigurationProperty(XML_PASSWORD_ATTRIBUTE)]
		public string Password
		{
			get
			{
				return (string)this[XML_PASSWORD_ATTRIBUTE];
			}
			set
			{
				this[XML_PASSWORD_ATTRIBUTE] = value;
			}
		}

		[OptionConfigurationProperty(XML_TIMEOUT_ATTRIBUTE, DefaultValue = "0:0:30")]
		public TimeSpan Timeout
		{
			get
			{
				return (TimeSpan)this[XML_TIMEOUT_ATTRIBUTE];
			}
			set
			{
				this[XML_TIMEOUT_ATTRIBUTE] = value;
			}
		}

		[OptionConfigurationProperty(XML_TIMEOUT_ATTRIBUTE, DefaultValue = 64)]
		public int PoolSize
		{
			get
			{
				return (int)this[XML_POOLSIZE_ATTRIBUTE];
			}
			set
			{
				this[XML_POOLSIZE_ATTRIBUTE] = Math.Max(value, 16);
			}
		}
		#endregion
	}
}
