using System;
using System.IO;
using System.Windows.Forms;
using System.Diagnostics;

namespace Fallout_Shelter_Save_Editor
{
    public partial class Form1 : Form
    {
        internal static OpenFileDialog openFile = new OpenFileDialog();
        internal static SaveFileDialog saveFileDialog = new SaveFileDialog();
        public Form1()
        {
            InitializeComponent();
        }

        private void button1_Click(object sender, EventArgs e)
        {
            byte[] decrypted = null;
            if (openFile.ShowDialog() == DialogResult.OK)
            {
                if (new FileInfo(openFile.FileName).Length > 1e7)
                {
                    throw new Exception("File exceeds maximum size of 10MB");
                }
                else
                {
                    decrypted = FSSE.Decrypt(openFile.FileName);
                }
            }
            saveFileDialog.FileName = openFile.FileName.Replace(".sav", "");
            saveFileDialog.Filter = "json|*.json";
            if (saveFileDialog.ShowDialog() == DialogResult.OK)
            {
                File.WriteAllBytes(saveFileDialog.FileName, decrypted);
            }
        }
        private void button2_Click(object sender, EventArgs e)
        {
            byte[] encrypt = null;
            openFile.Filter = "json|*.json";
            if (openFile.ShowDialog() == DialogResult.OK)
            {
                if (new FileInfo(openFile.FileName).Length > 1e7)
                {
                    throw new Exception("File exceeds maximum size of 10MB");
                }
                else
                {
                    encrypt = FSSE.Encrypt(openFile.FileName);
                }
            }
            saveFileDialog.FileName = openFile.FileName.Replace(".json", "");
            saveFileDialog.Filter = "sav|*.sav";
            if (saveFileDialog.ShowDialog() == DialogResult.OK)
            {
                File.WriteAllBytes(saveFileDialog.FileName, encrypt);
            }
        }
        string userName = System.Security.Principal.WindowsIdentity.GetCurrent().Name.Split('\\')[1];//grabs id/username then splits to just grab username
        private void button3_Click(object sender, EventArgs e)
        {
            Process.Start(@"C:\Users\" + userName + @"\AppData\Local\Packages\BethesdaSoftworks.FalloutShelter_3275kfvn8vcwc\SystemAppData\wgs");
        }

        private void button4_Click(object sender, EventArgs e)
        {
            Process.Start(@"C:\Users\" + userName + @"\AppData\Local\FalloutShelter");
        }
    }
}

