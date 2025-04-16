using System.Collections.Generic;
using System.IO.Ports;
using System.Linq;
using TIM.Common.CoreStandard;
using TIM.Devices.Framework.Common.Settings;
using TIM.Devices.Framework.ElectronicCash.ZVT.Blocks;
using TIM.Devices.Framework.ElectronicCash.ZVT.Blocks.Abstract;
using TIM.Devices.Framework.ElectronicCash.ZVT.Helper;
using TIM.Devices.Framework.ElectronicCash.ZVT.Terminals.Exceptions;

namespace TIM.Devices.Framework.ElectronicCash.ZVT.TransportLayers
{
    public class ComTransportLayer : ITransportLayer
    {
        private readonly SerialPort _comPort;

        public byte ComPort { get; set; }
        
        public ComTransportLayer(byte bytComPort, int intBaud)
        {
            ComPort = bytComPort;

            _comPort = new SerialPort("COM" + bytComPort)
            {
                BaudRate = intBaud,
                Handshake = Handshake.None,
                DataBits = 8,
                Parity = Parity.None,
                StopBits = StopBits.Two
            };
        }

        public void Open()
        {
            _comPort.Open();
        }

        public void Close()
        {
            try
            {
                _comPort.Close();
            }
            catch
            {
            }
        }

        public void SendAcknowledge(bool blnAck)
        {
            var bytByte = (byte)(blnAck ? Enums.SpecialChars.Acknowledged : Enums.SpecialChars.NotAcknowledged);

            TraceHelper.Add(true, bytByte);

            _comPort.Write(new[] { bytByte }, 0, 1);
        }

        public void SendAPDU<T>(T APDUParam) where T : APDU
        {
            var lstBytes = new List<byte>
            {
                // Start
                (byte) Enums.SpecialChars.DataLineEscape,    // 0x10
                (byte) Enums.SpecialChars.StartOfText        // 0x02
            };

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
                var responseAPDU = (ResponseAPDU)(APDU)APDUParam;
                lstBytes.Add(responseAPDU.ControlField.CCRC);
                lstBytes.Add(responseAPDU.ControlField.APRC);
                lstBytes.Add(responseAPDU.Length);
                lstBytes.AddRange(responseAPDU.Data);
            }

            // End
            lstBytes.Add((byte)Enums.SpecialChars.DataLineEscape); // 0x10
            lstBytes.Add((byte)Enums.SpecialChars.EndOfText); // 0x03

            // CRC
            var bytCrcData = typeof(T) == typeof(CommandAPDU)
                ? ((CommandAPDU)(APDU)APDUParam).CRCData.Concat(new[] { (byte)Enums.SpecialChars.EndOfText }).ToArray()
                : ((ResponseAPDU)(APDU)APDUParam).CRCData.Concat(new[] { (byte)Enums.SpecialChars.EndOfText }).ToArray();
            lstBytes.AddRange(CRCHelper.CalculateCRCBytes(bytCrcData));

            // Fill up doubled DLEs
            // Skip 2 cmd start bytes, 2 cmd type bytes and 1 size byte at the beginning
            // Skip 2 cmd end bytes, 2 crc bytes and the end
            for (var i = 5; i < lstBytes.Count - 4; ++i)
            {
                if (lstBytes[i] != (byte)Enums.SpecialChars.DataLineEscape)
                {
                    continue;
                }
                lstBytes.Insert(i, (byte)Enums.SpecialChars.DataLineEscape);
                ++i;
            }

            var bytSerialData = lstBytes.ToArray();

            TraceHelper.Add(true, bytSerialData);

            _comPort.Write(bytSerialData, 0, bytSerialData.Length);
        }

        public Enums.SpecialChars ReceiveAcknowledge()
        {
            byte bytByte = 0x10;
            try { bytByte = (byte)_comPort.ReadByte(); }
            catch { }

            TraceHelper.Add(false, bytByte);

            var state = (Enums.SpecialChars)bytByte;

            if (state != Enums.SpecialChars.Acknowledged &&
                state != Enums.SpecialChars.NotAcknowledged)
            {
                throw new TransportLayerException($"Received byte value wasn't an acknowledge state (value: 0x{bytByte})");
            }

            return state;
        }

