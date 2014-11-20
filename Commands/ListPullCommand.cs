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

namespace Zongsoft.Externals.Redis.Commands
{
	[Zongsoft.Services.CommandOption("direction", Type = typeof(ListCommandDirection), DefaultValue = ListCommandDirection.Head, Description = "${Text.ListCommand.Direction}")]
	public class ListPullCommand : RedisCommandBase
	{
		#region 构造函数
		public ListPullCommand() : base("Pull")
		{
		}

		public ListPullCommand(ServiceStack.Redis.IRedisClient redis) : base(redis, "Pull")
		{
		}
		#endregion

		#region 执行方法
		protected override object OnExecute(Services.CommandContext parameter)
		{
			if(parameter.Arguments.Length < 1)
				throw new Services.CommandException("Missing arguments.");

			var direction = (ListCommandDirection)parameter.Options["direction"];

			if(direction == ListCommandDirection.Head)
				return this.Invoke(parameter.Arguments, name => this.Redis.RemoveStartFromList(name));
			else
				return this.Invoke(parameter.Arguments, name => this.Redis.RemoveEndFromList(name));
		}
		#endregion

		#region 私有方法
		private object Invoke(string[] args, Func<string, string> remove)
		{
			if(args.Length == 1)
				return remove(args[0]);

			var result = new List<string>(args.Length);

			foreach(var arg in args)
			{
				result.Add(remove(arg));
			}

			return result.ToArray();
		}
		#endregion
	}
}
