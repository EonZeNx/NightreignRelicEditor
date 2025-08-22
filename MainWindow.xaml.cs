using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Diagnostics;
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
using System.Windows.Media.Effects;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using static System.Net.Mime.MediaTypeNames;

namespace NightreignRelicEditor;

public partial class MainWindow : Window
{
    private RelicManager relicManager;
    TextBlock[,] relicTextBlock;
    Button[,] clearEffectButtons;
    CheckBox[] activeRelics;

    System.Windows.Threading.DispatcherTimer monitorTimer = new System.Windows.Threading.DispatcherTimer();

    public MainWindow()
    {
        InitializeComponent();

        

        relicManager = new RelicManager();
        InitUI();
        LoadSettingsFile();
        LoadPresetFile();

        monitorTimer.Tick += monitorTimer_Tick;
        monitorTimer.Interval = TimeSpan.FromMilliseconds(1000);

        Closing += ExitProgram;

        if ((bool)checkboxAutoconnect.IsChecked)
            Connect();
    }

    private void InitUI()
    {
        listviewRelicEffects.ItemsSource = relicManager.relicEffects;
        listboxPresets.ItemsSource = relicPresets;
        FilterRelicEffectsBox();
        SetUIConnectionStatus();

        if (relicManager.ConnectionStatus != ConnectionStates.Connected)
        {
            buttonSetRelicsInGame.IsEnabled = false;
            buttonImportRelicsFromGame.IsEnabled = false;
        }

        relicTextBlock = new TextBlock[3, 3]
        {
            { textRelic1Slot1, textRelic1Slot2, textRelic1Slot3 },
            { textRelic2Slot1, textRelic2Slot2, textRelic2Slot3 },
            { textRelic3Slot1, textRelic3Slot2, textRelic3Slot3 },
        };

        clearEffectButtons = new Button[3, 3]
        {
            { buttonClearRelic1Slot1, buttonClearRelic1Slot2, buttonClearRelic1Slot3 },
            { buttonClearRelic2Slot1, buttonClearRelic2Slot2, buttonClearRelic2Slot3 },
            { buttonClearRelic3Slot1, buttonClearRelic3Slot2, buttonClearRelic3Slot3 },
        };

        activeRelics = new CheckBox[]
        {
            checkRelic1Active, checkRelic2Active, checkRelic3Active,
        };

        buttonClearRelic1Slot1.Click += (sender, e) => RemoveRelicEffect(0, 0);
        buttonClearRelic1Slot2.Click += (sender, e) => RemoveRelicEffect(0, 1);
        buttonClearRelic1Slot3.Click += (sender, e) => RemoveRelicEffect(0, 2);
        buttonClearRelic2Slot1.Click += (sender, e) => RemoveRelicEffect(1, 0);
        buttonClearRelic2Slot2.Click += (sender, e) => RemoveRelicEffect(1, 1);
        buttonClearRelic2Slot3.Click += (sender, e) => RemoveRelicEffect(1, 2);
        buttonClearRelic3Slot1.Click += (sender, e) => RemoveRelicEffect(2, 0);
        buttonClearRelic3Slot2.Click += (sender, e) => RemoveRelicEffect(2, 1);
        buttonClearRelic3Slot3.Click += (sender, e) => RemoveRelicEffect(2, 2);

        for (uint x = 0; x < 3; x++)
            UpdateRelicUIElements(x);
    }

    private void Connect()
    {
        buttonConnect.IsEnabled = false;
        relicManager.ConnectToNightreign();

        if (relicManager.ConnectionStatus == ConnectionStates.Connected)            
            monitorTimer.Start();
        else
            buttonConnect.IsEnabled = true;

        SetUIConnectionStatus();
    }

    private void monitorTimer_Tick(object? sender, EventArgs e)
    {
        ConnectionStates cs = relicManager.ConnectionStatus;

        if (cs != ConnectionStates.Connected)
        {
            monitorTimer.Stop();
            SetUIConnectionStatus();
        }
    }

