using Q42.HueApi.Interfaces;
using Q42.HueApi;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;
using System.Windows.Navigation;
using System.Diagnostics;
using Org.BouncyCastle.Asn1.X509;

namespace WpfHueEntDemo
{
    /// <summary>
    /// ConnectBridgeWindow.xaml の相互作用ロジック
    /// </summary>
    public partial class ConnectBridgeWindow : Window
    {
        private List<BridgeClass> bridgeClasses = new();



        public ConnectBridgeWindow()
        {
            InitializeComponent();
            if (GetBridgesInfo() != null)
                bridgeClasses = GetBridgesInfo();    
            ViewBridges();
        }


        public static List<BridgeClass>? GetBridgesInfo()
        {
            List<BridgeClass> b = new();
            if (Properties.Settings.Default.IPs == null)
                return null;
            int bridgeNum = Properties.Settings.Default.Names.Count;
            if (bridgeNum <= 0)
                return null;

            b = new();
            for (int i = 0; i < bridgeNum; i++)
            {
                string name = Properties.Settings.Default.Names[i];
                string ip = Properties.Settings.Default.IPs[i];
                string appkey = Properties.Settings.Default.AppKeys[i];
                string enkey = Properties.Settings.Default.EnKeys[i];
                b.Add(new BridgeClass(ip, appkey, enkey, name));
            }

            return b;
        }


        private async void btnGET_Click(object sender, RoutedEventArgs e)
        {
            if (tbBrigdeIp.Text == "")
                return;

            string ip = tbBrigdeIp.Text;
            string appName = "myhueapp";
            string devName = "myhuedevice";

            ILocalHueClient hueClient = new LocalHueClient(ip);
            MessageBox.Show("Press your bridge button!");
            Q42.HueApi.Models.Bridge.RegisterEntertainmentResult? bridgeInfo;
            try
            {
                bridgeInfo = await hueClient.RegisterAsync(appName, devName, true);
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
                return;
            }

            if (bridgeInfo == null)
                return;


            tbAppKey.Text = bridgeInfo.Username;
            tbEnKey.Text = bridgeInfo.StreamingClientKey;
        }

        private void btnAddBridge_Click(object sender, RoutedEventArgs e)
        {
            if (tbAppKey.Text == "")
                return;
            else if (tbEnKey.Text == "")
                return;
            else if (tbName.Text == "")
            {
                var result = MessageBox.Show("Don't you have to name this bridge?", "Name Check", MessageBoxButton.YesNo, MessageBoxImage.Question);
                if (result == MessageBoxResult.No)
                    return;
            }

            //initialize setting lists.
            if (Properties.Settings.Default.IPs == null)
            {
                Properties.Settings.Default.Names = new();
                Properties.Settings.Default.IPs = new();
                Properties.Settings.Default.AppKeys = new();
                Properties.Settings.Default.EnKeys = new();
            }
            else
            {
                Properties.Settings.Default.Names = new List<string>(Properties.Settings.Default.Names);
                Properties.Settings.Default.IPs = new List<string>(Properties.Settings.Default.IPs);
                Properties.Settings.Default.AppKeys = new List<string>(Properties.Settings.Default.AppKeys);
                Properties.Settings.Default.EnKeys.Add(tbEnKey.Text);
            }

            //add setting lists.
            Properties.Settings.Default.Names.Add(tbName.Text);
            Properties.Settings.Default.IPs.Add(tbBrigdeIp.Text);
            Properties.Settings.Default.AppKeys.Add(tbAppKey.Text);
            Properties.Settings.Default.EnKeys.Add(tbEnKey.Text);
            Properties.Settings.Default.Save();

            bridgeClasses = GetBridgesInfo();
            ViewBridges();
        }

        private void ViewBridges()
        {
            Action<object, EventArgs, BridgeClass, List<TextBox>> actionEdit = (sender, e, _b, _tbs) =>
            {
                int index = bridgeClasses.FindIndex(x => x.IP == _b.IP);
                var _ip = _tbs[1].Text;
                var _name = _tbs[0].Text;
                var _app = _tbs[2].Text;
                var _en = _tbs[3].Text;
                Properties.Settings.Default.IPs[index] = _ip;
                Properties.Settings.Default.Names[index] = _name;
                Properties.Settings.Default.AppKeys[index] = _app;
                Properties.Settings.Default.EnKeys[index] = _en;
                Properties.Settings.Default.Save();
                bridgeClasses = GetBridgesInfo();
                ViewBridges();
                MessageBox.Show("Changed \"" + _name + "\"'s information successfully.");
            };

            Action<object, EventArgs, BridgeClass> actionDelete = (sender, e, _b) =>
            {
                if (MessageBox.Show("Are you sure deleting this Bridge info?", "Delete Check", MessageBoxButton.YesNo) == MessageBoxResult.No)
                    return;
                int index = bridgeClasses.FindIndex(x => x.IP == _b.IP);
                Properties.Settings.Default.IPs.RemoveAt(index);
                Properties.Settings.Default.Names.RemoveAt(index);
                Properties.Settings.Default.AppKeys.RemoveAt(index);
                Properties.Settings.Default.EnKeys.RemoveAt(index);
                Properties.Settings.Default.Save();
                bridgeClasses = GetBridgesInfo();
                ViewBridges();
            };

            spBridgeList.Children.Clear();
            StackPanel _spB = new();
            _spB.Margin = new Thickness(0, 0, 0, 10);
            bridgeClasses.ForEach(b =>
            {
                List<TextBox> tblist = new();

                TextBox _tbName = new();
                _tbName.Text = b.NAME;
                _tbName.FontSize = 20;
                tblist.Add(_tbName);

                TextBox _tbIP = new();
                _tbIP.Text = b.IP;
                _tbIP.FontSize = 14;
                tblist.Add(_tbIP);

                TextBox _tbAppKey = new();
                _tbAppKey.Text = b.APP_KEY;
                _tbAppKey.FontSize = 14;
                tblist.Add(_tbAppKey);

                TextBox _tbEnKey = new();
                _tbEnKey.Text = b.EN_KEY;
                _tbEnKey.FontSize = 14;
                tblist.Add(_tbEnKey);

                StackPanel _spBtns = new();
                _spBtns.Orientation = Orientation.Horizontal;
                Button btnEdit = new();
                Button btnDelete = new();
                btnEdit.Click += (sender, e) => actionEdit(sender, e, b, tblist);
                btnEdit.Content = "Edit(refer IP)";
                btnEdit.Margin = new Thickness(3, 3, 3, 3);
                btnDelete.Click += (sender, e) => actionDelete(sender, e, b);
                btnDelete.Content = "Delete(refer IP)";
                btnDelete.Margin = new Thickness(3, 3, 3, 3);
                _spBtns.Children.Add(btnEdit);
                _spBtns.Children.Add(btnDelete);

                _spB.Children.Add(_tbName);
                _spB.Children.Add(_tbIP);
                _spB.Children.Add(_tbAppKey);
                _spB.Children.Add(_tbEnKey);
                _spB.Children.Add(_spBtns);
            });
            spBridgeList.Children.Add(_spB);
        }
    }
}
