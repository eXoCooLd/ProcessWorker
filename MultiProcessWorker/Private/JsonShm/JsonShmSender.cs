using MultiProcessWorker.Private.GenericShm;
using System.Text;

namespace MultiProcessWorker.Private.JsonShm
{
    internal class JsonShmSender<T> : ShmSender<T>
    {
        public JsonShmSender(string shmName) : base(shmName)
        {
        }

        protected override byte[] Serialize(T data)
        {
            var dataString = Newtonsoft.Json.JsonConvert.SerializeObject(data);
            return Encoding.UTF8.GetBytes(dataString);
        }
    }
}