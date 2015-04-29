using System;
using System.Collections.Generic;

namespace Zongsoft.Externals.Redis
{
	public abstract class RedisObjectBase : MarshalByRefObject, Zongsoft.Common.IDisposableObject
	{
		#region 事件定义
		public event EventHandler<Common.DisposedEventArgs> Disposed;
		#endregion

		#region 成员字段
		private string _name;
		private Zongsoft.Collections.ObjectPool<ServiceStack.Redis.IRedisClient> _redisPool;
		#endregion

		#region 构造函数
		protected RedisObjectBase(string name, Zongsoft.Collections.ObjectPool<ServiceStack.Redis.IRedisClient> redisPool)
		{
			if(string.IsNullOrWhiteSpace(name))
				throw new ArgumentNullException("name");

			if(redisPool == null)
				throw new ArgumentNullException("redisPool");

			_name = name.Trim();
			_redisPool = redisPool;
		}
		#endregion

		#region 公共属性
		/// <summary>
		/// 获取或设置Redis对象的名称。
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
				var redisPool = _redisPool;

				if(redisPool == null)
					throw new ObjectDisposedException(this.GetType().FullName);

				return redisPool.GetObject();
			}
		}

		public bool IsDisposed
		{
			get
			{
				return _redisPool == null;
			}
		}
		#endregion

		#region 保护属性
		protected Zongsoft.Collections.ObjectPool<ServiceStack.Redis.IRedisClient> RedisPool
		{
			get
			{
				return _redisPool;
			}
		}
		#endregion

		#region 处置方法
		public void Dispose()
		{
			this.Dispose(true);
			GC.SuppressFinalize(this);

			var disposed = this.Disposed;

			if(disposed != null)
				disposed(this, new Common.DisposedEventArgs(true));
		}

		protected virtual void Dispose(bool disposing)
		{
			_redisPool = null;
		}
		#endregion
	}
}
