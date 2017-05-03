using System;
using System.Linq;
using System.Diagnostics.Contracts;
using System.Collections.Generic;
using System.Drawing;
using System.Text;
using System.Threading;
using MyoSharp;
using MyoSharp.Commands;
using MyoSharp.Communication;
using MyoSharp.Device;
using MyoSharp.Discovery;
using MyoSharp.Exceptions;
using MyoSharp.Math;
using MyoSharp.Poses;

namespace EgmMyo
{
    #region Methods
    class MyoProgram
    {
        #region Static References
        // using YuMi datasheet axis and values, need to check
        public int MaxMyoZ = 1018; // check this is mazimum z range, robot z direction
        public int MinMyoZ = 94;
        public int MaxMyoX = 680;
        public int MinMyoX = 405;
        public int MaxMyoY = 664;
        public int MinMyoY = 451;
        public double MaxMyo1 = 168.5;
        public double MinMyo1 = -168.5;
        public double MaxMyo2 = 43.5;
        public double MinMyo2 = -143.5;
        public double MaxMyo7 = 168.5;
        public double MinMyo7 = -168.5;
        public double MaxMyo3 = 80;
        public double MinMyo3 = -123.5;
        public double MaxMyo4 = 138;
        public double MinMyo4 = -290;
        public double MaxMyo5 = 138;
        public double MinMyo5 = -88;
        public double MaxMyo6 = 229;
        public double MinMyo6 = -229;
        public const int NUMBER_OF_SENSORS = 8;
        #endregion
        Sensor abbSensor = null;
        OrientationDataEventArgs myoOrient = null;
        static EventWaitHandle _waitHandle = new AutoResetEvent(false);
        public MyoProgram(Sensor abbSensor)
        {
            this.abbSensor = abbSensor;
            this.abbSensor.Start();
            StartMyo2();
        }

        private void StartMyo2()
        {
            Console.WriteLine("1111111111111111111111111111111111111111111111111111111111111");
            bool MyoConnected = false;
            // create a hub that will manage Myo devices for us
            using (var channel = Channel.Create(
                ChannelDriver.Create(ChannelBridge.Create(),
                MyoErrorHandlerDriver.Create(MyoErrorHandlerBridge.Create()))))
            using (var hub = Hub.Create(channel))
            {
                // listen for when the Myo connects
                hub.MyoConnected += (sender, e) =>
                {
                    Console.WriteLine("Myo {0} has connected!", e.Myo.Handle);
                    e.Myo.Vibrate(VibrationType.Short);
                    _waitHandle.Set();
                    e.Myo.OrientationDataAcquired += Myo_OrientationDataAcquired;
                };

                // listen for when the Myo disconnects
                hub.MyoDisconnected += (sender, e) =>
                {
                    Console.WriteLine("Oh no! It looks like {0} arm Myo has disconnected!", e.Myo.Arm);
                    e.Myo.OrientationDataAcquired -= Myo_OrientationDataAcquired;
                };

                Console.WriteLine("2222222222222222222222222222222222222222222222222222222222222");

                // start listening for Myo data
                channel.StartListening();

                ConsoleRunner.UserInputHub(hub);
                _waitHandle.WaitOne();
                abbSensor.Start();
            }
        }
        #endregion
        #region Event Handlers
        public void Myo_OrientationDataAcquired(object sender, OrientationDataEventArgs e)
        {
            var multiplier = (float)(10);
            var roll = (float)(e.Roll* multiplier);
            var pitch = (float)(e.Pitch* multiplier);
            var yaw = (float)(e.Yaw* multiplier);

            abbSensor.axis1 = roll;
            abbSensor.axis2 = pitch;
            abbSensor.axis3 = yaw;

            
        }
        #endregion
    }
    
    static class ConsoleRunner
    {
        #region Methods
        internal static void UserInputHub(IHub hub)
        {
            string userInput;
            while (string.IsNullOrEmpty((userInput = Console.ReadLine())))
            {
                // uhhhhhh
            }
        }
        #endregion
    }


}