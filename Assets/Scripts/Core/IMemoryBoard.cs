namespace MemoryMatch.Core
{
    /// CPU が参照できる公開情報。裏向きの絵柄は VisiblePairId が -1 を返す。
    public interface IMemoryBoard
    {
        int CardCount { get; }
        GamePhase Phase { get; }
        bool IsSelectable(int index);
        CardState StateOf(int index);
        int VisiblePairId(int index);
    }
}
