using UnityEngine;

public class MapPreview : MonoBehaviour
{
    public Renderer textureRenderer;
    public MeshFilter meshFilter;
    public MeshRenderer meshRenderer;

    public enum DrawMode { NoiseMap, Mesh, FallofMap }
    public DrawMode drawMode;

    [Range(0, MeshSettings.numberOfSuportedLODs - 1)]
    public int PreviewLOD;

    public bool autoUpdate;

    public MeshSettings meshSettings;
    public HeightMapSettings heightMapSettings;

    public void DrawTexture(Texture2D texture)
    {
        textureRenderer.sharedMaterial.mainTexture = texture;
        textureRenderer.transform.localScale = new Vector3(texture.width, 1, texture.height) /10f;

        textureRenderer.gameObject.SetActive(true);
        meshFilter.gameObject.SetActive(false);
    }

    public void DrawMesh(MeshData meshData)
    {
        meshFilter.sharedMesh = meshData.CreateMesh();

        textureRenderer.gameObject.SetActive(false);
        meshFilter.gameObject.SetActive(true);
    }


    public void DrawMapInEditor()
    {
        if (heightMapSettings == null || meshSettings == null)
            return;

        HeightMap heightMap = HeightMapGenerator.GenerateHeightMap(meshSettings.numberOfVerticesPerLine, meshSettings.numberOfVerticesPerLine, heightMapSettings, Vector2.zero);

        if (drawMode == DrawMode.NoiseMap)
        {
            DrawTexture(TextureGenerator.TextureFromHeightMap(heightMap));
        }
        else if (drawMode == DrawMode.Mesh)
        {
            DrawMesh(MeshGenerator.GenerateTerrainMesh(heightMap.values, meshSettings, PreviewLOD));
        }
        else if (drawMode == DrawMode.FallofMap)
        {
            DrawTexture(TextureGenerator.TextureFromHeightMap(new HeightMap(FallofGenerator.GenerateFallofMap(meshSettings.numberOfVerticesPerLine),0,1)));
        }
    }

    void OnValuesUpdate()
    {
        if (!Application.isPlaying)
        {
            DrawMapInEditor();
        }
    }
    private void OnValidate()
    {
        if (meshSettings != null)
        {
            meshSettings.OnUpdate -= OnValuesUpdate; //to prevent suscribeing over and over
            meshSettings.OnUpdate += OnValuesUpdate;
        }
        if (heightMapSettings != null)
        {
            heightMapSettings.OnUpdate -= OnValuesUpdate; //to prevent suscribeing over and over
            heightMapSettings.OnUpdate += OnValuesUpdate;
        }
    }

}
