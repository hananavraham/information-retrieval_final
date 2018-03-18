using System;
using System.Collections.Generic;
using System.Data;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace information_retrieval
{
    class Robots
    {
        static string destPath = @"C:\Users\hananavr\Documents\Visual Studio 2015\Projects\information_retrieval\information_retrieval\storage Documents";
        static List<string> HiddenDocs = new List<string>();
        public static void copyFileAndCreateIndex()
        {
            string sourceFolder = @"C:\Users\hananavr\Documents\Visual Studio 2015\Projects\information_retrieval\information_retrieval\source Documents";
            if (Directory.GetFiles(sourceFolder).Length > 0)
            {
                MoveFilesToStorage();
                //parseFiles();
                string[] fileEntries = Directory.GetFiles(sourceFolder);
                foreach (string file in fileEntries)
                {
                    FileInfo f = new FileInfo(file);
                    parseAndAddToDB($@"{destPath}\{f.Name}");
                }

                    
            }             
        }

        public static List<string> getHiddenDocsList()
        {
            return HiddenDocs;
        }

        private static void MoveFilesToStorage()
        {
            string[] fileEntries = Directory.GetFiles(@"C:\Users\hananavr\Documents\Visual Studio 2015\Projects\information_retrieval\information_retrieval\source Documents");
            foreach (string file in fileEntries)
            {
                try
                {
                    FileInfo f = new FileInfo(file);
                    File.Copy(file, $@"{destPath}\{f.Name}");
                    File.Delete(file);
                }
                catch { }
            }
        }
     
        private static void parseFiles()
        {
            string[] fileEntries = Directory.GetFiles(destPath);
            foreach (string file in fileEntries)
                parseAndAddToDB(file);
        }

        public static void parseAndAddToDB(string file)
        {
            string pattern = @"doc\d*";
            string pattern1 = @"([0-9])\d*";
            string line = "";
            //find the doc number
            string index = Regex.Match(file, pattern).Value;
            index = Regex.Match(index, pattern1).Value;
            using (StreamReader sr = new StreamReader(file))
            {
                while ((line = sr.ReadLine()) != null)
                {
                    if (string.IsNullOrEmpty(line))
                        continue;
                    string[] words = parseLine(line);
                    addWordsToTable(words, index);
                }
            }
        }
        
        private static string[] parseLine(string line)
        {
            return line.Split(' ');
        }

        private static void addWordsToTable(string[] words,string index)
        {
            string w = "";
            foreach (string word in words)
            {
                w = word.TrimEnd(new Char[] { '"', ' ', '.', ';' , '?',':', '\\', '/' , ','});
                if(w.Contains('"'))
                    w = w.Substring(word.IndexOf('"') +1);
                sqlCommandManager.insertData($"word_index values('{w}','{index}')");
            }
        }

        public static List<string> addSummary(string index)
        {
            string line = "";
            string summary = "";
            List<string> sumAndLink = new List<string>();
            int count = 0;
            string file = $@"C:\Users\hananavr\Documents\Visual Studio 2015\Projects\information_retrieval\information_retrieval\storage Documents\doc{index}.txt";
            using (StreamReader sr = new StreamReader(file))
            {
                while ((line = sr.ReadLine()) != null)
                {
                    if (string.IsNullOrEmpty(line))
                        continue;
                    summary += line;
                    ++count;
                    if (count > 3) 
                    {
                        count = 0;
                        sumAndLink.Add(summary);
                        break;
                    }                    
                }
            }
            sumAndLink.Add(file);
            return sumAndLink;
        }

        //hide document from search
        public static void HideDocument(string doc)
        {
            HiddenDocs.Add(doc);
        }

        //checking if hidden doc index exists in search result table
        public static void checkHiddenDoc(ref DataTable dt)
        {
            foreach (DataRow row in dt.Rows)
            {
                if (row.RowState.ToString().Equals("Deleted"))
                    continue;
                if (checkHiddenIndex(row["doc_index"].ToString()))
                    row.Delete();
            }
        }

        //looping over hiddenDoc list
        private static bool checkHiddenIndex(string index)
        {
            foreach (string i in HiddenDocs)
            {
                if (i.Equals($@"doc{index}.txt"))
                    return true;
            }

            return false;
        }

        public static bool showDocument(string doc)
        {
            foreach (string item in HiddenDocs)
            {
                if (item.Equals(doc)) 
                {
                    HiddenDocs.Remove(item);
                    return true;
                }
                    
            }
            return false;
        }
    }
}
