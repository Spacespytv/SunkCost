using UnityEngine;

public class SwingingLantern : MonoBehaviour
{
    private Rigidbody2D rb;
    [SerializeField] private float impactForce = 12f;

    [Header("Audio Settings")]
    [SerializeField] private string swingSoundName = "Wind";
    [SerializeField] private float maxPitch = 1.5f;
    [SerializeField] private float minPitch = 0.8f;
    [SerializeField] private float velocityThreshold = 5f; 

    private Sound swingSound;

    void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
    }

    void Start()
    {
        swingSound = AudioManager.Instance.GetSound(swingSoundName);
        if (swingSound != null)
        {
            swingSound.source.loop = true;
            swingSound.source.volume = 0;
            swingSound.source.Play();
        }
    }

    void Update()
    {
        HandleReactiveAudio();
    }

    private void HandleReactiveAudio()
    {
        if (swingSound == null) return;

        float speed = Mathf.Abs(rb.angularVelocity);

        if (speed > velocityThreshold)
        {
            float t = Mathf.InverseLerp(velocityThreshold, 500f, speed);
            swingSound.source.pitch = Mathf.Lerp(minPitch, maxPitch, t);

            swingSound.source.volume = Mathf.Lerp(0f, swingSound.volume, t * 2f);
        }
        else
        {
            swingSound.source.volume = Mathf.MoveTowards(swingSound.source.volume, 0f, Time.deltaTime);
        }
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("Bullet"))
        {
            Vector2 direction = (transform.position - other.transform.position).normalized;
            Vector2 horizontalPush = new Vector2(direction.x, 0).normalized;

            AudioManager.Instance.PlayOneShot("Glass");
            ApplySwing(horizontalPush);
        }
    }

    public void ApplySwing(Vector2 forceDir)
    {
        rb.AddForce(forceDir * impactForce, ForceMode2D.Impulse);
    }
}