#region Copyright
// --------------------------------------------------------------------------------------------------------------------
// MIT License
// Copyright(c) 2018 Andre Wehrli

// Permission is hereby granted, free of charge, to any person obtaining a copy
// of this software and associated documentation files (the "Software"), to deal
// in the Software without restriction, including without limitation the rights
// to use, copy, modify, merge, publish, distribute, sublicense, and/or sell
// copies of the Software, and to permit persons to whom the Software is
// furnished to do so, subject to the following conditions:

// The above copyright notice and this permission notice shall be included in all
// copies or substantial portions of the Software.

// THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR
// IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY,
// FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE
// AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER
// LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM,
// OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE
// SOFTWARE.
// --------------------------------------------------------------------------------------------------------------------
#endregion Copyright

#region Used Namespaces
using MultiProcessWorker.Private.GenericShm;
using System.Text;
#endregion Used Namespaces

namespace MultiProcessWorker.Private.JsonShm
{
    /// <summary>
    /// Shared Memory Sender with JSON Serializer
    /// </summary>
    /// <typeparam name="T"></typeparam>
    internal class JsonShmSender<T> : ShmSender<T>
    {
        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="shmName"></param>
        public JsonShmSender(string shmName) : base(shmName)
        {
        }

        /// <summary>
        /// JSON Serializer
        /// </summary>
        /// <param name="data"></param>
        /// <returns></returns>
        protected override byte[] Serialize(T data)
        {
            var dataString = Newtonsoft.Json.JsonConvert.SerializeObject(data);
            return Encoding.UTF8.GetBytes(dataString);
        }
    }
}