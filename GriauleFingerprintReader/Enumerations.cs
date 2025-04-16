using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace GriauleFingerprintReader
{
    public enum SuccessCodes
    {
        GR_OK = 0, // Success 
        GR_BAD_QUALITY = 0, // Extraction succeeded, template has bad quality 
        GR_MEDIUM_QUALITY = 1, // Extraction succeeded, template has medium quality 
        GR_HIGH_QUALITY = 2, // Extraction succeeded, template has high quality 
        GR_MATCH = 1, // Fingerprints match 
        GR_NOT_MATCH = 0, // Fingerprints don't match 
        GR_DEFAULT_USED = 3 // A supplied parameter is invalid or out of range, default value will be used 
    }
    public enum EventCodes
    {
        GR_PLUG = 21, // A fingerprint reader was plugged on the machine 
        GR_UNPLUG = 20, // A fingerprint reader was unplugged from the machine 
        GR_FINGER_DOWN = 11, // A finger was placed over the fingerprint reader 
        GR_FINGER_UP = 10 // A finger was removed from the fingerprint reader 
    }
    public enum ImageValues
    {
        GR_DEFAULT_RES = 500, // Default resolution value for an image in DPI 
        GR_DEFAULT_DIM = 500, // Maximum width and height of an image in pixels that is processed on template extraction 
        GR_MAX_SIZE_TEMPLATE = 10000, // Maximum template size in bytes 
        GR_MAX_IMAGE_WIDTH = 1280, // Maximum acceptable image width in pixels 
        GR_MAX_IMAGE_HEIGHT = 1280, // Maximum acceptable image height in pixels 
        GR_MAX_RESOLUTION = 1000, // Maximum acceptable image resolution in DPI 
        GR_MIN_IMAGE_WIDTH = 50, // Minimum acceptable image width in pixels 
        GR_MIN_IMAGE_HEIGHT = 50, // Minimum acceptable image height in pixels 
        GR_MIN_RESOLUTION = 125, // Minimum acceptable image resolution in DPI 
        GR_IMAGE_NO_COLOR = 536870911 // No defined color for biometric display 
    }
}
