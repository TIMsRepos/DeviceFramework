using System;
using System.Runtime.Serialization;
using TIM.Devices.Framework.Database;

namespace TIM.Devices.Framework.Communication
{
    [DataContract]
    public class MediaDetectedEventArgs : EventArgs
    {
        #region Members

        private int intDeviceID;
        private string strECheckInID;
        private Int64? lngPersonID;
        private Int32? intServiceEmployeeID;
        private Int32? intRangeOfCheckInIDsID;
        private Int32? intCheckInID;
        private string strDeviceName;
        private long lngDate;

        #endregion

        #region DataMembers

        [DataMember]
        public int DeviceID
        {
            get { return intDeviceID; }
            internal set { intDeviceID = value; }
        }

        [DataMember]
        public string ECheckInID
        {
            get { return strECheckInID; }
            internal set { strECheckInID = value; }
        }

        [DataMember]
        public Int64? PersonID
        {
            get { return lngPersonID; }
            internal set { lngPersonID = value; }
        }

        [DataMember]
        public Int32? ServiceEmployeeID
        {
            get { return intServiceEmployeeID; }
            internal set { intServiceEmployeeID = value; }
        }

        [DataMember]
        public Int32? RangeOfCheckInIDsID
        {
            get { return intRangeOfCheckInIDsID; }
            internal set { intRangeOfCheckInIDsID = value; }
        }

        [DataMember]
        public Int32? CheckInID
        {
            get { return intCheckInID; }
            internal set { intCheckInID = value; }
        }

        [DataMember]
        public string DeviceName
        {
            get { return strDeviceName; }
            internal set { strDeviceName = value; }
        }

        [DataMember]
        public long Date
        {
            get { return lngDate; }
            internal set { lngDate = value; }
        }

        #endregion

        #region Constructors

        public MediaDetectedEventArgs(DeviceManagerEventArgs e)
            : this(e.DeviceID, e.Device.Name, e.MediaID)
        {
        }

        protected MediaDetectedEventArgs(int intDeviceID, string strDeviceName, string strMediaID)
            : base()
        {
            this.intDeviceID = intDeviceID;
            this.strECheckInID = strMediaID;
            this.lngPersonID = null;
            this.intServiceEmployeeID = null;
            this.intRangeOfCheckInIDsID = null;
            this.intCheckInID = null;
            this.strDeviceName = strDeviceName;
            this.lngDate = DateTime.Now.Ticks;

            // get IDs
            lngPersonID = -1;
            intServiceEmployeeID = -1;
            intRangeOfCheckInIDsID = -1;
            intCheckInID = -1;

            DBContext.DataContextRead.spSearchElectronicKey(
                intDeviceID, strECheckInID, DBContext.CurrentComputerSubsidiaryID,
                ref lngPersonID, ref intServiceEmployeeID,
                ref intRangeOfCheckInIDsID, ref intCheckInID);

            if (lngPersonID == -1) lngPersonID = null;
            if (intServiceEmployeeID == -1) intServiceEmployeeID = null;
            if (intRangeOfCheckInIDsID == -1) intRangeOfCheckInIDsID = null;
            if (intCheckInID == -1) intCheckInID = null;
        }

        #endregion
    }
}