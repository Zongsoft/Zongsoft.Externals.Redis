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

		public DictionaryGetCommand(ServiceStack.Redis.IRedisClient redis) : base(redis, "Get")
		{
		}
		#endregion

		#region 执行方法
		protected override object OnExecute(Services.CommandContext parameter)
		{
			if(parameter.Arguments.Length < 1)
				return null;

			if(parameter.Options.Contains("all"))
				return this.Redis.GetAllEntriesFromHash(parameter.Arguments[0]);

			if(parameter.Arguments.Length == 1)
				return this.Redis.ScanAllHashEntries(parameter.Arguments[0], (string)parameter.Options["pattern"], (int)parameter.Options["count"]);

			var keys = new string[parameter.Arguments.Length - 1];
			Array.Copy(parameter.Arguments, 1, keys, 0, keys.Length);

			return this.Redis.GetValuesFromHash(parameter.Arguments[0], keys);
		}
		#endregion
	}
}
