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
using System.Collections;
using System.Collections.Generic;

namespace Zongsoft.Externals.Redis.Commands
{
	public class GetCommand : RedisCommandBase
	{
		#region 构造函数
		public GetCommand() : base("Get")
		{
		}

		public GetCommand(IRedisService redis) : base(redis, "Get")
		{
		}
		#endregion

		#region 执行方法
		protected override object OnExecute(Services.CommandContext context)
		{
			if(context.Expression.Arguments.Length < 1)
				throw new Zongsoft.Services.CommandException("Missing arguments.");

			int index = 0;
			var result = new List<object>(context.Expression.Arguments.Length);

			for(int i = 0; i < context.Expression.Arguments.Length; i++)
			{
				var entry = this.Redis.GetEntry(context.Expression.Arguments[i]);

				if(entry == null)
				{
					context.Output.WriteLine(Services.CommandOutletColor.Red, $"The '{context.Expression.Arguments[i]}' entry is not existed.");
				}
				else
				{
					result.Add(entry);

					var entryType = this.Redis.GetEntryType(context.Expression.Arguments[i]);
					context.Output.Write(Services.CommandOutletColor.DarkGray, $"[{entryType}] ");

					var expiry = this.Redis.GetEntryExpiry(context.Expression.Arguments[i]);
					if(expiry.HasValue)
						context.Output.Write(Services.CommandOutletColor.DarkCyan, expiry.Value.ToString() + " ");

					switch(entryType)
					{
						case RedisEntryType.String:
							context.Output.WriteLine(entry);
							break;
						case RedisEntryType.Dictionary:
							context.Output.WriteLine(Services.CommandOutletColor.DarkYellow, $"The '{context.Expression.Arguments[i]}' dictionary have {((IRedisDictionary)entry).Count} entries.");

							foreach(DictionaryEntry item in (IDictionary)entry)
							{
								context.Output.Write(Services.CommandOutletColor.Gray, $"[{(++index).ToString()}] ");
								context.Output.Write(Services.CommandOutletColor.DarkGreen, item.Key.ToString());
								context.Output.Write(Services.CommandOutletColor.Cyan, " : ");
								context.Output.WriteLine(Services.CommandOutletColor.DarkGreen, item.Value.ToString());
							}

							break;
						case RedisEntryType.List:
							context.Output.WriteLine(Services.CommandOutletColor.DarkYellow, $"The '{context.Expression.Arguments[i]}' list(queue) have {((IRedisQueue)entry).Count} entries.");

							foreach(object item in (IEnumerable)entry)
							{
								context.Output.Write(Services.CommandOutletColor.Gray, $"[{(++index).ToString()}] ");

								if(item == null)
									context.Output.WriteLine(Services.CommandOutletColor.DarkGray, "NULL");
								else
									context.Output.WriteLine(Services.CommandOutletColor.DarkGreen, item.ToString());
							}

							break;
						case RedisEntryType.Set:
						case RedisEntryType.SortedSet:
							context.Output.WriteLine(Services.CommandOutletColor.DarkYellow, $"The '{context.Expression.Arguments[i]}' hashset have {((IRedisHashset)entry).Count} entries.");

							foreach(object item in (IEnumerable)entry)
							{
								context.Output.Write(Services.CommandOutletColor.Gray, $"[{(++index).ToString()}] ");

								if(item == null)
									context.Output.WriteLine(Services.CommandOutletColor.DarkGray, "NULL");
								else
									context.Output.WriteLine(Services.CommandOutletColor.DarkGreen, item.ToString());
							}

							break;
						default:
							context.Output.WriteLine();
							break;
					}
				}
			}

			if(result.Count == 1)
				return result[0];
			else
				return result;
		}
		#endregion
	}
}
