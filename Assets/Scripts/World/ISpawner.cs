public struct SpawnOccupancy
{
    public float MinX;
    public float MaxX;

    public bool IsValid => MaxX > MinX;

    public static SpawnOccupancy Point(float x, float halfWidth = 0.5f)
    {
        return new SpawnOccupancy { MinX = x - halfWidth, MaxX = x + halfWidth };
    }

    public static SpawnOccupancy Invalid => default;
}

public interface ISpawner
{
    bool CanSpawn();
    SpawnOccupancy Spawn(float position);
}
