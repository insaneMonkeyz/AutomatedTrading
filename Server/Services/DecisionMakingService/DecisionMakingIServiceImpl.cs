using AppComponents;
using Broker;
using Tools;

namespace DecisionMakingService
{
    internal partial class DecisionMakingService : IService
    {
        public Guid HostId { get; } = DI.Resolve<IBroker>().HostId;
        public Guid Id { get; } = new("DF28429C-578D-471A-B292-1D2706DE5618");
        public string Name { get; } = nameof(DecisionMakingService);
        public ServiceStatus Status { get; private set; }

        public void Initialize(object parameters)
        {
            throw new NotImplementedException();
        }

        public void Shutdown(object parameters)
        {
            throw new NotImplementedException();
        }

        public void Configure(object parameters)
        {
            throw new NotImplementedException();
        }

        public object GetConfiguration()
        {
            throw new NotImplementedException();
        }
    }
}