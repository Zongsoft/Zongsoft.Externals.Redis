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
	public class DictionaryIncrementCommand : RedisCommandBase
	{
		#region 构造函数
		public DictionaryIncrementCommand() : base("Increment")
		{
		}

		public DictionaryIncrementCommand(ServiceStack.Redis.IRedisClient redis) : base(redis, "Increment")
		{
		}
		#endregion

		#region 执行方法
		protected override object OnExecute(Services.CommandContext parameter)
		{
			if(parameter.Arguments.Length < 2)
				return 0L;

			int interval = 1;
			double? intervalDouble = null;

			if(parameter.Arguments.Length > 2)
			{
				if(!int.TryParse(parameter.Arguments[2], out interval))
					intervalDouble = double.Parse(parameter.Arguments[2]);
			}

			if(intervalDouble.HasValue)
				return this.Redis.IncrementValueInHash(parameter.Arguments[0], parameter.Arguments[1], intervalDouble.Value);
			else
				return this.Redis.IncrementValueInHash(parameter.Arguments[0], parameter.Arguments[1], interval);
		}
		#endregion
	}
}
