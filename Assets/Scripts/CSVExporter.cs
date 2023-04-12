using System.IO;
using UnityEngine;

namespace Assets.Scripts
{
    public class CSVExporter
    {
        public string fullFilePath;

        public CSVExporter(string filePath)
        {
            this.fullFilePath = filePath;
        }

        public void CreateTableIfNotExists(string[] headers)
        {
            if (!File.Exists(fullFilePath))
            {
                using (StreamWriter writer = new StreamWriter(fullFilePath))
                {
                    writer.WriteLine(string.Join(",", headers));
                }
            }
        }

        public void AddRecord(string[] row)
        {
            if (File.Exists(fullFilePath))
            {
                using (StreamWriter writer = new StreamWriter(fullFilePath, true))
                {
                    writer.WriteLine(string.Join(",", row));
                }
            }
        }
    }
}
