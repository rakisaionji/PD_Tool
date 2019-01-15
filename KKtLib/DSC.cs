using System;
using System.IO;
using System.Xml.Linq;
using System.Collections.Generic;
using KKtIO = KKtLib.IO.KKtIO;
using KKtXml = KKtLib.Xml.KKtXml;
using KKtMain = KKtLib.Main;
using KKtText = KKtLib.Text;

namespace KKtLib
{
    public class DSC
    {
        public DSC() { }

        public string filebase = "";
        public string filename = "";
        public int format = 0;
        public bool IsX = false;
        public bool IsPS4 = false;
        public List<DSCEntry> Diff = new List<DSCEntry>();

        public void NameParser(string file)
        {
            file = file.Replace("_1.dsc", ".dsc").Replace("easy", "").Replace("normal", "").
                Replace("hard", "").Replace("extreme", "").Replace("encore", "").Replace("success_", "").
                Replace("dayo", "").Replace("mouth", "").Replace("scene", ""). Replace("system", "");
            filebase = file.Replace(".dsc", "").Replace(".xml", "");
            filename = Path.GetFileNameWithoutExtension(file.Replace("_.dsc", ".dsc"));
        }

        public void DSCReader(string FileName, bool Parsed)
        {
            NameParser(FileName);
            DSCReader(filebase);
        }

        public void DSCReader(string filebase)
        {
            new DSC();
            Console.Clear();
            for (byte i = 0; i < 2; i++)
            {
                string suff = (i == 1 ? "_1.dsc" : ".dsc");
                DSCReader(filebase, "easy"    + suff, Difficulty.EASY   , 0, i == 1);
                DSCReader(filebase, "normal"  + suff, Difficulty.NORMAL , 0, i == 1);
                DSCReader(filebase, "hard"    + suff, Difficulty.HARD   , 0, i == 1);
                DSCReader(filebase, "extreme" + suff, Difficulty.EXTREME, 0, i == 1);
            }
            DSCReader(filebase, "encore.dsc", Difficulty.ENCORE, 0, false);
            for (byte i = 0; i < 2; i++)
            {
                string filecommon = filebase + (i == 0 ? "common\\" + Path.GetFileNameWithoutExtension(filebase): "");
                DSCReader(filecommon, "dayo.dsc"         , 0, Type.DAYO         , false);
                DSCReader(filecommon, "success_dayo.dsc" , 0, Type.SUCCESS_DAYO , false);
                DSCReader(filecommon, "mouth.dsc"        , 0, Type.MOUTH        , false);
                DSCReader(filecommon, "success_mouth.dsc", 0, Type.SUCCESS_MOUTH, false);
                DSCReader(filecommon, "scene.dsc"        , 0, Type.SCENE        , false);
                DSCReader(filecommon, "success_scene.dsc", 0, Type.SUCCESS_SCENE, false);
                DSCReader(filecommon, "system.dsc"       , 0, Type.SYSTEM       , false);
            }
        }
        
        public void DSCReader(string filebase, string suffix, Difficulty Difficulty, Type Type, bool Extra)
        {
            if (File.Exists(filebase + suffix))
            {
                DSCEntry diff = new DSCEntry
                {
                    Type = Type,
                    Diff = Difficulty,
                    Funcs =
                    new List<Func>(),
                    Header = new KKtMain.Header(),
                    Extra = Extra
                };
                DSCReader(filebase, suffix, ref diff);
                Diff.Add(diff);
            }
        }

        private int DSCReader(string filebase, string suffix, ref DSCEntry diff)
        {
            KKtIO reader = KKtIO.OpenReader(filebase + suffix);
            int FileSize = (int)reader.Length;
            diff.Header.Signature = reader.ReadInt32();
            if (diff.Header.Signature == 0x43535650)
            {
                diff.Header = reader.ReadHeader();
                while (reader.Position < reader.Length)
                {
                    int a0 = reader.ReadInt32();
                    if (a0 == 0x55)
                    {
                        int a1 = reader.ReadInt32();
                        int a2 = reader.ReadInt32();
                        int a3 = reader.ReadInt32();
                        if (a2 == 0x56)
                        {
                            diff.X = IsX = true;
                            break;
                        }
                    }
                    else
                        diff.X = IsX;
                }
                reader.Seek(diff.Header.Lenght, 0);
                diff.Header.Signature = reader.ReadInt32();
            }

            switch (diff.Header.Signature)
            {
                case 0x00000001:
                    reader.Seek(0, 0);
                    diff.Header.Signature = 0;
                    break;
                case 0x10120116:
                case 0x11021719:
                case 0x11032818:
                case 0x11062018:
                case 0x12020220:
                case 0x13013121:
                case 0x13081522:
                case 0x13122519:
                case 0x14012316:
                case 0x14031318:
                case 0x14050921:
                case 0x15021718:
                case 0x15122517:
                case 0x16030121:
                    break;
                case 0x13120420:
                case 0x20041213:
                    diff.DSCType = reader.ReadInt32();
                    break;
                default:
                    return 0;
            }

            int Signature = diff.Header.ActualSignature = diff.Header.Signature;

            int ID;
            Func func;
            List<int> Values = new List<int>();
            while (true)
            {
                if (reader.Position >= diff.Header.Lenght + FileSize)
                    break;
                ID = reader.ReadInt32(true);
                if (ID != 0x01)
                {
                    func = new Func { Timestamp = -1, ID = new List<ID>() };
                    func.ID.Add(GetFunc(ID, ref Signature, ref diff.Header.ActualSignature, ref reader));
                    diff.Funcs.Add(func);
                    ID = reader.ReadInt32(true);
                }
                if (ID == 0x01)
                {
                    func = new Func { Timestamp = reader.ReadInt32(true), ID = new List<ID>() };
                    while (true)
                    {
                        if (reader.Position >= diff.Header.Lenght + FileSize)
                            break;
                        Values = new List<int>();
                        ID = reader.ReadInt32(true);
                        if (ID == 0x01)
                        {
                            reader.Seek(-4, SeekOrigin.Current);
                            break;
                        }
                        else if (ID == 0x53524E45 || ID == 0x43464F45)
                            break;
                        else
                            func.ID.Add(GetFunc(ID, ref Signature, ref diff.Header.ActualSignature, ref reader));

                        if (reader.Position >= diff.Header.Lenght + FileSize)
                            break;
                        if (ID == 0x0 || ID == 0x20 || ID == 0x53524E45 || ID == 0x43464F45)
                            break;
                    }
                    diff.Funcs.Add(func);
                    if (reader.Position >= diff.Header.Lenght + FileSize)
                        break;
                    if (ID == 0x0 || ID == 0x20 || ID == 0x53524E45 || ID == 0x43464F45)
                        break;
                }
            }
            if (Signature == 0x13120420 && IsX)
                while (reader.Position < reader.Length)
                {
                    ID = reader.ReadInt32(true);
                    if ((ID == 0x53524E45 && !diff.PS4) || (IsPS4 && !diff.PS4))
                    {
                        diff.PS4 = IsPS4 = true;
                        break;
                    }
                }
            reader.Close();
            return 1;
        }

        private ID GetFunc(int ID, ref int Signature, ref int actualSignature, ref KKtIO reader)
        {
            List<int> Values = new List<int>();
            int FuncSize = GetFuncSize(ID, Signature);
            for (int i1 = 0; i1 < FuncSize; i1++)
            {
                if (Signature == 0x11032818 && i1 == 3 && ID == 6 &&
                    Values[1] != -1 && Values[2] != -1)
                {
                    Signature = actualSignature = 0x10120116;
                    FuncSize = 7;
                }
                Values.Add(reader.ReadInt32(true));
            }
            return new ID { Id = ID, Values = Values };
        }

        private int GetFuncSize(int ID, int Signature)
        {
            switch (Signature)
            {
                case 0x00000000:
                    return DT.GetFuncSize(ID);
                case 0x10120116:
                case 0x11021719:
                case 0x11032818:
                case 0x11062018:
                    return DT2.GetFuncSize(ID, Signature);
                case 0x12020220:
                    return F.GetFuncSize(ID);
                case 0x13120420:
                    if (!IsX)
                        return F2.GetFuncSize(ID);
                    else
                        return X.GetFuncSize(ID);
                case 0x13013121:
                case 0x13081522:
                case 0x13122519:
                case 0x14012316:
                case 0x14031318:
                case 0x14050921:
                case 0x15021718:
                case 0x15122517:
                case 0x16030121:
                    return FT.GetFuncSize(ID);
                case 0x20041213:
                    return F2.GetFuncSize(ID);
            }
            return 0;
        }

        public Format GetType(int Signature)
        {
            switch (Signature)
            {
                case 0x00000000:
                    return Format.DT;
                case 0x10120116:
                case 0x11021719:
                case 0x11032818:
                case 0x11062018:
                    return Format.DT2;
                case 0x12020220:
                    return Format.F;
                case 0x13120420:
                    if (!IsX)
                        return Format.F2LE;
                    else
                        return Format.X;
                case 0x13013121:
                case 0x13081522:
                case 0x13122519:
                case 0x14012316:
                case 0x14031318:
                case 0x14050921:
                case 0x15021718:
                case 0x15122517:
                case 0x16030121:
                    return Format.FT;
                case 0x20041213:
                    return Format.F2BE;
            }
            return Format.NULL;
        }

        public enum Format
        {
            NULL,
            DT  ,
            DT2 ,
            F   ,
            F2LE,
            F2BE,
            FT  ,
            X   ,
        }

