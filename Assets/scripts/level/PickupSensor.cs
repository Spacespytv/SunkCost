using UnityEngine;

public class PickupSensor : MonoBehaviour
{
    private EnergyCrystal mainScript;

    void Start()
    {
        mainScript = GetComponentInParent<EnergyCrystal>();
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.CompareTag("Player"))
        {
            mainScript.InitiateCollection();
        }
    }
}