    private void SetUIConnectionStatus()
    {
        if (relicManager.ConnectionStatus == ConnectionStates.Connected)
        {
            buttonConnect.IsEnabled = false;
            buttonSetRelicsInGame.IsEnabled = true;
            buttonImportRelicsFromGame.IsEnabled = true;

            textConnectionStatus.Foreground = Brushes.Black;
            textConnectionStatus.Text = "Connected";
        }
        else
        {
            buttonConnect.IsEnabled = true;
            buttonSetRelicsInGame.IsEnabled = false;
            buttonImportRelicsFromGame.IsEnabled = false;

            textConnectionStatus.Foreground = Brushes.Red;
        }

        switch (relicManager.ConnectionStatus)
        {
            
            case ConnectionStates.EACDetected:
                textConnectionStatus.Text = "EAC detected";
                break;
            case ConnectionStates.NightreignNotFound:
                textConnectionStatus.Text = "Nightreign process not found";
                break;
            case ConnectionStates.ConnectedOffsetsNotFound:
                textConnectionStatus.Text = "Nightreign process found, but relic memory addresses not found";
                break;
            case ConnectionStates.ConnectionLost:
                textConnectionStatus.Text = "Nightreign connection lost";
                break;
            case ConnectionStates.NotConnected:
                textConnectionStatus.Text = "Not connected";
                textConnectionStatus.Foreground = Brushes.Black;
                break;
        }
    }

    private void ExitProgram(object sender, EventArgs e)
    {
        SaveSettingsFile();
    }

    private void AddRelicEffectFromList(uint relic)
    {
        RelicEffect selected = (RelicEffect)listviewRelicEffects.SelectedItem;

        if (selected != null)
            AddRelicEffect(relic, selected);
    }

    private void AddRelicEffect(uint relic, RelicEffect effect)
    {
        relicManager.AddRelicEffect(relic, effect);
        UpdateRelicUIElements(relic);
        
    }

    private void RemoveRelicEffect(uint relic, uint slot)
    {
        Debug.Print("relic " + relic + " slot " + slot);
        relicManager.RemoveRelicEffect(relic, slot);
        UpdateRelicUIElements(relic);

    }

    private void UpdateRelicUIElements(uint relic)
    {
        for (uint x = 0; x < 3; x++)
        {
            relicTextBlock[relic, x].Text = relicManager.GetEffectDescription(relic, x);

            uint effectId = relicManager.GetRelicEffectId(relic, x);

            if (effectId == 0xFFFFFFFF)
                clearEffectButtons[relic, x].Visibility = Visibility.Hidden;
            else
                clearEffectButtons[relic, x].Visibility = Visibility.Visible;
        }

        VerifyRelic(relic);
    }

    //
    // Verification
    //

    private void VerifyRelic(uint relic)
    {
        RelicErrors[] errors = relicManager.VerifyRelic(relic);

        for (uint slot = 0; slot < 3; slot++)
        {
            switch (errors[slot])
            {
                case RelicErrors.Legitimate:
                    relicTextBlock[relic, slot].Foreground = Brushes.Black;
                    relicTextBlock[relic, slot].ToolTip = null;
                    break;
                case RelicErrors.NotRelicEffect:
                    SetTextVerify(relic, slot, "Effect is not a valid relic effect.", Brushes.Red);
                    break;
                case RelicErrors.MultipleFromCategory:
                    SetTextVerify(relic, slot, "This effect has the same category as another effect in this relic.", Brushes.Red);
                    break;
                case RelicErrors.UniqueRelicEffect:
                    SetTextVerify(relic, slot, "Relic effect is only for special unique relics.", Brushes.Orange);
                    break;
            }
        }
    }

    private void SetTextVerify(uint relic, uint slot, string errorText, Brush colour)
    {
        relicTextBlock[relic, slot].Foreground = colour;
        relicTextBlock[relic, slot].ToolTip = errorText;
    }

    //
    // Filter Relic Effects
    //

    private void textboxFilterEffects_TextChanged(object sender, TextChangedEventArgs e)
    {
        FilterRelicEffectsBox();
    }

    private void ToggleShowUniqueEffects(object sender, RoutedEventArgs e)
    {
        FilterRelicEffectsBox();
    }

    private void Button_ClearEffectFilter(object sender, RoutedEventArgs e)
    {
        textboxFilterEffects.Text = "";
    }

    public void FilterRelicEffectsBox()
    {
        ICollectionView view = CollectionViewSource.GetDefaultView(relicManager.relicEffects);
        view.Filter = (entry) =>
        {
            RelicEffect re = (RelicEffect)entry;

            return re.Description.ToLower().Contains(textboxFilterEffects.Text.ToLower())
                        & relicManager.VerifyEffectIsRelicEffect(re, (bool)checkboxShowUnique.IsChecked);
        };
    }

    //
    // Main UI Buttons
    //

