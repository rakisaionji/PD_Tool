using System;
using System.IO;

namespace PROJECTDIVA_Tool
{
    class XDSC
    {
        public static void DSC(string file)
        {
            HexParser(file);
            Func.Decoder(file, PreParser(file), "X");
        }

        public static void HexParser (string file)
        {
            int DSCint = BitConverter.ToInt32(File.ReadAllBytes(file), 0);
            byte[] PreDSC = File.ReadAllBytes(file);
            string DSC = "", DSC1 = "", DSC1temp = "", DSC2 = "";
            int I1 = 0, I2 = 0;
            foreach (byte dscbyte in PreDSC)
            {
                DSC = dscbyte.ToString("X2") + DSC;
                I1++;
                if (I1 == 4)
                {
                    if (DSC == "00000006" && DSC1temp == "")
                        DSC1temp = DSC;
                    else if (DSC1temp != "")
                    {
                        if (Notes(DSC))
                            DSC1 += ("\r\n" + DSC1temp + " " + DSC + " ");
                        else
                            DSC1 += (DSC1temp + " " + DSC + " ");
                        DSC1temp = "";
                    }
                    else
                        DSC1 += (DSC + " ");
                    DSC = "";
                    I2++;
                    I1 = 0;

                    if (I2 == 4)
                    {
                        DSC2 += DSC1;
                        DSC1 = "";
                        I2 = 0;
                    }
                }
            }
            File.WriteAllText(Path.ChangeExtension(file, "temp"), DSC2);
        }
        
        public static bool Notes(string dsc)
        {
            switch (dsc)
            {
                case "00000000":
                    break;
                case "00000001":
                    break;
                case "00000002":
                    break;
                case "00000003":
                    break;
                case "00000004":
                    break;
                case "00000005":
                    break;
                case "00000006":
                    break;
                case "00000007":
                    break;
                case "00000008":
                    break;
                case "00000009":
                    break;
                case "0000000A":
                    break;
                case "0000000B":
                    break;
                case "0000000C":
                    break;
                case "0000000D":
                    break;
                case "0000000E":
                    break;
                case "0000000F":
                    break;
                case "00000011":
                    break;
                case "00000019":
                    break;
                case "0000001A":
                    break;
                case "0000001B":
                    break;
                case "0000001C":
                    break;
                default:
                    return false;
            }
            return true;
        }
        

        public static int PreParser(string file)
        {
            string[] dsc = File.ReadAllLines(Path.ChangeExtension(file, "temp"));
            File.Delete(Path.ChangeExtension(file, "temp"));
            int I = 0, I1 = 1;
            foreach (string DSC1 in dsc)
                I++;
            string[] DSC = new string[I];
            string dsctemp1 = "";
            foreach (string DSC1 in dsc)
            {
                if (I1 > I)
                    break;
                string dsctemp = DSC1.Replace(" ", "");
                string dsctemp2 = "";
                if (dsctemp.Contains("00000006"))
                    dsctemp2 = dsctemp.Remove(104);
                if (I1 > 0)
                    DSC[I1 - 1] = dsctemp1 + dsctemp2;
                dsctemp1 = dsctemp.Remove(0, dsctemp.Length - 16);
                if (I > I1)
                    for (; !dsctemp1.StartsWith("00000001");)
                    {
                        if (dsctemp1.Contains("00000056"))
                            dsctemp1 = dsctemp.Remove(0, dsctemp.Length - 32).Remove(16);
                        if (dsctemp1.Contains("00000055"))
                            dsctemp1 = dsctemp.Remove(0, dsctemp.Length - 48).Remove(16);
                    }
                else
                    dsctemp1 = dsctemp.Remove(0, dsctemp.Length - 104).Remove(16);
                I1++;
            }

            File.WriteAllLines(Path.ChangeExtension(file, "hex"), DSC);
            return I;
        }
    }
}
