using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MediTrack.Frontend.Services;

// Esta clase ayuda a configurar HttpClient para confiar en certificados de desarrollo locales
public class DevHttpsConnectionHelper
{
    public HttpMessageHandler GetPlatformSpecificHttpMessageHandler()
    {
#if ANDROID
        var handler = new Platforms.Android.CustomAndroidMessageHandler();
        handler.ServerCertificateCustomValidationCallback = (message, cert, chain, errors) =>
        {
            if (cert != null && cert.Issuer.Equals("CN=localhost"))
                return true;
            return errors == System.Net.Security.SslPolicyErrors.None;
        };
        return handler;
#else
        return new HttpClientHandler();
#endif
    }
}