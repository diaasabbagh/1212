using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace SmartArchiver
{
    public partial class Form3 : Form
    {
        public Form3()
        {
            InitializeComponent();
        }

        public void Form3_Load(object sender, EventArgs e)
        {

        }

        public string SelectedAction{get; private set;}
        public string SelectedFile => listBox1.SelectedItem as string;
        private void button1_Click(object sender, EventArgs e)
        {
            this.DialogResult = DialogResult.OK;
            SelectedAction = "Extract";
            this.Close();
        }

        private void button2_Click(object sender, EventArgs e)
        {
            this.DialogResult = DialogResult.OK;
            SelectedAction = "ExtractAll";
            this.Close();
        }

        public void listBox1_SelectedIndexChanged(object sender, EventArgs e)
        {

        }
        public void LoadFileNames(List<string> fileNames)
        {
            listBox1.Items.Clear();
            foreach(var name in fileNames)
            {
                listBox1.Items.Add(name);
            }
        }
    }
}