        public void DSCWriter(string filebase)
        {
            string FileName = "";

            foreach (DSCEntry diff in Diff)
            {
                FileName = filebase + "_";
                if (diff.Diff != 0)
                {
                    if (diff.Diff == Difficulty.EASY)
                        FileName += "easy";
                    else if (diff.Diff == Difficulty.NORMAL)
                        FileName += "normal";
                    else if (diff.Diff == Difficulty.HARD)
                        FileName += "hard";
                    else if (diff.Diff == Difficulty.EXTREME)
                        FileName += "extreme";
                    else if (diff.Diff == Difficulty.ENCORE)
                        FileName += "encore";
                    if (diff.Extra)
                        FileName += "_1";
                }
                else if (diff.Type != 0)
                    if (diff.Type == Type.SYSTEM)
                        FileName += "KKtMain";
                    else if (diff.Type == Type.SCENE)
                        FileName += "scene";
                    else if (diff.Type == Type.MOUTH)
                        FileName += "mouth";
                    else if (diff.Type == Type.DAYO)
                        FileName += "dayo";
                    else if (diff.Type == Type.SUCCESS_SCENE)
                        FileName += "success_scene";
                    else if (diff.Type == Type.SUCCESS_MOUTH)
                        FileName += "success_mouth";
                    else if (diff.Type == Type.SUCCESS_DAYO)
                        FileName += "success_dayo";

                KKtIO writer = KKtIO.OpenWriter(FileName + ".dsc", true);
                writer.IsBE = diff.Header.Signature == 0x20041213;
                switch (diff.Header.Signature)
                {
                    case 0x13120420:
                    case 0x20041213:
                        writer.Write(KKtText.ToASCII("PVSC"));
                        writer.Write(0x00);
                        writer.Write(0x40);
                        if (diff.Header.Signature == 0x20041213)
                            writer.Write(0x18000000);
                        else
                            writer.Write(0x10000000);
                        writer.Write((long)0x00);
                        writer.Write((long)0x00);
                        if (!diff.X)
                        {
                            int val = 0;
                            if (diff.Diff != 0)
                            {
                                if (diff.Diff == Difficulty.EASY)
                                    val = 0x0;
                                else if (diff.Diff == Difficulty.NORMAL)
                                    val = 0x1;
                                else if (diff.Diff == Difficulty.HARD)
                                    val = 0x2;
                                else if (diff.Diff == Difficulty.EXTREME)
                                    val = 0x3;
                                else if (diff.Diff == Difficulty.ENCORE)
                                    val = 0x8;
                                if (diff.Extra)
                                    val += 9;
                            }
                            else if (diff.Type != 0)
                                if (diff.Type == Type.SYSTEM)
                                    val = 4;
                                else if (diff.Type == Type.SCENE)
                                    val = 5;
                                else if (diff.Type == Type.MOUTH)
                                    val = 6;
                                else if (diff.Type == Type.DAYO)
                                    val = 7;

                            Random r = new Random(val);
                            int val1 = (short)r.Next(0x000, 0xFFF);
                            int val2 = 0;
                            string filename = Path.GetFileNameWithoutExtension(filebase);
                            if (int.Parse(filename.Remove(0, filename.Length - 3)) == 0)
                            {
                                r = new Random(val);
                                val2 = (short)r.Next(0x0000, 0xFFFF);
                            }
                            else
                                val2 = short.Parse(filename.Remove(0, filename.Length - 3));
                            writer.Write(val << 28 | val1 << 16 | val2);
                        }
                        else
                            writer.Write(0x00);
                        writer.Write(0x00);
                        writer.Write((long)0x00);
                        writer.Write(diff.Header.Signature);
                        writer.Write(0x00);
                        writer.Write((long)0x00);
                        writer.Write(diff.Header.Signature, true);
                        writer.Write(diff.DSCType, true);
                        break;
                    default:
                        writer.Write(diff.Header.Signature);
                        break;
                }

                foreach (Func func in diff.Funcs)
                    if (func.ID.Count != 0)
                    {
                        if (func.Timestamp != -1)
                        {
                            writer.Write(0x01, true);
                            writer.Write(func.Timestamp, true);
                        }
                        foreach (ID id in func.ID)
                        {
                            writer.Write(id.Id, true);
                            foreach (int value in id.Values)
                                writer.Write(value, true);
                        }
                    }
                if (writer.Position % 0x10 != 0)
                {
                    writer.Align(16);
                    writer.Seek(-4, SeekOrigin.Current);
                    writer.Write(0x00);
                }

                switch (diff.Header.Signature)
                {
                    case 0x13120420:
                    case 0x20041213:
                        int Offset = (int)writer.Position;
                        writer.Seek(0x14, 0);
                        writer.Write(Offset - 0x40);
                        writer.Seek(Offset, 0);
                        Offset = (int)writer.Position;
                        writer.Seek(0x04, 0);
                        writer.Write(Offset - 0x40);
                        writer.Seek(Offset, 0);
                        writer.WriteEOFC(0);
                        break;
                }
                writer.Close();
            }
        }

        public void XMLReader(string filebase)
        {
            DSCEntry diff;
            Func func;
            ID id;

            Diff = new List<DSCEntry>();
            KKtXml Xml = new KKtXml();
            Xml.OpenXml(filebase + ".xml", true);
            foreach (XElement DSC in Xml.doc.Elements("DSC"))
                foreach (XElement DSCEntry in DSC.Elements())
                    if (DSCEntry.Name == "DSCEntry")
                    {
                        diff = new DSCEntry
                        {
                            Funcs = new
                            List<Func>(),
                            Header = new KKtMain.Header()
                        };
                        foreach (XAttribute Entry in DSCEntry.Attributes())
                        {
                            if (Entry.Name == "X")
                                diff.X = bool.Parse(Entry.Value);
                            else if (Entry.Name == "Type")
                                Enum.TryParse(Entry.Value, out diff.Type);
                            else if (Entry.Name == "Difficulty")
                                Enum.TryParse(Entry.Value, out diff.Diff);
                            else if (Entry.Name == "Signature")
                                diff.Header.Signature = int.Parse(Entry.Value);
                            else if (Entry.Name == "DSCType")
                                diff.DSCType = int.Parse(Entry.Value);
                            else if (Entry.Name == "Extra")
                                diff.Extra = bool.Parse(Entry.Value);
                            else if (Entry.Name == "ActualSignatiure")
                                diff.Header.ActualSignature = int.Parse(Entry.Value);
                        }

                        diff.Funcs = new List<Func>();
                        foreach (XElement Func in DSCEntry.Elements())
                        {
                            if (Func.Name == "Func")
                            {
                                func = new Func { ID = new List<ID>() };
                                foreach (XAttribute Timestamp in Func.Attributes())
                                    if (Timestamp.Name == "Timestamp")
                                        func.Timestamp = int.Parse(Timestamp.Value);

                                foreach (XElement ID in Func.Elements())
                                    if (ID.Name == "ID")
                                    {
                                        bool Add = false;
                                        id = new ID { Values = new List<int>() };
                                        foreach (XAttribute Name in ID.Attributes())
                                            if (Name.Name == "Name")
                                            {
                                                if (diff.Header.ActualSignature == 0)
                                                    diff.Header.ActualSignature = diff.Header.Signature;
                                                switch (diff.Header.ActualSignature)
                                                {
                                                    case 0x00000000:
                                                        Add = Enum.TryParse(Name.Value, out DT.FuncID Id1);
                                                        id.Id = (int)Id1;
                                                        break;
                                                    case 0x11021719:
                                                    case 0x11032818:
                                                    case 0x11062018:
                                                        Add = Enum.TryParse(Name.Value, out DT2.FuncID Id2);
                                                        id.Id = (int)Id2;
                                                        break;
                                                    case 0x12020220:
                                                        Add = Enum.TryParse(Name.Value, out F.FuncID Id3);
                                                        id.Id = (int)Id3;
                                                        break;
                                                    case 0x13120420:
                                                        if (!diff.X)
                                                        {
                                                            Add = Enum.TryParse(Name.Value, out F2.FuncID Id);
                                                            id.Id = (int)Id;
                                                        }
                                                        else
                                                        {
                                                            Add = Enum.TryParse(Name.Value, out X.FuncID Id);
                                                            id.Id = (int)Id;
                                                        }
                                                        break;
                                                    case 0x10120116:
                                                    case 0x13013121:
                                                    case 0x13081522:
                                                    case 0x13122519:
                                                    case 0x14012316:
                                                    case 0x14031318:
                                                    case 0x14050921:
                                                    case 0x15021718:
                                                    case 0x15122517:
                                                    case 0x16030121:
                                                        Add = Enum.TryParse(Name.Value, out FT.FuncID Id5);
                                                        id.Id = (int)Id5;
                                                        break;
                                                    case 0x20041213:
                                                        Add = Enum.TryParse(Name.Value, out F2.FuncID Id6);
                                                        id.Id = (int)Id6;
                                                        break;
                                                }
                                            }

                                        if (Add)
                                        {
                                            foreach (XElement value in ID.Elements())
                                                if (value.Name == "int")
                                                    id.Values.Add(int.Parse(value.Value));
                                            func.ID.Add(id);
                                        }
                                    }
                                diff.Funcs.Add(func);
                            }
                        }
                        Diff.Add(diff);
                    }
        }

        public void XMLWriter(string filebase)
        {
            XElement DSCEntry;
            XElement Func;
            XElement ID;

            KKtXml Xml = new KKtXml { Compact = true };
            if (File.Exists(filebase + ".xml"))
                File.Delete(filebase + ".xml");
            XElement DSC = new XElement("DSC");
            foreach (DSCEntry diff in Diff)
            {
                DSCEntry = new XElement("DSCEntry");
                if (diff.Diff != 0)
                    Xml.Writer(DSCEntry, diff.Diff.ToString().ToUpper(), "Difficulty");
                if (diff.Type != 0)
                    Xml.Writer(DSCEntry, diff.Type.ToString().ToUpper(), "Type");
                if (diff.Header.Signature != 0)
                    Xml.Writer(DSCEntry, diff.Header.Signature, "Signature");
                if (diff.Header.Signature != diff.Header.ActualSignature)
                    Xml.Writer(DSCEntry, diff.Header.ActualSignature, "ActualSignature");
                if (diff.Header.Signature == 0x13120420 || diff.Header.Signature == 0x20041213)
                    Xml.Writer(DSCEntry, diff.DSCType, "DSCType");
                if (diff.Extra)
                    Xml.Writer(DSCEntry, diff.Extra, "Extra");
                if (diff.Header.Signature == 0x13120420 && diff.X)
                    Xml.Writer(DSCEntry, diff.X, "X");
                if (diff.Header.Signature == 0x13120420 && diff.X && diff.PS4)
                    Xml.Writer(DSCEntry, diff.PS4, "PS4");

                foreach (Func func in diff.Funcs)
                {
                    Func = new XElement("Func");
                    Xml.Writer(Func, func.Timestamp, "Timestamp");
                    foreach (ID id in func.ID)
                    {
                        ID = new XElement("ID");
                        string Name = "";
                        switch (diff.Header.Signature)
                        {
                            case 0x00000000:
                                Name = ((DT.FuncID)id.Id).ToString().ToUpper();
                                break;
                            case 0x11032818:
                            case 0x11062018:
                            case 0x11021719:
                                Name = ((DT2.FuncID)id.Id).ToString().ToUpper();
                                break;
                            case 0x12020220:
                                Name = ((F.FuncID)id.Id).ToString().ToUpper();
                                break;
                            case 0x13120420:
                                if (!IsX)
                                    Name = ((F2.FuncID)id.Id).ToString().ToUpper();
                                else
                                    Name = ((X.FuncID)id.Id).ToString().ToUpper();
                                break;
                            case 0x10120116:
                            case 0x13013121:
                            case 0x13081522:
                            case 0x13122519:
                            case 0x14050921:
                            case 0x14012316:
                            case 0x14031318:
                            case 0x15021718:
                            case 0x15122517:
                            case 0x16030121:
                                Name = ((FT.FuncID)id.Id).ToString().ToUpper();
                                break;
                            case 0x20041213:
                                Name = ((F2.FuncID)id.Id).ToString().ToUpper();
                                break;
                        }
                        if (Name == "")
                            Name = id.Id.ToString();
                        Xml.Writer(ID, Name, "Name");
                        Xml.Compact = false;
                        foreach (int value in id.Values)
                            Xml.Writer(ID, value, "int");
                        Func.Add(ID);
                        Xml.Compact = true;
                    }
                    DSCEntry.Add(Func);
                }
                DSC.Add(DSCEntry);
            }
            Xml.doc.Add(DSC);
            Xml.SaveXml(filebase + ".xml");
        }

