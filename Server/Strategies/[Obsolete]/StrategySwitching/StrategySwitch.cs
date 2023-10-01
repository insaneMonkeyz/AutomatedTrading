using System;
using System.Collections.Generic;
using System.Linq;

namespace Strategies.StrategySwitching
{
    public interface ISwitchable
    {
        bool IsOn { get; set; }
    }

    public class StrategyToggleSwitch : StrategySwitch
    {
        public IEnumerable<ISwitchable> SwitchOff { get; }

        public StrategyToggleSwitch(IEnumerable<ISwitchable> switchOn,
            IEnumerable<ISwitchable> switchOff) : base(switchOn)
        {
            if (switchOff == null || switchOff == Enumerable.Empty<ISwitchable>())
                throw new ArgumentNullException($"Argument {nameof(switchOff)} is null or empty");

            SwitchOff = switchOff;
        }

        public override void Switch(bool isOn)
        {
            if (IsEnabled && isOn != State) Toggle();
        }

        private void Toggle()
        {
            lock (syncRoot)
            {
                foreach (var strategy in SwitchOff)
                {
                    strategy.IsOn = State;
                }

                State = !State;

                foreach (var strategy in SwitchOn)
                {
                    strategy.IsOn = State;
                }
            }
        }
    }
    public class StrategySwitch
    {
        public bool IsEnabled { get; set; }
        public bool State { get; protected set; }
        public IEnumerable<ISwitchable> SwitchOn { get; }

        protected readonly object syncRoot
            = new object();

        public StrategySwitch(IEnumerable<ISwitchable> switchOn)
        {
            if (switchOn == null || switchOn == Enumerable.Empty<ISwitchable>())
                throw new ArgumentNullException($"Argument {nameof(switchOn)} is null or empty");

            SwitchOn = switchOn;
        }

        public virtual void Switch(bool isOn)
        {
            if (IsEnabled && isOn != State)
            {
                State = !State;

                foreach (var strategy in SwitchOn)
                {
                    strategy.IsOn = State;
                }
            }
        }
    }
}
