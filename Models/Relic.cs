namespace NightreignRelicEditor.Models;

public class Relic
{
    public RelicEffect Slot1 { get; set; } = new();
    public RelicEffect Slot2 { get; set; } = new();
    public RelicEffect Slot3 { get; set; } = new();

    public bool IsDeepRelic { get; set; } = false;
    
    public RelicEffect CurseSlot1 { get; set; } = new(){ EffectId = 6_820_000 };
    public RelicEffect CurseSlot2 { get; set; } = new(){ EffectId = 6_820_000 };
    public RelicEffect CurseSlot3 { get; set; } = new(){ EffectId = 6_820_000 };

    public void SortEffects()
    {
        
    }
}
