using UnityEngine;
using System.Collections.Generic;

public class DecoZone : MonoBehaviour
{
    public ZoneOrientation orientation;

    [Header("Mid Settings")]
    [Tooltip("If orientation is Mid, check this to spawn vertically (between walls) instead of horizontally.")]
    [SerializeField] private bool midIsVertical = false;

    [Header("General Settings")]
    [Range(0, 50)][SerializeField] private int minDeco = 5;
    [Range(0, 50)][SerializeField] private int maxDeco = 10;
    [Tooltip("Distance between spawn points. Ensure the collider is large enough to fit these slots!")]
    [SerializeField] private float spacing = 1.5f;

    private BoxCollider2D zoneCollider;

    public void Generate(List<DecorationItem> library)
    {
        zoneCollider = GetComponent<BoxCollider2D>();
        if (zoneCollider == null)
        {
            Debug.LogError($"{gameObject.name} needs a BoxCollider2D to spawn decorations!");
            return;
        }

        Bounds bounds = zoneCollider.bounds;
        List<Vector2> spawnPoints = Generate1DPoints(bounds);

        int targetAmount = Random.Range(minDeco, maxDeco + 1);
        int actualSpawnAmount = Mathf.Min(targetAmount, spawnPoints.Count);

        for (int i = 0; i < actualSpawnAmount; i++)
        {
            if (spawnPoints.Count == 0) break;

            int index = Random.Range(0, spawnPoints.Count);
            Vector2 pos = spawnPoints[index];
            spawnPoints.RemoveAt(index);

            GameObject prefab = GetRandomPrefab(library);
            if (prefab != null)
            {
                GameObject instance = Instantiate(prefab, pos, Quaternion.identity, transform);
                ApplyOrientation(instance.transform);
            }
        }
    }

    private List<Vector2> Generate1DPoints(Bounds b)
    {
        List<Vector2> points = new List<Vector2>();

        if (orientation == ZoneOrientation.Floor || orientation == ZoneOrientation.Roof || (orientation == ZoneOrientation.Mid && !midIsVertical))
        {
            float centerY = b.center.y;
            for (float x = b.min.x + (spacing / 2); x < b.max.x; x += spacing)
            {
                points.Add(new Vector2(x, centerY));
            }
        }
        else
        {
            float centerX = b.center.x;
            for (float y = b.min.y + (spacing / 2); y < b.max.y; y += spacing)
            {
                points.Add(new Vector2(centerX, y));
            }
        }

        return points;
    }

    private void ApplyOrientation(Transform t)
    {
        t.rotation = Quaternion.identity;

        switch (orientation)
        {
            case ZoneOrientation.Roof:
                t.localScale = new Vector3(t.localScale.x, -1f, 1f);
                break;

            case ZoneOrientation.WallLeft:
                t.rotation = Quaternion.Euler(0, 0, -90f);
                break;

            case ZoneOrientation.WallRight:
                t.rotation = Quaternion.Euler(0, 0, 90f);
                break;

            case ZoneOrientation.Mid:
                if (midIsVertical)
                {
                    t.rotation = Quaternion.Euler(0, 0, 90f);
                }
                break;
        }

        bool isWallType = orientation == ZoneOrientation.WallLeft ||
                          orientation == ZoneOrientation.WallRight ||
                          (orientation == ZoneOrientation.Mid && midIsVertical);

        if (!isWallType && Random.value > 0.5f)
        {
            t.localScale = new Vector3(-t.localScale.x, t.localScale.y, 1f);
        }
    }

    private GameObject GetRandomPrefab(List<DecorationItem> library)
    {
        List<GameObject> validPrefabs = new List<GameObject>();

        foreach (var item in library)
        {
            bool isCompatible = false;
            switch (orientation)
            {
                case ZoneOrientation.Floor: isCompatible = item.canGoOnFloor; break;
                case ZoneOrientation.Roof: isCompatible = item.canGoOnRoof; break;
                case ZoneOrientation.WallLeft: isCompatible = item.canGoOnWallLeft; break;
                case ZoneOrientation.WallRight: isCompatible = item.canGoOnWallRight; break;
                case ZoneOrientation.Mid: isCompatible = item.canGoOnMid; break;
            }

            if (isCompatible && item.prefab != null)
            {
                validPrefabs.Add(item.prefab);
            }
        }

        if (validPrefabs.Count > 0)
        {
            return validPrefabs[Random.Range(0, validPrefabs.Count)];
        }

        return null;
    }
}