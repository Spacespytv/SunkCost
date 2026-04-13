using UnityEngine;

public class MuzzleFlash : MonoBehaviour
{
    [SerializeField] float lifeTime = 0.05f; 

    void Start()
    {
        transform.localRotation = Quaternion.Euler(0, 0, Random.Range(0, 360));
        Destroy(gameObject, lifeTime);
    }
}