using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Diagnostics;
using System.Drawing;
using System.IO;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;
using controllers;
using KeyboardHooksAPI;
using OpenWithSingleInstance;
using Valuechanges;
using XInputsAPI;

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
        private static bool play, replay, running, enablesticks;
        private static Stopwatch watchplay = new Stopwatch(), watchreplay = new Stopwatch(), watchrepeat = new Stopwatch();
        private static double elapseplay, elapsereplay, elapserepeat;
        private static bool Controller_Send_back, Controller_Send_start, Controller_Send_A, Controller_Send_B, Controller_Send_X, Controller_Send_Y, Controller_Send_up, Controller_Send_left, Controller_Send_down, Controller_Send_right, Controller_Send_leftstick, Controller_Send_rightstick, Controller_Send_leftbumper, Controller_Send_rightbumper, Controller_Send_lefttrigger, Controller_Send_righttrigger, Controller_Send_xbox;
        private static double Controller_Send_leftstickx, Controller_Send_leftsticky, Controller_Send_rightstickx, Controller_Send_rightsticky, Controller_Send_lefttriggerposition, Controller_Send_righttriggerposition;
        private int linecount = 0;
        private KeyboardHooks kh = new KeyboardHooks();
        private XBoxController XBC = new XBoxController();
        private XInput xi = new XInput();
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
                    OpenOption(Path.GetDirectoryName(filePath) + "/options/" + Path.GetFileName(filePath) + ".option");
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
                OpenOption(Path.GetDirectoryName(filePath) + "/options/" + Path.GetFileName(filePath) + ".option");
                string txt = File.ReadAllText(filePath, Encoding.UTF8);
                richTextBox1.Text = txt;
                filename = filePath;
                openFilePath = filePath;
                this.Text = filePath;
                fileTextSaved = richTextBox1.Text;
                justSaved = true;
            }
        }
        private void Form1_KeyDown(object sender, KeyEventArgs e)
        {
            OnKeyDown(e.KeyData);
        }
        private void richTextBox1_KeyDown(object sender, KeyEventArgs e)
        {
            OnKeyDown(e.KeyData);
        }
        private void richTextBox2_KeyDown(object sender, KeyEventArgs e)
        {
            OnKeyDown(e.KeyData);
        }
        private void OnKeyDown(Keys keyData)
        {
            if (keyData == Keys.F1)
            {
                const string message = "• Author: Michaël André Franiatte.\n\r\n\r• Contact: michael.franiatte@gmail.com.\n\r\n\r• Publisher: https://github.com/michaelandrefraniatte.\n\r\n\r• Copyrights: All rights reserved, no permissions granted.\n\r\n\r• License: Not open source, not free of charge to use.";
                const string caption = "About";
                MessageBox.Show(message, caption, MessageBoxButtons.OK, MessageBoxIcon.Information);
            }
            if (keyData == Keys.Escape)
            {
                this.Close();
            }
        }
        private void helpToolStripMenuItem_Click(object sender, EventArgs e)
        {
            const string message = "• Play: Use keyboard key shortcut LCtrl + P.\n\r\n\r• Replay: Use keyboard key shortcut LCtrl + R.\n\r\n\r• Stop: Use keyboard key shortcut LCtrl + U.\n\r\n\r• Save: Use keyboard key shortcut LCtrl + S.";
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
            kh.Scan();
            kh.BeginPolling();
            xi.Scan();
            xi.BeginPolling();
            XBC.Connect();
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
                valchanged(2, kh.Key_LeftControl & kh.Key_U);
                if (wd[2] == 1)
                {
                    Stop();
                }
                valchanged(3, kh.Key_LeftControl & kh.Key_S);
                if (wd[3] == 1)
                {
                    Save();
                }
                Thread.Sleep(50);
            }
        }
        private void Save()
        {
            if (!Directory.Exists("Replays"))
            {
                Directory.CreateDirectory("Replays");
            }
            if (!Directory.Exists(@"\Replays\options"))
            {
                Directory.CreateDirectory(@"\Replays\options");
            }
            string name = @"Replay_" + DateTime.Now.ToString().Replace("/", "_").Replace(":", "_").Replace(" ", "_");
            string RecordFileName = Application.StartupPath + @"\Replays\" + name;
            richTextBox1.SaveFile(RecordFileName, RichTextBoxStreamType.RichText);
            string optionFileName = Application.StartupPath + @"\Replays\options\" + name + ".option";
            SaveOption(optionFileName);
        }
        private void SaveOption(string completepath)
        {
            using (StreamWriter createdfile = new StreamWriter(completepath))
            {
                createdfile.WriteLine(enableSticksToolStripMenuItem.Checked);
                createdfile.WriteLine(emptyToolStripMenuItem.Text);
                createdfile.Close();
            }
        }
        private void OpenOption(string completepath)
        {
            using (StreamReader file = new StreamReader(completepath))
            {
                enableSticksToolStripMenuItem.Checked = bool.Parse(file.ReadLine());
                emptyToolStripMenuItem.Text = file.ReadLine();
                file.Close();
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
                        OpenOption(Path.GetDirectoryName(op.FileName) + "/options/" + Path.GetFileName(op.FileName) + ".option");
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
                    OpenOption(Path.GetDirectoryName(op.FileName) + "/options/" + Path.GetFileName(op.FileName) + ".option");
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
                if (!Directory.Exists(Path.GetDirectoryName(openFilePath) + "/options"))
                {
                    Directory.CreateDirectory(Path.GetDirectoryName(openFilePath) + "/options");
                }
                SaveOption(Path.GetDirectoryName(openFilePath) + "/options/" + Path.GetFileName(openFilePath) + ".option");
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
                if (!Directory.Exists(Path.GetDirectoryName(sf.FileName) + "/options"))
                {
                    Directory.CreateDirectory(Path.GetDirectoryName(sf.FileName) + "/options");
                }
                SaveOption(Path.GetDirectoryName(sf.FileName) + "/options/" + Path.GetFileName(sf.FileName) + ".option");
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
            xi.Close();
            XBC.Disconnect();
        }
        private void playToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (!play)
                Play();
            else
                Stop();
        }
        private void replayToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (!replay)
                Replay();
            else
                Stop();
        }
        private void Play()
        {
            if (replay)
            {
                replay = false;
                replayToolStripMenuItem.Text = "Replay";
                Thread.Sleep(100);
                Init();
                watchreplay.Stop();
            }
            if (!play)
            {
                play = true;
                playToolStripMenuItem.Text = "Stop";
                richTextBox1.Clear();
                enablesticks = enableSticksToolStripMenuItem.Checked;
                watchplay = new Stopwatch();
                watchplay.Start();
                Task.Run(() => taskPlay());
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
                enablesticks = enableSticksToolStripMenuItem.Checked;
                watchreplay = new Stopwatch();
                watchreplay.Start();
                Task.Run(() => taskReplay());
            }
        }
        private void Stop()
        {
            if (play)
            {
                play = false;
                playToolStripMenuItem.Text = "Play";
                Thread.Sleep(100);
                watchplay.Stop();
            }
            if (replay)
            {
                replay = false;
                replayToolStripMenuItem.Text = "Replay";
                Thread.Sleep(100);
                Init();
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
                if (enablesticks)
                {
                    ValueChange[0] = xi.ControllerThumbRightX;
                    if (ValueChange._ValueChange[0] != 0)
                        richTextBox1.AppendText(elapseplay + "; ControllerThumbRightX; " + xi.ControllerThumbRightX + "; \r\n");
                    ValueChange[1] = xi.ControllerThumbRightY;
                    if (ValueChange._ValueChange[1] != 0)
                        richTextBox1.AppendText(elapseplay + "; ControllerThumbRightY; " + xi.ControllerThumbRightY + "; \r\n");
                    ValueChange[2] = xi.ControllerThumbLeftX;
                    if (ValueChange._ValueChange[2] != 0)
                        richTextBox1.AppendText(elapseplay + "; ControllerThumbLeftX; " + xi.ControllerThumbLeftX + "; \r\n");
                    ValueChange[3] = xi.ControllerThumbLeftY;
                    if (ValueChange._ValueChange[3] != 0)
                        richTextBox1.AppendText(elapseplay + "; ControllerThumbLeftY; " + xi.ControllerThumbLeftY + "; \r\n");
                }
                ValueChange[4] = xi.ControllerTriggerLeftPosition;
                if (ValueChange._ValueChange[4] != 0)
                    richTextBox1.AppendText(elapseplay + "; ControllerTriggerLeftPosition; " + xi.ControllerTriggerLeftPosition + "; \r\n");
                ValueChange[5] = xi.ControllerTriggerRightPosition;
                if (ValueChange._ValueChange[5] != 0)
                    richTextBox1.AppendText(elapseplay + "; ControllerTriggerRightPosition; " + xi.ControllerTriggerRightPosition + "; \r\n");
                ValueChange[6] = xi.ControllerButtonAPressed ? 1 : -1;
                if (ValueChange._ValueChange[6] != 0)
                    richTextBox1.AppendText(elapseplay + "; ControllerButtonAPressed; " + xi.ControllerButtonAPressed + "; \r\n");
                ValueChange[7] = xi.ControllerButtonBPressed ? 1 : -1;
                if (ValueChange._ValueChange[7] != 0)
                    richTextBox1.AppendText(elapseplay + "; ControllerButtonBPressed; " + xi.ControllerButtonBPressed + "; \r\n");
                ValueChange[8] = xi.ControllerButtonXPressed ? 1 : -1;
                if (ValueChange._ValueChange[8] != 0)
                    richTextBox1.AppendText(elapseplay + "; ControllerButtonXPressed; " + xi.ControllerButtonXPressed + "; \r\n");
                ValueChange[9] = xi.ControllerButtonYPressed ? 1 : -1;
                if (ValueChange._ValueChange[9] != 0)
                    richTextBox1.AppendText(elapseplay + "; ControllerButtonYPressed; " + xi.ControllerButtonYPressed + "; \r\n");
                ValueChange[10] = xi.ControllerButtonShoulderLeftPressed ? 1 : -1;
                if (ValueChange._ValueChange[10] != 0)
                    richTextBox1.AppendText(elapseplay + "; ControllerButtonShoulderLeftPressed; " + xi.ControllerButtonShoulderLeftPressed + "; \r\n");
                ValueChange[11] = xi.ControllerButtonShoulderRightPressed ? 1 : -1;
                if (ValueChange._ValueChange[11] != 0)
                    richTextBox1.AppendText(elapseplay + "; ControllerButtonShoulderRightPressed; " + xi.ControllerButtonShoulderRightPressed + "; \r\n");
                ValueChange[12] = xi.ControllerThumbpadRightPressed ? 1 : -1;
                if (ValueChange._ValueChange[12] != 0)
                    richTextBox1.AppendText(elapseplay + "; ControllerThumbpadRightPressed; " + xi.ControllerThumbpadRightPressed + "; \r\n");
                ValueChange[13] = xi.ControllerThumbpadLeftPressed ? 1 : -1;
                if (ValueChange._ValueChange[13] != 0)
                    richTextBox1.AppendText(elapseplay + "; ControllerThumbpadLeftPressed; " + xi.ControllerThumbpadLeftPressed + "; \r\n");
                ValueChange[14] = xi.ControllerButtonUpPressed ? 1 : -1;
                if (ValueChange._ValueChange[14] != 0)
                    richTextBox1.AppendText(elapseplay + "; ControllerButtonUpPressed; " + xi.ControllerButtonUpPressed + "; \r\n");
                ValueChange[15] = xi.ControllerButtonLeftPressed ? 1 : -1;
                if (ValueChange._ValueChange[15] != 0)
                    richTextBox1.AppendText(elapseplay + "; ControllerButtonLeftPressed; " + xi.ControllerButtonLeftPressed + "; \r\n");
                ValueChange[16] = xi.ControllerButtonDownPressed ? 1 : -1;
                if (ValueChange._ValueChange[16] != 0)
                    richTextBox1.AppendText(elapseplay + "; ControllerButtonDownPressed; " + xi.ControllerButtonDownPressed + "; \r\n");
                ValueChange[17] = xi.ControllerButtonRightPressed ? 1 : -1;
                if (ValueChange._ValueChange[17] != 0)
                    richTextBox1.AppendText(elapseplay + "; ControllerButtonRightPressed; " + xi.ControllerButtonRightPressed + "; \r\n");
                ValueChange[18] = xi.ControllerButtonBackPressed ? 1 : -1;
                if (ValueChange._ValueChange[18] != 0)
                    richTextBox1.AppendText(elapseplay + "; ControllerButtonBackPressed; " + xi.ControllerButtonBackPressed + "; \r\n");
                ValueChange[19] = xi.ControllerButtonStartPressed ? 1 : -1;
                if (ValueChange._ValueChange[19] != 0)
                    richTextBox1.AppendText(elapseplay + "; ControllerButtonStartPressed; " + xi.ControllerButtonStartPressed + "; \r\n");
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
                    string line = richTextBox2.Lines[linecount];
                    string[] data = line.Split(';');
                    double time = Convert.ToSingle(data[0]);
                    if (elapsereplay >= time)
                    {
                        richTextBox2.Select(richTextBox2.GetFirstCharIndexFromLine(linecount), richTextBox2.Lines[linecount].Length);
                        richTextBox2.SelectionColor = Color.Red;
                        linecount++;
                        if (enablesticks)
                        {
                            if (data[1] == " ControllerThumbRightX")
                            {
                                Controller_Send_rightstickx = Convert.ToSingle(data[2]);
                            }
                            if (data[1] == " ControllerThumbRightY")
                            {
                                Controller_Send_rightsticky = Convert.ToSingle(data[2]);
                            }
                            if (data[1] == " ControllerThumbLeftX")
                            {
                                Controller_Send_leftstickx = Convert.ToSingle(data[2]);
                            }
                            if (data[1] == " ControllerThumbLeftY")
                            {
                                Controller_Send_leftsticky = Convert.ToSingle(data[2]);
                            }
                        }
                        if (data[1] == " ControllerTriggerLeftPosition")
                        {
                            Controller_Send_lefttriggerposition = Convert.ToSingle(data[2]);
                        }
                        if (data[1] == " ControllerTriggerRightPosition")
                        {
                            Controller_Send_righttriggerposition = Convert.ToSingle(data[2]);
                        }
                        if (data[1] == " ControllerButtonAPressed")
                        {
                            Controller_Send_A = bool.Parse(data[2]);
                        }
                        if (data[1] == " ControllerButtonBPressed")
                        {
                            Controller_Send_B = bool.Parse(data[2]);
                        }
                        if (data[1] == " ControllerButtonXPressed")
                        {
                            Controller_Send_X = bool.Parse(data[2]);
                        }
                        if (data[1] == " ControllerButtonYPressed")
                        {
                            Controller_Send_Y = bool.Parse(data[2]);
                        }
                        if (data[1] == " ControllerButtonShoulderLeftPressed")
                        {
                            Controller_Send_leftbumper = bool.Parse(data[2]);
                        }
                        if (data[1] == " ControllerButtonShoulderRightPressed")
                        {
                            Controller_Send_rightbumper = bool.Parse(data[2]);
                        }
                        if (data[1] == " ControllerThumbpadRightPressed")
                        {
                            Controller_Send_rightstick = bool.Parse(data[2]);
                        }
                        if (data[1] == " ControllerThumbpadLeftPressed")
                        {
                            Controller_Send_leftstick = bool.Parse(data[2]);
                        }
                        if (data[1] == " ControllerButtonUpPressed")
                        {
                            Controller_Send_up = bool.Parse(data[2]);
                        }
                        if (data[1] == " ControllerButtonLeftPressed")
                        {
                            Controller_Send_left = bool.Parse(data[2]);
                        }
                        if (data[1] == " ControllerButtonDownPressed")
                        {
                            Controller_Send_down = bool.Parse(data[2]);
                        }
                        if (data[1] == " ControllerButtonRightPressed")
                        {
                            Controller_Send_right = bool.Parse(data[2]);
                        }
                        if (data[1] == " ControllerButtonBackPressed")
                        {
                            Controller_Send_back = bool.Parse(data[2]);
                        }
                        if (data[1] == " ControllerButtonStartPressed")
                        {
                            Controller_Send_start = bool.Parse(data[2]);
                        }
                    }
                    XBC.Set(Controller_Send_back, Controller_Send_start, Controller_Send_A, Controller_Send_B, Controller_Send_X, Controller_Send_Y, Controller_Send_up, Controller_Send_left, Controller_Send_down, Controller_Send_right, Controller_Send_leftstick, Controller_Send_rightstick, Controller_Send_leftbumper, Controller_Send_rightbumper, Controller_Send_leftstickx, Controller_Send_leftsticky, Controller_Send_rightstickx, Controller_Send_rightsticky, Controller_Send_lefttriggerposition, Controller_Send_righttriggerposition, Controller_Send_xbox);
                }
                else
                {
                    if (emptyToolStripMenuItem.Text == "empty" | emptyToolStripMenuItem.Text == "")
                    {
                        replay = false;
                        replayToolStripMenuItem.Text = "Replay";
                        Thread.Sleep(100);
                        Init();
                        watchreplay.Stop();
                        break;
                    }
                    else
                    {
                        linecount = 0;
                        Init();
                        watchreplay.Stop();
                        watchrepeat = new Stopwatch();
                        watchrepeat.Start();
                        elapserepeat = 0;
                        while (elapserepeat < (int)(Convert.ToSingle(emptyToolStripMenuItem.Text) * 1000))
                        {
                            if (!replay)
                                break;
                            elapserepeat = (double)watchrepeat.ElapsedTicks / (Stopwatch.Frequency / (1000L * 1000L));
                            Thread.Sleep(100);
                        }
                        watchrepeat.Stop();
                        watchreplay = new Stopwatch();
                        watchreplay.Start();
                    }
                }
                Thread.Sleep(1);
            }
        }
        private void Init()
        {
            Controller_Send_rightstickx = 0;
            Controller_Send_rightsticky = 0;
            Controller_Send_leftstickx = 0;
            Controller_Send_leftsticky = 0;
            Controller_Send_up = false;
            Controller_Send_left = false;
            Controller_Send_down = false;
            Controller_Send_right = false;
            Controller_Send_back = false;
            Controller_Send_start = false;
            Controller_Send_leftstick = false;
            Controller_Send_leftbumper = false;
            Controller_Send_rightbumper = false;
            Controller_Send_A = false;
            Controller_Send_B = false;
            Controller_Send_X = false;
            Controller_Send_Y = false;
            Controller_Send_rightstick = false;
            Controller_Send_lefttriggerposition = 0;
            Controller_Send_righttriggerposition = 0;
            XBC.Set(Controller_Send_back, Controller_Send_start, Controller_Send_A, Controller_Send_B, Controller_Send_X, Controller_Send_Y, Controller_Send_up, Controller_Send_left, Controller_Send_down, Controller_Send_right, Controller_Send_leftstick, Controller_Send_rightstick, Controller_Send_leftbumper, Controller_Send_rightbumper, Controller_Send_leftstickx, Controller_Send_leftsticky, Controller_Send_rightstickx, Controller_Send_rightsticky, Controller_Send_lefttriggerposition, Controller_Send_righttriggerposition, Controller_Send_xbox);
        }
    }
}