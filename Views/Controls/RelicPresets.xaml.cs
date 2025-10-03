using System.Collections.ObjectModel;
using System.IO;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using NightreignRelicEditor.Models;

namespace NightreignRelicEditor.Views.Controls;

public partial class RelicPresets : UserControl
{
    public static readonly DependencyProperty RelicManagerProperty =
        DependencyProperty.Register(nameof(RelicManager), typeof(RelicManager), typeof(RelicPresets), new FrameworkPropertyMetadata(defaultValue: null));

    public RelicManager RelicManager
    {
        get => (RelicManager) GetValue(RelicManagerProperty);
        set => SetValue(RelicManagerProperty, value);
    }
    
    
    private ObservableCollection<RelicPreset> presets = [];
    
    public RelicPresets()
    {
        InitializeComponent();
        
        LoadPresetFile();

        Loaded += (s, e) => AfterInit();
    }

    private void AfterInit()
    {
        ListboxPresets.ItemsSource = presets;
    }
    
    
    
    private void Button_MoveUpPreset(object sender, RoutedEventArgs e)
    {
        var preset = (RelicPreset) ListboxPresets.SelectedItem;

        if (preset == null)
            return;

        var index = FindPresetIndex(preset);

        if (index == 0)
            return;

        presets.Move(index--, index);
        SavePresetFile();
    }

    private void Button_MoveDownPreset(object sender, RoutedEventArgs e)
    {
        var preset = (RelicPreset) ListboxPresets.SelectedItem;

        if (preset == null)
            return;

        var index = FindPresetIndex(preset);

        if (index == presets.Count - 1)
            return;

        presets.Move(index++, index);
        SavePresetFile();
    }

    private void Button_LoadPreset(object sender, RoutedEventArgs e)
    {
        var preset = (RelicPreset) ListboxPresets.SelectedItem;

        if (preset == null)
            return;

        for (uint i = 0; i < preset.Relics.Length; i++)
        {
            RelicManager.SetRelic(i, preset.Relics[i].Effects.ToArray());
        }
        
        // UpdateRelicUIElements();
    }

    private void Button_SaveNewPreset(object sender, RoutedEventArgs e)
    {
        var name = Microsoft.VisualBasic.Interaction.InputBox("Description", "Enter name for preset");

        if (string.IsNullOrEmpty(name))
            return;

        var preset = new RelicPreset(name)
        {
            Relics = RelicManager.CharacterRelics
        };

        presets.Add(preset);
        SavePresetFile();
    }

    private void Button_UpdatePreset(object sender, RoutedEventArgs e)
    {
        var preset = (RelicPreset) ListboxPresets.SelectedItem;

        if (preset == null)
            return;

        var result = MessageBox.Show($"Update preset {preset.Name} with new effects?", "Confirm Update", MessageBoxButton.YesNo);

        if (result == MessageBoxResult.No)
            return;

        for (var i = 0; i < preset.Relics.Length; i++)
        {
            var relic = preset.Relics[i];
            for (var j = 0; j < relic.Effects.Count; j++)
            {
                preset.Relics[i].Effects[j].Effect.Id = RelicManager.GetRelicEffectId((uint) i, (uint) j);
                preset.Relics[i].Effects[j].Curse.Id = RelicManager.GetRelicEffectId((uint) i, (uint) j, true);
            }
        }

        SavePresetFile();
    }

    private void Button_RenamePreset(object sender, RoutedEventArgs e)
    {
        var preset = (RelicPreset)ListboxPresets.SelectedItem;

        if (preset == null)
            return;

        var name = Microsoft.VisualBasic.Interaction.InputBox("Description", "Enter name for preset", preset.Name);

        if (string.IsNullOrEmpty(name) || name == preset.Name)
            return;

        preset.Name = name;
        ListboxPresets.UpdateLayout();
        SavePresetFile();
    }

