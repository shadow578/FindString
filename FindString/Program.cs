//may not work
//#define USE_PARALLEL_SEARCH

using System;
using System.IO;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace FindString
{
    class Program
    {
        static void Main(string[] args)
        {
            DirectoryInfo searchDir;
            StreamWriter logFileWriter = null;
            string searchPattern;
            bool searchPatternIsRegex;
            bool shouldLogToFile;

            #region Setup Runtime Vars by user
            //get search directory
            do
            {
                searchDir = new DirectoryInfo(UserGetString("Enter Directory to Search in: "));
            } while (!searchDir.Exists);

            //get search pattern
            searchPattern = UserGetString("Enter Pattern to Search for (supports RegEx): ");

            //ask for regex
            searchPatternIsRegex = UserGetString("Does the Search Pattern use RegEx? (y/n): ").ToLower().Contains("y");

            //ask for log to file
            shouldLogToFile = UserGetString("Do you want to log to a file? (y/n): ").ToLower().Contains("y");

            //ask for log file path
            if (shouldLogToFile)
            {
                string logFilePath;
                do
                {
                    logFilePath = UserGetString("Enter Log file Path: ");
                } while (!TryCreateTextFile(logFilePath, out logFileWriter));
            }
            #endregion

            //all set- up now, start the search!
            int matches = 0;
            FindStringIn(searchDir, (file, lineNr, line) =>
            {
                //count this match
                matches++;

                //get immediate characters AROUND match pattern (only support non- regex for now)
                string immediateStr;
                if (!searchPatternIsRegex)
                {
                    int matchStartIndex = line.IndexOf(searchPattern, StringComparison.InvariantCultureIgnoreCase);
                    int immediateStrLength = searchPattern.Length + 20;
                    if ((matchStartIndex + immediateStrLength) >= line.Length)
                    {
                        immediateStrLength = (line.Length - matchStartIndex - 1);
                    }

                    immediateStr = line.Substring(matchStartIndex, immediateStrLength);
                }
                else
                {
                    //not supporting regex for now
                    immediateStr = "";
                }

                //write info to console
                Console.WriteLine($"Match in \"{file.Name}\" line {lineNr}: {immediateStr}");

                //write to log file if enabled
                if (shouldLogToFile && logFileWriter != null)
                {
                    logFileWriter.WriteLine($"Match in \"{file.Name}\" line {lineNr}: {(string.IsNullOrWhiteSpace(immediateStr) ? line : immediateStr)}");
                }
            }, searchPattern, searchPatternIsRegex);

            //search ended
            logFileWriter?.Close();
            Console.WriteLine($"Search finished! Found {matches} Matches.");
            Console.ReadLine();
        }

        static string UserGetString(string msg)
        {
            string o;
            do
            {
                Console.Write(msg);
                o = Console.ReadLine();
            } while (string.IsNullOrWhiteSpace(o));
            return o;
        }

        static bool TryCreateTextFile(string path, out StreamWriter writer)
        {
            //dummy writer
            writer = null;

            //abort if exists
            if (File.Exists(path)) return false;

            try
            {
                writer = File.CreateText(path);
                return true;
            }
            catch (Exception)
            {
                //failed
                return false;
            }
        }

        static void FindStringIn(DirectoryInfo searchDir, Action<FileInfo /*matched file*/, int /*line*/, string /*line contents*/> matchAction, string searchFor, bool useRegex)
        {
#if USE_PARALLEL_SEARCH
            Parallel.ForEach(searchDir.GetFilesIncludingSubdirs(), (file) =>
#else
            foreach (FileInfo file in searchDir.GetFilesIncludingSubdirs())
#endif
            {
                using (StreamReader sr = file.OpenText())
                {
                    int lnNum = 0;
                    while (!sr.EndOfStream)
                    {
                        string ln = sr.ReadLine();
                        lnNum++;

                        if (MatchLine(ln, searchFor, useRegex))
                        {
                            //found one!
                            matchAction(file, lnNum, ln);
                        }
                    }
                }
#if USE_PARALLEL_SEARCH
            });
#else
            }
#endif
        }

        static bool MatchLine(string line, string searchFor, bool useRegex)
        {
            if (useRegex)
            {
                Regex regex = new Regex(searchFor);
                return regex.IsMatch(line);
            }
            else
            {
                return line.ToLower().Contains(searchFor.ToLower());
            }
        }
    }
}