        public struct DSCEntry
        {
            public bool X;
            public bool PS4;
            public bool Extra;
            public int DSCType;
            public Type Type;
            public Difficulty Diff;
            public List<Func> Funcs;
            public KKtMain.Header Header;
        }

        public struct Func
        {
            public int Timestamp;
            public List<ID> ID;
        }

        public struct ID
        {
            public int Id;
            public List<int> Values;
        }

        public enum Difficulty
        {
            EASY = 0x01,
            NORMAL = 0x02,
            HARD = 0x03,
            EXTREME = 0x04,
            ENCORE = 0x05,
        }

        public enum Type
        {
            MOUTH = 0x01,
            SCENE = 0x02,
            SYSTEM = 0x03,
            SUCCESS_MOUTH = 0x04,
            SUCCESS_SCENE = 0x05,
            DAYO = 0x06,
            SUCCESS_DAYO = 0x07,
        }

        public enum Note7
        {
            NoteType,
            XPos,
            YPos,
            angle1,
            angle2,
            amplitude,
            WaveCount,
        }

        public enum Note11
        {
            NoteType,
            HoldTimer,
            HoldEnd,
            XPos,
            YPos,
            angle1,
            WaveCount,
            angle2,
            amplitude,
            NoteTimer,
            UnknownVariable,
        }

        public enum Note12
        {
            NoteType,
            HoldTimer,
            HoldEnd,
            XPos,
            YPos,
            angle1,
            WaveCount,
            angle2,
            amplitude,
            NoteTimer,
            UnknownVariable,
            NoteEvent,
        }

        public class DT
        {
            public static int GetFuncSize(int ID)
            {
                switch ((FuncID)ID)
                {
                    case FuncID.END: return 0;
                    case FuncID.TIME: return 1;
                    case FuncID.MIKU_MOVE: return 3;
                    case FuncID.MIKU_ROT: return 1;
                    case FuncID.MIKU_DISP: return 1;
                    case FuncID.MIKU_SHADOW: return 1;
                    case FuncID.TARGET: return 7;
                    case FuncID.SET_MOTION: return 3;
                    case FuncID.SET_PLAYDATA: return 1;
                    case FuncID.EFFECT: return 5;
                    case FuncID.FADEIN_FIELD: return 2;
                    case FuncID.EFFECT_OFF: return 1;
                    case FuncID.SET_CAMERA: return 5;
                    case FuncID.DATA_CAMERA: return 2;
                    case FuncID.CHANGE_FIELD: return 1;
                    case FuncID.HIDE_FIELD: return 1;
                    case FuncID.MOVE_FIELD: return 3;
                    case FuncID.FADEOUT_FIELD: return 2;
                    case FuncID.EYE_ANIM: return 2;
                    case FuncID.MOUTH_ANIM: return 3;
                    case FuncID.HAND_ANIM: return 4;
                    case FuncID.LOOK_ANIM: return 3;
                    case FuncID.EXPRESSION: return 3;
                    case FuncID.LOOK_CAMERA: return 4;
                    case FuncID.LYRIC: return 1;
                    case FuncID.MUSIC_PLAY: return 0;
                    case FuncID.MODE_SELECT: return 1;
                    case FuncID.EDIT_MOTION: return 3;
                    case FuncID.BAR_TIME_SET: return 2;
                    case FuncID.SHADOWHEIGHT: return 1;
                    case FuncID.EDIT_FACE: return 1;
                    case FuncID.MOVE_CAMERA: return 19;
                    case FuncID.PV_END: return 1;
                    case FuncID.SHADOWPOS: return 2;
                    case FuncID.NEAR_CLIP: return 1;
                    case FuncID.CLOTH_WET: return 1;
                }
                return 0;
            }

            public enum FuncID
            {
                END = 0x00,
                TIME = 0x01,
                MIKU_MOVE = 0x02,
                MIKU_ROT = 0x03,
                MIKU_DISP = 0x04,
                MIKU_SHADOW = 0x05,
                TARGET = 0x06,
                SET_MOTION = 0x07,
                SET_PLAYDATA = 0x08,
                EFFECT = 0x09,
                FADEIN_FIELD = 0x0A,
                EFFECT_OFF = 0x0B,
                SET_CAMERA = 0x0C,
                DATA_CAMERA = 0x0D,
                CHANGE_FIELD = 0x0E,
                HIDE_FIELD = 0x0F,
                MOVE_FIELD = 0x10,
                FADEOUT_FIELD = 0x11,
                EYE_ANIM = 0x12,
                MOUTH_ANIM = 0x13,
                HAND_ANIM = 0x14,
                LOOK_ANIM = 0x15,
                EXPRESSION = 0x16,
                LOOK_CAMERA = 0x17,
                LYRIC = 0x18,
                MUSIC_PLAY = 0x19,
                MODE_SELECT = 0x1A,
                EDIT_MOTION = 0x1B,
                BAR_TIME_SET = 0x1C,
                SHADOWHEIGHT = 0x1D,
                EDIT_FACE = 0x1E,
                MOVE_CAMERA = 0x1F,
                PV_END = 0x20,
                SHADOWPOS = 0x21,
                NEAR_CLIP = 0x22,
                CLOTH_WET = 0x23,
            }

            public enum TargetID
            {
                TRIANGLE = 0x00,
                CIRCLE = 0x01,
                CROSS = 0x02,
                SQUARE = 0x03,
                TRIANGLE_HOLD = 0x04,
                CIRCLE_HOLD = 0x05,
                CROSS_HOLD = 0x06,
                SQUARE_HOLD = 0x07,
            }
        }

        public class DT2
        {
            public static int GetFuncSize(int ID, int Signature)
            {
                switch ((FuncID)ID)
                {
                    case FuncID.END: return 0;
                    case FuncID.TIME: return 1;
                    case FuncID.MIKU_MOVE: return 4;
                    case FuncID.MIKU_ROT: return 2;
                    case FuncID.MIKU_DISP: return 2;
                    case FuncID.MIKU_SHADOW: return 2;
                    case FuncID.TARGET: return 11;
                    case FuncID.SET_MOTION: return 4;
                    case FuncID.SET_PLAYDATA: return 2;
                    case FuncID.EFFECT: return 6;
                    case FuncID.FADEIN_FIELD: return 2;
                    case FuncID.EFFECT_OFF: return 1;
                    case FuncID.SET_CAMERA: return 6;
                    case FuncID.DATA_CAMERA: return 2;
                    case FuncID.CHANGE_FIELD: return 1;
                    case FuncID.HIDE_FIELD: return 1;
                    case FuncID.MOVE_FIELD: return 3;
                    case FuncID.FADEOUT_FIELD: return 2;
                    case FuncID.EYE_ANIM: return 3;
                    case FuncID.MOUTH_ANIM: return 5;
                    case FuncID.HAND_ANIM: return 5;
                    case FuncID.LOOK_ANIM: return 4;
                    case FuncID.EXPRESSION: return 4;
                    case FuncID.LOOK_CAMERA: return 5;
                    case FuncID.LYRIC: return 2;
                    case FuncID.MUSIC_PLAY: return 0;
                    case FuncID.MODE_SELECT: return 2;
                    case FuncID.EDIT_MOTION: return 4;
                    case FuncID.BAR_TIME_SET: return 2;
                    case FuncID.SHADOWHEIGHT: return 2;
                    case FuncID.EDIT_FACE: return 1;
                    case FuncID.MOVE_CAMERA: return 21;
                    case FuncID.PV_END: return 1;
                    case FuncID.SHADOWPOS: return 3;
                    case FuncID.EDIT_LYRIC: return 2;
                    case FuncID.EDIT_TARGET: return 5;
                    case FuncID.EDIT_MOUTH: return 1;
                    case FuncID.SET_CHARA: return 1;
                    case FuncID.EDIT_MOVE: return 7;
                    case FuncID.EDIT_SHADOW: return 1;
                    case FuncID.EDIT_EYELID: return 1;
                    case FuncID.EDIT_EYE: return 2;
                    case FuncID.EDIT_ITEM: return 1;
                    case FuncID.EDIT_EFFECT: return 2;
                    case FuncID.EDIT_DISP: return 1;
                    case FuncID.EDIT_HAND_ANIM: return 2;
                    case FuncID.AIM: return 3;
                    case FuncID.HAND_ITEM: return 3;
                    case FuncID.EDIT_BLUSH: return 1;
                    case FuncID.NEAR_CLIP: return 2;
                    case FuncID.CLOTH_WET: return 2;
                    case FuncID.LIGHT_ROT: return 3;
                    case FuncID.SCENE_FADE: return 6;
                    case FuncID.TONE_TRANS: return 6;
                    case FuncID.SATURATE: return 1;
                    case FuncID.FADE_MODE: return 1;
                    case FuncID.AUTO_BLINK: return 2;
                    case FuncID.PARTS_DISP: return 3;
                    case FuncID.TARGET_FLYING_TIME: return 1;
                    case FuncID.CHARA_SIZE: return 2;
                    case FuncID.CHARA_HEIGHT_ADJUST: return 2;
                    case FuncID.ITEM_ANIM: return 4;
                    case FuncID.CHARA_POS_ADJUST: return 4;
                    case FuncID.SCENE_ROT: return 1;
                }
                return 0;
            }

