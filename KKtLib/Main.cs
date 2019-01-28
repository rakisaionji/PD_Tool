using System;
using System.Windows.Forms;
using System.Globalization;
using System.Collections.Generic;

namespace KKtLib
{
    public unsafe class Main
    {
        public static int NumWorkers = Math.Min(Environment.ProcessorCount, 0x100);
        
        public static void ConsoleDesign(string text)
        {
            string Text = "█                                                  █";
            if (text == "Fill")
                Text = "████████████████████████████████████████████████████";
            else
                Text = "█  " + text + Text.Remove(0, text.Length + 3);
            Console.WriteLine(Text);
        }

        public static void ConsoleDesign(bool Fill)
        {
            if (Fill)
                ConsoleDesign("Fill");
            else
                ConsoleDesign("");
        }

        public static string TimeFormatHHmmssfff = "{0:d2}:{1:d2}:{2:d2}.{3:d3}";

        public static void WriteTime(TimeSpan time, bool WriteLine)
        {
            if (WriteLine)
                Console.WriteLine(TimeFormatHHmmssfff,
                    time.Hours, time.Minutes, time.Seconds, time.Milliseconds);
            else
                Console.Write    (TimeFormatHHmmssfff,
                    time.Hours, time.Minutes, time.Seconds, time.Milliseconds);
        }

        public static void WriteTime(TimeSpan time, bool WriteLine, string Text)
        {
            if (WriteLine)
                Console.WriteLine(TimeFormatHHmmssfff + " - " + Text,
                    time.Hours, time.Minutes, time.Seconds, time.Milliseconds);
            else
                Console.Write    (TimeFormatHHmmssfff + " - " + Text,
                    time.Hours, time.Minutes, time.Seconds, time.Milliseconds);
        }

        public static string Choose(int code, string filetype, out string[] FileNames) =>
            Choose(code, filetype, out string InitialDirectory, out FileNames);

        public static string Choose(int code, string filetype, out string InitialDirectory, out string[] FileNames)
        {
            InitialDirectory = "";
            FileNames = new string[0];
            if (code == 1)
            {
                Console.WriteLine("Choose file(s) to open:");
                OpenFileDialog ofd = new OpenFileDialog { InitialDirectory =
                    Application.StartupPath, Multiselect = true };
                switch (filetype)
                {
                    case "a3da":
                        ofd.Filter = "A3DA files (*.a3da)|*.a3da";
                        break;
                    case "bin":
                        ofd.Filter = "BIN files (*.bin, *.xml)|*.bin;*.xml|" +
                            "BIN files (*.bin)|*.bin|XML files (*.xml)|*.xml";
                        break;
                    case "bon":
                        ofd.Filter = "BON files (*.bon, *.bin, *.xml)|*.bon;*.bin;*.xml|BON files " +
                            "(*.bon)|*.bon|BIN files (*.bin)|*.bin|XML files (*.xml)|*.xml";
                        break;
                    case "diva":
                        ofd.Filter = "DIVA files (*.diva, *.wav)|*.diva;*.wav|DIVA files " +
                            "(*.diva)|*.diva|WAV files (*.wav)|*.wav";
                        break;
                    case "dsc":
                        ofd.Filter = "DSC files (*.dsc, *.xml)|*.dsc;*.xml|" +
                            "DSC files (*.dsc)|*.dsc|XML files (*.xml)|*.xml";
                        break;
                    case "farc":
                        ofd.Filter = "FARC Archives (*.farc)|*.farc";
                        break;
                    case "image":
                        ofd.Filter = "Image files (*.dds, *.gif, *.png, *.tif, *.tiff)|*.dds;*.png;*.tif;*.tiff|" +
                            "DDS files (*.dds)|*.dds|PNG files (*.png)|*.png|TIFF files (*.tif, *.tiff)|*.tif;*.tiff";
                        break;
                    case "kki":
                        ofd.Filter = "KKI files (*.kki)|*.kki";
                        break;
                    case "model":
                        ofd.Filter = "Model files (*.bin, *.osd, *.osi, *.txd, *txi, *.xml)|" +
                            "*.bin;*.osd;*.osi;*.txd;*txi;*.xml|BIN files (*.bin)|*.bin|" +
                            "OSD files (*.osd)|*.osd|OSI files (*.osi)|*.osi|TXD files (*.txd)|" +
                            "*.txd|TXI files (*.txi)|*.txi|XML files (*.xml)|*.xml";
                        break;
                    case "mot":
                        ofd.Filter = "MOT files (*.mot, *.xml)|*.mot;*.xml|" +
                            "MOT files (*.mot)|*.mot|XML files (*.xml)|*.xml";
                        break;
                    case "osd":
                        ofd.Filter = "OSD files (*.osd)|*.osd";
                        break;
                    case "osi":
                        ofd.Filter = "OSI files (*.osi)|*.osi";
                        break;
                    case "png":
                        ofd.Filter = "PNG files (*.png)|*.png";
                        break;
                    case "ppd":
                        ofd.Filter = "PPD files (*.ppd, *.pak, *.mod)|*.ppd;*.pak;*.mod|PPD files " +
                            "(*.ppd)|*.ppd|BIN files (*.pak)|*.pak|XML files (*.mod)|*.mod";
                        break;
                    case "str":
                        ofd.Filter = "STR files (*.str, *.bin, *.xml)|*.str;*.bin;*.xml|STR files " +
                            "(*.str)|*.str|BIN files (*.bin)|*.bin|XML files (*.xml)|*.xml";
                        break;
                    case "txd":
                        ofd.Filter = "TXD files (*.txd)|*.txd";
                        break;
                    case "txi":
                        ofd.Filter = "TXI files (*.txi)|*.txi";
                        break;
                    case "vag":
                        ofd.Filter = "VAG files (*.vag)|*.vag";
                        break;
                    case "xml":
                        ofd.Filter = "XML files (*.xml)|*.xml";
                        break;
                    default:
                        ofd.Filter = "All files (*.*)|*.*";
                        break;
                }
                if (ofd.ShowDialog() == DialogResult.OK)
                {
                    InitialDirectory = ofd.InitialDirectory.ToString();
                    FileNames = ofd.FileNames;
                }
            }
            else if (code == 2)
            {
                FolderBrowserDialog fbd = new FolderBrowserDialog();
                Console.WriteLine("Choose folder:");
                fbd.SelectedPath = Application.StartupPath;
                if (fbd.ShowDialog() == DialogResult.OK)
                    return fbd.SelectedPath.ToString();
            }
            return "";
        }

