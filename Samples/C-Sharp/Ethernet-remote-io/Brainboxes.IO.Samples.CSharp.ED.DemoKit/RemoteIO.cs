using Brainboxes.IO;
using System;
using System.Collections.Generic;
using System.Net.Sockets;
using System.Text;
using System.Linq;
using System.Collections.Concurrent;

namespace BBDemoApp
{
    public class RemoteIO
    {
        int[] numberEncoding =
        {
            0xEE, //0
			0x28, //1
			0xCD, //2
			0x6D, //3
			0x2B, //4
			0x67, //5
			0xE7, //6
			0x2C, //7
			0xEF, //8
			0x6F, //9
			0xAF, //A
			0xE3, //B
			0xC6, //C
			0xE9, //D
			0xC7, //E
			0x87, //F
		};

        string errorCode = "C7";

        private readonly ED527 ed527;
        private readonly ED204 ed204;

        private readonly string ed527ip;
        private readonly string ed204ip;
        private FixedSizedQueue<double> previousCountTimeSpans = new FixedSizedQueue<double>(10);
        private DateTime previousCountTime = DateTime.MinValue;
        private FixedSizedQueue<float> previousAvailability = new FixedSizedQueue<float>(75);

        private System.Threading.Timer availabilityTimer;
        public EDDeviceState state = new EDDeviceState();


        public RemoteIO(string ed527ip = "YOUR_DEVICE_IP", string ed204ip = "YOUR_DEVICE_IP")
        {
            this.ed527ip = ed527ip;
            this.ed204ip = ed204ip;
            ed527 = EDDevice.Create(ed527ip) as ED527; //  new ED527(new TCPConnection(this.ed527ip));
            ed204 = EDDevice.Create(ed204ip) as ED204; // new ED204(new TCPConnection(this.ed204ip));
            for (int i = 0; i < 100; i++)
            {
                previousAvailability.Enqueue(0);
            }
        }

        public void Start()
        {
            try
            {
                ed204.Connect();
                ed527.Connect();
            }
            catch(SocketException e)
            {
                throw new Exception("Cannot connect to ED Device are you connected to the Brainboxes-Demo network?", e);
            }
            _configureHandlers();
            state.currentNumber = 0;
            
            //_incrementCount();
        }

        private void _configureHandlers()
        {
            //when green button pressed put the fan on
            ed204.Inputs[0].IOLineRisingEdge += (l, d, t) => _setFan(true);
            //when red button pressed turn the fan off
            ed204.Inputs[1].IOLineRisingEdge += (l, d, t) => _setFan(false);
            //when proximity sensor gos from high to low increment the LCD count
            ed204.Inputs[2].IOLineFallingEdge += (l, d, t) => _incrementCount();
            //if the slider is moved set or remove the error state
            ed204.Inputs[3].IOLineChanged += (l, d, t) => _setSliderRight(l.Value == 1);

            //on any changes to the Ed-527 call function
            ed527.IOLinesChanged += _monitorCurrentState;
            availabilityTimer = new System.Threading.Timer(updateAvailability, null, 1000, 1000);

        }
        private void updateAvailability(object b)
        {
            previousAvailability.Enqueue(state.isFanOn ? 1.0f : 0.0f);
            state.rollingAvailability = previousAvailability.queue.Average() * 100;
            this.OnChange(state);
        }
        private void _monitorCurrentState(List<IOLine> lines, EDDevice device)
        {
            state.absoluteCount = ed204.IOLines[2].Count;

          
            int outputStates = device.IOLines.Select(l => (l.Value) << l.IONumber).Sum();
            int currentLCDNumber = 0xef & outputStates; //ef represents the 7 LEDs on the display
            Console.WriteLine("Output states = " + outputStates + " current LCD number = " + currentLCDNumber);
            int index = Array.IndexOf(numberEncoding, currentLCDNumber);
            Console.WriteLine(index+" = "+currentLCDNumber.ToString("X"));

            state.currentNumber = index;
            this.OnChange(state);
        }

