using Microsoft.VisualBasic.FileIO;
using System.Text.RegularExpressions;

namespace Farum.Utils.FolderSync
{
    public static class FolderSynchronization
    {
        private static string _sourceFolder = string.Empty;
        private static string _replicaFolder = string.Empty;

        // Interval in ms
        private static int _interval;

        private static List<string> _sourceRelativeDirectories = new List<string>();
        private static List<string> _sourceRelativeFiles = new List<string>();

        private static List<string> _replicaRelativeDirectories = new List<string>();
        private static List<string> _replicaRelativeFiles = new List<string>();

        /// <summary>
        /// Method gets directories and updates _RelativeDirectories List from target folder.
        /// </summary>
        /// <param name="source">Folder type: FolderTypeEnum.Source | FolderTypeEnum.Replica</param>
        private static void GetAllDirectiries(FolderTypeEnum source)
        {
            if (source == FolderTypeEnum.Source)
            {
                _sourceRelativeDirectories.Clear();

                foreach (var directory in Directory.GetDirectories(_sourceFolder, "*.*", System.IO.SearchOption.AllDirectories))
                {
                    string relativeDir = directory.Replace(_sourceFolder, "");
                    _sourceRelativeDirectories.Add(relativeDir);
                }
            }

            if (source == FolderTypeEnum.Replica)
            {
                _replicaRelativeDirectories.Clear();

                foreach (var directory in Directory.GetDirectories(_replicaFolder, "*.*", System.IO.SearchOption.AllDirectories))
                {
                    string relativeDir = directory.Replace(_replicaFolder, "");
                    _replicaRelativeDirectories.Add(relativeDir);
                }
            }
        }

        /// <summary>
        /// Method gets all files from given directory and updates corresponding _relativeFiles List.
        /// </summary>
        /// <param name="source">Folder type: FolderTypeEnum.Source | FolderTypeEnum.Replica</param>
        private static void GetAllFiles(FolderTypeEnum source)
        {
            if (source == FolderTypeEnum.Source)
            {
                _sourceRelativeFiles.Clear();

                foreach (var file in Directory.GetFiles(_sourceFolder, "*.*", System.IO.SearchOption.AllDirectories))
                {
                    string relativeFileDir = file.Replace(_sourceFolder, "");
                    _sourceRelativeFiles.Add(relativeFileDir);
                }
            }

            if (source == FolderTypeEnum.Replica)
            {
                _replicaRelativeFiles.Clear();

                foreach (var file in Directory.GetFiles(_replicaFolder, "*.*", System.IO.SearchOption.AllDirectories))
                {
                    string relativeFileDir = file.Replace(_replicaFolder, "");
                    _replicaRelativeFiles.Add(relativeFileDir);
                }
            }
        }

        /// <summary>
        /// Method removes all unmatching directories present within Replica folder.
        /// This covers following scenarios:
        /// - Folder renaming in source
        /// - Folder deletion in source
        /// - Accidental new folder in replica (third party created folder in target folder)
        /// </summary>
        private static void DeleteDirectiories()
        {
            Console.WriteLine();
            Console.WriteLine("======================");
            Console.WriteLine("REPLICA CLEANUP - DIRECTORIES");
            Console.WriteLine("======================");

            GetAllDirectiries(FolderTypeEnum.Source);
            GetAllDirectiries(FolderTypeEnum.Replica);

            List<string> additionalDirsInReplica = _replicaRelativeDirectories.Except(_sourceRelativeDirectories).ToList();
            if(additionalDirsInReplica.Count != 0) 
            {
                Console.WriteLine("Additional directories in Replica:");
                foreach (string directory in additionalDirsInReplica)
                    Console.WriteLine(directory);

                // Delete additional directories
                foreach (string directory in additionalDirsInReplica)
                {
                    try
                    {
                        string targetPath = Path.Combine(_replicaFolder, directory);
                        FileSystem.DeleteDirectory(targetPath, UIOption.OnlyErrorDialogs, RecycleOption.SendToRecycleBin);
                        FileLog.DirectoryDeleted(targetPath);
                    }
                    catch (DirectoryNotFoundException ex)
                    {
                        // In case if parent dir will be deleted faster than child dir. 
                        Console.WriteLine("Attempted to delete directory: " + Path.Combine(_replicaFolder, directory) + " but it does not exist.");
                    }

                }
            }
            else
            {
                Console.WriteLine("No additional directories in Replica directory found.");
            }
        }

        /// <summary>
        /// Method removes all unmatching files present within Replica folder.
        /// This covers following scenarios:
        /// - File renaming in source
        /// - File deletion in source
        /// - Accidental new file in replica (third party created folder in target folder)
        /// </summary>
        private static void DeleteFiles()
        {
            Console.WriteLine();
            Console.WriteLine("======================");
            Console.WriteLine("REPLICA CLEANUP - FILES");
            Console.WriteLine("======================");
            GetAllFiles(FolderTypeEnum.Source);
            GetAllFiles(FolderTypeEnum.Replica);

            List<string> additionalFilesInReplica = _replicaRelativeFiles.Except(_sourceRelativeFiles).ToList();
            if (additionalFilesInReplica.Count != 0)
            {
                Console.WriteLine("Additional files in Replica:");
                foreach (string file in additionalFilesInReplica)
                    Console.WriteLine(file);

                // Delete additional directories
                foreach (string file in additionalFilesInReplica)
                {
                    try
                    {
                        string targetPath = Path.Combine(_replicaFolder, file);
                        FileSystem.DeleteFile(Path.Combine(_replicaFolder, file), UIOption.OnlyErrorDialogs, RecycleOption.SendToRecycleBin);
                        FileLog.FileDeleted(targetPath);
                    }
                    catch (FileNotFoundException ex)
                    {
                        Console.WriteLine("Attempted to delete file: " + Path.Combine(_replicaFolder, file) + " but it does not exist.");
                    }

                }
            }
            else
            {
                Console.WriteLine("No additional files in Replica directory found.");
            }
        }

