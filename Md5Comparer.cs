using System.Security.Cryptography;

namespace Farum.Utils.FolderSync
{
    // Source: https://dev.to/emrahsungu/how-to-compare-two-files-using-net-really-really-fast-2pd9
    public class Md5Comparer : FileComparer
    {

        public Md5Comparer(string filePath01, string filePath02) : base(filePath01, filePath02)
        {
        }

        protected override bool OnCompare()
        {

            using var fileStream01 = FileInfo1.OpenRead();
            using var fileStream02 = FileInfo2.OpenRead();
            using var md5Creator = MD5.Create();

            var fileStream01Hash = md5Creator.ComputeHash(fileStream01);
            var fileStream02Hash = md5Creator.ComputeHash(fileStream02);

            for (var i = 0; i < fileStream01Hash.Length; i++)
            {
                if (fileStream01Hash[i] != fileStream02Hash[i])
                {
                    return false;
                }
            }
            return true;
        }
    }
}