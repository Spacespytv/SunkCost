using UnityEngine;

public class CameraShake : MonoBehaviour
{
    private Vector3 homePosition;
    private float shakeTimeRemaining;
    private float shakePower;
    private float shakeFadeTime;

    [SerializeField] private float rotationMultiplier = 5f;

    void Start()
    {
        homePosition = transform.localPosition;
    }

    void Update()
    {
        if (shakeTimeRemaining > 0)
        {
            shakeTimeRemaining -= Time.deltaTime;

            float xAmount = Random.Range(-1f, 1f) * shakePower;
            float yAmount = Random.Range(-1f, 1f) * shakePower;
            float rAmount = Random.Range(-1f, 1f) * shakePower * rotationMultiplier;

            transform.localPosition = homePosition + new Vector3(xAmount, yAmount, 0f);
            transform.localRotation = Quaternion.Euler(0f, 0f, rAmount);

            shakePower = Mathf.MoveTowards(shakePower, 0f, shakeFadeTime * Time.deltaTime);
        }
        else
        {
            transform.localPosition = homePosition;
            transform.localRotation = Quaternion.identity;
        }
    }

    public void StartShake(float duration, float power)
    {
        shakeTimeRemaining = duration;
        shakePower = power;
        shakeFadeTime = power / duration;
    }
}