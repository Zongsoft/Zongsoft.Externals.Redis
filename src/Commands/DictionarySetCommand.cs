/*
 * Authors:
 *   钟峰(Popeye Zhong) <zongsoft@gmail.com>
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

namespace Zongsoft.Externals.Redis.Commands
{
	public class DictionarySetCommand : RedisCommandBase
	{
		#region 构造函数
		public DictionarySetCommand() : base("Set")
		{
		}

		public DictionarySetCommand(IRedisService redis) : base(redis, "Set")
		{
		}
		#endregion

		#region 执行方法
		protected override object OnExecute(Services.CommandContext parameter)
		{
			if(parameter.Expression.Arguments.Length < 3)
				throw new Zongsoft.Services.CommandException("Missing arguments.");

			var dictionary = this.Redis.GetDictionary(parameter.Expression.Arguments[0]);

			if(parameter.Expression.Arguments.Length == 3)
			{
				dictionary[parameter.Expression.Arguments[1]] = parameter.Expression.Arguments[2];
				return null;
			}

			var items = new KeyValuePair<string, string>[(parameter.Expression.Arguments.Length - 1) / 2];

			for(int i = 0; i < items.Length; i++)
			{
				items[i] = new KeyValuePair<string,string>(parameter.Expression.Arguments[i * 2 + 1], parameter.Expression.Arguments[i * 2 + 2]);
			}

			dictionary.SetRange(items);

			return null;
		}
		#endregion
	}
}
