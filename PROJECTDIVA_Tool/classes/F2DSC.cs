using System;
using System.IO;
using System.Globalization;

namespace PROJECTDIVA_Tool
{
    class F2DSC
    {
        public static void DSC(string file)
        {
            Func.Decoder(file, HexParser(file), "F2");
        }

        public static int HexParser(string file)
        {
            int DSCint = BitConverter.ToInt32(File.ReadAllBytes(file), 0);
            byte[] PreDSC = File.ReadAllBytes(file);
            string DSC = "", DSC1 = "", DSC2 = "";
            int I = 0, I1 = 0, I2 = 0, I3 = 0;
            foreach (byte dscbyte in PreDSC)
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
                            DSC2 += (DSC1 + "\r\n");
                            DSC1 = "";
                            I2 = 0;
                            I3++;
                        }
                    }
                }
                else
                    I++;
            File.WriteAllText(Path.ChangeExtension(file, "hex"), DSC2.Remove(DSC2.Length - 2).Replace(" ", ""));
            return I3;
        }
    }
}
