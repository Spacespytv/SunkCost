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
    public static decoMaster Instance; 
    public List<DecorationItem> decoLibrary;

    void Awake()
    {
        if (Instance == null) Instance = this;
    }

    void Start()
    {
        RefreshDecos();
    }

    public void RefreshDecos()
    {
        DecoZone[] zones = Object.FindObjectsByType<DecoZone>(FindObjectsSortMode.None);

        foreach (var zone in zones)
        {
            zone.CleanAndGenerate(decoLibrary);
        }
    }
}