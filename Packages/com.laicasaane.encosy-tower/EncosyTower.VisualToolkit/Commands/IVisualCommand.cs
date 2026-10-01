using UnityEngine.Scripting;

namespace EncosyTower.VisualToolkit.Commands
{
    [RequireImplementors]
    public interface IVisualCommand
    {
        void Execute();
    }
}
