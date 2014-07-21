using System;

namespace Zongsoft.Externals.Redis.Commands
{
	public abstract class RedisCommandBase : Zongsoft.Services.CommandBase<Zongsoft.Services.CommandContext>
	{
		#region 成员字段
		private ServiceStack.Redis.IRedisClient _redis;
		#endregion

		#region 构造函数
		protected RedisCommandBase()
		{
		}

		protected RedisCommandBase(string name) : base(name)
		{
		}

		protected RedisCommandBase(ServiceStack.Redis.IRedisClient redis)
		{
			_redis = redis;
		}

		protected RedisCommandBase(ServiceStack.Redis.IRedisClient redis, string name) : base(name)
		{
			_redis = redis;
		}
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

		#region 重写方法
		public override bool CanExecute(Services.CommandContext parameter)
		{
			return _redis != null && base.CanExecute(parameter);
		}
		#endregion
	}
}
