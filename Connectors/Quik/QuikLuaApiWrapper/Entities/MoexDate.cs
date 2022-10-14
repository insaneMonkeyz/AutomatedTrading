using System;

namespace MoexCommonTypes
{
    public struct MoexDate
    {
        public MoexDate(uint value) : this()
        {
            _value = value;
        }

        public uint Value
        {
            get => _value;
            set
            {
                _value = value;
                _dayHasValue = false;
                _monthHasValue = false;
                _yearHasValue = false;
                _dateHasValue = false;
            }
        }
        uint _value;

        public int Day
        {
            get
            {
                if (!_dayHasValue)
                {
                    _day = (int)(_value % 100);
                    _dayHasValue = true;
                }

                return _day;
            }
        }
        int _day;
        bool _dayHasValue;

        public int Month
        {
            get
            {
                if (!_monthHasValue)
                {
                    _month = (int)(_value / 100 % 100);
                    _monthHasValue = true;
                }

                return _month;
            }
        }
        int _month;
        bool _monthHasValue;

        public int Year
        {
            get
            {
                if (!_yearHasValue)
                {
                    _year = (int)(_value / 10_000);
                    _yearHasValue = true;
                }

                return _year;
            }
        }
        int _year;
        bool _yearHasValue;

        public DateTime Date
        {
            get
            {
                if (!_dateHasValue)
                {
                    _date = new DateTime(Year, Month, Day);
                    _dateHasValue = true;
                }

                return _date;
            }
        }
        DateTime _date;
        bool _dateHasValue;
    }
}
