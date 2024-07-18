using UnityEngine;

public class AudioData : ScriptableObject
{
    public AudioClip clip;
    [ReadOnly] public Beat[] beats;
}
