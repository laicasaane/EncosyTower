using UnityEngine;

namespace EncosyTower.Samples.Processing
{
    internal sealed class LoadingIndicator : MonoBehaviour
    {
        [SerializeField] private float _rotationSpeed = 2f;
        [SerializeField] private Transform _transform;

        private void Update()
        {
            _transform.Rotate(Vector3.forward, _rotationSpeed * Time.smoothDeltaTime);
        }
    }
}
