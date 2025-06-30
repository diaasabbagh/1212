using System;
using System.Windows.Forms;

namespace SmartArchiver
{
    public partial class PasswordForm : Form
    {
        public string Password => passwordBox.Text;
        public PasswordForm()
        {
            InitializeComponent();
        }

        private void button1_Click(object sender, EventArgs e)
        {
            this.DialogResult = DialogResult.OK;
            this.Close();
        }
    }
}