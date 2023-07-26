using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using Q42.HueApi;
using Q42.HueApi.Interfaces;
using Q42.HueApi.Streaming.Models;
using Q42.HueApi.Streaming.Extensions;
using Q42.HueApi.ColorConverters;
using Q42.HueApi.ColorConverters.HSB;
using MahApps.Metro.Controls;
using Rug.Osc;
using System.Numerics;
using System.Diagnostics;
using System.Drawing;
using System.Windows.Threading;
using MaterialDesignColors.ColorManipulation;
using Org.BouncyCastle.Asn1.X500;
using ControlzEx.Standard;

namespace WpfHueEntDemo
{
    public struct LIMITVALUE
    {
        public LIMITANGLES AGL;
        public LIMITPOSITIONS POS;
        public LIMITVALUE()
        {
            AGL = new LIMITANGLES();
            POS = new LIMITPOSITIONS();
        }

        public void ChangeLEFT_MODE(bool mode)
        {
            POS.UPPER.LEFT_MODE = mode;
            POS.BOTTOM.LEFT_MODE = mode;
            POS.RIGHT.LEFT_MODE = mode;
            POS.LEFT.LEFT_MODE = mode;
        }
        public void SetAngle(string ublr)
        {
            if (ublr == "UPPER")
                AGL.UPPER = CalAngle(POS.UPPER.PELVIS, POS.UPPER.GetTargetHandVec3());
            else if (ublr == "BOTTOM")
                AGL.BOTTOM = CalAngle(POS.BOTTOM.PELVIS, POS.BOTTOM.GetTargetHandVec3());
            else if (ublr == "LEFT")
                AGL.LEFT = CalAngle(POS.LEFT.PELVIS, POS.LEFT.GetTargetHandVec3());
            else
                AGL.RIGHT = CalAngle(POS.RIGHT.PELVIS, POS.RIGHT.GetTargetHandVec3());
        }
        private ANGLE CalAngle(Vector3 v0, Vector3 v1)
        {
            double radV = Math.Atan2(v1.Z - v0.Z, v1.Y - v0.Y);
            double radH = Math.Atan2(v1.Z - v0.Z, v1.X - v0.X);
            float aglV = (float)(radV * 180d / Math.PI);
            float aglH = (float)(radH * 180d / Math.PI) * (-1f);
            return new ANGLE(aglV, aglH);
        }
    }

    public struct LIMITANGLES
    {
        public ANGLE UPPER;
        public ANGLE BOTTOM;
        public ANGLE LEFT;
        public ANGLE RIGHT;
        public LIMITANGLES(ANGLE u, ANGLE b, ANGLE l, ANGLE r)
        {
            UPPER = u; BOTTOM = b; LEFT = l; RIGHT = r;
        }
    }
    ///<summary>
    ///Struct for angle
    ///</summary>
    public struct ANGLE
    {
        public double V;
        public double H;
        public ANGLE(double v, double h)
        {
            V = v; H = h;
        }
    }

    public struct LIMITPOSITIONS
    {
        public TRACKEDVECTOR3 UPPER;
        public TRACKEDVECTOR3 BOTTOM;
        public TRACKEDVECTOR3 LEFT;
        public TRACKEDVECTOR3 RIGHT;
        public LIMITPOSITIONS()
        {
            Vector3 vec0 = new Vector3(0, 0, 0);
            UPPER = new TRACKEDVECTOR3();
            BOTTOM = new TRACKEDVECTOR3();
            LEFT = new TRACKEDVECTOR3();
            RIGHT = new TRACKEDVECTOR3();
            UPPER.SetVector3(vec0, vec0, vec0, vec0);
            BOTTOM.SetVector3(vec0, vec0, vec0, vec0);
            LEFT.SetVector3(vec0, vec0, vec0, vec0);
            RIGHT.SetVector3(vec0, vec0, vec0, vec0);
        }
    }

    public struct TRACKEDVECTOR3
    {
        public Vector3 R_HAND;
        public Vector3 L_HAND;
        public Vector3 PELVIS;
        public Vector3 NECK;
        public bool LEFT_MODE;

