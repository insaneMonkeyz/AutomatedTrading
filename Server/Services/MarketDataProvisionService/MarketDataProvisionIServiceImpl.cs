using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using AppComponents;
using Broker;
using Quik;
using Tools;

namespace MarketDataProvisionService
{
    internal partial class MarketDataProvisionService : IService
    {
        public Guid HostId { get; } = DI.Resolve<IBroker>().HostId;
        public Guid Id { get; } = new("81AE6806-7BD7-41B9-AE0E-4D1F431D7EEA");
        public string Name { get; } = nameof(MarketDataProvisionService);
        public ServiceStatus Status { get; private set; }

        [ServiceInitializer]
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
                DI.RegisterInstance(this as IMarketDataProvisionService);
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

        private bool _initialized;
        private IQuik? _quik;
    }
}
