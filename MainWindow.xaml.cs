using System.Collections.ObjectModel;
using System.ComponentModel;
using System.IO;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace NightreignRelicEditor;

public partial class MainWindow : Window
{
    private RelicManager relicManager;
    TextBlock[,] relicTextBlock;

    public MainWindow()
    {
        InitializeComponent();
        relicManager = new RelicManager();
        InitUI();

        relicTextBlock = new TextBlock[3, 3]
        {
            { textRelic1Slot1, textRelic1Slot2, textRelic1Slot3 },
            { textRelic2Slot1, textRelic2Slot2, textRelic2Slot3 },
            { textRelic3Slot1, textRelic3Slot2, textRelic3Slot3 },
        };
    }

    private void InitUI()
    {
        LoadPresetFile();

        listRelicEffects.ItemsSource = relicManager.relicEffects;
        listboxPresets.ItemsSource = relicPresets;

        SetUIConnectionStatus();

        if (relicManager.ConnectionStatus != RelicManager.ConnectionStates.Connected)
        {
            buttonSetRelicsInGame.IsEnabled = false;
            buttonImportRelicsFromGame.IsEnabled = false;
        }
    }

    private void SetUIConnectionStatus()
    {
        switch (relicManager.ConnectionStatus)
        {
            case RelicManager.ConnectionStates.Connected:
                textConnectionStatus.Text = "Connected";
                textConnectionStatus.Foreground = Brushes.Black;
                break;
            case RelicManager.ConnectionStates.EACDetected:
                textConnectionStatus.Text = "EAC detected";
                textConnectionStatus.Foreground = Brushes.Red;
                break;
            case RelicManager.ConnectionStates.NightreignNotFound:
                textConnectionStatus.Text = "Nightreign process not found";
                textConnectionStatus.Foreground = Brushes.Red;
                break;
            case RelicManager.ConnectionStates.ConnectedOffsetsNotFound:
                textConnectionStatus.Text = "Nightreign process found, but relic memory addresses not found";
                textConnectionStatus.Foreground = Brushes.Red;
                break;
            case RelicManager.ConnectionStates.NotConnected:
                textConnectionStatus.Text = "Not connected";
                textConnectionStatus.Foreground = Brushes.Black;
                break;
        }
    }

    private void SetRelicEffectFromList(uint relic, uint slot)
    {
        RelicEffect selected = (RelicEffect)listRelicEffects.SelectedItem;
        SetRelicEffect(relic, slot, selected.EffectID);
    }

    private void SetRelicEffect(uint relic, uint slot, uint effect)
    {
        relicManager.SetRelicEffect(relic, slot, effect);
        SetRelicEffectText(relic, slot, effect);
    }

    private void SetRelicEffectText(uint relic, uint slot, uint effect)
    {
        (string Description, bool Valid) relicText = relicManager.GetEffectDescription(effect);

        relicTextBlock[relic, slot].Text = relicText.Description;

        if (relicText.Valid == false)
            relicTextBlock[relic, slot].Foreground = Brushes.Red;
        else
            relicTextBlock[relic, slot].Foreground = Brushes.Black;
    }

    //
    // Relic slot buttons
    //

    private void Button_SetRelic1Slot1(object sender, RoutedEventArgs e)
    {
        SetRelicEffectFromList(0, 0);
    }

    private void Button_SetRelic1Slot2(object sender, RoutedEventArgs e)
    {
        SetRelicEffectFromList(0, 1);
    }

    private void Button_SetRelic1Slot3(object sender, RoutedEventArgs e)
    {
        SetRelicEffectFromList(0, 2);
    }

    private void Button_SetRelic2Slot1(object sender, RoutedEventArgs e)
    {
        SetRelicEffectFromList(1, 0);
    }

    private void Button_SetRelic2Slot3(object sender, RoutedEventArgs e)
    {
        SetRelicEffectFromList(1, 1);
    }

    private void Button_SetRelic2Slot2(object sender, RoutedEventArgs e)
    {
        SetRelicEffectFromList(1, 2);
    }

    private void Button_SetRelic3Slot1(object sender, RoutedEventArgs e)
    {
        SetRelicEffectFromList(2, 0);
    }

    private void Button_SetRelic3Slot3(object sender, RoutedEventArgs e)
    {
        SetRelicEffectFromList(2, 1);
    }

    private void Button_SetRelic3Slot2(object sender, RoutedEventArgs e)
    {
        SetRelicEffectFromList(2, 2);
    }



    //
    // Filters
    //

