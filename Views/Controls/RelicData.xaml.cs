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
    public RelicManager RelicManager
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
    
    public string RelicName => $"Relic {RelicSlot + 1}";
    
    
    protected TextBlock[] relicTextBlock;
    protected Button[] clearEffectButtons;
    
    
    public RelicData()
    {
        InitializeComponent();
        
        relicTextBlock = [textSlot1, textSlot2, textSlot3];
        clearEffectButtons = [buttonClearSlot1, buttonClearSlot2, buttonClearSlot3];
        
        buttonClearSlot1.Click += (sender, e) => RemoveRelicEffect(0);
        buttonClearSlot2.Click += (sender, e) => RemoveRelicEffect(1);
        buttonClearSlot3.Click += (sender, e) => RemoveRelicEffect(2);
    }
    
    private void RemoveRelicEffect(uint slot)
    {
        Debug.Print("relic " + RelicSlot + " slot " + slot);
        RelicManager.RemoveRelicEffect(RelicSlot, slot);
        UpdateUIElements();
    }
    

    private void VerifyRelic()
    {
        var errors = RelicManager.VerifyRelic(RelicSlot);

        for (uint slot = 0; slot < 3; slot++)
        {
            switch (errors[slot])
            {
                case RelicErrors.Legitimate:
                    relicTextBlock[slot].Foreground = Brushes.Black;
                    relicTextBlock[slot].ToolTip = null;
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
        relicTextBlock[slot].Foreground = colour;
        relicTextBlock[slot].ToolTip = errorText;
    }
    
    public void UpdateUIElements()
    {
        for (uint x = 0; x < 3; x++)
        {
            relicTextBlock[x].Text = RelicManager.GetEffectDescription(RelicSlot, x);

            var effectId = RelicManager.GetRelicEffectId(RelicSlot, x);

            clearEffectButtons[x].Visibility = effectId == 0xFFFFFFFF
                ? Visibility.Hidden : Visibility.Visible;
        }

        VerifyRelic();
    }
}