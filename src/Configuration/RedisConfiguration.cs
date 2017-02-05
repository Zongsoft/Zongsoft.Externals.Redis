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
using System.Collections.Generic;

using Zongsoft.Options;
using Zongsoft.Options.Configuration;

namespace Zongsoft.Externals.Redis.Configuration
{
	public class RedisConfiguration : OptionConfigurationElement, IRedisConfiguration
	{
		#region 常量定义
		private const string XML_DEFAULT_ATTRIBUTE = "defaultConnection";

		private const string XML_CONNECTIONSTRING_ELEMENT = "connectionString";
		private const string XML_CONNECTIONSTRINGS_COLLECTION = "connectionStrings";
		#endregion

		#region 公共属性
		[OptionConfigurationProperty(XML_DEFAULT_ATTRIBUTE, Behavior = OptionConfigurationPropertyBehavior.IsRequired)]
		public string DefaultConnectionName
		{
			get
			{
				return (string)this[XML_DEFAULT_ATTRIBUTE];
			}
			set
			{
				if(string.IsNullOrWhiteSpace(value))
					throw new ArgumentNullException();

				this[XML_DEFAULT_ATTRIBUTE] = value;
			}
		}

		public string DefaultConnectionString
		{
			get
			{
				if(string.IsNullOrWhiteSpace(this.DefaultConnectionName))
					return null;

				return (string)this.ConnectionStrings.GetValue(this.DefaultConnectionName);
			}
		}

		[OptionConfigurationProperty(XML_CONNECTIONSTRINGS_COLLECTION, Type = typeof(SettingElementCollection), ElementName = XML_CONNECTIONSTRING_ELEMENT, Behavior = OptionConfigurationPropertyBehavior.IsRequired)]
		public ISettingsProvider ConnectionStrings
		{
			get
			{
				return (SettingElementCollection)this[XML_CONNECTIONSTRINGS_COLLECTION];
			}
		}
		#endregion
	}
}
