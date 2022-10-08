using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Core
{
    public class AutomatedTrading
    {
        private AutomatedTrading()
        {

        }

        #region Singleton
        private static AutomatedTrading Core { get; } = new AutomatedTrading();
        public static AutomatedTrading Create() => Core;
        #endregion

        private const string CONFIG_PATH = "AutomatedTrading.cfg";
    }
}
