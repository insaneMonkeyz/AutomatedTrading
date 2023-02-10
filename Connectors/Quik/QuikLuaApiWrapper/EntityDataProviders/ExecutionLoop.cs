namespace Quik.EntityProviders
{
    internal class ExecutionLoop
    {
        private bool _running = false;
        private Thread? _thread;

        public event Action? Execute;
        public event Action? Starting;
        public event Action? Stopping;

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
            Starting?.Invoke();

            while (_running)
            {
                try
                {
#pragma warning disable CS8602
                    Execute();
#pragma warning restore CS8602
                }
                catch (Exception ex)
                {
                    ex.ToString().DebugPrintWarning();
                }
            }

            Stopping?.Invoke();
        }
    }
}
