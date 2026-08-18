using HexAutoStorage.Configuration;
using UnityEngine;

namespace HexAutoStorage.Features
{
    internal class StorageRadiusVisualizer : MonoBehaviour
    {
        private const int SegmentCount = 96;
        private const float LineWidth = 0.4f;
        private const float HeightOffset = 0.05f;
        private const float RaycastHeight = 20f;
        private const float RaycastDistance = 50f;

        private LineRenderer _lineRenderer;
        private Smelter _smelter;
        private int _terrainMask;

        private void Awake()
        {
            _smelter = GetComponent<Smelter>();

            if (_smelter == null)
            {
                Plugin.Log.LogDebug($"StorageRadiusVisualizer attached to non-Smelter object: {gameObject.name}");
                
                Destroy(this);
                return;
            }

            _terrainMask = LayerMask.GetMask("terrain");

#if DEBUG
            if (_terrainMask == 0)
            {
                Plugin.Log.LogDebug("StorageRadiusVisualizer could not find the 'terrain' layer.");
            }
#endif

            _lineRenderer = gameObject.AddComponent<LineRenderer>();
            _lineRenderer.loop = true;
            _lineRenderer.useWorldSpace = true;
            _lineRenderer.positionCount = SegmentCount;
            _lineRenderer.startWidth = LineWidth;
            _lineRenderer.endWidth = LineWidth;
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

            for (int i = 0; i < SegmentCount; i++)
            {
                float angle = i * Mathf.PI * 2f / SegmentCount;

                Vector3 worldPoint = transform.position + new Vector3(
                    Mathf.Cos(angle) * radius,
                    0f,
                    Mathf.Sin(angle) * radius);

                Vector3 rayOrigin = worldPoint + Vector3.up * RaycastHeight;

                if (Physics.Raycast(rayOrigin, Vector3.down, out RaycastHit hit, RaycastDistance, _terrainMask))
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