        public static void ChooseSave(int code, string filetype, out string InitialDirectory, out string[] FileNames)
        {
            InitialDirectory = "";
            FileNames = new string[0];
            Console.WriteLine("Choose file to save:");
            SaveFileDialog sfd = new SaveFileDialog { InitialDirectory = Application.StartupPath };
            switch (filetype)
            {
                case "kki":
                    sfd.Filter = "KKI file (*.kki)|*.kki";
                    break;
            }
            if (sfd.ShowDialog() == DialogResult.OK)
            {
                InitialDirectory = sfd.InitialDirectory.ToString();
                FileNames = sfd.FileNames;
            }
        }

        public static POF AddPOF(Header Header)
        {
            POF POF = new POF
            {
                Offsets = new List<int>(),
                POFOffsets = new List<long>(),
                Offset = Header.DataSize + Header.Lenght
            };
            return POF;
        }

        public static string ToString(float? d, byte round)
        { if (d == null) return null; return ToString((float)d, round); }

        public static string ToString(float? d)
        { if (d == null) return null; return ToString((float)d); }

        public static string ToString(float d, byte round)
        { return Math.Round(d, round).ToString().ToLower().Replace(
            NumberFormatInfo.CurrentInfo.NumberDecimalSeparator, "."); }

        public static string ToString(float d)
        { return            d      .ToString().ToLower().Replace(
            NumberFormatInfo.CurrentInfo.NumberDecimalSeparator, "."); }

        public static string ToString(double? d, byte round)
        { if (d == null) return null; return ToString((double)d, round); }

        public static string ToString(double? d)
        { if (d == null) return null; return ToString((double)d); }

        public static string ToString(double d, byte round)
        { return Math.Round(d, round).ToString().ToLower().Replace(
            NumberFormatInfo.CurrentInfo.NumberDecimalSeparator, "."); }

        public static string ToString(double d)
        { return            d      .ToString().ToLower().Replace(
            NumberFormatInfo.CurrentInfo.NumberDecimalSeparator, "."); }

