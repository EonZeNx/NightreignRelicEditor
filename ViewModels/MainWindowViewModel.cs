using System.ComponentModel;
using System.Runtime.CompilerServices;
using NightreignRelicEditor.Models;

namespace NightreignRelicEditor.ViewModels;

public class MainWindowViewModel : INotifyPropertyChanged
{
    public event PropertyChangedEventHandler? PropertyChanged;

    private void OnPropertyChanged([CallerMemberName] string? name = null) =>
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
    
    private RelicManager relicManager;
    public RelicManager RelicManager
    {
        get => relicManager;
        set { relicManager = value; OnPropertyChanged(); }
    }

    public MainWindowViewModel()
    {
        relicManager = new RelicManager();
    }
}