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
    public partial class XTEAWindow : Window
    {
        XTEA XTEA;
        Regex Numbers = new Regex("^[0-9]*$");
        FileSystemWatcher watcher;
        string DestinationFolder; 
        string DestinationDecFolder;
        string WatchedFolder2;
        string KeysLocation2;
        public XTEAWindow()
        {

            InitializeComponent();
        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            this.Plaintext.Text = "";
            Ciphertext.Text = "";
        }

        #region check

        public bool checkAll()
        {
            if (Numbers.IsMatch(Rounds.Text) && KeyBox.Text != "")
                return true;
            return false;
        }

        private void KeyBox_TextChanged(object sender, TextChangedEventArgs e)
        {
            Wkey = KeyBox.Text;
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
            KeyBox.Text = "1234568789";
            Rounds.Text = "12";
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
                Plaintext.Text = File.ReadAllText(openFileDialog.FileName);
            else
                return;

            string file = openFileDialog.FileName.Split("\\").Last();

            XTEA = new XTEA();
            string enc = XTEA.Encrypt(Plaintext.Text, KeyBox.Text, uint.Parse(Rounds.Text));
            Ciphertext.Text = enc;

            Directory.CreateDirectory(DestinationFolder);
            Directory.CreateDirectory(KeysLocation2);
            SaveKeys(KeyBox.Text, Rounds.Text, file);
            File.WriteAllText(DestinationFolder + "\\" + file, enc);
        }
        private void Decrypt_Click(object sender, RoutedEventArgs e)
        {
            OpenFileDialog openFileDialog = new OpenFileDialog();
            openFileDialog.Filter = "Text files (*.txt)|*.txt|All files (*.*)|*.*";
            if (openFileDialog.ShowDialog() == true)
                Plaintext.Text = File.ReadAllText(openFileDialog.FileName);
            else 
                return;

            string file = openFileDialog.FileName.Split("\\").Last();
            if (!File.Exists(KeysLocation2 + "\\" + file))
            {
                MessageBox.Show("There are no keys saved for this file.");
                return;
            }
            string [] keys = File.ReadAllLines(KeysLocation2 + "\\" + file);

            XTEA = new XTEA();
            string dec = XTEA.Decrypt(Plaintext.Text, keys[0], uint.Parse(keys[1]));
            Ciphertext.Text = dec;

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

        private void SaveKeys(string rotor, string ringsettings, string file)
        {
            using (StreamWriter writetext = new StreamWriter(KeysLocation2 + "\\" + file))
            {
                writetext.WriteLine(rotor);
                writetext.WriteLine(ringsettings);
            }
        }

        string Wround;
        string Wkey;

        private void FileSystemWatcherToggle_Checked(object sender, RoutedEventArgs e)
        {
            if (!checkAll())
            {
                FileSystemWatcherToggle.IsChecked = false;
                return;
            }
            KeyBox.IsEnabled = false;
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
        }

        private void FileSystemWatcherToggle_Unchecked(object sender, RoutedEventArgs e)
        {
            KeyBox.IsEnabled = true;
            DestinationDec.IsEnabled = true;
            KeysLocation.IsEnabled = true;
            Destination.IsEnabled = true;
            Encrypt.IsEnabled = true;
            Decrypt.IsEnabled = true;
            WatchedFolder.IsEnabled = true;
        }

        private void WatcherEncrypt(object sender, FileSystemEventArgs e)
        {
            string file = e.Name;
            XTEA = new XTEA();
            string enc = XTEA.Encrypt(File.ReadAllText(e.FullPath), Wkey, uint.Parse(Wround));

            Directory.CreateDirectory(DestinationFolder);
            Directory.CreateDirectory(KeysLocation2);
            SaveKeys( Wkey, Wround, file);
            File.WriteAllText(DestinationFolder + "\\" + file, enc);
        }

        private void Rounds_TextChanged(object sender, TextChangedEventArgs e)
        {
            if (Numbers.IsMatch(Rounds.Text))
            {
                Rounds.Foreground = Brushes.Black;
                Wround = Rounds.Text;
            }
            else
                Rounds.Foreground = Brushes.Red;
        }
    }
}
