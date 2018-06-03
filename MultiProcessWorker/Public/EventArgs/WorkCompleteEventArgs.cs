
namespace MultiProcessWorker.Public.EventArgs
{
    public class WorkCompleteEventArgs : System.EventArgs
    {
        public System.Guid WorkId { get; }

        internal WorkCompleteEventArgs(System.Guid workId)
        {
            WorkId = workId;
        }
    }
}
