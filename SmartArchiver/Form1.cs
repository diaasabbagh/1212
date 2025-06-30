using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;
using SmartArchiver.Compression;

namespace SmartArchiver
{
    public partial class Form1 : Form
    {
        private CancellationTokenSource _tokenSource;
        public Form1(IEnumerable<string> startupFiles = null)
        {
            InitializeComponent();
            listBox1.HorizontalScrollbar = true;
            warningLabel1.Visible = false;

            if (startupFiles != null)
            {
                foreach (var file in startupFiles)
                {
                    listBox1.Items.Add(file);
                }
            }
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

        private async void button3_Click(object sender, EventArgs e)
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
                    var selectedItems = listBox1.Items.Cast<string>().ToList();
                    var files = FileUtils.ExpandFileList(selectedItems);
                    _tokenSource = new CancellationTokenSource();
                    cancelButton.Enabled = true;
                    try
                    {
                        string pwd = optionsForm.Password;
                        double ratio = await Task.Run(() =>
                        {
                            if (optionsForm.SelectedMethod == "Huffman")
                            {
                                return HuffmanArchive.CompressFiles(files, archiveName + ".huff", _tokenSource.Token, pwd);
                            }
                            else
                            {
                                return ShannonFanoArchive.CompressFiles(files, archiveName + ".shfn", _tokenSource.Token, pwd);
                            }
                        });
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
                    finally
                    {
                        cancelButton.Enabled = false;
                        _tokenSource = null;
                    }
                }
            }
        }

        private void button4_Click(object sender, EventArgs e)
        {
            var selectedItems = listBox1.SelectedItems.Cast<object>().ToList();
            foreach (var item in selectedItems)
            {
                listBox1.Items.Remove(item);
            }
        }

        private async void button2_Click(object sender, EventArgs e)
        {
            using (OpenFileDialog ofd = new OpenFileDialog())
            {
                ofd.Filter = "Archive Files|*.huff;*.shfn";
                if (ofd.ShowDialog() == DialogResult.OK)
                {
                    using (FolderBrowserDialog fbd = new FolderBrowserDialog())
                    {
                        if (fbd.ShowDialog() == DialogResult.OK)
                        {
                            _tokenSource = new CancellationTokenSource();
                            cancelButton.Enabled = true; List<string> filenames;
                            string extList = Path.GetExtension(ofd.FileName).ToLowerInvariant();
                            string password = null;
                            if (EncryptionUtils.IsEncrypted(ofd.FileName))
                            {
                                using (var pf = new PasswordForm())
                                {
                                    if (pf.ShowDialog() == DialogResult.OK)
                                    {
                                        password = pf.Password;
                                    }
                                    else
                                    {
                                        cancelButton.Enabled = false;
                                        _tokenSource = null;
                                        return;
                                    }
                                }
                            }
                            if (extList == ".huff")
                            {
                                filenames = HuffmanArchive.GetFileNames(ofd.FileName, password);
                            }
                            else
                            {
                                filenames = ShannonFanoArchive.GetFileNames(ofd.FileName, password);
                            }
                            using (var optionsForm = new Form3()) {
                                optionsForm.LoadFileNames(filenames);
                                if (optionsForm.ShowDialog() == DialogResult.OK)
                                {
                                    string action = optionsForm.SelectedAction;
                                    if(action == "ExtractAll") {
                                        try
                                        {
                                            await Task.Run(() =>
                                            {
                                                string ext = Path.GetExtension(ofd.FileName).ToLowerInvariant();
                                                if (ext == ".huff")
                                                {
                                                    HuffmanArchive.ExtractAll(ofd.FileName, fbd.SelectedPath, _tokenSource.Token, password);
                                                }
                                                else
                                                {
                                                    ShannonFanoArchive.ExtractAll(ofd.FileName, fbd.SelectedPath, _tokenSource.Token, password);
                                                }
                                            });
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
                                    else
                                    {
                                        string selectedFile = optionsForm.SelectedFile;
                                        if (!string.IsNullOrEmpty(selectedFile))
                                        {
                                            try
                                            {
                                                await Task.Run(() =>
                                                {
                                                    string ext = Path.GetExtension(ofd.FileName).ToLowerInvariant();
                                                    if (ext == ".huff")
                                                    {
                                                        HuffmanArchive.ExtractFile(ofd.FileName, selectedFile, fbd.SelectedPath, _tokenSource.Token, password);
                                                    }
                                                    else
                                                    {
                                                        ShannonFanoArchive.ExtractFile(ofd.FileName, selectedFile, fbd.SelectedPath, _tokenSource.Token, password);
                                                    }
                                                });
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
                                        else
                                        {
                                            MessageBox.Show("Please select a file to extract.", "No file selected", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                                        }
                                    }
                                    cancelButton.Enabled = false;
                                    _tokenSource = null;
                                }
                            }
                        }
                    }
                }
            }
        }

        private void warningLabel1_Click(object sender, EventArgs e)
        {

        }

        private void button5_Click(object sender, EventArgs e)
        {
            using (FolderBrowserDialog dialog = new FolderBrowserDialog())
            {
                if (dialog.ShowDialog() == DialogResult.OK)
                {
                    listBox1.Items.Add(dialog.SelectedPath);
                }
            }
        }
        private void cancelButton_Click(object sender, EventArgs e)
        {
            _tokenSource?.Cancel();
        }
    }
}
