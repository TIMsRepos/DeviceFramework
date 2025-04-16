using System;
using System.Collections.Generic;

namespace TIM.Devices.Framework.Common
{
    public class FilterManager<T>
    {
        public event EventHandler<GenericEventArgs<List<T>>> Filtered;

        private IEnumerable<T> eElements;

        public List<Predicate<T>> Predicates { get; set; }

        public FilterManager(IEnumerable<T> eElements)
        {
            Predicates = new List<Predicate<T>>();
            this.eElements = eElements;
        }

        public void Filter()
        {
            List<T> lstElements = new List<T>();

            bool blnOk;
            foreach (T MyObj in eElements)
            {
                blnOk = true;
                foreach (Predicate<T> MyPred in Predicates)
                    blnOk &= MyPred(MyObj);
                if (blnOk)
                    lstElements.Add(MyObj);
            }

            OnFiltered(new GenericEventArgs<List<T>>(lstElements));
        }

        protected virtual void OnFiltered(GenericEventArgs<List<T>> evtArgs)
        {
            EventHandler<GenericEventArgs<List<T>>> evtHandler = Filtered;
            if (evtHandler != null)
                evtHandler(this, evtArgs);
        }
    }
}