using System.Text;

namespace KKtLib
{
    public class Text
    {
        public static string ToASCII(byte[] Array) => Encoding.ASCII.GetString(Array);
        public static string ToUTF8 (byte[] Array) => Encoding.UTF8 .GetString(Array);
        public static byte[] ToASCII(string Data ) => Encoding.ASCII.GetBytes (Data );
        public static byte[] ToUTF8 (string Data ) => Encoding.UTF8 .GetBytes (Data );
        public static byte[] ToASCII(char[] Data ) => Encoding.ASCII.GetBytes (Data );
        public static byte[] ToUTF8 (char[] Data ) => Encoding.UTF8 .GetBytes (Data );
    }
}
