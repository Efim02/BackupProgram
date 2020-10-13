using System;
using System.Text.Json;
using System.IO;
using System.Threading.Tasks;
using System.IO.Compression;

using static System.Console;

namespace BackupCopying
{
    class Program
    {
        static Setting setting;
        static LevelLog levelLog = LevelLog.Debug;
        static string logPath = null;
        static async Task Main()
        {
            try
            {
                await ReadSettingBuckap();
                ArchivingDirectory();
                WriteLogFile("The program has finished working", LevelLog.Info);
            }
            catch (Exception ex)
            {
                WriteLine(ex.Message);
                WriteLine("Press enter or close program . . .");          //Добавлено что бы можно было увидеть фразу ошибки
                Read();
            }
        }
        static void ArchivingDirectory()
        {
            string pathArchiv = logPath.Replace("_LOG.txt", ".zip");
            if (File.Exists(pathArchiv))            //Проверка на существования файла с таким же именем (что не возможно)
            {
                File.Delete(pathArchiv);
                WriteLogFile("An archive with the same name was deleted", LevelLog.Info);
            }
            DirectoryInfo directory = new DirectoryInfo($@"{setting.targetPath}/FolderTemporary");
            if (directory.Exists)
            {
                directory.Delete(true);
                WriteLogFile("A folder with the same name as the Temporary folder was deleted.", LevelLog.Info);
            }
            directory.Create();
            WriteLogFile("A temporary folder has been created from which the archive will be created", LevelLog.Debug);

            bool hasFiles = false;

            for (int i = 0; i < setting.sourcePath.Length; i++)     //Копируем каталоги (названия) и их файлы во временную папку
            {
                if (Directory.Exists(setting.sourcePath[i]))           //Проверка на существование исходной папки
                {
                    hasFiles = true;
                    WriteLogFile("The source folder is being processed - " + setting.sourcePath[i], LevelLog.Info);
                    DirectoryInfo info = new DirectoryInfo(setting.sourcePath[i]);      //Приходится так не экономно использовать память так как мы не знаем как запишут изначально путь (локально)
                    string newFolder = Directory.CreateDirectory(directory.FullName+"/"+info.Name).FullName;
                    foreach (string dirPath in Directory.GetDirectories(info.FullName, "*", SearchOption.AllDirectories))
                    {
                        string path = dirPath.Replace(info.FullName, newFolder);
                        Directory.CreateDirectory(path);
                        WriteLogFile("A folder with the same name as the original was copied - "+path, LevelLog.Debug);
                    }

                    foreach (string newPath in Directory.GetFiles(info.FullName, "*.*", SearchOption.AllDirectories))
                    {
                        string path = newPath.Replace(info.FullName, newFolder);
                        File.Copy(newPath, path, true);
                        WriteLogFile("A file with the same name as the original was copied - " + path, LevelLog.Debug);
                    }
                }
                else
                {
                    WriteLogFile(setting.sourcePath[i] + " -> The path to the source folder is incorrect or does not exist", LevelLog.Error);
                }
            }
            if (hasFiles)
            {
                ZipFile.CreateFromDirectory(directory.FullName, pathArchiv, CompressionLevel.Optimal, false);
                WriteLogFile("An archive is created - " + directory.FullName, LevelLog.Debug);
            }
            directory.Delete(true);
            WriteLogFile("Temporary folder is deleted - " + directory.FullName, LevelLog.Debug);
        }
        static async Task ReadSettingBuckap()       //Считывает setting.json
        {
            if(File.Exists("setting.json"))
            using (FileStream fs = new FileStream("setting.json", FileMode.Open))
            {
                var options = new JsonSerializerOptions { WriteIndented = true };
                setting = await JsonSerializer.DeserializeAsync<Setting>(fs, options);
                WriteLogFile("LOG", LevelLog.Info);     //Инициализация лог файла
            }
            else
            {
                await WriteSettingBuckap();
                throw new Exception("-setting.json- is missing, so a new file was created, write down the new parameters, and restart the program!");
            }
        }
        static async Task WriteSettingBuckap()  //Пересоздает setting.json если его нет
        {
            var options = new JsonSerializerOptions { WriteIndented = true };
            Setting setting = new Setting { sourcePath = new string[] { "source1", "source3" }, targetPath = "target", levelLogo = "Debug"};
            FileStream fs = new FileStream("setting.json", FileMode.OpenOrCreate);
            await JsonSerializer.SerializeAsync<Setting>(fs, setting, options);
            fs.Close();
        }
        static void WriteLogFile(string mes, LevelLog level)     //Запись в лог файл
        {
            if (logPath == null)
            {
                logPath = setting.targetPath + "/" + DateTime.Now.ToString().Replace(":", "_").Replace(".", "_").Replace(" ", "_") + "_LOG.txt";
                if (!Directory.Exists(setting.targetPath))      //Проверка на существование целевой папки
                    Directory.CreateDirectory(setting.targetPath);
                using (StreamWriter writer = new StreamWriter(logPath, true, System.Text.Encoding.UTF8)) { }    //Создаем таким образом лог файл
                if (setting.levelLogo == LevelLog.Debug.ToString())
                    levelLog = LevelLog.Debug;
                else if (setting.levelLogo == LevelLog.Info.ToString())
                    levelLog = LevelLog.Info;
                else if (setting.levelLogo == LevelLog.Error.ToString())
                    levelLog = LevelLog.Error;
                else
                {
                    levelLog = LevelLog.Debug;
                    WriteLogFile("LOG LEVEL IS SPECIFIED INCORRECTLY!", LevelLog.Error);
                }
                WriteLogFile("The program started working", LevelLog.Info);
                if (setting.sourcePath.Length == 0)
                    WriteLogFile("The source folders for the specified paths do not exist", LevelLog.Error);
            }
            else
            {
                if( (int)levelLog <= (int)level)
                using (StreamWriter writer = new StreamWriter(logPath, true, System.Text.Encoding.UTF8))
                {
                    writer.WriteLine(level.ToString() + ": " + mes);
                }
            }
        }
    }
}   