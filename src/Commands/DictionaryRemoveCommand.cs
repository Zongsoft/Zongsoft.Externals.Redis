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
	public class DictionaryRemoveCommand : RedisCommandBase
	{
		#region 构造函数
		public DictionaryRemoveCommand() : base("Remove")
		{
		}

		public DictionaryRemoveCommand(IRedisService redis) : base(redis, "Remove")
		{
		}
		#endregion

		#region 执行方法
		protected override object OnExecute(Services.CommandContext context)
		{
			if(context.Expression.Arguments.Length < 2)
				throw new Zongsoft.Services.CommandException("Missing arguments.");

			var dictionary = this.Redis.GetEntry<IRedisDictionary>(context.Expression.Arguments[0]);

			if(dictionary == null)
			{
				context.Error.WriteLine($"The '{context.Expression.Arguments[0]}' dictionary is not existed.");
				return 0;
			}

			int count = 0;

			for(int i = 1; i < context.Expression.Arguments.Length; i++)
			{
				count += dictionary.Remove(context.Expression.Arguments[i]) ? 1 : 0;
			}

			return count;
		}
		#endregion
	}
}
