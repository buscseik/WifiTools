using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.Devices.Enumeration;
using Windows.Devices.WiFi;
using Windows.Security.Credentials;


// This two resource are referenced for the project
// C:\Program Files (x86)\Windows Kits\10\UnionMetadata\10.0.19041.0\Windows.winmd 
// C:\Program Files (x86)\Microsoft SDKs\NuGetPackages\System.Runtime.WindowsRuntime\4.0.0\ref\netcore50\System.Runtime.WindowsRuntime.dll 

/*
 - This video give a brief explanation for async in csharp
    https://www.youtube.com/watch?v=2moh18sh5p4
 - Async with console explanation
    https://recaffeinate.co/post/how-to-await-console-application/
 - UWP - WifiAdapter - connect example
    https://stackoverflow.com/questions/67209588/c-sharp-connect-to-wifi-network-by-windows-runtime-api-localsystem-vs-networkse
 - Native WIFI API example
    https://stackoverflow.com/questions/29120398/accessing-a-method-within-a-class-within-a-class-in-the-nativewifi-library
 - Native wifi documentation
    https://docs.microsoft.com/en-us/windows/win32/api/wlanapi/nf-wlanapi-wlanconnect
 - WIFI WPS with UWP module
    https://stackoverflow.com/questions/43002906/wifi-wps-client-start-in-windows-10-in-script-or-code
 - UWP examples (with Wifi example)
    https://docs.microsoft.com/en-us/samples/microsoft/windows-universal-samples/wifidirect/
 - UWP Windows.Devices.WiFi documentation
    https://docs.microsoft.com/en-us/uwp/api/windows.devices.wifi?view=winrt-22000
 - Another example of native wifi
    https://stackoverflow.com/questions/25808620/c-sharp-connect-to-wifi-network-with-managed-wifi-api
 - WiFi profile manipulation in powershell
    https://github.com/jcwalker/WiFiProfileManagement
    https://4sysops.com/archives/manage-wifi-connection-in-windows-10-with-powershell/
 */


/*
 Tasks:
    - imput for timeout
    - imput for interface
    - input for PIN 
 */

namespace ConsoleApp2
{
    internal class Program
    {
        static async Task Main(string[] args)
        {
            if (args.Count() < 4)
            {
                Console.WriteLine("Please check parameters. \r\n Eg.: ConnectWPS wpspush <ssid> <bssid> <timeout> <WifiAdapterGuid>\r\nConnectWPS wpspin <ssid> <bssid> <timeout> <WifiAdapterGuid> <pin>");
            }
            else
            {
                if (args[0] == "wpspin") { await ConnectWifiAsync(Convert.ToInt32(args[3]), args[1], args[2], args[4], "wpspin", args[5]); }
                else { await ConnectWifiAsync(Convert.ToInt32(args[3]) * 1000, args[1], args[2], args[4], "wpspush", "0000"); }
            }

            // await ConnectWifiAsync(20000, "VM9788180", "34:2c:c4:d7:2e:66", "764eab9b-801f-42b5-9e9b-7d2038cdcc66");
        }
        static private async Task ConnectWifiAsync(int connection_timeout, string SSID, string BSSID, string adapter_id, string wpstype, string wpspin)
        {
            DeviceInformationCollection dic = await DeviceInformation.FindAllAsync(WiFiAdapter.GetDeviceSelector());

            for (int i = 0; i < dic.Count; i++)
            {
                var adapter = await WiFiAdapter.FromIdAsync(dic[i].Id);
                if (adapter.NetworkAdapter.NetworkAdapterId.ToString() == adapter_id)
                {
                    // Console.WriteLine(adapter.NetworkAdapter.NetworkAdapterId);
                    // This will give back the adatper id guid
                    bool is_network_found = false;
                    foreach (var an in adapter.NetworkReport.AvailableNetworks)
                    {

                        if (an.Ssid == SSID && an.Bssid == BSSID)
                        {
                            is_network_found = true;
                            await adapter.ScanAsync();

                            WiFiConnectionMethod connection_method = WiFiConnectionMethod.WpsPushButton;
                            PasswordCredential creds = null;

                            if (wpstype == "wpspush")
                            {
                                connection_method = WiFiConnectionMethod.WpsPushButton;


                            }
                            else
                            {
                                connection_method = WiFiConnectionMethod.WpsPin;
                                creds = new PasswordCredential();
                                creds.Password = wpspin;

                            }

                            var ConnectTask = adapter.ConnectAsync(an, WiFiReconnectionKind.Automatic, creds, "", connection_method).AsTask();
                            if (!ConnectTask.Wait(connection_timeout))
                            {
                                ConnectTask.AsAsyncOperation().Cancel();
                                Console.WriteLine("Connection failed within timeout");

                            }
                            else
                            {
                                Console.WriteLine("Connection completed within timeout");

                            }
                        }
                    }
                    if (!is_network_found) Console.WriteLine("Network is not found");
                }
            }
        }
    }
}