            public enum FuncID
            {
                END = 0x00,
                TIME = 0x01,
                MIKU_MOVE = 0x02,
                MIKU_ROT = 0x03,
                MIKU_DISP = 0x04,
                MIKU_SHADOW = 0x05,
                TARGET = 0x06,
                SET_MOTION = 0x07,
                SET_PLAYDATA = 0x08,
                EFFECT = 0x09,
                FADEIN_FIELD = 0x0A,
                EFFECT_OFF = 0x0B,
                SET_CAMERA = 0x0C,
                DATA_CAMERA = 0x0D,
                CHANGE_FIELD = 0x0E,
                HIDE_FIELD = 0x0F,
                MOVE_FIELD = 0x10,
                FADEOUT_FIELD = 0x11,
                EYE_ANIM = 0x12,
                MOUTH_ANIM = 0x13,
                HAND_ANIM = 0x14,
                LOOK_ANIM = 0x15,
                EXPRESSION = 0x16,
                LOOK_CAMERA = 0x17,
                LYRIC = 0x18,
                MUSIC_PLAY = 0x19,
                MODE_SELECT = 0x1A,
                EDIT_MOTION = 0x1B,
                BAR_TIME_SET = 0x1C,
                SHADOWHEIGHT = 0x1D,
                EDIT_FACE = 0x1E,
                MOVE_CAMERA = 0x1F,
                PV_END = 0x20,
                SHADOWPOS = 0x21,
                EDIT_LYRIC = 0x22,
                EDIT_TARGET = 0x23,
                EDIT_MOUTH = 0x24,
                SET_CHARA = 0x25,
                EDIT_MOVE = 0x26,
                EDIT_SHADOW = 0x27,
                EDIT_EYELID = 0x28,
                EDIT_EYE = 0x29,
                EDIT_ITEM = 0x2A,
                EDIT_EFFECT = 0x2B,
                EDIT_DISP = 0x2C,
                EDIT_HAND_ANIM = 0x2D,
                AIM = 0x2E,
                HAND_ITEM = 0x2F,
                EDIT_BLUSH = 0x30,
                NEAR_CLIP = 0x31,
                CLOTH_WET = 0x32,
                LIGHT_ROT = 0x33,
                SCENE_FADE = 0x34,
                TONE_TRANS = 0x35,
                SATURATE = 0x36,
                FADE_MODE = 0x37,
                AUTO_BLINK = 0x38,
                PARTS_DISP = 0x39,
                TARGET_FLYING_TIME = 0x3A,
                CHARA_SIZE = 0x3B,
                CHARA_HEIGHT_ADJUST = 0x3C,
                ITEM_ANIM = 0x3D,
                CHARA_POS_ADJUST = 0x3E,
                SCENE_ROT = 0x3F,
            }

            public enum TargetID
            {
                TRIANGLE = 0x00,
                CIRCLE = 0x01,
                CROSS = 0x02,
                SQUARE = 0x03,
                TRIANGLE_DOUBLE = 0x04,
                CIRCLE_DOUBLE = 0x05,
                CROSS_DOUBLE = 0x06,
                SQUARE_DOUBLE = 0x07,
                TRIANGLE_HOLD = 0x08,
                CIRCLE_HOLD = 0x09,
                CROSS_HOLD = 0x0A,
                SQUARE_HOLD = 0x0B,
            }
        }

        public class F
        {
            public static int GetFuncSize(int ID)
            {
                switch ((FuncID)ID)
                {
                    case FuncID.END: return 0;
                    case FuncID.TIME: return 1;
                    case FuncID.MIKU_MOVE: return 4;
                    case FuncID.MIKU_ROT: return 2;
                    case FuncID.MIKU_DISP: return 2;
                    case FuncID.MIKU_SHADOW: return 2;
                    case FuncID.TARGET: return 11;
                    case FuncID.SET_MOTION: return 4;
                    case FuncID.SET_PLAYDATA: return 2;
                    case FuncID.EFFECT: return 6;
                    case FuncID.FADEIN_FIELD: return 2;
                    case FuncID.EFFECT_OFF: return 1;
                    case FuncID.SET_CAMERA: return 6;
                    case FuncID.DATA_CAMERA: return 2;
                    case FuncID.CHANGE_FIELD: return 1;
                    case FuncID.HIDE_FIELD: return 1;
                    case FuncID.MOVE_FIELD: return 3;
                    case FuncID.FADEOUT_FIELD: return 2;
                    case FuncID.EYE_ANIM: return 3;
                    case FuncID.MOUTH_ANIM: return 5;
                    case FuncID.HAND_ANIM: return 5;
                    case FuncID.LOOK_ANIM: return 4;
                    case FuncID.EXPRESSION: return 4;
                    case FuncID.LOOK_CAMERA: return 5;
                    case FuncID.LYRIC: return 2;
                    case FuncID.MUSIC_PLAY: return 0;
                    case FuncID.MODE_SELECT: return 2;
                    case FuncID.EDIT_MOTION: return 4;
                    case FuncID.BAR_TIME_SET: return 2;
                    case FuncID.SHADOWHEIGHT: return 2;
                    case FuncID.EDIT_FACE: return 1;
                    case FuncID.MOVE_CAMERA: return 21;
                    case FuncID.PV_END: return 1;
                    case FuncID.SHADOWPOS: return 3;
                    case FuncID.EDIT_LYRIC: return 2;
                    case FuncID.EDIT_TARGET: return 5;
                    case FuncID.EDIT_MOUTH: return 1;
                    case FuncID.SET_CHARA: return 1;
                    case FuncID.EDIT_MOVE: return 7;
                    case FuncID.EDIT_SHADOW: return 1;
                    case FuncID.EDIT_EYELID: return 1;
                    case FuncID.EDIT_EYE: return 2;
                    case FuncID.EDIT_ITEM: return 1;
                    case FuncID.EDIT_EFFECT: return 2;
                    case FuncID.EDIT_DISP: return 1;
                    case FuncID.EDIT_HAND_ANIM: return 2;
                    case FuncID.AIM: return 3;
                    case FuncID.HAND_ITEM: return 3;
                    case FuncID.EDIT_BLUSH: return 1;
                    case FuncID.NEAR_CLIP: return 2;
                    case FuncID.CLOTH_WET: return 2;
                    case FuncID.LIGHT_ROT: return 3;
                    case FuncID.SCENE_FADE: return 6;
                    case FuncID.TONE_TRANS: return 6;
                    case FuncID.SATURATE: return 1;
                    case FuncID.FADE_MODE: return 1;
                    case FuncID.AUTO_BLINK: return 2;
                    case FuncID.PARTS_DISP: return 3;
                    case FuncID.TARGET_FLYING_TIME: return 1;
                    case FuncID.CHARA_SIZE: return 2;
                    case FuncID.CHARA_HEIGHT_ADJUST: return 2;
                    case FuncID.ITEM_ANIM: return 4;
                    case FuncID.CHARA_POS_ADJUST: return 4;
                    case FuncID.SCENE_ROT: return 1;
                    case FuncID.EDIT_MOT_SMOOTH_LEN: return 2;
                    case FuncID.PV_BRANCH_MODE: return 1;
                    case FuncID.DATA_CAMERA_START: return 2;
                    case FuncID.MOVIE_PLAY: return 1;
                    case FuncID.MOVIE_DISP: return 1;
                    case FuncID.WIND: return 3;
                    case FuncID.OSAGE_STEP: return 3;
                    case FuncID.OSAGE_MV_CCL: return 3;
                    case FuncID.CHARA_COLOR: return 2;
                    case FuncID.SE_EFFECT: return 1;
                    case FuncID.EDIT_MOVE_XYZ: return 9;
                    case FuncID.EDIT_EYELID_ANIM: return 3;
                    case FuncID.EDIT_INSTRUMENT_ITEM: return 2;
                    case FuncID.EDIT_MOTION_LOOP: return 4;
                    case FuncID.EDIT_EXPRESSION: return 2;
                    case FuncID.EDIT_EYE_ANIM: return 3;
                    case FuncID.EDIT_MOUTH_ANIM: return 2;
                    case FuncID.EDIT_CAMERA: return 24;
                    case FuncID.EDIT_MODE_SELECT: return 1;
                    case FuncID.PV_END_FADEOUT: return 2;
                }
                return 0;
            }

            public enum FuncID
            {
                END = 0x00,
                TIME = 0x01,
                MIKU_MOVE = 0x02,
                MIKU_ROT = 0x03,
                MIKU_DISP = 0x04,
                MIKU_SHADOW = 0x05,
                TARGET = 0x06,
                SET_MOTION = 0x07,
                SET_PLAYDATA = 0x08,
                EFFECT = 0x09,
                FADEIN_FIELD = 0x0A,
                EFFECT_OFF = 0x0B,
                SET_CAMERA = 0x0C,
                DATA_CAMERA = 0x0D,
                CHANGE_FIELD = 0x0E,
                HIDE_FIELD = 0x0F,
                MOVE_FIELD = 0x10,
                FADEOUT_FIELD = 0x11,
                EYE_ANIM = 0x12,
                MOUTH_ANIM = 0x13,
                HAND_ANIM = 0x14,
                LOOK_ANIM = 0x15,
                EXPRESSION = 0x16,
                LOOK_CAMERA = 0x17,
                LYRIC = 0x18,
                MUSIC_PLAY = 0x19,
                MODE_SELECT = 0x1A,
                EDIT_MOTION = 0x1B,
                BAR_TIME_SET = 0x1C,
                SHADOWHEIGHT = 0x1D,
                EDIT_FACE = 0x1E,
                MOVE_CAMERA = 0x1F,
                PV_END = 0x20,
                SHADOWPOS = 0x21,
                EDIT_LYRIC = 0x22,
                EDIT_TARGET = 0x23,
                EDIT_MOUTH = 0x24,
                SET_CHARA = 0x25,
                EDIT_MOVE = 0x26,
                EDIT_SHADOW = 0x27,
                EDIT_EYELID = 0x28,
                EDIT_EYE = 0x29,
                EDIT_ITEM = 0x2A,
                EDIT_EFFECT = 0x2B,
                EDIT_DISP = 0x2C,
                EDIT_HAND_ANIM = 0x2D,
                AIM = 0x2E,
                HAND_ITEM = 0x2F,
                EDIT_BLUSH = 0x30,
                NEAR_CLIP = 0x31,
                CLOTH_WET = 0x32,
                LIGHT_ROT = 0x33,
                SCENE_FADE = 0x34,
                TONE_TRANS = 0x35,
                SATURATE = 0x36,
                FADE_MODE = 0x37,
                AUTO_BLINK = 0x38,
                PARTS_DISP = 0x39,
                TARGET_FLYING_TIME = 0x3A,
                CHARA_SIZE = 0x3B,
                CHARA_HEIGHT_ADJUST = 0x3C,
                ITEM_ANIM = 0x3D,
                CHARA_POS_ADJUST = 0x3E,
                SCENE_ROT = 0x3F,
                EDIT_MOT_SMOOTH_LEN = 0x40,
                PV_BRANCH_MODE = 0x41,
                DATA_CAMERA_START = 0x42,
                MOVIE_PLAY = 0x43,
                MOVIE_DISP = 0x44,
                WIND = 0x45,
                OSAGE_STEP = 0x46,
                OSAGE_MV_CCL = 0x47,
                CHARA_COLOR = 0x48,
                SE_EFFECT = 0x49,
                EDIT_MOVE_XYZ = 0x4A,
                EDIT_EYELID_ANIM = 0x4B,
                EDIT_INSTRUMENT_ITEM = 0x4C,
                EDIT_MOTION_LOOP = 0x4D,
                EDIT_EXPRESSION = 0x4E,
                EDIT_EYE_ANIM = 0x4F,
                EDIT_MOUTH_ANIM = 0x50,
                EDIT_CAMERA = 0x51,
                EDIT_MODE_SELECT = 0x52,
                PV_END_FADEOUT = 0x53,
            }

