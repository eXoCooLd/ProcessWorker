# ProcessWorker

Prototype of a ProcessWorker that starts jobs in a new process and returns there return-value

For example a public static method:

```
public class ProcessWorkerExample
{
	public void StartRemoteWork()
	{
		string result = ProcessWorker.RunAndWait(RemoteWorkJob);
	}
	
	public static string RemoteWorkJob()
	{
		return "Remote Job Done"
	}
}
```