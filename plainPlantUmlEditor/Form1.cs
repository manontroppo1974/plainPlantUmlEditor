using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

using System.IO;
using System.Reflection;

namespace plainPlantUmlEditor
{
    public partial class Form1 : Form
    {
        string curFile = null;
        string curDir = null;

        private const uint IMF_DUALFONT = 0x80;
        private const uint WM_USER = 0x0400;
        private const uint EM_SETLANGOPTIONS = WM_USER + 120;
        private const uint EM_GETLANGOPTIONS = WM_USER + 121;

        [System.Runtime.InteropServices.DllImport("USER32.dll")]
        private static extern uint SendMessage(
            System.IntPtr hWnd,
            uint msg,
            uint wParam,
            uint lParam); 

        public Form1()
        {
            InitializeComponent();
        }

        private void textBox1_DragDrop(object sender, DragEventArgs e)
        {
            myDragDrop(sender, e);
        }

        private void textBox1_TextChanged(object sender, EventArgs e)
        {

        }

        private void textBox1_DragEnter(object sender, DragEventArgs e)
        {
            myDragEnter(sender, e);
        }

        private void richTextBox1_DragDrop(object sender, DragEventArgs e)
        {
            myDragDrop(sender, e);
        }

        private void richTextBox1_DragEnter(object sender, DragEventArgs e)
        {
            myDragEnter(sender, e);
        }

        private void Form1_KeyDown(object sender, KeyEventArgs e)
        {
            // ctrl+S
            if (e.KeyCode == Keys.S && e.Control == true)
            {
                Console.WriteLine(curFile);

                if (curFile == null || e.Shift == true)
                {
                    //ファイル保存ダイアログを表示する
                    SaveFileDialog sfd = new SaveFileDialog();
                    sfd.FileName = "";
                    sfd.InitialDirectory = curDir;
                    sfd.Filter = "plantUMLファイル(*.puml)|*.puml|すべてのファイル(*.*)|*.*";
                    sfd.FilterIndex = 1;
                    sfd.Title = "保存先のファイルを選択してください";
                    sfd.RestoreDirectory = true;
                    sfd.OverwritePrompt = true;
                    sfd.CheckPathExists = true;

                    if (sfd.ShowDialog() == DialogResult.OK)
                    {
                        curFile = sfd.FileName;
                        curDir = System.IO.Path.GetDirectoryName(curFile);
                        Console.WriteLine(curFile);
                    }
                    else
                    {
                        Console.WriteLine("Save cancel.");
                        return;
                    }
                }

                using (StreamWriter sw = new StreamWriter(curFile, false, Encoding.UTF8))
                {
                    //sw.Write(textBox1.Text);
                    sw.Write(richTextBox1.Text);
                }

                this.Text = curFile;
                doPlantUml();
            }

            // ctrl+o
            else if(e.KeyCode == Keys.O && e.Control == true)
            {
                //OpenFileDialogを表示する
                OpenFileDialog ofd = new OpenFileDialog();
                ofd.FileName = "";
                ofd.InitialDirectory = curDir;
                ofd.Filter = "plantUMLファイル(*.puml)|*.puml|すべてのファイル(*.*)|*.*";
                ofd.FilterIndex = 1;
                ofd.Title = "開くファイルを選択してください";
                ofd.RestoreDirectory = true;
                ofd.CheckFileExists = true;
                ofd.CheckPathExists = true;

                if (ofd.ShowDialog() == DialogResult.OK)
                {
                    /*
                    curFile = ofd.FileName;
                    curDir = System.IO.Path.GetDirectoryName(curFile);
                    Console.WriteLine(curFile);
                    */
                    openFile(ofd.FileName);
                }
                else
                {
                    Console.WriteLine("Open cancel.");
                    return;
                }
            }

            // ctrl+c
            else if (e.KeyCode == Keys.C && e.Control == true)
            {
                Console.WriteLine("Clipboard.SetImage: pictureBox1.Image");
                Clipboard.SetImage(pictureBox1.Image);
            }

        }

        private void Form1_DragDrop(object sender, DragEventArgs e)
        {
            myDragDrop(sender, e);
        }

        private void Form1_DragEnter(object sender, DragEventArgs e)
        {
            myDragEnter(sender, e);
        }

        private void myDragEnter(object sender, DragEventArgs e)
        {
            //ファイルがドラッグされている場合、カーソルを変更する
            if (e.Data.GetDataPresent(DataFormats.FileDrop))
            {
                e.Effect = DragDropEffects.Copy;
            }
        }

