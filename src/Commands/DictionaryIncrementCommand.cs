/*
 * Authors:
 *   钟峰(Popeye Zhong) <9555843@qq.com>
 *
 * Copyright (C) 2014-2017 Zongsoft Corporation <http://www.zongsoft.com>
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
	[Zongsoft.Services.CommandOption("interval", Type = typeof(int), DefaultValue = 1)]
	public class DictionaryIncrementCommand : RedisCommandBase
	{
		#region 构造函数
		public DictionaryIncrementCommand() : base("Increment")
		{
		}

		public DictionaryIncrementCommand(IRedisService redis) : base(redis, "Increment")
		{
		}
		#endregion

		#region 执行方法
		protected override object OnExecute(Services.CommandContext context)
		{
			if(context.Expression.Arguments.Length < 2)
				throw new Zongsoft.Services.CommandException("The arguments is not enough.");

			if(context.Expression.Arguments.Length % 2 != 0)
				throw new Zongsoft.Services.CommandException("The count arguments must be an even number.");

			var interval = context.Expression.Options.GetValue<int>("interval");
			var result = new List<long>(context.Expression.Arguments.Length / 2);

			for(int i = 0; i < context.Expression.Arguments.Length / 2; i++)
			{
				var dictionary = this.Redis.GetEntry<IRedisDictionary>(context.Expression.Arguments[i * 2]);

				if(dictionary == null)
				{
					context.Error.WriteLine($"The '{context.Expression.Arguments[i * 2]}' dictionary is not existed.");
					return 0;
				}

				result.Add(dictionary.Increment(context.Expression.Arguments[i * 2 + 1], interval));
				context.Output.WriteLine(result.ToString());
			}

			if(result.Count == 1)
				return result[0];
			else
				return result;
		}
		#endregion
	}
}
