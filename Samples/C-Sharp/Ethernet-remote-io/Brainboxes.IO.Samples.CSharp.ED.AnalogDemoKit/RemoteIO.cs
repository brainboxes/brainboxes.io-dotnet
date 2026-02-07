using Brainboxes.IO;
using System;
using System.Collections.Generic;
using System.Net.Sockets;
using System.Text;
using System.Linq;
using System.Diagnostics;

using System.Threading;
using System.Text.RegularExpressions;

namespace BBDemoApp
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
        private readonly ED560 ed560;
        private readonly ED549 ed549;


        private readonly string ed527ip;
        private readonly string ed204ip;
        private readonly string ed560ip;
        private readonly string ed549ip;

        public AnalogEDState state = new AnalogEDState();

        private Timer _ainAndDemoTimer;


        //public RemoteIO(string ed560ip = "YOUR_DEVICE_IP", string ed549ip = "YOUR_DEVICE_IP")
        public RemoteIO(string ed560ip = "YOUR_DEVICE_IP", string ed549ip = "YOUR_DEVICE_IP")
        {
            ed560 = EDDevice.Create(ed560ip) as ED560;
            ed549 = EDDevice.Create(ed549ip) as ED549;

            _ainAndDemoTimer = new Timer(demoTimerCallback, null, 250, 250);
            AIOLineChangedEventHandler dial1Change = (line, device, value, changeType) =>
            {
                //detect change of dial1 AIN2
                //Set voltmeter to that value
                _setVoltmeter(value);
            };
            AIOLineChangedEventHandler dial2Change = (line, device, value, changeType) =>
            {
                //detect change of dial 2 AIN3
                //Set ammeter to that value
                if (value != 0)
                {
                    _setAmmeter((16 * ((Math.Abs(value))/ 10)) + 4);
                }
                else
                {
                    _setAmmeter((Math.Abs(value)) + 4);
                }
            };
            AIOLineChangedEventHandler xslideChange = (line, device, value, changeType) =>
            {
                //detect change of xsilde AIN5
                //Set caesar to that value
                _setCaesar(value + 5);
            };
            AIOLineChangedEventHandler yslideChange = (line, device, value, changeType) =>
            {
                //detect change of y slide AIN4
                //Set fan to that value
                _setFan(value + 5);
            };
            ed549.AInputs[2].SubscribeToDeltaEvent(ref dial1Change, 1);
            ed549.AInputs[3].SubscribeToDeltaEvent(ref dial2Change, 2.5);//Dial 2 is jumpy so set to higher delta
            ed549.AInputs[4].SubscribeToDeltaEvent(ref xslideChange, 1);
            ed549.AInputs[5].SubscribeToDeltaEvent(ref yslideChange, 1);
        }

        public void Start()
        {
            try
            {
                ed560.Connect();
                ed549.Connect();
                //ed204.Connect();
                //ed527.Connect();
            }
            catch (SocketException e)
            {
                throw new Exception("Cannot connect to ED Device are you connected to the Brainboxes-Demo network?", e);
            }
            //_configureHandlers();
            //state.currentNumber = -1;
            _monitorCurrentState(ed560);
            //_incrementCount();
        }


        private void _monitorCurrentState(EDDevice device)
        {
            string currentFan = device.SendCommand("$0163");
            state.fan = Convert.ToDouble(currentFan.Substring(4, 6));
            string currentAmmeter = device.SendCommand("$0162");
            state.ammeter = Convert.ToDouble(currentAmmeter.Substring(4, 6));
            string currentVoltmeter = device.SendCommand("$0161");
            state.voltmeter = Convert.ToDouble(currentVoltmeter.Substring(4, 6));
            string currentCaesar = device.SendCommand("$0160");
            state.caesar = Convert.ToDouble(currentCaesar.Substring(4, 6));

            this.OnChange(state);
        }

        //private void _incrementCount()
        //{
        //    if (state.isSliderRight) return; //we're in an error state
        //    state.currentNumber = (state.currentNumber + 1) % 10;
        //    string command = "#0100" + numberEncoding[state.currentNumber];
        //    ed527.SendCommand(command);
        //    this.OnChange(state);
        //}

        private void _setFan(double value)
        {
            ed560.SendCommand("#013" + value);
            state.fan = value;
            this.OnChange(state);
        }
        private void _setCaesar(double value)
        {
            ed560.SendCommand("#010" + value);
            state.caesar = value;
            this.OnChange(state);
        }
        private void _setAmmeter(double value)
        {
            ed560.SendCommand("#012" + value);
            state.ammeter = value;
            this.OnChange(state);
        }
        private void _setVoltmeter(double value)
        {
            ed560.SendCommand("#011" + value);
            state.voltmeter = value;
            this.OnChange(state);
        }
        private void _setDemoMode(bool isDemoMode)
        {
            state.isDemoMode = isDemoMode;
            this.OnChange(state);
        }

        Random r = new Random();
        float percent = 0;
        private void demoTimerCallback(object p)
        {
            if (state.isDemoMode)
            {
                double newValueAmmeter = -8 * Math.Sin(2 * Math.PI * percent / 100) + 12;
                double newValueCaesar = 5 * Math.Sin(2 * Math.PI * percent / 100) + 5;
                double newValueVoltmeter = 5 * Math.Cos((2 * Math.PI * percent / 100)) + 5;
                double newValueFan = -5 * Math.Cos((2 * Math.PI * percent / 100)) + 5;
                percent = (percent + 2.5f) % 100.0f;

                //_setAmmeter(newValueAmmeter);
                ed560.SendCommand("#012" + newValueAmmeter);
                state.ammeter = newValueAmmeter;

                //_setFan(newValueFan);
                ed560.SendCommand("#013" + newValueFan);
                state.fan = newValueFan;

                //_setVoltmeter(newValueVoltmeter);
                ed560.SendCommand("#011" + newValueVoltmeter);
                state.voltmeter = newValueVoltmeter;

                //_setCaesar(newValueCaesar);
                ed560.SendCommand("#010" + newValueCaesar);
                state.caesar = newValueCaesar;
            }
            try
            {
                if (!ed549.IsConnected)
                {
                    ed549.Connect();
                }
                string inputStatus = ed549.SendCommand("#01");
                inputStatus = inputStatus.Substring(1);

                //e.g. >+01.301+01.576+09.741+08.963+3.0215-3.1745+08.074+00.000
                //state.temp = ed549.AInputs[0].AValue * 20;
                //state.humidity = ed549.AInputs[1].AValue / 10;
                //state.dial1 = ed549.AInputs[2].AValue;
                //state.dial2 = ed549.AInputs[3].AValue;
                //state.ySlide = ed549.AInputs[4].AValue;
                //state.xSlide = ed549.AInputs[5].AValue;
                //state.waterTemp = ed549.AInputs[6].AValue;

                state.temp = Convert.ToDouble(inputStatus.Substring(0, 7)) * 20.0;
                state.humidity = Convert.ToDouble(inputStatus.Substring(7, 7)) / 10.0;
                state.dial1 = Math.Round(Convert.ToDouble(inputStatus.Substring(14, 7)) / 0.9);  //if the dial went to 11
                state.dial2 = Math.Round(Convert.ToDouble(inputStatus.Substring(21, 7)) / 0.9);
                state.ySlide = Convert.ToDouble(inputStatus.Substring(28, 7)) / 5.0;
                state.xSlide = Convert.ToDouble(inputStatus.Substring(35, 7)) / 5.0;
                double waterTemp = Convert.ToDouble((inputStatus.Substring(42, 7))); 
                state.waterTemp = ((waterTemp * 280) -1775) / 26; //26y = 280x - 1775// 19.5C  = 8.15mA 67.1C = 12.57mA
            }
            catch (Exception e)
            {
                Console.WriteLine(e.Message);
            }
            if (this.OnChange != null)
            {
                this.OnChange(state);
            }
        }

        //private void _setSliderRight(bool toRight)
        //{
        //    state.isSliderRight = toRight;
        //    if (state.isSliderRight)
        //    {
        //        string command = "#0100" + errorCode;
        //        ed527.SendCommand(command);
        //        this.OnChange(state);
        //    }
        //    else
        //    {
        //        state.currentNumber--; //set the number back
        //        _incrementCount();
        //    }
        //}

        //private void _setBeeping(bool toOn)
        //{
        //    if (toOn)
        //    {
        //        state.isBeeping = true;
        //        this.OnChange(state);
        //        //beep momentarily
        //        ed527.Outputs[8].Value = 1;
        //        ed527.Outputs[8].Value = 0;
        //        state.isBeeping = false;

        //        this.OnChange(state);
        //    }
        //}

        public void Stop()
        {
            //ed527.Disconnect();
            //ed204.Disconnect();
            ed560.Disconnect();
            ed549.Disconnect();
        }

        public void UpdateState(AnalogEDState newState)
        {
            if (newState.Equals(this.state))
            {
                return;
            }
            Debug.WriteLine("State changed:");

            Debug.WriteLine(this.state.WhatHasChanged(newState));

            if (newState.ammeter != state.ammeter)
            {
                _setAmmeter(newState.ammeter);
            }
            if (newState.voltmeter != state.voltmeter)
            {
                _setVoltmeter(newState.voltmeter);
            }
            if (newState.caesar != state.caesar)
            {
                _setCaesar(newState.caesar);
            }
            if (newState.fan != state.fan)
            {
                _setFan(newState.fan);
            }
            if (newState.isDemoMode != state.isDemoMode)
            {
                _setDemoMode(newState.isDemoMode);
            }
        }


        public event Action<AnalogEDState> OnChange;
    }
    
    public class AnalogEDState
    {
        public double voltmeter = 0; //AOut1
        public double ammeter = 0; //AOut2
        public double caesar = 0; //AOut0
        public double fan = 0; //AOut3  // DIAL1 AIN2
        public bool isDemoMode = false;

        public double temp = 32.0; //AIN0
        public double humidity = 0; //AIN1

        public double dial1 = 0; //AIN2
        public double dial2 = 0; //AIN3

        public double waterTemp = 0; //AIN6

        public double xSlide = 0; //AIN5
        public double ySlide = 0; //AIN4




        public override bool Equals(object obOther)
        {
            if (null == obOther)
                return false;
            if (object.ReferenceEquals(this, obOther))
                return true;
            AnalogEDState other = obOther as AnalogEDState;
            if (other == null)
                return false;
            return this.caesar == other.caesar &&
                this.fan == other.fan &&
                this.ammeter == other.ammeter &&
                this.voltmeter == other.voltmeter &&
                this.isDemoMode == other.isDemoMode &&
                this.temp == other.temp &&
                this.humidity == other.humidity &&
                this.dial1 == other.dial1 &&
                this.dial2 == other.dial2 &&
                this.waterTemp == other.waterTemp &&
                this.xSlide == other.xSlide &&
                this.ySlide == other.ySlide;

        }
        public string WhatHasChanged(AnalogEDState that)
        {
            StringBuilder sb = new StringBuilder();
            if (this.caesar != that.caesar)
            {
                sb.AppendFormat("\tceasar has changed from {0:x} to {1:x}\n", this.caesar.ToString(), that.caesar.ToString());
            }
            if (this.fan != that.fan)
            {
                sb.AppendFormat("\tfan has changed from {0:x} to {1:x}\n", this.fan.ToString(), that.fan.ToString());
            }
            if (this.ammeter != that.ammeter)
            {
                sb.AppendFormat("\tammeter has changed from {0:x} to {1:x}\n", this.ammeter.ToString(), that.ammeter.ToString());
            }
            if (this.voltmeter != that.voltmeter)
            {
                sb.AppendFormat("\tvoltmeter has changed from {0:x} to {1:x}\n", this.voltmeter.ToString(), that.voltmeter.ToString());
            }
            if (this.isDemoMode != that.isDemoMode)
            {
                sb.AppendFormat("\tisDemoMode has changed from {0} to {1}\n", this.isDemoMode, that.isDemoMode);
            }

            if (this.temp != that.temp)
            {
                sb.AppendFormat("\ttemp has changed from {0:x} to {1:x}\n", this.temp.ToString(), that.temp.ToString());
            }
            if (this.humidity != that.humidity)
            {
                sb.AppendFormat("\thumidity has changed from {0:x} to {1:x}\n", this.humidity.ToString(), that.humidity.ToString());
            }
            if (this.dial1 != that.dial1)
            {
                sb.AppendFormat("\tdial1 has changed from {0:x} to {1:x}\n", this.dial1.ToString(), that.dial1.ToString());
            }
            if (this.dial2 != that.dial2)
            {
                sb.AppendFormat("\tdial2 has changed from {0:x} to {1:x}\n", this.dial2.ToString(), that.dial2.ToString());
            }
            if (this.waterTemp != that.waterTemp)
            {
                sb.AppendFormat("\twaterTemp has changed from {0} to {1}\n", this.waterTemp, that.waterTemp);
            }
            if (this.xSlide != that.xSlide)
            {
                sb.AppendFormat("\txSlide has changed from {0:x} to {1:x}\n", this.xSlide.ToString(), that.xSlide.ToString());
            }
            if (this.ySlide != that.ySlide)
            {
                sb.AppendFormat("\tySlide has changed from {0} to {1}\n", this.ySlide, that.ySlide);
            }

            return sb.ToString();

        }
        //public string ToJSON()
        //{
        //    return "{\"caesar\": " + caesar + ", \"fan\": " + fan + ", \"ammeter\": " + ammeter + ", \"voltmeter\": " + voltmeter + " }";
        //}
        //public static ED560State FromJson(string jsonString)
        //{
        //    JsonObject root = JsonValue.Parse(jsonString).GetObject();
        //    ED560State state = new ED560State()
        //    {
        //        caesar = Convert.ToDouble(root.GetNamedNumber("caesar")),
        //        fan = Convert.ToDouble(root.GetNamedNumber("fan")),
        //        ammeter = Convert.ToDouble(root.GetNamedNumber("ammeter")),
        //        voltmeter = Convert.ToDouble(root.GetNamedNumber("voltmeter")),
        //    };
        //    return state;
        //}
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
