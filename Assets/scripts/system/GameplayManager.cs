using UnityEngine;

public class GameplayManager : MonoBehaviour
{
    public static GameplayManager Instance;

    public Transform elevatorCog;
    public float batteryLevel = 0f;
    public int currentLayer = 1;

    void Awake()
    {
        if (Instance == null) Instance = this;
        else Destroy(gameObject);
    }

    public void UpdateBattery(float amount)
    {
        batteryLevel += amount;
    }
}