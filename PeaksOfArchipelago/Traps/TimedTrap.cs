using PeaksOfArchipelago.MonoBehaviours;
using System.Collections;

namespace PeaksOfArchipelago.Traps
{
    internal class TimedTrap : Trap
    {
        private readonly Action _onStart;
        private readonly Action _onEnd;
        private readonly float _duration;
        private Func<bool> _condition;

        public bool IsRunning { get; private set; }

        public TimedTrap(string name, string message, Action onStart, Action onEnd, float duration, Func<bool> condition = null)
        {
            Name = name;
            Message = message;
            _onStart = onStart;
            _duration = duration;
            _condition = condition;
            _onEnd = onEnd;
        }

        public void SetCondition(Func<bool> condition)
        {
            _condition = condition;
        }

        public override bool IsAvailable()
        {
            return !IsRunning && (_condition == null || _condition());
        }

        public override void Execute(TrapHandler handler)
        {
            IsRunning = true;
            _onStart?.Invoke();

            handler.StartTimer(Name, _duration, () => { _onEnd?.Invoke();  IsRunning = false; });
        }
    }
}
