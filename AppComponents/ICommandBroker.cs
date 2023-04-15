using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AppComponents.AppStructureConcepts
{
    public interface ICommandBroker
    {
        void RegisterExecutiveService(object service);
        void SendCommand(object command);
    }
}
