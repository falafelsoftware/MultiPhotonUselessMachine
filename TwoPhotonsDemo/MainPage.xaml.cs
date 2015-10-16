using Newtonsoft.Json;
using System;
using System.IO;
using System.Net;
using System.Text;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;

namespace TwoPhotonsDemo
{
    public sealed partial class MainPage : Page
    {
        const string PHOTON1DEVICEID = "PHOTON1DEVICEIDHERE";
        const string PHOTON2DEVICEID = "PHOTON2DEVICEIDHERE";
        const string ACCESS_TOKEN = "YOURACCESSTOKENHERE";
        public MainPage()
        {
            this.InitializeComponent();
        }
        private async void Open_Stream_Click(object sender, RoutedEventArgs e)
        {
            (sender as Button).IsEnabled = false;
            string url = String.Format("https://api.particle.io/v1/devices/{0}/events/NeedCover?access_token={1}", PHOTON1DEVICEID, ACCESS_TOKEN);
            WebRequest request = WebRequest.Create(url);
            request.Method = "GET";
            using (WebResponse response = await request.GetResponseAsync())
            {
                using (Stream stream = response.GetResponseStream())
                {
                    StreamReader reader = new StreamReader(stream);
                    int blankRead = 0;
                    while (blankRead <= 1)
                    {
                        // Event comes streaming in 3 lines. blank line, 
                        // "event: NeedCover", 
                        // then a json line that starts with "data:",
                        // If more than a couple blank lines in a row then we're done with the stream.
                        var str = await reader.ReadLineAsync();
                        if (string.IsNullOrEmpty(str))
                        {
                            ++blankRead;
                        }
                        else if (str == "event: NeedCover")
                        {
                            blankRead = 0;
                            var data = await reader.ReadLineAsync();
                            var jsonData = JsonConvert.DeserializeObject<ParticleEvent>(data.Substring(data.IndexOf("data:") + 5));
                            streamResultTextBox.Text = jsonData.data;
                        }
                    }
                }
            }
            (sender as Button).IsEnabled = true;
        }
        private async void Get_Variable_Click(object sender, RoutedEventArgs e)
        {
            string url = String.Format("https://api.particle.io/v1/devices/{0}/Light?access_token={1}", PHOTON1DEVICEID, ACCESS_TOKEN); 
            WebRequest request = WebRequest.Create(url);
            request.Method = "GET";
            using (WebResponse response = await request.GetResponseAsync())
            {
                using (var stream = response.GetResponseStream())
                {
                    StreamReader reader = new StreamReader(stream);
                    var str = await reader.ReadToEndAsync();
                    var jsonData = JsonConvert.DeserializeObject<ParticleVariable>(str);
                    resultTextBox.Text = jsonData.result;
                }
            }
        }
        private async void RadioButton_Click(object sender, RoutedEventArgs e)
        {
            string url = String.Format("https://api.particle.io/v1/devices/{0}/Position?access_token={1}", PHOTON2DEVICEID, ACCESS_TOKEN);
            var request = WebRequest.Create(url);
            var postData = "value="+(sender as RadioButton).Content;
            var data = Encoding.ASCII.GetBytes(postData);
            request.Method = "POST";
            request.ContentType = "application/x-www-form-urlencoded";
            using (var stream = await request.GetRequestStreamAsync())
            {
                stream.Write(data, 0, data.Length);
            }
            var response = await request.GetResponseAsync();
            var responseString = new StreamReader(response.GetResponseStream()).ReadToEnd();
            postTextBox.Text = responseString;
        }
    }
    public class ParticleEvent
    {
        public string data { get; set; }
        public string ttl { get; set; }
        public string published_at { get; set; }
        public string coreid { get; set; }
    }
    public class ParticleVariable
    {
        public string cmd { get; set; }
        public string name { get; set; }
        public string result { get; set; }
        public CoreInfo coreInfo { get; set; }
    }
    public class CoreInfo
    {
        public string last_app { get; set; }
        public string last_heard { get; set; }
        public string connected { get; set; }
        public string last_handshake_at { get; set; }
        public string deviceID { get; set; }
        public string product_id { get; set; }        
    }
}
