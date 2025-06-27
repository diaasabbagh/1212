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
using SmartArchiver.Compression;

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
                    var files = listBox1.Items.Cast<string>().ToList();
                    var tokenSource = new System.Threading.CancellationTokenSource();
                    try
                    {
                        double ratio = HuffmanArchive.CompressFiles(files, archiveName + ".huff", tokenSource.Token);
                        MessageBox.Show($"Compression complete. Ratio: {ratio:F2}%", "Done", MessageBoxButtons.OK, MessageBoxIcon.Information);
                    }
                    catch (OperationCanceledException)
                    {
                        MessageBox.Show("Compression canceled.");
                    }
                    catch (Exception ex)
                    {
                        MessageBox.Show(ex.Message, "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    }
                }
            }
        }

        private void button2_Click(object sender, EventArgs e)
        {
            using (OpenFileDialog ofd = new OpenFileDialog())
            {
                ofd.Filter = "Huffman Archive|*.huff";
                if (ofd.ShowDialog() == DialogResult.OK)
                {
                    using (FolderBrowserDialog fbd = new FolderBrowserDialog())
                    {
                        if (fbd.ShowDialog() == DialogResult.OK)
                        {
                            var tokenSource = new System.Threading.CancellationTokenSource();
                            try
                            {
                                HuffmanArchive.ExtractAll(ofd.FileName, fbd.SelectedPath, tokenSource.Token);
                                MessageBox.Show("Extraction complete.", "Done", MessageBoxButtons.OK, MessageBoxIcon.Information);
                            }
                            catch (OperationCanceledException)
                            {
                                MessageBox.Show("Extraction canceled.");
                            }
                            catch (Exception ex)
                            {
                                MessageBox.Show(ex.Message, "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                            }
                        }
                    }
                }
            }
        }

        private void warningLabel1_Click(object sender, EventArgs e)
        {

        }
    }
}
