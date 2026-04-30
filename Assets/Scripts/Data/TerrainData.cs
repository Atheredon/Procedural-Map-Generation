using UnityEngine;

[CreateAssetMenu]
public class MeshSettings : UpdateableData
{
    public const int numberOfSuportedLODs = 5;// Limit lod levels to 0 - 4 to be able to create diffrent sized chunks
    public const int numberOfSuportedChunkSizes = 9;
    //Max mesh size in unity 255^2 to make an square and make sure vertices count is divisible by even numbers max is 240, as the divisers are 1,2,4,6,8 these are the all of the meaningful values that is smaller than 240 thats divisible by all of them
    public static readonly int[] suportedChunkSizes = { 48, 72, 96, 120, 144, 168, 192, 216, 240 };

    public float meshScale = 2.5f;

    [Range(0, numberOfSuportedChunkSizes - 1)]
    public int chunkSizeIndex;

    // Number of vertecies per line of the mesh rendered at LOD = 0. Includes the 2 extra vertecies thats only used for calculating normals.
    public int numberOfVerticesPerLine { get => suportedChunkSizes[chunkSizeIndex] + 1; }

    public float meshWorldSize {  get => (numberOfVerticesPerLine - 3) * meshScale;} // -1 for w -1 line count, -2 for excluding the 2 added extra
}
