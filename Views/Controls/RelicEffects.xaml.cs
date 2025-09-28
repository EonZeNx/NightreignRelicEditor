using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using NightreignRelicEditor.Models;
using NightreignRelicEditor.ViewModels;

namespace NightreignRelicEditor.Views.Controls;

public partial class RelicEffects : UserControl
{
    public static readonly DependencyProperty RelicManagerProperty =
        DependencyProperty.Register(nameof(RelicManager), typeof(RelicManager), typeof(RelicEffects), new PropertyMetadata(null, OnRelicManagerChanged));

    public RelicManager? RelicManager
    {
        get => (RelicManager) GetValue(RelicManagerProperty);
        set => SetValue(RelicManagerProperty, value);
    }
    
    private static void OnRelicManagerChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
        if (d is RelicEffects { DataContext: RelicEffectsViewModel vm })
            vm.RelicManager = (RelicManager) e.NewValue;
    }


    public RelicEffects()
    {
        InitializeComponent();
        
        Loaded += (s, e) => AfterInit();
    }

    
    protected void AfterInit()
    {
        listviewRelicEffects.ItemsSource = RelicManager?.AllRelicEffects;
        FilterRelicEffectsBox();
    }

    private void TextboxFilterEffects_OnTextChanged(object sender, TextChangedEventArgs e)
    {
        FilterRelicEffectsBox();
    }

    private void ToggleShowUniqueEffects(object sender, RoutedEventArgs e)
    {
        FilterRelicEffectsBox();
    }

    private void ClearEffectFilter(object sender, RoutedEventArgs e)
    {
        textboxFilterEffects.Text = "";
    }
    
    public void FilterRelicEffectsBox()
    {
        if (RelicManager is null)
            return;
        
        var view = CollectionViewSource.GetDefaultView(RelicManager.AllRelicEffects);
        view.Filter = (entry) =>
        {
            var re = (RelicEffect) entry;

            return re.Description.ToLower().Contains(textboxFilterEffects.Text.ToLower())
                   & RelicManager.VerifyEffectIsRelicEffect(re, (checkboxShowUnique.IsChecked ?? false));
        };
    }

    
    private void Button_SetRelicEffect(object sender, RoutedEventArgs e)
    {
        throw new NotImplementedException();
    }
}