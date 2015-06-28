﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;

namespace NDSB.FileUtils
{
    public static class CSVHelper
    {
        public static void MergeToOneFile(string[] filePaths)
        {
            List<string[]> enumerators = new List<string[]>();
            for (int i = 0; i < filePaths.Length; i++)
                enumerators.Add(LinesEnumerator.YieldLinesOfFile(filePaths[i]).ToArray());

            List<string> toWrite = new List<string>();

            for (int i = 0; i < enumerators[0].Length; i++)
            {
                string line = "";
                for(int j =0 ; j < enumerators.Count; j++)
                    line += enumerators[j][i] + ";";
                line.Remove(line.Length - 1);
                toWrite.Add(line);
            }

            File.WriteAllLines(Path.GetDirectoryName(filePaths[0]) + "\\Merged.csv",toWrite.ToArray());

        }
    }
}
