using System;
using System.IO;
using System.Text;
using System.Threading;
using System.Windows.Forms;

namespace EncriptionTask
{
    public partial class Form1 : Form
    {
        // private Thread t;
        private string _text;
        private string _key;
        private StringBuilder _proccesedText = new StringBuilder();
        private CancellationTokenSource cts;

        public Form1()
        {
            InitializeComponent();
        }

        private void ProgressPosition(int progress)
        {
            if (progressBar1.InvokeRequired)
            {
                progressBar1.BeginInvoke(new Action(() => progressBar1.Value = progress));
                LabelProgress.BeginInvoke(new Action(() => LabelProgress.Text = progress.ToString()+@"%"));
            }
            else
            {
                progressBar1.Value=progress;
                LabelProgress.Text = progress.ToString()+@"%";
            }
        }

        private void EncDec(CancellationToken token)
        {
           
            GetKey(); 
            ReadFromFile();
            _proccesedText.Clear();
            for (int c = 0; c < _text.Length-1; c++)
            {
                if (token.IsCancellationRequested)
                {
                    ReverseOperation();
                    return;
                }
                _proccesedText.Append((char)(_text[c] ^ (uint)_key[c % _key.Length]));
                ProgressPosition(((c+1)*100/(_text.Length-1)));
                Thread.Sleep(100);
            }
            WriteToFile();
            ProgressPosition(0);
            LabelProgress.BeginInvoke(new Action(() => LabelProgress.Text = @"Done"));
        }

        private void ReverseOperation()
        {
            File.WriteAllText(TxtBoxFile.Text, _text);
            ProgressPosition(0);
            LabelProgress.BeginInvoke(new Action(() => LabelProgress.Text = @"Cancelled"));
        }

        private void GetKey()
        {
            try
            {
                if (String.IsNullOrEmpty(TextBoxPassword.Text)) throw new Exception("Enter Password");
                _key = TextBoxPassword.Text;
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message, @"Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void ReadFromFile()
        {
            try
            {
                _text = "";
                foreach (string line in File.ReadLines(TxtBoxFile.Text))
                {
                    _text += line + "\n";
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message, @"Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            } 
        }

        private void WriteToFile()
        {
            File.WriteAllText(TxtBoxFile.Text, _proccesedText.ToString());
        }

        private void FileBtn_Click(object sender, EventArgs e)
        {
            var fd = new OpenFileDialog();
            fd.Filter = @"Text|*.txt|All|*.*";
            if (fd.ShowDialog() == DialogResult.OK) TxtBoxFile.Text = fd.FileName;
        }

        private void BtnStart_Click(object sender, EventArgs e)
        {
            cts = new CancellationTokenSource();
            ThreadPool.QueueUserWorkItem(_ =>
            {
                EncDec(cts.Token);
            });
        }

        private void BtnCancel_Click(object sender, EventArgs e)
        {
             cts.Cancel();
        }
    }
}