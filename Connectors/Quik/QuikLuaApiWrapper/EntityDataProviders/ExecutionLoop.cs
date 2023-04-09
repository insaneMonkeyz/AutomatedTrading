namespace Quik.EntityProviders
{
    internal class ExecutionLoop
    {
        private readonly Log _log = LogManagement.GetLogger<ExecutionLoop>();
        private bool _running = false;

        public event Action? Execute;
        public event Action Starting = delegate { };
        public event Action Stopping = delegate { };

        public virtual void Enter()
        {
            if (!_running)
            {
                _running = true;
                Loop();
            }
        }
        public virtual void Abort()
        {
            if (_running)
            {
                _running = false;
            }
        }

        private void Loop()
        {
            Starting();

            while (_running)
            {
                try
                {
#pragma warning disable CS8602
                    Execute();
#pragma warning restore CS8602
                }
                catch (Exception e)
                {
                    _log.Error("Exception in execution loop", e);
                }
            }

            Stopping();
        }
    }
}
