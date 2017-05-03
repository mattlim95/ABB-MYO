using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Diagnostics.Contracts;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using System.Net;
using System.Net.Sockets;
using abb.egm;
using EgmMyo;

namespace EgmMyo {
    class Program
    { // listen on an assigned port for inbound messages
        public static int IpPortNumber = 6510; //6510
        public static bool exitProg = false;
        static void Main()
        {
            Sensor s = new Sensor();
            MyoProgram m = new MyoProgram(s);
        }
    }

    public class Sensor
    {
        private Thread _sensorThread = null;
        private UdpClient _udpServer = null;
        private bool _exitThread = false;
        private uint _seqNumber = 0;
        private ManualResetEvent _sensorSignal;

        public float axis1 { get; set; }
        public float axis2 { get; set; }
        public float axis3 { get; set; }
        public void SensorThread()
        {
            try
            {   // create an udp client and listen on any address and the port IpPortNumber
                // allow sockets to be reused for local network transmissions
                // needed for simulations 
                var remoteEp = new IPEndPoint(IPAddress.Any, Program.IpPortNumber);
                _udpServer = new System.Net.Sockets.UdpClient();
                _udpServer.ExclusiveAddressUse = false;
                _udpServer.Client.SetSocketOption(SocketOptionLevel.Socket, SocketOptionName.ReuseAddress, true);
                _udpServer.Client.Bind(remoteEp);

                while (_exitThread == false)
                {
                    // Get message from robot
                    var data = _udpServer.Receive(ref remoteEp);
                    if (data != null)
                    {
                        // de-serialize inbound message from robot using Google Protocol Buffer
                        EgmRobot robot = EgmRobot.CreateBuilder().MergeFrom(data).Build();

                        // display inbound message
                        DisplayInboundMessage(robot);

                        // create a new outbound sensor message
                        EgmSensor.Builder sensor = EgmSensor.CreateBuilder();
                        CreateSensorMessage(sensor);

                        using (MemoryStream memoryStream = new MemoryStream())
                        {
                            EgmSensor sensorMessage = sensor.Build();
                            sensorMessage.WriteTo(memoryStream);

                            // send the udp message to the robot
                            int bytesSent = _udpServer.Send(memoryStream.ToArray(), (int)memoryStream.Length, remoteEp);
                            if (bytesSent < 0)
                            { Console.WriteLine("Error send to robot"); }
                        }


                    }
                }
            }
            catch (Exception e)
            {
                Console.WriteLine("Error in receive Sockets : " + e.Message); Console.WriteLine(e.Data);
                Console.WriteLine(e.HResult); Console.WriteLine(e.Source); Console.WriteLine(e.StackTrace);
            }

        }
        // Display message from robot
        void DisplayInboundMessage(EgmRobot robot)
        {
            if (robot.HasHeader && robot.Header.HasSeqno && robot.Header.HasTm)
            {
                Console.WriteLine("Seq={0} tm={1}", robot.Header.Seqno.ToString(), robot.Header.Tm.ToString());
            }
            else
            {
                Console.WriteLine("No header in robot message");
            }
        }
        // Create a sensor message to send to the robot
        void CreateSensorMessage(EgmSensor.Builder sensor)
        {
            // create a header
            EgmHeader.Builder hdr = new EgmHeader.Builder();
            hdr.SetSeqno(_seqNumber++).SetTm((uint)DateTime.Now.Ticks).SetMtype(EgmHeader.Types.MessageType.MSGTYPE_CORRECTION);
            // Timestamp in milliseconds , sent by sensor, MSGTYPE_DATA if sent from robot controller
            sensor.SetHeader(hdr);
            // create some sensor data
            EgmPlanned.Builder planned = new EgmPlanned.Builder();
            EgmPose.Builder pos = new EgmPose.Builder();
            EgmQuaternion.Builder pq = new EgmQuaternion.Builder();
            EgmCartesian.Builder pc = new EgmCartesian.Builder();
            EgmJoints.Builder pj = new EgmJoints.Builder();
            pj.Build();
            // use this to set systematic changes to coordinates
            pj.AddJoints(1).AddJoints(2).AddJoints(3);
            pj.SetJoints(0, this.axis1);
            pj.SetJoints(1, this.axis2);
            pj.SetJoints(2, this.axis3);
            planned.SetJoints(pj); // bind joints object to planned
            sensor.SetPlanned(planned); // bind planned to sensor obj
                                        // display sensor data
            DisplaySensorMessage(this.axis1, this.axis2, this.axis3);
            return;
        }
        void DisplaySensorMessage(float roll, float pitch, float yaw)
        {
            Console.WriteLine("Roll={0} Pitch={1} Yaw={2}", roll, pitch, yaw);

        }
        // Start a thread to listen on inbound messages
        public void Start()
        {
            _sensorThread = new Thread(new ThreadStart(SensorThread));
            _sensorThread.Start();
        }
        // Stop and exit thread
        public void Stop()
        {
            _exitThread = true;
            _sensorThread.Abort();
        }
        // Suspend thread
    }   

}
