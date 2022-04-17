using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Threading;
using System.Windows.Forms;
using System.Configuration;
using System.Collections.Specialized;
using System.Net;
using System.IO;
using System.IO.Compression;
using System.Reflection;
using System.Diagnostics;

namespace FTPDownloader
{
    public partial class Form1 : Form
    {
        public Form1()
        {
            InitializeComponent();
        }

        private void настройкиToolStripMenuItem_Click(object sender, EventArgs e)
        {
            Form settings = new Form2();
            settings.Show(); // отображаем Form2
            //this.Hide(); // скрываем Form1 (this - текущая форма)
        }

        private void оПрограммеToolStripMenuItem_Click(object sender, EventArgs e)
        {
            Form about = new Form3();
            about.Show(); // отображаем Form3
            //this.Hide(); // скрываем Form1 (this - текущая форма)
        }

        private void button1_Click(object sender, EventArgs e)
        {
            string timeToDownload = ReadSetting("Интервал синхронизации");
            int timeINT = Convert.ToInt32(timeToDownload)*60000;
            timer1.Interval = timeINT;
            timer1.Start();                      

            if (!Directory.Exists(@"Temp")) Directory.CreateDirectory(@"Temp");
            if (!Directory.Exists(@"Txt")) Directory.CreateDirectory(@"Txt");

            string currentDate = DateTime.Now.Date.ToString("dd-MM-yy") + ".txt";

            string server = ReadSetting("Адрес сервера");
            string folderForAll = ReadSetting("Папка для сохранения всех файлов");
            string folderForDBF = ReadSetting("Папка для справочников");
            DirectoryInfo destinDBF = new DirectoryInfo(folderForDBF + "\\");
            DirectoryInfo sourceTemp = new DirectoryInfo(@"Temp");

            //Скачивание файла рассылки
            string remoteUri = server + currentDate; //DD-MM-YY.txt         
            currentDate = @"Txt\" + currentDate;

            if (!File.Exists(currentDate))
            {
                printToLoggerAndList("Текущий файл рассылки " + remoteUri);
                if (!downloadFile(remoteUri, currentDate) ) return;               

                //Удаление всех файлов из папки Temp            
                printToLoggerAndList("Начался процесс очистки папки Temp");

                deletTempFiles();

                //Скачивание всех файлов, указанных в файле-рассылке
                printToLoggerAndList("Запуск процесса скачивания файлов согласно файлу рассылки");

                downloadAllFils();

                //Копирование всех файлов из Temp в папку сохранения всех файлов            
                copyAllFiles();

                //Разархивирование всех файлов в папке Temp 

                unArjAllFiles();
                Thread.Sleep(1000);

                //Копирование файлов *.dbf в папку справочников
                printToLoggerAndList("Начался процесс копирования всех файлов справочников в папку " + folderForDBF);

                copyToDBF();
            }
            else 
            {
                printToLoggerAndList("За текущую дату файл рассылки уже был скачан и обработан");
            }       

        }

        private void Form1_Load(object sender, EventArgs e)
        {
           
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

        private void timer1_Tick(object sender, EventArgs e)
        {
            if (!Directory.Exists(@"Temp")) Directory.CreateDirectory(@"Temp");
            if (!Directory.Exists(@"Txt")) Directory.CreateDirectory(@"Txt");

            string currentDate = DateTime.Now.Date.ToString("dd-MM-yy") + ".txt";

            string server = ReadSetting("Адрес сервера");
            string folderForAll = ReadSetting("Папка для сохранения всех файлов");
            string folderForDBF = ReadSetting("Папка для справочников");
            DirectoryInfo destinDBF = new DirectoryInfo(folderForDBF + "\\");
            DirectoryInfo sourceTemp = new DirectoryInfo(@"Temp");

            //Скачивание файла рассылки
            string remoteUri = server + currentDate; //DD-MM-YY.txt         
            currentDate = @"Txt\" + currentDate;

            if (!File.Exists(currentDate))
            {
                printToLoggerAndList("Текущий файл рассылки " + remoteUri);
                if (!downloadFile(remoteUri, currentDate)) return;

                //Удаление всех файлов из папки Temp            
                printToLoggerAndList("Начался процесс очистки папки Temp");
                deletTempFiles();

                //Скачивание всех файлов, указанных в файле-рассылке
                printToLoggerAndList("Запуск процесса скачивания файлов согласно файлу рассылки");
                downloadAllFils();

                //Копирование всех файлов из Temp в папку сохранения всех файлов            
                copyAllFiles();

                //Разархивирование всех файлов в папке Temp 

                unArjAllFiles();
                Thread.Sleep(1000);

                //Копирование файлов *.dbf в папку справочников
                printToLoggerAndList("Начался процесс копирования всех файлов справочников в папку " + folderForDBF);

                copyToDBF();
            }
            else
            {
                printToLoggerAndList("За текущую дату файл рассылки уже был скачан и обработан");
            }

        }
        static class Logger
        {
            //----------------------------------------------------------
            // Статический метод записи строки в файл лога без переноса
            //----------------------------------------------------------
            public static void Write(string text)
            {
                using (StreamWriter sw = new StreamWriter(Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location) + "\\log.txt", true))
                {
                    sw.Write(text);
                }
            }

            //---------------------------------------------------------
            // Статический метод записи строки в файл лога с переносом
            //---------------------------------------------------------
            public static void WriteLine(string message)
            {
                using (StreamWriter sw = new StreamWriter(Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location) + "\\log.txt", true))
                {
                    sw.WriteLine(String.Format("{0,-23} {1}", DateTime.Now.ToString() + ":", message));
                }
            }
        }

