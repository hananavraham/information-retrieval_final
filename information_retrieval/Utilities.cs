using System;
using System.Collections.Generic;
using System.Data;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace information_retrieval
{
    class Utilities
    {
        public static string isSentence { get; set; }        // if search string include few words
        private static bool checkMultiWordsSearch(string docIndex,string[] words)
        {
            string file = $@"C:\Users\hananavr\Documents\Visual Studio 2015\Projects\information_retrieval\information_retrieval\storage Documents\doc{docIndex}.txt";
            using (StreamReader sr = new StreamReader(file))
            {
                string text = sr.ReadToEnd();
                if (text.ToLower().Contains($"{words[0]} {words[1]} {words[2]}"))
                    return true;
            }
                return false;
        }


        //parsing the serach command string
        public static DataTable parseSerachCommand(string searchCommand)
        {
            if (searchCommand.IndexOf(" ") == 0)      // removing whitespace from the beginning
            {
                int index = Regex.Match(searchCommand, "[a-zA-Z]").Index;
                searchCommand = searchCommand.Substring(index);
            }
            
            DataTable dt = new DataTable();
            List<string> queryOp = new List<string>();
            //List<string> patterns = new List<string>();
            //patterns.Add(@"(.)\w(.+?)\)");      // pattern for () inside ()
            //patterns.Add(@"\w(.+?)\&");
            //patterns.Add(@"\w(.+?)\&\s(.+?)\w(.*)");
            //patterns.Add(@"\w(.+?)\|\s(.+?)\w(.*)");
            //patterns.Add(@"!\s(.+?)\w(.*)");    // pattern for NOT 
            //string NEARpattern = @"\bword1\W+(?:\w+\W+){dist1,dist2}?word2\b";
            string[] w;

            if (searchCommand.Contains('"'))        // for string include few words like: "and his wife"
            {
                w = searchCommand.Split(' ');
                if (w.Length > 1)
                {  
                    Utilities.removeWhiteSpace(ref w);
                    w[0] = Regex.Match(w[0], @"[a-zA-Z]+").Value;
                    w[2] = Regex.Match(w[2], @"[a-zA-Z]+").Value;
                    dt = sqlCommandManager.selectCommand($"SELECT a.word_name,b.word_name, a.doc_index FROM documents.word_index as a JOIN documents.word_index as b WHERE b.word_name ='{w[0]}' and a.word_name ='{w[1]}' and a.doc_index = b.doc_index");
                    foreach (DataRow row in dt.Rows)
                    {
                        if (row.RowState.ToString().Equals("Deleted"))
                            continue;
                        string docIndex = row["doc_index"].ToString();
                        if (!checkMultiWordsSearch(docIndex, w))
                        {
                            row.Delete();
                        }
                            
                    }
                    isSentence = $"{w[0]} {w[1]} {w[2]}";
                }

                else 
                {
                    searchCommand = Regex.Match(w[0], @"[a-zA-Z]\w*").Value;
                    if (checkStopList(searchCommand))      
                        dt = sqlCommandManager.selectCommand($"select * from word_index WHERE word_name ='{searchCommand}'");
                    else
                        dt = sqlCommandManager.selectCommand($"select * from word_index WHERE word_name Like '%{searchCommand}%'");
                }
            }

            else if (searchCommand.Contains("["))   // For NEAR search
            {
                string[] w1 = { };
                string[] words = searchCommand.Split('[');
                if (words[0].Contains('&'))
                {
                    w1 = words[0].Split('&');
                    removeWhiteSpace(ref w1);                    
                }
                string[] indexes = words[1].Split(',');
                indexes[1] = indexes[1].Substring(0, indexes[1].Length - 1);
                dt = runNearCommand(w1, indexes[0], indexes[1]);
            }

            else if (searchCommand.Contains("("))
            {
                Char[] operands = new Char[2];
                string[] words = Regex.Split(searchCommand,@"\)");
                for (int i =0; i < words.Length; ++i)
                {
                    if (words[i].Contains('&'))
                        operands[i] = '&';
                    else if (words[i].Contains('|'))
                        operands[i] = '|';
                    else if (words[i].Contains('!'))
                        operands[i] = '!';
                    words[i] = Regex.Match(words[i], @"\w\w(.*)").Value;                    
                }

                string[] firstOp;
                if (operands[0].Equals('&'))
                {
                    firstOp = Regex.Split(words[0], @"\&");
                    removeWhiteSpace(ref firstOp);

                    if (operands[1].Equals('|'))
                    {
                        // AND + OR command
                    }

                    else if (operands[1].Equals('!'))
                    {
                        // AND + NOT command
                        dt = sqlCommandManager.selectCommand($"SELECT a.word_name,b.word_name, a.doc_index FROM documents.word_index as a JOIN documents.word_index as b WHERE b.word_name ='{firstOp[0]}' and a.word_name ='{firstOp[1]}' and a.doc_index = b.doc_index");
                        NotCommand(ref dt, words[1]);
                    }
                }

                else if (operands[0].Equals('|'))
                {
                    firstOp = Regex.Split(words[0], @"\|");
                    removeWhiteSpace(ref firstOp);

                    if (operands[1].Equals('|'))
                    {
                        // OR + OR command
                        dt = sqlCommandManager.selectCommand($"select * from word_index where word_name ='{firstOp[0]}' OR word_name ='{firstOp[1]}' OR word_name = '{words[1]}'");                     
                    }

                    else if (operands[1].Equals('!'))
                    {
                        // OR + NOT command
                        dt = sqlCommandManager.selectCommand($"select * from word_index where word_name ='{firstOp[0]}' OR word_name ='{firstOp[1]}'");
                        NotCommand(ref dt, words[1]);
                    }
                }

                return dt;   

            }

            else if (Utilities.checkStopList(searchCommand))   //checking if its "stopList" word
            {
                return dt;
            }


            else if (searchCommand.Contains("&") || searchCommand.Contains("|"))  //operators
            {
                string[] words;
                if (searchCommand.Contains("&"))    // contains AND
                {
                    words = searchCommand.Split('&');
                    Utilities.removeWhiteSpace(ref words);
                    dt = sqlCommandManager.selectCommand($"SELECT a.word_name,b.word_name, a.doc_index FROM documents.word_index as a JOIN documents.word_index as b WHERE b.word_name ='{words[0]}' and a.word_name ='{words[1]}' and a.doc_index = b.doc_index");
                }

                else     // contains OR
                {
                    words = searchCommand.Split('|');
                    Utilities.removeWhiteSpace(ref words);
                    dt = sqlCommandManager.selectCommand($"select * from word_index where word_name ='{words[0]}' OR word_name ='{words[1]}' ");
                }
            }

            else
            {
                dt = sqlCommandManager.selectCommand($"select * from word_index WHERE word_name Like '%{searchCommand}%'");
            }

            Robots.checkHiddenDoc(ref dt);   //checking if dt includes hidden index
            
            return dt;
        }

        // for not command
        private static void NotCommand(ref DataTable dt,string word)
        {
            foreach (DataRow row in dt.Rows)
            {
                string index = row["doc_index"].ToString();
                string file = $@"C:\Users\hananavr\Documents\Visual Studio 2015\Projects\information_retrieval\information_retrieval\storage Documents\doc{index}.txt";

                using (StreamReader sr = new StreamReader(file))
                {
                    string text = sr.ReadToEnd();
                    if (text.ToLower().Contains(word))
                        row.Delete();
                }
            }
        }

        //insert document into RichText
        public static void readDocumentAndAddToRichText(string id, List<string> words, ref RichTextBox r)
        {
            string file = $@"C:\Users\hananavr\Documents\Visual Studio 2015\Projects\information_retrieval\information_retrieval\storage Documents\doc{id}.txt";            
            string[] readText = File.ReadAllLines(file);
            foreach (string line in readText)
            {
                r.SelectionColor = Color.Black;
                r.SelectionFont = new Font("Arial", 11);
                r.AppendText(line);
                if (string.IsNullOrEmpty(isSentence))
                {
                    foreach (string word in words)      //marking the searched word in Red
                    {
                        HighlightPhrase(ref r, word, Color.Red);
                    }
                    r.AppendText(Environment.NewLine);
                    r.SelectionColor = r.ForeColor;
                }
                                                                                     
            }

            if (!string.IsNullOrEmpty(isSentence))
            {
                string text = r.Text.ToLower();
                foreach (Match m in Regex.Matches(text, isSentence))
                {
                    r.SelectionStart = m.Index;
                    r.SelectionLength = isSentence.Length;
                    r.SelectionColor = Color.Red;
                }
            }

            isSentence = string.Empty;
        }

        // mark all seraching words in red
        static private void HighlightPhrase(ref RichTextBox box, string phrase, Color color)
        {
            int pos = box.SelectionStart;
            string s = box.Text;
            for (int ix = 0; ;)
            {
                int jx = s.IndexOf(phrase, ix, StringComparison.CurrentCultureIgnoreCase);
                if (jx < 0)
                    break;

                box.SelectionStart = jx;
                box.SelectionLength = phrase.Length;
                box.SelectionColor = color;
                ix = jx + 1;
            }
            box.SelectionStart = pos;
            box.SelectionLength = 0;
        }

        //remove white space
        public static void removeWhiteSpace(ref string[] words)
        {
            words[0] = Regex.Match(words[0], "[a-zA-Z]+").ToString();
            //if (words[0].Contains(" "))
            //    words[0] = removeCharFromBegin(words[0], ' ');
            words[1] = Regex.Match(words[1], "[a-zA-Z]+").ToString();
            //if (words[1].Contains(" "))
            //    words[1] = removeCharFromEnd(words[1], ' ');
        }

        private static string removeCharFromBegin(string word, char c)
        {
            return word.Substring(0, word.IndexOf(c));
        }

        private static string removeCharFromEnd(string word, char c)
        {
            return word.Substring(word.IndexOf(" ") + 1);
        }

        public static string[] parseStatment(string word)
        {
            string[] words = word.Split(' ');
            words[0] = removeCharFromBegin(words[0], '(');
            words[0] = removeCharFromEnd(words[1], ' ');
            words[1] = removeCharFromEnd(words[1], ' ');
            words[2] = removeCharFromEnd(words[1], ')');
            return words;
        }

        //checking Stop list word
        public static bool checkStopList(string word)
        {
            List<string> stopList = new List<string>();
            stopList.Add("a");
            stopList.Add("all");
            stopList.Add("and");
            stopList.Add("any");
            stopList.Add("at");
            stopList.Add("be");
            stopList.Add("do");
            stopList.Add("for");
            stopList.Add("her");
            stopList.Add("how");
            stopList.Add("if");
            stopList.Add("is");
            stopList.Add("not");
            stopList.Add("see");
            stopList.Add("the");
            stopList.Add("their");
            stopList.Add("when");
            stopList.Add("why");

            foreach (string s in stopList)
            {
                if (stopList.Any(word.ToLower().Equals))
                    return true;
            }
            return false;
        }

        public static bool checkAdmin(string user, string pass)
        {
            if (user.Equals("admin") && pass.Equals("12345"))
                return true;
            return false;
        }

        public static bool HandleUploadFile(string filePath)
        {
            int fileIndex = checkStorageLength();
            string newFile = copyFileToStorage(filePath, fileIndex);
            if (string.IsNullOrEmpty(newFile))
                return false;
            //File.Delete(filePath);
            Robots.parseAndAddToDB(newFile);
            return true;
        }

        private static int checkStorageLength()
        {
            var dir = Directory.GetFiles($@"C:\Users\hananavr\Documents\Visual Studio 2015\Projects\information_retrieval\information_retrieval\storage Documents");
            return dir.Length + 1;
        }

        private static string copyFileToStorage(string file, int index)
        {
            try
            {
                string destFile = $@"C:\Users\hananavr\Documents\Visual Studio 2015\Projects\information_retrieval\information_retrieval\storage Documents\doc{index}.txt";
                File.Copy(file, destFile);
                return destFile;
            }
            catch { return null; }

        }

        public static List<string> getDocsList()
        {
            List<string> docs = new List<string>();
            Directory.GetFiles(@"C:\Users\hananavr\Documents\Visual Studio 2015\Projects\information_retrieval\information_retrieval\storage Documents");
            var list = Directory.GetFiles(@"C:\Users\hananavr\Documents\Visual Studio 2015\Projects\information_retrieval\information_retrieval\storage Documents");
            foreach (string doc in Directory.GetFiles(@"C:\Users\hananavr\Documents\Visual Studio 2015\Projects\information_retrieval\information_retrieval\storage Documents"))
            {
                docs.Add(doc.Substring(doc.LastIndexOf('\\') + 1));
            }
            return docs;
        }

        public static List<string> getHiddenDocList()
        {
            return Robots.getHiddenDocsList();
        }

        // NEAR command
        private static DataTable runNearCommand(string[] words,string index1,string index2)
        {
            DataTable dt;
            dt = sqlCommandManager.selectCommand($"SELECT a.word_name,b.word_name, a.doc_index FROM documents.word_index as a JOIN documents.word_index as b WHERE b.word_name ='{words[0]}' and a.word_name ='{words[1]}' and a.doc_index = b.doc_index");
            foreach (DataRow row in dt.Rows)
            {
                string index = row["doc_index"].ToString();
                string file = $@"C:\Users\hananavr\Documents\Visual Studio 2015\Projects\information_retrieval\information_retrieval\storage Documents\doc{index}.txt";
                
                using (StreamReader sr = new StreamReader(file))
                {
                    string text = sr.ReadToEnd();
                    text = text.ToLower();
                    var matches = Regex.Matches(text, words[0] +@"\W+(?:\w+\W+){"+index1 + "," +index2 +"}?" + words[1]);
                    if (matches.Count == 0)
                        row.Delete();                          
                }
            }
            return dt;
        }
    }
}