    private void textboxFilterEffects_TextChanged(object sender, TextChangedEventArgs e)
    {
        ICollectionView view = CollectionViewSource.GetDefaultView(relicManager.relicEffects);
        view.Filter = (entry) =>
        {
            RelicEffect re = (RelicEffect)entry;
            return re.Description.ToLower().Contains(textboxFilterEffects.Text.ToLower());
        };
    }

    private void Button_ClearEffectFilter(object sender, RoutedEventArgs e)
    {
        textboxFilterEffects.Text = "";
    }

    private void FilterValidEffects(object sender, RoutedEventArgs e)
    {
        ICollectionView view = CollectionViewSource.GetDefaultView(relicManager.relicEffects);

        view.Filter = (entry) =>
        {
            RelicEffect re = (RelicEffect)entry;
            return re.Category != 0;
        };
    }

    private void UnfilterValidEffects(object sender, RoutedEventArgs e)
    {
        ICollectionView view = CollectionViewSource.GetDefaultView(relicManager.relicEffects);

        view.Filter = (entry) =>
        {
            return true;
        };
    }

    

    //
    // Main UI Buttons
    //

    private void Button_SetRelicsInGame(object sender, RoutedEventArgs e)
    {
        for (uint x = 0; x < 3; x++)
        {
            for (uint y = 0; y < 3; y++)
            {
                relicManager.SetRelicSlotInGame(x, y, relicManager.GetRelicEffect(x, y));
            }
        }
    }

    private void Button_GetRelicsFromGame(object sender, RoutedEventArgs e)
    {
        for (uint x = 0; x < 3; x++)
        {
            for (uint y = 0; y < 3; y++)
            {
                SetRelicEffect(x, y, relicManager.GetRelicSlotInGame(x, y));
            }
        }
    }

    //
    // Preset tab UI
    //

    private void Button_MoveUpPreset(object sender, RoutedEventArgs e)
    {
        int index = FindPresetIndex(listboxPresets.SelectedItem as RelicPreset);

        if (index == 0)
            return;

        relicPresets.Move(index--, index);
        SavePresetFile();
    }

    private void Button_MoveDownPreset(object sender, RoutedEventArgs e)
    {
        int index = FindPresetIndex(listboxPresets.SelectedItem as RelicPreset);

        if (index == relicPresets.Count - 1)
            return;

        relicPresets.Move(index++, index);
        SavePresetFile();
    }

    private void Button_LoadPreset(object sender, RoutedEventArgs e)
    {
        LoadPreset();
    }

    private void Button_CreatePreset(object sender, RoutedEventArgs e)
    {
        CreatePreset();
    }


    private void Button_UpdatePreset(object sender, RoutedEventArgs e)
    {
        UpdatePreset();
    }

    private void Button_RenamePreset(object sender, RoutedEventArgs e)
    {
        RenamePreset();
    }

    private void Button_DeletePreset(object sender, RoutedEventArgs e)
    {
        DeletePreset();
    }


    private void comboboxFilterPresets_TextChanged(object sender, TextChangedEventArgs e)
    {
        ICollectionView view = CollectionViewSource.GetDefaultView(relicPresets);
        view.Filter = (entry) =>
        {
            RelicPreset re = (RelicPreset)entry;
            return re.Name.ToLower().Contains(comboboxFilterPresets.Text.ToLower());
        };
    }

    private void Button_ClearPresetFilter(object sender, RoutedEventArgs e)
    {
        comboboxFilterPresets.Text = "";
    }



    //
    // Presets
    // 

    ObservableCollection<RelicPreset> relicPresets = new ObservableCollection<RelicPreset>();

    class RelicPreset : INotifyPropertyChanged
    {
        private string name = "";
        public event PropertyChangedEventHandler PropertyChanged;

        private void NotifyPropertyChanged([CallerMemberName] String propertyName = "")
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

        public string Name
        {
            get
            {
                return name;
            }

            set
            {
                name = value;
                NotifyPropertyChanged();
            }
        }

        public uint[,] RelicSlot { get; set; }

        public RelicPreset(string name)
        {
            Name = name;
            RelicSlot = new uint[3, 3];
        }
    }

