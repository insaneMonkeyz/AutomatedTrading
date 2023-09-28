using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using AppComponents.Messaging.Results;
using Quik;
using Tools;

namespace MarketExecutionService
{
    internal partial class MarketExecutionService : IDisposable
    {
        private IQuik _quik;

        public MarketExecutionService()
        {
            _quik = DI.Resolve<IQuik>() ?? throw new Exception("Cannot resolve the instance of Quik");
        }

        #region IDisposable
        private bool disposed;

        public void Dispose()
        {
            if (!disposed)
            {
                disposed = true;
                _quik = null;
            }
        }
        #endregion
    }
}
