using UnityEngine;
using System.Collections;

public class ElevatorMotor : MonoBehaviour
{
    public static ElevatorMotor Instance;

    [Header("Components")]
    [SerializeField] private Transform cogTransform;

    [Header("Settings")]
    [SerializeField] private float totalRotation = 720f;

    private void Awake()
    {
        if (Instance == null) Instance = this;
    }

    public IEnumerator DescendRoutine(float duration, float targetYOffset)
    {
        float elapsed = 0;
        Vector3 startPos = transform.position;
        Vector3 endPos = new Vector3(startPos.x, startPos.y - targetYOffset, startPos.z);

        float startRotation = cogTransform != null ? cogTransform.eulerAngles.z : 0;

        ElevatorCog cogScript = null;
        if (cogTransform != null)
        {
            cogScript = cogTransform.GetComponent<ElevatorCog>();
            if (cogScript != null) cogScript.isAutoRotating = false;
        }

        while (elapsed < duration)
        {
            elapsed += Time.deltaTime;
            float t = elapsed / duration;
            float easedT = Mathf.SmoothStep(0f, 1f, t);

            transform.position = Vector3.Lerp(startPos, endPos, easedT);

            if (cogTransform != null)
            {
                float currentZ = Mathf.Lerp(startRotation, startRotation - totalRotation, easedT);
                cogTransform.eulerAngles = new Vector3(0, 0, currentZ);
            }

            yield return null;
        }

        transform.position = endPos;

        if (cogScript != null)
        {
            cogScript.SyncRotation();
            cogScript.isAutoRotating = true;
        }
    }


}