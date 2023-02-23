using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Quik.Entities;
using Quik.EntityDataProviders.QuikApiWrappers;
using Quik.EntityProviders;

namespace Quik.EntityDataProviders
{
    internal class TransactionsProvider
    {
        public void Submit(Order order)
        {
            TransactionBuilder.MakeNewOrder()
        }
    }
}
