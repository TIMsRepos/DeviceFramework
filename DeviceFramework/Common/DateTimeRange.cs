using System;
using System.Collections.Generic;
using TIM.Common.CoreStandard;
using TIM.Devices.Framework.Common.Extensions;

namespace TIM.Devices.Framework.Common
{
    /// <summary>
    /// Represents a range between two DateTime values
    /// </summary>
    public class DateTimeRange
    {
        #region Fields

        private DateTime dtFrom;
        private DateTime dtTo;

        #endregion

        #region Properties

        /// <summary>
        /// The begin of the range
        /// </summary>
        public DateTime From
        {
            get { return dtFrom; }
            set
            {
                if (value > dtTo)
                    throw new ArgumentException("'From' may not be after 'To'");
                dtFrom = value;
            }
        }

        /// <summary>
        /// The end of the range
        /// </summary>
        public DateTime To
        {
            get { return dtTo; }
            set
            {
                if (value < dtFrom)
                    throw new ArgumentException("'To' may not be before 'From'");
                dtTo = value;
            }
        }

        public IEnumerable<DateTime> Days
        {
            get { return GetDateTimeEnumerable(d => d.AddDays(1)); }
        }
        #endregion

        #region Constructors

        public DateTimeRange(DateTime dtFrom, DateTime dtTo)
        {
            SetFromAndTo(dtFrom, dtTo);
        }

        #endregion

        #region Methods

        /// <summary>
        /// Sets the values for 'From' and 'To' in one step
        /// </summary>
        /// <param name="dtFrom">From</param>
        /// <param name="dtTo">To</param>
        public void SetFromAndTo(DateTime dtFrom, DateTime dtTo)
        {
            if (dtFrom > dtTo)
                throw new ArgumentException("Negative ranges are not allowed");
            this.dtFrom = dtFrom;
            this.dtTo = dtTo;
        }

        private IEnumerable<DateTime> GetDateTimeEnumerable(Func<DateTime, DateTime> MyFunc)
        {
            DateTime dtCurrent = From;
            do
            {
                yield return dtCurrent;
                dtCurrent = MyFunc(dtCurrent);
            }
            while (dtCurrent <= To);
        }

        public string ToString(Enums.StringFormat MyStringFormat)
        {
            if ((MyStringFormat & Enums.StringFormat.CutByMonth) == Enums.StringFormat.CutByMonth &&
                From.Month == To.Month &&
                From.Year == To.Year)
            {
                return string.Format("{0:00}. - {1:00}.{2:00}.{3:0000}", From.Day, To.Day, From.Month, From.Year);
            }
            else if ((MyStringFormat & Enums.StringFormat.CutByYear) == Enums.StringFormat.CutByYear &&
                From.Year == To.Year)
            {
                return string.Format("{0:00}.{1:00}. - {2:00}.{3:00}.{4:0000}", From.Day, From.Month, To.Day, To.Month, From.Year);
            }
            else
            {
                return string.Format("{0:00}.{1:00}.{2:0000} - {3:00}.{4:00}.{5:0000}", From.Day, From.Month, From.Year, To.Day, To.Month, To.Year);
            }
        }

        public override string ToString()
        {
            return ToString(Enums.StringFormat.CutByMonth | Enums.StringFormat.CutByYear);
        }

        #endregion
    }
}