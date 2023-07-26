using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Q42.HueApi.Streaming;
using Q42.HueApi.Streaming.Models;
using System.Diagnostics;

namespace WpfHueEntDemo
{
    public class StreamingSetup
    {
        public static async Task<StreamingGroup> CreateStreamingSetupAsync(string argIp, string argKey, string argEkey)
        {
            //IPとHue用のキーを設定：キーはusernameとEntertainmentkey
            string ip = argIp;
            string key = argKey;
            string ekey = argEkey;

            StreamingHueClient client = new StreamingHueClient(ip, key, ekey);
            var all = await client.LocalHueClient.GetEntertainmentGroups ();
            var group = all.FirstOrDefault ();

            var stream = new StreamingGroup(group.Locations);
            await client.Connect(group.Id);
            client.AutoUpdate(stream, new CancellationToken(), 50);
            Debug.Print("EntertainmentArea's Name: "+group.Name + ", Connected.");
            return stream;
        }
    }
}
