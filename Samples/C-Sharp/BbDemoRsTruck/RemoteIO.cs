using Brainboxes.IO;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using System.Collections.Concurrent;
using System.Threading.Tasks;
using Windows.Data.Json;

namespace BbDemoRsTruck
{
    public class RemoteIO
    {
        string[] numberEncoding =
        {
            "EE", //0
            "28", //1
            "CD", //2
            "6D", //3
            "2B", //4
            "67", //5
            "E7", //6
            "2C", //7
            "EF", //8
            "6F", //9
            "AF", //A
            "E3", //B
            "C6", //C
            "E9", //D
            "C7", //E
            "87", //F
        };

        string errorCode = "C7";

        private readonly ED560 ed560;
        private readonly ED588 ed588;

        private readonly string ed588ip;
        private readonly string ed560ip;
        private FixedSizedQueue<double> previousCountTimeSpans = new FixedSizedQueue<double>(10);
        private DateTime previousCountTime = DateTime.MinValue;
        private FixedSizedQueue<float> previousAvailability = new FixedSizedQueue<float>(75);

        private Timer availabilityTimer;

        public AnalogEDState state = new AnalogEDState();

        private Timer _inAndDemoTimer;


        public RemoteIO(string ed560ip = "YOUR_DEVICE_IP", string ed588ip = "YOUR_DEVICE_IP")
        {
            ed560 = EDDevice.Create(ed560ip) as ED560;
            ed588 = EDDevice.Create(ed588ip) as ED588;
            ed588.Inputs[0].IOLineFallingEdge += (l, d, t) =>
            {
                _setCounter();
            };
            //Fill the availability queue
            for (int i = 0; i < 100; i++)
            {
                previousAvailability.Enqueue(0);
            }
        }
        public void Start()
        {
            try
            {
                ed560.Connect();
                ed588.Connect();
            }
            catch (SocketException e)
            {
                throw new Exception("Cannot connect to ED Device are you connected to the Brainboxes-Demo network?", e);
            }
            _monitorCurrentEd560State(ed560);
            _monitorCurrentEd588State(ed588);
            availabilityTimer = new Timer(updateAvailability, null, 1000, 1000);
        }

        private void _configureHandlers()
        {/*
            //when green button pressed put the fan on
            ed204.Inputs[0].IOLineRisingEdge += (l, d, t) => _setFan(true);
            //when red button pressed turn the fan off
            ed204.Inputs[1].IOLineRisingEdge += (l, d, t) => _setFan(false);
            //when proximity sensor gos from high to low increment the LCD count
            ed204.Inputs[2].IOLineFallingEdge += (l, d, t) => _incrementCount();
            //if the slider is moved set or remove the error state
            ed204.Inputs[3].IOLineChanged += (l, d, t) => _setSliderRight(l.Value == 1);
*/
         //on any changes to the Ed-527 call function
         //ed527.IOLinesChanged += _monitorCurrentState;
        }

        private void _monitorCurrentEd588State(EDDevice device)
        {
            state.counterTrue = device.Inputs[0].Count;
            this.OnChange(state);
        }
        private void _monitorCurrentEd560State(EDDevice device)
        {
            state.motorSpeed = device.AOutputs[0].AValue;
            this.OnChange(state);
        }


        private void _setMotorOn(bool value)
        {
            if (value)
            {
                ed588.Outputs[0].Value = 0;
                Thread.Sleep(200);
                ed588.Outputs[1].Value = 1;

                Thread.Sleep(1000);
                ed588.Outputs[1].Value = 0;
            }
            else
            {
                ed588.Outputs[1].Value = 0;
                ed588.Outputs[0].Value = 1;
            }
            state.motorOn = value;
            this.OnChange(state);
        }
        private void _setMotorSpeed(double value)
        {
            if (value <= 4.5)
            {
                ed560.AOutputs[0].AValue = value;
                state.motorSpeed = value;
                this.OnChange(state);
            }
        }
        private void _setCounter()
        {
            state.counterTrue = ed588.Inputs[0].Count;
            if (state.counterScreen < 9)
            {
                state.counterScreen = state.counterScreen + 1;
            }
            else
            {
                state.counterScreen = 0;
            }
            this.OnChange(state);
        }
        private void updateAvailability(object b)
        {
            previousAvailability.Enqueue(state.motorOn ? 1.0f : 0.0f);
            state.availability = previousAvailability.queue.Average() * 100;
            this.OnChange(state);
        }

