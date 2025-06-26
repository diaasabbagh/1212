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

namespace SmartArchiver
{
    public partial class Form1 : Form
    {
        public Form1()
        {
            InitializeComponent();
            listBox1.HorizontalScrollbar = true;
            warningLabel1.Visible = false;
        }


        private void button1_Click(object sender, EventArgs e)
        {
            using (OpenFileDialog openFileDialog = new OpenFileDialog())
            {
                openFileDialog.Multiselect = true;
                if (openFileDialog.ShowDialog() == DialogResult.OK)
                {
                    //listBox1.Items.Clear();
                    foreach (var file in openFileDialog.FileNames)
                    {
                        listBox1.Items.Add(file);
                    }
                }
            }
        }

        private void listBox1_SelectedIndexChanged(object sender, EventArgs e)
        {

        }

        private void button3_Click(object sender, EventArgs e)
        {
            if (listBox1.Items.Count == 0)
            {
                warningLabel1.Text = "You have not added any files!";
                warningLabel1.Visible = true;
                return;
            }
            warningLabel1.Visible = false;
            using (var optionsForm = new Form2())
            {
                if (optionsForm.ShowDialog() == DialogResult.OK)
                {
                    string archiveName = optionsForm.ArchiveName;
                    //string password = chkEncrypt.Checked ? optionsForm.Password : null;


                    // Call your compression logic here:
                    //CompressFiles(archiveName, password);
                }
            }
        }

        private void warningLabel1_Click(object sender, EventArgs e)
        {

        }
    }
}
