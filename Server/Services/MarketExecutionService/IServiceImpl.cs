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
        public Guid HostId
        {
            get => throw new NotImplementedException();
            set => throw new NotImplementedException();
        }
        public Guid Id { get; } = new("D7A8C1CA-9CCA-46D4-A4C0-E7AEE87B50E7");
        public string Name { get; } = nameof(MarketExecutionService);
        public object Status => throw new NotImplementedException();

        public void Initialize(object parameters)
        {
            // since the application is currently a single-host
            // there is simply no one whom to forward the requests
            var isExecutive = true;

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