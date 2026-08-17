using UnityEngine;

namespace HexAutoStorage.Features
{
    internal class StorageRadiusVisualizer : MonoBehaviour
    {
        private const int SegmentCount = 96;
        private const float HeightOffset = 0.05f;
        private const float RaycastHeight = 10f;
        private const float RaycastDistance = 30f;

        private LineRenderer _lineRenderer;

        private void Awake()
        {
            _lineRenderer = gameObject.AddComponent<LineRenderer>();

            _lineRenderer.loop = true;
            _lineRenderer.useWorldSpace = true;
            _lineRenderer.positionCount = SegmentCount;
            _lineRenderer.startWidth = 0.05f;
            _lineRenderer.endWidth = 0.05f;
            _lineRenderer.material = new Material(Shader.Find("Sprites/Default"));
        }

        private void Update()
        {
            if (Plugin.Instance == null || !Plugin.ModEnabled.Value || !Plugin.ShowStorageRadius.Value)
            {
                _lineRenderer.enabled = false;
                return;
            }

            _lineRenderer.enabled = true;

            float radius = Plugin.StorageRadius.Value;

            for (int i = 0; i < SegmentCount; i++)
            {
                float angle = i * Mathf.PI * 2f / SegmentCount;

                var offset = new Vector3(
                    Mathf.Cos(angle) * radius,
                    0f,
                    Mathf.Sin(angle) * radius);

                Vector3 worldPoint = transform.position + offset;
                Vector3 rayOrigin = worldPoint + Vector3.up * RaycastHeight;

                if (Physics.Raycast(rayOrigin, Vector3.down, out RaycastHit hit, RaycastDistance))
                {
                    worldPoint.y = hit.point.y + HeightOffset;
                }
                else
                {
                    worldPoint.y = transform.position.y + HeightOffset;
                }

                _lineRenderer.SetPosition(i, worldPoint);
            }
        }

        private void OnDestroy()
        {
            if (_lineRenderer != null && _lineRenderer.material != null)
            {
                Destroy(_lineRenderer.material);
            }
        }
    }
}