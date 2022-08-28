using Core.Concepts.Entities;

namespace Core.Concepts.AppComponents
{
    public delegate void OrderExecutionEventHandler(IOrderExecution orderExecution);
    public delegate void OrderChangedEventHandler(IOrder order);

    public interface IExecutionProvider
    {
        void PlaceOrder(IOrder order);
        void ChangeOrder(IOrder order);
        void CancelOrder(IOrder order);

        public event OrderExecutionEventHandler OrderExecuted;
        public event OrderChangedEventHandler OrderChanged;
    }
}
