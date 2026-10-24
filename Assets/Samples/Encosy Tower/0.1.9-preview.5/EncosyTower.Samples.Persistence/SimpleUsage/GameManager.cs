using EncosyTower.PubSub;
using UnityEngine;

namespace EncosyTower.Samples.Persistence.SimpleUsage
{
    internal class GameManager : MonoBehaviour
    {
        private void Start()
        {
            var publisher = GlobalMessenger.Publisher.Scope<ScreenScope>();
            ShowMainMenuScreenMsg.Publish(in publisher);
        }
    }
}
