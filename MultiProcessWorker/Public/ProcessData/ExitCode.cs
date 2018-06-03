namespace MultiProcessWorker.Public.ProcessData
{
    public enum ExitCode
    {
        Undefined = 0,
        ErrorCrash = -1,
        ErrorParentCrash = -2,
        Ok = 1
    }
}