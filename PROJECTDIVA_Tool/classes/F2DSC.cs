using System;
using System.IO;
using System.Globalization;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PROJECTDIVA_Tool
{
    class F2DSC
    {
        public static void DSC(string file)
        {
            Decoder(file, HexParser(file));
        }

        public static void Decoder(string file, int I)
        {
            Console.WriteLine("{0} notes", I);
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
            int[] Angle2 = new int[I];
            uint[] Amplitude = new uint[I];
            uint[] NoteTimer = new uint[I];
            uint[] UnknownVariable = new uint[I];
            int[] EndInt = new int[I];
            int I1 = 0;
            string notes = "#Note: StartID, Timestamp, StructOpcode, NoteType, HoldTimer, HoldEnd, XPos, YPos, Angle1, " +
                "WaveCount, Angle2, Amplitude, NoteTimer, UnknownVariable, EndInt"; 

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
                Angle2[I1] = int.Parse(DSC.Remove(0, 80).Remove(8), NumberStyles.HexNumber);
                Amplitude[I1] = uint.Parse(DSC.Remove(0, 88).Remove(8), NumberStyles.HexNumber);
                NoteTimer[I1] = uint.Parse(DSC.Remove(0, 96).Remove(8), NumberStyles.HexNumber);
                UnknownVariable[I1] = uint.Parse(DSC.Remove(0, 104).Remove(8), NumberStyles.HexNumber);
                EndInt[I1] = int.Parse(DSC.Remove(0, 112), NumberStyles.HexNumber);
                Console.WriteLine("Note {0}: {1}, {2}, {3}, {4}, {5}, {6}, {7}, {8}, {9}, {10}, {11}, {12}, {13}, {14}, {15}",
                    I1, StartID[I1], Timestamp[I1], StructOpcode[I1], NoteType[I1], HoldTimer[I1], HoldEnd[I1], XPos[I1],
                    YPos[I1], Angle1[I1], WaveCount[I1], Angle2[I1], Amplitude[I1], NoteTimer[I1], UnknownVariable[I1],
                    EndInt[I1]);
                notes += "\nNote" + I1 + ": " + StartID[I1] + ", " + Timestamp[I1] + ", " + StructOpcode[I1] + ", " +
                    NoteType[I1] + ", " + HoldTimer[I1] + ", " + HoldEnd[I1] + ", " + XPos[I1] + ", " + YPos[I1] +", " +
                    Angle1[I1] + ", " + WaveCount[I1] + ", " + Angle2[I1] + ", " + Amplitude[I1] + ", " + NoteTimer[I1] + 
                    ", " + UnknownVariable[I1] + ", " + EndInt[I1];
                I1++;
            }
            File.WriteAllText(Path.ChangeExtension(file, "out.txt"), notes);
        }

        public static int HexParser(string file)
        {
            int DSCint = BitConverter.ToInt32(File.ReadAllBytes(file), 0);
            byte[] PreDSC = File.ReadAllBytes(file);
            string DSC = "", DSC1 = "", DSC2 = "";
            int I = 0, I1 = 0, I2 = 0, I3 = 0;
            foreach (byte dscbyte in PreDSC)
            {
                if (I > 71)
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
                            DSC2 += (DSC1 + "\n");
                            DSC1 = "";
                            I2 = 0;
                            I3++;
                        }
                    }
                }
                else
                    I++;
            }
            File.WriteAllText(Path.ChangeExtension(file, ".txt"), DSC2.Remove(DSC2.Length - 1).Replace(" ", ""));
            return I3;
        }
    }
}
