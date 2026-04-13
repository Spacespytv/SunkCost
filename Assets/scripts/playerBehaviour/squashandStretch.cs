using UnityEngine;

public class ProceduralAnimator : MonoBehaviour
{
    [SerializeField] private Transform visualTransform;
    [SerializeField] private Rigidbody2D rb;

    [Header("Settings")]
    [SerializeField] private float squashStrength = 0.15f;
    [SerializeField] private float stretchStrength = 0.1f;
    [SerializeField] private float smoothness = 10f;

    private Vector3 originalScale;
    private float currentFacing = 1f;
    private float juiceX = 1f; 
    private float juiceY = 1f;

    private void Start()
    {
        originalScale = visualTransform.localScale;
        juiceX = originalScale.x;
        juiceY = originalScale.y;
    }

    private void Update()
    {
        if (Mathf.Abs(visualTransform.localScale.x) > 0.01f)
        {
            currentFacing = Mathf.Sign(visualTransform.localScale.x);
        }

        float targetJuiceY = originalScale.y;
        float targetJuiceX = originalScale.x;

        float velocityY = rb.velocity.y;
        if (Mathf.Abs(velocityY) > 0.1f)
        {
            float stretch = Mathf.Abs(velocityY) * stretchStrength * 0.05f;
            targetJuiceY += stretch;
            targetJuiceX -= stretch;
        }

        juiceX = Mathf.Lerp(juiceX, targetJuiceX, Time.deltaTime * smoothness);
        juiceY = Mathf.Lerp(juiceY, targetJuiceY, Time.deltaTime * smoothness);

        visualTransform.localScale = new Vector3(juiceX * currentFacing, juiceY, originalScale.z);
    }

    public void ApplyLandingSquash()
    {
        juiceX = originalScale.x + squashStrength;
        juiceY = originalScale.y - squashStrength;
    }
}