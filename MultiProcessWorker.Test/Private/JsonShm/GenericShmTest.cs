using MultiProcessWorker.Private.JsonShm;
using NUnit.Framework;

namespace MultiProcessWorker.Test.Private.GenericShm
{
    [TestFixture]
    public class GenericShmTest
    {
        [Test]
        public void ShmSendReciveTest()
        {
            const string testData = "The Test Data";

            const string shmName = "TestShm";
            using (var shmSender = new JsonShmSender<string>(shmName))
            {
                using (var shmReceiver = new JsonShmReceiver<string>(shmName))
                {
                    var send = shmSender.SaveToSharedMemory(testData);
                    Assert.IsTrue(send);

                    var recived = shmReceiver.LoadFromSharedMemory();
                    Assert.AreEqual(testData, recived);
                }
            }
        }
    }
}
