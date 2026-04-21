using UnityEngine;

[CreateAssetMenu]
public class TerrainData : UpdateableData
{
    public float meshHightMultiplier;
    public AnimationCurve meshHeightCurve;
    public bool useFallofMap;

    public float uniformScale = 2.5f;
}