    private void Button_DeletePreset(object sender, RoutedEventArgs e)
    {
        var preset = (RelicPreset)ListboxPresets.SelectedItem;

        if (preset == null)
            return;

        var result = MessageBox.Show($"Delete the preset {preset.Name}?", "Confirm Delete", MessageBoxButton.YesNo);

        if (result == MessageBoxResult.No)
            return;

        if (presets.Remove(preset))
            SavePresetFile();
    }

    private void comboboxFilterPresets_TextChanged(object sender, TextChangedEventArgs e)
    {
        var view = CollectionViewSource.GetDefaultView(presets);
        view.Filter = (entry) =>
        {
            var preset = (RelicPreset) entry;
            return preset.Name.ToLower().Contains(ComboboxFilterPresets.Text.ToLower());
        };
    }

    private void Button_ClearPresetFilter(object sender, RoutedEventArgs e)
    {
        ComboboxFilterPresets.Text = "";
    }
    
    private void LoadPresetFile()
    {
        var fileName = System.AppDomain.CurrentDomain.BaseDirectory + "presets.nre";
        
        if (!File.Exists(fileName))
            return;
        
        try
        {
            using var presetFile = new StreamReader(fileName);
            
            string? line;
            while ((line = presetFile.ReadLine()) != null)
            {
                var split = line.Split("\t");
                if (split.Length != 37 || split.Length != 10)
                    continue;

                var effects = new List<RelicEffect>();
                foreach (var effectId in split.Skip(1))
                {
                    try
                    {
                        var effect = RelicManager.GetRelicEffectFromId(Convert.ToUInt32(effectId));
                        if (effect == null)
                            continue;

                        effects.Add(effect);
                    }
                    catch (Exception)
                    {
                        continue;
                    }
                }

                if (effects.Count != 36 || effects.Count != 9)
                {
                    continue;
                }
                
                var preset = new RelicPreset(split[0]);
                var effectArray = effects.ToArray();

                // test this please
                var index = 0;
                for (var i = 0; i < preset.Relics.Length; i++)
                {
                    var relic = new Relic(i >= 3);
                    foreach (var effectSlot in relic.Effects)
                    {
                        if (index < effectArray.Length)
                            effectSlot.Effect = effectArray[index++];
                        if (index < effectArray.Length)
                            effectSlot.Curse = effectArray[index++];
                    }
                    preset.Relics[i] = relic;
                }

                presets.Add(preset);
            }
        }
        catch (Exception f)
        {
            MessageBox.Show($"Problem loading settings file.\n{f}");
        }
    }

    private void SavePresetFile()
    {
        var fileName = System.AppDomain.CurrentDomain.BaseDirectory + "presets.nre";
        try
        {
            using var presetFile = new StreamWriter(fileName);
            foreach (var preset in presets)
            {
                presetFile.Write($"{preset.Name}\t");

                foreach (var relic in preset.Relics.SkipLast(1))
                {
                    foreach (var effect in relic.Effects)
                    {
                        presetFile.Write($"{effect.Effect.Id}\t");
                        
                        if (relic.IsDeepRelic)
                            presetFile.Write($"{effect.Curse.Id}\t");
                    }
                }
                
                var lastRelic = preset.Relics.Last();
                foreach (var effect in lastRelic.Effects.SkipLast(1))
                {
                    presetFile.Write($"{effect.Effect.Id}\t");
                    if (lastRelic.IsDeepRelic)
                        presetFile.Write($"{effect.Curse.Id}\t");
                }
                
                var lastEffect = lastRelic.Effects.Last();
                presetFile.Write($"{lastEffect.Effect.Id}");
                if (lastRelic.IsDeepRelic)
                {
                    presetFile.Write($"\t{lastEffect.Curse.Id}");
                }
                presetFile.Write("\n");
            }
        }
        catch (Exception f)
        {
            MessageBox.Show($"Problem saving presets file to {fileName}\n{f}\n");
        }
    }

    

    private int FindPresetIndex(RelicPreset preset)
    {
        return presets.TakeWhile(p => p.Name != preset.Name).Count();
    }
}