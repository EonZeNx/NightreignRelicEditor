namespace NightreignRelicEditor.Models;

public class Relic
{
    public List<RelicEffectSlot> Effects { get; set; } = [new(), new(), new()];
    public bool IsDeepRelic { get; set; } = false;

    public bool AddEffect(RelicEffect effect,  bool isCurse = false)
    {
        if (Effects.Count >= 3)
            return false;

        var relicSlot =  new RelicEffectSlot();
        
        if (isCurse)
        {
            if (!IsDeepRelic || !effect.IsCurse)
                return false;

            relicSlot.Curse = effect;
        }
        else
        {
            relicSlot.Effect = effect;
        }
        
        Effects.Add(relicSlot);
        return true;
    }

    public bool SetEffect(RelicEffect effect, uint slot, bool isCurse = false)
    {
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
        Effects = Effects.OrderBy(x => x.Effect.OrderGroup).ThenBy(x => x.Effect.Id).ToList();
    }
}
