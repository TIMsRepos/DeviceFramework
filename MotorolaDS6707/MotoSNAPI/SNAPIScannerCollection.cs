namespace TIM.Devices.MotorolaDS6707.MotoSNAPI
{
    public class SnapiScannerCollection
    {
        /// <summary>
        /// The enumerator method required to be Enumerable
        /// </summary>
        /// <returns>Returns the enumerator of the scanner collection</returns>
        ///
        public System.Collections.IEnumerator GetEnumerator()
        {
            for (int i = 0; i < Manager.Instance.numScanners; ++i)
            {
                yield return Manager.knownScanners[i];
            }
        }
    }
}