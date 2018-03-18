using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace information_retrieval
{
    public partial class Form2 : Form
    {
        public Form2()
        {
            InitializeComponent();
        }

        //comboBox documents list
        private void comboBox1_Enter(object sender, EventArgs e)
        {
            comboBox1.DataSource = Utilities.getDocsList();
            comboBox1.DisplayMember = "name";
            comboBox1.SelectedIndex = -1;
        }

        //close admin screen
        private void button4_Click(object sender, EventArgs e)
        {
            this.Close();
        }

        //hide Document
        private void button2_Click(object sender, EventArgs e)
        {
            if(groupBox1.Visible == false)
                groupBox1.Visible = true;
            else
                groupBox1.Visible = false;
        }

        //hide selected document
        private void button5_Click(object sender, EventArgs e)
        {
            Robots.HideDocument(comboBox1.SelectedValue.ToString());
            MessageBox.Show($@"Hide document {comboBox1.SelectedValue.ToString()} succesfully.");
        }

        //upload Document
        private void button1_Click(object sender, EventArgs e)
        {
            if (openFileDialog1.ShowDialog() == DialogResult.OK)
            {
                string file = openFileDialog1.FileName;
                if (Utilities.HandleUploadFile(file))
                    MessageBox.Show("Upload file and update words Succesfull");
                else
                    MessageBox.Show("Error, something was wrong with upload file");
            }
            
        }

        //Show hidden document button
        private void button3_Click(object sender, EventArgs e)
        {
            if (groupBox2.Visible == false)
                groupBox2.Visible = true;
            else
                groupBox2.Visible = false;
        }

        //Hidden document list
        private void comboBox2_Enter(object sender, EventArgs e)
        {
            comboBox2.DataSource = Utilities.getHiddenDocList();
            comboBox2.DisplayMember = "name";
            comboBox2.SelectedIndex = -1;
        }

        //return document from hidden to visible
        private void button6_Click(object sender, EventArgs e)
        {
            if(Robots.showDocument(comboBox2.SelectedValue.ToString()))
                MessageBox.Show($@"document change visibility succesfully.");
            else
                MessageBox.Show($@"document change visibility not succesfully!!");
        }
    }
}
