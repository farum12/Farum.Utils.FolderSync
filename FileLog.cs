namespace Farum.Utils.FolderSync
{
    public class FileLog
    {
        private static string _logFolder = string.Empty;

        private static void WriteMessageInConsoleAndFile(string message)
        {
            message = $"{ DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss:ff")} {message}";

            Console.WriteLine(message);
            using (StreamWriter sw = File.AppendText(Path.Combine(_logFolder, "Log.txt")))
            {
                sw.WriteLine(message);
            }
        }

        public static void DirectoryDeleted(string targetPath)
        {
            string message = "Successfully deleted directory and all its contents: " + targetPath;
            WriteMessageInConsoleAndFile(message);
        }

        public static void FileCopied(string sourcePath, string targetPath)
        {
            string message = $"Copying Source file from {sourcePath} to {targetPath}";
            WriteMessageInConsoleAndFile(message);
        }

        public static void FileUpdated(string sourcePath, string targetPath)
        {
            string message = $"Copying and overwriting Source file from {sourcePath} to {targetPath}";
            WriteMessageInConsoleAndFile(message);
        }

        public static void FileDeleted(string targetPath)
        {
            string message = "Successfully deleted file: " + targetPath;
            WriteMessageInConsoleAndFile(message);
        }

        public static void SetFolder(string path)
        {
            if (!path.EndsWith("\\"))
            {
                path = path + "\\";
            }

            if (!Path.Exists(path))
            {
                throw new ArgumentException("Input Path: " + path + " does not exist!");
            }
            else
            {
                Console.WriteLine("Path: " + path + " passed initial verification.");

                _logFolder = path;
            }
        }
    }
}
