using UnityEngine;

[CreateAssetMenu]
public class UpdateableData : ScriptableObject
{
    public event System.Action OnUpdate;
    public bool autoUpdate;

    protected virtual void OnValidate()
    {
        if (autoUpdate)
        {
            NotifyUpdatedValues();
        }
    }

    public void NotifyUpdatedValues()
    {
        if(OnUpdate  != null)
        {
            OnUpdate();
        }
    }
}
