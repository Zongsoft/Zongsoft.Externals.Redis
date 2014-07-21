using System;
using System.Collections.Generic;

namespace Zongsoft.Externals.Redis.Commands
{
	public class RedisCommandLoader : Zongsoft.Services.CommandLoaderBase
	{
		#region 成员字段
		private ServiceStack.Redis.IRedisClient _redis;
		#endregion

		#region 公共属性
		public ServiceStack.Redis.IRedisClient Redis
		{
			get
			{
				return _redis;
			}
			set
			{
				if(value == null)
					throw new ArgumentNullException();

				_redis = value;
			}
		}
		#endregion

		#region 加载方法
		protected override bool OnLoad(Services.CommandTreeNode node)
		{
			node.Children.Add("List");
			node.Children.Add("Hashset");
			node.Children.Add("Dictionary");

			node.Children.Add(new GetCommand(_redis));
			node.Children.Add(new SetCommand(_redis));

			//返回加载成功
			return true;
		}
		#endregion
	}
}
