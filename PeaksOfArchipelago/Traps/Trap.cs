using PeaksOfArchipelago.MonoBehaviours;
using System.Collections;

namespace PeaksOfArchipelago.Traps
{
    internal abstract class Trap
    {
        public string Name = "";
        public string Message;

        public virtual bool IsAvailable()
        {
            return true;
        }

        public abstract IEnumerator Execute(TrapHandler handler);
    }
}