            public enum TargetID
            {
                TRIANGLE = 0x00,
                CIRCLE = 0x01,
                CROSS = 0x02,
                SQUARE = 0x03,
                TRIANGLE_DOUBLE = 0x04,
                CIRCLE_DOUBLE = 0x05,
                CROSS_DOUBLE = 0x06,
                SQUARE_DOUBLE = 0x07,
                TRIANGLE_HOLD = 0x08,
                CIRCLE_HOLD = 0x09,
                CROSS_HOLD = 0x0A,
                SQUARE_HOLD = 0x0B,
                STAR = 0x0C,
                STAR_HOLD = 0x0D,
                STAR_DOUBLE = 0x0E,
                CHANCE_STAR = 0x0F,
                EDIT_CHANCE_STAR = 0x10,
                STAR_UP = 0x12,
                STAR_RIGHT = 0x13,
                STAR_DOWN = 0x14,
                STAR_LEFT = 0x15,
            }
        }

        public class FT
        {
            public static int GetFuncSize(int ID)
            {
                switch ((FuncID)ID)
                {
                    case FuncID.END: return 0;
                    case FuncID.TIME: return 1;
                    case FuncID.MIKU_MOVE: return 4;
                    case FuncID.MIKU_ROT: return 2;
                    case FuncID.MIKU_DISP: return 2;
                    case FuncID.MIKU_SHADOW: return 2;
                    case FuncID.TARGET: return 7;
                    case FuncID.SET_MOTION: return 4;
                    case FuncID.SET_PLAYDATA: return 2;
                    case FuncID.EFFECT: return 6;
                    case FuncID.FADEIN_FIELD: return 2;
                    case FuncID.EFFECT_OFF: return 1;
                    case FuncID.SET_CAMERA: return 6;
                    case FuncID.DATA_CAMERA: return 2;
                    case FuncID.CHANGE_FIELD: return 1;
                    case FuncID.HIDE_FIELD: return 1;
                    case FuncID.MOVE_FIELD: return 3;
                    case FuncID.FADEOUT_FIELD: return 2;
                    case FuncID.EYE_ANIM: return 3;
                    case FuncID.MOUTH_ANIM: return 5;
                    case FuncID.HAND_ANIM: return 5;
                    case FuncID.LOOK_ANIM: return 4;
                    case FuncID.EXPRESSION: return 4;
                    case FuncID.LOOK_CAMERA: return 5;
                    case FuncID.LYRIC: return 2;
                    case FuncID.MUSIC_PLAY: return 0;
                    case FuncID.MODE_SELECT: return 2;
                    case FuncID.EDIT_MOTION: return 4;
                    case FuncID.BAR_TIME_SET: return 2;
                    case FuncID.SHADOWHEIGHT: return 2;
                    case FuncID.EDIT_FACE: return 1;
                    case FuncID.MOVE_CAMERA: return 21;
                    case FuncID.PV_END: return 1;
                    case FuncID.SHADOWPOS: return 3;
                    case FuncID.EDIT_LYRIC: return 2;
                    case FuncID.EDIT_TARGET: return 5;
                    case FuncID.EDIT_MOUTH: return 1;
                    case FuncID.SET_CHARA: return 1;
                    case FuncID.EDIT_MOVE: return 7;
                    case FuncID.EDIT_SHADOW: return 1;
                    case FuncID.EDIT_EYELID: return 1;
                    case FuncID.EDIT_EYE: return 2;
                    case FuncID.EDIT_ITEM: return 1;
                    case FuncID.EDIT_EFFECT: return 2;
                    case FuncID.EDIT_DISP: return 1;
                    case FuncID.EDIT_HAND_ANIM: return 2;
                    case FuncID.AIM: return 3;
                    case FuncID.HAND_ITEM: return 3;
                    case FuncID.EDIT_BLUSH: return 1;
                    case FuncID.NEAR_CLIP: return 2;
                    case FuncID.CLOTH_WET: return 2;
                    case FuncID.LIGHT_ROT: return 3;
                    case FuncID.SCENE_FADE: return 6;
                    case FuncID.TONE_TRANS: return 6;
                    case FuncID.SATURATE: return 1;
                    case FuncID.FADE_MODE: return 1;
                    case FuncID.AUTO_BLINK: return 2;
                    case FuncID.PARTS_DISP: return 3;
                    case FuncID.TARGET_FLYING_TIME: return 1;
                    case FuncID.CHARA_SIZE: return 2;
                    case FuncID.CHARA_HEIGHT_ADJUST: return 2;
                    case FuncID.ITEM_ANIM: return 4;
                    case FuncID.CHARA_POS_ADJUST: return 4;
                    case FuncID.SCENE_ROT: return 1;
                    case FuncID.MOT_SMOOTH: return 2;
                    case FuncID.PV_BRANCH_MODE: return 1;
                    case FuncID.DATA_CAMERA_START: return 2;
                    case FuncID.MOVIE_PLAY: return 1;
                    case FuncID.MOVIE_DISP: return 1;
                    case FuncID.WIND: return 3;
                    case FuncID.OSAGE_STEP: return 3;
                    case FuncID.OSAGE_MV_CCL: return 3;
                    case FuncID.CHARA_COLOR: return 2;
                    case FuncID.SE_EFFECT: return 1;
                    case FuncID.EDIT_MOVE_XYZ: return 9;
                    case FuncID.EDIT_EYELID_ANIM: return 3;
                    case FuncID.EDIT_INSTRUMENT_ITEM: return 2;
                    case FuncID.EDIT_MOTION_LOOP: return 4;
                    case FuncID.EDIT_EXPRESSION: return 2;
                    case FuncID.EDIT_EYE_ANIM: return 3;
                    case FuncID.EDIT_MOUTH_ANIM: return 2;
                    case FuncID.EDIT_CAMERA: return 24;
                    case FuncID.EDIT_MODE_SELECT: return 1;
                    case FuncID.PV_END_FADEOUT: return 2;
                    case FuncID.TARGET_FLAG: return 1;
                    case FuncID.ITEM_ANIM_ATTACH: return 3;
                    case FuncID.SHADOW_RANGE: return 1;
                    case FuncID.HAND_SCALE: return 3;
                    case FuncID.LIGHT_POS: return 4;
                    case FuncID.FACE_TYPE: return 1;
                    case FuncID.SHADOW_CAST: return 2;
                    case FuncID.EDIT_MOTION_F: return 6;
                    case FuncID.FOG: return 3;
                    case FuncID.BLOOM: return 2;
                    case FuncID.COLOR_COLLE: return 3;
                    case FuncID.DOF: return 3;
                    case FuncID.CHARA_ALPHA: return 4;
                    case FuncID.AOTO_CAP: return 1;
                    case FuncID.MAN_CAP: return 1;
                    case FuncID.TOON: return 3;
                    case FuncID.SHIMMER: return 3;
                    case FuncID.ITEM_ALPHA: return 4;
                    case FuncID.MOVIE_CUT_CHG: return 2;
                    case FuncID.CHARA_LIGHT: return 3;
                    case FuncID.STAGE_LIGHT: return 3;
                    case FuncID.AGEAGE_CTRL: return 8;
                    case FuncID.PSE: return 2;
                }
                return 0;
            }

            public enum FuncID
            {
                END = 0x00,
                TIME = 0x01,
                MIKU_MOVE = 0x02,
                MIKU_ROT = 0x03,
                MIKU_DISP = 0x04,
                MIKU_SHADOW = 0x05,
                TARGET = 0x06,
                SET_MOTION = 0x07,
                SET_PLAYDATA = 0x08,
                EFFECT = 0x09,
                FADEIN_FIELD = 0x0A,
                EFFECT_OFF = 0x0B,
                SET_CAMERA = 0x0C,
                DATA_CAMERA = 0x0D,
                CHANGE_FIELD = 0x0E,
                HIDE_FIELD = 0x0F,
                MOVE_FIELD = 0x10,
                FADEOUT_FIELD = 0x11,
                EYE_ANIM = 0x12,
                MOUTH_ANIM = 0x13,
                HAND_ANIM = 0x14,
                LOOK_ANIM = 0x15,
                EXPRESSION = 0x16,
                LOOK_CAMERA = 0x17,
                LYRIC = 0x18,
                MUSIC_PLAY = 0x19,
                MODE_SELECT = 0x1A,
                EDIT_MOTION = 0x1B,
                BAR_TIME_SET = 0x1C,
                SHADOWHEIGHT = 0x1D,
                EDIT_FACE = 0x1E,
                MOVE_CAMERA = 0x1F,
                PV_END = 0x20,
                SHADOWPOS = 0x21,
                EDIT_LYRIC = 0x22,
                EDIT_TARGET = 0x23,
                EDIT_MOUTH = 0x24,
                SET_CHARA = 0x25,
                EDIT_MOVE = 0x26,
                EDIT_SHADOW = 0x27,
                EDIT_EYELID = 0x28,
                EDIT_EYE = 0x29,
                EDIT_ITEM = 0x2A,
                EDIT_EFFECT = 0x2B,
                EDIT_DISP = 0x2C,
                EDIT_HAND_ANIM = 0x2D,
                AIM = 0x2E,
                HAND_ITEM = 0x2F,
                EDIT_BLUSH = 0x30,
                NEAR_CLIP = 0x31,
                CLOTH_WET = 0x32,
                LIGHT_ROT = 0x33,
                SCENE_FADE = 0x34,
                TONE_TRANS = 0x35,
                SATURATE = 0x36,
                FADE_MODE = 0x37,
                AUTO_BLINK = 0x38,
                PARTS_DISP = 0x39,
                TARGET_FLYING_TIME = 0x3A,
                CHARA_SIZE = 0x3B,
                CHARA_HEIGHT_ADJUST = 0x3C,
                ITEM_ANIM = 0x3D,
                CHARA_POS_ADJUST = 0x3E,
                SCENE_ROT = 0x3F,
                MOT_SMOOTH = 0x40,
                PV_BRANCH_MODE = 0x41,
                DATA_CAMERA_START = 0x42,
                MOVIE_PLAY = 0x43,
                MOVIE_DISP = 0x44,
                WIND = 0x45,
                OSAGE_STEP = 0x46,
                OSAGE_MV_CCL = 0x47,
                CHARA_COLOR = 0x48,
                SE_EFFECT = 0x49,
                EDIT_MOVE_XYZ = 0x4A,
                EDIT_EYELID_ANIM = 0x4B,
                EDIT_INSTRUMENT_ITEM = 0x4C,
                EDIT_MOTION_LOOP = 0x4D,
                EDIT_EXPRESSION = 0x4E,
                EDIT_EYE_ANIM = 0x4F,
                EDIT_MOUTH_ANIM = 0x50,
                EDIT_CAMERA = 0x51,
                EDIT_MODE_SELECT = 0x52,
                PV_END_FADEOUT = 0x53,
                TARGET_FLAG = 0x54,
                ITEM_ANIM_ATTACH = 0x55,
                SHADOW_RANGE = 0x56,
                HAND_SCALE = 0x57,
                LIGHT_POS = 0x58,
                FACE_TYPE = 0x59,
                SHADOW_CAST = 0x5A,
                EDIT_MOTION_F = 0x5B,
                FOG = 0x5C,
                BLOOM = 0x5D,
                COLOR_COLLE = 0x5E,
                DOF = 0x5F,
                CHARA_ALPHA = 0x60,
                AOTO_CAP = 0x61,
                MAN_CAP = 0x62,
                TOON = 0x63,
                SHIMMER = 0x64,
                ITEM_ALPHA = 0x65,
                MOVIE_CUT_CHG = 0x66,
                CHARA_LIGHT = 0x67,
                STAGE_LIGHT = 0x68,
                AGEAGE_CTRL = 0x69,
                PSE = 0x6A,
            }

