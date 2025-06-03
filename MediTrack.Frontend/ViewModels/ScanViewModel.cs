//using System;
//using System.ComponentModel; // Para INotifyPropertyChanged
//using System.Runtime.CompilerServices; // Para CallerMemberName
//using System.Windows.Input; // Para ICommand
//using MediTrack.Frontend.Services; // Para IBarcodeScannerService
//using System.Collections.Generic;
//using System.Linq;
//using System.Text;
//using System.Threading.Tasks;
//using ZXing; // Añade esta línea
//using ZXing.Mobile;

//namespace MediTrack.Frontend.ViewModels;


//public class ScanViewModel : INotifyPropertyChanged
//{
//    private readonly IBarcodeScannerService _scanner;

//    private string _scanResult;
//    public string ScanResult
//    {
//        get => _scanResult;
//        set
//        {
//            _scanResult = value;
//            OnPropertyChanged();
//        }
//    }

//    public ICommand ScanCommand { get; }

//    public ScanViewModel(IBarcodeScannerService scanner)
//    {
//        _scanner = scanner;
//        ScanCommand = new Command(async () => await ScanBarcode());
//    }

//    private async Task ScanBarcode()
//    {
//        ScanResult = "Escaneando...";
//        ScanResult = await _scanner.ScanBarcodeAsync();
//    }

//    public event PropertyChangedEventHandler PropertyChanged;
//    protected void OnPropertyChanged([CallerMemberName] string name = null)
//    {
//        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
//    }
//}