        public T ReceiveAPDU<T>() where T : APDU
        {
            var lstBytes = new List<byte>
            {
            // Start
                (byte) _comPort.ReadByte(),// 0x10
                (byte) _comPort.ReadByte(),// 0x02
            // Body (APDU)
                (byte) _comPort.ReadByte(),// CCRC
                (byte) _comPort.ReadByte(),// APRC
                (byte) _comPort.ReadByte()// LEN
            };

            var bytData = new byte[lstBytes[lstBytes.Count - 1]];
            byte bytLastData = 0;
            for (var i = 0; i < bytData.Length; ++i)
            {
                bytData[i] = (byte)_comPort.ReadByte(); // DATA

                // skip double DLEs
                if (bytLastData == (byte)Enums.SpecialChars.DataLineEscape &&
                    bytData[i] == (byte)Enums.SpecialChars.DataLineEscape)
                {
                    --i;
                    // Force unknown last data
                    // Without: [ 0x10, 0x10 ]             => [ 0x10 ]
                    // Without: [ 0x10, 0x10, 0x10 ]       => [ 0x10 ]
                    // Without: [ 0x10, 0x10, 0x10, 0x10 ] => [ 0x10 ]
                    // With:    [ 0x10, 0x10, 0x10, 0x10 ] => [ 0x10, 0x10 ]
                    bytLastData = 0;
                }
                else
                {
                    bytLastData = bytData[i];
                }
            }
            lstBytes.AddRange(bytData);

            // End
            lstBytes.Add((byte)_comPort.ReadByte()); // 0x10
            lstBytes.Add((byte)_comPort.ReadByte()); // 0x03

            // CRC
            lstBytes.Add((byte)_comPort.ReadByte()); // LOW CRC
            lstBytes.Add((byte)_comPort.ReadByte()); // HIGH CRC

            TraceHelper.Add(false, lstBytes.ToArray());

            // Validation
            if (lstBytes[0] != (byte)Enums.SpecialChars.DataLineEscape ||
                lstBytes[1] != (byte)Enums.SpecialChars.StartOfText)
            {
                throw new TransportLayerException("Invalid package start sequence");
            }
            if (lstBytes[lstBytes.Count - 4] != (byte)Enums.SpecialChars.DataLineEscape ||
                lstBytes[lstBytes.Count - 3] != (byte)Enums.SpecialChars.EndOfText)
            {
                throw new TransportLayerException("Invalid package end sequence");
            }

            byte[] bytCrcData;
            T receiveAPDU;
            if (typeof(T) == typeof(CommandAPDU))
            {
                var commandAPDU = new CommandAPDU(new CommandControlField(lstBytes[2], lstBytes[3]), bytData);
                bytCrcData = commandAPDU.CRCData.Concat(new[] { (byte)Enums.SpecialChars.EndOfText }).ToArray();
                receiveAPDU = (T)(APDU)commandAPDU;
            }
            else
            {
                var responseAPDU = new ResponseAPDU(new ResponseControlField(lstBytes[2], lstBytes[3]), bytData);
                bytCrcData = responseAPDU.CRCData.Concat(new[] { (byte)Enums.SpecialChars.EndOfText }).ToArray();
                receiveAPDU = (T)(APDU)responseAPDU;
            }

            // CRC Check
            var bytCrc = CRCHelper.CalculateCRCBytes(bytCrcData);
            if (lstBytes[lstBytes.Count - 2] != bytCrc[0] ||
                lstBytes[lstBytes.Count - 1] != bytCrc[1])
            {
                throw new TransportLayerException("Corrupt CRC");
            }

            return receiveAPDU;
        }

        public void Dispose()
        {
            Close();
        }
    }
}