            public enum TargetID
            {
                TRIANGLE = 0x00,
                CIRCLE = 0x01,
                CROSS = 0x02,
                SQUARE = 0x03,
                TRIANGLE_HOLD = 0x04,
                CIRCLE_HOLD = 0x05,
                CROSS_HOLD = 0x06,
                SQUARE_HOLD = 0x07,
                RANDOM = 0x08,
                RANDOM_HOLD = 0x09,
                PREVIOUS = 0x0A,
                SLIDE_L = 0x0C,
                SLIDE_R = 0x0D,
                SLIDE_CHAIN_L = 0x0F,
                SLIDE_CHAIN_R = 0x10,
                GREEN_SQUARE = 0x11,
                TRIANGLE_CHANCE = 0x12,
                CIRCLE_CHANCE = 0x13,
                CROSS_CHANCE = 0x14,
                SQUARE_CHANCE = 0x15,
                SLIDE_L_CHANCE = 0x17,
                SLIDE_R_CHANCE = 0x18,
            }
        }

        public class F2
        {
            public static int GetFuncSize(int ID)
            {
                switch ((FuncID)ID)
                {
                    case FuncID.END: return 0;
                    case FuncID.TIME: return 1;
                    case FuncID.MIKU_MOVE: return 4;
                    case FuncID.MIKU_ROT: return 2;
                    case FuncID.MIKU_DISP: return 2;
                    case FuncID.MIKU_SHADOW: return 2;
                    case FuncID.TARGET: return 12;
                    case FuncID.SET_MOTION: return 4;
                    case FuncID.SET_PLAYDATA: return 2;
                    case FuncID.EFFECT: return 6;
                    case FuncID.FADEIN_FIELD: return 2;
                    case FuncID.EFFECT_OFF: return 1;
                    case FuncID.SET_CAMERA: return 6;
                    case FuncID.DATA_CAMERA: return 2;
                    case FuncID.CHANGE_FIELD: return 2;
                    case FuncID.HIDE_FIELD: return 1;
                    case FuncID.MOVE_FIELD: return 3;
                    case FuncID.FADEOUT_FIELD: return 2;
                    case FuncID.EYE_ANIM: return 3;
                    case FuncID.MOUTH_ANIM: return 5;
                    case FuncID.HAND_ANIM: return 5;
                    case FuncID.LOOK_ANIM: return 4;
                    case FuncID.EXPRESSION: return 4;
                    case FuncID.LOOK_CAMERA: return 5;
                    case FuncID.LYRIC: return 2;
                    case FuncID.MUSIC_PLAY: return 0;
                    case FuncID.MODE_SELECT: return 2;
                    case FuncID.EDIT_MOTION: return 4;
                    case FuncID.BAR_TIME_SET: return 2;
                    case FuncID.SHADOWHEIGHT: return 2;
                    case FuncID.EDIT_FACE: return 1;
                    case FuncID.MOVE_CAMERA: return 21;
                    case FuncID.PV_END: return 1;
                    case FuncID.SHADOWPOS: return 3;
                    case FuncID.EDIT_LYRIC: return 2;
                    case FuncID.EDIT_TARGET: return 5;
                    case FuncID.EDIT_MOUTH: return 1;
                    case FuncID.SET_CHARA: return 1;
                    case FuncID.EDIT_MOVE: return 7;
                    case FuncID.EDIT_SHADOW: return 1;
                    case FuncID.EDIT_EYELID: return 1;
                    case FuncID.EDIT_EYE: return 2;
                    case FuncID.EDIT_ITEM: return 1;
                    case FuncID.EDIT_EFFECT: return 2;
                    case FuncID.EDIT_DISP: return 1;
                    case FuncID.EDIT_HAND_ANIM: return 2;
                    case FuncID.AIM: return 3;
                    case FuncID.HAND_ITEM: return 3;
                    case FuncID.EDIT_BLUSH: return 1;
                    case FuncID.NEAR_CLIP: return 2;
                    case FuncID.CLOTH_WET: return 2;
                    case FuncID.LIGHT_ROT: return 3;
                    case FuncID.SCENE_FADE: return 6;
                    case FuncID.TONE_TRANS: return 6;
                    case FuncID.SATURATE: return 1;
                    case FuncID.FADE_MODE: return 1;
                    case FuncID.AUTO_BLINK: return 2;
                    case FuncID.PARTS_DISP: return 3;
                    case FuncID.TARGET_FLYING_TIME: return 1;
                    case FuncID.CHARA_SIZE: return 2;
                    case FuncID.CHARA_HEIGHT_ADJUST: return 2;
                    case FuncID.ITEM_ANIM: return 4;
                    case FuncID.CHARA_POS_ADJUST: return 4;
                    case FuncID.SCENE_ROT: return 1;
                    case FuncID.EDIT_MOT_SMOOTH_LEN: return 2;
                    case FuncID.PV_BRANCH_MODE: return 1;
                    case FuncID.DATA_CAMERA_START: return 2;
                    case FuncID.MOVIE_PLAY: return 1;
                    case FuncID.MOVIE_DISP: return 1;
                    case FuncID.WIND: return 3;
                    case FuncID.OSAGE_STEP: return 3;
                    case FuncID.OSAGE_MV_CCL: return 3;
                    case FuncID.CHARA_COLOR: return 2;
                    case FuncID.SE_EFFECT: return 1;
                    case FuncID.EDIT_MOVE_XYZ: return 9;
                    case FuncID.EDIT_EYELID_ANIM: return 3;
                    case FuncID.EDIT_INSTRUMENT_ITEM: return 2;
                    case FuncID.EDIT_MOTION_LOOP: return 4;
                    case FuncID.EDIT_EXPRESSION: return 2;
                    case FuncID.EDIT_EYE_ANIM: return 3;
                    case FuncID.EDIT_MOUTH_ANIM: return 2;
                    case FuncID.EDIT_CAMERA: return 22;
                    case FuncID.EDIT_MODE_SELECT: return 1;
                    case FuncID.PV_END_FADEOUT: return 2;
                    case FuncID.RESERVE0: return 1;
                    case FuncID.RESERVE1: return 6;
                    case FuncID.RESERVE2: return 1;
                    case FuncID.RESERVE3: return 9;
                    case FuncID.PV_AUTH_LIGHT_PRIORITY: return 2;
                    case FuncID.PV_CHARA_LIGHT: return 3;
                    case FuncID.PV_STAGE_LIGHT: return 3;
                    case FuncID.TARGET_EFFECT: return 11;
                    case FuncID.FOG: return 3;
                    case FuncID.BLOOM: return 2;
                    case FuncID.COLOR_CORRECTION: return 3;
                    case FuncID.DOF: return 3;
                    case FuncID.CHARA_ALPHA: return 4;
                    case FuncID.AUTO_CAPTURE_BEGIN: return 1;
                    case FuncID.MANUAL_CAPTURE: return 1;
                    case FuncID.TOON_EDGE: return 3;
                    case FuncID.SHIMMER: return 3;
                    case FuncID.ITEM_ALPHA: return 4;
                    case FuncID.MOVIE_CUT: return 1;
                    case FuncID.EDIT_CAMERA_BOX: return 112;
                    case FuncID.EDIT_STAGE_PARAM: return 1;
                    case FuncID.EDIT_CHANGE_FIELD: return 1;
                    case FuncID.MIKUDAYO_ADJUST: return 7;
                    case FuncID.LYRIC_2: return 2;
                    case FuncID.LYRIC_READ: return 2;
                    case FuncID.LYRIC_READ_2: return 2;
                    case FuncID.ANNOTATION: return 5;
                }
                return 0;
            }

