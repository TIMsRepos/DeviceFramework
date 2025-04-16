using System.Collections.Generic;
using System.Net;
using System.Net.Sockets;
using TIM.Common.CoreStandard;
using TIM.Devices.Framework.Common.Settings;
using TIM.Devices.Framework.ElectronicCash.ZVT.Blocks;
using TIM.Devices.Framework.ElectronicCash.ZVT.Blocks.Abstract;
using TIM.Devices.Framework.ElectronicCash.ZVT.Helper;
using TIM.Devices.Framework.ElectronicCash.ZVT.Terminals.Exceptions;

namespace TIM.Devices.Framework.ElectronicCash.ZVT.TransportLayers
{
    public class IPTransportLayer : ITransportLayer
    {
        private readonly TcpClient _tcpClient;
        private NetworkStream _networkStream;

        public IPEndPoint IPEndPoint { get; }

        public IPTransportLayer()
            : this(GetHost(), GetPort())
        {
        }

        public IPTransportLayer(string strHost, int intPort)
            : this(new IPEndPoint(IPAddress.Parse(strHost), intPort))
        {
        }

        public IPTransportLayer(IPEndPoint ipEndPoint)
        {
            IPEndPoint = ipEndPoint;
            _tcpClient = new TcpClient();
        }

        public void Open()
        {
            _tcpClient.Connect(IPEndPoint);
            _networkStream = _tcpClient.GetStream();
        }

        public void Close()
        {
            _tcpClient.Close();
        }

        public void SendAcknowledge(bool blnAck)
        {
            /*byte bytByte = (byte) (blnAck ? SpecialChars.Acknowledged : SpecialChars.NotAcknowledged);

            TraceHelper.Add(true, bytByte);

            MyStream.WriteByte(bytByte);*/
        }

        public void SendAPDU<T>(T APDUParam) where T : Blocks.Abstract.APDU
        {
            var lstBytes = new List<byte>();

            // Body (APDU)
            if (typeof(T) == typeof(CommandAPDU))
            {
                var commandAPDU = (CommandAPDU)(APDU)APDUParam;
                lstBytes.Add(commandAPDU.ControlField.CLASS);
                lstBytes.Add(commandAPDU.ControlField.INSTR);
                lstBytes.Add(commandAPDU.Length);
                lstBytes.AddRange(commandAPDU.Data);
            }
            else
            {
                ResponseAPDU MyResAPDU = (ResponseAPDU)(APDU)APDUParam;
                lstBytes.Add(MyResAPDU.ControlField.CCRC);
                lstBytes.Add(MyResAPDU.ControlField.APRC);
                lstBytes.Add(MyResAPDU.Length);
                lstBytes.AddRange(MyResAPDU.Data);
            }

            byte[] bytSerialData = lstBytes.ToArray();

            TraceHelper.Add(true, bytSerialData);

            _networkStream.Write(bytSerialData, 0, bytSerialData.Length);
        }

        public Enums.SpecialChars ReceiveAcknowledge()
        {
            /*byte bytByte = 0x10;
            try { bytByte = (byte)MyStream.ReadByte(); }
            catch { }

            TraceHelper.Add(false, bytByte);

            SpecialChars MyState = (SpecialChars)bytByte;

            if (MyState != SpecialChars.Acknowledged &&
                MyState != SpecialChars.NotAcknowledged)
                throw new TransportLayerException(string.Format("Received byte value wasn't an acknowledge state (value: 0x{0})", bytByte));

            return MyState;*/

            return Enums.SpecialChars.Acknowledged;
        }

        public T ReceiveAPDU<T>() where T : APDU
        {
            var lstBytes = new List<byte>
            {
                // Body (APDU)
                (byte) _networkStream.ReadByte(),// CCRC
                (byte) _networkStream.ReadByte(),// APRC
                (byte) _networkStream.ReadByte() // LEN
            };

            var bytData = new byte[lstBytes[lstBytes.Count - 1]];
            for (var i = 0; i < bytData.Length; ++i)
            {
                bytData[i] = (byte)_networkStream.ReadByte(); // DATA
            }
            lstBytes.AddRange(bytData);

            TraceHelper.Add(false, lstBytes.ToArray());

            T receiveAPDU;
            if (typeof(T) == typeof(CommandAPDU))
            {
                var commandAPDU = new CommandAPDU(new CommandControlField(lstBytes[0], lstBytes[1]), bytData);
                receiveAPDU = (T)(APDU)commandAPDU;
            }
            else
            {
                var responseAPDU = new ResponseAPDU(new ResponseControlField(lstBytes[0], lstBytes[1]), bytData);
                receiveAPDU = (T)(APDU)responseAPDU;
            }

            return receiveAPDU;
        }

        public void Dispose()
        {
            Close();
        }

        private static string GetHost()
        {
            if (SettingsManager.Empty(Enums.ComputerSetting.EC_Terminal, Enums.ComputerDetailSetting.Host))
            {
                throw new TerminalWrongConfigException("Host is missing");
            }
            return SettingsManager.GetValue<string>(Enums.ComputerSetting.EC_Terminal, Enums.ComputerDetailSetting.Host);
        }

        private static int GetPort()
        {
            if (SettingsManager.Empty(Enums.ComputerSetting.EC_Terminal, Enums.ComputerDetailSetting.Port))
            {
                throw new TerminalWrongConfigException("Port is missing");
            }
            return SettingsManager.GetValue<int>(Enums.ComputerSetting.EC_Terminal, Enums.ComputerDetailSetting.Port);
        }
    }
}