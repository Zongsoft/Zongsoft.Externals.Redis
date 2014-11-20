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
	[Zongsoft.Services.CommandOption("direction", Type = typeof(ListCommandDirection), DefaultValue = ListCommandDirection.Tail, Description = "${Text.ListCommand.Direction}")]
	public class ListPushCommand : RedisCommandBase
	{
		#region 构造函数
		public ListPushCommand() : base("Push")
		{
		}

		public ListPushCommand(ServiceStack.Redis.IRedisClient redis) : base(redis, "Push")
		{
		}
		#endregion

		#region 执行方法
		protected override void Run(Services.CommandContext parameter)
		{
			if(parameter.Arguments.Length < 2)
				throw new Services.CommandException("Missing arguments.");

			var direction = (ListCommandDirection)parameter.Options["direction"];

			if(direction == ListCommandDirection.Head)
				this.Invoke(parameter.Arguments,
					(name, value) => this.Redis.PrependItemToList(name, value),
					(name, values) => this.Redis.PrependRangeToList(name, System.Linq.Enumerable.ToList(values)));
			else
				this.Invoke(parameter.Arguments,
					(name, value) => this.Redis.AddItemToList(name, value),
					(name, values) => this.Redis.AddRangeToList(name, System.Linq.Enumerable.ToList(values)));
		}
		#endregion

		#region 私有方法
		private void Invoke(string[] args, Action<string, string> addItem, Action<string, IEnumerable<string>> addRange)
		{
			if(args.Length == 2)
			{
				addItem(args[0], args[1]);
			}
			else
			{
				var values = new string[args.Length - 1];
				args.CopyTo(values, 1);

				addRange(args[0], values);
			}
		}
		#endregion
	}
}