    private void LoadPresetFile()
    {
        try
        {
            string file = System.AppDomain.CurrentDomain.BaseDirectory + "presets.nre";

            if (!File.Exists(file))
                return;

            using (StreamReader sr = new StreamReader(file))
            {
                string line;
                while ((line = sr.ReadLine()) != null)
                {
                    string[] split = line.Split("\t");
                    if (split.Length == 10)
                    {
                        RelicPreset relic = new RelicPreset(split[0]);

                        relic.RelicSlot[0, 0] = Convert.ToUInt32(split[1]);
                        relic.RelicSlot[0, 1] = Convert.ToUInt32(split[2]);
                        relic.RelicSlot[0, 2] = Convert.ToUInt32(split[3]);

                        relic.RelicSlot[1, 0] = Convert.ToUInt32(split[4]);
                        relic.RelicSlot[1, 1] = Convert.ToUInt32(split[5]);
                        relic.RelicSlot[1, 2] = Convert.ToUInt32(split[6]);

                        relic.RelicSlot[2, 0] = Convert.ToUInt32(split[7]);
                        relic.RelicSlot[2, 1] = Convert.ToUInt32(split[8]);
                        relic.RelicSlot[2, 2] = Convert.ToUInt32(split[9]);

                        relicPresets.Add(relic);
                    }
                }
            }
        }
        catch (Exception f)
        {
            MessageBox.Show("Problem loading settings file.");
        }
    }

    private void SavePresetFile()
    {
        string file = System.AppDomain.CurrentDomain.BaseDirectory + "presets.nre";
        try
        {
            using (StreamWriter sw = new StreamWriter(file))
            {
                foreach (RelicPreset preset in relicPresets)
                {
                    sw.WriteLine(preset.Name+ "\t"
                        + preset.RelicSlot[0, 0] + "\t"
                        + preset.RelicSlot[0, 1] + "\t"
                        + preset.RelicSlot[0, 2] + "\t"
                        + preset.RelicSlot[1, 0] + "\t"
                        + preset.RelicSlot[1, 1] + "\t"
                        + preset.RelicSlot[1, 2] + "\t"
                        + preset.RelicSlot[2, 0] + "\t"
                        + preset.RelicSlot[2, 1] + "\t"
                        + preset.RelicSlot[2, 2]);
                }
            }
        }
        catch (Exception e)
        {
            MessageBox.Show("Problem saving presets file to " + file);
        }
    }

    private void CreatePreset()
    {
        string name = Microsoft.VisualBasic.Interaction.InputBox("Description", "Enter name for preset");

        if (string.IsNullOrEmpty(name))
            return;

        RelicPreset relic = new RelicPreset(name);

        for (uint x = 0; x < 3; x++)
        {
            for (uint y = 0; y < 3; y++)
            {
                relic.RelicSlot[x, y] = relicManager.GetRelicEffect(x, y);
            }
        }
        relicPresets.Add(relic);

        SavePresetFile();
    }

    private void LoadPreset()
    {
        RelicPreset relic = (RelicPreset)listboxPresets.SelectedItem;

        if (relic == null)
            return;

        for (uint x = 0; x < 3; x++)
        {
            for (uint y = 0; y < 3; y++)
            {
                SetRelicEffect(x, y, relic.RelicSlot[x, y]);
            }
        }
    }

    private void UpdatePreset()
    {
        MessageBoxResult result = MessageBox.Show("Update preset " + (listboxPresets.SelectedItem as RelicPreset).Name + " with new effects?", "Corfirm Update", MessageBoxButton.YesNo);

        if (result == MessageBoxResult.No)
            return;

        for (uint x = 0; x < 3; x++)
        {
            for (uint y = 0; y < 3; y++)
            {
                (listboxPresets.SelectedItem as RelicPreset).RelicSlot[x, y] = relicManager.GetRelicEffect(x, y);
            }
        }

        SavePresetFile();
    }

    private void RenamePreset()
    {
        string name = Microsoft.VisualBasic.Interaction.InputBox("Description", "Enter name for preset", (listboxPresets.SelectedItem as RelicPreset).Name);

        if (string.IsNullOrEmpty(name) || name == (listboxPresets.SelectedItem as RelicPreset).Name)
            return;

        (listboxPresets.SelectedItem as RelicPreset).Name = name;
        listboxPresets.UpdateLayout();
        SavePresetFile();
    }

    private void DeletePreset()
    {
        MessageBoxResult result = MessageBox.Show("Delete the preset " + (listboxPresets.SelectedItem as RelicPreset).Name + "?", "Corfirm Delete", MessageBoxButton.YesNo);

        if (result == MessageBoxResult.No)
            return;

        if (relicPresets.Remove((RelicPreset)listboxPresets.SelectedItem))
            SavePresetFile();
    }

    private int FindPresetIndex(RelicPreset rp)
    {
        int i = 0;

        foreach (RelicPreset r in relicPresets)
        {
            if (r.Name == rp.Name)
                break;
            else
                i++;
        }

        return i;
    }
}