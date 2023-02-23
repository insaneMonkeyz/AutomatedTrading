using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Quik.EntityProviders.QuikApiWrappers;
using Quik.Lua;

namespace Quik.EntityDataProviders.QuikApiWrappers
{
    internal class TransactionWrapper
    {
        public const string NAME = "trans_reply";
        public const string CALLBACK_METHOD = "OnTransReply";

        #region General
        private const string ID = "trans_id";
        private const string UNIQUE_ID = "uid";
        private const string SERVER_TRANSACTION_ID = "server_trans_id";
        private const string STATUS = "status";
        private const string RESULT_DESCRIPTION = "result_msg";
        private const string TIMESTAMP = "date_time";
        private const string FLAGS = "flags";
        private const string ERROR_CODE = "error_code";
        private const string ERROR_SITE = "error_source";
        private const string RESPONSE_RECEPTION_TIMESTAMP = "gate_reply_time";
        #endregion

        #region Optional
        //NULLABLE
        private const string ORDER_ID = "order_num";
        private const string PRICE = "price";
        private const string SIZE = "quantity";
        private const string REST = "balance";
        private const string FIRM_ID = "firm_id";
        private const string ACCOUNT = "account";
        private const string CLIENT_CODE = "client_code";
        private const string CLASS_CODE = "class_code";
        private const string TICKER = "sec_code";
        private const string EXCHANGE_ORDER_ID = "exchange_code";
        #endregion

        public enum TransactionStatus : long
        {
            SentFromQuikToServer = 0,
            ReceivedByServer = 1,
            ExchangeUnavailable = 2,
            Completed = 3,
            RejectedByQuik = 4,
            RejectedByQuikServer = 5,
            NotEnoughMoney = 6,
            NotSupportedByQiuk = 10,
            InvalidDigitalSignature = 11,
            TimeoutExpired = 12,
            CrossExecution = 13,
            RejectedByBroker = 14,
            AcceptedAsException = 15,
            UserRefusedToContinue = 16
        }

        public enum ErrorSite : long
        {
            None = 0,
            Quik = 1,
            QuikServer = 2,
            LimitsSupervisor = 3,
            Exchange = 4
        }

        private static LuaWrap _context;

        public static void SetContext(LuaWrap lua)
        {
            _context = lua;
        }

        public static long Id
        {
            get => _context.ReadRowValueLong(ID);
        }
        public static ErrorSite ErrorSource
        {
            get => (ErrorSite)_context.ReadRowValueLong(ERROR_SITE);
        }
        public static TransactionStatus Status
        {
            get => (TransactionStatus)_context.ReadRowValueLong(STATUS);
        }
        public static string? ResultDescription
        {
            get => _context.ReadRowValueString(RESULT_DESCRIPTION);
        }
        public static DateTimeOffset Timestamp
        {
            get => TimeWrapper.GetTime(_context, TIMESTAMP).GetValueOrDefault();
        }
        public static DateTimeOffset ResponseReceptionTimestamp
        {
            get => TimeWrapper.GetTime(_context, RESPONSE_RECEPTION_TIMESTAMP).GetValueOrDefault();
        }
        public static long? ExchangeAssignedOrderId
        {
            get => _context.TryFetchLongFromTable(EXCHANGE_ORDER_ID, out long result) ? result : null;
        }
    }
}
