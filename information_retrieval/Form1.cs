using MySql.Data.MySqlClient;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Drawing.Printing;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace information_retrieval
{
    public partial class Form1 : Form
    {
        public static string connString = "Server=localhost;Port=3306;Database=documents;Uid=root;password=1307;";
        public Form1()
        {
            InitializeComponent();
            MySqlConnection conn = sqlCommandManager.createConnection();
            sqlCommandManager.createDBdata();
            Robots.copyFileAndCreateIndex();
        }

        // checking if "ENTER" key pressed
        private void CheckKeys(object sender, System.Windows.Forms.KeyPressEventArgs e)
        {
            if (e.KeyChar == (char)13)
            {
                button1_Click(sender,e);
            }
        }

        //search button
        private void button1_Click(object sender, EventArgs e)
        {
           if (string.IsNullOrEmpty(textBox1.Text))
                return;
            DataTable dt = new DataTable();
           dt = Utilities.parseSerachCommand(textBox1.Text.ToLower());
           if (dt.Rows.Count == 0)
                return;  
           /*
           if (textBox1.Text.Contains("("))
           {
                Utilities.parseStatment(textBox1.Text);
           }
           if(Utilities.checkStopList(textBox1.Text))   //checking if its "stopList" word
           {
                return;
           }

            DataTable dt = new DataTable();
            if (textBox1.Text.Contains("&") || textBox1.Text.Contains("!") || textBox1.Text.Contains("|"))  //operators
            {
                string[] words;
                if (textBox1.Text.Contains("&"))    // contains AND
                {
                    words = textBox1.Text.Split('&');
                    Utilities.removeWhiteSpace(ref words);
                    dt = sqlCommandManager.selectCommand($"SELECT a.word_name,b.word_name, a.doc_index FROM documents.word_index as a JOIN documents.word_index as b WHERE b.word_name ='{words[0]}' and a.word_name ='{words[1]}' and a.doc_index = b.doc_index");
                }
                else if (textBox1.Text.Contains("!"))    // contains NOT
                {
                    //need to be : (word & word) ! word.....
                    words = textBox1.Text.Split('!');
                    Utilities.removeWhiteSpace(ref words);
                }
                else     // contains OR
                {
                    words = textBox1.Text.Split('|');
                    Utilities.removeWhiteSpace(ref words);
                    dt = sqlCommandManager.selectCommand($"select * from word_index where word_name ='{words[0]}' OR word_name ='{words[1]}' ");
                }
            }

            else 
            {
                dt = sqlCommandManager.selectCommand($"select * from word_index where word_name ='{textBox1.Text}'");
            }


            Robots.checkHiddenDoc(ref dt);   //checking if dt includes hidden index
            */

            dataGridView1.DataSource = dt;
            DataGridViewColumn column = dataGridView1.Columns["Summary"];
            column.Width = 350;
            dataGridView1.DefaultCellStyle.WrapMode = DataGridViewTriState.True;
            dataGridView1.Visible = true;
            label4.Visible = true;
        }

        // Show admin window
        private void button2_Click(object sender, EventArgs e)
        {
            textBox2.Text = "";
            textBox3.Text = "";
            groupBox1.Visible = true;
            panel1.Visible = true;
            textBox2.Focus();
        }

        // click tab button on admin User name text box
        private void textBox2_Leave(object sender, EventArgs e)
        {
            textBox3.Focus();
        }


        //login window check if ENTER pressed
        private void textBox3_KeyPress(object sender, KeyPressEventArgs e)
        {
            if (e.KeyChar == (char)13)
            {
                button3_Click(sender, e);
            }
        }


        // verify admin details and upload file
        private void button3_Click(object sender, EventArgs e)
        {
            if (Utilities.checkAdmin(textBox2.Text,textBox3.Text))
            {
                Form2 form2 = new Form2();
                form2.Show();
                panel1.Visible = false;
                groupBox1.Visible = false;
            }
            else
            {
                MessageBox.Show(@"sorry, wrong username or password");
            }
        }

        // hide admin window
        private void button4_Click(object sender, EventArgs e)
        {
            panel1.Visible = false;
            groupBox1.Visible = false;
            textBox2.Text = "";
            textBox3.Text = "";
        }

        //show selected document
        private void dataGridView1_CellContentClick(object sender, DataGridViewCellEventArgs e)
        {
            int rowIndex = e.RowIndex;
            richTextBox1.Text = "";
            List<string> words = new List<string>();
            
            DataGridViewRow row = dataGridView1.Rows[rowIndex];
            string id = dataGridView1.Rows[rowIndex].Cells["doc_index"].Value.ToString();
            words.Add(dataGridView1.Rows[rowIndex].Cells["word_name"].Value.ToString());
            for (int i =1; i <= dataGridView1.Columns.Count; ++i)
            {
                try
                {
                    words.Add(dataGridView1.Rows[rowIndex].Cells[$"word_name{i}"].Value.ToString());
                }
                catch { }              
            }
                     
            Utilities.readDocumentAndAddToRichText(id, words, ref richTextBox1);
        }

        //print document1
        private void button6_Click(object sender, EventArgs e)
        {
            if (printDialog1.ShowDialog() == DialogResult.OK)
            {
                printPreviewDialog1.Document = printDocument1;
                printDocument1.PrintPage += new PrintPageEventHandler(doc_PrintPage);
                if (printDialog1.ShowDialog() == DialogResult.OK)
                    printDocument1.Print();
            }
        }

        //print document2
        void doc_PrintPage(object sender, PrintPageEventArgs e)
        {
            int x = 10;
            int y = 0;
            int charpos = 0;
            while (charpos < richTextBox1.Text.Length)
            {
                if(richTextBox1.Text[charpos] == '\n')
                {
                    charpos++;
                    y += 20;
                    x = 10;
                }
                else if (richTextBox1.Text[charpos] == '\r')
                {
                    charpos++;
                }
                else 
                {
                    richTextBox1.Select(charpos, 1);
                    e.Graphics.DrawString(richTextBox1.SelectedText, richTextBox1.SelectionFont, new
                        SolidBrush(richTextBox1.SelectionColor), new PointF(x, y));
                    x = x + 6;
                    charpos++;
                }
            }
        }        
    }
}
