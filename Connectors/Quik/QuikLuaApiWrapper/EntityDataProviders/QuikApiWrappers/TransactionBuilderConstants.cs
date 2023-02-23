using System.Text;

namespace Quik.EntityDataProviders.QuikApiWrappers
{
    internal partial class TransactionBuilder
    {
        public const string TABLE_NAME = "trans_reply";
        public const string CALLBACK_METHOD = "OnTransReply";

        //TRANSACTION
        private const string TRANSACTION_ID_PARAM = "TRANS_ID";
        private const string CLASS_CODE_PARAM = "CLASSCODE";
        private const string TICKER_PARAM = "SECCODE";
        private const string ACCOUNT_PARAM = "ACCOUNT";
        private const string CLIENT_CODE_PARAM = "CLIENT_CODE";
        private const string ACTION_PARAM = "ACTION";
        private const string OPERATION_PARAM = "OPERATION";
        private const string EXECUTION_TYPE_PARAM = "TYPE";
        private const string EXECUTION_CONDITION_PARAM = "EXECUTION_CONDITION";
        private const string SIZE_PARAM = "QUANTITY";
        private const string MODE_PARAM = "MODE";
        private const string PRICE_PARAM = "PRICE";
        private const string COMMENT_PARAM = "COMMENT";
        private const string ORDER_ID_PARAM = "ORDER_KEY";
        private const string CHECK_LIMITS_PARAM = "CHECK_LIMITS";
        private const string BASE_CONTRACT_PARAM = "BASE_CONTRACT";
        private const string KILL_ACTIVE_ORDERS_PARAM = "KILL_ACTIVE_ORDERS";

        private const string MOVE_ORDER_OLD_ID_PARAM = "FIRST_ORDER_NUMBER";
        private const string MOVE_ORDER_OLD_SIZE_PARAM = "FIRST_ORDER_NEW_QUANTITY";
        private const string MOVE_ORDER_OLD_PRICE_PARAM = "FIRST_ORDER_NEW_PRICE";
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

        private const string BUY_OPERATION_PARAM = "B";
        private const string SELL_OPERATION_PARAM = "S";

        private const string QUEUE_ORDER_PARAM = "PUT_IN_QUEUE";
        private const string FILL_OR_KILL_ORDER_PARAM = "FILL_OR_KILL";
        private const string CANCEL_BALANCE_ORDER_PARAM = "KILL_BALANCE";

        private const string STOP_ORDER_PARAM = "SIMPLE_STOP_ORDER";
        private const string TAKE_PROFIT_ORDER_PARAM = "TAKE_PROFIT_STOP_ORDER";

        private const string ORDER_GTC_EXPIRY_PARAM = "GTC";
        private const string ORDER_TODAY_EXPIRY_PARAM = "TODAY";

        private const string ORDER_TYPE_LIMIT_PARAM = "L";
        private const string ORDER_TYPE_MARKET_PARAM = "M";
    }
}
