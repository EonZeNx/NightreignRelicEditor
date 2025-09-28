using System.Diagnostics;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using NightreignRelicEditor.Models;

namespace NightreignRelicEditor.Views.Controls;

partial class RelicData : UserControl
{
    public static readonly DependencyProperty RelicManagerProperty =
        DependencyProperty.Register(nameof(RelicManager), typeof(RelicManager), typeof(RelicData), new PropertyMetadata(null));
    public RelicManager? RelicManager
    {
        get => (RelicManager) GetValue(RelicManagerProperty);
        set => SetValue(RelicManagerProperty, value);
    }
    
    public static readonly DependencyProperty RelicSlotProperty =
        DependencyProperty.Register(nameof(RelicSlot), typeof(uint), typeof(RelicData), new PropertyMetadata(uint.MinValue));
    public uint RelicSlot
    {
        get => (uint) GetValue(RelicSlotProperty);
        set => SetValue(RelicSlotProperty, value);
    }

    public bool IsActive { get; set; } = true;

    // slot 4 - 6 inclusive
    public bool IsDeepRelic => RelicSlot > 2;
    public string RelicName => $"{(IsDeepRelic ? "Deep " : "")}Relic {RelicSlot + 1}";
    
    
    protected TextBlock[] relicEffectTextBlocks;
    protected TextBlock[] relicCurseTextBlocks;
    protected Button[] clearEffectButtons;
    
    
    public RelicData()
    {
        InitializeComponent();
        
        relicEffectTextBlocks = [textSlot1, textSlot2, textSlot3];
        relicCurseTextBlocks = [textSlot1Curse, textSlot2Curse, textSlot3Curse];
        clearEffectButtons = [buttonSlot1Clear, buttonSlot2Clear, buttonSlot3Clear];
        
        buttonSlot1Clear.Click += (sender, e) => RemoveRelicEffect(0);
        buttonSlot2Clear.Click += (sender, e) => RemoveRelicEffect(1);
        buttonSlot3Clear.Click += (sender, e) => RemoveRelicEffect(2);
    }
    
    private void RemoveRelicEffect(uint slot)
    {
        if (RelicManager is null)
            return;
        
        Debug.Print("relic " + RelicSlot + " slot " + slot);
        RelicManager.RemoveRelicEffect(RelicSlot, slot);
        UpdateUIElements();
    }
    

    private void VerifyRelic()
    {
        if (RelicManager is null)
            return;
        
        var errors = RelicManager.VerifyRelic(RelicSlot);

        for (uint slot = 0; slot < 3; slot++)
        {
            switch (errors[slot])
            {
                case RelicErrors.Legitimate:
                    relicEffectTextBlocks[slot].Foreground = Brushes.Black;
                    relicEffectTextBlocks[slot].ToolTip = null;
                    break;
                case RelicErrors.NotRelicEffect:
                    SetTextVerify(slot, "Effect is not a valid relic effect.", Brushes.Red);
                    break;
                case RelicErrors.MultipleFromCategory:
                    SetTextVerify(slot, "This effect has the same category as another effect in this relic.", Brushes.Red);
                    break;
                case RelicErrors.UniqueRelicEffect:
                    SetTextVerify(slot, "Relic effect is only for special unique relics.", Brushes.Orange);
                    break;
            }
        }
    }
    
    private void SetTextVerify(uint slot, string errorText, Brush colour)
    {
        relicEffectTextBlocks[slot].Foreground = colour;
        relicEffectTextBlocks[slot].ToolTip = errorText;
    }
    
    public void UpdateUIElements()
    {
        if (RelicManager is null)
            return;
        
        for (uint x = 0; x < 3; x++)
        {
            relicEffectTextBlocks[x].Text = RelicManager.GetRelicEffectDescription(RelicSlot, x);
            relicCurseTextBlocks[x].Text = RelicManager.GetRelicEffectDescription(RelicSlot, x, true);

            var effectId = RelicManager.GetRelicEffectId(RelicSlot, x);

            clearEffectButtons[x].Visibility = effectId == 0xFFFFFFFF
                ? Visibility.Hidden : Visibility.Visible;
        }

        VerifyRelic();
    }
}