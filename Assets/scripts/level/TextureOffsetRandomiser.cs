using UnityEngine;

public class TextureOffsetRandomizer : MonoBehaviour
{
    public static TextureOffsetRandomizer Instance;

    [Header("Settings")]
    [SerializeField] private string texturePropertyName = "_MainTex";
    [SerializeField] private bool randomizeX = true;
    [SerializeField] private bool randomizeY = false;

    private Material mat;

    private void Awake()
    {
        if (Instance == null) Instance = this;

        Renderer ren = GetComponent<Renderer>();
        if (ren != null)
        {
            mat = ren.material;
        }
    }

    private void Start()
    {
        RandomizeOffset();
    }

    public void RandomizeOffset()
    {
        if (mat == null) return;

        float offsetX = randomizeX ? Random.Range(0f, 1f) : mat.GetTextureOffset(texturePropertyName).x;
        float offsetY = randomizeY ? Random.Range(0f, 1f) : mat.GetTextureOffset(texturePropertyName).y;

        mat.SetTextureOffset(texturePropertyName, new Vector2(offsetX, offsetY));
    }
}