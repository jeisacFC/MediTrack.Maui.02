using Android.App;
using Android.Runtime;
using ZXing.Mobile; // Añade este using

namespace MediTrack.Frontend;

[Application]
public class MainApplication : MauiApplication
{
	public MainApplication(IntPtr handle, JniHandleOwnership ownership)
		: base(handle, ownership)
	{
	}

	protected override MauiApp CreateMauiApp() => MauiProgram.CreateMauiApp();

    //public override void OnCreate()
    //{
    //    base.OnCreate(); // Importante mantener esta línea

    //    // Inicializa ZXing para Android
    //    ZXing.Mobile.MobileBarcodeScanner.Initialize(Application);
    //}

}
