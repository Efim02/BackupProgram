
namespace BackupCopying
{
    class Setting
    {
        public string[] sourcePath { get; set; }
        public string targetPath { get; set; }
        public string levelLogo { get; set; }       //Можно было бы реализовать с помощью перечисления, воспользовавшись сторонними библиотеками, но если без этого, в json файлах сохраняются цифрами, а это нам не нужно поэтому string
    }
    enum LevelLog
    {
        Debug,
        Info,
        Error
    }
}