        /// <summary>
        /// Method which copies only new files from Source to Replica folder.
        /// </summary>
        private static void CopyNewFiles()
        {
            Console.WriteLine();
            Console.WriteLine("======================");
            Console.WriteLine("COPYING NEW FILES");
            Console.WriteLine("======================");
            GetAllFiles(FolderTypeEnum.Source);
            GetAllFiles(FolderTypeEnum.Replica);

            List<string> additionalFilesInSource = _sourceRelativeFiles.Except(_replicaRelativeFiles).ToList();
            if (additionalFilesInSource.Count != 0)
            {
                Console.WriteLine("New files in Source (missing in Replica):");
                foreach (string file in additionalFilesInSource)
                    Console.WriteLine(file);

                foreach (string file in additionalFilesInSource)
                {
                    try
                    {
                        string targetSourceFileName = Path.Combine(_sourceFolder, file);
                        string targetReplicaFileName = Path.Combine(_replicaFolder, file);
                        FileSystem.CopyFile(targetSourceFileName, targetReplicaFileName, false);
                        FileLog.FileCopied(targetSourceFileName, targetReplicaFileName);

                    } catch (IOException ex)
                    {
                        Console.WriteLine($"Target file exist: {ex.Message}");
                    }
                }
            }
            else 
            {
                Console.WriteLine("No new files in Source.");
            }
        }

        /// <summary>
        /// Method which analyze existing files both in Source and Replica, 
        /// compares them with MD5 and replaces if any difference found.
        /// </summary>
        private static void SyncExistingFiles()
        {
            Console.WriteLine();
            Console.WriteLine("======================");
            Console.WriteLine("SYNCING EXISTING FILES");
            Console.WriteLine("======================");
            GetAllFiles(FolderTypeEnum.Source);
            GetAllFiles(FolderTypeEnum.Replica);

            List<string> sameFilesInReplica = _replicaRelativeFiles.Intersect(_replicaRelativeFiles).ToList();
            if (sameFilesInReplica.Count != 0)
            {
                Console.WriteLine("Files existing both in Source and Replica:");
                foreach (string file in sameFilesInReplica)
                    Console.WriteLine(file);

                foreach (string file in sameFilesInReplica)
                {
                    string targetSourceFileName = Path.Combine(_sourceFolder, file);
                    string targetReplicaFileName = Path.Combine(_replicaFolder, file);

                    Md5Comparer md5Comparer = new Md5Comparer(targetSourceFileName, targetReplicaFileName);
                    var fileChangedFlag = md5Comparer.Compare();

                    if (fileChangedFlag)
                    {
                        Console.WriteLine("Source File did not change:" + file);
                    } else
                    {
                        Console.WriteLine("Detected difference in Source file: " + file);
                        FileLog.FileUpdated(targetSourceFileName, targetReplicaFileName);
                        FileSystem.CopyFile(targetSourceFileName, targetReplicaFileName, true);
                    }
                }
            }
        }

        /// <summary>
        /// Main loop for the FolderSynchronization.
        /// </summary>
        /// <returns></returns>
        public static async Task Loop()
        {
            while (true)
            {
                ExecuteSync();
                await Task.Delay(_interval);
            }
        }

        public static void ExecuteSync()
        {
            DeleteDirectiories();
            DeleteFiles();
            CopyNewFiles();
            SyncExistingFiles();
        }

        /// <summary>
        /// Lists all files and directories in Source folder.
        /// </summary>
        public static void ListAllFilesAndDirectories()
        {
            Console.WriteLine("======================");
            Console.WriteLine("LISTING ALL FILES AND DIRECTORIES (RELATIVE):");
            Console.WriteLine("======================");

            GetAllDirectiries(FolderTypeEnum.Source);
            GetAllFiles(FolderTypeEnum.Source);

            foreach (string directory in _sourceRelativeDirectories)
            {
                Console.WriteLine(directory);
            }

            foreach (string file in _sourceRelativeFiles)
            {
                Console.WriteLine(file);
            }
        }

        public static void SetFolder(string path, FolderTypeEnum folderType)
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

                if (folderType == FolderTypeEnum.Source) _sourceFolder = path;
                if (folderType == FolderTypeEnum.Replica) _replicaFolder = path;
            }
        }

        public static void SetInterval(string interval)
        {
            string secondsPattern = "(\\d+)([s])";
            string minutesPattern = "(\\d+)([m])";

            var secondsMatches = Regex.Matches(interval, secondsPattern);
            var minutesMatches = Regex.Matches(minutesPattern, minutesPattern);

            if (secondsMatches.Count > 0)
            {
                Console.WriteLine("Interval set to: " + secondsMatches[0].Value + " seconds");
                _interval = Int32.Parse(secondsMatches[0].Groups[1].Value) * 1000;
            }

            if (minutesMatches.Count > 0)
            {
                Console.WriteLine("Interval set to: " + secondsMatches[0].Value + " minutes");
                _interval = Int32.Parse(secondsMatches[0].Groups[1].Value) * 1000 * 60;
            }

            if (secondsMatches.Count ==  0 && minutesMatches.Count == 0)
            {
                throw new ArgumentException("Incorrect argument for interval! Ensure That interval is set as Xs or Xm, where X is a number.");
            }
        }
    }
}