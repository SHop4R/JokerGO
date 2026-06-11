namespace JokerGO.Game.ObjectPool
{
    /// <summary>Optional hooks a pooled component can implement to reset itself.</summary>
    public interface IPoolable
    {
        void OnSpawn();
        void OnReturn();
    }
}
