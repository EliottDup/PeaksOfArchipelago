using PeaksOfArchipelago.MonoBehaviours;
using System.Collections;

namespace PeaksOfArchipelago.Traps
{
    internal class InstantTrap : Trap
    {
        private readonly Action _action;
        private readonly Func<bool> _condition;

        public InstantTrap(string message, Action action, Func<bool> condition = null)
        {
            Message = message;
            _action = action;
            _condition = condition;
        }

        public override bool IsAvailable()
        {
            return _condition == null || _condition();
        }

        public override void Execute(TrapHandler handler)
        {
            _action?.Invoke();
        }
    }
}
