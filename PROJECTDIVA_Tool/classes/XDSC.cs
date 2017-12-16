using System;
using System.IO;
using System.Globalization;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PROJECTDIVA_Tool
{
    class XDSC
    {
        public static void DSC(string file)
        {
            HexParser(file);
            Decoder(file, PreParser(file));
            Console.Read();
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
                            DSC1 += ("\n" + DSC1temp + " " + DSC + " ");
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
            File.WriteAllText(Path.ChangeExtension(file, ".temp"), DSC2);
            Functions(file, ".temp");
        }
        
        public static void Decoder(string file, int I)
        {
            string[] dsc = File.ReadAllLines(Path.ChangeExtension(file, ".txt"));
            //File.Delete(Path.ChangeExtension(file, ".txt"));

            uint[] StartID = new uint[I];
            uint[] Timestamp = new uint[I];
            uint[] StructOpcode = new uint[I];
            uint[] NoteType = new uint[I];
            int[] HoldTimer = new int[I];
            int[] HoldEnd = new int[I];
            uint[] XPos = new uint[I];
            uint[] YPos = new uint[I];
            int[] Angle1 = new int[I];
            int[] WaveCount = new int[I];
            int[] NoteTimer = new int[I];
            uint[] Curve = new uint[I];
            uint[] NoteSpeed = new uint[I];
            uint[] UnknownVariable = new uint[I];
            int[] EndInt = new int[I];
            int I1 = 0;
            string notes = "#Note: StartID, Timestamp, StructOpcode, NoteType, HoldTimer, HoldEnd, XPos, YPos, Angle1, " +
                "WaveCount, NoteTimer, Curve, NoteSpeed, UnknownVariable, EndInt"; 

            foreach (string DSC in dsc)
            {
                StartID[I1] = uint.Parse(DSC.Remove(8), NumberStyles.HexNumber);
                Timestamp[I1] = uint.Parse(DSC.Remove(0, 8).Remove(8), NumberStyles.HexNumber);
                StructOpcode[I1] = uint.Parse(DSC.Remove(0, 16).Remove(8), NumberStyles.HexNumber);
                NoteType[I1] = uint.Parse(DSC.Remove(0, 24).Remove(8), NumberStyles.HexNumber);
                HoldTimer[I1] = int.Parse(DSC.Remove(0, 32).Remove(8), NumberStyles.HexNumber);
                HoldEnd[I1] = int.Parse(DSC.Remove(0, 40).Remove(8), NumberStyles.HexNumber);
                XPos[I1] = uint.Parse(DSC.Remove(0, 48).Remove(8), NumberStyles.HexNumber);
                YPos[I1] = uint.Parse(DSC.Remove(0, 56).Remove(8), NumberStyles.HexNumber);
                Angle1[I1] = int.Parse(DSC.Remove(0, 64).Remove(8), NumberStyles.HexNumber);
                WaveCount[I1] = int.Parse(DSC.Remove(0, 72).Remove(8), NumberStyles.HexNumber);
                NoteTimer[I1] = int.Parse(DSC.Remove(0, 80).Remove(8), NumberStyles.HexNumber);
                Curve[I1] = uint.Parse(DSC.Remove(0, 88).Remove(8), NumberStyles.HexNumber);
                NoteSpeed[I1] = uint.Parse(DSC.Remove(0, 96).Remove(8), NumberStyles.HexNumber);
                UnknownVariable[I1] = uint.Parse(DSC.Remove(0, 104).Remove(8), NumberStyles.HexNumber);
                EndInt[I1] = int.Parse(DSC.Remove(0, 112), NumberStyles.HexNumber);
                Console.WriteLine("Note {0}: {1}, {2}, {3}, {4}, {5}, {6}, {7}, {8}, {9}, {10}, {11}, {12}, {13}, {14}, {15}",
                    I1, StartID[I1], Timestamp[I1], StructOpcode[I1], NoteType[I1], HoldTimer[I1], HoldEnd[I1], XPos[I1],
                    YPos[I1], Angle1[I1], WaveCount[I1], NoteTimer[I1], Curve[I1], NoteSpeed[I1], UnknownVariable[I1],
                    EndInt[I1]);
                notes += "\nNote" + I1 + ": " + StartID[I1] + ", " + Timestamp[I1] + ", " + StructOpcode[I1] + ", " +
                    NoteType[I1] + ", " + HoldTimer[I1] + ", " + HoldEnd[I1] + ", " + XPos[I1] + ", " + YPos[I1] +", " +
                    Angle1[I1] + ", " + WaveCount[I1] + ", " + NoteTimer[I1] + ", " + Curve[I1] + ", " + NoteSpeed[I1] + 
                    ", " + UnknownVariable[I1] + ", " + EndInt[I1];
                I1++;
            }
            File.WriteAllText(Path.ChangeExtension(file, "out.txt"), notes);
        }

        public static void Functions(string file, string code)
        {
            string tempdsc = BitConverter.ToString(File.ReadAllBytes(Path.ChangeExtension(file, ".temp")));
            File.Delete(Path.ChangeExtension(file, ".temp"));
            tempdsc = tempdsc.Replace("-", "");
            if (code == ".temp")
                tempdsc = tempdsc.Replace("200A", "200D0A");
            else if (code == ".txt")
                tempdsc = tempdsc.Remove(0, 4).Remove(tempdsc.Length - 8);
            byte[] bytes = new byte[tempdsc.Length / 2];
            for (int i = 0; i < tempdsc.Length; i += 2)
                bytes[i / 2] = Convert.ToByte(tempdsc.Substring(i, 2), 16);
            File.WriteAllBytes(Path.ChangeExtension(file, code), bytes);
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
            string[] dsc = File.ReadAllLines(Path.ChangeExtension(file, ".temp"));
            File.Delete(Path.ChangeExtension(file, ".temp"));
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
            File.WriteAllLines(Path.ChangeExtension(file, ".temp"), DSC);
            Functions(file, ".txt");
            return I;
        }

    }
}
