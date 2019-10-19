using System;
using System.IO.Ports;

namespace GateAccessControl.Communication
{
    public class SerialCOM
    {
        private String port;
        private int baudr;
        public SerialPort _serialPort;

        public SerialCOM(String port = "COM4", int baudr = 9600)
        {
            this.port = port;
            this.baudr = baudr;
        }

        public bool Open()
        {
            try
            {
                _serialPort = new SerialPort();
                _serialPort.PortName = port;//Set your board COM
                _serialPort.BaudRate = baudr;
                _serialPort.ReadTimeout = 5000;
                _serialPort.Open();
                return true;
            }
            catch
            {
                return false;
            }
        }

        public String ReceiveData()
        {
            try
            {
                if (!_serialPort.IsOpen)
                {
                    return "Null";
                }

                return _serialPort.ReadLine();
            }
            catch
            {
                return "Null";
            }
        }

        public void Close()
        {
            if (_serialPort.IsOpen)
            {
                _serialPort.Close();
            }
        }
    }
}
