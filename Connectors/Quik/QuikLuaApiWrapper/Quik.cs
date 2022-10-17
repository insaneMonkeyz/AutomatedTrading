using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using BasicConcepts;
using Core.AppComponents.BusinessLogicConcepts;
using QuikLuaApi.Entities;

namespace QuikLuaApiWrapper
{
    public class Quik : IMarketDataProvider
    {
        public void Subscribe<T>(ISecurity subject, Action<T> newDataHandler, Predicate<T> filter = null)
        {
            throw new NotImplementedException();
        }

        public void Unsubscribe<T>(ISecurity subject)
        {
            throw new NotImplementedException();
        }

        public static IOptimizedOrderBook GetOrderbook(ISecurity security)
        {
            return new OrderBook()
            {
                Security = security
            };
        }
    }
}
