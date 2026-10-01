using System.Threading;
using EncosyTower.Initialization;
using EncosyTower.Tasks;

namespace EncosyTower.Persistences
{
    public interface IPersistDirectory : IInitializable, IDeinitializable
    {
        string Id { get; set; }

        void CreateDataIfNotExist();

        void MarkDirty(bool isDirty);

        UnityTask LoadEntireDirectoryAsync(SourcePriority priority, CancellationToken token);

        UnityTask SaveEntireDirectoryAsync(SaveDestination destination, CancellationToken token);
    }
}
