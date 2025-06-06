using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MediTrack.Frontend.Platforms.Android;

// Esta clase hereda del manejador de mensajes de red nativo de Android.
// La usamos para poder personalizar el comportamiento de HttpClient en esta plataforma,
// específicamente para confiar en certificados de desarrollo locales.
internal class CustomAndroidMessageHandler : Xamarin.Android.Net.AndroidMessageHandler
{
}
