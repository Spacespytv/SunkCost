using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum ZoneOrientation { Floor, Roof, WallLeft, WallRight, Mid }

[System.Serializable]
public class DecorationItem
{
    public string name;
    public GameObject prefab;
    public bool canGoOnFloor;
    public bool canGoOnRoof;
    public bool canGoOnWallLeft;
    public bool canGoOnWallRight;
    public bool canGoOnMid; 
}

public class decoMaster : MonoBehaviour
{
    public List<DecorationItem> decoLibrary;

    void Start()
    {
        DecoZone[] zones = Object.FindObjectsByType<DecoZone>(FindObjectsSortMode.None);

        if (zones.Length == 0)
        {
            Debug.LogWarning("DecoMaster: No DecoZones found in the scene!");
            return;
        }

        foreach (var zone in zones)
        {
            zone.Generate(decoLibrary);
        }
    }
}