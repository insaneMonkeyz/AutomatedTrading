using AppComponents;
using AppComponents.Delegates;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Tools;
using Tools.Logging;
using Tools.Serialization;

namespace Broker
{
    internal class Broker : IBroker
    {
        public Guid HostId { get; private set; }

        public void Publish<T>(string feed, T data)
        {
            throw new NotImplementedException();
        }
        public void RegisterService(IService service)
        {
            throw new NotImplementedException();
        }
        public void SubscribeFeed<T>(string feed, FeedSubscriber<T> receiver)
        {
            throw new NotImplementedException();
        }
        public void UnregisterService(IService service)
        {
            throw new NotImplementedException();
        }
        public void UnsubscribeFeed<T>(string feed, FeedSubscriber<T> receiver)
        {
            throw new NotImplementedException();
        }

        [ServiceInitializer]
        public void Initialize(object? arg)
        {
            var cfg = (arg as JObject).SafeExtractObject<BrokerConfiguration>()
                ?? BrokerConfiguration.CreateDefault();

            HostId = cfg.HostId;

            DI.RegisterInstance<IBroker>(this);
        }

        private static Log _log = LogManagement.GetLogger<Broker>();
    }
}