    private void Button_SetRelicsInGame(object sender, RoutedEventArgs e)
    {
        if (relicManager.ConnectionStatus != ConnectionStates.Connected)
            return;

        for (uint x = 0; x < 3; x++)
        {
            if (!(bool)activeRelics[x].IsChecked)
                continue;

            RelicErrors[] error = relicManager.VerifyRelic(x);

            for (uint y = 0; y < 3; y++)
            {
                switch (error[y])
                {
                    case RelicErrors.NotRelicEffect:
                        MessageBox.Show("Cannot inject relic with illegal effects.");
                        return;
                    case RelicErrors.MultipleFromCategory:
                        MessageBox.Show("Cannot inject relic with multiple effects from the same category.");
                        return;
                }
            }
        }

        for (uint relic = 0; relic < 3; relic++)
        {
            if ((bool)activeRelics[relic].IsChecked)
            {
                relicManager.SetRelicInGame(relic);
            }
        }
    }

    private void Button_GetRelicsFromGame(object sender, RoutedEventArgs e)
    {
        if (relicManager.ConnectionStatus != ConnectionStates.Connected)
            return;

        for (uint x = 0; x < 3; x++)
        {
            if ((bool)activeRelics[x].IsChecked)
            {
                relicManager.GetRelicFromGame(x);
                UpdateRelicUIElements(x);
            }
        }
    }

    private void Button_Connect(object sender, RoutedEventArgs e)
    {
        Connect();
    }

    //
    // Relic effect tab UI
    //

    private void Button_AddRelicEffect1(object sender, RoutedEventArgs e)
    {
        AddRelicEffectFromList(0);
    }

    private void Button_AddRelicEffect2(object sender, RoutedEventArgs e)
    {
        AddRelicEffectFromList(1);
    }

    private void Button_AddRelicEffect3(object sender, RoutedEventArgs e)
    {
        AddRelicEffectFromList(2);
    }

    //
    // Preset tab UI
    //

    private void Button_MoveUpPreset(object sender, RoutedEventArgs e)
    {
        RelicPreset preset = (RelicPreset)listboxPresets.SelectedItem;

        if (preset == null)
            return;

        int index = FindPresetIndex(preset);

        if (index == 0)
            return;

        relicPresets.Move(index--, index);
        SavePresetFile();
    }

    private void Button_MoveDownPreset(object sender, RoutedEventArgs e)
    {
        RelicPreset preset = (RelicPreset)listboxPresets.SelectedItem;

        if (preset == null)
            return;

        int index = FindPresetIndex(preset);

        if (index == relicPresets.Count - 1)
            return;

        relicPresets.Move(index++, index);
        SavePresetFile();
    }

    private void Button_LoadPreset(object sender, RoutedEventArgs e)
    {
        RelicPreset preset = (RelicPreset)listboxPresets.SelectedItem;

        if (preset == null)
            return;

        for (uint x = 0; x < 3; x++)
        {
            if ((bool)activeRelics[x].IsChecked)
            {
                uint[] effectId = new uint[3];

                for (uint y = 0; y < 3; y++)
                    effectId[y] = preset.EffectId[x, y];

                relicManager.SetRelic(x, effectId);
                UpdateRelicUIElements(x);
            }
        }
    }

    private void Button_SaveNewPreset(object sender, RoutedEventArgs e)
    {
        string name = Microsoft.VisualBasic.Interaction.InputBox("Description", "Enter name for preset");

        if (string.IsNullOrEmpty(name))
            return;

        RelicPreset relic = new RelicPreset(name);

        for (uint x = 0; x < 3; x++)
        {
            for (uint y = 0; y < 3; y++)
            {
                relic.EffectId[x, y] = relicManager.GetRelicEffectId(x, y);
            }
        }
        relicPresets.Add(relic);

        SavePresetFile();
    }

    private void Button_UpdatePreset(object sender, RoutedEventArgs e)
    {
        RelicPreset preset = (RelicPreset)listboxPresets.SelectedItem;

        if (preset == null)
            return;

        MessageBoxResult result = MessageBox.Show("Update preset " + preset.Name + " with new effects?", "Corfirm Update", MessageBoxButton.YesNo);

        if (result == MessageBoxResult.No)
            return;

        for (uint x = 0; x < 3; x++)
        {
            for (uint y = 0; y < 3; y++)
            {
                preset.EffectId[x, y] = relicManager.GetRelicEffectId(x, y);
            }
        }

        SavePresetFile();
    }

