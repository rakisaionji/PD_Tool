using System;
using System.IO;
using System.Globalization;

namespace PROJECTDIVA_Tool
{
    class XDSC
    {
        public static void DSC(string file)
        {
            HexParser(file);
            Decoder(file, PreParser(file));
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
                    {
                        DSC1temp = DSC;
                    }
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
        
        public static void Decoder(string file, int I)
        {
            string[] dsc = File.ReadAllLines(Path.ChangeExtension(file, "hex"));
            //File.Delete(Path.ChangeExtension(file, ".txt"));

            //uint[] StartID = new uint[I];
            double[] Timestamp = new double[I];
            //uint[] StructOpcode = new uint[I];
            uint[] NoteType = new uint[I];
            double[] HoldTimer = new double[I];
            int[] HoldEnd = new int[I];
            double[] XPos = new double[I];
            double[] YPos = new double[I];
            double[] Angle1 = new double[I];
            int[] WaveCount = new int[I];
            double[] NoteTimer = new double[I];
            uint[] Curve = new uint[I];
            double[] NoteSpeed = new double[I];
            //uint[] UnknownVariable = new uint[I];
            //int[] EndInt = new int[I];
            int I1 = 0;
            string notes = ",timestamp,type,holdLength,bIsHoldEnd,posX,posY,entryAngle,oscillationFrequency,oscillationAngle," +
                "oscillationAmplitude,timeout";

            foreach (string DSC in dsc)
            {
                if (DSC.StartsWith("00000001"))
                {
                    //StartID[I1] = uint.Parse(DSC.Remove(8), NumberStyles.HexNumber);
                    Timestamp[I1] = Convert.ToDouble(uint.Parse(DSC.Remove(0, 8).Remove(8), NumberStyles.HexNumber)) / 100000;
                    //StructOpcode[I1] = uint.Parse(DSC.Remove(0, 16).Remove(8), NumberStyles.HexNumber);
                    NoteType[I1] = uint.Parse(DSC.Remove(0, 24).Remove(8), NumberStyles.HexNumber);
                    HoldTimer[I1] = Convert.ToDouble(int.Parse(DSC.Remove(0, 32).Remove(8), NumberStyles.HexNumber));
                    HoldEnd[I1] = int.Parse(DSC.Remove(0, 40).Remove(8), NumberStyles.HexNumber);
                    XPos[I1] = Convert.ToDouble(uint.Parse(DSC.Remove(0, 48).Remove(8), NumberStyles.HexNumber)) / 10000;
                    YPos[I1] = Convert.ToDouble(uint.Parse(DSC.Remove(0, 56).Remove(8), NumberStyles.HexNumber)) / 10000;
                    Angle1[I1] = Convert.ToDouble(int.Parse(DSC.Remove(0, 64).Remove(8), NumberStyles.HexNumber)) / 100000;
                    WaveCount[I1] = int.Parse(DSC.Remove(0, 72).Remove(8), NumberStyles.HexNumber);
                    NoteTimer[I1] = Convert.ToDouble(int.Parse(DSC.Remove(0, 80).Remove(8), NumberStyles.HexNumber)) / 100000;
                    Curve[I1] = uint.Parse(DSC.Remove(0, 88).Remove(8), NumberStyles.HexNumber);
                    NoteSpeed[I1] = Convert.ToDouble(uint.Parse(DSC.Remove(0, 96).Remove(8), NumberStyles.HexNumber)) / 1000;

                    int tempI1 = I1 + 1;

                    if (HoldTimer[I1] != -1)
                        HoldTimer[I1] = HoldTimer[I1] / 1000000;

                    Console.WriteLine("{0},{1},{2},{3},{4},{5},{6},{7},{8},{9},{10},{11}",
                        I1, Timestamp[I1], NoteType[I1], HoldTimer[I1], HoldEnd[I1], XPos[I1], YPos[I1], Angle1[I1],
                        WaveCount[I1], NoteTimer[I1], Curve[I1], NoteSpeed[I1]);
                    notes += "\r\n" + tempI1 + "," + Timestamp[I1] + "," + NoteType[I1] + "," + HoldTimer[I1] + "," +
                        HoldEnd[I1] + "," + XPos[I1] + "," + YPos[I1] + "," + Angle1[I1] + "," + WaveCount[I1] + "," +
                        NoteTimer[I1] + "," + Curve[I1] + "," + NoteSpeed[I1];
                    I1++;
                }
            }
            File.WriteAllText(Path.ChangeExtension(file, "csv"), notes);
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
            Console.WriteLine("{0} notes", I - 1);
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