            public enum FuncID
            {
                END = 0x00,
                TIME = 0x01,
                MIKU_MOVE = 0x02,
                MIKU_ROT = 0x03,
                MIKU_DISP = 0x04,
                MIKU_SHADOW = 0x05,
                TARGET = 0x06,
                SET_MOTION = 0x07,
                SET_PLAYDATA = 0x08,
                EFFECT = 0x09,
                FADEIN_FIELD = 0x0A,
                EFFECT_OFF = 0x0B,
                SET_CAMERA = 0x0C,
                DATA_CAMERA = 0x0D,
                CHANGE_FIELD = 0x0E,
                HIDE_FIELD = 0x0F,
                MOVE_FIELD = 0x10,
                FADEOUT_FIELD = 0x11,
                EYE_ANIM = 0x12,
                MOUTH_ANIM = 0x13,
                HAND_ANIM = 0x14,
                LOOK_ANIM = 0x15,
                EXPRESSION = 0x16,
                LOOK_CAMERA = 0x17,
                LYRIC = 0x18,
                MUSIC_PLAY = 0x19,
                MODE_SELECT = 0x1A,
                EDIT_MOTION = 0x1B,
                BAR_TIME_SET = 0x1C,
                SHADOWHEIGHT = 0x1D,
                EDIT_FACE = 0x1E,
                MOVE_CAMERA = 0x1F,
                PV_END = 0x20,
                SHADOWPOS = 0x21,
                EDIT_LYRIC = 0x22,
                EDIT_TARGET = 0x23,
                EDIT_MOUTH = 0x24,
                SET_CHARA = 0x25,
                EDIT_MOVE = 0x26,
                EDIT_SHADOW = 0x27,
                EDIT_EYELID = 0x28,
                EDIT_EYE = 0x29,
                EDIT_ITEM = 0x2A,
                EDIT_EFFECT = 0x2B,
                EDIT_DISP = 0x2C,
                EDIT_HAND_ANIM = 0x2D,
                AIM = 0x2E,
                HAND_ITEM = 0x2F,
                EDIT_BLUSH = 0x30,
                NEAR_CLIP = 0x31,
                CLOTH_WET = 0x32,
                LIGHT_ROT = 0x33,
                SCENE_FADE = 0x34,
                TONE_TRANS = 0x35,
                SATURATE = 0x36,
                FADE_MODE = 0x37,
                AUTO_BLINK = 0x38,
                PARTS_DISP = 0x39,
                TARGET_FLYING_TIME = 0x3A,
                CHARA_SIZE = 0x3B,
                CHARA_HEIGHT_ADJUST = 0x3C,
                ITEM_ANIM = 0x3D,
                CHARA_POS_ADJUST = 0x3E,
                SCENE_ROT = 0x3F,
                EDIT_MOT_SMOOTH_LEN = 0x40,
                PV_BRANCH_MODE = 0x41,
                DATA_CAMERA_START = 0x42,
                MOVIE_PLAY = 0x43,
                MOVIE_DISP = 0x44,
                WIND = 0x45,
                OSAGE_STEP = 0x46,
                OSAGE_MV_CCL = 0x47,
                CHARA_COLOR = 0x48,
                SE_EFFECT = 0x49,
                EDIT_MOVE_XYZ = 0x4A,
                EDIT_EYELID_ANIM = 0x4B,
                EDIT_INSTRUMENT_ITEM = 0x4C,
                EDIT_MOTION_LOOP = 0x4D,
                EDIT_EXPRESSION = 0x4E,
                EDIT_EYE_ANIM = 0x4F,
                EDIT_MOUTH_ANIM = 0x50,
                EDIT_CAMERA = 0x51,
                EDIT_MODE_SELECT = 0x52,
                PV_END_FADEOUT = 0x53,
                RESERVE0 = 0x54,
                RESERVE1 = 0x55,
                RESERVE2 = 0x56,
                RESERVE3 = 0x57,
                PV_AUTH_LIGHT_PRIORITY = 0x58,
                PV_CHARA_LIGHT = 0x59,
                PV_STAGE_LIGHT = 0x5A,
                TARGET_EFFECT = 0x5B,
                FOG = 0x5C,
                BLOOM = 0x5D,
                COLOR_CORRECTION = 0x5E,
                DOF = 0x5F,
                CHARA_ALPHA = 0x60,
                AUTO_CAPTURE_BEGIN = 0x61,
                MANUAL_CAPTURE = 0x62,
                TOON_EDGE = 0x63,
                SHIMMER = 0x64,
                ITEM_ALPHA = 0x65,
                MOVIE_CUT = 0x66,
                EDIT_CAMERA_BOX = 0x67,
                EDIT_STAGE_PARAM = 0x68,
                EDIT_CHANGE_FIELD = 0x69,
                MIKUDAYO_ADJUST = 0x6A,
                LYRIC_2 = 0x6B,
                LYRIC_READ = 0x6C,
                LYRIC_READ_2 = 0x6D,
                ANNOTATION = 0x6E,
            }

            public enum TargetID
            {
                TRIANGLE = 0x00,
                CIRCLE = 0x01,
                CROSS = 0x02,
                SQUARE = 0x03,
                TRIANGLE_DOUBLE = 0x04,
                CIRCLE_DOUBLE = 0x05,
                CROSS_DOUBLE = 0x06,
                SQUARE_DOUBLE = 0x07,
                TRIANGLE_HOLD = 0x08,
                CIRCLE_HOLD = 0x09,
                CROSS_HOLD = 0x0A,
                SQUARE_HOLD = 0x0B,
                STAR = 0x0C,
                STAR_HOLD = 0x0D,
                STAR_DOUBLE = 0x0E,
                CHANCE_STAR = 0x0F,
                EDIT_CHANCE_STAR = 0x10,
                LINK_STAR = 0x16,
                LINK_STAR_END = 0x17,
            }
        }

        public class X
        {
            public static int GetFuncSize(int ID)
            {
                switch ((FuncID)ID)
                {
                    case FuncID.END: return 0;
                    case FuncID.TIME: return 1;
                    case FuncID.MIKU_MOVE: return 4;
                    case FuncID.MIKU_ROT: return 2;
                    case FuncID.MIKU_DISP: return 2;
                    case FuncID.MIKU_SHADOW: return 2;
                    case FuncID.TARGET: return 12;
                    case FuncID.SET_MOTION: return 4;
                    case FuncID.SET_PLAYDATA: return 2;
                    case FuncID.EFFECT: return 6;
                    case FuncID.FADEIN_FIELD: return 2;
                    case FuncID.EFFECT_OFF: return 1;
                    case FuncID.SET_CAMERA: return 6;
                    case FuncID.DATA_CAMERA: return 2;
                    case FuncID.CHANGE_FIELD: return 2;
                    case FuncID.HIDE_FIELD: return 1;
                    case FuncID.MOVE_FIELD: return 3;
                    case FuncID.FADEOUT_FIELD: return 2;
                    case FuncID.EYE_ANIM: return 3;
                    case FuncID.MOUTH_ANIM: return 5;
                    case FuncID.HAND_ANIM: return 5;
                    case FuncID.LOOK_ANIM: return 4;
                    case FuncID.EXPRESSION: return 4;
                    case FuncID.LOOK_CAMERA: return 5;
                    case FuncID.LYRIC: return 2;
                    case FuncID.MUSIC_PLAY: return 0;
                    case FuncID.MODE_SELECT: return 2;
                    case FuncID.EDIT_MOTION: return 4;
                    case FuncID.BAR_TIME_SET: return 2;
                    case FuncID.SHADOWHEIGHT: return 2;
                    case FuncID.EDIT_FACE: return 1;
                    case FuncID.MOVE_CAMERA: return 21;
                    case FuncID.PV_END: return 1;
                    case FuncID.SHADOWPOS: return 3;
                    case FuncID.EDIT_LYRIC: return 2;
                    case FuncID.EDIT_TARGET: return 5;
                    case FuncID.EDIT_MOUTH: return 1;
                    case FuncID.SET_CHARA: return 1;
                    case FuncID.EDIT_MOVE: return 7;
                    case FuncID.EDIT_SHADOW: return 1;
                    case FuncID.EDIT_EYELID: return 1;
                    case FuncID.EDIT_EYE: return 2;
                    case FuncID.EDIT_ITEM: return 1;
                    case FuncID.EDIT_EFFECT: return 2;
                    case FuncID.EDIT_DISP: return 1;
                    case FuncID.EDIT_HAND_ANIM: return 2;
                    case FuncID.AIM: return 3;
                    case FuncID.HAND_ITEM: return 3;
                    case FuncID.EDIT_BLUSH: return 1;
                    case FuncID.NEAR_CLIP: return 2;
                    case FuncID.CLOTH_WET: return 2;
                    case FuncID.LIGHT_ROT: return 3;
                    case FuncID.SCENE_FADE: return 6;
                    case FuncID.TONE_TRANS: return 6;
                    case FuncID.SATURATE: return 1;
                    case FuncID.FADE_MODE: return 1;
                    case FuncID.AUTO_BLINK: return 2;
                    case FuncID.PARTS_DISP: return 3;
                    case FuncID.TARGET_FLYING_TIME: return 1;
                    case FuncID.CHARA_SIZE: return 2;
                    case FuncID.CHARA_HEIGHT_ADJUST: return 2;
                    case FuncID.ITEM_ANIM: return 4;
                    case FuncID.CHARA_POS_ADJUST: return 4;
                    case FuncID.SCENE_ROT: return 1;
                    case FuncID.EDIT_MOT_SMOOTH_LEN: return 2;
                    case FuncID.PV_BRANCH_MODE: return 1;
                    case FuncID.DATA_CAMERA_START: return 2;
                    case FuncID.MOVIE_PLAY: return 1;
                    case FuncID.MOVIE_DISP: return 1;
                    case FuncID.WIND: return 3;
                    case FuncID.OSAGE_STEP: return 3;
                    case FuncID.OSAGE_MV_CCL: return 3;
                    case FuncID.CHARA_COLOR: return 2;
                    case FuncID.SE_EFFECT: return 1;
                    case FuncID.EDIT_MOVE_XYZ: return 2;
                    case FuncID.EDIT_EYELID_ANIM: return 2;
                    case FuncID.EDIT_INSTRUMENT_ITEM: return 2;
                    case FuncID.EDIT_MOTION_LOOP: return 4;
                    case FuncID.EDIT_EXPRESSION: return 2;
                    case FuncID.EDIT_EYE_ANIM: return 3;
                    case FuncID.EDIT_MOUTH_ANIM: return 2;
                    case FuncID.EDIT_CAMERA: return 22;
                    case FuncID.EDIT_MODE_SELECT: return 2;
                    case FuncID.PV_END_FADEOUT: return 2;
                    case FuncID.CREDIT_TITLE: return 1;
                    case FuncID.BAR_POINT: return 1;
                    case FuncID.BEAT_POINT: return 1;
                    case FuncID.RESERVE: return 9;
                    case FuncID.PV_AUTH_LIGHT_PRIORITY: return 2;
                    case FuncID.PV_CHARA_LIGHT: return 3;
                    case FuncID.PV_STAGE_LIGHT: return 3;
                    case FuncID.TARGET_EFFECT: return 11;
                    case FuncID.FOG: return 3;
                    case FuncID.BLOOM: return 2;
                    case FuncID.COLOR_CORRECTION: return 3;
                    case FuncID.DOF: return 3;
                    case FuncID.CHARA_ALPHA: return 4;
                    case FuncID.AUTO_CAPTURE_BEGIN: return 1;
                    case FuncID.MANUAL_CAPTURE: return 1;
                    case FuncID.TOON_EDGE: return 3;
                    case FuncID.SHIMMER: return 3;
                    case FuncID.ITEM_ALPHA: return 4;
                    case FuncID.MOVIE_CUT: return 1;
                    case FuncID.EDIT_CAMERA_BOX: return 112;
                    case FuncID.EDIT_STAGE_PARAM: return 1;
                    case FuncID.EDIT_CHANGE_FIELD: return 1;
                    case FuncID.MIKUDAYO_ADJUST: return 7;
                    case FuncID.LYRIC_2: return 2;
                    case FuncID.LYRIC_READ: return 2;
                    case FuncID.LYRIC_READ_2: return 2;
                    case FuncID.ANNOTATION: return 5;
                    case FuncID.STAGE_EFFECT: return 2;
                    case FuncID.SONG_EFFECT: return 3;
                    case FuncID.SONG_EFFECT_ATTACH: return 3;
                    case FuncID.LIGHT_AUTH: break;
                    case FuncID.FADE: break;
                    case FuncID.SET_STAGE_EFFECT_ENV: return 2;
                    case FuncID.COMMON_EFFECT_AET_FRONT: break;
                    case FuncID.COMMON_EFFECT_AET_FRONT_LOW: break;
                    case FuncID.COMMON_EFFECT_PARTICLE: break;
                    case FuncID.SONG_EFFECT_ALPHA_SORT: break;
                    case FuncID.LOOK_CAMERA_FACE_LIMIT: break;
                    case FuncID.ITEM_LIGHT: break;
                    case FuncID.CHARA_EFFECT: break;
                    case FuncID.MARKER: return 3;
                    case FuncID.CHARA_EFFECT_CHARA_LIGHT: return 2;
                    case FuncID.ENABLE_COMMON_LIGHT_TO_CHARA: return 3;
                    case FuncID.ENABLE_FXAA: break;
                    case FuncID.ENABLE_TEMPORAL_AA: break;
                    case FuncID.ENABLE_REFLECTION: return 2;
                    case FuncID.BANK_BRANCH: return 2;
                    case FuncID.BANK_END: break;
                    case FuncID.VR_LIVE_MOVIE: break;
                    case FuncID.VR_CHEER: break;
                    case FuncID.VR_CHARA_PSMOVE: break;
                    case FuncID.VR_MOVE_PATH: break;
                    case FuncID.VR_SET_BASE: break;
                    case FuncID.VR_TECH_DEMO_EFFECT: break;
                    case FuncID.VR_TRANSFORM: break;
                    case FuncID.GAZE: break;
                    case FuncID.TECH_DEMO_GESUTRE: break;
                    case FuncID.VR_CHEMICAL_LIGHT_COLOR: break;
                    case FuncID.VR_LIVE_MOB: break;
                    case FuncID.VR_LIVE_HAIR_OSAGE: break;
                    case FuncID.VR_LIVE_LOOK_CAMERA: break;
                    case FuncID.VR_LIVE_CHEER: break;
                    case FuncID.VR_LIVE_GESTURE: break;
                    case FuncID.VR_LIVE_CLONE: break;
                    case FuncID.VR_LOOP_EFFECT: break;
                    case FuncID.VR_LIVE_ONESHOT_EFFECT: break;
                    case FuncID.VR_LIVE_PRESENT: break;
                    case FuncID.VR_LIVE_TRANSFORM: break;
                    case FuncID.VR_LIVE_FLY: break;
                    case FuncID.VR_LIVE_CHARA_VOICE: break;
                }
                return 2;
            }

