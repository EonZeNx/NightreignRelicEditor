namespace NightreignRelicEditor.Models;

public class RelicEffect
{
    public uint Id { get; set; } = uint.MaxValue;
    public string Description { get; set; } = "Empty";
    public int Category { get; set; }
    public int OrderGroup { get; set; } = int.MaxValue;
    public uint Slot1Weight { get; set; }

    public bool IsDeepEffect => Id is >= 6_000_000 and < 7_000_000;
    public bool IsCurse => Id is >= 6_820_000 and < 7_000_000;
    public bool IsEmpty => Id == uint.MaxValue;

    public override string ToString()
    {
        return $"{Id}: {Description}";
    }
}