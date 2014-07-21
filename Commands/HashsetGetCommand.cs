using System;
using System.Collections.Generic;

namespace Zongsoft.Externals.Redis.Commands
{
	[Zongsoft.Services.CommandOption("all", Description = "${Text.MapGetCommand.All}")]
	[Zongsoft.Services.CommandOption("count", Type = typeof(int), DefaultValue = 25, Description = "${Text.MapGetCommand.Count}")]
	[Zongsoft.Services.CommandOption("pattern", Type = typeof(string), Description = "${Text.MapGetCommand.Pattern}")]
	public class HashsetGetCommand : RedisCommandBase
	{
		#region 构造函数
		public HashsetGetCommand() : base("Get")
		{
		}

		public HashsetGetCommand(ServiceStack.Redis.IRedisClient redis) : base(redis, "Get")
		{
		}
		#endregion

		#region 执行方法
		protected override object OnExecute(Services.CommandContext parameter)
		{
			if(parameter.Arguments.Length < 1)
				return null;

			if(parameter.Options.Contains("all"))
				return this.Redis.GetAllItemsFromSet(parameter.Arguments[0]);

			return this.Redis.ScanAllSetItems(parameter.Arguments[0], (string)parameter.Options["pattern"], (int)parameter.Options["count"]);
		}
		#endregion
	}
}
