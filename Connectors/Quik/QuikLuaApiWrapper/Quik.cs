//using System;
//using System.Collections.Generic;
//using System.Diagnostics;
//using System.Linq;
//using System.Text;
//using System.Threading.Tasks;
//using BasicConcepts;
//using BasicConcepts.SecuritySpecifics;
//using BasicConcepts.SecuritySpecifics.Options;
//using Core.AppComponents.BusinessLogicConcepts;
//using Quik.EntityDataProviders;
//using Quik.Entities;
//using Quik.EntityDataProviders.QuikApiWrappers;

//namespace QuikLua
//{
//    public class QuikGate
//    {
//        private List<DerivativesTradingAccount> _accounts;

//        public static QuikGate Instance { get; } = new QuikGate();

//        public IEnumerable<ITradingAccount> Accounts => _accounts;

//        private QuikGate()
//        {
//            AccountDataProvider.Instance.ApproveChangingAccount = OnDerivativesAccountChanging;
//            AccountDataProvider.Instance.EntityChanged = OnDerivativesAccountChanged;
//            _accounts = AccountDataProvider.Instance.GetAllEntities();
//            _accounts.RemoveAll(acc => !acc.IsMoneyAccount);

//            foreach (var acc in _accounts)
//            {
//                Debug.Print("Account added: " + acc.ToString());
//            }
//        }

//        private DerivativesTradingAccount? OnDerivativesAccountChanging(bool isMoneyAcc, string clientCode, string firmId)
//        {
//            if (!isMoneyAcc)
//            {
//                return null;
//            }

//            // not using linq to avoid creating closures in hot paths
//            for (int i = 0; i < _accounts.Count; i++)
//            {
//                var acc = _accounts[i];

//                if (acc.AccountCode == clientCode && firmId == acc.FirmId)
//                {
//                    return acc;
//                }
//            }

//            if (AccountDataProvider.Instance.CreateFromCallback() is DerivativesTradingAccount newacc)
//            {
//                _accounts.Add(newacc);
//            }

//            return null;
//        }
//        private void OnDerivativesAccountChanged(DerivativesTradingAccount account)
//        {
//            Debug.Print("Account updated! " + account.ToString());
//        }
//    }
//}
