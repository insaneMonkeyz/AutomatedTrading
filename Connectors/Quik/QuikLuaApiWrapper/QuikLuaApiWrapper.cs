using System.Diagnostics;
using System.Security.Cryptography;
using System.Xml.Linq;
using BasicConcepts;
using BasicConcepts.SecuritySpecifics.Options;
using QuikLuaApi.Entities;
using QuikLuaApiWrapper;
using QuikLuaApiWrapper.Entities;
using static System.Formats.Asn1.AsnWriter;

namespace QuikLuaApi
{

    public class QuikLuaApiWrapper
    {
        [Flags]
        internal enum OrderbookParsingResult : long
        {
            BidsParsed = 1,
            AsksParsed = 2
        }

        private static LuaState _quikState;
        private static LuaState _localState;

        public static bool IsConnected
        {
            get
            {
                _localState.ExecFunction("isConnected", LuaApi.TYPE_NUMBER);
                if (_localState.TryPopLong(out long value))
                {
                     return value == LuaApi.TRUE;
                }
                else
                {
                    _localState.PopFromStack();
                    return false;
                }
            }
        }

        internal bool TryGetSecurity(string classCode, string ticker, out ISecurity security)
        {
            if (_localState.ExecFunction("getSecurityInfo", LuaApi.TYPE_TABLE, classCode, ticker))
            {
                //optiontype
                //code STRING  Код инструмента                          BRX2BRZ2 or Si65000BL2
                var code = _localState.ReadRowValueString("code");
                //name STRING  Наименование инструмента                 Календ.спрэд BR-11.22-12.22 or Si-12.22M151222CA65000
                var name = _localState.ReadRowValueString("name");
                //short_name STRING  Короткое наименование инструмента  BRX2BRZ2 or Si065000BL2
                var short_name = _localState.ReadRowValueString("short_name");
                //class_code  STRING Код класса инструментов            FUTSPREAD
                var class_code = _localState.ReadRowValueString("class_code");
                //class_name STRING  Наименование класса инструментов   МБ Деривативы: Спреды между фьючерсами
                var class_name = _localState.ReadRowValueString("class_name");
                //face_value  NUMBER Номинал                            0.0
                var face_value = _localState.ReadRowValueString("face_value");
                //face_unit STRING  Валюта номинала                     SUR
                var face_unit = _localState.ReadRowValueString("face_unit");
                //scale NUMBER  Точность(количество значащих цифр после запятой)    2
                var scale = _localState.ReadRowValueString("scale");
                //mat_date NUMBER  Дата погашения               20221101 - expiry
                var mat_date = _localState.ReadRowValueString("mat_date");
                //lot_size NUMBER  Размер лота                  10
                var lot_size = _localState.ReadRowValueString("lot_size");
                //isin_code STRING  ISIN
                var isin_code = _localState.ReadRowValueString("isin_code");
                //min_price_step  NUMBER Минимальный шаг цены   0.01
                var min_price_step = _localState.ReadRowValueString("min_price_step");
                //bsid STRING Bloomberg ID
                var bsid = _localState.ReadRowValueString("bsid");
                //cusip_code STRING  CUSIP
                var cusip_code = _localState.ReadRowValueString("cusip_code");
                //stock_code  STRING StockCode
                var stock_code = _localState.ReadRowValueString("stock_code");
                //couponvalue NUMBER  Размер купона             0.0
                var couponvalue = _localState.ReadRowValueString("couponvalue");
                //sell_leg_classcode STRING  Код класса инструмента ноги на продажу SPBFUT
                //buy_mat_date NUMBER  Дата расчетов сделки на покупку              20221201
                var buy_mat_date = _localState.ReadRowValueString("buy_mat_date");
                //sell_mat_date  NUMBER Дата расчетов сделки на продажу             20221101
                var sell_mat_date = _localState.ReadRowValueString("sell_mat_date");
                //option_strike NUMBER  Страйк опциона                              0.0 or 65000.0

                ISecurity? nearTermLeg = default;
                ISecurity? longTermLeg = default;

                
                ISecurity GetStock(string ticker)
                {

                }

                switch (classCode)
                {
                    case MoexSpecifics.OPTIONS_CLASS_CODE:
                        {

                            break;
                        }
                    case MoexSpecifics.CALENDAR_SPREADS_CLASS_CODE:
                        {
                            var sell_leg_classcode = _localState.ReadRowValueString("sell_leg_classcode");
                            var sell_leg_seccode = _localState.ReadRowValueString("sell_leg_seccode");
                            var buy_leg_classcode = _localState.ReadRowValueString("buy_leg_classcode");
                            var buy_leg_seccode = _localState.ReadRowValueString("buy_leg_seccode");

                            if (sell_leg_classcode != null && sell_leg_seccode != null &&
                                 buy_leg_classcode != null &&  buy_leg_seccode != null)
                            {
                                nearTermLeg = GetSecurity(sell_leg_classcode, sell_leg_seccode);
                                longTermLeg = GetSecurity(buy_leg_classcode, buy_leg_seccode);
                            }
                            break;
                        }
                    case MoexSpecifics.FUTURES_CLASS_CODE:
                        {
                            break;
                        }
                    case MoexSpecifics.STOCK_CLASS_CODE:
                        {
                            break;
                        }
                    default:
                        return null;
                }

                ISecurity security = classCode switch
                {
                    MoexSpecifics.STOCK_CLASS_CODE 
                        => SecuritiesFactory.CreateStock(ticker, name),
                    MoexSpecifics.OPTIONS_CLASS_CODE 
                        => ,
                    MoexSpecifics.FUTURES_CLASS_CODE => new Futures(),
                    MoexSpecifics.CALENDAR_SPREADS_CLASS_CODE => new CalendarSpread(),
                    _ => throw new ArgumentException($"{classCode} is not a supported class code.", nameof(classCode))
                };
            }
        }
        internal OrderbookParsingResult UpdateOrderBook(IOptimizedOrderBook orderbook, string classCode, string ticker)
        {
            OrderbookParsingResult result = default;

            if (_localState.ExecFunction("getQuoteLevel2", LuaApi.TYPE_TABLE, classCode, ticker))
            {
                static bool processQuotes(string countField, string quotesField, Action<OneSideQuotesReader> reader)
                {
                    if (_localState.TryFetchLongFromTable(countField, out long qCount) && qCount > 0)
                    {
                        _localState.PushColumnValueTable(quotesField);
                        reader(ReadRowValueQuotes);
                        _localState.PopFromStack();
                        return true;
                    }
                    else
                    {
                        return false;
                    }
                }

                if (processQuotes("bid_count", "bid", orderbook.UseBids))
                {
                    result |= OrderbookParsingResult.BidsParsed;
                }
                if (processQuotes("offer_count", "offer", orderbook.UseAsks))
                {
                    result |= OrderbookParsingResult.AsksParsed;
                }
            }

            // pop getQuoteLevel2 result
            _localState.PopFromStack();
            return result;
        }

