using System;
using System.Diagnostics;
using System.Drawing;
using System.IO;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;
using SendInputs;
using KeyboardHooksAPI;
using MouseHooksAPI;
using OpenWithSingleInstance;
using Valuechanges;

namespace PlayAndReplay
{
    public partial class Form1 : Form
    {
        [DllImport("winmm.dll", EntryPoint = "timeBeginPeriod")]
        public static extern uint TimeBeginPeriod(uint ms);
        [DllImport("winmm.dll", EntryPoint = "timeEndPeriod")]
        public static extern uint TimeEndPeriod(uint ms);
        [DllImport("ntdll.dll", EntryPoint = "NtSetTimerResolution")]
        public static extern void NtSetTimerResolution(uint DesiredResolution, bool SetResolution, ref uint CurrentResolution);
        public static uint CurrentResolution = 0;
        private static string openFilePath = "", fileTextSaved = "";
        private static bool justSaved = true, onopenwith = false;
        private static DialogResult result;
        private static string filename = "";
        private static bool play, replay, running, enablemouse;
        private static Stopwatch watchplay = new Stopwatch(), watchreplay = new Stopwatch();
        private static double elapseplay, elapsereplay;
        static string KeyboardMouseDriverType = "sendinput"; static double MouseMoveX; static double MouseMoveY; static double MouseAbsX; static double MouseAbsY; static double MouseDesktopX; static double MouseDesktopY; static bool SendLeftClick; static bool SendRightClick; static bool SendMiddleClick; static bool SendWheelUp; static bool SendWheelDown; static bool SendLeft; static bool SendRight; static bool SendUp; static bool SendDown; static bool SendLButton; static bool SendRButton; static bool SendCancel; static bool SendMBUTTON; static bool SendXBUTTON1; static bool SendXBUTTON2; static bool SendBack; static bool SendTab; static bool SendClear; static bool SendReturn; static bool SendSHIFT; static bool SendCONTROL; static bool SendMENU; static bool SendPAUSE; static bool SendCAPITAL; static bool SendKANA; static bool SendHANGEUL; static bool SendHANGUL; static bool SendJUNJA; static bool SendFINAL; static bool SendHANJA; static bool SendKANJI; static bool SendEscape; static bool SendCONVERT; static bool SendNONCONVERT; static bool SendACCEPT; static bool SendMODECHANGE; static bool SendSpace; static bool SendPRIOR; static bool SendNEXT; static bool SendEND; static bool SendHOME; static bool SendLEFT; static bool SendUP; static bool SendRIGHT; static bool SendDOWN; static bool SendSELECT; static bool SendPRINT; static bool SendEXECUTE; static bool SendSNAPSHOT; static bool SendINSERT; static bool SendDELETE; static bool SendHELP; static bool SendAPOSTROPHE; static bool Send0; static bool Send1; static bool Send2; static bool Send3; static bool Send4; static bool Send5; static bool Send6; static bool Send7; static bool Send8; static bool Send9; static bool SendA; static bool SendB; static bool SendC; static bool SendD; static bool SendE; static bool SendF; static bool SendG; static bool SendH; static bool SendI; static bool SendJ; static bool SendK; static bool SendL; static bool SendM; static bool SendN; static bool SendO; static bool SendP; static bool SendQ; static bool SendR; static bool SendS; static bool SendT; static bool SendU; static bool SendV; static bool SendW; static bool SendX; static bool SendY; static bool SendZ; static bool SendLWIN; static bool SendRWIN; static bool SendAPPS; static bool SendSLEEP; static bool SendNUMPAD0; static bool SendNUMPAD1; static bool SendNUMPAD2; static bool SendNUMPAD3; static bool SendNUMPAD4; static bool SendNUMPAD5; static bool SendNUMPAD6; static bool SendNUMPAD7; static bool SendNUMPAD8; static bool SendNUMPAD9; static bool SendMULTIPLY; static bool SendADD; static bool SendSEPARATOR; static bool SendSUBTRACT; static bool SendDECIMAL; static bool SendDIVIDE; static bool SendF1; static bool SendF2; static bool SendF3; static bool SendF4; static bool SendF5; static bool SendF6; static bool SendF7; static bool SendF8; static bool SendF9; static bool SendF10; static bool SendF11; static bool SendF12; static bool SendF13; static bool SendF14; static bool SendF15; static bool SendF16; static bool SendF17; static bool SendF18; static bool SendF19; static bool SendF20; static bool SendF21; static bool SendF22; static bool SendF23; static bool SendF24; static bool SendNUMLOCK; static bool SendSCROLL; static bool SendLeftShift; static bool SendRightShift; static bool SendLeftControl; static bool SendRightControl; static bool SendLMENU; static bool SendRMENU;
        private int linecount = 0;
        private KeyboardHooks kh = new KeyboardHooks();
        private MouseHooks mh = new MouseHooks();
        public static Sendinput sendinput = new Sendinput();
        public static Valuechange ValueChange = new Valuechange();
        private static int[] wd = { 2, 2, 2, 2, 2, 2, 2, 2, 2, 2, 2, 2 };
        private static int[] wu = { 2, 2, 2, 2, 2, 2, 2, 2, 2, 2, 2, 2 };
        public static void valchanged(int n, bool val)
        {
            if (val)
            {
                if (wd[n] <= 1)
                {
                    wd[n] = wd[n] + 1;
                }
                wu[n] = 0;
            }
            else
            {
                if (wu[n] <= 1)
                {
                    wu[n] = wu[n] + 1;
                }
                wd[n] = 0;
            }
        }
        public Form1(string filePath)
        {
            InitializeComponent();
            if (filePath != null)
            {
                onopenwith = true;
                OpenFileWith(filePath);
            }
        }
        protected override void WndProc(ref Message m)
        {
            if (m.Msg == MessageHelper.WM_COPYDATA)
            {
                COPYDATASTRUCT _dataStruct = Marshal.PtrToStructure<COPYDATASTRUCT>(m.LParam);
                string _strMsg = Marshal.PtrToStringUni(_dataStruct.lpData, _dataStruct.cbData / 2);
                OpenFileWith(_strMsg);
            }
            base.WndProc(ref m);
        }
        public void OpenFileWith(string filePath)
        {
            if (!justSaved)
            {
                result = MessageBox.Show("Content will be lost! Are you sure?", "Open", MessageBoxButtons.OKCancel);
                if (result == DialogResult.OK)
                {
                    richTextBox1.Clear();
                    string txt = File.ReadAllText(filePath, Encoding.UTF8);
                    richTextBox1.Text = txt;
                    filename = filePath;
                    openFilePath = filePath;
                    this.Text = filePath;
                    fileTextSaved = richTextBox1.Text;
                    justSaved = true;
                }
            }
            else
            {
                richTextBox1.Clear();
                string txt = File.ReadAllText(filePath, Encoding.UTF8);
                richTextBox1.Text = txt;
                filename = filePath;
                openFilePath = filePath;
                this.Text = filePath;
                fileTextSaved = richTextBox1.Text;
                justSaved = true;
            }
        }
        private void helpToolStripMenuItem_Click(object sender, EventArgs e)
        {
            const string message = "• Play: Use keyboard key shortcut LCtrl + P.\n\r\n\r• Replay: Use keyboard key shortcut LCtrl + R.";
            const string caption = "About";
            MessageBox.Show(message, caption, MessageBoxButtons.OK, MessageBoxIcon.Information);
        }
        private void aboutToolStripMenuItem_Click(object sender, EventArgs e)
        {
            const string message = "• Author: Michaël André Franiatte.\n\r\n\r• Contact: michael.franiatte@gmail.com.\n\r\n\r• Publisher: https://github.com/michaelandrefraniatte.\n\r\n\r• Copyrights: All rights reserved, no permissions granted.\n\r\n\r• License: Not open source, not free of charge to use.";
            const string caption = "About";
            MessageBox.Show(message, caption, MessageBoxButtons.OK, MessageBoxIcon.Information);
        }
        private void Form1_Load(object sender, EventArgs e)
        {
            try
            {
                if (!onopenwith)
                {
                    if (File.Exists(Application.StartupPath + @"\tempsave"))
                    {
                        using (System.IO.StreamReader file = new System.IO.StreamReader(Application.StartupPath + @"\tempsave"))
                        {
                            filename = file.ReadLine();
                        }
                        if (filename != "")
                        {
                            string txt = File.ReadAllText(filename, Encoding.UTF8);
                            richTextBox1.Text = txt;
                            openFilePath = filename;
                            this.Text = filename;
                            fileTextSaved = richTextBox1.Text;
                            justSaved = true;
                        }
                    }
                }
            }
            catch { }
            TimeBeginPeriod(1);
            NtSetTimerResolution(1, true, ref CurrentResolution);
            Task.Run(() => Start());
        }
        private void Start()
        {
            running = true;
            mh.Scan();
            kh.Scan();
            mh.BeginPolling();
            kh.BeginPolling();
            sendinput.Connect();
            Task.Run(() => task());
        }
        private void task()
        {
            for (; ; )
            {
                if (!running)
                    break;
                valchanged(0, kh.Key_LeftControl & kh.Key_P);
                if (wd[0] == 1)
                {
                    Play();
                }
                valchanged(1, kh.Key_LeftControl & kh.Key_R);
                if (wd[1] == 1)
                {
                    Replay();
                }
                Thread.Sleep(50);
            }
        }
        private void newToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (!justSaved)
            {
                result = MessageBox.Show("Content will be lost! Are you sure?", "New", MessageBoxButtons.OKCancel);
                if (result == DialogResult.OK)
                {
                    richTextBox1.Clear();
                    this.Text = "TextEditor";
                    openFilePath = "";
                    fileTextSaved = richTextBox1.Text;
                    justSaved = true;
                }
            }
            else
            {
                richTextBox1.Clear();
                this.Text = "TextEditor";
                openFilePath = "";
                fileTextSaved = richTextBox1.Text;
                justSaved = true;
            }
        }
        private void openToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (!justSaved)
            {
                result = MessageBox.Show("Content will be lost! Are you sure?", "Open", MessageBoxButtons.OKCancel);
                if (result == DialogResult.OK)
                {
                    OpenFileDialog op = new OpenFileDialog();
                    op.Filter = "All Files(*.*)|*.*";
                    if (op.ShowDialog() == DialogResult.OK)
                    {
                        richTextBox1.Clear();
                        string txt = File.ReadAllText(op.FileName, Encoding.UTF8);
                        richTextBox1.Text = txt;
                        filename = op.FileName;
                        openFilePath = op.FileName;
                        this.Text = op.FileName;
                        fileTextSaved = richTextBox1.Text;
                        justSaved = true;
                    }
                }
            }
            else
            {
                OpenFileDialog op = new OpenFileDialog();
                op.Filter = "All Files(*.*)|*.*";
                if (op.ShowDialog() == DialogResult.OK)
                {
                    richTextBox1.Clear();
                    string txt = File.ReadAllText(op.FileName, Encoding.UTF8);
                    richTextBox1.Text = txt;
                    filename = op.FileName;
                    openFilePath = op.FileName;
                    this.Text = op.FileName;
                    fileTextSaved = richTextBox1.Text;
                    justSaved = true;
                }
            }
        }
        private void saveToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (openFilePath == null | openFilePath == "")
            {
                saveAsToolStripMenuItem_Click(sender, e);
            }
            else
            {
                File.WriteAllText(openFilePath, richTextBox1.Text, Encoding.UTF8);
                fileTextSaved = richTextBox1.Text;
                justSaved = true;
            }
        }
        private void saveAsToolStripMenuItem_Click(object sender, EventArgs e)
        {
            SaveFileDialog sf = new SaveFileDialog();
            sf.Filter = "All Files(*.txt)|*.txt";
            if (sf.ShowDialog() == DialogResult.OK)
            {
                File.WriteAllText(sf.FileName, richTextBox1.Text, Encoding.UTF8);
                filename = sf.FileName;
                openFilePath = sf.FileName;
                this.Text = sf.FileName;
                fileTextSaved = richTextBox1.Text;
                justSaved = true;
            }
        }
        private void richTextBox1_TextChanged(object sender, EventArgs e)
        {
            if (fileTextSaved != richTextBox1.Text)
                justSaved = false;
        }
        private void Form1_FormClosing(object sender, FormClosingEventArgs e)
        {
            if (!justSaved)
            {
                result = MessageBox.Show("Content will be lost! Are you sure?", "Exit", MessageBoxButtons.OKCancel);
                if (result == DialogResult.Cancel)
                    e.Cancel = true;
            }
            if (filename != "")
            {
                using (System.IO.StreamWriter createdfile = new System.IO.StreamWriter(Application.StartupPath + @"\tempsave"))
                {
                    createdfile.WriteLine(filename);
                }
            }
            running = false;
            Thread.Sleep(100);
            kh.Close();
            mh.Close();
            sendinput.Disconnect();
        }
        private void playToolStripMenuItem_Click(object sender, EventArgs e)
        {
            Play();
        }
        private void replayToolStripMenuItem_Click(object sender, EventArgs e)
        {
            Replay();
        }
        private void Play()
        {
            if (replay)
            {
                replay = false;
                replayToolStripMenuItem.Text = "Replay";
                Thread.Sleep(100);
                watchreplay.Stop();
            }
            if (!play)
            {
                play = true;
                playToolStripMenuItem.Text = "Stop";
                richTextBox1.Clear();
                enablemouse = optionToolStripMenuItem.Checked;
                watchplay = new Stopwatch();
                watchplay.Start();
                Task.Run(() => taskPlay());
            }
            else
            {
                play = false;
                playToolStripMenuItem.Text = "Play";
                Thread.Sleep(100);
                watchplay.Stop();
            }
        }
        private void Replay()
        {
            if (play)
            {
                play = false;
                playToolStripMenuItem.Text = "Play";
                Thread.Sleep(100);
                watchplay.Stop();
            }
            if (!replay)
            {
                replay = true;
                replayToolStripMenuItem.Text = "Stop";
                richTextBox2.Clear();
                string[] lines = richTextBox1.Text.Split(new[] { Environment.NewLine }, StringSplitOptions.RemoveEmptyEntries);
                foreach (string line in lines)
                {
                    richTextBox2.AppendText(line + ";\r\n");
                }
                linecount = 0;
                Init();
                enablemouse = optionToolStripMenuItem.Checked;
                watchreplay = new Stopwatch();
                watchreplay.Start();
                Task.Run(() => taskReplay());
            }
            else
            {
                replay = false;
                replayToolStripMenuItem.Text = "Replay";
                Thread.Sleep(100);
                watchreplay.Stop();
            }
        }
        private void taskPlay()
        {
            for (; ; )
            {
                if (!play)
                    break;
                elapseplay = (double)watchplay.ElapsedTicks / (Stopwatch.Frequency / (1000L * 1000L));
                ValueChange[0] = kh.Key_0 ? 1 : -1;
                if (ValueChange._ValueChange[0] != 0)
                    richTextBox1.AppendText(elapseplay + "; Key_0; " + kh.Key_0 + "; \r\n");
                ValueChange[1] = kh.Key_1 ? 1 : -1;
                if (ValueChange._ValueChange[1] != 0)
                    richTextBox1.AppendText(elapseplay + "; Key_1; " + kh.Key_1 + "; \r\n");
                ValueChange[2] = kh.Key_2 ? 1 : -1;
                if (ValueChange._ValueChange[2] != 0)
                    richTextBox1.AppendText(elapseplay + "; Key_2; " + kh.Key_2 + "; \r\n");
                ValueChange[3] = kh.Key_3 ? 1 : -1;
                if (ValueChange._ValueChange[3] != 0)
                    richTextBox1.AppendText(elapseplay + "; Key_3; " + kh.Key_3 + "; \r\n");
                ValueChange[4] = kh.Key_4 ? 1 : -1;
                if (ValueChange._ValueChange[4] != 0)
                    richTextBox1.AppendText(elapseplay + "; Key_4; " + kh.Key_4 + "; \r\n");
                ValueChange[5] = kh.Key_5 ? 1 : -1;
                if (ValueChange._ValueChange[5] != 0)
                    richTextBox1.AppendText(elapseplay + "; Key_5; " + kh.Key_5 + "; \r\n");
                ValueChange[6] = kh.Key_6 ? 1 : -1;
                if (ValueChange._ValueChange[6] != 0)
                    richTextBox1.AppendText(elapseplay + "; Key_6; " + kh.Key_6 + "; \r\n");
                ValueChange[7] = kh.Key_7 ? 1 : -1;
                if (ValueChange._ValueChange[7] != 0)
                    richTextBox1.AppendText(elapseplay + "; Key_7; " + kh.Key_7 + "; \r\n");
                ValueChange[8] = kh.Key_8 ? 1 : -1;
                if (ValueChange._ValueChange[8] != 0)
                    richTextBox1.AppendText(elapseplay + "; Key_8; " + kh.Key_8 + "; \r\n");
                ValueChange[9] = kh.Key_9 ? 1 : -1;
                if (ValueChange._ValueChange[9] != 0)
                    richTextBox1.AppendText(elapseplay + "; Key_9; " + kh.Key_9 + "; \r\n");
                ValueChange[10] = kh.Key_A ? 1 : -1;
                if (ValueChange._ValueChange[10] != 0)
                    richTextBox1.AppendText(elapseplay + "; Key_A; " + kh.Key_A + "; \r\n");
                ValueChange[11] = kh.Key_B ? 1 : -1;
                if (ValueChange._ValueChange[11] != 0)
                    richTextBox1.AppendText(elapseplay + "; Key_B; " + kh.Key_B + "; \r\n");
                ValueChange[12] = kh.Key_C ? 1 : -1;
                if (ValueChange._ValueChange[12] != 0)
                    richTextBox1.AppendText(elapseplay + "; Key_C; " + kh.Key_C + "; \r\n");
                ValueChange[13] = kh.Key_D ? 1 : -1;
                if (ValueChange._ValueChange[13] != 0)
                    richTextBox1.AppendText(elapseplay + "; Key_D; " + kh.Key_D + "; \r\n");
                ValueChange[14] = kh.Key_E ? 1 : -1;
                if (ValueChange._ValueChange[14] != 0)
                    richTextBox1.AppendText(elapseplay + "; Key_E; " + kh.Key_E + "; \r\n");
                ValueChange[15] = kh.Key_F ? 1 : -1;
                if (ValueChange._ValueChange[15] != 0)
                    richTextBox1.AppendText(elapseplay + "; Key_F; " + kh.Key_F + "; \r\n");
                ValueChange[16] = kh.Key_G ? 1 : -1;
                if (ValueChange._ValueChange[16] != 0)
                    richTextBox1.AppendText(elapseplay + "; Key_G; " + kh.Key_G + "; \r\n");
                ValueChange[17] = kh.Key_H ? 1 : -1;
                if (ValueChange._ValueChange[17] != 0)
                    richTextBox1.AppendText(elapseplay + "; Key_H; " + kh.Key_H + "; \r\n");
                ValueChange[18] = kh.Key_I ? 1 : -1;
                if (ValueChange._ValueChange[18] != 0)
                    richTextBox1.AppendText(elapseplay + "; Key_I; " + kh.Key_I + "; \r\n");
                ValueChange[19] = kh.Key_J ? 1 : -1;
                if (ValueChange._ValueChange[19] != 0)
                    richTextBox1.AppendText(elapseplay + "; Key_J; " + kh.Key_J + "; \r\n");
                ValueChange[20] = kh.Key_K ? 1 : -1;
                if (ValueChange._ValueChange[20] != 0)
                    richTextBox1.AppendText(elapseplay + "; Key_K; " + kh.Key_K + "; \r\n");
                ValueChange[21] = kh.Key_L ? 1 : -1;
                if (ValueChange._ValueChange[21] != 0)
                    richTextBox1.AppendText(elapseplay + "; Key_L; " + kh.Key_L + "; \r\n");
                ValueChange[22] = kh.Key_M ? 1 : -1;
                if (ValueChange._ValueChange[22] != 0)
                    richTextBox1.AppendText(elapseplay + "; Key_M; " + kh.Key_M + "; \r\n");
                ValueChange[23] = kh.Key_N ? 1 : -1;
                if (ValueChange._ValueChange[23] != 0)
                    richTextBox1.AppendText(elapseplay + "; Key_N; " + kh.Key_N + "; \r\n");
                ValueChange[24] = kh.Key_O ? 1 : -1;
                if (ValueChange._ValueChange[24] != 0)
                    richTextBox1.AppendText(elapseplay + "; Key_O; " + kh.Key_O + "; \r\n");
                ValueChange[25] = kh.Key_P ? 1 : -1;
                if (ValueChange._ValueChange[25] != 0)
                    richTextBox1.AppendText(elapseplay + "; Key_P; " + kh.Key_P + "; \r\n");
                ValueChange[26] = kh.Key_Q ? 1 : -1;
                if (ValueChange._ValueChange[26] != 0)
                    richTextBox1.AppendText(elapseplay + "; Key_Q; " + kh.Key_Q + "; \r\n");
                ValueChange[27] = kh.Key_R ? 1 : -1;
                if (ValueChange._ValueChange[27] != 0)
                    richTextBox1.AppendText(elapseplay + "; Key_R; " + kh.Key_R + "; \r\n");
                ValueChange[28] = kh.Key_S ? 1 : -1;
                if (ValueChange._ValueChange[28] != 0)
                    richTextBox1.AppendText(elapseplay + "; Key_S; " + kh.Key_S + "; \r\n");
                ValueChange[29] = kh.Key_T ? 1 : -1;
                if (ValueChange._ValueChange[29] != 0)
                    richTextBox1.AppendText(elapseplay + "; Key_T; " + kh.Key_T + "; \r\n");
                ValueChange[30] = kh.Key_U ? 1 : -1;
                if (ValueChange._ValueChange[30] != 0)
                    richTextBox1.AppendText(elapseplay + "; Key_U; " + kh.Key_U + "; \r\n");
                ValueChange[31] = kh.Key_V ? 1 : -1;
                if (ValueChange._ValueChange[31] != 0)
                    richTextBox1.AppendText(elapseplay + "; Key_V; " + kh.Key_V + "; \r\n");
                ValueChange[32] = kh.Key_W ? 1 : -1;
                if (ValueChange._ValueChange[32] != 0)
                    richTextBox1.AppendText(elapseplay + "; Key_W; " + kh.Key_W + "; \r\n");
                ValueChange[33] = kh.Key_X ? 1 : -1;
                if (ValueChange._ValueChange[33] != 0)
                    richTextBox1.AppendText(elapseplay + "; Key_X; " + kh.Key_X + "; \r\n");
                ValueChange[34] = kh.Key_Y ? 1 : -1;
                if (ValueChange._ValueChange[34] != 0)
                    richTextBox1.AppendText(elapseplay + "; Key_Y; " + kh.Key_Y + "; \r\n");
                ValueChange[35] = kh.Key_Z ? 1 : -1;
                if (ValueChange._ValueChange[35] != 0)
                    richTextBox1.AppendText(elapseplay + "; Key_Z; " + kh.Key_Z + "; \r\n");
                ValueChange[36] = kh.Key_F1 ? 1 : -1;
                if (ValueChange._ValueChange[36] != 0)
                    richTextBox1.AppendText(elapseplay + "; Key_F1; " + kh.Key_F1 + "; \r\n");
                ValueChange[37] = kh.Key_F2 ? 1 : -1;
                if (ValueChange._ValueChange[37] != 0)
                    richTextBox1.AppendText(elapseplay + "; Key_F2; " + kh.Key_F2 + "; \r\n");
                ValueChange[38] = kh.Key_F3 ? 1 : -1;
                if (ValueChange._ValueChange[38] != 0)
                    richTextBox1.AppendText(elapseplay + "; Key_F3; " + kh.Key_F3 + "; \r\n");
                ValueChange[39] = kh.Key_F4 ? 1 : -1;
                if (ValueChange._ValueChange[39] != 0)
                    richTextBox1.AppendText(elapseplay + "; Key_F4; " + kh.Key_F4 + "; \r\n");
                ValueChange[40] = kh.Key_F5 ? 1 : -1;
                if (ValueChange._ValueChange[40] != 0)
                    richTextBox1.AppendText(elapseplay + "; Key_F5; " + kh.Key_F5 + "; \r\n");
                ValueChange[41] = kh.Key_F6 ? 1 : -1;
                if (ValueChange._ValueChange[41] != 0)
                    richTextBox1.AppendText(elapseplay + "; Key_F6; " + kh.Key_F6 + "; \r\n");
                ValueChange[42] = kh.Key_F7 ? 1 : -1;
                if (ValueChange._ValueChange[42] != 0)
                    richTextBox1.AppendText(elapseplay + "; Key_F7; " + kh.Key_F7 + "; \r\n");
                ValueChange[43] = kh.Key_F8 ? 1 : -1;
                if (ValueChange._ValueChange[43] != 0)
                    richTextBox1.AppendText(elapseplay + "; Key_F8; " + kh.Key_F8 + "; \r\n");
                ValueChange[44] = kh.Key_F9 ? 1 : -1;
                if (ValueChange._ValueChange[44] != 0)
                    richTextBox1.AppendText(elapseplay + "; Key_F9; " + kh.Key_F9 + "; \r\n");
                ValueChange[45] = kh.Key_F10 ? 1 : -1;
                if (ValueChange._ValueChange[45] != 0)
                    richTextBox1.AppendText(elapseplay + "; Key_F10; " + kh.Key_F10 + "; \r\n");
                ValueChange[46] = kh.Key_F11 ? 1 : -1;
                if (ValueChange._ValueChange[46] != 0)
                    richTextBox1.AppendText(elapseplay + "; Key_F11; " + kh.Key_F11 + "; \r\n");
                ValueChange[47] = kh.Key_F12 ? 1 : -1;
                if (ValueChange._ValueChange[47] != 0)
                    richTextBox1.AppendText(elapseplay + "; Key_F12; " + kh.Key_F12 + "; \r\n");
                ValueChange[48] = kh.Key_F13 ? 1 : -1;
                if (ValueChange._ValueChange[48] != 0)
                    richTextBox1.AppendText(elapseplay + "; Key_F13; " + kh.Key_F13 + "; \r\n");
                ValueChange[49] = kh.Key_F14 ? 1 : -1;
                if (ValueChange._ValueChange[49] != 0)
                    richTextBox1.AppendText(elapseplay + "; Key_F14; " + kh.Key_F14 + "; \r\n");
                ValueChange[50] = kh.Key_F15 ? 1 : -1;
                if (ValueChange._ValueChange[50] != 0)
                    richTextBox1.AppendText(elapseplay + "; Key_F15; " + kh.Key_F15 + "; \r\n");
                ValueChange[51] = kh.Key_F16 ? 1 : -1;
                if (ValueChange._ValueChange[51] != 0)
                    richTextBox1.AppendText(elapseplay + "; Key_F16; " + kh.Key_F16 + "; \r\n");
                ValueChange[52] = kh.Key_F17 ? 1 : -1;
                if (ValueChange._ValueChange[52] != 0)
                    richTextBox1.AppendText(elapseplay + "; Key_F17; " + kh.Key_F17 + "; \r\n");
                ValueChange[53] = kh.Key_F18 ? 1 : -1;
                if (ValueChange._ValueChange[53] != 0)
                    richTextBox1.AppendText(elapseplay + "; Key_F18; " + kh.Key_F18 + "; \r\n");
                ValueChange[54] = kh.Key_F19 ? 1 : -1;
                if (ValueChange._ValueChange[54] != 0)
                    richTextBox1.AppendText(elapseplay + "; Key_F19; " + kh.Key_F19 + "; \r\n");
                ValueChange[55] = kh.Key_F20 ? 1 : -1;
                if (ValueChange._ValueChange[55] != 0)
                    richTextBox1.AppendText(elapseplay + "; Key_F20; " + kh.Key_F20 + "; \r\n");
                ValueChange[56] = kh.Key_F21 ? 1 : -1;
                if (ValueChange._ValueChange[56] != 0)
                    richTextBox1.AppendText(elapseplay + "; Key_F21; " + kh.Key_F21 + "; \r\n");
                ValueChange[57] = kh.Key_F22 ? 1 : -1;
                if (ValueChange._ValueChange[57] != 0)
                    richTextBox1.AppendText(elapseplay + "; Key_F22; " + kh.Key_F22 + "; \r\n");
                ValueChange[58] = kh.Key_F23 ? 1 : -1;
                if (ValueChange._ValueChange[58] != 0)
                    richTextBox1.AppendText(elapseplay + "; Key_F23; " + kh.Key_F23 + "; \r\n");
                ValueChange[59] = kh.Key_F24 ? 1 : -1;
                if (ValueChange._ValueChange[59] != 0)
                    richTextBox1.AppendText(elapseplay + "; Key_F24; " + kh.Key_F24 + "; \r\n");
                ValueChange[60] = kh.Key_NUMPAD0 ? 1 : -1;
                if (ValueChange._ValueChange[60] != 0)
                    richTextBox1.AppendText(elapseplay + "; Key_NUMPAD0; " + kh.Key_NUMPAD0 + "; \r\n");
                ValueChange[61] = kh.Key_NUMPAD1 ? 1 : -1;
                if (ValueChange._ValueChange[61] != 0)
                    richTextBox1.AppendText(elapseplay + "; Key_NUMPAD1; " + kh.Key_NUMPAD1 + "; \r\n");
                ValueChange[62] = kh.Key_NUMPAD2 ? 1 : -1;
                if (ValueChange._ValueChange[62] != 0)
                    richTextBox1.AppendText(elapseplay + "; Key_NUMPAD2; " + kh.Key_NUMPAD2 + "; \r\n");
                ValueChange[63] = kh.Key_NUMPAD3 ? 1 : -1;
                if (ValueChange._ValueChange[63] != 0)
                    richTextBox1.AppendText(elapseplay + "; Key_NUMPAD3; " + kh.Key_NUMPAD3 + "; \r\n");
                ValueChange[64] = kh.Key_NUMPAD4 ? 1 : -1;
                if (ValueChange._ValueChange[64] != 0)
                    richTextBox1.AppendText(elapseplay + "; Key_NUMPAD4; " + kh.Key_NUMPAD4 + "; \r\n");
                ValueChange[65] = kh.Key_NUMPAD5 ? 1 : -1;
                if (ValueChange._ValueChange[65] != 0)
                    richTextBox1.AppendText(elapseplay + "; Key_NUMPAD5; " + kh.Key_NUMPAD5 + "; \r\n");
                ValueChange[66] = kh.Key_NUMPAD6 ? 1 : -1;
                if (ValueChange._ValueChange[66] != 0)
                    richTextBox1.AppendText(elapseplay + "; Key_NUMPAD6; " + kh.Key_NUMPAD6 + "; \r\n");
                ValueChange[67] = kh.Key_NUMPAD7 ? 1 : -1;
                if (ValueChange._ValueChange[67] != 0)
                    richTextBox1.AppendText(elapseplay + "; Key_NUMPAD7; " + kh.Key_NUMPAD7 + "; \r\n");
                ValueChange[68] = kh.Key_NUMPAD8 ? 1 : -1;
                if (ValueChange._ValueChange[68] != 0)
                    richTextBox1.AppendText(elapseplay + "; Key_NUMPAD8; " + kh.Key_NUMPAD8 + "; \r\n");
                ValueChange[69] = kh.Key_NUMPAD9 ? 1 : -1;
                if (ValueChange._ValueChange[69] != 0)
                    richTextBox1.AppendText(elapseplay + "; Key_NUMPAD9; " + kh.Key_NUMPAD9 + "; \r\n");
                ValueChange[70] = kh.Key_LWIN ? 1 : -1;
                if (ValueChange._ValueChange[70] != 0)
                    richTextBox1.AppendText(elapseplay + "; Key_LWIN; " + kh.Key_LWIN + "; \r\n");
                ValueChange[71] = kh.Key_RWIN ? 1 : -1;
                if (ValueChange._ValueChange[71] != 0)
                    richTextBox1.AppendText(elapseplay + "; Key_RWIN; " + kh.Key_RWIN + "; \r\n");
                ValueChange[72] = kh.Key_APPS ? 1 : -1;
                if (ValueChange._ValueChange[72] != 0)
                    richTextBox1.AppendText(elapseplay + "; Key_APPS; " + kh.Key_APPS + "; \r\n");
                ValueChange[73] = kh.Key_SLEEP ? 1 : -1;
                if (ValueChange._ValueChange[73] != 0)
                    richTextBox1.AppendText(elapseplay + "; Key_SLEEP; " + kh.Key_SLEEP + "; \r\n");
                ValueChange[74] = kh.Key_LBUTTON ? 1 : -1;
                if (ValueChange._ValueChange[74] != 0)
                    richTextBox1.AppendText(elapseplay + "; Key_LBUTTON; " + kh.Key_LBUTTON + "; \r\n");
                ValueChange[75] = kh.Key_RBUTTON ? 1 : -1;
                if (ValueChange._ValueChange[75] != 0)
                    richTextBox1.AppendText(elapseplay + "; Key_RBUTTON; " + kh.Key_LBUTTON + "; \r\n");
                ValueChange[76] = kh.Key_CANCEL ? 1 : -1;
                if (ValueChange._ValueChange[76] != 0)
                    richTextBox1.AppendText(elapseplay + "; Key_CANCEL; " + kh.Key_CANCEL + "; \r\n");
                ValueChange[77] = kh.Key_MBUTTON ? 1 : -1;
                if (ValueChange._ValueChange[77] != 0)
                    richTextBox1.AppendText(elapseplay + "; Key_MBUTTON; " + kh.Key_MBUTTON + "; \r\n");
                ValueChange[78] = kh.Key_XBUTTON1 ? 1 : -1;
                if (ValueChange._ValueChange[78] != 0)
                    richTextBox1.AppendText(elapseplay + "; Key_XBUTTON1; " + kh.Key_XBUTTON1 + "; \r\n");
                ValueChange[79] = kh.Key_XBUTTON2 ? 1 : -1;
                if (ValueChange._ValueChange[79] != 0)
                    richTextBox1.AppendText(elapseplay + "; Key_XBUTTON2; " + kh.Key_XBUTTON2 + "; \r\n");
                ValueChange[80] = kh.Key_BACK ? 1 : -1;
                if (ValueChange._ValueChange[80] != 0)
                    richTextBox1.AppendText(elapseplay + "; Key_BACK; " + kh.Key_BACK + "; \r\n");
                ValueChange[81] = kh.Key_Tab ? 1 : -1;
                if (ValueChange._ValueChange[81] != 0)
                    richTextBox1.AppendText(elapseplay + "; Key_Tab; " + kh.Key_Tab + "; \r\n");
                ValueChange[82] = kh.Key_CLEAR ? 1 : -1;
                if (ValueChange._ValueChange[82] != 0)
                    richTextBox1.AppendText(elapseplay + "; Key_CLEAR; " + kh.Key_CLEAR + "; \r\n");
                ValueChange[83] = kh.Key_Return ? 1 : -1;
                if (ValueChange._ValueChange[83] != 0)
                    richTextBox1.AppendText(elapseplay + "; Key_Return; " + kh.Key_Return + "; \r\n");
                ValueChange[84] = kh.Key_SHIFT ? 1 : -1;
                if (ValueChange._ValueChange[84] != 0)
                    richTextBox1.AppendText(elapseplay + "; Key_SHIFT; " + kh.Key_SHIFT + "; \r\n");
                ValueChange[85] = kh.Key_CONTROL ? 1 : -1;
                if (ValueChange._ValueChange[85] != 0)
                    richTextBox1.AppendText(elapseplay + "; Key_CONTROL; " + kh.Key_CONTROL + "; \r\n");
                ValueChange[86] = kh.Key_MENU ? 1 : -1;
                if (ValueChange._ValueChange[86] != 0)
                    richTextBox1.AppendText(elapseplay + "; Key_MENU; " + kh.Key_MENU + "; \r\n");
                ValueChange[87] = kh.Key_PAUSE ? 1 : -1;
                if (ValueChange._ValueChange[87] != 0)
                    richTextBox1.AppendText(elapseplay + "; Key_PAUSE; " + kh.Key_PAUSE + "; \r\n");
                ValueChange[88] = kh.Key_CAPITAL ? 1 : -1;
                if (ValueChange._ValueChange[88] != 0)
                    richTextBox1.AppendText(elapseplay + "; Key_CAPITAL; " + kh.Key_CAPITAL + "; \r\n");
                ValueChange[89] = kh.Key_KANA ? 1 : -1;
                if (ValueChange._ValueChange[89] != 0)
                    richTextBox1.AppendText(elapseplay + "; Key_KANA; " + kh.Key_KANA + "; \r\n");
                ValueChange[90] = kh.Key_HANGEUL ? 1 : -1;
                if (ValueChange._ValueChange[90] != 0)
                    richTextBox1.AppendText(elapseplay + "; Key_HANGEUL; " + kh.Key_HANGEUL + "; \r\n");
                ValueChange[91] = kh.Key_HANGUL ? 1 : -1;
                if (ValueChange._ValueChange[91] != 0)
                    richTextBox1.AppendText(elapseplay + "; Key_HANGUL; " + kh.Key_HANGUL + "; \r\n");
                ValueChange[92] = kh.Key_JUNJA ? 1 : -1;
                if (ValueChange._ValueChange[92] != 0)
                    richTextBox1.AppendText(elapseplay + "; Key_JUNJA; " + kh.Key_JUNJA + "; \r\n");
                ValueChange[93] = kh.Key_FINAL ? 1 : -1;
                if (ValueChange._ValueChange[93] != 0)
                    richTextBox1.AppendText(elapseplay + "; Key_FINAL; " + kh.Key_FINAL + "; \r\n");
                ValueChange[94] = kh.Key_HANJA ? 1 : -1;
                if (ValueChange._ValueChange[94] != 0)
                    richTextBox1.AppendText(elapseplay + "; Key_HANJA; " + kh.Key_HANJA + "; \r\n");
                ValueChange[95] = kh.Key_KANJI ? 1 : -1;
                if (ValueChange._ValueChange[95] != 0)
                    richTextBox1.AppendText(elapseplay + "; Key_KANJI; " + kh.Key_KANJI + "; \r\n");
                str += "Key_Escape : " + Key_Escape + Environment.NewLine;
                str += "Key_CONVERT : " + Key_CONVERT + Environment.NewLine;
                str += "Key_NONCONVERT : " + Key_NONCONVERT + Environment.NewLine;
                str += "Key_ACCEPT : " + Key_ACCEPT + Environment.NewLine;
                str += "Key_MODECHANGE : " + Key_MODECHANGE + Environment.NewLine;
                str += "Key_Space : " + Key_Space + Environment.NewLine;
                str += "Key_PRIOR : " + Key_PRIOR + Environment.NewLine;
                str += "Key_NEXT : " + Key_NEXT + Environment.NewLine;
                str += "Key_END : " + Key_END + Environment.NewLine;
                str += "Key_HOME : " + Key_HOME + Environment.NewLine;
                str += "Key_LEFT : " + Key_LEFT + Environment.NewLine;
                str += "Key_UP : " + Key_UP + Environment.NewLine;
                str += "Key_RIGHT : " + Key_RIGHT + Environment.NewLine;
                str += "Key_DOWN : " + Key_DOWN + Environment.NewLine;
                str += "Key_SELECT : " + Key_SELECT + Environment.NewLine;
                str += "Key_PRINT : " + Key_PRINT + Environment.NewLine;
                str += "Key_EXECUTE : " + Key_EXECUTE + Environment.NewLine;
                str += "Key_SNAPSHOT : " + Key_SNAPSHOT + Environment.NewLine;
                str += "Key_INSERT : " + Key_INSERT + Environment.NewLine;
                str += "Key_DELETE : " + Key_DELETE + Environment.NewLine;
                str += "Key_HELP : " + Key_HELP + Environment.NewLine;
                str += "Key_APOSTROPHE : " + Key_APOSTROPHE + Environment.NewLine;
                str += "Key_MULTIPLY : " + Key_MULTIPLY + Environment.NewLine;
                str += "Key_ADD : " + Key_ADD + Environment.NewLine;
                str += "Key_SEPARATOR : " + Key_SEPARATOR + Environment.NewLine;
                str += "Key_SUBTRACT : " + Key_SUBTRACT + Environment.NewLine;
                str += "Key_DECIMAL : " + Key_DECIMAL + Environment.NewLine;
                str += "Key_DIVIDE : " + Key_DIVIDE + Environment.NewLine;
                str += "Key_NUMLOCK : " + Key_NUMLOCK + Environment.NewLine;
                str += "Key_SCROLL : " + Key_SCROLL + Environment.NewLine;
                str += "Key_LeftShift : " + Key_LeftShift + Environment.NewLine;
                str += "Key_RightShift : " + Key_RightShift + Environment.NewLine;
                str += "Key_LeftControl : " + Key_LeftControl + Environment.NewLine;
                str += "Key_RightControl : " + Key_RightControl + Environment.NewLine;
                str += "Key_LMENU : " + Key_LMENU + Environment.NewLine;
                str += "Key_RMENU : " + Key_RMENU + Environment.NewLine;
                if (enablemouse)
                {
                    string str = "CursorX : " + CursorX + Environment.NewLine;
                    str += "CursorY : " + CursorY + Environment.NewLine;
                    str += "MouseX : " + MouseX + Environment.NewLine;
                    str += "MouseY : " + MouseY + Environment.NewLine;
                    str += "MouseZ : " + MouseZ + Environment.NewLine;
                    str += "MouseRightButton : " + MouseRightButton + Environment.NewLine;
                    str += "MouseLeftButton : " + MouseLeftButton + Environment.NewLine;
                    str += "MouseMiddleButton : " + MouseMiddleButton + Environment.NewLine;
                    str += "MouseXButton : " + MouseXButton + Environment.NewLine;
                    str += "MouseButtonX : " + MouseButtonX + Environment.NewLine;
                }
                Thread.Sleep(1);
            }
        }
        private void taskReplay()
        {
            for (; ; )
            {
                if (!replay)
                    break;
                if (linecount < richTextBox2.Lines.Length)
                {
                    elapsereplay = (double)watchreplay.ElapsedTicks / (Stopwatch.Frequency / (1000L * 1000L));
                    var line = richTextBox2.Lines[linecount];
                    var data = line.Split(';');
                    double time = Convert.ToSingle(data[0]);
                    if (elapsereplay >= time)
                    {
                        richTextBox2.Select(richTextBox2.GetFirstCharIndexFromLine(linecount), richTextBox2.Lines[linecount].Length);
                        richTextBox2.SelectionColor = Color.Red;
                        linecount++;
                        if (data[1] == " Key_0")
                        {
                            Send0 = bool.Parse(data[2]);
                        }
                        str += "Key_1 : " + Key_1 + Environment.NewLine;
                        str += "Key_2 : " + Key_2 + Environment.NewLine;
                        str += "Key_3 : " + Key_3 + Environment.NewLine;
                        str += "Key_4 : " + Key_4 + Environment.NewLine;
                        str += "Key_5 : " + Key_5 + Environment.NewLine;
                        str += "Key_6 : " + Key_6 + Environment.NewLine;
                        str += "Key_7 : " + Key_7 + Environment.NewLine;
                        str += "Key_8 : " + Key_8 + Environment.NewLine;
                        str += "Key_9 : " + Key_9 + Environment.NewLine;
                        str += "Key_A : " + Key_A + Environment.NewLine;
                        str += "Key_B : " + Key_B + Environment.NewLine;
                        str += "Key_C : " + Key_C + Environment.NewLine;
                        str += "Key_D : " + Key_D + Environment.NewLine;
                        str += "Key_E : " + Key_E + Environment.NewLine;
                        str += "Key_F : " + Key_F + Environment.NewLine;
                        str += "Key_G : " + Key_G + Environment.NewLine;
                        str += "Key_H : " + Key_H + Environment.NewLine;
                        str += "Key_I : " + Key_I + Environment.NewLine;
                        str += "Key_J : " + Key_J + Environment.NewLine;
                        str += "Key_K : " + Key_K + Environment.NewLine;
                        str += "Key_L : " + Key_L + Environment.NewLine;
                        str += "Key_M : " + Key_M + Environment.NewLine;
                        str += "Key_N : " + Key_N + Environment.NewLine;
                        str += "Key_O : " + Key_O + Environment.NewLine;
                        str += "Key_P : " + Key_P + Environment.NewLine;
                        str += "Key_Q : " + Key_Q + Environment.NewLine;
                        str += "Key_R : " + Key_R + Environment.NewLine;
                        str += "Key_S : " + Key_S + Environment.NewLine;
                        str += "Key_T : " + Key_T + Environment.NewLine;
                        str += "Key_U : " + Key_U + Environment.NewLine;
                        str += "Key_V : " + Key_V + Environment.NewLine;
                        str += "Key_W : " + Key_W + Environment.NewLine;
                        str += "Key_X : " + Key_X + Environment.NewLine;
                        str += "Key_Y : " + Key_Y + Environment.NewLine;
                        str += "Key_Z : " + Key_Z + Environment.NewLine;
                        str += "Key_F1 : " + Key_F1 + Environment.NewLine;
                        str += "Key_F2 : " + Key_F2 + Environment.NewLine;
                        str += "Key_F3 : " + Key_F3 + Environment.NewLine;
                        str += "Key_F4 : " + Key_F4 + Environment.NewLine;
                        str += "Key_F5 : " + Key_F5 + Environment.NewLine;
                        str += "Key_F6 : " + Key_F6 + Environment.NewLine;
                        str += "Key_F7 : " + Key_F7 + Environment.NewLine;
                        str += "Key_F8 : " + Key_F8 + Environment.NewLine;
                        str += "Key_F9 : " + Key_F9 + Environment.NewLine;
                        str += "Key_F10 : " + Key_F10 + Environment.NewLine;
                        str += "Key_F11 : " + Key_F11 + Environment.NewLine;
                        str += "Key_F12 : " + Key_F12 + Environment.NewLine;
                        str += "Key_F13 : " + Key_F13 + Environment.NewLine;
                        str += "Key_F14 : " + Key_F14 + Environment.NewLine;
                        str += "Key_F15 : " + Key_F15 + Environment.NewLine;
                        str += "Key_F16 : " + Key_F16 + Environment.NewLine;
                        str += "Key_F17 : " + Key_F17 + Environment.NewLine;
                        str += "Key_F18 : " + Key_F18 + Environment.NewLine;
                        str += "Key_F19 : " + Key_F19 + Environment.NewLine;
                        str += "Key_F20 : " + Key_F20 + Environment.NewLine;
                        str += "Key_F21 : " + Key_F21 + Environment.NewLine;
                        str += "Key_F22 : " + Key_F22 + Environment.NewLine;
                        str += "Key_F23 : " + Key_F23 + Environment.NewLine;
                        str += "Key_F24 : " + Key_F24 + Environment.NewLine;
                        str += "Key_NUMPAD0 : " + Key_NUMPAD0 + Environment.NewLine;
                        str += "Key_NUMPAD1 : " + Key_NUMPAD1 + Environment.NewLine;
                        str += "Key_NUMPAD2 : " + Key_NUMPAD2 + Environment.NewLine;
                        str += "Key_NUMPAD3 : " + Key_NUMPAD3 + Environment.NewLine;
                        str += "Key_NUMPAD4 : " + Key_NUMPAD4 + Environment.NewLine;
                        str += "Key_NUMPAD5 : " + Key_NUMPAD5 + Environment.NewLine;
                        str += "Key_NUMPAD6 : " + Key_NUMPAD6 + Environment.NewLine;
                        str += "Key_NUMPAD7 : " + Key_NUMPAD7 + Environment.NewLine;
                        str += "Key_NUMPAD8 : " + Key_NUMPAD8 + Environment.NewLine;
                        str += "Key_NUMPAD9 : " + Key_NUMPAD9 + Environment.NewLine;
                        str += "Key_LWIN : " + Key_LWIN + Environment.NewLine;
                        str += "Key_RWIN : " + Key_RWIN + Environment.NewLine;
                        str += "Key_APPS : " + Key_APPS + Environment.NewLine;
                        str += "Key_SLEEP : " + Key_SLEEP + Environment.NewLine;
                        str += "Key_LBUTTON : " + Key_LBUTTON + Environment.NewLine;
                        str += "Key_RBUTTON : " + Key_RBUTTON + Environment.NewLine;
                        str += "Key_CANCEL : " + Key_CANCEL + Environment.NewLine;
                        str += "Key_MBUTTON : " + Key_MBUTTON + Environment.NewLine;
                        str += "Key_XBUTTON1 : " + Key_XBUTTON1 + Environment.NewLine;
                        str += "Key_XBUTTON2 : " + Key_XBUTTON2 + Environment.NewLine;
                        str += "Key_BACK : " + Key_BACK + Environment.NewLine;
                        str += "Key_Tab : " + Key_Tab + Environment.NewLine;
                        str += "Key_CLEAR : " + Key_CLEAR + Environment.NewLine;
                        str += "Key_Return : " + Key_Return + Environment.NewLine;
                        str += "Key_SHIFT : " + Key_SHIFT + Environment.NewLine;
                        str += "Key_CONTROL : " + Key_CONTROL + Environment.NewLine;
                        str += "Key_MENU : " + Key_MENU + Environment.NewLine;
                        str += "Key_PAUSE : " + Key_PAUSE + Environment.NewLine;
                        str += "Key_CAPITAL : " + Key_CAPITAL + Environment.NewLine;
                        str += "Key_KANA : " + Key_KANA + Environment.NewLine;
                        str += "Key_HANGEUL : " + Key_HANGEUL + Environment.NewLine;
                        str += "Key_HANGUL : " + Key_HANGUL + Environment.NewLine;
                        str += "Key_JUNJA : " + Key_JUNJA + Environment.NewLine;
                        str += "Key_FINAL : " + Key_FINAL + Environment.NewLine;
                        str += "Key_HANJA : " + Key_HANJA + Environment.NewLine;
                        str += "Key_KANJI : " + Key_KANJI + Environment.NewLine;
                        str += "Key_Escape : " + Key_Escape + Environment.NewLine;
                        str += "Key_CONVERT : " + Key_CONVERT + Environment.NewLine;
                        str += "Key_NONCONVERT : " + Key_NONCONVERT + Environment.NewLine;
                        str += "Key_ACCEPT : " + Key_ACCEPT + Environment.NewLine;
                        str += "Key_MODECHANGE : " + Key_MODECHANGE + Environment.NewLine;
                        str += "Key_Space : " + Key_Space + Environment.NewLine;
                        str += "Key_PRIOR : " + Key_PRIOR + Environment.NewLine;
                        str += "Key_NEXT : " + Key_NEXT + Environment.NewLine;
                        str += "Key_END : " + Key_END + Environment.NewLine;
                        str += "Key_HOME : " + Key_HOME + Environment.NewLine;
                        str += "Key_LEFT : " + Key_LEFT + Environment.NewLine;
                        str += "Key_UP : " + Key_UP + Environment.NewLine;
                        str += "Key_RIGHT : " + Key_RIGHT + Environment.NewLine;
                        str += "Key_DOWN : " + Key_DOWN + Environment.NewLine;
                        str += "Key_SELECT : " + Key_SELECT + Environment.NewLine;
                        str += "Key_PRINT : " + Key_PRINT + Environment.NewLine;
                        str += "Key_EXECUTE : " + Key_EXECUTE + Environment.NewLine;
                        str += "Key_SNAPSHOT : " + Key_SNAPSHOT + Environment.NewLine;
                        str += "Key_INSERT : " + Key_INSERT + Environment.NewLine;
                        str += "Key_DELETE : " + Key_DELETE + Environment.NewLine;
                        str += "Key_HELP : " + Key_HELP + Environment.NewLine;
                        str += "Key_APOSTROPHE : " + Key_APOSTROPHE + Environment.NewLine;
                        str += "Key_MULTIPLY : " + Key_MULTIPLY + Environment.NewLine;
                        str += "Key_ADD : " + Key_ADD + Environment.NewLine;
                        str += "Key_SEPARATOR : " + Key_SEPARATOR + Environment.NewLine;
                        str += "Key_SUBTRACT : " + Key_SUBTRACT + Environment.NewLine;
                        str += "Key_DECIMAL : " + Key_DECIMAL + Environment.NewLine;
                        str += "Key_DIVIDE : " + Key_DIVIDE + Environment.NewLine;
                        str += "Key_NUMLOCK : " + Key_NUMLOCK + Environment.NewLine;
                        str += "Key_SCROLL : " + Key_SCROLL + Environment.NewLine;
                        str += "Key_LeftShift : " + Key_LeftShift + Environment.NewLine;
                        str += "Key_RightShift : " + Key_RightShift + Environment.NewLine;
                        str += "Key_LeftControl : " + Key_LeftControl + Environment.NewLine;
                        str += "Key_RightControl : " + Key_RightControl + Environment.NewLine;
                        str += "Key_LMENU : " + Key_LMENU + Environment.NewLine;
                        str += "Key_RMENU : " + Key_RMENU + Environment.NewLine;
                        if (enablemouse)
                        {
                            string str = "CursorX : " + CursorX + Environment.NewLine;
                            str += "CursorY : " + CursorY + Environment.NewLine;
                            str += "MouseX : " + MouseX + Environment.NewLine;
                            str += "MouseY : " + MouseY + Environment.NewLine;
                            str += "MouseZ : " + MouseZ + Environment.NewLine;
                            str += "MouseRightButton : " + MouseRightButton + Environment.NewLine;
                            str += "MouseLeftButton : " + MouseLeftButton + Environment.NewLine;
                            str += "MouseMiddleButton : " + MouseMiddleButton + Environment.NewLine;
                            str += "MouseXButton : " + MouseXButton + Environment.NewLine;
                            str += "MouseButtonX : " + MouseButtonX + Environment.NewLine;
                        }
                    }
                }
                sendinput.Set(KeyboardMouseDriverType, MouseMoveX, MouseMoveY, MouseAbsX, MouseAbsY, MouseDesktopX, MouseDesktopY, SendLeftClick, SendRightClick, SendMiddleClick, SendWheelUp, SendWheelDown, SendLeft, SendRight, SendUp, SendDown, SendLButton, SendRButton, SendCancel, SendMBUTTON, SendXBUTTON1, SendXBUTTON2, SendBack, SendTab, SendClear, SendReturn, SendSHIFT, SendCONTROL, SendMENU, SendPAUSE, SendCAPITAL, SendKANA, SendHANGEUL, SendHANGUL, SendJUNJA, SendFINAL, SendHANJA, SendKANJI, SendEscape, SendCONVERT, SendNONCONVERT, SendACCEPT, SendMODECHANGE, SendSpace, SendPRIOR, SendNEXT, SendEND, SendHOME, SendLEFT, SendUP, SendRIGHT, SendDOWN, SendSELECT, SendPRINT, SendEXECUTE, SendSNAPSHOT, SendINSERT, SendDELETE, SendHELP, SendAPOSTROPHE, Send0, Send1, Send2, Send3, Send4, Send5, Send6, Send7, Send8, Send9, SendA, SendB, SendC, SendD, SendE, SendF, SendG, SendH, SendI, SendJ, SendK, SendL, SendM, SendN, SendO, SendP, SendQ, SendR, SendS, SendT, SendU, SendV, SendW, SendX, SendY, SendZ, SendLWIN, SendRWIN, SendAPPS, SendSLEEP, SendNUMPAD0, SendNUMPAD1, SendNUMPAD2, SendNUMPAD3, SendNUMPAD4, SendNUMPAD5, SendNUMPAD6, SendNUMPAD7, SendNUMPAD8, SendNUMPAD9, SendMULTIPLY, SendADD, SendSEPARATOR, SendSUBTRACT, SendDECIMAL, SendDIVIDE, SendF1, SendF2, SendF3, SendF4, SendF5, SendF6, SendF7, SendF8, SendF9, SendF10, SendF11, SendF12, SendF13, SendF14, SendF15, SendF16, SendF17, SendF18, SendF19, SendF20, SendF21, SendF22, SendF23, SendF24, SendNUMLOCK, SendSCROLL, SendLeftShift, SendRightShift, SendLeftControl, SendRightControl, SendLMENU, SendRMENU);
                Thread.Sleep(1);
            }
        }
        private void Init()
        {
            MouseMoveX = 0;
            MouseMoveY = 0;
            MouseDesktopX = 0;
            MouseDesktopY = 0;
            MouseAbsX = 0;
            MouseAbsY = 0;
            SendD = false;
            SendQ = false;
            SendZ = false;
            SendS = false;
            Send8 = false;
            Send7 = false;
            Send9 = false;
            Send6 = false;
            SendB = false;
            Send1 = false;
            Send2 = false;
            Send3 = false;
            Send4 = false;
            SendSpace = false;
            SendLeftShift = false;
            SendE = false;
            SendA = false;
            SendV = false;
            SendEscape = false;
            SendTab = false;
            SendR = false;
            SendF = false;
            SendT = false;
            SendG = false;
            SendY = false;
            SendU = false;
            SendX = false;
            SendC = false;
            SendRightClick = false;
            SendLeftClick = false;
        }
    }
}