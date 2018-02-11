using System;
using System.IO;
using System.Globalization;

namespace PROJECTDIVA_Tool
{
    public class Func
    {
        public static void Decoder(string file, int I, string ver)
        {

            if (ver == "X")
                Console.WriteLine("{0} notes", I - 1);
            else
                Console.WriteLine("{0} notes", I - 1);
            string[] dsc = File.ReadAllLines(Path.ChangeExtension(file, "hex"));

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
            double[] Angle2 = new double[I];
            uint[] Amplitude = new uint[I];
            double[] NoteTimer = new double[I];
            int I1 = 0;
            string notes = ",timestamp,type,holdLength,bIsHoldEnd,posX,posY,entryAngle,oscillationFrequency,oscillationAngle," +
                "oscillationAmplitude,timeout";

            foreach (string DSCin in dsc)
                if (DSCin.StartsWith("00000001") || DSCin.StartsWith("01000000"))
                {
                    string DSC = "", DSCout = "";
                    bool BigEndian = false;
                    DSC = DSCin;
                    if (ver == "F2")
                    {
                        for (int i = 1; i < 16; i++)
                        {
                            string DSCtemp = DSCin + "00000000";
                            if (i == 1)
                                DSCtemp = DSCtemp.Remove(8);
                            else
                                DSCtemp = DSCtemp.Remove(0, 8 * (i - 1)).Remove(8);

                            if (DSCtemp.Contains("01000000") && !BigEndian)
                                BigEndian = true;

                            if (BigEndian)
                                DSCtemp = BEtoLE(DSCtemp, 4);

                            DSCout += DSCtemp;
                        }
                        DSC = DSCout;
                    }

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
                    Angle2[I1] = Convert.ToDouble(int.Parse(DSC.Remove(0, 80).Remove(8), NumberStyles.HexNumber)) / 100000;
                    Amplitude[I1] = uint.Parse(DSC.Remove(0, 88).Remove(8), NumberStyles.HexNumber);
                    NoteTimer[I1] = Convert.ToDouble(uint.Parse(DSC.Remove(0, 96).Remove(8), NumberStyles.HexNumber)) / 1000;
                    //UnknownVariable[I1] = uint.Parse(DSC.Remove(0, 104).Remove(8), NumberStyles.HexNumber);
                    //EndInt[I1] = int.Parse(DSC.Remove(0, 112), NumberStyles.HexNumber);
                    int tempI1 = I1 + 1;

                    if (HoldTimer[I1] != -1)
                        HoldTimer[I1] = HoldTimer[I1] / 1000000;

                    notes += "\r\n" + tempI1 + "," + RC(Timestamp[I1]) + "," + NoteIDParser(NoteType[I1]) + "," +
                        RC(HoldTimer[I1]) + "," + HoldEnd[I1] + "," + RC(XPos[I1]) + "," + RC(YPos[I1]) + "," +
                        RC(Angle1[I1]) + "," + WaveCount[I1] + "," + RC(Angle2[I1]) + "," + Amplitude[I1] + "," +
                        RC(NoteTimer[I1]);
                    I1++;
                }
            File.WriteAllText(Path.ChangeExtension(file, "csv"), notes);
            dsc = null;
            notes = null;
            GC.Collect();
        }

        public static string RC(double In)
        {
            return In.ToString().Replace(",", ".");
        }

        public static string BEtoLE(string BE, int Length)
        {
            if (Length == 2)
                return BE.Remove(0, 2) + BE.Remove(2);
            else if (Length == 3)
                return BE.Remove(0, 4) + BE.Remove(0, 2).Remove(2) + BE.Remove(2);
            else
                return BE.Remove(0, 6) + BE.Remove(0, 4).Remove(2) + BE.Remove(0, 2).Remove(2) + BE.Remove(2);
        }

        public static string NoteIDParser(uint note)
        {
            string Note = "";
            if (note == 0)
                Note = "TRIANGLE";
            else if (note == 1)
                Note = "CIRCLE";
            else if (note == 2)
                Note = "CROSS";
            else if (note == 3)
                Note = "SQUARE";
            else if (note == 4)
                Note = "TRIANGLE_DOUBLE";
            else if (note == 5)
                Note = "CIRCLE_DOUBLE";
            else if (note == 6)
                Note = "CROSS_DOUBLE";
            else if (note == 7)
                Note = "SQUARE_DOUBLE";
            else if (note == 8)
                Note = "TRIANGLE_HOLD";
            else if (note == 9)
                Note = "CIRCLE_HOLD";
            else if (note == 10)
                Note = "CROSS_HOLD";
            else if (note == 11)
                Note = "SQUARE_HOLD";
            else if (note == 12)
                Note = "STAR";
            else if (note == 14)
                Note = "STAR_DOUBLE";
            else if (note == 15)
                Note = "CHANCE_STAR";
            else if (note == 22)
                Note = "LINK_STAR";
            else if (note == 23)
                Note = "LINK_STAR_END";
            else if (note == 25)
                Note = "TRIANGLE_RUSH";
            else if (note == 26)
                Note = "CIRCLE_RUSH";
            else if (note == 27)
                Note = "CROSS_RUSH";
            else if (note == 28)
                Note = "SQUARE_RUSH";
            return Note;
        }

    }
}
