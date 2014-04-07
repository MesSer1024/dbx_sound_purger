using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SoundPurger
{
    static class AppSettings
    {
        public static string RootFolder { get; set; }
        public static FileInfo[] FilesToRemove { get; set; }

        internal static void addModifiedFile(FileInfo file)
        {
            if (!_modifiedFiles.Contains(file))
                _modifiedFiles.Add(file);
        }

        private static List<FileInfo> _modifiedFiles = new List<FileInfo>();
        public static List<FileInfo> ModifiedFiles { get { return _modifiedFiles; } }
        private static StringBuilder _sb = new StringBuilder();

        internal static void writeModification(string p)
        {
            Console.WriteLine(p);
            _sb.AppendLine(p);
        }

        internal static void writeLogToFileAndClear(FileInfo file)
        {
            using (var sw = new StreamWriter(file.FullName, false))
            {
                sw.Write(_sb.ToString());
                sw.Flush();
                sw.Close();
            }
            _sb.Clear();
        }
    }
}
