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
	public class HashsetSetCommand : RedisCommandBase
	{
		#region 构造函数
		public HashsetSetCommand() : base("Set")
		{
		}

		public HashsetSetCommand(ServiceStack.Redis.IRedisClient redis) : base(redis, "Set")
		{
		}
		#endregion

		#region 执行方法
		protected override void Run(Services.CommandContext context)
		{
			if(context.Arguments.Length < 2)
				throw new Services.CommandException("Missing arguments.");

			if(context.Arguments.Length == 2)
			{
				this.Redis.AddItemToSet(context.Arguments[0], context.Arguments[1]);
				return;
			}

			var values = new string[context.Arguments.Length - 1];
			Array.Copy(context.Arguments, 1, values, 0, values.Length);

			this.Redis.AddRangeToSet(context.Arguments[0], new List<string>(values));
		}
		#endregion
	}
}
