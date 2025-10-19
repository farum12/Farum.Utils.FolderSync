// See https://aka.ms/new-console-template for more information

using Farum.Utils.FolderSync;

class Program
{
    static async Task Main(string[] args)
    {
        if (args.Length < 4)
        {
            throw new ArgumentException("Incorrect amount of arguments!");
        }

        FolderSynchronization.SetFolder(args[0], FolderTypeEnum.Source);
        FolderSynchronization.SetFolder(args[1], FolderTypeEnum.Replica);
        FolderSynchronization.SetInterval(args[2]);
        FileLog.SetFolder(args[3]);

        FolderSynchronization.ListAllFilesAndDirectories();
        
        await FolderSynchronization.Loop();
    }
}