        Random r = new Random();
        float percent = 0;
        public void Stop()
        {
            ed560.Disconnect();
            ed588.Disconnect();
        }

        public void UpdateState(AnalogEDState newState)
        {
            if (newState.Equals(this.state))
            {
                return;
            }
            Debug.WriteLine("State changed:");

            Debug.WriteLine(this.state.WhatHasChanged(newState));

            if (newState.counterTrue != state.counterTrue)
            {
                _setCounter();
            }
            if (newState.motorSpeed != state.motorSpeed)
            {
                _setMotorSpeed(newState.motorSpeed);
            }
            if (newState.motorOn != state.motorOn)
            {
                _setMotorOn(newState.motorOn);
            }
            if (newState.counterScreen != state.counterScreen)
            {
                state.counterScreen = newState.counterScreen;
            }
            this.OnChange(state);
        }


        public event Action<AnalogEDState> OnChange;
    }
    public class AnalogEDState
    {
        public double motorSpeed = 0; //AOut0
        public int counterScreen = 0;
        public int counterTrue = 0; //DIn0
        public bool motorOn = false; //DOut0  
        public float availability = 0;
        public DateTime rollingTactTime = DateTime.MinValue;

        public override bool Equals(object obOther)
        {
            if (null == obOther)
                return false;
            if (object.ReferenceEquals(this, obOther))
                return true;
            AnalogEDState other = obOther as AnalogEDState;
            if (other == null)
                return false;
            return this.counterTrue == other.counterTrue &&
                this.motorOn == other.motorOn &&
                this.counterScreen == other.counterScreen &&
                this.motorSpeed == other.motorSpeed;
        }
        public string WhatHasChanged(AnalogEDState that)
        {
            StringBuilder sb = new StringBuilder();
            if (this.counterTrue != that.counterTrue)
            {
                sb.AppendFormat("\tcounter2 has changed from {0:x} to {1:x}\n", this.counterTrue.ToString(), that.counterTrue.ToString());
            }
            if (this.motorOn != that.motorOn)
            {
                sb.AppendFormat("\tmotorOn has changed from {0:x} to {1:x}\n", this.motorOn.ToString(), that.motorOn.ToString());
            }
            if (this.counterScreen != that.counterScreen)
            {
                sb.AppendFormat("\tcounter1 has changed from {0:x} to {1:x}\n", this.counterScreen.ToString(), that.counterScreen.ToString());
            }
            if (this.motorSpeed != that.motorSpeed)
            {
                sb.AppendFormat("\tmotorSpeed has changed from {0:x} to {1:x}\n", this.motorSpeed.ToString(), that.motorSpeed.ToString());
            }
            if (this.availability != that.availability)
            {
                sb.AppendFormat("\trollingAvailability has changed from {0} to {1}\n", this.availability.ToString(), that.availability.ToString());
            }
            return sb.ToString();

        }
    }
    public class FixedSizedQueue<T>
    {
        private readonly object privateLockObject = new object();
        public readonly ConcurrentQueue<T> queue = new ConcurrentQueue<T>();
        public int Size { get; private set; }
        public FixedSizedQueue(int size)
        {
            Size = size;
        }
        public void Enqueue(T obj)
        {
            queue.Enqueue(obj);
            lock (privateLockObject)
            {
                while (queue.Count > Size)
                {
                    T outObj;
                    queue.TryDequeue(out outObj);
                }
            }
        }
    }
}