        /// <summary>
        /// Entry Point. This method gets called from the lua wrapper of the Quik trading terminal
        /// </summary>
        /// <param name="L">Pointer to the Lua state object</param>
        /// <returns></returns>
        public unsafe int Initialize(void* L)
        {
            LuaState lua = L;

            try
            {
                lua.TieProxyLibrary("NativeToManagedProxy");
                lua.RegisterCallback(Main, "main");
            }
            catch (Exception ex)
            {
                var msg = ex.Message;
                return -1;
            }

            return 0;
        }

        private static int Main(IntPtr state)
        {
            _localState = state;

            try
            {
                System.Diagnostics.Debugger.Launch();

                var api = new QuikLuaApiWrapper();

                var book = new OrderBook();

                api.UpdateOrderBook(book, "SPBFUT", "BRZ2");
                Debug.Print("\n\n");
                api.GetSecurity("FUTSPREAD", "BRX2BRZ2");
                Debug.Print("\n\n");
                api.GetSecurity("SPBFUT", "BRZ2");
                Debug.Print("\n\n");
                api.GetSecurity("SPBOPT", "Si65000BL2");

                //foreach (var entity in _registeredEntities)
                //{
                //    LuaApi.lua_pushnil(state);
                //    LuaApi.lua_setglobal(state, entity);
                //}

                // пока что уберем, т.к. квик падает
                //LuaApi.lua_close(state);  
            }
            catch (Exception ex)
            {
                var msg = ex.Message;
                return -1;
            }
            return 1;
        }

