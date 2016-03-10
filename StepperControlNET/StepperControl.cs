using System;
using System.Collections.Generic;
using Thorlabs.MotionControl.Benchtop.StepperMotorCLI;
using Thorlabs.MotionControl.DeviceManagerCLI;
using Thorlabs.MotionControl.GenericMotorCLI;
using Thorlabs.MotionControl.GenericMotorCLI.ControlParameters;
using Thorlabs.MotionControl.GenericMotorCLI.AdvancedMotor;
using Thorlabs.MotionControl.GenericMotorCLI.Settings;
using System.Threading;
using System.Linq;

namespace StepperControlNET
{
    class StepperControl
    {
        #region Properties
        private string _serialNo;
        private static bool _taskComplete;
        private static ulong _taskID;
        private short channelno;
        private BenchtopStepperMotor _device;
        private StepperMotorChannel _channel;
        private MotorConfiguration _motorSettings;
        private ThorlabsBenchtopStepperMotorSettings _currentDeviceSettings;
        private DeviceInfo _deviceInfo;


        public string serialNo
        {
            get { return _serialNo; }
            set { _serialNo = value; }
        }
        public BenchtopStepperMotor device
        {
            get { return _device; }
            set { _device = value; }
        }
        public StepperMotorChannel channel
        {
            get { return _channel; }
            set { _channel = value; }
        }
        public MotorConfiguration motorSettings
        {
            get { return _motorSettings; }
            set { _motorSettings = value; }
        }
        #endregion

        #region Constructor
        public StepperControl(string serialnumber, short channelval)
        {
            DeviceManagerCLI.BuildDeviceList();
            List<string> serialNumbers = DeviceManagerCLI.GetDeviceList(BenchtopStepperMotor.DevicePrefix40);
            if (serialNumbers.Contains(serialnumber))
            {
                serialNo = serialnumber;
                channelno = channelval;
                device = BenchtopStepperMotor.CreateBenchtopStepperMotor(serialNo);
            }
            else
            {
                // the requested serial number is not a BSC103 or is not connected            
                Console.WriteLine("{0} is not a valid serial number", serialNo);
                Console.ReadKey();
                return;
            }
        }
        #endregion

        #region Methods
        public void connect()
        {
            device.Connect(serialNo);
            channel = device.GetChannel(channelno);
            if (!channel.IsSettingsInitialized())
            {
                try
                {
                    channel.WaitForSettingsInitialized(5000);
                }
                catch (Exception)
                {
                    Console.WriteLine("Settings failed to initialize");
                }
            }
            channel.StartPolling(250);
            motorSettings = channel.GetMotorConfiguration(serialNo);
            _currentDeviceSettings = channel.MotorDeviceSettings as ThorlabsBenchtopStepperMotorSettings;
            _deviceInfo = channel.GetDeviceInfo();
            Console.WriteLine("Device {0} = {1}", _deviceInfo.SerialNumber, _deviceInfo.Name);
        }

        public void disconnect()
        {
            channel.StopPolling();
            device.Disconnect();
        }

        public void Home_Method1(IGenericAdvancedMotor device)
        {
            try
            {
                Console.WriteLine("Homing device");
                device.Home(60000);
            }
            catch (Exception)
            {
                Console.WriteLine("Failed to home device");
                Console.ReadKey();
                return;
            }
            Console.WriteLine("Device Homed");
        }

        public static void Move_Method1(IGenericAdvancedMotor device, decimal position)
        {
            try
            {
                Console.WriteLine("Moving Device to {0}", position);
                device.MoveTo(position, 60000);
            }
            catch (Exception)
            {
                Console.WriteLine("Failed to move to position");
                Console.ReadKey();
                return;
            }
            Console.WriteLine("Device Moved");
        }

        
        public static void CommandCompleteFunction(ulong taskID)
        {
            if ((_taskID > 0) && (_taskID == taskID))
            {
                _taskComplete = true;
            }
        }

        public static void Home_Method2(IGenericAdvancedMotor device)
        {
            Console.WriteLine("Homing device");
            _taskComplete = false;
            _taskID = device.Home(CommandCompleteFunction);
            while (!_taskComplete)
            {
                Thread.Sleep(500);
                StatusBase status = device.Status;
                Console.WriteLine("Device Homing {0}", status.Position);
                // will need some timeout functionality;     
            }
            Console.WriteLine("Device Homed");
        }

        public static void Move_Method2(IGenericAdvancedMotor device, decimal position)
        {
            Console.WriteLine("Moving Device to {0}", position);
            _taskComplete = false;
            _taskID = device.MoveTo(position, CommandCompleteFunction);
            while (!_taskComplete)
            {
                Thread.Sleep(500);
                StatusBase status = device.Status;
                Console.WriteLine("Device Moving {0}", status.Position);
                // will need some timeout functionality;           
            }
            Console.WriteLine("Device Moved");
        }
        #endregion
    }
}
