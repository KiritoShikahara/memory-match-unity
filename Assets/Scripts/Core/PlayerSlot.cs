namespace MemoryMatch.Core
{
    public enum SeatKind { Human, Cpu, None }

    public enum CpuLevel { Weak, Normal, Strong }

    public sealed class PlayerSlot
    {
        public PlayerSlot(string name, SeatKind kind, CpuLevel level = CpuLevel.Normal)
        {
            Name = name;
            Kind = kind;
            Level = level;
        }

        public string Name { get; }
        public SeatKind Kind { get; }
        public CpuLevel Level { get; }
    }
}