    private void Button_RenamePreset(object sender, RoutedEventArgs e)
    {
        RelicPreset preset = (RelicPreset)listboxPresets.SelectedItem;

        if (preset == null)
            return;

        string name = Microsoft.VisualBasic.Interaction.InputBox("Description", "Enter name for preset", preset.Name);

        if (string.IsNullOrEmpty(name) || name == preset.Name)
            return;

        preset.Name = name;
        listboxPresets.UpdateLayout();
        SavePresetFile();
    }

    private void Button_DeletePreset(object sender, RoutedEventArgs e)
    {
        RelicPreset preset = (RelicPreset)listboxPresets.SelectedItem;

        if (preset == null)
            return;

        MessageBoxResult result = MessageBox.Show("Delete the preset " + preset.Name + "?", "Corfirm Delete", MessageBoxButton.YesNo);

        if (result == MessageBoxResult.No)
            return;

        if (relicPresets.Remove(preset))
            SavePresetFile();
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
        public event PropertyChangedEventHandler? PropertyChanged;

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

        public uint[,] EffectId { get; set; }

        public RelicPreset(string name)
        {
            Name = name;
            EffectId = new uint[3, 3];
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
                string? line;
                while ((line = sr.ReadLine()) != null)
                {
                    string[] split = line.Split("\t");
                    if (split.Length == 10)
                    {
                        RelicPreset relic = new RelicPreset(split[0]);

                        relic.EffectId[0, 0] = Convert.ToUInt32(split[1]);
                        relic.EffectId[0, 1] = Convert.ToUInt32(split[2]);
                        relic.EffectId[0, 2] = Convert.ToUInt32(split[3]);

                        relic.EffectId[1, 0] = Convert.ToUInt32(split[4]);
                        relic.EffectId[1, 1] = Convert.ToUInt32(split[5]);
                        relic.EffectId[1, 2] = Convert.ToUInt32(split[6]);

                        relic.EffectId[2, 0] = Convert.ToUInt32(split[7]);
                        relic.EffectId[2, 1] = Convert.ToUInt32(split[8]);
                        relic.EffectId[2, 2] = Convert.ToUInt32(split[9]);

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
                    sw.WriteLine(preset.Name + "\t"
                        + preset.EffectId[0, 0] + "\t"
                        + preset.EffectId[0, 1] + "\t"
                        + preset.EffectId[0, 2] + "\t"
                        + preset.EffectId[1, 0] + "\t"
                        + preset.EffectId[1, 1] + "\t"
                        + preset.EffectId[1, 2] + "\t"
                        + preset.EffectId[2, 0] + "\t"
                        + preset.EffectId[2, 1] + "\t"
                        + preset.EffectId[2, 2]);
                }
            }
        }
        catch (Exception f)
        {
            MessageBox.Show("Problem saving presets file to " + file + "\n" + f.ToString());
        }
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

    //
    // Settings file
    //

    enum Settings
    {
        autoconnect,
    }

    private void SaveSettingsFile()
    {
        string file = System.AppDomain.CurrentDomain.BaseDirectory + "settings.nre";
        try
        {
            using (StreamWriter sw = new StreamWriter(file))
            {
                sw.WriteLine(Settings.autoconnect.ToString() + "\t" +
                    ((bool)checkboxAutoconnect.IsChecked ? "1" : "0"));
            }
        }
        catch (Exception e)
        {
            Debug.Print("Problem saving settings file to " + file);
        }
    }

    private void LoadSettingsFile()
    {
        try
        {
            string file = System.AppDomain.CurrentDomain.BaseDirectory + "settings.nre";

            if (!File.Exists(file))
                return;

            using (StreamReader sr = new StreamReader(file))
            {
                string line;
                while ((line = sr.ReadLine()) != null)
                {
                    string[] split = line.Split("\t");
                    if (split.Length == 2)
                    {
                        switch (split[0])
                        {
                            case "autoconnect":
                                checkboxAutoconnect.IsChecked = (Convert.ToUInt32(split[1]) == 1) ? true : false;
                                break;
                        }
                    }
                }
            }
        }
        catch (Exception f)
        {
            MessageBox.Show("Problem loading settings file.");
        }
    }

    //
    //
    //

    //

    private void Listview_RightClickRelicEffects(object sender, MouseEventArgs e)
    {
        // Get the clicked item
        var clickedItem = (sender as ListView)?.SelectedItem;

        if (clickedItem != null)
        {
            // Show the context menu
            ItemContextMenu.IsOpen = true;
        }

    }

}