using MultiProcessWorker.Private.GenericShm;
using System.Text;

namespace MultiProcessWorker.Private.JsonShm
{
    internal class JsonShmReceiver<T> : ShmReceiver<T>
    {
        public JsonShmReceiver(string shmName) : base(shmName)
        {
        }

        protected override T Deserialize(byte[] data)
        {
            var text = Encoding.UTF8.GetString(data).Trim(NullChar);
            return Newtonsoft.Json.JsonConvert.DeserializeObject<T>(text);
        }
    }
}