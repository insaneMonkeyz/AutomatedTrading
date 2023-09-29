using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AppComponents
{
    public interface IService
    {
        Guid HostId { get; }
        Guid Id { get; }
        string Name { get; }
        object Status { get; }

        object GetConfiguration();
        void Configure(object parameters);
        void Initialize(object parameters);
        void Shutdown(object parameters);
    }
}
