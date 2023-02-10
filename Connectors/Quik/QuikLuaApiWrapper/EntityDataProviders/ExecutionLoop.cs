namespace Quik.EntityProviders
{
    internal class ExecutionLoop
    {
        private bool _running = false;
        private Thread? _thread;

        public event Action? Execute;
        public event Action? Starting;
        public event Action? Stopping;

        public virtual void Start()
        {
            if (!_running)
            {
                _running = true;
                _thread = new Thread(Loop);
                _thread.Start();
            }
        }
        public virtual void Stop()
        {
            if (_running)
            {
                _running = false;
                _thread = null;
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
