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
using System.Windows.Navigation;
using System.Windows.Shapes;
using System.IO;
using Microsoft.Win32;
using System.Text.RegularExpressions;

namespace WpfApp2
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        Enigma enigma;
        Regex ATZ = new Regex("^[A-Z]{3}$");
        Regex ATC = new Regex("^[A-C]{3}$");
        Regex FileTest = new Regex("^[A-Za-z]+$");
        FileSystemWatcher watcher;
        string DestinationFolder; 
        string DestinationDecFolder;
        string WatchedFolder2;
        string KeysLocation2;
        public MainWindow()
        {

            InitializeComponent();
        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            this.Plaintext.Text = "";
            Ciphertext.Text = "";
        }

        #region check
        private void RingSettings_TextChanged(object sender, TextChangedEventArgs e)
        {
            if (ATZ.IsMatch(RingSettings.Text))
                RingSettings.Foreground = Brushes.Black;
            else
                RingSettings.Foreground = Brushes.Red;
        }

        private void Rotors_TextChanged(object sender, TextChangedEventArgs e)
        {
            if (ATC.IsMatch(Rotors.Text))
                Rotors.Foreground = Brushes.Black;
            else
                Rotors.Foreground = Brushes.Red;
        }

        private void KeySettings_TextChanged(object sender, TextChangedEventArgs e)
        {
            if (ATZ.IsMatch(KeySettings.Text))
                KeySettings.Foreground = Brushes.Black;
            else
                KeySettings.Foreground = Brushes.Red;
        }
        public bool checkAll()
        {
            if (ATZ.IsMatch(KeySettings.Text) && ATZ.IsMatch(RingSettings.Text) && ATC.IsMatch(Rotors.Text) && plugBoardTest)
                return true;
            return false;
        }

        bool plugBoardTest;

        private void PlugBoard_TextChanged(object sender, TextChangedEventArgs e)
        {
            Regex regex = new Regex("([A-Z][A-Z] ){12}[A-Z][A-Z]{1}");
            if(!regex.IsMatch(PlugBoard.Text))
            {
                plugBoardTest = false;
                PlugBoard.Foreground = Brushes.Red;
                return;
            }
            Dictionary<char, char> plug = new Dictionary<char, char>();
            String[] pairs = PlugBoard.Text.Split(" ");
            StringBuilder stringBuilder = new StringBuilder("YRUHQSLDPXNGOKMIEBFZCWVJAT");
            foreach (string pair in pairs)
            {
                if (!(plug.TryAdd(pair[0], pair[1]) && plug.TryAdd(pair[1], pair[0])))
                {
                    PlugBoard.Foreground = Brushes.Red;
                    plugBoardTest = false;
                    MessageBox.Show("One or more characters from pair: " + pair[0] + pair[1] + " is already used.");
                    return;
                }
                    stringBuilder[pair[0] - 'A'] = pair[1];
                    stringBuilder[pair[1] - 'A'] = pair[0];
            }
            PlugBoard.Foreground = Brushes.Black;
            plugBoardTest = true;
            Wplug = stringBuilder.ToString();
        }
        
        private void Destination_TextChanged(object sender, TextChangedEventArgs e)
        {
            if (Destination.Text.EndsWith("\\"))
                DestinationFolder = Destination.Text.Substring(0, Destination.Text.Length - 1);
            else
                DestinationFolder = Destination.Text;
        }

        private void DestinationDec_TextChanged(object sender, TextChangedEventArgs e)
        {
            if (DestinationDec.Text.EndsWith("\\"))
                DestinationDecFolder = DestinationDec.Text.Substring(0, DestinationDec.Text.Length - 1);
            else
                DestinationDecFolder = DestinationDec.Text;
        }
        private void WatchedFolder_TextChanged(object sender, TextChangedEventArgs e)
        {
            if (WatchedFolder.Text.EndsWith("\\"))
                WatchedFolder2 = WatchedFolder.Text.Substring(0, WatchedFolder.Text.Length - 1);
            else
                WatchedFolder2 = WatchedFolder.Text;
        }

        private void KeysLocation_TextChanged(object sender, TextChangedEventArgs e)
        {
            if (KeysLocation.Text.EndsWith("\\"))
                KeysLocation2 = KeysLocation.Text.Substring(0, KeysLocation.Text.Length - 1);
            else
                KeysLocation2 = KeysLocation.Text;
        }

        #endregion

        private void OpenFileBtn_Click(object sender, RoutedEventArgs e)
        {
            OpenFileDialog openFileDialog = new OpenFileDialog();
            openFileDialog.Filter = "Text files (*.txt)|*.txt|All files (*.*)|*.*";
            Directory.CreateDirectory(DestinationFolder);
            openFileDialog.InitialDirectory = DestinationFolder;
            if (openFileDialog.ShowDialog() == true)
                Plaintext.Text = File.ReadAllText(openFileDialog.FileName);
        }

        private void MainWindow1_Initialized(object sender, EventArgs e)
        {
            Plaintext.Text = "";
            Ciphertext.Text = "";
            PlugBoard.Text = "AY BR CU DH EQ FS GL IP JX KN MO TZ VW";
            DestinationDec.Text = "C:\\Users\\" + Environment.UserName + "\\Desktop\\ZastitaInformacija\\Decryption";
            WatchedFolder.Text = "C:\\Users\\" + Environment.UserName + "\\Desktop\\ZastitaInformacija\\Watched";
            KeysLocation.Text = "C:\\Users\\" + Environment.UserName + "\\Desktop\\ZastitaInformacija\\Kljucevi";
            Destination.Text = "C:\\Users\\" + Environment.UserName + "\\Desktop\\ZastitaInformacija\\Encryption";
        }


        private void Encrypt_Click(object sender, RoutedEventArgs e)
        {
            if (!checkAll())
                return;
            OpenFileDialog openFileDialog = new OpenFileDialog();
            openFileDialog.Filter = "Text files (*.txt)|*.txt|All files (*.*)|*.*";
            if (openFileDialog.ShowDialog() == true)
                Plaintext.Text = RemoveSpecialCharacters(File.ReadAllText(openFileDialog.FileName));
            else
                return;

            string file = openFileDialog.FileName.Split("\\").Last();

            enigma = new Enigma(Rotors.Text, RingSettings.Text, Wplug, KeySettings.Text);
            string enc = enigma.EncryptString(Plaintext.Text);
            Ciphertext.Text = enc;

            List<string> keys = new List<string>();
            keys.Add(Rotors.Text);
            keys.Add(RingSettings.Text);
            keys.Add(Wplug);
            keys.Add(KeySettings.Text);
            keys.Add(CRC.Hash(toUpper(Plaintext.Text)));

            Directory.CreateDirectory(DestinationFolder);
            Directory.CreateDirectory(KeysLocation2);
            SaveKeys(file, keys);
            File.WriteAllText(DestinationFolder + "\\" + file, enc);
        }
        private void Decrypt_Click(object sender, RoutedEventArgs e)
        {
            OpenFileDialog openFileDialog = new OpenFileDialog();
            openFileDialog.Filter = "Text files (*.txt)|*.txt|All files (*.*)|*.*";
            if (openFileDialog.ShowDialog() == true)
                Plaintext.Text = RemoveSpecialCharacters(File.ReadAllText(openFileDialog.FileName));
            else 
                return;

            string file = openFileDialog.FileName.Split("\\").Last();
            if (!File.Exists(KeysLocation2 + "\\" + file))
            {
                MessageBox.Show("There are no keys saved for this file.");
                return;
            }
            string [] keys = File.ReadAllLines(KeysLocation2 + "\\" + file);

            enigma = new Enigma(keys[0], keys[1], keys[2], keys[3]);
            string dec = enigma.EncryptString(Plaintext.Text);
            Ciphertext.Text = dec;

            if (CRCbox.IsChecked == true)
                if (keys[4] != CRC.Hash(dec))
                    MessageBox.Show("CRC hash does not match.");

            Directory.CreateDirectory(DestinationDecFolder);
            File.WriteAllText(DestinationDecFolder + "\\" + file, dec);
        }

        public string RemoveSpecialCharacters(string str)
        {
            StringBuilder sb = new StringBuilder();
            foreach (char c in str)
            {
                if ((c >= 'A' && c <= 'Z') || (c >= 'a' && c <= 'z'))
                {
                    sb.Append(c);
                }
            }
            return sb.ToString();
        }

        private string toUpper(string s)
        {
            StringBuilder stringBuilder = new StringBuilder();

            foreach (char c in s)
            {
                stringBuilder.Append(char.ToUpper(c));
            }
            return stringBuilder.ToString();
        }

        private void SaveKeys(string file, List<string> keys)
        {
            using (StreamWriter writetext = new StreamWriter(KeysLocation2 + "\\" + file))
                foreach (string key in keys)
                    writetext.WriteLine(key);

        }

        string Wrotors;
        string Wring;
        string Wkey;
        string Wplug;
        bool crcCheck;

        private void FileSystemWatcherToggle_Checked(object sender, RoutedEventArgs e)
        {
            if (!checkAll())
            {
                FileSystemWatcherToggle.IsChecked = false;
                return;
            }
            Wrotors = Rotors.Text;
            Wring = RingSettings.Text;
            Wkey = KeySettings.Text;
            Rotors.IsEnabled = false;
            PlugBoard.IsEnabled = false;
            RingSettings.IsEnabled = false;
            KeySettings.IsEnabled = false;
            Destination.IsEnabled = false;
            DestinationDec.IsEnabled = false;
            KeysLocation.IsEnabled = false;
            Encrypt.IsEnabled = false;
            Decrypt.IsEnabled = false;
            WatchedFolder.IsEnabled = false;
            Directory.CreateDirectory(WatchedFolder2);
            watcher = new FileSystemWatcher(WatchedFolder2);
            watcher.Created += WatcherEncrypt;
            watcher.EnableRaisingEvents = true;
            CRCbox.IsEnabled = false;
        }

        private void FileSystemWatcherToggle_Unchecked(object sender, RoutedEventArgs e)
        {
            PlugBoard.IsEnabled = true;
            Rotors.IsEnabled = true;
            RingSettings.IsEnabled = true;
            DestinationDec.IsEnabled = true;
            KeysLocation.IsEnabled = true;
            KeySettings.IsEnabled = true;
            Destination.IsEnabled = true;
            Encrypt.IsEnabled = true;
            Decrypt.IsEnabled = true;
            WatchedFolder.IsEnabled = true;
            CRCbox.IsEnabled = true;
        }

        private void WatcherEncrypt(object sender, FileSystemEventArgs e)
        {
            string file = e.Name;
            enigma = new Enigma(Wrotors, Wring, Wplug, Wkey);
            string enc = enigma.EncryptString(RemoveSpecialCharacters(File.ReadAllText(e.FullPath)));

            List<string> keys = new List<string>();
            keys.Add(Wrotors);
            keys.Add(Wring);
            keys.Add(Wplug);
            keys.Add(Wkey);
            keys.Add(CRC.Hash(toUpper(RemoveSpecialCharacters(File.ReadAllText(e.FullPath)))));


            Directory.CreateDirectory(DestinationFolder);
            Directory.CreateDirectory(KeysLocation2);
            SaveKeys(file, keys);
            File.WriteAllText(DestinationFolder + "\\" + file, enc);
        }

        private void CRCbox_Checked(object sender, RoutedEventArgs e)
        {
            crcCheck = true;
        }

        private void CRCbox_Unchecked(object sender, RoutedEventArgs e)
        {
            crcCheck = false;
        }
    }
}
