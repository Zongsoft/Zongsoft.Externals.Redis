﻿/*
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

namespace Zongsoft.Externals.Redis.Commands
{
	[Zongsoft.Services.CommandOption("all", Description = "${Text.DictionaryGetCommand.All}")]
	[Zongsoft.Services.CommandOption("count", Type = typeof(int), DefaultValue = 25, Description = "${Text.DictionaryGetCommand.Count}")]
	[Zongsoft.Services.CommandOption("pattern", Type = typeof(string), Description = "${Text.DictionaryGetCommand.Pattern}")]
	public class DictionaryGetCommand : RedisCommandBase
	{
		#region 构造函数
		public DictionaryGetCommand() : base("Get")
		{
		}

		public DictionaryGetCommand(IRedisService redis) : base(redis, "Get")
		{
		}
		#endregion

		#region 执行方法
		protected override object OnExecute(Services.CommandContext parameter)
		{
			if(parameter.Arguments.Length < 1)
				throw new Zongsoft.Services.CommandException("Missing arguments.");

			var dictionary = this.Redis.GetDictionary(parameter.Arguments[0]);

			switch(parameter.Arguments.Length)
			{
				case 1:
					if(parameter.Options.Contains("all"))
						return (IEnumerable<KeyValuePair<string, string>>)dictionary;
					else
						throw new Zongsoft.Services.CommandException("Missing arguments.");
				case 2:
					return dictionary[parameter.Arguments[1]];
			}

			var keys = new string[parameter.Arguments.Length - 1];
			Array.Copy(parameter.Arguments, 1, keys, 0, keys.Length);

			return dictionary.GetValues(keys);
		}
		#endregion
	}
}