        bool downloadFile(String uri, String path) {
            try
            {
                WebClient myWebClient = new WebClient();
                myWebClient.DownloadFile(uri, path);
                printToLoggerAndList("Файл " + uri + " скачался в файл " + path);
                return true;
            }
            catch
            {
                printToLoggerAndList("Ошибка при скачивании файла " + uri); 
                return false;
            }
        }

        void copyAllFiles() {
            string folderForAll = ReadSetting("Папка для сохранения всех файлов");
            DirectoryInfo sourceTemp = new DirectoryInfo(@"Temp");

            string currentMonth = DateTime.Now.Date.ToString("MM");
            string currentYear = DateTime.Now.Date.ToString("yyyy");

            if (!Directory.Exists(folderForAll + "\\" + currentYear)) {
                Directory.CreateDirectory(folderForAll + "\\" + currentYear);
                printToLoggerAndList("Создана папка " + folderForAll + "\\" + currentYear);                
            }

            if (!Directory.Exists(folderForAll + "\\" + currentYear + "\\" + currentMonth)) {
                Directory.CreateDirectory(folderForAll + "\\" + currentYear + "\\" + currentMonth);
                printToLoggerAndList("Создана папка " + folderForAll + "\\" + currentYear + "\\" + currentMonth);
            }

            printToLoggerAndList("Начался процесс копирования всех файлов в папку " + folderForAll + "\\" + currentYear + "\\" + currentMonth);           

            DirectoryInfo destin = new DirectoryInfo(folderForAll + "\\" + currentYear + "\\" + currentMonth + "\\");

            foreach (var item in sourceTemp.GetFiles())
            {
                item.CopyTo(destin + item.Name, true);
                printToLoggerAndList("Файл " + item.Name + " скопирован в " + folderForAll + "\\" + currentYear + "\\" + currentMonth);                
            }         

        }

        void downloadAllFils() {
            string currentDate = @"Txt\" + DateTime.Now.Date.ToString("dd-MM-yy") + ".txt";
            string server = ReadSetting("Адрес сервера");

            var lines = File.ReadAllLines(currentDate);
            int i = 0;
            foreach (var line in lines)
            {
                i++;
                string[] words = line.Split(new char[] { ' ' });
                if (!words[0].StartsWith("!!!") & i > 2)
                {
                    string fullPathToFile = server + words[0];
                    fullPathToFile = fullPathToFile.Replace("\\", "/");
                    string[] filePathMass = words[0].Split(new char[] { '\\' });
                    string fileNameFTP = "Temp" + "\\" + filePathMass[filePathMass.Length - 1].ToString();
                    printToLoggerAndList("Попытка скачать файл " + fullPathToFile + " в " + fileNameFTP);
                    downloadFile(fullPathToFile, fileNameFTP);
                }
            }

        }

        void unArjAllFiles() {
            try
            {
                Process.Start("7z.exe", " x " + "-y " + " -oTemp " + @"Temp\*.arj");                
                printToLoggerAndList("Начался процесс разархивирования файлов в папке Temp");
            }
            catch
            {
                printToLoggerAndList("Неудачное разархивирование в папке Temp");
            }
        }

        void copyToDBF() {
            string folderForDBF = ReadSetting("Папка для справочников");
            DirectoryInfo sourceTemp = new DirectoryInfo(@"Temp");
            DirectoryInfo destinDBF = new DirectoryInfo(folderForDBF + "\\");

            foreach (var item in sourceTemp.GetFiles())
            {
                Logger.WriteLine("Обработка файла для копирования в DBF" + item.Name);
                if (item.Extension.ToString().ToLower() == ".dbf")
                {
                    item.CopyTo(destinDBF + item.Name, true);
                    printToLoggerAndList("Файл " + item.Name + " скопирован в " + folderForDBF);                    
                }
            }
        }

        void deletTempFiles() {
            DirectoryInfo sourceTemp = new DirectoryInfo(@"Temp");
            foreach (var item in sourceTemp.GetFiles())
            {
                try
                {
                    item.Delete();
                    printToLoggerAndList("Файл " + item.Name + " удален из папки Temp");                    
                }
                catch
                {
                    printToLoggerAndList("Ошибка удаления файла " + item.Name + " из папки Temp");                    
                }
            }
        }

        void printToLoggerAndList(String s) {
            listBox1.Items.Add(s);
            listBox1.SelectedIndex = listBox1.Items.Count - 1;
            listBox1.SelectedIndex = -1;
            Logger.WriteLine(s);
        }

    }
}
