//using AppComponents;

//namespace _notintendettouse
//{
//    interface IService
//    {
//        event Action<object>? NewEvent;
//        Task<object> DoAsync(object arg);
//    }

//    static class ServiceFactory
//    {
//        public static IService CreateLocal() => new ExecutiveService();
//        public static IService CreateRemote() => new BrokerProxy();
//    }

//    class ServiceRouter : IService
//    {
//        IService? local;
//        IService? remote;

//        // от событий надо избавиться т.к. они не позволят задать филтрацию.
//        // подписка должна осуществляться выделенными методами
//        public event Action<object>? NewEvent;

//        public Task<object> DoAsync(object arg)
//        {
//            var tasks = new List<object>(2);

//            if (local is not null)
//            {
//                tasks.Add(local.DoAsync(arg));
//            }

//            if (remote is not null)
//            {
//                tasks.Add(remote.DoAsync(arg));
//            }
//        }
//    }

//    internal class ServiceShell : IService
//    {
//        public event Action<object>? NewEvent;
//        public Action<Action<object>>? OnSubscribedNewEvent;
//        public Action<Action<object>>? OnUnsubscribedNewEvent;

//        public async Task<object> DoAsync(object arg)
//        {
//            return DoAsync(arg);
//        }
//        public Func<object, Task<object>> OnDoAsync;
//    }

//    class BrokerProxy : IService
//    {
//        private IBroker _broker;

//        public event Action<object>? NewEvent
//        {
//            add => _broker.Subscribe<object, object>("ISrvc.NewEvent", value, NoTransformation);
//            remove => _broker.Unsubscribe(Guid.Empty, value);
//        }

//        public async Task<object> DoAsync(object arg)
//        {
//            _broker.Publish("ISrvc.Do.Request", arg);
//            return _broker.Receive<object>("ISrvc.Do.Response");
//        }

//        internal void AttachExecutiveService(IService service)
//        {
//            // подпишемся на события чтобы пробрасывать их в брокер
//            service.NewEvent += RaiseNewEvent;

//            // подпишемся на выполнение команды Do 
//            _broker.Subscribe<object, object>("ISrvc.Do.Request", async (arg) => await service.DoAsync(arg), NoTransformation);
//        }
//        internal void DetachExecutiveService(IService service)
//        {
//            throw new NotImplementedException();
//        }

//        void RaiseNewEvent(object arg)
//        {
//            _broker.Publish("ISrvc.NewEvent", arg);
//        }

//        object NoTransformation(object arg) => arg;
//    }

//    class ExecutiveService : IService
//    {
//        BrokerProxy _remoteProxy = ServiceFactory.CreateRemote() as BrokerProxy;

//        public ExecutiveService()
//        {
//            // подпишем брокера который привяжется к нашим методам и событиям
//            // чтобы удаленные подписчики могли сюда обращаться
//            _remoteProxy.AttachExecutiveService(this);
//        }

//        public event Action<object>? NewEvent;

//        public void RaiseEvent(object args)
//        {
//            NewEvent?.Invoke(args);
//        }

//        public async Task<object> DoAsync(object arg)
//        {
//            return Task.Delay(1000);
//        }
//    }

//    class Consumer
//    {
//        Consumer()
//        {
//            //              IService is ServiceRouter
//            //var service = DI.Resolve<IService>();

//            //service.DoAsync(null).Wait();
//        }
//    }
//}