        public TRACKEDVECTOR3()
        {
            Vector3 vec0 = new Vector3(0, 0, 0);
            R_HAND = vec0; L_HAND = vec0; PELVIS = vec0; NECK = vec0;
            LEFT_MODE = false;
        }
        public void SetVector3(Vector3 rhand, Vector3 lhand, Vector3 pelvis, Vector3 neck)
        {
            R_HAND = rhand; L_HAND = lhand; PELVIS = pelvis; NECK = neck;
        }
        public Vector3 GetTargetHandVec3()
        {
            if (LEFT_MODE) return L_HAND;
            return R_HAND;
        }

    }

    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : MetroWindow
    {
        private List<BridgeClass> Bridges = new List<BridgeClass>();

        public MainWindow()
        {
            InitializeComponent();
            Bridges = ConnectBridgeWindow.GetBridgesInfo();
        }

        
        static int[] targetIDs = { 2, 3, 5, 6, 7, 8, 9, 10, 11 };
        public struct Keys
        {
            public string ip;
            public string appKey;
            public string Enkey;
            public string name;
            public Keys(string argip, string argappKey, string argEnkey, string argname)
            {
                ip = argip; appKey = argappKey; Enkey = argEnkey; name = argname;
            }
        }


        //private async void btConnect_Click(object sender, RoutedEventArgs e)
        //{
        //    //string ip = "192.168.11.4";
        //    string ip = "192.168.0.8";
        //    string appName = "myhueapp";
        //    string devName = "myhuedevice";
        //    ILocalHueClient hueClient = new LocalHueClient(ip);
        //    MessageBox.Show("Press your bridge button!");
        //    var appKey = await hueClient.RegisterAsync(appName, devName, true);
        //    tbKey.Text = appKey.Username;//kiQ7gdQkNy9CaF0g9SPtfEAEemjz926oAsH22pq-
        //    tbEnKey.Text = appKey.StreamingClientKey;//8D89E773F483D8FE26B08E5E402E3116
        //}

        //private Keys b1, b2, b3;
        private async void btRun_Click(object sender, RoutedEventArgs e)
        {
            if (Bridges == null || Bridges.Count <= 0)
                return;
            ////b1 = new Keys("192.168.11.3", "kiQ7gdQkNy9CaF0g9SPtfEAEemjz926oAsH22pq-", "8D89E773F483D8FE26B08E5E402E3116", "Bridge1");
            //b1 = new Keys("192.168.0.8", "1qprndliXpJrcFqPRdwuChsIZoGC62xhhhxkPbQ5", "DA7675A344AAE7948C304F0A4619016A", "Bridge1");
            //b2 = new Keys("192.168.11.2", "mLjkZSMLrgYj4-oHuACDiJH7tbDI4bwYCUsINHWr", "4C0AA4782B980F658E60782D733E5EC4", "Bridge2");
            //b3 = new Keys("192.168.11.4", "ucxbc14f2OrgdZhpMg93TrO0mw0T3OowjqdWdcvF", "B6C30C02D994717CF30F2C3513CDCB1F", "Bridge3");

            InitializeLimit();

            //await Start(b1.ip, b1.appKey, b1.Enkey);
            //await Start(b2.ip, b2.appKey, b2.Enkey);
            //await Start(b3.ip, b3.appKey, b3.Enkey);
            InitializeTimer();
        }


        private EntertainmentLayer entLayer;
        private List<EntertainmentLayer> entLayers = new List<EntertainmentLayer>();
        private string preHex;
        private DispatcherTimer _timer;

        private void InitializeTimer()
        {
            _timer = new DispatcherTimer();
            _timer.Interval = new TimeSpan(0, 0, 0, 0, 10);
            _timer.Tick += (e, s) => { TimerMethod(); };
            this.Closing += (e, s) => { _timer.Stop(); };
            _timer.Start();
        }
        private void TimerMethod()
        {
            Manage_Process_OSC();
        }
        private async Task Start(string ip, string key, string enKey)
        {
            StreamingGroup stream = await StreamingSetup.CreateStreamingSetupAsync(ip, key, enKey);
            entLayers.Add(stream.GetNewLayer(isBaseLayer: true));
            preHex = "#222222";
        }

        private static CancellationTokenSource WaitAndNext(CancellationTokenSource cts, int waitTime)
        {
            Thread.Sleep(waitTime);
            cts.Cancel();
            cts = new CancellationTokenSource();
            return cts;
        }

        private void ColorPicker_MouseMove(object sender, MouseEventArgs e)
        {
            if (entLayers == null) return;
            System.Drawing.Color col = System.Drawing.Color.FromArgb(cp1.Color.R, cp1.Color.G, cp1.Color.B);
            lbColor.Content = System.Drawing.ColorTranslator.ToHtml(col);
            currentHSB = RGBExtensions.GetHSB(new RGBColor(lbColor.Content.ToString().Remove(0, 1)));
            if (preHex == lbColor.Content.ToString()) return;
            preHex = lbColor.Content.ToString();
            foreach (var item in entLayers) { item.SetState(CancellationToken.None, new RGBColor(lbColor.Content.ToString().Remove(0, 1)), 1, TimeSpan.FromSeconds(0.1f)); }
        }

        private void tile3_Click(object sender, RoutedEventArgs e)
        {
            int[] targetIDs = new int[] { int.Parse(tbDivID.Text) };
            targetIDs = new int[] { 0, 1, 2, 3, 4, 5, 6, 7, 8, 9 };
            Div_Flash(targetIDs);
        }
        private void Div_Flash(int[] targetIDs)
        {
            try
            {
                //var lights = entLayers[1].OrderBy(a => a.Id).ToList();  //IDごとにソート
                //Func<int, int[], bool> func1 = (id, arr) => { return Array.Exists(arr, x => x == id); };
                //IEnumerable<EntertainmentLight> lightsList = loc_lights.Where(a => func1(a.Id, targetIDs));

                var loc_lights = entLayers[1].OrderBy(x => x.LightLocation.X).ToList(); //X座標ごとにソート
                Func<double, int[], bool> func2 = (index, arr) => { return Array.Exists(arr, x => x == index); };
                List<EntertainmentLight> lightList = loc_lights.Where(a => func2(a.LightLocation.X, targetIDs)).ToList();

                Vector3 tmpVec = new Vector3(0.5f, 0.5f, 0.5f);
                HSB hsb = new HSB((int)(360 * (1.0f - tmpVec.X) * 180.04f), (byte)(255 * (1.0f - tmpVec.Z)), (byte)(255 * (1.0f - tmpVec.Y)));
                lightList[0].SetState(CancellationToken.None, hsb.GetRGB(), 2, TimeSpan.FromSeconds(0.1f));
            }
            catch { return; }
        }
        private void tile1_Click(object sender, RoutedEventArgs e)
        {
            Random rnd = new Random();
            var lights = entLayer.OrderBy(a => a.Id).ToList();
            HSB hsb = RGBExtensions.GetHSB(new RGBColor(lbColor.Content.ToString().Remove(0, 1)));

            Task.Run(() => EffectTask(lights, hsb));

            Predicate<double>[] pList = new Predicate<double>[3];
            pList[0] = (x => x % 2 == 0);
            pList[1] = (x => 0 < x);
        }

        private async Task EffectTask(List<EntertainmentLight> lights, HSB hsb)
        {
            CancellationTokenSource cts = new CancellationTokenSource();
            int inTime = 150;
            int outTime = 10;

            Func<int, int[], bool>[] fList = new Func<int, int[], bool>[1];
            fList[0] = (id, arr) => { return Array.Exists(arr, x => x == id); };

            lights.SetState(cts.Token, new RGBColor(0, 0, 0), 1, TimeSpan.FromMilliseconds(1));
            cts = WaitAndNext(cts, 1);

            for (int i = 0; i < lights.Count; i++)
            {
                int[] targets = new int[] { 1, 2, 3, 4, 5, 6, 7, 8, 9, 10, 11 };//ID番号で指定可能
                IEnumerable<EntertainmentLight> lightsList = lights.Where(a => fList[0](a.Id, targets));
                Kira_Flash(lightsList, hsb, ref cts, inTime: inTime, outTime: outTime);
                await Task.Delay(inTime + outTime);
            }

            lights.SetState(cts.Token, new RGBColor(0, 0, 0), 1, TimeSpan.FromMilliseconds(1));
            cts = WaitAndNext(cts, 1);
        }

        private void Kira_Flash(IEnumerable<EntertainmentLight> l, HSB hsb, ref CancellationTokenSource cts, int inTime = 200, int outTime = 150)
        {
            hsb.Brightness = 100;
            l.SetState(cts.Token, hsb.GetRGB(), 1, TimeSpan.FromMilliseconds(inTime));
            cts = WaitAndNext(cts, inTime);
            hsb.Brightness = 0;
            l.SetState(cts.Token, hsb.GetRGB(), 1, TimeSpan.FromMilliseconds(outTime));
            cts = WaitAndNext(cts, outTime);
        }

        private void tile2_Click(object sender, RoutedEventArgs e)
        {
            CancellationTokenSource cts = new CancellationTokenSource();
            HSB hsb = RGBExtensions.GetHSB(new RGBColor(lbColor.Content.ToString().Remove(0, 1)));
            hsb.Brightness = 255;
            int inTime = 100;//100
            int midTime = 800;//800
            int outTime = 200;//200
            entLayer.SetState(cts.Token, hsb.GetRGB(), 1, TimeSpan.FromMilliseconds(inTime));
            cts = WaitAndNext(cts, midTime);
            hsb.Brightness = 0;
            entLayer.SetState(cts.Token, hsb.GetRGB(), 1, TimeSpan.FromMilliseconds(outTime));
            cts = WaitAndNext(cts, outTime);
        }

        private OscReceiver _oscReceiver;
        private Task? _oscReceiveTask = null;


        /// <summary>
        /// OSCでパケットを受け取ったときの処理
        /// </summary>
        private TRACKEDVECTOR3 _currentTrackedVec3 = new TRACKEDVECTOR3();
        private void tsOSC_Toggled(object sender, RoutedEventArgs e)
        {
            gbMode.IsEnabled = !gbMode.IsEnabled;
            if (tsOSC.IsOn)
            {
                _oscReceiver = new OscReceiver(12345);
                _oscReceiver.Connect();
                _oscReceiveTask = new Task(() =>
                {
                    try
                    {
                        while (_oscReceiver.State != OscSocketState.Closed)
                        {
                            OscPacket packet = _oscReceiver.Receive();

                            string str = packet.ToString();
                            //Debug.Print(str);
                            int s, e;

                            string msgadr = "";
                            if (((s = str.IndexOf('/')) >= 0) && ((e = str.IndexOf(',', s + 1)) >= 0))
                            {
                                msgadr = str.Substring(s + 1, e - s - 1);
                            }

                            if (((s = str.IndexOf('"')) >= 0) && (e = str.IndexOf('"', s + 1)) >= 0)
                            {
                                string recieveData = str.Substring(s + 1, e - s - 1);
                                //Debug.Print(recieveData);
                                float[] arr = Array.ConvertAll(recieveData.Split(','), float.Parse);

                                Vector3 _pelvis = new Vector3(arr[0], arr[1], arr[2]);
                                Vector3 _rightHand = new Vector3(arr[3], arr[4], arr[5]);
                                Vector3 _leftHand = new Vector3(arr[6], arr[7], arr[8]);
                                Vector3 _neck = new Vector3(arr[9], arr[10], arr[11]);
                                _currentTrackedVec3.SetVector3(_rightHand, _leftHand, _pelvis, _neck);
                            }
                        }
                    }
                    catch (Exception ex)
                    {
                        if (_oscReceiver.State == OscSocketState.Connected)
                            MessageBox.Show(ex.ToString());
                    }
                });
                _oscReceiveTask.Start();
            }
            else
            {
                _oscReceiver.Close();
            }
        }

        private void Manage_Process_OSC()
        {
            if (!tsOSC.IsOn) return;
            //Debug.Print(_currentTrackedVec3.LEFT_MODE.ToString());
            var targetHand = _currentTrackedVec3.GetTargetHandVec3();
            if ((bool)rbA.IsChecked)
            {
                CalculateAngle(_currentTrackedVec3.PELVIS, targetHand);
                Process_OSC_AT(_currentAgl.H);
            }
            else if ((bool)rbB.IsChecked)
            {
                Process_OSC_BT(_currentTrackedVec3.PELVIS.Z);
            }
            else if ((bool)rbC.IsChecked)
            {
                Process_OSC_C(targetHand);
            }
            else if ((bool)rbT.IsChecked)
            {
                Process_OSC_T(_currentTrackedVec3.R_HAND, _currentTrackedVec3.L_HAND, _currentTrackedVec3.PELVIS, _currentTrackedVec3.NECK);
            }
            else if ((bool)rbK.IsChecked)
            {
                Process_OSC_K(_currentTrackedVec3.R_HAND, _currentTrackedVec3.L_HAND, _currentTrackedVec3.PELVIS, _currentTrackedVec3.NECK);
            }
        }


        HSB currentHSB;
        int currentIndex = 0;
        private void Process_OSC_A(float agl)
        {
            HSB hsb = currentHSB;
            var lights = entLayer.OrderBy(a => a.Id).ToList();
            int lNum = lights.Count;
            PointF edgeAgl = new PointF(80f, 200f);
            float intervalAgl = (edgeAgl.Y - edgeAgl.X) / lNum;
            float constraint = intervalAgl * 1.5f;
            var aglList = new List<float>();

            for (int i = 0; i < lNum; i++)
                aglList.Add(edgeAgl.X + intervalAgl * i);
            float minDiff = 100000f;
            int minIndex = 0;
            for (int i = 0; i < lNum; i++)
            {
                float diff = Math.Abs(agl - aglList[i]);
                if (diff < minDiff)
                {
                    minDiff = diff; minIndex = i;
                }
            }
            if (currentIndex != minIndex)
            {
                lights.SetState(CancellationToken.None, new RGBColor(0, 0, 0), 1, TimeSpan.FromMilliseconds(1));
                currentIndex = minIndex;
            }
            lights[minIndex].SetState(CancellationToken.None, hsb.GetRGB(), TimeSpan.FromMilliseconds(0.1f));

        }
        //Multi Threading Version

        private void Process_OSC_AT(double agl)
        {
            HSB hsb = currentHSB;
            var lights = entLayer.OrderBy(a => a.Id).ToList();
            int lNum = lights.Count;
            double intervalAgl = (_limitValue.AGL.RIGHT.H - _limitValue.AGL.LEFT.H) / lNum;
            double constraint = intervalAgl * 1.2f;
            var aglList = new List<double>();

            for (int i = 0; i < lNum; i++)
                aglList.Add(_limitValue.AGL.LEFT.H + intervalAgl * i);
            List<Task> taskList = new List<Task>();
            for (int i = 0; i < lNum; i++)
            {
                var l = lights[i];
                var aagl = aglList[i];
                Task tmpTask = Task.Run(async () =>
                {
                    var tmpHsb = hsb;
                    double diff = Math.Abs(agl - aagl);
                    int bri = (int)((constraint - diff) / constraint * 200.0f);
                    tmpHsb.Brightness = bri;
                    l.SetState(CancellationToken.None, tmpHsb.GetRGB(), TimeSpan.FromMilliseconds(0.1f));
                });
            }
            Task.WhenAll(taskList);
        }

        private void Process_OSC_BT(double z)
        {
            HSB hsb = currentHSB;
            var lights = entLayer.OrderBy(a => a.Id).ToList();
            int lNum = lights.Count;
            //Debug.Print(lNum.ToString());
            double intervalPos = (_limitValue.POS.BOTTOM.PELVIS.Z - _limitValue.POS.UPPER.PELVIS.Z) / lNum;
            double constraint = intervalPos * 1.2f;
            var zList = new List<double>();
            for (int i = 0; i < lNum; i++)
                zList.Add(_limitValue.POS.UPPER.PELVIS.Z + intervalPos * i);

            List<Task> taskList = new List<Task>();
            for (int i = 0; i < lNum; i++)
            {
                var l = lights[i];
                var zz = zList[i];
                Task tmpTask = Task.Run(async () =>
                {
                    var tmpHsb = hsb;
                    double diff = Math.Abs(z - zz);
                    int bri = (int)((constraint - diff) / constraint * 200.0f);
                    tmpHsb.Brightness = bri;
                    l.SetState(CancellationToken.None, tmpHsb.GetRGB(), TimeSpan.FromMilliseconds(0.1f));
                });
            }
            Task.WhenAll(taskList);
        }
        private void Process_OSC_C(Vector3 vec)
        {
            if (entLayer == null) return;
            float xmin = -0.6f; float xmax = 0.3f;
            float ymin = 0.5f; float ymax = 1.5f;
            float zmin = 0.6f; float zmax = 2.0f;
            Vector3 tmpVec = new Vector3();
            tmpVec.X = xmax - vec.X / (xmax - xmin);
            tmpVec.Y = ymax - vec.Y / (ymax - ymin);
            tmpVec.Z = zmax - vec.Z / (zmax - zmin);
            if (tmpVec.X > 1.0f) tmpVec.X = 1.0f;
            else if (tmpVec.X < 0.0f) tmpVec.X = 0.0f;
            if (tmpVec.Y > 1.0f) tmpVec.Y = 1.0f;
            else if (tmpVec.Y < 0.0f) tmpVec.Y = 0.0f;
            if (tmpVec.Z > 1.0f) tmpVec.Z = 1.0f;
            else if (tmpVec.Z < 0.0f) tmpVec.Z = 0.0f;

            HSB hsb = new HSB((int)(360 * (1.0f - tmpVec.X) * 182.04f), (byte)(255 * (1.0f - tmpVec.Z)), (byte)(255 * (1.0f - tmpVec.Y)));

            entLayer.SetState(CancellationToken.None, hsb.GetRGB(), 1, TimeSpan.FromSeconds(0.1f));
        }

        #region ThrowingSection
        Stopwatch sw = new Stopwatch();
        private void Process_OSC_T(Vector3 rHand, Vector3 lHand, Vector3 pelvis, Vector3 neck)
        {
            //2点座標間の接近
            Func<Vector3, Vector3, float, bool> func1 = (a, b, limit) => { if (Math.Abs(a.X - b.X) < limit && Math.Abs(a.Y - b.Y) < limit && Math.Abs(a.Z - b.Z) < limit) { return true; } return false; };
            //腕が前に振られたか
            Func<Vector3, Vector3, float, bool> func2 = (a, b, limit) => { if (a.X - b.X > limit) { return true; } return false; };

            if (entLayer == null) return;

            if (func1(rHand, neck, 0.2f)) { sw.Start(); }//右腕
            else if (func2(rHand, neck, 0.3f))
            {
                sw.Stop();
                if (sw.Elapsed.Milliseconds > 0 && sw.Elapsed.Milliseconds < 2000) { ThrowingFlash(); }
                else { sw = new Stopwatch(); }
            }
            /*  振りかぶる左腕で誤作動しちゃう
            if (func1(lHand, neck, 0.3f)) { sw.Start(); }//左腕
            else if (func2(lHand, neck, 0.3f))
            {
                sw.Stop();
                if (sw.Elapsed.Milliseconds > 0 && sw.Elapsed.Milliseconds < 2000) { ThrowingFlash(); }
                else { sw = new Stopwatch(); }
            }*/
        }

        private async void ThrowingFlash()
        {
            Random rnd = new Random();
            int[] ids = new int[] { 2, 3, 5, 6, 7, 8, 9, 10, 11 };
            CancellationTokenSource cts = new CancellationTokenSource();
            Vector3 tmpVec = new Vector3(rnd.Next(0, 360), 255f, 120f);
            HSB hsb = new HSB((int)(360 * (1.0f - tmpVec.X) * 180.4f), (byte)(255 * (1.0f - tmpVec.Z)), (byte)(255 * (1.0f - tmpVec.Y)));
            hsb.Brightness = 255;
            for (int i = 0; i < ids.Length; i++)
            {
                int[] IDs = new int[] { ids[i] };
                MakeLightList(IDs).SetState(CancellationToken.None, hsb.GetRGB(), 2, TimeSpan.FromSeconds(0.1f));
                await Task.Delay(10);
            }
            hsb.Brightness = 0;
            for (int i = 0; i < ids.Length; i++)
            {
                int[] IDs = new int[] { ids[i] };
                MakeLightList(IDs).SetState(CancellationToken.None, hsb.GetRGB(), 2, TimeSpan.FromSeconds(0.1f));
                await Task.Delay(10);
            }
        }
        #endregion



        private IEnumerable<EntertainmentLight> MakeLightList(int[] targetId)
        {
            var lights = entLayer.OrderBy(a => a.Id).ToList();
            Func<int, int[], bool> func1 = (id, arr) => { return Array.Exists(arr, x => x == id); };
            IEnumerable<EntertainmentLight> lightList = lights.Where(a => func1(a.Id, targetId));
            return lightList;
        }


        #region KameHameHa
        Stopwatch swKame = new Stopwatch();
        Stopwatch swHame = new Stopwatch();
        Stopwatch swHa = new Stopwatch();
        private void Process_OSC_K(Vector3 rHand, Vector3 lHand, Vector3 pelvis, Vector3 neck)
        {
            //2点座標間の接近
            Func<Vector3, Vector3, float, bool> func1 = (a, b, limit) => { if (Math.Abs(a.X - b.X) < limit && Math.Abs(a.Y - b.Y) < limit && Math.Abs(a.Z - b.Z) < limit) { return true; } return false; };
            //腕が後ろに振られたか
            Func<Vector3, Vector3, Vector3, float, bool> func2 = (a, b, c, limit) => { if ((Math.Abs(a.X - c.X) < limit) && (Math.Abs(b.X - c.X) < limit)) return true; return false; };
            //腕が前にあるか
            Func<Vector3, Vector3, Vector3, float, bool> func3 = (a, b, c, limit) => { if (a.X - c.X > limit && b.X - c.X > limit) { return true; } return false; };
            //StopWatchのリセット
            Func<Stopwatch, Stopwatch, Stopwatch, float, bool> func4 = (sw1, sw2, sw3, limit) =>
            {
                sw1.Stop();
                if (sw1.Elapsed.Milliseconds > 0) { sw1.Start(); return false; }//波カウントしてるとき
                else if (sw1.Elapsed.Seconds > 5) { return true; }//波カウント5秒経過したとき

                return false;
            };

            if ((func1(rHand, lHand, 0.1f) && func3(rHand, lHand, pelvis, 0.3f)) || Keyboard.IsKeyDown(Key.A))//最初の構え、かーめー
            {
                swHame.Stop();
                if (swKame.Elapsed.Milliseconds <= 0) { swKame.Start(); } //Debug.Print("かーーめーーー"); }
                else if ((swHame.Elapsed.Milliseconds > 0) || Keyboard.IsKeyDown(Key.D)) { swHa.Start(); HaaFlash(); }//Debug.Print("波あああああ！！！"); }
            }
            else if (func2(rHand, lHand, pelvis, 0.1f))
            {
                swKame.Stop();
                if ((swKame.Elapsed.Milliseconds > 0) || Keyboard.IsKeyDown(Key.S)) { swHame.Start(); HameCharge(); }//Debug.Print("はーーーーめーーー"); }
            }
            //Debug.Print("かめ:" + swKame.Elapsed.Seconds.ToString() + "はめ:" + swHame.Elapsed.Seconds.ToString() + "はあああ:" + swHa.Elapsed.Seconds.ToString());
            swHa.Stop();
            if (swHa.Elapsed.Seconds > 3 && !func1(rHand, lHand, 0.1f)) { swKame.Reset(); swHame.Reset(); swHa.Reset(); }
            else if (swHa.Elapsed.Milliseconds != 0) { swHa.Start(); }
        }

        private void HameCharge()
        {
            int[] targetID = new int[] { 2 };
            CancellationTokenSource cts = new CancellationTokenSource();
            Vector3 tmpVec = new Vector3(180f, 255f, 120f);
            HSB hsb = new HSB((int)(360 * (1.0f - tmpVec.X) * 180.4f), (byte)(255 * (1.0f - tmpVec.Z)), (byte)(255 * (1.0f - tmpVec.Y)));
            hsb.Brightness = 255;
            int intime = 3000;
            MakeLightList(targetID).SetState(cts.Token, hsb.GetRGB(), 1, TimeSpan.FromMilliseconds(intime));
        }

        private async void HaaFlash()
        {
            int[] ids = new int[] { 2, 3, 5, 6, 7, 8, 9, 10, 11 };
            CancellationTokenSource cts = new CancellationTokenSource();
            Vector3 tmpVec = new Vector3(180f, 255f, 120f);
            HSB hsb = new HSB((int)(360 * (1.0f - tmpVec.X) * 180.4f), (byte)(255 * (1.0f - tmpVec.Z)), (byte)(255 * (1.0f - tmpVec.Y)));
            hsb.Brightness = 255;
            for (int i = 0; i < ids.Length; i++)
            {
                int[] IDs = new int[] { ids[i] };
                MakeLightList(IDs).SetState(CancellationToken.None, hsb.GetRGB(), 2, TimeSpan.FromSeconds(0.1f));
                await Task.Delay(10);
            }
            hsb.Brightness = 0;
            for (int i = 0; i < ids.Length; i++)
            {
                int[] IDs = new int[] { ids[i] };
                MakeLightList(IDs).SetState(CancellationToken.None, hsb.GetRGB(), 2, TimeSpan.FromSeconds(0.1f));
                await Task.Delay(10);
            }
        }
        #endregion


        private ANGLE _currentAgl;//X=Vertical Y=Horizontal
        private void CalculateAngle(Vector3 v0, Vector3 v1)
        {
            double radV = Math.Atan2(v1.Z - v0.Z, v1.Y - v0.Y);
            double radH = Math.Atan2(v1.Z - v0.Z, v1.X - v0.X);
            float aglV = (float)(radV * 180d / Math.PI);
            float aglH = (float)(radH * 180d / Math.PI) * (-1f);
            _currentAgl = new ANGLE(aglV, aglH);
            //Debug.Print(_currentAgl.H + "," + _currentAgl.V);
        }
        private void MetroWindow_Closed(object sender, EventArgs e)
        {
            if (_oscReceiver != null)
                _oscReceiver.Close();
        }

        //LIMITANGLE _limitAgl = new LIMITANGLE(20d, 30d, 60d, 140d);
        //LIMITPOSITION _limitPosition = new LIMITPOSITION(0, 0, 0, 0);
        LIMITVALUE _limitValue;
        private void InitializeLimit()
        {
            _limitValue = new LIMITVALUE();
            _limitValue.AGL = new LIMITANGLES(new ANGLE(20d, 0d), new ANGLE(30d, 0d), new ANGLE(0d, 60d), new ANGLE(0d, 140d));
            _limitValue.POS = new LIMITPOSITIONS();//from OSC data
        }
        private void btUBRL_Click(object sender, RoutedEventArgs e)
        {
            Button btn = (Button)sender;
            string tag = btn.Tag.ToString();
            if (tag == "UPPER")
            {
                _limitValue.POS.UPPER = _currentTrackedVec3;
                _limitValue.SetAngle(tag);//Set ANGLE
                btn.Content = (float)_limitValue.POS.UPPER.PELVIS.Z;
            }
            else if (tag == "BOTTOM")
            {
                _limitValue.POS.BOTTOM = _currentTrackedVec3;
                _limitValue.SetAngle(tag);
                btn.Content = (float)_limitValue.POS.BOTTOM.PELVIS.Z;
            }
            else if (tag == "LEFT")
            {
                _limitValue.POS.LEFT = _currentTrackedVec3;
                _limitValue.SetAngle(tag);
                btn.Content = (int)_limitValue.AGL.LEFT.H;
            }
            else if (tag == "RIGHT")
            {
                _limitValue.POS.RIGHT = _currentTrackedVec3;
                _limitValue.SetAngle(tag);
                btn.Content = (int)_limitValue.AGL.RIGHT.H;
            }
        }

        private void tsLEFT_Toggled(object sender, RoutedEventArgs e)
        {
            if (tsLEFT.IsOn) _currentTrackedVec3.LEFT_MODE = true;
            else _currentTrackedVec3.LEFT_MODE = false;
            _limitValue.ChangeLEFT_MODE(_currentTrackedVec3.LEFT_MODE);
        }

        private void tsThrow_Toggled(object sender, RoutedEventArgs e)
        {
        }

        private void btnReset_Click(object sender, RoutedEventArgs e)
        {
            sw.Reset();
            swKame.Reset();
            swHame.Reset();
            swHa.Reset();
            HSB hsb = currentHSB;
            hsb.Brightness = 0;
            MakeLightList(targetIDs).SetState(CancellationToken.None, hsb.GetRGB(), 1, TimeSpan.FromSeconds(0.1f));
        }

        private void btnThrow_Click(object sender, RoutedEventArgs e)
        {
            ThrowingFlash();
        }

        private void btnConnect_Click(object sender, RoutedEventArgs e)
        {
            ConnectBridgeWindow connect = new();
            try
            {
                connect.Show();
            }
            catch
            {
                connect = new();
                connect.Show();
            }
        }

        private void btnHame_Click(object sender, RoutedEventArgs e)
        {
            HameCharge();
        }



        private void btnHa_Click(object sender, RoutedEventArgs e)
        {
            HaaFlash();
        }
    }
}