        public static  float ToSingle(string s)
        { return  float.   Parse(s.Replace(".", NumberFormatInfo.
            CurrentInfo.NumberDecimalSeparator)); }

        public static   bool ToSingle(string s, out float? value)
        { bool Val = ToSingle(s, out  float val); value = val; return Val; }

        public static   bool ToSingle(string s, out float value)
        { return  float.TryParse(s.Replace(".", NumberFormatInfo.
            CurrentInfo.NumberDecimalSeparator), out value); }

        public static double ToDouble(string s)
        { return double.   Parse(s.Replace(".", NumberFormatInfo.
            CurrentInfo.NumberDecimalSeparator)); }

        public static   bool ToDouble(string s, out double? value)
        { bool Val = ToDouble(s, out double val); value = val; return Val; }

        public static   bool ToDouble(string s, out double value)
        { return double.TryParse(s.Replace(".", NumberFormatInfo.
            CurrentInfo.NumberDecimalSeparator), out value); }
        
        public static string NullTerminated(string Source, ref int i, byte End)
        {
            string s = "";
            while (true)
            {
                byte a = (byte)Source[i];
                i++;
                if (a == End)
                    break;
                else
                    s += Convert.ToChar(a);
            }
            return s;
        }

        public static void FloorCeiling(ref double Value)
        {
            if (Value % 1 >= 0.5)
                Value = (long)(Value + 0.5);
            else
                Value = (long)(Value);
        }

        public static double FloorCeiling(double Value)
        {
            if (Value % 1 >= 0.5)
                return (long)(Value + 0.5);
            else
                return (long)(Value);
        }

        public static long Align(long value, long alignement)
        { return (value % alignement == 0) ? value : (value + alignement - value % alignement); }

        public static bool StartsWith(Dictionary<string, object> Dict, string args, char Split)
        { return StartsWith(Dict, args.Split(Split)); }

        public static bool StartsWith(Dictionary<string, object> Dict, string args)
        { return StartsWith(Dict, args.Split('.')); }

        public static bool StartsWith(Dictionary<string, object> Dict, string[] args)
        {
            Dictionary<string, object> bufDict = new Dictionary<string, object>();
            if (Dict == null)
                Dict = new Dictionary<string, object>();
            if (args.Length > 1)
            {
                string[] NewArgs = new string[args.Length - 1];
                for (int i = 0; i < args.Length - 1; i++)
                    NewArgs[i] = args[i + 1];
                if (!Dict.ContainsKey(args[0]))
                    return false;
                bufDict = (Dictionary<string, object>)Dict[args[0]];
                return StartsWith(bufDict, NewArgs);
            }
            return Dict.ContainsKey(args[0]);
        }

        public static bool FindValue(Dictionary<string, object> Dict, string args, ref   bool value, char Split)
        { if (FindValue(Dict, args.Split(Split), out string val))
                return bool.TryParse(val, out value);   return false; }

        public static bool FindValue(Dictionary<string, object> Dict, string args, ref    int value, char Split)
        { if (FindValue(Dict, args.Split(Split), out string val))
                return  int.TryParse(val, out value);   return false; }

        public static bool FindValue(Dictionary<string, object> Dict, string args, ref double value, char Split)
        { if (FindValue(Dict, args.Split(Split), out string val))
                return      ToDouble(val, out value);   return false; }

        public static bool FindValue(Dictionary<string, object> Dict, string args, ref string value, char Split)
        { if (FindValue(Dict, args.Split(Split), out string val))
            { value = val;           return true; } return false; }

        public static bool FindValue(Dictionary<string, object> Dict, string args, ref   bool value)
        { if (FindValue(Dict, args.Split('.'  ), out string val))
                return bool.TryParse(val, out value);   return false; }

        public static bool FindValue(Dictionary<string, object> Dict, string args, ref    int value)
        { if (FindValue(Dict, args.Split('.'  ), out string val))
                return  int.TryParse(val, out value);   return false; }

        public static bool FindValue(Dictionary<string, object> Dict, string args, ref double value)
        { if (FindValue(Dict, args.Split('.'  ), out string val))
                return      ToDouble(val, out value);   return false; }

        public static bool FindValue(Dictionary<string, object> Dict, string args, ref    int? value)
        { if (FindValue(Dict, args.Split('.'), out string val)) { bool Val =  int.TryParse(val,
            out  int _value); value = _value; return Val; }  return false; }

