namespace TIM.Devices.MotorolaDS6707.MotoSNAPI
{
    public enum SnapiParamIds
    {
        ImageFileType = 0x130,
        VideoViewfinder = 0x144
    }

    public enum SnapiImageTypes : int
    {
        ///<summary>Unknown or unrecognized image format</summary>
        Unknown = 0,

        /// <summary>The desired image format is Windows bitmap</summary>
        Bmp = 3,

        /// <summary>The desired image format is JPEG</summary>
        Jpeg = 1,

        /// <summary>The desired image format is TIFF</summary>
        Tiff = 4
    }

    public abstract class SnapiParam
    {
        protected SnapiParam(short id)
        {
            paramId = id;
        }

        public SnapiParamIds ParamId
        {
            get { return (SnapiParamIds)ParamId; }
        }

        public short RawId
        {
            get { return this.paramId; }
        }

        private short paramId;
    }

    public class InvalidParamClass : System.Exception
    {
        public InvalidParamClass()
            : base("The parameter object passed is not based on SnapiSimpleParam")
        {
        }
    }

    /// <summary>
    /// A simple, non-string, type parameter for a scanner
    /// </summary>
    public class SnapiSimpleParam : SnapiParam
    {
        /// <summary>
        /// Initialize a new instance of the SnapiSimpleParam class, using the specified
        /// paramerer id and base parameter value type
        /// </summary>
        /// <param name="id">The id of the parameter</param>
        /// <param name="value">The integer-based value for the parameter</param>
        public SnapiSimpleParam(SnapiParamIds id, short value)
            : base((short)id)
        {
            this.value = value;
        }

        /// <summary>
        /// Initialize a new instance of the SnapiSimpleParam class, using the specified
        /// paramerer id and base parameter value type
        /// </summary>
        /// <param name="id">The id of the parameter</param>
        /// <param name="value">The integer-based value for the parameter</param>
        public SnapiSimpleParam(int id, short value)
            : base((short)id)
        {
            this.value = value;
        }

        /// <summary>
        /// Obtain the base, simple parameter value
        /// </summary>
        public short RawValue
        {
            get { return value; }
        }

        protected short value;
    }

    /// <summary>
    /// A simple boolean value parameter for a scanner
    /// </summary>
    public class SnapiBoolParam : SnapiSimpleParam
    {
        /// <summary>
        /// Initialize a new instance of the SnapiBoolParam class, using the specified
        /// parameter id and value
        /// </summary>
        /// <param name="id">The id of the parameter</param>
        /// <param name="value">The boolean value of the parameter</param>
        public SnapiBoolParam(SnapiParamIds id, bool value)
            : base(id, (short)(value ? 1 : 0))
        {
        }

        /// <summary>
        /// Initialize a new instance of the SnapiBoolParam class, using the specified
        /// parameter id and value
        /// </summary>
        /// <param name="id">The id of the parameter</param>
        /// <param name="value">The boolean value of the parameter</param>
        public SnapiBoolParam(int id, bool value)
            : base(id, (short)(value ? 1 : 0))
        {
        }

        /// <summary>
        /// The boolean value of the parameter
        /// </summary>
        public bool Value
        {
            get { return value != 0; }
            set { this.value = (short)(Value ? 1 : 0); }
        }
    }

    /// <summary>
    /// A simple integer value parameter for a scanner
    /// </summary>
    public class SnapiIntParam : SnapiSimpleParam
    {
        /// <summary>
        /// Initialize a new instance of the SnapiIntParam class, using the specified parameter
        /// id and value
        /// </summary>
        /// <param name="id">The id of the parameter</param>
        /// <param name="value">The integer value of the parameter</param>
        public SnapiIntParam(SnapiParamIds id, int value)
            : base(id, (short)value)
        {
        }

        /// <summary>
        /// Initialize a new instance of the SnapiIntParam class, using the specified parameter
        /// id and value
        /// </summary>
        /// <param name="id">The id of the parameter</param>
        /// <param name="value">The integer value of the parameter</param>
        public SnapiIntParam(int id, int value)
            : base(id, (short)value)
        {
        }

        /// <summary>
        /// The integer value of the parameter
        /// </summary>
        public int Value
        {
            get { return value; }
            set { this.value = (short)Value; }
        }
    }
}