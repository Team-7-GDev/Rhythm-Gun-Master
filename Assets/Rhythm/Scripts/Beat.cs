[System.Serializable]
public struct Beat
{
    public float strength;
    public float time;

    public Beat(float strength, float time)
    {
        this.strength = strength;
        this.time = time;
    }
}
