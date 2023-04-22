//using AppComponents;
//using Quik;
//using Quik.Entities;
//using Tools;
//using TradingConcepts;

//namespace MarketExecutionService
//{
//    public interface IMarketExecutionService
//    {
//        event Action<IOrder> OrderChanged;
//        event Action<IOrderExecution> OrderExecuted;

//        IEnumerable<IOrder> Orders { get; }
//        IEnumerable<IOrderExecution> Executions { get; }

//        IOrder PlaceNew(IOrderSubmission order);
//        void Change(IOrder order, Decimal5 newPrice, long newSize);
//        void Cancel(IOrder order);
//    }

//    internal sealed class LocalMarketExecutionServiceImplementation : IMarketExecutionService
//    {
//        private readonly IQuik _quik;

//        public static LocalMarketExecutionServiceImplementation Instance { get; } = new();
//        private LocalMarketExecutionServiceImplementation() 
//        {
//            _quik = DI.Resolve<IQuik>() ?? throw new NotImplementedException("Need to initialize Quik and register it in DI container");
//        }

//        public IEnumerable<IOrder> Orders
//        {
//            get => _quik.GetOrders();
//        }
//        public IEnumerable<IOrderExecution> Executions
//        {
//            get => throw new NotImplementedException("Batch fetching of executions is still not implemented..");
//        }

//        public event Action<IOrder> OrderChanged
//        {
//               add => _quik.OrderChanged += value;
//            remove => _quik.OrderChanged -= value;
//        }
//        public event Action<IOrderExecution> OrderExecuted
//        {
//               add => _quik.NewOrderExecution += value;
//            remove => _quik.NewOrderExecution -= value;
//        }

//        public void Cancel(IOrder order)
//        {
//            // when adding new conections, keep in mind that each of them only accepts platform specific entities.
//            _quik.CancelOrder(order);
//        }
//        public void Change(IOrder order, Decimal5 newPrice, long newSize)
//        {
//            _quik.ChangeOrder(order, newPrice, newSize);
//        }
//        public IOrder PlaceNew(IOrderSubmission order)
//        {
//            return _quik.PlaceNewOrder(order as MoexOrderSubmission);
//        }
//    }

//    internal sealed class RemoteMarketExecutionServiceImplementation : IMarketExecutionService
//    {
//        // method calls to serialized commands that will be sent to a remote implementation
//    }

//    internal sealed class MultiHostMarketExecutionServiceImplementation : IMarketExecutionService
//    {
//        private IMarketExecutionService _localService = LocalMarketExecutionServiceImplementation.Instance;
//        private IMarketExecutionService _remoteService = new RemoteMarketExecutionServiceImplementation();
//    }

//    internal interface IServiceHub
//    {
//        void RegisterService(IService service);
//        TService GetService<TService>() where TService : IService;
//    }
//}