using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Unity;

namespace Core
{
    public class Core
    {
        private const string CONFIG_PATH = "AutomatedTrading.cfg";

        private Core()
        {
            var uc = new UnityContainer(); 
            uc.RegisterSingleton<Core>();
            uc.Resolve<Core>();
        }

        public static void Initialize()
        {
            // init message and command brokers
            // init services
        }

    }
}
