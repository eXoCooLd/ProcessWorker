# ProcessWorker

Prototype of a ProcessWorker that starts jobs in a new process and returns there return-values back to the main process

## Usage for a public static method:

```C#
public class ProcessWorkerExample
{
	public void StartRemoteWork()
	{
		string result = ProcessWorker.RunAndWait(RemoteWorkJob);
	}
	
	public static string RemoteWorkJob()
	{
		return "Remote Job Done";
	}
}
```

## License
[MIT](LICENSE)