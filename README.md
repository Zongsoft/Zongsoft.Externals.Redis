Zongsoft.Externals.Redis
========================

关于 Redis 操作的常用命令集。


## 使用方法

```
	public class Example
	{
		public Zongsoft.Services.CommandExecutorBase Executor
		{
			get;
			set;
		}

		public StorageFile GetFile(Guid fileId)
		{
			//获取通过属性注入的命令执行器
			var executor = this.Executor;

			if(executor == null)
				throw new InvalidOperation("Invalid Command-Executor.");

			//通过 Redis 命令集中的 DictionaryGetCommand 命令来获取指定文件编号(fileId)对应的实体成员字典
			var dictionary = executor.Execute(string.Format("/redis.dictionary.get -all 'storages.file:{0:n}'", fileId)) as IDictionary<string, string>;

			//将获取的字典转换成指定类型的实体对象
			return Utility.ToObject<StorageFile>(dictionary);
		}
	}
```
