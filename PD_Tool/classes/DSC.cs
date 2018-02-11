using System;
using System.IO;

namespace PROJECTDIVA_Tool
{
    class DSC
    {
        public static void DSCParser(string file, string ver)
        {
            Func.Decoder(file, HexParser(file, ver), ver);
        }

        public static int HexParser(string file, string ver)
        {
            byte[] PreDSC = File.ReadAllBytes(file);
            int I = 0, I1 = 0;
            if (ver == "F2")
            {
                string DSC = "", DSC1 = "", DSC2 = "";
                int I2 = 0, I3 = 0;
                foreach (byte dscbyte in PreDSC)
                    if (I3 > 71)
                    {
                        DSC += dscbyte.ToString("X2");
                        I1++;
                        if (I1 == 4)
                        {
                            DSC1 += (DSC + " ");
                            DSC = "";
                            I2++;
                            I1 = 0;
                            if (I2 == 15)
                            {
                                DSC2 += (DSC1 + "\r\n");
                                DSC1 = "";
                                I2 = 0;
                                I++;
                            }
                        }
                    }
                    else
                        I3++;
                File.WriteAllText(Path.ChangeExtension(file, "hex"), DSC2.Remove(DSC2.Length - 2).Replace(" ", ""));
            }
            else if (ver == "X")
            {
                string DSC = "", DSC2 = "", DSC3 = "", DSC3temp = "";
                foreach (byte dscbyte in PreDSC)
                {
                    DSC = dscbyte.ToString("X2") + DSC;
                    I++;
                    if (I == 4)
                    {
                        if (DSC == "00000006" && DSC3temp == "")
                            DSC3temp = DSC;
                        else if (DSC3temp != "")
                        {
                            if (Notes(DSC))
                                DSC3 += ("\r\n" + DSC3temp + " " + DSC + " ");
                            else
                                DSC3 += (DSC3temp + " " + DSC + " ");
                            DSC3temp = "";
                        }
                        else
                            DSC3 += (DSC + " ");
                        DSC = "";
                        I1++;
                        I = 0;

                        if (I1 == 4)
                        {
                            DSC2 += DSC3;
                            DSC3 = "";
                            I1 = 0;
                        }
                    }
                }
                File.WriteAllText(Path.ChangeExtension(file, "temp"), DSC2);
                string[] dsc = File.ReadAllLines(Path.ChangeExtension(file, "temp"));
                File.Delete(Path.ChangeExtension(file, "temp"));
                I = 0;
                I1 = 1;
                foreach (string DSC1 in dsc)
                    I++;
                DSC = "";
                string dsctemp1 = "";
                foreach (string DSC1 in dsc)
                {
                    if (I1 > I)
                        break;
                    string dsctemp = DSC1.Replace(" ", "");
                    string dsctemp2 = "";
                    if (dsctemp.Contains("00000006"))
                        dsctemp2 = dsctemp.Remove(104);
                    string DSCtemp = dsctemp1 + dsctemp2;
                    if (I1 > 0 && DSCtemp != "")
                        DSC += (DSCtemp + "\r\n");
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
                dsc = null;
                File.WriteAllText(Path.ChangeExtension(file, "hex"), DSC.Remove(DSC.Length - 2));
            }
            PreDSC = null;
            GC.Collect();
            return I;
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
    }
}
