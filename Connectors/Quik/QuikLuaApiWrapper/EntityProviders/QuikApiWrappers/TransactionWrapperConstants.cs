using System.Text;

namespace Quik.EntityProviders.QuikApiWrappers
{
    internal partial class TransactionWrapper
    {
        public const string NAME = "trans_reply";
        public const string CALLBACK_METHOD = "OnTransReply";
        public const string SEND_TRANSACTION_METHOD = "sendTransaction";

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
        private const string EXCHANGE_CODE = "exchange_code";
        #endregion

        //TRANSACTION
        private const string TRANSACTION_ID_PARAM = "TRANS_ID";
        private const string CLASS_CODE_PARAM = "CLASSCODE";
        private const string TICKER_PARAM = "SECCODE";
        private const string ACCOUNT_PARAM = "ACCOUNT";
        private const string CLIENT_CODE_PARAM = "CLIENT_CODE";
        private const string ACTION_PARAM = "ACTION";
        private const string OPERATION_PARAM = "OPERATION";
        private const string ORDER_TYPE_PARAM = "TYPE";
        private const string EXECUTION_CONDITION_PARAM = "EXECUTION_CONDITION";
        private const string SIZE_PARAM = "QUANTITY";
        private const string MODE_PARAM = "MODE";
        private const string PRICE_PARAM = "PRICE";
        private const string COMMENT_PARAM = "COMMENT";
        private const string ORDER_ID_PARAM = "ORDER_KEY";
        private const string CHECK_LIMITS_PARAM = "CHECK_LIMITS";
        private const string BASE_CONTRACT_PARAM = "BASE_CONTRACT";
        private const string KILL_ACTIVE_ORDERS_PARAM = "KILL_ACTIVE_ORDERS";

        private const string MOVE_ORDER_ID_PARAM = "FIRST_ORDER_NUMBER";
        private const string MOVE_ORDER_SIZE_PARAM = "FIRST_ORDER_NEW_QUANTITY";
        private const string MOVE_ORDER_PRICE_PARAM = "FIRST_ORDER_NEW_PRICE";
        private const string MOVE_ORDER_NEW_ID_PARAM = "SECOND_ORDER_NUMBER";
        private const string MOVE_ORDER_NEW_SIZE_PARAM = "SECOND_ORDER_NEW_QUANTITY";
        private const string MOVE_ORDER_NEW_PRICE_PARAM = "SECOND_ORDER_NEW_PRICE";

        // STOP ORDER PARAMS
        private const string STOP_ORDER_ID_PARAM = "STOP_ORDER_KEY";
        private const string STOP_PRICE_PARAM = "STOPPRICE";
        private const string STOP_ORDER_KIND_PARAM = "STOP_ORDER_KIND";
        private const string TRIGGER_SECURITY_CLASS_CODE_PARAM = "STOPPRICE_CLASSCODE";
        private const string TRIGGER_SECURITY_TICKER_PARAM = "STOPPRICE_SECCODE";
        private const string TRIGGER_SECURITY_PRICE_CONDITION_PARAM = "STOPPRICE_CONDITION";
        private const string EXPIRY_DATE_PARAM = "EXPIRY_DATE";
        // STOP AND TAKE ORDERS PARAMS
        private const string STOPTAKE_STOP_PRICE_PARAM = "STOPPRICE2";
        private const string STOPTAKE_STOP_IS_MARKET_PARAM = "MARKET_STOP_LIMIT";
        private const string STOPTAKE_TAKE_IS_MARKET_PARAM = "MARKET_TAKE_PROFIT";
        private const string STOPTAKE_IS_GTD_PARAM = "IS_ACTIVE_IN_TIME";
        private const string STOPTAKE_ACTIVATION_TIME_PARAM = "ACTIVE_FROM_TIME";
        private const string STOPTAKE_DISACTIVATION_TIME_PARAM = "ACTIVE_TO_TIME";
        // STOP WITH LINKED LIMIT ORDER
        private const string STOPLINKED_LINKED_ORDER_PRICE_PARAM = "LINKED_ORDER_PRICE";
        private const string STOPLINKED_KILL_ON_PARTIALLY_FILLED_PARAM = "KILL_IF_LINKED_ORDER_PARTLY_FILLED";

        // ACTION
        private const string ACTION_NEW_ORDER_PARAM = "NEW_ORDER";
        private const string ACTION_MOVE_ORDER_PARAM = "MOVE_ORDERS";
        private const string ACTION_CANCEL_ORDER_PARAM = "KILL_ORDER";
        private const string ACTION_NEW_STOP_ORDER_PARAM = "NEW_STOP_ORDER";
        private const string ACTION_CANCEL_STOP_ORDER_PARAM = "KILL_STOP_ORDER";
        private const string ACTION_CANCEL_ALL_ORDERS_PARAM = "KILL_ALL_ORDERS";
        private const string ACTION_CANCEL_ALL_FUTURES_ORDERS_PARAM = "KILL_ALL_FUTURES_ORDERS";
        private const string ACTION_CANCEL_ALL_STOP_ORDERS_PARAM = "KILL_ALL_STOP_ORDERS";
        private const string IS_SET_PARAM = "YES";
        private const string IS_NOT_SET_PARAM = "NO";

        public const string BUY_OPERATION_PARAM = "B";
        public const string SELL_OPERATION_PARAM = "S";

        public const string QUEUE_ORDER_PARAM = "PUT_IN_QUEUE";
        public const string FILL_OR_KILL_ORDER_PARAM = "FILL_OR_KILL";
        public const string CANCEL_BALANCE_ORDER_PARAM = "KILL_BALANCE";

        private const string STOP_ORDER_PARAM = "SIMPLE_STOP_ORDER";
        private const string TAKE_PROFIT_ORDER_PARAM = "TAKE_PROFIT_STOP_ORDER";

        public const string ORDER_GTC_EXPIRY_PARAM = "GTC";
        public const string ORDER_TODAY_EXPIRY_PARAM = "TODAY";

        public const string ORDER_TYPE_LIMIT_PARAM = "L";
        public const string ORDER_TYPE_MARKET_PARAM = "M";

        private const string MOVE_ORDER_MODE_SAME_SIZE = "0";
        private const string MOVE_ORDER_NEW_SIZE_MODE = "1";
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
    }
}
