using System;
using System.Collections.Generic;

namespace Zongsoft.Externals.Redis
{
	public abstract class RedisObjectBase : MarshalByRefObject
	{
		#region 成员字段
		private string _name;
		private ServiceStack.Redis.IRedisClient _redis;
		private Zongsoft.Common.IObjectReference<ServiceStack.Redis.IRedisClient> _redisReference;
		#endregion

		#region 构造函数
		protected RedisObjectBase(string name, ServiceStack.Redis.IRedisClient redis)
		{
			if(string.IsNullOrWhiteSpace(name))
				throw new ArgumentNullException("name");

			if(redis == null)
				throw new ArgumentNullException("redis");

			_name = name.Trim();
			_redis = redis;
		}

		protected RedisObjectBase(string name, Zongsoft.Common.IObjectReference<ServiceStack.Redis.IRedisClient> redisReference)
		{
			if(string.IsNullOrWhiteSpace(name))
				throw new ArgumentNullException("name");

			if(redisReference == null)
				throw new ArgumentNullException("redisReference");

			_name = name.Trim();
			_redisReference = redisReference;
		}
		#endregion

		#region 公共属性
		/// <summary>
		/// 获取或设置队列的名称。
		/// </summary>
		public string Name
		{
			get
			{
				return _name;
			}
			set
			{
				if(string.IsNullOrWhiteSpace(value))
					throw new ArgumentNullException();

				_name = value.Trim();
			}
		}

		/// <summary>
		/// 获取或设置当前Redis对象所依附的<seealso cref="ServiceStack.Redis.IRedisClient"/>对象。
		/// </summary>
		public ServiceStack.Redis.IRedisClient Redis
		{
			get
			{
				if(_redis != null)
					return _redis;

				var result = _redisReference == null ? null : _redisReference.Target;

				if(result == null)
					throw new ObjectDisposedException("Redis");

				return result;
			}
			set
			{
				if(value == null)
					throw new ArgumentNullException();

				_redis = value;
			}
		}
		#endregion
	}
}
