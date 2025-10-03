namespace NightreignRelicEditor.Models;

public class Relic(bool isDeepRelic = false)
{
    public List<RelicEffectSlot> Effects { get; } = [new(), new(), new()];
    public bool IsDeepRelic { get; } = isDeepRelic;

    public bool SetEffect(RelicEffect effect, uint slot, bool isCurse = false)
    {
        if (!IsDeepRelic && (effect.IsCurse || isCurse))
            return false;
        
        if (slot >= Effects.Count)
            Effects.Add(new RelicEffectSlot());
        
        var currentEffect = Effects[(int)slot];
        if (isCurse)
            currentEffect.Curse = effect;
        else
            currentEffect.Effect = effect;

        return true;
    }

    public bool RemoveEffect(int slotIndex, bool isCurse = false)
    {
        if (slotIndex < 0 || slotIndex >= Effects.Count)
            return false;
        
        if (isCurse)
        {
            Effects[slotIndex].ClearCurse();

            return true;
        }
        
        Effects[slotIndex].ClearEffect();

        return true;
    }

    public void ClearSlot()
    {
        foreach (var effectSlot in Effects)
        {
            effectSlot.ClearSlot();
        }
    }

    public void SortEffects()
    {
        Effects.Sort((a, b) =>
        {
            var groupCompare = a.Effect.OrderGroup.CompareTo(b.Effect.OrderGroup);
            return groupCompare != 0 
                ? groupCompare 
                : a.Effect.Id.CompareTo(b.Effect.Id);
        });
    }
}