            public enum FuncID
            {
                END = 0x00,
                TIME = 0x01,
                MIKU_MOVE = 0x02,
                MIKU_ROT = 0x03,
                MIKU_DISP = 0x04,
                MIKU_SHADOW = 0x05,
                TARGET = 0x06,
                SET_MOTION = 0x07,
                SET_PLAYDATA = 0x08,
                EFFECT = 0x09,
                FADEIN_FIELD = 0x0A,
                EFFECT_OFF = 0x0B,
                SET_CAMERA = 0x0C,
                DATA_CAMERA = 0x0D,
                CHANGE_FIELD = 0x0E,
                HIDE_FIELD = 0x0F,
                MOVE_FIELD = 0x10,
                FADEOUT_FIELD = 0x11,
                EYE_ANIM = 0x12,
                MOUTH_ANIM = 0x13,
                HAND_ANIM = 0x14,
                LOOK_ANIM = 0x15,
                EXPRESSION = 0x16,
                LOOK_CAMERA = 0x17,
                LYRIC = 0x18,
                MUSIC_PLAY = 0x19,
                MODE_SELECT = 0x1A,
                EDIT_MOTION = 0x1B,
                BAR_TIME_SET = 0x1C,
                SHADOWHEIGHT = 0x1D,
                EDIT_FACE = 0x1E,
                MOVE_CAMERA = 0x1F,
                PV_END = 0x20,
                SHADOWPOS = 0x21,
                EDIT_LYRIC = 0x22,
                EDIT_TARGET = 0x23,
                EDIT_MOUTH = 0x24,
                SET_CHARA = 0x25,
                EDIT_MOVE = 0x26,
                EDIT_SHADOW = 0x27,
                EDIT_EYELID = 0x28,
                EDIT_EYE = 0x29,
                EDIT_ITEM = 0x2A,
                EDIT_EFFECT = 0x2B,
                EDIT_DISP = 0x2C,
                EDIT_HAND_ANIM = 0x2D,
                AIM = 0x2E,
                HAND_ITEM = 0x2F,
                EDIT_BLUSH = 0x30,
                NEAR_CLIP = 0x31,
                CLOTH_WET = 0x32,
                LIGHT_ROT = 0x33,
                SCENE_FADE = 0x34,
                TONE_TRANS = 0x35,
                SATURATE = 0x36,
                FADE_MODE = 0x37,
                AUTO_BLINK = 0x38,
                PARTS_DISP = 0x39,
                TARGET_FLYING_TIME = 0x3A,
                CHARA_SIZE = 0x3B,
                CHARA_HEIGHT_ADJUST = 0x3C,
                ITEM_ANIM = 0x3D,
                CHARA_POS_ADJUST = 0x3E,
                SCENE_ROT = 0x3F,
                EDIT_MOT_SMOOTH_LEN = 0x40,
                PV_BRANCH_MODE = 0x41,
                DATA_CAMERA_START = 0x42,
                MOVIE_PLAY = 0x43,
                MOVIE_DISP = 0x44,
                WIND = 0x45,
                OSAGE_STEP = 0x46,
                OSAGE_MV_CCL = 0x47,
                CHARA_COLOR = 0x48,
                SE_EFFECT = 0x49,
                EDIT_MOVE_XYZ = 0x4A,
                EDIT_EYELID_ANIM = 0x4B,
                EDIT_INSTRUMENT_ITEM = 0x4C,
                EDIT_MOTION_LOOP = 0x4D,
                EDIT_EXPRESSION = 0x4E,
                EDIT_EYE_ANIM = 0x4F,
                EDIT_MOUTH_ANIM = 0x50,
                EDIT_CAMERA = 0x51,
                EDIT_MODE_SELECT = 0x52,
                PV_END_FADEOUT = 0x53,
                CREDIT_TITLE = 0x54,
                BAR_POINT = 0x55,
                BEAT_POINT = 0x56,
                RESERVE = 0x57,
                PV_AUTH_LIGHT_PRIORITY = 0x58,
                PV_CHARA_LIGHT = 0x59,
                PV_STAGE_LIGHT = 0x5A,
                TARGET_EFFECT = 0x5B,
                FOG = 0x5C,
                BLOOM = 0x5D,
                COLOR_CORRECTION = 0x5E,
                DOF = 0x5F,
                CHARA_ALPHA = 0x60,
                AUTO_CAPTURE_BEGIN = 0x61,
                MANUAL_CAPTURE = 0x62,
                TOON_EDGE = 0x63,
                SHIMMER = 0x64,
                ITEM_ALPHA = 0x65,
                MOVIE_CUT = 0x66,
                EDIT_CAMERA_BOX = 0x67,
                EDIT_STAGE_PARAM = 0x68,
                EDIT_CHANGE_FIELD = 0x69,
                MIKUDAYO_ADJUST = 0x6A,
                LYRIC_2 = 0x6B,
                LYRIC_READ = 0x6C,
                LYRIC_READ_2 = 0x6D,
                ANNOTATION = 0x6E,
                STAGE_EFFECT = 0x6F,
                SONG_EFFECT = 0x70,
                SONG_EFFECT_ATTACH = 0x71,
                LIGHT_AUTH = 0x72,
                FADE = 0x73,
                SET_STAGE_EFFECT_ENV = 0x74,
                COMMON_EFFECT_AET_FRONT = 0x75,
                COMMON_EFFECT_AET_FRONT_LOW = 0x76,
                COMMON_EFFECT_PARTICLE = 0x77,
                SONG_EFFECT_ALPHA_SORT = 0x78,
                LOOK_CAMERA_FACE_LIMIT = 0x79,
                ITEM_LIGHT = 0x7A,
                CHARA_EFFECT = 0x7B,
                MARKER = 0x7C,
                CHARA_EFFECT_CHARA_LIGHT = 0x7D,
                ENABLE_COMMON_LIGHT_TO_CHARA = 0x7E,
                ENABLE_FXAA = 0x7F,
                ENABLE_TEMPORAL_AA = 0x80,
                ENABLE_REFLECTION = 0x81,
                BANK_BRANCH = 0x82,
                BANK_END = 0x83,
                VR_LIVE_MOVIE = 0x84,
                VR_CHEER = 0x85,
                VR_CHARA_PSMOVE = 0x86,
                VR_MOVE_PATH = 0x87,
                VR_SET_BASE = 0x88,
                VR_TECH_DEMO_EFFECT = 0x89,
                VR_TRANSFORM = 0x8A,
                GAZE = 0x8B,
                TECH_DEMO_GESUTRE = 0x8C,
                VR_CHEMICAL_LIGHT_COLOR = 0x8D,
                VR_LIVE_MOB = 0x8E,
                VR_LIVE_HAIR_OSAGE = 0x8F,
                VR_LIVE_LOOK_CAMERA = 0x90,
                VR_LIVE_CHEER = 0x91,
                VR_LIVE_GESTURE = 0x92,
                VR_LIVE_CLONE = 0x93,
                VR_LOOP_EFFECT = 0x94,
                VR_LIVE_ONESHOT_EFFECT = 0x95,
                VR_LIVE_PRESENT = 0x96,
                VR_LIVE_TRANSFORM = 0x97,
                VR_LIVE_FLY = 0x98,
                VR_LIVE_CHARA_VOICE = 0x99,
            }

            public enum TargetID
            {
                TRIANGLE = 0x00,
                CIRCLE = 0x01,
                CROSS = 0x02,
                SQUARE = 0x03,
                TRIANGLE_DOUBLE = 0x04,
                CIRCLE_DOUBLE = 0x05,
                CROSS_DOUBLE = 0x06,
                SQUARE_DOUBLE = 0x07,
                TRIANGLE_HOLD = 0x08,
                CIRCLE_HOLD = 0x09,
                CROSS_HOLD = 0x0A,
                SQUARE_HOLD = 0x0B,
                STAR = 0x0C,
                STAR_HOLD = 0x0D,
                STAR_DOUBLE = 0x0E,
                CHANCE_STAR = 0x0F,
                EDIT_CHANCE_STAR = 0x10,
                STAR_RUSH = 0x11,
                LINK_STAR = 0x16,
                LINK_STAR_END = 0x17,
                CIRCLE_RUSH_TEST = 0x18,
                TRIANGLE_RUSH = 0x19,
                CIRCLE_RUSH = 0x1A,
                CROSS_RUSH = 0x1B,
                SQUARE_RUSH = 0x1C,
            }
        }
    }
}