        private void _incrementCount()
        {
            state.currentNumber = (state.currentNumber + 1) % 16;
            state.absoluteCount = ed204.IOLines[2].Count;
            DateTime now = DateTime.UtcNow;
            if (previousCountTime != DateTime.MinValue)
            {
                TimeSpan ts = (now - previousCountTime);
                if (ts < TimeSpan.FromMinutes(5)) // ignore anything over 5 mins
                {
                    this.previousCountTimeSpans.Enqueue(ts.Ticks);
                    state.rollingTactTime = new DateTime(Convert.ToInt64(previousCountTimeSpans.queue.Average()));
                }
            }
            previousCountTime = now;
            int outputState = numberEncoding[state.currentNumber];
            Console.WriteLine("Current number is " + state.currentNumber);
            if (state.isSliderRight)
            {
                outputState |= 1 << 4; //if the slider is right also set the light on
            }
            ed527.Protocol.SetAllDigitalOutputLineStates(outputState, 8);
            this.OnChange(state);
        }

        private void _setFan(bool toOn)
        {
            ed527.Outputs[9].Value = toOn ? 1 : 0;
            state.isFanOn = toOn;
            this.OnChange(state);
        }


        private void _setSliderRight(bool toRight)
        {
            if (!ed527.IsConnected) return;

            state.isSliderRight = toRight;
            ed527.IOLines[4].Value = toRight ? 1 : 0;
            this.OnChange(state);
        }

        private void _setBeeping(bool toOn)
        {
            if (toOn)
            {
                state.isBeeping = true;
                this.OnChange(state);
                //beep momentarily
                ed527.Outputs[8].Value = 1;
                ed527.Outputs[8].Value = 0;
                state.isBeeping = false;

                this.OnChange(state);
            }
        }

        public void Stop()
        {
            ed527.Disconnect();
            ed204.Disconnect();
        }

        public void UpdateState(EDDeviceState newState)
        {
            if(newState.Equals(this.state))
            {
                return;
            }
            Console.WriteLine("State changed:");
            //make sure it is within bounds
            newState.currentNumber = (newState.currentNumber) % 16;

            Console.WriteLine(this.state.WhatHasChanged(newState));

            if(newState.currentNumber != state.currentNumber)
            {
                _incrementCount();
            }
            if(newState.isFanOn != state.isFanOn)
            {
                _setFan(newState.isFanOn);
            }
            if(newState.isBeeping != state.isBeeping)
            {
                _setBeeping(newState.isBeeping);
            }
        }

        public event Action<EDDeviceState> OnChange;
    }

    public class EDDeviceState
    {
        //ED-527
        public int currentNumber = 0; //DOUT0 - DOUT7
        public bool isFanOn = false; //DOUT8
        public bool isBeeping = false; //DOUT9

        //ED-204
        public bool isSliderRight = false; //DIN3
        public float rollingAvailability = 0;
        public DateTime rollingTactTime = DateTime.MinValue;
        public int absoluteCount = 0;

        public override bool Equals(object obOther)
        {
            if (null == obOther)
                return false;
            if (object.ReferenceEquals(this, obOther))
                return true;
            EDDeviceState other = obOther as EDDeviceState;
            if (other == null)
                return false;
            return this.currentNumber == other.currentNumber &&
                this.isFanOn == other.isFanOn &&
                this.isBeeping == other.isBeeping &&
                this.isSliderRight == other.isSliderRight &&
                this.absoluteCount == other.absoluteCount; 
        }

        public string WhatHasChanged(EDDeviceState that)
        {
            StringBuilder sb = new StringBuilder();
            if (this.currentNumber != that.currentNumber)
            {
                sb.AppendFormat("\tcurrentNumber has changed from {0} to {1}\n", this.currentNumber, that.currentNumber);
            }
            if (this.isFanOn != that.isFanOn)
            {
                sb.AppendFormat("\tisFanOn has changed from {0} to {1}\n", this.isFanOn, that.isFanOn);
            }
            if (this.isBeeping != that.isBeeping)
            {
                sb.AppendFormat("\tisBeeping has changed from {0} to {1}\n", this.isBeeping, that.isBeeping);
            }
            if (this.isSliderRight != that.isSliderRight)
            {
                sb.AppendFormat("\tisSliderRight has changed from {0} to {1}\n", this.isSliderRight, that.isSliderRight);
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
