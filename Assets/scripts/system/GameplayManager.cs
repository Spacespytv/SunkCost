using UnityEngine;
using TMPro; 

public class GameplayManager : MonoBehaviour
{
    public static GameplayManager Instance;

    [Header("Progression")]
    public int currentLayer = 1;
    [SerializeField] private TextMeshProUGUI layerText;

    [Header("Elevator Settings")]
    public Transform elevatorCog;
    public float currentBattery = 0f;
    public float maxBattery = 100f;

    [Header("UI Reference")]
    [SerializeField] private BatteryUI batteryUI;

    void Awake()
    {
        if (Instance == null) Instance = this;
    }

    void Start()
    {
        UpdateLayerUI();
    }

    public void AdvanceLayer()
    {
        currentLayer++;
        UpdateLayerUI();
    }

    private void UpdateLayerUI()
    {
        if (layerText != null)
        {
            layerText.text = "Layer " + currentLayer;
        }
    }

    public void UpdateBattery(float amount)
    {
        currentBattery += amount;
        currentBattery = Mathf.Clamp(currentBattery, 0, maxBattery);

        if (batteryUI != null)
        {
            batteryUI.UpdateUI(currentBattery, maxBattery);
        }
    }
}