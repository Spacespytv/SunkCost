using UnityEngine;
using System.Collections;
using UnityEngine.InputSystem;
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

    [Header("Player Settings")]
    [SerializeField] private GameObject playerObj;
    [SerializeField] private Vector3 spawnPosition;

    [Header("Difficulty Curves")]
    [SerializeField] private AnimationCurve batteryCurve;
    [SerializeField] private float batteryMultiplier = 16f;

    public bool isExtracting = false;

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
        float curvePoint = batteryCurve.Evaluate(currentLayer);
        float totalValue = curvePoint * batteryMultiplier;
        maxBattery = Mathf.RoundToInt(totalValue);

        UpdateLayerUI();

        if (batteryUI != null)
        {
            batteryUI.UpdateUI(currentBattery, maxBattery);
        }
    }

    private void UpdateLayerUI()
    {
        if (layerText != null)
        {
            layerText.text = "LAYER " + currentLayer;
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

    public void StartExtraction()
    {
        if (isExtracting) return;
        StartCoroutine(ExtractionSequence());
    }

    private IEnumerator ExtractionSequence()
    {
        isExtracting = true;

        if (EnemySpawner.Instance != null) EnemySpawner.Instance.SetPause(true);

        PlayerInput pInput = playerObj.GetComponent<PlayerInput>();
        if (pInput != null) pInput.enabled = false;

        playerMovement pMove = playerObj.GetComponent<playerMovement>();
        GunController pGun = playerObj.GetComponent<GunController>();

        if (pMove != null) pMove.SetControlState(false);
        if (pGun != null) pGun.SetControlState(false);

        EnergyCrystal[] crystals = Object.FindObjectsByType<EnergyCrystal>(FindObjectsSortMode.None);
        foreach (EnergyCrystal crystal in crystals)
        {
            if (crystal != null)
            {
                crystal.InitiateCollection(false); 
                yield return new WaitForSecondsRealtime(0.03f); 
            }
        }

        Enemy[] enemies = Object.FindObjectsByType<Enemy>(FindObjectsSortMode.None);
        foreach (Enemy enemy in enemies)
        {
            if (enemy != null)
            {
                enemy.Die(false); 
                yield return new WaitForSecondsRealtime(0.05f);
            }
        }

        yield return new WaitForSecondsRealtime(1.0f);

        if (SceneFader.Instance != null) SceneFader.Instance.FadeOut();
        yield return new WaitForSecondsRealtime(0.6f);

        playerObj.transform.position = spawnPosition;


        PlayerHealth pHealth = playerObj.GetComponent<PlayerHealth>();
        if (pHealth != null) pHealth.ResetHealth();

        AdvanceLayer();

        if (decoMaster.Instance != null)
        {
            decoMaster.Instance.RefreshDecos();
        }

        if (EnemySpawner.Instance != null)
        {
            EnemySpawner.Instance.RestartSpawner(); 
        }

        currentBattery = 0f;
        if (batteryUI != null) batteryUI.UpdateUI(0, maxBattery);

        yield return new WaitForSecondsRealtime(0.2f);

        if (SceneFader.Instance != null) SceneFader.Instance.FadeToDim(0f, 1.0f);

        if (pInput != null) pInput.enabled = true;
        if (pMove != null) pMove.SetControlState(true);
        if (pGun != null) pGun.SetControlState(true);

        isExtracting = false;
    }
}