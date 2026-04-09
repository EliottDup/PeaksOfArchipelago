using PeaksOfArchipelago.MonoBehaviours;
using System.Collections;

namespace PeaksOfArchipelago.Traps
{
    internal class TimedTrap : Trap
    {
        private readonly Func<IEnumerator> _onStart;
        private readonly float _duration;

        public bool IsRunning { get; private set; }

        public TimedTrap(string name, string message, Func<IEnumerator> onStart, float duration)
        {
            Name = name;
            Message = message;
            _onStart = onStart;
            _duration = duration;
        }

        public override bool IsAvailable()
        {
            return !IsRunning;
        }

        public override IEnumerator Execute(TrapHandler handler)
        {
            IsRunning = true;
            yield return _onStart();

            handler.StartTimer(Name, _duration, () => IsRunning = false);
        }
    }
}
