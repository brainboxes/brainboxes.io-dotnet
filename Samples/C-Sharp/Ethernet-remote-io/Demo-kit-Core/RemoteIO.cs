using Brainboxes.IO;
using System;
using System.Collections.Generic;
using System.Net.Sockets;
using System.Text;
using System.Linq;

namespace Demo_kit_Core
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

        private readonly ED527 ed527;
        private readonly ED204 ed204;

        private readonly string ed527ip;
        private readonly string ed204ip;

        public EDDeviceState state = new EDDeviceState();


        public RemoteIO(string ed527ip = "YOUR_DEVICE_IP", string ed204ip = "YOUR_DEVICE_IP")
        {
            this.ed527ip = ed527ip;
            this.ed204ip = ed204ip;
            ed527 = EDDevice.Create(ed527ip) as ED527; //  new ED527(new TCPConnection(this.ed527ip));
            ed204 = EDDevice.Create(ed204ip) as ED204; // new ED204(new TCPConnection(this.ed204ip));
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
            state.currentNumber = -1;
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
        }

        private void _monitorCurrentState(List<IOLine> lines, EDDevice device)
        {
            int outputStates = device.IOLines.Select(l => (l.Value) << l.IONumber).Sum();
            int currentLCDNumber = 0xef & outputStates; //ef represents the 7 LEDs on the display

            int index = Array.IndexOf(numberEncoding, currentLCDNumber.ToString("X"));
            Console.WriteLine(index+" = "+currentLCDNumber.ToString("X"));
            state.currentNumber = index;
            this.OnChange(state);
        }

        private void _incrementCount()
        {
            if (state.isSliderRight) return; //we're in an error state
            state.currentNumber = (state.currentNumber + 1) % 10;
            string command = "#0100" + numberEncoding[state.currentNumber];
            ed527.SendCommand(command);
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
            state.isSliderRight = toRight;
            if (state.isSliderRight)
            {
                string command = "#0100" + errorCode;
                ed527.SendCommand(command);
                this.OnChange(state);
            }
            else
            {
                state.currentNumber--; //set the number back
                _incrementCount();
            }
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
            newState.currentNumber = (newState.currentNumber) % 10;

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
                this.isSliderRight == other.isSliderRight;
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
}
