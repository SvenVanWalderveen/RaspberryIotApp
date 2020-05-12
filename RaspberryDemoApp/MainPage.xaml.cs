using Microsoft.Azure.Devices.Client;
using Newtonsoft.Json;
using PCSC;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using System.Text;
using Windows.Devices.Enumeration;
using Windows.Devices.SmartCards;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Navigation;

// The Blank Page item template is documented at https://go.microsoft.com/fwlink/?LinkId=402352&clcid=0x409

namespace RaspberryDemoApp
{
    /// <summary>
    /// An empty page that can be used on its own or navigated to within a Frame.
    /// </summary>
    public sealed partial class MainPage : Page
    {
        static DeviceClient deviceClient;
        static string iotHubUri = "DataTest.azure-devices.net";
        static string deviceKey = "A2IpQ3l2cNt2/FD1xfffhu/lAfP7x+3RPqB31cdvKRc=";
        public MainPage()
        {
            this.InitializeComponent();
            deviceClient = DeviceClient.Create(iotHubUri, new DeviceAuthenticationWithRegistrySymmetricKey("raspberrytestsven", deviceKey), TransportType.Mqtt);
        }

        private async void Event_SendMessageToHub(object sender, RoutedEventArgs e)
        {
            string atr = GetAtr();

            if (!string.IsNullOrEmpty(atr))
            {
                outputError.Text = atr;
            }
            
            //try
            //{
            //    var telemetryData = new
            //    {
            //        deviceId = "Raspberry Test device",
            //        value = "Hello"
            //    };
            //    var messageSTring = JsonConvert.SerializeObject(telemetryData);
            //    var message = new Message(Encoding.ASCII.GetBytes(messageSTring));

            //    await deviceClient.SendEventAsync(message);
            //    outputError.Text = string.Format("{0}: Message succesfully send", DateTime.Now.ToString("dd-MM-yyyy HH:mm:ss"));
            //} 
            //catch(Exception ex)
            //{
            //    outputError.Text = ex.Message;
            //    outputError.Text = outputError.Text + "\n" + ex.StackTrace;
            //}
            
        }

        private string GetAtr()
        {
            var contextFactory = ContextFactory.Instance;
            using (var context = contextFactory.Establish(SCardScope.System))
            {
                var readerNames = context.GetReaders();

                if (IsEmpty(readerNames))
                {
                    outputSmartcardFound.IsChecked = false;
                    outputNumSmartcardReaders.Text = "0";
                    return null;
                }
                else
                {
                    outputSmartcardFound.IsChecked = true;
                    outputNumSmartcardReaders.Text = string.Format("{0}", readerNames.Count());
                }
                return DisplayAtrForEachReader(context, readerNames);                
            }
        }
        private bool IsEmpty(ICollection<string> readerNames) => readerNames == null || readerNames.Count == 0;
        private string DisplayAtrForEachReader(ISCardContext context, IEnumerable<string> readerNames)
        {
            foreach (var readerName in readerNames)
            {
                try
                {
                    using (var reader = context.ConnectReader(readerName, SCardShareMode.Shared, SCardProtocol.Any))
                    {
                        return DisplayAtr(reader);
                    }
                }
                catch (Exception exception)
                {
                    outputError.Text = exception.Message;
                    return null;
                }
            }
            return null;
        }

        /// <summary>
        /// Receive and print ATR string attribute
        /// </summary>
        /// <param name="reader">Connected smart-card reader instance</param>
        private string DisplayAtr(ICardReader reader)
        {
            try
            {
                var atr = reader.GetAttrib(SCardAttribute.AtrString);
                return BitConverter.ToString(atr ?? new byte[] { });
            }
            catch (Exception exception)
            {
                outputError.Text = exception.Message;
                return null;
            }
        }
    }
}