        private void myDragDrop(object sender, DragEventArgs e)
        {
            string[] fileName = (string[])e.Data.GetData(DataFormats.FileDrop, false);
            if (fileName.Length <= 0)
            {
                return;
            }

            Console.WriteLine(fileName[0]);

            /*
            if (!File.Exists(fileName[0])) return;

            using (StreamReader sr = new StreamReader(fileName[0], System.Text.Encoding.UTF8))
            {
                string txt = sr.ReadToEnd();
                textBox1.Text = txt;
                curFile = fileName[0];
                curDir = System.IO.Path.GetDirectoryName(curFile);
                this.Text = curFile;
            }
            */
            openFile(fileName[0]);

            //doPlantUml();
        }

        private void openFile(string fileName)
        {
            if (!File.Exists(fileName)) return;

            using (StreamReader sr = new StreamReader(fileName, System.Text.Encoding.UTF8))
            {
                string txt = sr.ReadToEnd();
                //textBox1.Text = txt;
                richTextBox1.Text = txt;
                curFile = fileName;
                curDir = System.IO.Path.GetDirectoryName(curFile);
                this.Text = curFile;
            }

            doPlantUml();
        }

        private void doPlantUml()
        {
            Assembly myAssembly = Assembly.GetEntryAssembly();
            string appFile = myAssembly.Location;
            string appDir = System.IO.Path.GetDirectoryName(appFile);

            string pngFile = System.IO.Path.ChangeExtension(curFile, "png");
            DateTime lastTimeCur = File.GetLastWriteTime(curFile);
            DateTime lastTimePng = DateTime.MinValue;
            if (File.Exists(pngFile))
            {
                lastTimePng = File.GetLastWriteTime(pngFile);
            }
            Console.WriteLine(curFile + ": " + lastTimeCur);
            Console.WriteLine(pngFile + ": " + lastTimePng);

            if (lastTimeCur > lastTimePng)
            {
                System.Diagnostics.Process pro = new System.Diagnostics.Process();
                pro.StartInfo.FileName = "java";            // コマンド名
                pro.StartInfo.Arguments = @"-jar """ + appDir + @"\plantuml.jar"" -charset utf-8 """ + curFile + @"""";               // 引数
                pro.StartInfo.CreateNoWindow = true;            // DOSプロンプトの黒い画面を非表示
                pro.StartInfo.UseShellExecute = false;          // プロセスを新しいウィンドウで起動するか否か
                pro.StartInfo.RedirectStandardOutput = true;    // 標準出力をリダイレクトして取得したい
                pro.StartInfo.RedirectStandardError = true;
                Console.WriteLine(pro.StartInfo.Arguments);

                pro.Start();

                string so = pro.StandardOutput.ReadToEnd();
                string se = pro.StandardError.ReadToEnd();
                Console.WriteLine(so);
                Console.WriteLine(se);
            }

            if (File.Exists(pngFile))
            {
                pictureBox1.ImageLocation = pngFile;
                pictureBox1.Refresh();
            }
        }

        private void Form1_Load(object sender, EventArgs e)
        {
            this.Size = Properties.Settings.Default.formSize;
            this.Location = Properties.Settings.Default.formPosition;
            //this.textBox1.Size = Properties.Settings.Default.textBoxSize;
            this.richTextBox1.Size = Properties.Settings.Default.textBoxSize;
            this.curDir = Properties.Settings.Default.lastDir;

            this.richTextBox1.AllowDrop = true;
            this.richTextBox1.DragEnter += new System.Windows.Forms.DragEventHandler(this.richTextBox1_DragEnter);
            this.richTextBox1.DragDrop += new System.Windows.Forms.DragEventHandler(this.richTextBox1_DragDrop);
            richTextBox1.LanguageOption = RichTextBoxLanguageOptions.UIFonts;

            // リッチテキストボックスのフォントが勝手に変わるのを抑制
            uint lParam;
            lParam = SendMessage(richTextBox1.Handle, EM_GETLANGOPTIONS, 0, 0);
            lParam &= ~IMF_DUALFONT;
            SendMessage(richTextBox1.Handle, EM_SETLANGOPTIONS, 0, lParam);
        }

        private void Form1_FormClosing(object sender, FormClosingEventArgs e)
        {
            Properties.Settings.Default.formSize = this.Size;
            Properties.Settings.Default.formPosition = this.Location;
            //Properties.Settings.Default.textBoxSize = this.textBox1.Size;
            Properties.Settings.Default.textBoxSize = this.richTextBox1.Size;
            Properties.Settings.Default.lastDir = this.curDir;

            Properties.Settings.Default.Save();
        }

        private void Form1_Shown(object sender, EventArgs e)
        {
            var args = Environment.GetCommandLineArgs();
            if (args.Length >= 2)
            {
                //textBoxArgs.Text = args.Skip(1).Aggregate((a, b) => a + b);
                if (args.Count() >= 2 /*&& Path.GetExtension(args[1]).ToLower() == ".puml"*/)
                {
                    openFile(args[1]);
                }
            }
        }
    }
}
