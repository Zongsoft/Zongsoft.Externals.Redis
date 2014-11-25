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
	public class DictionaryCountCommand : RedisCommandBase
	{
		#region 构造函数
		public DictionaryCountCommand() : base("Count")
		{
		}

		public DictionaryCountCommand(IRedisService redis) : base(redis, "Count")
		{
		}
		#endregion

		#region 执行方法
		protected override object OnExecute(Services.CommandContext parameter)
		{
			if(parameter.Arguments.Length < 1)
				throw new Services.CommandException("Missing arguments.");

			if(parameter.Arguments.Length == 1)
				return this.Redis.GetDictionary(parameter.Arguments[0]).Count;

			var result = new long[parameter.Arguments.Length];

			for(int i = 0; i < result.Length; i++)
			{
				result[i] = this.Redis.GetDictionary(parameter.Arguments[i]).Count;
			}

			return result;
		}
		#endregion
	}
}
