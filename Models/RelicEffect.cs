namespace NightreignRelicEditor.Models;

public class RelicEffect
{
    public uint EffectId { get; set; }
    public string Description { get; set; } = "-";
    public int Category { get; set; }
    public int OrderGroup { get; set; }
    public uint Slot1Weight { get; set; }

    public bool IsDeepEffect => EffectId is >= 6_000_000 and < 7_000_000;
    public bool IsCurse => EffectId is >= 6_820_000 and < 7_000_000;
}