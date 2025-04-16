using System;
using System.Collections.Generic;
using System.Runtime.Serialization;
using TIM.Devices.Framework.Common;
using TIM.Devices.Framework.Database;

namespace TIM.Devices.Framework.Communication
{
    [DataContract]
    public class MediaDetectedEventArgs2 : EventArgs
    {
        #region Fields

        private Int64? lngPersonID;
        private Int32? intServiceEmployeeID;
        private Int32? intRangeOfCheckInIDsID;
        private Int32? intCheckInID;

        #endregion

        #region DataMembers

        [DataMember]
        public int DeviceID { get; internal set; }

        [DataMember]
        public int DeviceFrameworkDeviceID { get; internal set; }

        [DataMember]
        public string ECheckInID { get; internal set; }

        [DataMember]
        public long? PersonID
        {
            get => lngPersonID;
            internal set => lngPersonID = value;
        }

        [DataMember]
        public int? ServiceEmployeeID
        {
            get => intServiceEmployeeID;
            internal set => intServiceEmployeeID = value;
        }

        [DataMember]
        public int? RangeOfCheckInIDsID
        {
            get => intRangeOfCheckInIDsID;
            internal set => intRangeOfCheckInIDsID = value;
        }

        [DataMember]
        public int? CheckInID
        {
            get => intCheckInID;
            internal set => intCheckInID = value;
        }

        [DataMember]
        public string DeviceName { get; internal set; }

        [DataMember]
        public long Date { get; internal set; }

        [DataMember]
        public DeviceCapabilities DeviceCapabilities { get; internal set; }

        [DataMember]
        public Dictionary<Payloads, object> Payload { get; }

        #endregion

        #region Constructors

        public MediaDetectedEventArgs2(DeviceManagerMediaDetectedEventArgs2 e)
            : this(e.DeviceID, e.Device.Name, e.Device.DeviceCapabilities, e.Payload[Payloads.MediaID].ToString(), e.Payload, e.DeviceFrameworkDeviceID)
        {
        }

        protected MediaDetectedEventArgs2(int intDeviceID, string strDeviceName, DeviceCapabilities MyDeviceCapabilities, string strMediaID, Dictionary<Payloads, object> dicPayload, int deviceFrameworkDeviceID)
        {
            Payload = dicPayload;
            DeviceCapabilities = MyDeviceCapabilities;

            DeviceID = intDeviceID;
            ECheckInID = strMediaID;
            lngPersonID = null;
            intServiceEmployeeID = null;
            intRangeOfCheckInIDsID = null;
            intCheckInID = null;
            DeviceName = strDeviceName;
            Date = DateTime.Now.Ticks;
            DeviceFrameworkDeviceID = deviceFrameworkDeviceID;

            // get IDs
            lngPersonID = -1;
            intServiceEmployeeID = -1;
            intRangeOfCheckInIDsID = -1;
            intCheckInID = -1;

            DBContext.DataContextRead.spSearchElectronicKey(
                intDeviceID, ECheckInID, DBContext.CurrentComputerSubsidiaryID,
                ref lngPersonID, ref intServiceEmployeeID,
                ref intRangeOfCheckInIDsID, ref intCheckInID);

            if (lngPersonID == -1)
                lngPersonID = dicPayload.ContainsKey(Payloads.PersonID) ? Convert.ToInt64(dicPayload[Payloads.PersonID].ToString()) : default(long?);
            if (intServiceEmployeeID == -1)
                intServiceEmployeeID = null;
            if (intRangeOfCheckInIDsID == -1)
                intRangeOfCheckInIDsID = null;
            if (intCheckInID == -1)
                intCheckInID = null;
        }

        #endregion
    }
}