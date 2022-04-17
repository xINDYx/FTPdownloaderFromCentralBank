using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Configuration;
using System.Collections.Specialized;

namespace FTPDownloader
{
    public partial class Form2 : Form
    {
        public Form2()
        {
            InitializeComponent();
        }

        private void button1_Click(object sender, EventArgs e)
        {
            using (var dialog = new FolderBrowserDialog())
                if (dialog.ShowDialog() == DialogResult.OK)
                    textBox2.Text = dialog.SelectedPath;
        }

        private void button2_Click(object sender, EventArgs e)
        {
            using (var dialog = new FolderBrowserDialog())
                if (dialog.ShowDialog() == DialogResult.OK)
                    textBox3.Text = dialog.SelectedPath;
        }

        private void button3_Click(object sender, EventArgs e)
        {
            AddUpdateAppSettings("Адрес сервера", textBox1.Text);
            AddUpdateAppSettings("Папка для сохранения всех файлов", textBox2.Text);
            AddUpdateAppSettings("Папка для справочников", textBox3.Text);
            AddUpdateAppSettings("Интервал синхронизации", textBox4.Text);

            this.Close();
        }

        static void AddUpdateAppSettings(string key, string value)
        {
            try
            {
                var configFile = ConfigurationManager.OpenExeConfiguration(ConfigurationUserLevel.None);
                var settings = configFile.AppSettings.Settings;
                if (settings[key] == null)
                {
                    settings.Add(key, value);
                }
                else
                {
                    settings[key].Value = value;
                }
                configFile.Save(ConfigurationSaveMode.Modified);
                ConfigurationManager.RefreshSection(configFile.AppSettings.SectionInformation.Name);
            }
            catch (ConfigurationErrorsException)
            {
                Console.WriteLine("Error writing app settings");
            }
        }

        static String ReadSetting(string key)
        {
            try
            {
                var appSettings = ConfigurationManager.AppSettings;
                string result = appSettings[key] ?? "Not Found";
                return result;
            }
            catch (ConfigurationErrorsException)
            {
                return "Error reading app settings";
            }
        }

        private void Form2_Load(object sender, EventArgs e)
        {
            textBox1.Text = ReadSetting("Адрес сервера");
            textBox2.Text = ReadSetting("Папка для сохранения всех файлов");
            textBox3.Text = ReadSetting("Папка для справочников");
            textBox4.Text = ReadSetting("Интервал синхронизации");            
        }
    }
}
