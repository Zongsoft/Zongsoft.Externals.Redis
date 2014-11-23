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

using Zongsoft.Options;
using Zongsoft.Options.Configuration;

namespace Zongsoft.Externals.Redis.Configuration
{
	public class RedisConfigurationElement : OptionConfigurationElement
	{
		#region 常量定义
		private const string XML_NAME_ATTRIBUTE = "name";
		private const string XML_ADDRESS_ATTRIBUTE = "address";
		private const string XML_DB_ATTRIBUTE = "db";
		private const string XML_PASSWORD_ATTRIBUTE = "password";
		private const string XML_TIMEOUT_ATTRIBUTE = "timeout";
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

		[OptionConfigurationProperty(XML_DB_ATTRIBUTE, DefaultValue = 0)]
		public int DbIndex
		{
			get
			{
				return (int)this[XML_DB_ATTRIBUTE];
			}
			set
			{
				if(value < 0)
					throw new ArgumentOutOfRangeException();

				this[XML_DB_ATTRIBUTE] = value;
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
		#endregion
	}
}