        public static bool FindValue(Dictionary<string, object> Dict, string args, ref double? value)
        { if (FindValue(Dict, args.Split('.'  ), out string val))
                return      ToDouble(val, out value);   return false; }

        public static bool FindValue(Dictionary<string, object> Dict, string args, ref string value)
        { if (FindValue(Dict, args.Split('.'  ), out string val))
            { value = val;           return true; } return false; }

        public static bool FindValue(Dictionary<string, object> Dict, string[] args, out string value)
        {
            value = "";
            Dictionary<string, object> bufDict = new Dictionary<string, object>();
            if (Dict == null)
                Dict = new Dictionary<string, object>();
            if (args.Length > 1)
            {
                string[] NewArgs = new string[args.Length - 1];
                for (int i = 0; i < args.Length - 1; i++)
                    NewArgs[i] = args[i + 1];
                if (!Dict.ContainsKey(args[0]))
                {
                    value = "";
                    return false;
                }
                bufDict = (Dictionary<string, object>)Dict[args[0]];
                return FindValue(bufDict, NewArgs, out value);
            }
            if (!Dict.ContainsKey(args[0]))
                return false;
            if (args.Length == 1)
                if (Dict[args[0]].GetType() == Dict.GetType())
                    return FindValue(bufDict, args, out value);
                else if (Dict[args[0]].GetType() != typeof(string))
                    return false;

            value = (string)Dict[args[0]];
            return true;
        }
        
        public static void GetDictionary(ref Dictionary<string, object> Dict,
            string args, string value, char Split)
        { GetDictionary(ref Dict, args.Split(Split), value); }

        public static void GetDictionary(ref Dictionary<string, object> Dict, string args, string value)
        { GetDictionary(ref Dict, args.Split('.'), value); }

        public static void GetDictionary(ref Dictionary<string, object> Dict, string[] args, string value)
        {
            Dictionary<string, object> bufDict = new Dictionary<string, object>();
            if (Dict == null)
                Dict = new Dictionary<string, object>();
            if (args.Length > 1)
            {
                string[] NewArgs = new string[args.Length - 1];
                for (int i = 0; i < args.Length - 1; i++)
                    NewArgs[i] = args[i + 1];
                if (!Dict.ContainsKey(args[0]))
                    Dict.Add(args[0], null);
                if (Dict[args[0]] != null)
                    if (Dict[args[0]].GetType() == typeof(string))
                        Dict[args[0]] = new Dictionary<string, object> { { "", Dict[args[0]] } };
                bufDict = (Dictionary<string, object>)Dict[args[0]];
                GetDictionary(ref bufDict, NewArgs, value);
                Dict[args[0]] = bufDict;
            }
            else if (!Dict.ContainsKey(args[0]))
                Dict.Add(args[0], value);
        }

        public static string ToTitleCase(string s)
        { return CultureInfo.CurrentCulture.TextInfo.ToTitleCase(s); }
        
        public static string   ToBase64(double Data) => Convert.  ToBase64String(BitConverter.GetBytes(Data));
        public static string   ToBase64(byte[] Data) => Convert.  ToBase64String(Data);
        public static byte[] FromBase64(string Data) => Convert.FromBase64String(Data);

        public struct WAVHeader
        {
            public int Size;
            public int Channels;
            public int SampleRate;
            public int HeaderSize;
            public int ChannelMask;
            public bool IsSupported;
            public short Bytes;
            public ushort Format;
        }

        public struct Header
        {
            public bool IsX;
            public bool IsBE;
            public int ID;
            public int Lenght;
            public int DataSize;
            public int Signature;
            public int SectionSize;
            public int ActualSignature;
            public Format Format;
        }

        public enum Format : byte
        {
            NULL =  0,
            DT   =  1,
            DT2  =  2,
            DTe  =  3,
            F    =  4,
            FT   =  5,
            F2LE =  6,
            F2BE =  7,
            X    =  8,
            XHD  =  9,
            MGF  = 10,
        }

        public struct POF
        {
            public byte Type;
            public int Lenght;
            public int Offset;
            public int LastOffset;
            public List<int> Offsets;
            public List<long> POFOffsets;
            public Header Header;
        }

        public struct ThreadArgs
        {
            public int Thread;
            public int ID;
        }
    }    
}
