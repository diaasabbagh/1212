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
    public partial class Form2 : Form
    {
        public string ArchiveName => archiveNameBox.Text.Trim();
        public string Password => passwordBox.Text;
        public string SelectedMethod => comboBox1.SelectedItem as string;

        public Form2()
        {
            InitializeComponent();
            warningLabel.Visible = false;
            comboBox1.SelectedIndex = 0;
        }

        private void comboBox1_SelectedIndexChanged(object sender, EventArgs e)
        {

        }

        private void button1_Click(object sender, EventArgs e)
        {



            if (string.IsNullOrWhiteSpace(ArchiveName))
            {
                warningLabel.Text = "Archive name cannot be empty.";
                warningLabel.Visible = true;
                return;
            }
            char[] invalidChars = Path.GetInvalidFileNameChars();
            if (ArchiveName.Any(c => invalidChars.Contains(c)))
            {
                warningLabel.Text = "Archive name contains invalid characters.";
                warningLabel.Visible = true;
                return;
            }
            DialogResult = DialogResult.OK;
            Close();
        }
    }
}
