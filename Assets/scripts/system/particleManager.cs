using UnityEngine;

public class ParticleManager : MonoBehaviour
{
    public static ParticleManager Instance; 

    [System.Serializable]
    public struct ParticleEffect
    {
        public string name;
        public GameObject prefab;
    }

    [SerializeField] private ParticleEffect[] effects;

    private void Awake()
    {
        Instance = this;
    }

    public void PlayEffect(string effectName, Vector2 position, Quaternion rotation)
    {
        foreach (var effect in effects)
        {
            if (effect.name == effectName)
            {
                Instantiate(effect.prefab, position, rotation);
                return;
            }
        }
        Debug.LogWarning("Effect " + effectName + " not found in ParticleManager!");
    }
}