﻿using SharpDX.DirectInput;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Forms;

namespace MobiFlight
{
    public class JoystickManager
    {
        public event ButtonEventHandler OnButtonPressed;
        Timer PollTimer = new Timer();
        List<Joystick> joysticks = new List<Joystick>();

        public JoystickManager ()
        {
            PollTimer.Interval = 50;
            PollTimer.Tick += PollTimer_Tick;
        }
        private void PollTimer_Tick(object sender, EventArgs e)
        {
            foreach (MobiFlight.Joystick js in joysticks)
            {
                js?.Update();
            }
        }

        public void Start()
        {
            PollTimer.Start();
        }

        public void Stop()
        {
            PollTimer.Stop();
        }

        public List<MobiFlight.Joystick> GetJoysticks()
        {
            return joysticks;
        }

        public void Connect(IntPtr Handle)
        {
            SharpDX.DirectInput.DirectInput di = new SharpDX.DirectInput.DirectInput();
            joysticks?.Clear();

            List<SharpDX.DirectInput.DeviceInstance> devices = di.GetDevices(DeviceClass.GameControl, DeviceEnumerationFlags.AttachedOnly).ToList();
            //List<SharpDX.DirectInput.DeviceInstance> devices = di.GetDevices(SharpDX.DirectInput.DeviceType.Joystick, DeviceEnumerationFlags.AttachedOnly).ToList();
            //devices.AddRange(di.GetDevices(SharpDX.DirectInput.DeviceType.Gamepad, DeviceEnumerationFlags.AttachedOnly));
            //devices.AddRange(di.GetDevices(SharpDX.DirectInput.DeviceType.Flight, DeviceEnumerationFlags.AttachedOnly));
            foreach (DeviceInstance d in devices)
            {
                Log.Instance.log("Found attached DirectInput Device: " + d.InstanceName + ", Type: " + d.Type.ToString() + ", SubType: " + d.Subtype, LogSeverity.Debug);

                if (!IsSupportedDeviceType(d)) continue;

                MobiFlight.Joystick js = new MobiFlight.Joystick(new SharpDX.DirectInput.Joystick(di, d.InstanceGuid));
                
                if (!HasAxisOrButtons(js)) continue;

                Log.Instance.log("Adding attached Joystick Device: " + d.InstanceName + " Buttons " + js.Capabilities.ButtonCount + ", Axis: " + js.Capabilities.AxeCount, LogSeverity.Debug);
                js.Connect(Handle); 
                joysticks.Add(js);
                js.OnButtonPressed += Js_OnButtonPressed;
                js.OnAxisChanged += Js_OnAxisChanged;
            }
        }

        private bool HasAxisOrButtons(Joystick js)
        {
            return
                js.Capabilities.AxeCount > 0 ||
                js.Capabilities.ButtonCount > 0;
        }

        private bool IsSupportedDeviceType(DeviceInstance d)
        {
            return
                d.Type == SharpDX.DirectInput.DeviceType.Joystick ||
                d.Type == SharpDX.DirectInput.DeviceType.Gamepad ||
                d.Type == SharpDX.DirectInput.DeviceType.Flight ||
                d.Type == SharpDX.DirectInput.DeviceType.Supplemental ||
                d.Type == SharpDX.DirectInput.DeviceType.FirstPerson;
        }

        private void Js_OnAxisChanged(object sender, InputEventArgs e)
        {
            OnButtonPressed?.Invoke(sender, e);
        }

        private void Js_OnButtonPressed(object sender, InputEventArgs e)
        {
            OnButtonPressed?.Invoke(sender, e);
        }

        internal Joystick GetJoystickBySerial(string serial)
        {
            Joystick result = null;

            result = joysticks.Find((Joystick js) => { return (js.Serial == serial); });

            return result;
        }
    }
}
