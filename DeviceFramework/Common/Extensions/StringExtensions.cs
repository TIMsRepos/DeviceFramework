using System.Text;

namespace TIM.Devices.Framework.Common.Extensions
{
    public static class StringExtensions
    {
        public static string Divide(this string strText, int intMaxLength, string strDivider)
        {
            return Divide(strText, intMaxLength, strDivider, new char[] { ' ', '.', ',', '-', '/', '!', '?', ')', ']', '>', '}' });
        }

        public static string Divide(this string strText, int intMaxLength, string strDivider, char[] chrTokens)
        {
            StringBuilder sbBuilder = new StringBuilder();
            int intLastTokenIdx = 0;
            int i, k;
            for (i = 0; i < strText.Length; ++i)
            {
                for (k = 0; k < chrTokens.Length; ++k)
                {
                    if (strText[i] == chrTokens[k])
                    {
                        if (strText.IndexOfAny(chrTokens, i + 1) - intLastTokenIdx > intMaxLength - 1)
                        {
                            sbBuilder.Append(strText.Substring(intLastTokenIdx, i - intLastTokenIdx + 1).Trim());
                            sbBuilder.Append(strDivider);
                            intLastTokenIdx = i + 1;
                            break;
                        }
                        else if (strText.IndexOfAny(chrTokens, i + 1) < 0)
                        {
                            if (strText.Length - i <= intMaxLength)
                            {
                                sbBuilder.Append(strText.Substring(intLastTokenIdx, strText.Length - intLastTokenIdx).Trim());
                                i = strText.Length;
                            }
                            else
                            {
                                sbBuilder.Append(strText.Substring(intLastTokenIdx, i - intLastTokenIdx + 1).Trim());
                                sbBuilder.Append(strDivider);
                                intLastTokenIdx = i + 1;
                            }
                            break;
                        }
                    }
                }
            }
            return sbBuilder.ToString();
        }
    }
}