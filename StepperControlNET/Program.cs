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
    class Program
    {
        static void Main(string[] args)
        {
            bool Done = false;
            StepperControl sc = new StepperControl("40834774", (short)1);
            decimal position = 0m;
            decimal velocity = 0m;
            string rl; //console readline
            sc.connect();
            Console.Title = "Stepper Control";
            
            while (!Done)
            {
                Console.WriteLine("Input Command [Pos (deg), Vel (deg/s)]: ");
                rl = Console.ReadLine();
                if (rl.ToLower().Contains('q'))
                {
                    Console.WriteLine("quitting application...");
                    Done = true; break;
                }
                if (rl.ToLower().Contains('h'))
                {
                    StepperControl.Home_Method2(sc.channel);
                    bool homed = sc.channel.Status.IsHomed;
                }
                else
                {
                    string[] split = rl.Split(',');
                    if (split.Length == 2)
                    {
                        position = decimal.Parse(split[0]);
                        velocity = decimal.Parse(split[1]);
                        Console.WriteLine("echo:  position = {0} deg, velocity {1} deg/s", position, velocity);
                        if (position != 0)
                        {
                            // update velocity if required using real world methods    
                            if (velocity != 0)
                            {
                                VelocityParameters velPars = sc.channel.GetVelocityParams();
                                velPars.MaxVelocity = velocity;
                                sc.channel.SetVelocityParams(velPars);
                            }
                            StepperControl.Move_Method2(sc.channel, position);
                            Decimal newPos = sc.channel.Position;
                            Console.WriteLine("Device Moved to {0}", newPos);
                        }
                    }
                }
                
                
            }
            sc.disconnect();
        }

        
    }
}