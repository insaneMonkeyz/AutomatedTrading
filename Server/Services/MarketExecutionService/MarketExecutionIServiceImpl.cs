using AppComponents;
using Broker;
using Quik;
using Tools;
using TradingConcepts;
using TradingConcepts.CommonImplementations;

namespace MarketExecutionService
{
    internal partial class MarketExecutionService : IService
    {
        public Guid HostId { get; } = DI.Resolve<IBroker>().HostId;
        public Guid Id { get; } = new("D7A8C1CA-9CCA-46D4-A4C0-E7AEE87B50E7");
        public string Name { get; } = nameof(MarketExecutionService);
        public ServiceStatus Status { get; private set; }

        public void Initialize(object parameters)
        {
            if (_initialized)
            {
                throw new Exception($"Service {Name} is already initialized");
            }

            // since the application is currently a single-host
            // there is simply no one whom to forward the requests
            var isExecutive = true;

            _quik = DI.Resolve<IQuik>() ?? throw new Exception("Cannot resolve the instance of Quik");

            if (isExecutive)
            {
                DI.Resolve<IBroker>().RegisterService(this);
                DI.RegisterInstance(this);
            }
            else
            {
                // implement a service shell and use it as a facade to address all requests to the broker
                // that will route them to the executive service located on another host

                throw new NotImplementedException("Proxying is not supported yet");
                
                var shell = new object();
                DI.RegisterInstance(shell);
            }

            _initialized = true;
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

        private IQuik? _quik;
        private bool _initialized;
    }
}