        private static void ReadRowValueQuotes(Quote[] quotes, Operations operation, long marketDepth)
        {
            const int LAST_ITEM = -1;
            const int SECOND_ITEM = -2;

            var dataLen = (long)LuaApi.lua_rawlen(_localState, LAST_ITEM);
            var quotesSize = Math.Min(dataLen, marketDepth);

            if (quotesSize > 0)
            {
                long passed = 0;
                long thisIndex = 0;
                long luaIndex = 1;
                long increment = 1;

                if (operation == Operations.Buy && quotesSize != 1)
                {
                    luaIndex = dataLen - quotesSize - 1;
                    thisIndex = quotesSize - 1;
                    increment = -1;
                }

                while (passed < quotesSize)
                {
                    if (LuaApi.lua_rawgeti(_localState, LAST_ITEM, luaIndex++) != LuaApi.TYPE_TABLE)
                    {
                        _localState.PopFromStack();
                        throw new QuikApiException("Array of quotes ended prior than expected. ");
                    }

                    if (_localState.LastItemIsTable() &&
                        _localState.TryFetchDecimalFromTable("price", out Decimal5 price) &&
                        _localState.TryFetchLongFromTable("quantity", out long size))
                    {
                        quotes[thisIndex] = new Quote
                        {
                            Price = price,
                            Size = size,
                            Operation = operation
                        };

                        _localState.PopFromStack();

                        thisIndex += increment;
                        passed++;
                    }
                    else
                    {
                        _localState.PopFromStack();

                        break;
                    }
                }
            }
        }
        private bool TryGetOptionType(string ticker, out OptionTypes type)
        {
            /*
                Большинство параметров описаны в документации на терминал QUIK
                -Раздел 8. Алгоритмический язык QPILE
                --Функции для получения значений Таблицы текущих торгов
                ---Значения параметров функций

                Кроме того существует возможность узнать имя любого параметра из таблицы текущих торгов.
                Достаточно вывести таблицу в Excel по DDE с установленной галкой "Формальные заголовки"
             */

            type = default;

            if (_localState.ExecFunction("getParamEx", LuaApi.TYPE_TABLE, MoexSpecifics.OPTIONS_CLASS_CODE, ticker, "optiontype"))
            {
                if (_localState.ReadRowValueLong("result") == LuaApi.TRUE)
                {
                    Enum.TryParse<OptionTypes>(
                        _localState.ReadRowValueString("param_value"), out type);
                }
            }

            _localState.PopFromStack();

            return type != default;
        }
        private ISecurity GetOption(string ticker)
        {
            if (_localState.TryFetchStringFromTable("name", out string descr) &&
                _localState.TryFetchDecimalFromTable("option_strike", out Decimal5 strike) &&
                _localState.TryFetchStringFromTable("base_active_classcode", out string undClass) &&
                _localState.TryFetchStringFromTable("base_active_seccode", out string undTicker) &&
                _localState.TryFetchStringFromTable("mat_date", out string mat_date) &&
                mat_date.TryConvertToMoexExpiry(out DateTimeOffset expiry) &&
                TryGetSecurity(undClass, undTicker, out ISecurity underlying) &&
                TryGetOptionType(ticker, out OptionTypes type))
            {
                return new Option
                {
                    Ticker = ticker,
                    Description = descr,
                    Strike = strike,
                    OptionType = type,
                    Expiry = expiry,
                    Underlying = underlying
                };
            }

            return null;
        }
    }
}