using System.Runtime.InteropServices;

namespace TradingConcepts.CommonImplementations
{
    [StructLayout(LayoutKind.Explicit, Size = STRUCT_SIZE_BYTES)]
    public struct Quote : IQuote
    {
        public const int STRUCT_SIZE_BYTES = sizeof(long) * 3;

        [FieldOffset(0)]
        private Operations _operation;
        [FieldOffset(8)]
        private Decimal5 _price;
        [FieldOffset(16)]
        private long _size;

        public Operations Operation
        {
            get => _operation;
            set => _operation = value;
        }
        public Decimal5 Price
        {
            get => _price;
            set => _price = value;
        }
        public long Size
        {
            get => _size;
            set => _size = value;
        }


        public static bool operator ==(Quote one, Quote another)
        {
            return one.Equals(another);
        }
        public static bool operator !=(Quote one, Quote another)
        {
            return !one.Equals(another);
        }

        public override bool Equals(object? other)
        {
            return other is Quote q &&
                q._operation == _operation &&
                q._price == _price &&
                q._size == _size;
        }
        public bool Equals(IQuote? other)
        {
            return other is not null &&
                other.Operation == Operation &&
                other.Price == Price &&
                other.Size == Size;
        }
        public bool Equals(Quote other)
        {
            return other.Operation == Operation &&
                   other.Price == Price &&
                   other.Size == Size;
        }

        public override int GetHashCode()
        {
            return HashCode.Combine(_operation, _price, _size);
        }
        public override string ToString()
        {
            return $"{Operation} {Size}x{Price}";
        }
    }
}