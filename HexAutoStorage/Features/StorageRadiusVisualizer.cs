using HexAutoStorage.Configuration;
using UnityEngine;

namespace HexAutoStorage.Features
{
    internal class StorageRadiusVisualizer : MonoBehaviour
    {
        private const int SegmentCount = 96;
        private const float HeightOffset = 0.05f;

        private LineRenderer _lineRenderer;
        private Smelter _smelter;

        private void Awake()
        {
            _smelter = GetComponent<Smelter>();

            if (_smelter == null)
            {
#if DEBUG
                Plugin.Log.LogWarning($"StorageRadiusVisualizer attached to non-Smelter object: {gameObject.name}");
#endif
                Destroy(this);
                return;
            }

            _lineRenderer = gameObject.AddComponent<LineRenderer>();
            _lineRenderer.loop = true;
            _lineRenderer.useWorldSpace = true;
            _lineRenderer.positionCount = SegmentCount;
            _lineRenderer.startWidth = 0.05f;
            _lineRenderer.endWidth = 0.05f;
            _lineRenderer.material = new Material(Shader.Find("Sprites/Default"));
            _lineRenderer.enabled = false;
        }

        private void Update()
        {
            if (_lineRenderer == null)
            {
                return;
            }

            if (Plugin.Instance == null ||
                !StorageConfig.ModEnabled.Value ||
                !StorageConfig.ShowStorageRadius.Value ||
                SmelterHoverTracker.HoveredSmelter != _smelter)
            {
                _lineRenderer.enabled = false;
                return;
            }

            _lineRenderer.enabled = true;

            float radius = StorageConfig.StorageRadius.Value;
            float height = transform.position.y + HeightOffset;

            for (int i = 0; i < SegmentCount; i++)
            {
                float angle = i * Mathf.PI * 2f / SegmentCount;

                var offset = new Vector3(
                    Mathf.Cos(angle) * radius,
                    0f,
                    Mathf.Sin(angle) * radius);

                Vector3 worldPoint = transform.position + offset;
                worldPoint.y = height;

                _lineRenderer.SetPosition(i, worldPoint);
            }
        }

        private void OnDestroy()
        {
            if (_lineRenderer == null)
            {
                return;
            }

            if (_lineRenderer.material != null)
            {
                Destroy(_lineRenderer.material);
            }

            Destroy(_lineRenderer);
            _lineRenderer = null;
        }
    }
}