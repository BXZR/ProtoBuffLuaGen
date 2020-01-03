using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;

namespace Magic.GameEditor
{
    public static class StringUtility
    {
        private static StringBuilder m_StringBuilder = new StringBuilder(512);

        #region  StringBuilder.ConcatString

        public static string ConcatString<T1,T2>(T1 s1, T2 s2)
        {
            m_StringBuilder.Length = 0;
            m_StringBuilder.Append(s1);
            m_StringBuilder.Append(s2);
            return m_StringBuilder.ToString();
        }
        public static string ConcatString<T1,T2,T3 >(T1 s1, T2 s2, T3 s3)
        {
            m_StringBuilder.Length = 0;
            m_StringBuilder.Append(s1);
            m_StringBuilder.Append(s2);
            m_StringBuilder.Append(s3);
            return m_StringBuilder.ToString();
        }
        public static string ConcatString<T1, T2, T3,T4>(T1 s1, T2 s2, T3 s3, T4 s4)
        {
            m_StringBuilder.Length = 0;
            m_StringBuilder.Append(s1);
            m_StringBuilder.Append(s2);
            m_StringBuilder.Append(s3);
            m_StringBuilder.Append(s4);
            return m_StringBuilder.ToString();
        }
        public static string ConcatString<T1, T2, T3, T4,T5>(T1 s1, T2 s2, T3 s3, T4 s4, T5 s5)
        {
            m_StringBuilder.Length = 0;
            m_StringBuilder.Append(s1);
            m_StringBuilder.Append(s2);
            m_StringBuilder.Append(s3);
            m_StringBuilder.Append(s4);
            m_StringBuilder.Append(s5);
            return m_StringBuilder.ToString();
        }
        public static string ConcatString<T1, T2, T3, T4, T5,T6>(T1 s1, T2 s2, T3 s3, T4 s4, T5 s5,T6 s6)
        {
            m_StringBuilder.Length = 0;
            m_StringBuilder.Append(s1);
            m_StringBuilder.Append(s2);
            m_StringBuilder.Append(s3);
            m_StringBuilder.Append(s4);
            m_StringBuilder.Append(s5);
            m_StringBuilder.Append(s6);
            return m_StringBuilder.ToString();
        }
        public static string ConcatString<T1, T2, T3, T4, T5, T6,T7>(T1 s1, T2 s2, T3 s3, T4 s4, T5 s5, T6 s6, T7 s7)
        {
            m_StringBuilder.Length = 0;
            m_StringBuilder.Append(s1);
            m_StringBuilder.Append(s2);
            m_StringBuilder.Append(s3);
            m_StringBuilder.Append(s4);
            m_StringBuilder.Append(s5);
            m_StringBuilder.Append(s6);
            m_StringBuilder.Append(s7);
            return m_StringBuilder.ToString();
        }
        public static string ConcatString<T1, T2, T3, T4, T5, T6, T7,T8>(T1 s1, T2 s2, T3 s3, T4 s4, T5 s5, T6 s6, T7 s7, T8 s8)
        {
            m_StringBuilder.Length = 0;
            m_StringBuilder.Append(s1);
            m_StringBuilder.Append(s2);
            m_StringBuilder.Append(s3);
            m_StringBuilder.Append(s4);
            m_StringBuilder.Append(s5);
            m_StringBuilder.Append(s6);
            m_StringBuilder.Append(s7);
            m_StringBuilder.Append(s8);
            return m_StringBuilder.ToString();
        }
        public static string ConcatString<T1, T2, T3, T4, T5, T6, T7, T8,T9,T10,T11,T12,T13,T14,T15>(T1 s1, T2 s2, T3 s3, T4 s4, T5 s5, T6 s6, T7 s7, 
            T8 s8, T9 s9,T10 s10,T11 s11, T12 s12, T13 s13, T14 s14,T15 s15)
        {
            m_StringBuilder.Length = 0;
            m_StringBuilder.Append(s1);
            m_StringBuilder.Append(s2);
            m_StringBuilder.Append(s3);
            m_StringBuilder.Append(s4);
            m_StringBuilder.Append(s5);
            m_StringBuilder.Append(s6);
            m_StringBuilder.Append(s7);
            m_StringBuilder.Append(s8);
            m_StringBuilder.Append(s9);
            m_StringBuilder.Append(s10);
            m_StringBuilder.Append(s11);
            m_StringBuilder.Append(s12);
            m_StringBuilder.Append(s13);
            m_StringBuilder.Append(s14);
            m_StringBuilder.Append(s15);
            return m_StringBuilder.ToString();
        }

        //I18系列插件专用,其他不要用
        public static string CultureInfoFormString<T2>(CultureInfo s1, string s2, T2 t2)
        {
            m_StringBuilder.Length = 0;
            m_StringBuilder.AppendFormat(s1, s2, t2);
            return m_StringBuilder.ToString();
        }
        #endregion
    }
}
