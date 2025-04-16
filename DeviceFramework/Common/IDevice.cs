using System;
using System.Collections.Generic;
using TIM.Common.Data.Entities;
using TIM.Devices.Framework.Common.Settings;

namespace TIM.Devices.Framework.Common
{
    /// <summary>
    /// The interface for all devices managed by the DeviceManager.
    /// </summary>
    /// <remarks>While working with a device (e.g. in a MediaFound EventHandler)
    /// all GetXXX() operations need to be placed inside a try-catch block, catching
    /// the possible MissingMediaException.</remarks>
    public interface IDevice
    {
        #region Events

        /// <summary>
        /// Event fires if MediaSearchEnabled is active and a media is found by the device
        /// </summary>
        event EventHandler<DeviceMediaDetectedEventArgs2> MediaDetected;

        /// <summary>
        /// Event fires if MediaSearchEnabled is active and a media is removed from the device
        /// </summary>
        event EventHandler<DeviceMediaRemovedEventArgs2> MediaRemoved;

        #endregion

        #region Getter & Setter

        /// <summary>
        /// Contains the name of the device, e.g. for selection
        /// </summary>
        string Name { get; }

        /// <summary>
        /// Contains an array of Type objects representing all supported content types for Get/SetMediaContent
        /// </summary>
        Type[] SupportedContentTypes { get; }

        /// <summary>
        /// Checks whether a media is available for reading/writing
        /// </summary>
        bool MediaAvailable { get; }

        /// <summary>
        /// Checks whether the device supports writing
        /// </summary>
        bool IsWriteable { get; }

        /// <summary>
        /// Checks whether the device supports a cash value
        /// </summary>
        bool SupportsCash { get; }

        /// <summary>
        /// Checks whether the device supports content addtionally to the unique media id
        /// </summary>
        bool SupportsContent { get; }

        /// <summary>
        /// Checks whether the device supports the abbility to track the media movement - found => removed
        /// </summary>
        bool SupportsTracking { get; }

        /// <summary>
        /// Checks whether the device supports the manual request
        /// </summary>
        bool SupportsRequest { get; }

        /// <summary>
        /// Checks whether the device supports triggering, e.g. an open/close relay
        /// </summary>
        bool SupportsTrigger { get; }

        /// <summary>
        /// Checks whether the device supports toggling, e.g. a switching relay
        /// </summary>
        bool SupportsToggle { get; }

        /// <summary>
        /// Defines whether the devives is looking for a media and fires MediaFound when found
        /// </summary>
        /// <remarks>The value gets disabled when the MediaFound event gets fired.</remarks>
        bool MediaSearchEnabled { get; set; }

        /// <summary>
        /// Defines a string containing multiple settings for the device like COM port, baudrate etc.
        /// </summary>
        ConfigString ConfigString { get; set; }

        /// <summary>
        /// Defines the values that can or must be part of the ConfigString
        /// </summary>
        ConfigStringDescription[] ConfigStringDescriptions { get; }

        /// <summary>
        /// Defines the capabilities of the device
        /// </summary>
        DeviceCapabilities DeviceCapabilities { get; }

        bool DeviceIsOpen { get; set; }

        Enumerations.DeviceIDs DeviceID { get; }

        string Message { get; set; }

        bool IsConnected { get; }

        int DeviceFrameworkDeviceID { get; }

        bool SupportsUnblockLockerKey { get; }

        #endregion

        #region Methods

        /// <summary>
        /// Tries to self configure itself by using computer related settings etc.
        /// </summary>
        bool SelfConfig();

        /// <summary>
        /// Opens the devices after configuration and before usage
        /// </summary>
        void Open();

        /// <summary>
        /// Closes the devices after usage for cleanup
        /// </summary>
        void Close();

        /// <summary>
        /// Requests the MediaFound event manually
        /// </summary>
        void Request();

        /// <summary>
        /// Gets the unique id of the media
        /// </summary>
        /// <returns>The unique media id</returns>
        string GetMediaID();

        /// <summary>
        /// Gets the content of the media
        /// </summary>
        /// <returns>The media content object</returns>
        object GetMediaContent();

        /// <summary>
        /// Gets the cash value on the media
        /// </summary>
        /// <returns>The media cash value</returns>
        float GetMediaCash();

        /// <summary>
        /// Sets the content on the media
        /// </summary>
        /// <param name="objContent">The content, needs to be a Type from SupportedContentTypes</param>
        void SetMediaContent(object objContent);

        /// <summary>
        /// Sets the cash value on the media
        /// </summary>
        /// <param name="fltCash">The cash value</param>
        void SetMediaCash(float fltCash);

        /// <summary>
        /// Triggers the relay for the given time
        /// </summary>
        /// <param name="tsActive">The time to be active for the relay</param>
        void Trigger(TimeSpan tsActive);

        /// <summary>
        /// Toggles the relay for the given time, waits and toggles it again
        /// </summary>
        /// <param name="tsActive">The time to be active for the relay</param>
        /// <param name="tsBreak">The time to wait between the two toggles</param>
        void Toggle(TimeSpan tsActive, TimeSpan tsBreak);

        /// <summary>
        /// Triggers or toggles the relay, configurations need to be passed by the ConfigString
        /// </summary>
        void TriggerOrToggle();

        /// <summary>
        /// Returns true if device is not configured for automated check in and can be opened with TIM's Devices
        /// </summary>
        bool AllowOpenDevice(List<DeviceDetail> deviceDetails, List<int> usedDeviceDetailIDs);

        bool BlockOrUnblockLockerKey(bool unblock);

        #endregion
    }
}