using UnityEngine;

public class TextureOffsetRandomizer : MonoBehaviour
{
    [Header("Settings")]
    [SerializeField] private string texturePropertyName = "_MainTex";
    [SerializeField] private bool randomizeX = true;
    [SerializeField] private bool randomizeY = false;

    private void Start()
    {
        Renderer ren = GetComponent<Renderer>();

        if (ren == null)
        {
            Debug.LogWarning($"{gameObject.name} has no Renderer to offset textures on!");
            return;
        }

        Material mat = ren.material;

        float offsetX = randomizeX ? Random.Range(0f, 1f) : mat.GetTextureOffset(texturePropertyName).x;
        float offsetY = randomizeY ? Random.Range(0f, 1f) : mat.GetTextureOffset(texturePropertyName).y;

        mat.SetTextureOffset(texturePropertyName, new Vector2(offsetX, offsetY));
    }
}