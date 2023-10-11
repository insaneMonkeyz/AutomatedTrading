using AppComponents;
using AppComponents.Delegates;

namespace Broker
{
    public interface IBroker
    {
        Guid HostId { get; }

        void RegisterService(IService service);
        void UnregisterService(IService service);
        void SubscribeFeed<T>(string feed, FeedSubscriber<T> receiver);
        void UnsubscribeFeed<T>(string feed, FeedSubscriber<T> receiver);
        void Publish<T>(string feed, T data);
    }
}