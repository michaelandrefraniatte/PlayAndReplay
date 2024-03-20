using SharpDX.XInput;
using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using System.Threading.Tasks;
using Xinputs;

namespace XInputsAPI
{
    public class XInput
    {
        [DllImport("winmm.dll", EntryPoint = "timeBeginPeriod")]
        private static extern uint TimeBeginPeriod(uint ms);
        [DllImport("winmm.dll", EntryPoint = "timeEndPeriod")]
        private static extern uint TimeEndPeriod(uint ms);
        [DllImport("ntdll.dll", EntryPoint = "NtSetTimerResolution")]
        private static extern void NtSetTimerResolution(uint DesiredResolution, bool SetResolution, ref uint CurrentResolution);
        private static uint CurrentResolution = 0;
        private bool running, formvisible;
        private int number, inc;
        private static List<Controller> gamepads = new List<Controller>();
        private Controller gp;
        private Form1 form1 = new Form1();
        public XInput()
        {
            TimeBeginPeriod(1);
            NtSetTimerResolution(1, true, ref CurrentResolution);
            running = true;
        }
        public void ViewData()
        {
            if (!form1.Visible)
            {
                formvisible = true;
                form1.SetVisible();
            }
        }
        public void Close()
        {
            running = false;
        }
        private void taskD()
        {
            for (; ; )
            {
                if (!running)
                    break;
                ProcessStateLogic();
                System.Threading.Thread.Sleep(1);
                if (formvisible)
                {
                    string str = "ControllerButtonAPressed : " + ControllerButtonAPressed + Environment.NewLine;
                    str += "ControllerButtonBPressed : " + ControllerButtonBPressed + Environment.NewLine;
                    str += "ControllerButtonXPressed : " + ControllerButtonXPressed + Environment.NewLine;
                    str += "ControllerButtonYPressed : " + ControllerButtonYPressed + Environment.NewLine;
                    str += "ControllerButtonStartPressed : " + ControllerButtonStartPressed + Environment.NewLine;
                    str += "ControllerButtonBackPressed : " + ControllerButtonBackPressed + Environment.NewLine;
                    str += "ControllerButtonDownPressed : " + ControllerButtonDownPressed + Environment.NewLine;
                    str += "ControllerButtonUpPressed : " + ControllerButtonUpPressed + Environment.NewLine;
                    str += "ControllerButtonLeftPressed : " + ControllerButtonLeftPressed + Environment.NewLine;
                    str += "ControllerButtonRightPressed : " + ControllerButtonRightPressed + Environment.NewLine;
                    str += "ControllerButtonShoulderLeftPressed : " + ControllerButtonShoulderLeftPressed + Environment.NewLine;
                    str += "ControllerButtonShoulderRightPressed : " + ControllerButtonShoulderRightPressed + Environment.NewLine;
                    str += "ControllerThumbpadLeftPressed : " + ControllerThumbpadLeftPressed + Environment.NewLine;
                    str += "ControllerThumbpadRightPressed : " + ControllerThumbpadRightPressed + Environment.NewLine;
                    str += "ControllerTriggerLeftPosition : " + ControllerTriggerLeftPosition + Environment.NewLine;
                    str += "ControllerTriggerRightPosition : " + ControllerTriggerRightPosition + Environment.NewLine;
                    str += "ControllerThumbLeftX : " + ControllerThumbLeftX + Environment.NewLine;
                    str += "ControllerThumbLeftY : " + ControllerThumbLeftY + Environment.NewLine;
                    str += "ControllerThumbRightX : " + ControllerThumbRightX + Environment.NewLine;
                    str += "ControllerThumbRightY : " + ControllerThumbRightY + Environment.NewLine;
                    str += Environment.NewLine;
                    form1.SetLabel1(str);
                }
            }
        }
        public void BeginPolling()
        {
            Task.Run(() => taskD());
        }
        private Controller[] controller = new Controller[] { null, null, null, null };
        private SharpDX.XInput.State xistate;
        private int xinum = 0;
        public bool ControllerButtonAPressed;
        public bool ControllerButtonBPressed;
        public bool ControllerButtonXPressed;
        public bool ControllerButtonYPressed;
        public bool ControllerButtonStartPressed;
        public bool ControllerButtonBackPressed;
        public bool ControllerButtonDownPressed;
        public bool ControllerButtonUpPressed;
        public bool ControllerButtonLeftPressed;
        public bool ControllerButtonRightPressed;
        public bool ControllerButtonShoulderLeftPressed;
        public bool ControllerButtonShoulderRightPressed;
        public bool ControllerThumbpadLeftPressed;
        public bool ControllerThumbpadRightPressed;
        public double ControllerTriggerLeftPosition;
        public double ControllerTriggerRightPosition;
        public double ControllerThumbLeftX;
        public double ControllerThumbLeftY;
        public double ControllerThumbRightX;
        public double ControllerThumbRightY;
        public bool Scan(int number = 0)
        {
            this.number = number;
            inc = number < 2 ? 0 : number - 1;
            if (number <= 1)
            {
                controller = new Controller[] { null, null, null, null };
                xinum = 0;
                var controllers = new[] { new Controller(UserIndex.One), new Controller(UserIndex.Two), new Controller(UserIndex.Three), new Controller(UserIndex.Four) };
                foreach (var selectControler in controllers)
                {
                    if (selectControler.IsConnected)
                    {
                        controller[xinum] = selectControler;
                        gamepads.Add(controller[xinum]);
                        xinum++;
                    }
                }
            }
            if (gamepads.Count == 0)
            {
                return false;
            }
            else
            {
                gp = gamepads[inc];
                return true;
            }
        }
        public void Init()
        {
        }
        private void ProcessStateLogic()
        {
            xistate = gp.GetState();
            if (xistate.Gamepad.Buttons.HasFlag(GamepadButtonFlags.A))
                ControllerButtonAPressed = true;
            else
                ControllerButtonAPressed = false;
            if (xistate.Gamepad.Buttons.HasFlag(GamepadButtonFlags.B))
                ControllerButtonBPressed = true;
            else
                ControllerButtonBPressed = false;
            if (xistate.Gamepad.Buttons.HasFlag(GamepadButtonFlags.X))
                ControllerButtonXPressed = true;
            else
                ControllerButtonXPressed = false;
            if (xistate.Gamepad.Buttons.HasFlag(GamepadButtonFlags.Y))
                ControllerButtonYPressed = true;
            else
                ControllerButtonYPressed = false;
            if (xistate.Gamepad.Buttons.HasFlag(GamepadButtonFlags.Start))
                ControllerButtonStartPressed = true;
            else
                ControllerButtonStartPressed = false;
            if (xistate.Gamepad.Buttons.HasFlag(GamepadButtonFlags.Back))
                ControllerButtonBackPressed = true;
            else
                ControllerButtonBackPressed = false;
            if (xistate.Gamepad.Buttons.HasFlag(GamepadButtonFlags.DPadDown))
                ControllerButtonDownPressed = true;
            else
                ControllerButtonDownPressed = false;
            if (xistate.Gamepad.Buttons.HasFlag(GamepadButtonFlags.DPadUp))
                ControllerButtonUpPressed = true;
            else
                ControllerButtonUpPressed = false;
            if (xistate.Gamepad.Buttons.HasFlag(GamepadButtonFlags.DPadLeft))
                ControllerButtonLeftPressed = true;
            else
                ControllerButtonLeftPressed = false;
            if (xistate.Gamepad.Buttons.HasFlag(GamepadButtonFlags.DPadRight))
                ControllerButtonRightPressed = true;
            else
                ControllerButtonRightPressed = false;
            if (xistate.Gamepad.Buttons.HasFlag(GamepadButtonFlags.LeftShoulder))
                ControllerButtonShoulderLeftPressed = true;
            else
                ControllerButtonShoulderLeftPressed = false;
            if (xistate.Gamepad.Buttons.HasFlag(GamepadButtonFlags.RightShoulder))
                ControllerButtonShoulderRightPressed = true;
            else
                ControllerButtonShoulderRightPressed = false;
            if (xistate.Gamepad.Buttons.HasFlag(GamepadButtonFlags.LeftThumb))
                ControllerThumbpadLeftPressed = true;
            else
                ControllerThumbpadLeftPressed = false;
            if (xistate.Gamepad.Buttons.HasFlag(GamepadButtonFlags.RightThumb))
                ControllerThumbpadRightPressed = true;
            else
                ControllerThumbpadRightPressed = false;
            ControllerTriggerLeftPosition = xistate.Gamepad.LeftTrigger;
            ControllerTriggerRightPosition = xistate.Gamepad.RightTrigger;
            ControllerThumbLeftX = xistate.Gamepad.LeftThumbX;
            ControllerThumbLeftY = xistate.Gamepad.LeftThumbY;
            ControllerThumbRightX = xistate.Gamepad.RightThumbX;
            ControllerThumbRightY = xistate.Gamepad.RightThumbY;
        }
    }
}