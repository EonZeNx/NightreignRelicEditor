using System.ComponentModel;
using System.Runtime.CompilerServices;
using NightreignRelicEditor.Models;

namespace NightreignRelicEditor.ViewModels;

public class MainWindowViewModel : INotifyPropertyChanged
{
    public event PropertyChangedEventHandler PropertyChanged;
    void OnPropertyChanged([CallerMemberName] string name = null) =>
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
    
    private RelicManager _relicManager;
    public RelicManager RelicManager
    {
        get => _relicManager;
        set { _relicManager = value; OnPropertyChanged(); }
    }

    public MainWindowViewModel()
    {
        RelicManager = new RelicManager();
    }
}