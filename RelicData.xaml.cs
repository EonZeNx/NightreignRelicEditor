using System.Windows;
using System.Windows.Controls;

namespace NightreignRelicEditor;

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
        DependencyProperty.Register(nameof(RelicSlot), typeof(int), typeof(RelicData), new PropertyMetadata(0));
    public int RelicSlot
    {
        get => (int) GetValue(RelicManagerProperty);
        set => SetValue(RelicManagerProperty, value);
    }
    
    
    TextBlock[] relicTextBlock;
    Button[] clearEffectButtons;
    
    
    public RelicData()
    {
        InitializeComponent();
        
        relicTextBlock = [textSlot1, textSlot2, textSlot3];
        clearEffectButtons = [buttonClearSlot1, buttonClearSlot2, buttonClearSlot3];
    }
}