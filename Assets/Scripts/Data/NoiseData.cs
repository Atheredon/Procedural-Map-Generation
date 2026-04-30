using UnityEngine;

[CreateAssetMenu]
public class HeightMapSettings : UpdateableData
{
    public NoiseSettings noiseSettings;

    public float hightMultiplier;
    public AnimationCurve heightCurve;
    public bool useFallofMap;

    public float minHeight
    {
        get
        {
            return hightMultiplier * heightCurve.Evaluate(0);
        }
    }

    public float maxHeight
    {
        get
        {
            return hightMultiplier * heightCurve.Evaluate(1);
        }
    }

    protected override void OnValidate()
    {
        noiseSettings.ValidateValues();

        base.OnValidate();
    }
}
