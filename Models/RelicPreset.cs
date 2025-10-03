using System.ComponentModel;
using System.Runtime.CompilerServices;

namespace NightreignRelicEditor.Models;

public class RelicPreset : INotifyPropertyChanged
{
    public event PropertyChangedEventHandler? PropertyChanged;

    private void NotifyPropertyChanged([CallerMemberName] string? name = null) => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));

    private string relicName = "";
    public string Name
    {
        get => relicName;
        set { relicName = value; NotifyPropertyChanged(); }
    }

    public Relic[] Relics { get; init; } = new Relic[6];

    public RelicPreset(string name)
    {
        Name = name;
    }
}