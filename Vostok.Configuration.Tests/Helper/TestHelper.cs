using System.IO;

namespace Vostok.Configuration.Tests.Helper
{
    internal static class TestHelper
    {
        private static int fileNum = 1;

        /// <summary>
        /// Creates test text file
        /// </summary>
        /// <param name="testName">File name prefix to distinguish test files from each other</param>
        /// <param name="text">File body</param>
        /// <param name="name">User file name. If null generates automatically using <paramref name="testName"/>. Use it to rewrite existing file.</param>
        /// <returns>Generated or user file name</returns>
        public static string CreateFile(string testName, string text, string name = null)
        {
            if (string.IsNullOrWhiteSpace(name))
                name = $"{testName}_test_{fileNum++}.tst";

            using (var file = new StreamWriter(name, false))
                file.WriteLine(text);

            return name;
        }

        /// <summary>
        /// Deletes all test files by prefix <paramref name="testName"/>: prefix_*
        /// </summary>
        public static void DeleteAllFiles(string testName)
        {
            var dir = Directory.GetCurrentDirectory();
            foreach (var file in Directory.EnumerateFiles(dir, $"{testName}_*"))
                File.Delete(file);
        }
    }
}