using UnityEngine;
using System.Collections;
using UnityEngine.InputSystem;
using TMPro;
using static UnityEngine.Rendering.DebugUI;

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
        StartCoroutine(InitialArrival());
    }

    private IEnumerator InitialArrival()
    {
        SetPlayerControls(false);

        if (ElevatorMotor.Instance != null)
        {
            float resetHeight = 15f;
            Vector3 currentPos = ElevatorMotor.Instance.transform.position;
            ElevatorMotor.Instance.transform.position = new Vector3(currentPos.x, resetHeight, currentPos.z);

            playerObj.transform.SetParent(ElevatorMotor.Instance.transform);
            playerObj.transform.localPosition = new Vector3(0f, -1.5f, 0f);

            yield return ElevatorMotor.Instance.DescendRoutine(2.0f, 15f, true, true);
        }

        playerObj.transform.SetParent(null);
        SetPlayerControls(true);
        TutorialHUD.OnElevatorLanded?.Invoke();
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

        CrosshairController crosshair = FindFirstObjectByType<CrosshairController>();
        if (crosshair != null) crosshair.SetVisibility(false);

        AudioManager.Instance.Play("Button");
        if (HitEffects.Instance != null) HitEffects.Instance.PlayWinFlash();
        if (EnemySpawner.Instance != null) EnemySpawner.Instance.SetPause(true);

        SetPlayerControls(false);

        if (ElevatorMotor.Instance != null)
        {
            playerObj.transform.SetParent(ElevatorMotor.Instance.transform);

            float currentLocalX = playerObj.transform.localPosition.x;
            playerObj.transform.localPosition = new Vector3(currentLocalX, -1.5f, 0f);

            StartCoroutine(ElevatorMotor.Instance.DescendRoutine(3.0f, 15f, false, false));
        }

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

        if (ElevatorMotor.Instance != null) ElevatorMotor.Instance.StopAllCoroutines();

        AdvanceLayer();

        AudioManager.Instance.ResetAllRisingPitches();

        if (ElevatorMotor.Instance != null)
        {
            float resetHeight = 15f;
            Vector3 currentPos = ElevatorMotor.Instance.transform.position;
            ElevatorMotor.Instance.transform.position = new Vector3(currentPos.x, resetHeight, currentPos.z);
            ElevatorMotor.Instance.transform.GetChild(0).localRotation = Quaternion.identity;

            playerObj.transform.SetParent(ElevatorMotor.Instance.transform);
            playerObj.transform.localPosition = new Vector3(0f, -1.5f, 0f);
        }

        PlayerHealth pHealth = playerObj.GetComponent<PlayerHealth>();
        if (pHealth != null) pHealth.ResetHealth();
        if (TextureOffsetRandomizer.Instance != null) TextureOffsetRandomizer.Instance.RandomizeOffset();
        if (decoMaster.Instance != null) decoMaster.Instance.RefreshDecos();

        currentBattery = 0f;
        if (batteryUI != null) batteryUI.UpdateUI(0, maxBattery);

        if (ElevatorMotor.Instance != null)
        {
            if (SceneFader.Instance != null) SceneFader.Instance.FadeToDim(0f, 1.5f);

            yield return ElevatorMotor.Instance.DescendRoutine(2.0f, 15f, true, true);
        }

        playerObj.transform.SetParent(null);

        yield return new WaitForSecondsRealtime(0.2f);

        SetPlayerControls(true);

        if (crosshair != null) crosshair.SetVisibility(true);

        if (EnemySpawner.Instance != null) EnemySpawner.Instance.RestartSpawner();

        isExtracting = false;
    }

    private void SetPlayerControls(bool state)
    {
        PlayerInput pInput = playerObj.GetComponent<PlayerInput>();
        playerMovement pMove = playerObj.GetComponent<playerMovement>();
        GunController pGun = playerObj.GetComponent<GunController>();
        Rigidbody2D rb = playerObj.GetComponent<Rigidbody2D>();
        Collider2D col = playerObj.GetComponent<Collider2D>();

        if (pInput != null) pInput.enabled = state;
        if (pMove != null) pMove.SetControlState(state);
        if (pGun != null) pGun.SetControlState(state);

        if (rb != null) rb.simulated = state;
        if (col != null) col.enabled = state;
    }
} 