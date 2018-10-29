//#define A3DAList
using System;
using System.IO;
using System.Xml;
using System.Text;
using System.Collections.Generic;

namespace PD_Tool.Tools
{
    class A3D
    {
        static readonly bool A3DCOpt = true;
        static readonly string BO = "bin_offset";
        static readonly string MTBO = "model_transform.bin_offset";
        static readonly string Dot = ".";

        static int SOi0 = 0;
        static int SOi1 = 0;
        static int Offset = 0;
        static int[] SO0 = new int[0];
        static int[] SO1 = new int[0];
        static string name = "";
        static string nameInt = "";
        static string nameView = "";
        static string[] dataArray = new string[4];
        static A3DA Data = new A3DA();
        static Values UsedValues = new Values();

#if A3DAList
        static List<string> A = new List<string>();
        static List<string> A1 = new List<string>();
        static List<string> B = new List<string>();
#endif

        public static void Processor()
        {
            Console.Title = "PD_Tool: Converter Tools: A3DA Tools";
            System.Choose(1, "a3da", out string InitialDirectory, out string[] FileNames);

            foreach (string file in FileNames)
            {
                string filepath = file.Replace(Path.GetExtension(file), "");
                string ext = Path.GetExtension(file);
                Console.Title = "PD_Tool: Converter Tools: A3DA Tools: " + Path.GetFileNameWithoutExtension(file);
                switch (ext.ToLower())
                {
                    case ".a3da":
                        A3DAReader(filepath, ext);
#if !A3DAList
                        XMLWriter(filepath);
                        break;
                    case ".xml":
                        XMLReader(filepath);
                        if (Data.Header.Signature == 0x5F5F5F41)
                            A3DAWriter(filepath, false);
                        else if (Data.Header.Signature == 0x5F5F5F43)
                            A3DCWriter(filepath);
#endif
                        break;
                }
                GC.Collect();
            }

#if A3DAList
            A.Sort();
            System.writer = new FileStream(".a3da", FileMode.Create, FileAccess.ReadWrite, FileShare.ReadWrite);
            for (int i = 0; i < A.Count; i++)
                for (int i1 = 0; i1 < A.Count; i1++)
                    if (A[i] == A1[i1])
                    {
                        System.Write(A1[i1]);
                        for (int i2 = 0; i2 < 16 - (int)Math.Round
                            (((double)A1[i1].Length - (A1[i1].Length % 4)) / 4, 0); i2++)
                            System.Write("\t");
                        System.Write("", B[i1]);
                        break;
                    }
            System.writer.Close();
#endif
        }

        static int A3DAReader(string file, string ext)
        {
            name = "";
            Offset = 0;
            nameInt = "";
            nameView = "";
            dataArray = new string[4];

            Data = new A3DA
            {
                _ = new _(),
                Camera = new Camera { Auxiliary = new CameraAuxiliary(), Root = new CameraRoot[0] },
                Chara = new ModelTransform[0],
                Curve = new Curve[0],
                Light = new Light[0],
                DOF = new ModelTransform(),
                Event = new Event[0],
                Fog = new Fog[0],
                Header = new System.Header(),
                MObjectHRC = new MObjectHRC[0],
                MObjectHRCList = new string[0],
                Motion = new string[0],
                Name = new List<string>(),
                Object = new Object[0],
                ObjectList = new string[0],
                ObjectHRC = new ObjectHRC[0],
                ObjectHRCList = new string[0],
                Point = new ModelTransform[0],
                PlayControl = new PlayControl { Begin = 0, FPS = 0, Offset = 0, Size = 0 },
                PostProcess = new PostProcess(),
                Value = new List<string>(),
                Head = new Header()
            };

            System.reader = new FileStream(file + ext, FileMode.Open, FileAccess.ReadWrite, FileShare.ReadWrite);

            System.format = System.Format.F;
            Data.Header.Signature = System.ReadInt32();
            if (Data.Header.Signature == 0x41443341)
                Data.Header = System.HeaderReader();
            else if (Data.Header.Signature != 0x44334123)
                return 0;

            Offset = (int)System.reader.Position - 4;
            Data.Header.Signature = System.ReadInt32();

            if (Data.Header.Signature == 0x5F5F5F41)
                System.reader.Seek(0x10, 0);
            else if (Data.Header.Signature == 0x5F5F5F43)
            {
                System.reader.Seek(Offset + 0x10, 0);
                System.ReadInt32();
                System.ReadInt32();
                Data.Head.HeaderOffset = System.ReadInt32(true, true);

                System.reader.Seek(Offset + Data.Head.HeaderOffset, 0);
                if (System.ReadInt32() != 0x50)
                    return 0;
                Data.Head.StringOffset = System.ReadInt32(true, true);
                Data.Head.StringLength = System.ReadInt32(true, true);
                Data.Head.Count = System.ReadInt32(true, true);
                if (System.ReadInt32() != 0x4C42)
                    return 0;
                Data.Head.BinaryOffset = System.ReadInt32(true, true);
                Data.Head.BinaryLength = System.ReadInt32(true, true);

                System.reader.Seek(Offset + Data.Head.StringOffset, 0);
            }
            else
                return 0;

            byte[] STRData = null;
            if (Data.Header.Signature == 0x5F5F5F43)
                STRData = System.ReadBytes(Data.Head.StringLength);
            else
                STRData = System.ReadBytes((int)System.reader.Length - 0x10);

            string[] TempSTRData = Encoding.UTF8.GetString(STRData).Split('\n');
            STRData = null;
            for (int i = 0; i < TempSTRData.Length; i++)
                if (!TempSTRData[i].StartsWith("#"))
                {
                    dataArray = TempSTRData[i].Split('=');
                    if (dataArray.Length == 2)
                    {
#if A3DAList
                        if (!dataArray[0].Contains("key") && !dataArray[0].Contains("raw_data.value_list"))
                            if (!A.Contains(dataArray[0].Replace(".data", "").Replace(".type", "")))
                            {
                                A.Add(dataArray[0].Replace(".data", "").Replace(".type", ""));
                                A1.Add(dataArray[0].Replace(".data", "").Replace(".type", ""));
                                B.Add(Path.GetFileName(file + ext));
                            }
                    }
                }
#else
                        Data.Name.Add(dataArray[0]);
                        Data.Value.Add(dataArray[1]);
                    }
                }

            A3DAReader();

            if (Data.Header.Signature == 0x5F5F5F43)
            {
                System.reader.Seek(Offset + Data.Head.BinaryOffset, 0);
                Offset = (int)System.reader.Position;
                A3DCReader();
            }
#endif
            System.reader.Close();
            return 1;
        }

#if !A3DAList
        static void A3DAWriter(string file, bool A3DC)
        {
            DateTime date = DateTime.Now;

            if (A3DC)
            {
                if (Data._.CompressF16 != 0)
                    System.Write("#-compress_f16", "");
                System.Write("#" + System.ToTitleCase(date.ToString("ddd") + " " + System.ToTitleCase(
                    date.ToString("MMM") + " " + date.ToString("dd HH:mm:ss yyyy") + "\n")));
                if (Data._.CompressF16 != 0)
                    System.Write("_.compress_f16=", Data._.CompressF16.ToString());
            }
            else
            {
                System.writer = new FileStream(file + ".a3da", FileMode.Create, FileAccess.ReadWrite, FileShare.ReadWrite);
                System.writer.SetLength(0);

                System.Write("#A3DA__________\n");
                System.Write("#" + System.ToTitleCase(date.ToString("ddd") + " " + System.ToTitleCase(
                    date.ToString("MMM") + " " + date.ToString("dd HH:mm:ss yyyy") + "\n")));
            }

            System.Write("_.converter.version=", Data._.ConverterVersion.ToString());
            System.Write("_.file_name=", Data._.FileName.ToString());
            System.Write("_.property.version=", Data._.PropertyVersion.ToString());

            name = "camera_auxiliary.";

            KeyWriter(name, Data.Camera.Auxiliary.AutoExposure, A3DC, "auto_exposure");
            KeyWriter(name, Data.Camera.Auxiliary.Exposure, A3DC, "exposure");
            KeyWriter(name, Data.Camera.Auxiliary.Gamma, A3DC, "gamma");
            KeyWriter(name, Data.Camera.Auxiliary.GammaRate, A3DC, "gamma_rate");
            KeyWriter(name, Data.Camera.Auxiliary.Saturate, A3DC, "saturate");

            if (Data.Camera.Root.Length != 0)
            {
                SO0 = SortWriter(Data.Camera.Root.Length);
                SOi0 = 0;
                for (int i0 = 0; i0 < Data.Camera.Root.Length; i0++)
                {
                    SOi0 = SO0[i0];
                    name = "camera_root." + SOi0 + Dot;
                    nameInt = "camera_root." + SOi0 + ".interest.";
                    nameView = "camera_root." + SOi0 + ".view_point.";

                    KeyWriter(nameInt, ref Data.Camera.Root[SOi0].Interest.MT, 0b1001111, A3DC);
                    KeyWriter(name, ref Data.Camera.Root[SOi0].MT, 0b1001110, A3DC);
                    System.Write(nameView + "aspect=", System.ToString(Data.Camera.Root[i0].ViewPoint.Aspect, 5));
                    if (Data.Camera.Root[i0].ViewPoint.CameraApertureH != -1)
                        System.Write(nameView + "camera_aperture_h=",
                            System.ToString(Data.Camera.Root[i0].ViewPoint.CameraApertureH, 5));
                    if (Data.Camera.Root[i0].ViewPoint.CameraApertureW != -1)
                        System.Write(nameView + "camera_aperture_w=",
                            System.ToString(Data.Camera.Root[i0].ViewPoint.CameraApertureW, 5));
                    KeyWriter(nameView + "focal_length.", Data.Camera.Root[SOi0].ViewPoint.FocalLength, A3DC);
                    KeyWriter(nameView + "fov.", Data.Camera.Root[SOi0].ViewPoint.FOV, A3DC);
                    System.Write(nameView + "fov_is_horizontal=",
                        Data.Camera.Root[i0].ViewPoint.FOVHorizontal.ToString());
                    KeyWriter(nameView + "roll.", Data.Camera.Root[SOi0].ViewPoint.Roll, A3DC);
                    KeyWriter(nameView, ref Data.Camera.Root[SOi0].ViewPoint.MT, 0b1001111, A3DC);
                    KeyWriter(name, ref Data.Camera.Root[SOi0].MT, 0b0001, A3DC);
                }
                System.Write("camera_root.length=", Data.Camera.Root.Length.ToString());
            }

            if (Data.Chara.Length != 0)
            {
                SO0 = SortWriter(Data.Chara.Length);
                name = "chara.";

                for (int i0 = 0; i0 < Data.Chara.Length; i0++)
                    KeyWriter(name + SO0[i0] + Dot, ref Data.Chara[SO0[i0]], 0b1001111, A3DC);
                System.Write(name + "length=", Data.Chara.Length.ToString());
            }

            if (Data.Curve.Length != 0)
            {
                SO0 = SortWriter(Data.Curve.Length);
                SOi0 = 0;

                for (int i0 = 0; i0 < Data.Curve.Length; i0++)
                {
                    SOi0 = SO0[i0];
                    name = "curve." + SOi0 + Dot;

                    KeyWriter(name + "cv.", Data.Curve[SOi0].CV, A3DC);
                    System.Write(name + "name=", Data.Curve[SOi0].Name);
                }
                System.Write("curve.length=", Data.Curve.Length.ToString());
            }

            if (Data.DOF.Name != null)
                KeyWriter("dof.", ref Data.DOF, 0b1111111, A3DC);

            if (Data.Event.Length != 0)
            {
                SO0 = SortWriter(Data.Event.Length);
                SOi0 = 0;
                for (int i0 = 0; i0 < Data.Event.Length; i0++)
                {
                    SOi0 = SO0[i0];
                    name = "event." + SOi0 + Dot;

                    System.Write(name + "begin=", System.ToString(Data.Event[SOi0].Begin));
                    System.Write(name + "clip_begin=", System.ToString(Data.Event[SOi0].ClipBegin));
                    System.Write(name + "clip_en=", System.ToString(Data.Event[SOi0].ClipEnd));
                    System.Write(name + "end=", System.ToString(Data.Event[SOi0].End));
                    System.Write(name + "name=", Data.Event[SOi0].Name);
                    System.Write(name + "param1=", Data.Event[SOi0].Param1);
                    System.Write(name + "ref=", Data.Event[SOi0].Ref);
                    System.Write(name + "time_ref_scale=", System.ToString(Data.Event[SOi0].TimeRefScale));
                    System.Write(name + "type=", Data.Event[SOi0].Type.ToString());
                }
            }

            if (Data.Fog.Length != 0)
            {
                SO0 = SortWriter(Data.Fog.Length);
                SOi0 = 0;
                for (int i0 = 0; i0 < Data.Fog.Length; i0++)
                {
                    SOi0 = SO0[i0];
                    name = "fog." + SOi0 + Dot;

                    KeyWriter(name, Data.Fog[SOi0].Diffuse, A3DC, "Diffuse");
                    KeyWriter(name, Data.Fog[SOi0].Density, A3DC, "density");
                    KeyWriter(name, Data.Fog[SOi0].End, A3DC, "end");
                    System.Write(name + "id=", Data.Fog[SOi0].Id.ToString());
                    KeyWriter(name, Data.Fog[SOi0].Start, A3DC, "start");
                }
                System.Write("fog.length=", Data.Fog.Length.ToString());
            }

            if (Data.Light.Length != 0)
            {
                SO0 = SortWriter(Data.Light.Length);
                SOi0 = 0;
                for (int i0 = 0; i0 < Data.Light.Length; i0++)
                {
                    SOi0 = SO0[i0];
                    name = "light." + SOi0 + Dot;

                    KeyWriter(name, Data.Light[SOi0].Ambient, A3DC, "Ambient");
                    KeyWriter(name, Data.Light[SOi0].Diffuse, A3DC, "Diffuse");
                    KeyWriter(name, Data.Light[SOi0].Incandescence, A3DC, "Incandescence");
                    KeyWriter(name, Data.Light[SOi0].Specular, A3DC, "Specular");
                    System.Write(name + "id=", Data.Light[SOi0].Id.ToString());
                    System.Write(name + "name=", Data.Light[SOi0].Name);
                    KeyWriter(name + "position.", ref Data.Light[SOi0].Position, 0b1001111, A3DC);
                    KeyWriter(name + "spot_direction.", ref Data.Light[SOi0].SpotDirection, 0b1001111, A3DC);
                    System.Write(name + "type=", Data.Light[SOi0].Type);
                }
                System.Write("light.length=", Data.Light.Length.ToString());
            }

            if (Data.MObjectHRC.Length != 0)
            {
                SO0 = SortWriter(Data.MObjectHRC.Length);
                SOi0 = 0;
                for (int i0 = 0; i0 < Data.MObjectHRC.Length; i0++)
                {
                    SOi0 = SO0[i0];
                    name = "m_objhrc." + SOi0 + Dot;

                    if (Data.MObjectHRC[SOi0].Instance != null)
                    {
                        name = "m_objhrc." + SOi0 + ".instance.";
                        int[] SO1 = SortWriter(Data.MObjectHRC[i0].Instance.Length);
                        int SOi1 = 0;
                        for (int i1 = 0; i1 < Data.MObjectHRC[i0].Instance.Length; i1++)
                        {
                            SOi1 = SO1[i1];
                            KeyWriter(name + SOi1 + Dot, ref Data.MObjectHRC[SOi0].Instance[SOi1], 0b1111111, A3DC);
                        }
                        System.Write(name + "length=",
                            Data.MObjectHRC[SOi0].Instance.Length.ToString());
                    }
                    System.Write(name + "name=", Data.MObjectHRC[SOi0].Name);
                    if (Data.MObjectHRC[SOi0].Node != null)
                    {
                        name = "m_objhrc." + SOi0 + ".node.";
                        int[] SO1 = SortWriter(Data.MObjectHRC[i0].Node.Length);
                        int SOi1 = 0;
                        for (int i1 = 0; i1 < Data.MObjectHRC[i0].Node.Length; i1++)
                        {
                            SOi1 = SO1[i1];
                            System.Write(name + SOi1 + ".name=",
                                Data.MObjectHRC[SOi0].Node[SOi1].Name);
                            System.Write(name + SOi1 + ".parent=",
                                Data.MObjectHRC[SOi0].Node[SOi1].Parent.ToString());
                            KeyWriter(name + SOi1 + Dot,
                                ref Data.MObjectHRC[SOi0].Node[SOi1].MT, 0b1001111, A3DC);

                        }
                        System.Write(name + "length=", Data.MObjectHRC[SOi0].Node.Length.ToString());
                    }
                }
                System.Write("m_objhrc.length=", Data.MObjectHRC.Length.ToString());
            }

            if (Data.MObjectHRCList.Length != 0)
            {
                SO0 = SortWriter(Data.MObjectHRCList.Length);
                name = "m_objhrc_list.";
                for (int i0 = 0; i0 < Data.MObjectHRCList.Length; i0++)
                    System.Write(name + SO0[i0] + "=", Data.MObjectHRCList[SO0[i0]]);
                System.Write(name + "length=", Data.MObjectHRCList.Length.ToString());
            }

            if (Data.Motion.Length != 0)
            {
                SO0 = SortWriter(Data.Motion.Length);
                name = "motion.";
                for (int i0 = 0; i0 < Data.Motion.Length; i0++)
                    System.Write(name + SO0[i0] + ".name=", Data.Motion[SO0[i0]]);
                System.Write(name + "length=", Data.Motion.Length.ToString());
            }

            if (Data.Object.Length != 0)
            {
                SO0 = SortWriter(Data.Object.Length);
                SOi0 = 0;
                for (int i0 = 0; i0 < Data.Object.Length; i0++)
                {
                    SOi0 = SO0[i0];
                    name = "object." + SOi0 + Dot;

                    KeyWriter(name, ref Data.Object[SOi0].MT, 0b1000000, A3DC);
                    if (Data.Object[SOi0].Morph != null)
                    {
                        System.Write(name + "morph=", Data.Object[SOi0].Morph);
                        System.Write(name + "morph_offset=", Data.Object[SOi0].MorphOffset.ToString());
                    }
                    KeyWriter(name, ref Data.Object[SOi0].MT, 0b100000, A3DC);
                    System.Write(name + "parent_name=", Data.Object[SOi0].ParentName);
                    KeyWriter(name, ref Data.Object[SOi0].MT, 0b1100, A3DC);

                    if (Data.Object[SOi0].TexPat != null)
                    {
                        nameView = name + "tex_pat.";

                        SO1 = SortWriter(Data.Object[SOi0].TexPat.Length);
                        SOi1 = 0;
                        for (int i1 = 0; i1 < Data.Object[SOi0].TexPat.Length; i1++)
                        {
                            SOi1 = SO1[i1];
                            System.Write(nameView + SOi1 + ".name=", Data.Object[SOi0].TexPat[SOi1].Name);
                            System.Write(nameView + SOi1 + ".pat=", Data.Object[SOi0].TexPat[SOi1].Pat);
                            System.Write(nameView + SOi1 + ".pat_offset=", Data.Object[SOi0].TexPat[SOi1].PatOffset.ToString());
                        }
                        System.Write(nameView + "length=" + Data.Object[SOi0].TexPat.Length + "\n");
                    }

                    if (Data.Object[SOi0].TexTrans != null)
                    {
                        SO1 = SortWriter(Data.Object[SOi0].TexTrans.Length);
                        SOi1 = 0;
                        nameView = name + "tex_transform.";
                        for (int i1 = 0; i1 < Data.Object[SOi0].TexTrans.Length; i1++)
                        {
                            SOi1 = SO1[i1];
                            System.Write(nameView + SOi1 + ".name=", Data.Object[SOi0].TexTrans[SOi1].Name);
                            KeyWriter(nameView + SOi1 + Dot, Data.Object[SOi0].
                                TexTrans[SOi1].CoverageU, A3DC, "coverageU");
                            KeyWriter(nameView + SOi1 + Dot, Data.Object[SOi0].
                                TexTrans[SOi1].CoverageV, A3DC, "coverageV");
                            KeyWriter(nameView + SOi1 + Dot, Data.Object[SOi0].
                                TexTrans[SOi1].OffsetU, A3DC, "offsetU");
                            KeyWriter(nameView + SOi1 + Dot, Data.Object[SOi0].
                                TexTrans[SOi1].OffsetV, A3DC, "offsetV");
                            KeyWriter(nameView + SOi1 + Dot, Data.Object[SOi0].
                                TexTrans[SOi1].RepeatU, A3DC, "repeatU");
                            KeyWriter(nameView + SOi1 + Dot, Data.Object[SOi0].
                                TexTrans[SOi1].RepeatV, A3DC, "repeatV");
                            KeyWriter(nameView + SOi1 + Dot, Data.Object[SOi0].
                                TexTrans[SOi1].Rotate, A3DC, "rotate");
                            KeyWriter(nameView + SOi1 + Dot, Data.Object[SOi0].
                                TexTrans[SOi1].RotateFrame, A3DC, "rotateFrame");
                            KeyWriter(nameView + SOi1 + Dot, Data.Object[SOi0].
                                TexTrans[SOi1].TranslateFrameU, A3DC, "translateFrameU");
                            KeyWriter(nameView + SOi1 + Dot, Data.Object[SOi0].
                                TexTrans[SOi1].TranslateFrameV, A3DC, "translateFrameV");
                        }
                        System.Write(nameView + "length=" + Data.Object[SOi0].TexTrans.Length + "\n");
                    }

                    KeyWriter(name, ref Data.Object[SOi0].MT, 0b10011, A3DC);
                }
                System.Write("object.length=", Data.Object.Length.ToString());
            }

            if (Data.ObjectList.Length != 0)
            {
                SO0 = SortWriter(Data.ObjectList.Length);
                for (int i0 = 0; i0 < Data.ObjectList.Length; i0++)
                    System.Write("object_list." + SO0[i0] + "=", Data.ObjectList[SO0[i0]].ToString());
                System.Write("object_list.length=", Data.ObjectList.Length.ToString());
            }

            if (Data.ObjectHRC.Length != 0)
            {
                SO0 = SortWriter(Data.ObjectHRC.Length);
                SOi0 = 0;
                for (int i0 = 0; i0 < Data.ObjectHRC.Length; i0++)
                {
                    SOi0 = SO0[i0];
                    System.Write("objhrc." + SOi0 + ".name=", Data.ObjectHRC[SOi0].Name);
                    if (Data.ObjectHRC[SOi0].Node != null)
                    {
                        SO1 = SortWriter(Data.ObjectHRC[i0].Node.Length);
                        SOi1 = 0;
                        name = "objhrc." + SOi0 + ".node.";
                        for (int i1 = 0; i1 < Data.ObjectHRC[i0].Node.Length; i1++)
                        {
                            SOi1 = SO1[i1];
                            KeyWriter(name + SOi1 + Dot, ref Data.ObjectHRC[SOi0].Node[SOi1].MT, 0b1000000, A3DC);
                            System.Write(name + SOi1 + ".name=", Data.ObjectHRC[SOi0].Node[SOi1].Name);
                            System.Write(name + SOi1 + ".parent=", Data.ObjectHRC[SOi0].Node[SOi1].Parent.ToString());
                            KeyWriter(name + SOi1 + Dot, ref Data.ObjectHRC[SOi0].Node[SOi1].MT, 0b1111, A3DC);
                        }
                        System.Write(name + "length=", Data.ObjectHRC[SOi0].Node.Length.ToString());
                    }
                    name = "objhrc." + SOi0 + Dot;

                    if (Data.ObjectHRC[SOi0].Shadow != 0)
                        System.Write(name + "shadow=", Data.ObjectHRC[SOi0].Shadow.ToString());
                    System.Write(name + "uid_name=", Data.ObjectHRC[SOi0].UIDName);
                }
                System.Write("objhrc.length=", Data.ObjectHRC.Length.ToString());
            }

            if (Data.ObjectHRCList.Length != 0)
            {
                SO0 = SortWriter(Data.ObjectHRCList.Length);
                for (int i0 = 0; i0 < Data.ObjectHRCList.Length; i0++)
                    System.Write("objhrc_list." + SO0[i0] + "=", Data.ObjectHRCList[SO0[i0]]);
                System.Write("objhrc_list.length=", Data.ObjectHRCList.Length.ToString());
            }

            System.Write("play_control.begin=", Data.PlayControl.Begin.ToString());
            System.Write("play_control.fps=", Data.PlayControl.FPS.ToString());
            System.Write("play_control.size=", (Data.PlayControl.Size - Data.PlayControl.Offset).ToString());

            if (Data.PostProcess.Boolean)
            {
                name = "post_process.";
                KeyWriter(name, Data.PostProcess.Ambient, A3DC, "Ambient");
                KeyWriter(name, Data.PostProcess.Diffuse, A3DC, "Diffuse");
                KeyWriter(name, Data.PostProcess.Specular, A3DC, "Specular");
                KeyWriter(name, Data.PostProcess.LensFlare, A3DC, "lens_flare");
                KeyWriter(name, Data.PostProcess.LensGhost, A3DC, "lens_ghost");
                KeyWriter(name, Data.PostProcess.LensShaft, A3DC, "lens_shaft");
            }

            SO0 = SortWriter(Data.Point.Length);
            if (Data.Point.Length != 0)
            {
                for (int i0 = 0; i0 < Data.Point.Length; i0++)
                    KeyWriter("point." + SO0[i0] + Dot, ref Data.Point[SO0[i0]], 0b1111, A3DC);
                System.Write("point.length=", Data.Point.Length.ToString());
            }

            if (!A3DC)
                System.writer.Close();
        }

        static void A3DCWriter(string file)
        {
            if (A3DCOpt)
                UsedValues = new Values { BinOffset = new List<int>(), Value = new List<double>() };
            DateTime date = DateTime.Now;

            System.writer = new FileStream(file + ".a3da", FileMode.Create, FileAccess.ReadWrite, FileShare.ReadWrite);
            System.writer.SetLength(0);

            if (Data._.CompressF16 != 0)
            {
                int a = int.Parse(Data._.ConverterVersion);
                int b = BitConverter.ToInt32(Encoding.UTF8.GetBytes(Data._.FileName), 0);
                int c = int.Parse(Data._.PropertyVersion);
                int d = (int)((long)a * b * c);
                Random r = new Random(d);
                int e = r.Next();
                int f = r.Next();
                int g = r.Next();
                int h = r.Next();

                System.Write(Encoding.ASCII.GetBytes("A3DA"));
                System.Write(0x00);
                System.Write(0x40);
                System.Write(0x10000000);
                System.Write((long)0x00);
                System.Write((long)0x00);
                System.Write(e);
                System.Write(0x00);
                System.Write((long)0x00);
                System.Write(0x01131010);
                System.Write(f);
                System.Write(g);
                System.Write(h);
            }

            int A3DCStart = Offset = (int)System.writer.Position;
            System.writer.Seek(0x40, SeekOrigin.Current);

            int BinaryStart = Offset = (int)System.writer.Position;

            int BinaryEnd = (int)System.writer.Position;

            for (byte i = 0; i < 2; i++)
            {
                if (i == 1)
                {
                    if (Data.Camera.Auxiliary.Boolean)
                    {
                        KeyWriter(ref Data.Camera.Auxiliary.AutoExposure);
                        KeyWriter(ref Data.Camera.Auxiliary.Exposure);
                        KeyWriter(ref Data.Camera.Auxiliary.Gamma);
                        KeyWriter(ref Data.Camera.Auxiliary.GammaRate);
                        KeyWriter(ref Data.Camera.Auxiliary.Saturate);
                    }

                    for (int i0 = 0; i0 < Data.Camera.Root.Length; i0++)
                    {
                        KeyWriter(ref Data.Camera.Root[i0].Interest.MT);
                        KeyWriter(ref Data.Camera.Root[i0].MT);
                        KeyWriter(ref Data.Camera.Root[i0].ViewPoint.MT);
                        KeyWriter(ref Data.Camera.Root[i0].ViewPoint.FOV);
                        KeyWriter(ref Data.Camera.Root[i0].ViewPoint.Roll);
                        KeyWriter(ref Data.Camera.Root[i0].ViewPoint.FocalLength);
                    }

                    for (int i0 = 0; i0 < Data.Chara.Length; i0++)
                        KeyWriter(ref Data.Chara[i0]);

                    for (int i0 = 0; i0 < Data.Curve.Length; i0++)
                        KeyWriter(ref Data.Curve[i0].CV);

                    if (Data.DOF.Name != null)
                        KeyWriter(ref Data.DOF);

                    for (int i0 = 0; i0 < Data.Fog.Length; i0++)
                    {
                        KeyWriter(ref Data.Fog[i0].Diffuse);
                        KeyWriter(ref Data.Fog[i0].Density);
                        KeyWriter(ref Data.Fog[i0].End);
                        KeyWriter(ref Data.Fog[i0].Start);
                    }

                    for (int i0 = 0; i0 < Data.Light.Length; i0++)
                    {
                        KeyWriter(ref Data.Light[i0].Ambient);
                        KeyWriter(ref Data.Light[i0].Diffuse);
                        KeyWriter(ref Data.Light[i0].Incandescence);
                        KeyWriter(ref Data.Light[i0].Specular);
                        KeyWriter(ref Data.Light[i0].Position);
                        KeyWriter(ref Data.Light[i0].SpotDirection);
                    }

                    for (int i0 = 0; i0 < Data.MObjectHRC.Length; i0++)
                    {
                        if (Data.MObjectHRC[i0].Instance != null)
                            for (int i1 = 0; i1 < Data.MObjectHRC[i0].Instance.Length; i1++)
                                KeyWriter(ref Data.MObjectHRC[i0].Instance[i1]);
                        if (Data.MObjectHRC[i0].Node != null)
                            for (int i1 = 0; i1 < Data.MObjectHRC[i0].Node.Length; i1++)
                                KeyWriter(ref Data.MObjectHRC[i0].Node[i1].MT);
                    }

                    for (int i0 = 0; i0 < Data.Object.Length; i0++)
                    {
                        KeyWriter(ref Data.Object[i0].MT);
                        if (Data.Object[i0].TexTrans != null)
                            for (int i1 = 0; i1 < Data.Object[i0].TexTrans.Length; i1++)
                            {
                                KeyWriter(ref Data.Object[i0].TexTrans[i1].TranslateFrameU);
                                KeyWriter(ref Data.Object[i0].TexTrans[i1].TranslateFrameV);
                            }
                    }

                    for (int i0 = 0; i0 < Data.ObjectHRC.Length; i0++)
                        if (Data.ObjectHRC[i0].Node != null)
                            for (int i1 = 0; i1 < Data.ObjectHRC[i0].Node.Length; i1++)
                                KeyWriter(ref Data.ObjectHRC[i0].Node[i1].MT);

                    for (int i0 = 0; i0 < Data.Point.Length; i0++)
                        KeyWriter(ref Data.Point[i0]);

                    if (Data.PostProcess.Boolean)
                    {
                        KeyWriter(ref Data.PostProcess.Ambient);
                        KeyWriter(ref Data.PostProcess.Diffuse);
                        KeyWriter(ref Data.PostProcess.Specular);
                        KeyWriter(ref Data.PostProcess.LensFlare);
                        KeyWriter(ref Data.PostProcess.LensGhost);
                        KeyWriter(ref Data.PostProcess.LensShaft);
                    }

                    System.writer.Seek(System.Align(System.writer.Position, 0x10), 0);
                    BinaryEnd = (int)System.writer.Position;
                }

                System.writer.Seek(BinaryStart, 0);

                for (int i0 = 0; i0 < Data.Camera.Root.Length; i0++)
                {
                    KeyOffsetWriter(ref Data.Camera.Root[i0].Interest.MT, i == 1);
                    KeyOffsetWriter(ref Data.Camera.Root[i0].MT, i == 1);
                    KeyOffsetWriter(ref Data.Camera.Root[i0].ViewPoint.MT, i == 1);
                }

                if (Data.DOF.Name != null)
                    KeyOffsetWriter(ref Data.DOF, i == 1);

                for (int i0 = 0; i0 < Data.Light.Length; i0++)
                {
                    KeyOffsetWriter(ref Data.Light[i0].Position, i == 1);
                    KeyOffsetWriter(ref Data.Light[i0].SpotDirection, i == 1);
                }

                for (int i0 = 0; i0 < Data.MObjectHRC.Length; i0++)
                    if (Data.MObjectHRC[i0].Node != null)
                        for (int i1 = 0; i1 < Data.MObjectHRC[i0].Node.Length; i1++)
                            KeyOffsetWriter(ref Data.MObjectHRC[i0].Node[i1].MT, i == 1);

                for (int i0 = 0; i0 < Data.Object.Length; i0++)
                    KeyOffsetWriter(ref Data.Object[i0].MT, i == 1);

                for (int i0 = 0; i0 < Data.ObjectHRC.Length; i0++)
                    if (Data.ObjectHRC[i0].Node != null)
                        for (int i1 = 0; i1 < Data.ObjectHRC[i0].Node.Length; i1++)
                            KeyOffsetWriter(ref Data.ObjectHRC[i0].Node[i1].MT, i == 1);
            }

            System.writer.Seek(BinaryEnd, 0);

            A3DAWriter(file, true);

            int A3DCEnd = (int)System.writer.Position;

            if (Data._.CompressF16 != 0)
            {
                System.writer.Seek(System.Align(System.writer.Position, 0x10), 0);
                A3DCEnd = (int)System.writer.Position;
                System.EOFCWriter();
                System.writer.Seek(0x04, 0);
                System.Write(A3DCEnd - A3DCStart);
                System.writer.Seek(0x14, 0);
                System.Write(A3DCEnd - A3DCStart);
            }

            System.writer.Seek(A3DCStart, 0);
            System.Write("#A3D", "C__________");
            System.Write(0x2000);
            System.Write(0x00);
            System.Write(0x20, true, true);
            System.Write(0x10000200);
            System.Write(0x50);
            System.Write(BinaryEnd, true, true);
            System.Write(A3DCEnd - BinaryEnd, true, true);
            System.Write(0x01, true, true);
            System.Write(0x4C42);
            System.Write(BinaryStart, true, true);
            System.Write(BinaryEnd - BinaryStart, true, true);
            System.Write(0x20, true, true);
            
            System.writer.Close();
        }

        static void A3DAReader()
        {
            for (int i = 0; i < Data.Name.Count; i++)
            {
                if (Data.Name[i] == "_.compress_f16")
                    Data._.CompressF16 = int.Parse(Data.Value[i]);
                else if (Data.Name[i] == "_.converter.version")
                    Data._.ConverterVersion = Data.Value[i];
                else if (Data.Name[i] == "_.file_name")
                    Data._.FileName = Data.Value[i];
                else if (Data.Name[i] == "_.property.version")
                    Data._.PropertyVersion = Data.Value[i];
                else if (Data.Name[i] == "camera_root.length")
                    Data.Camera.Root = new CameraRoot[int.Parse(Data.Value[i])];
                else if (Data.Name[i] == "chara.length")
                    Data.Chara = new ModelTransform[int.Parse(Data.Value[i])];
                else if (Data.Name[i] == "curve.length")
                    Data.Curve = new Curve[int.Parse(Data.Value[i])];
                else if (Data.Name[i] == "dof.name")
                    Data.DOF.Name = Data.Value[i];
                else if (Data.Name[i] == "event.length")
                    Data.Event = new Event[int.Parse(Data.Value[i])];
                else if (Data.Name[i] == "fog.length")
                    Data.Fog = new Fog[int.Parse(Data.Value[i])];
                else if (Data.Name[i] == "light.length")
                    Data.Light = new Light[int.Parse(Data.Value[i])];
                else if (Data.Name[i] == "m_objhrc.length")
                    Data.MObjectHRC = new MObjectHRC[int.Parse(Data.Value[i])];
                else if (Data.Name[i] == "m_objhrc_list.length")
                    Data.MObjectHRCList = new string[int.Parse(Data.Value[i])];
                else if (Data.Name[i] == "motion.length")
                    Data.Motion = new string[int.Parse(Data.Value[i])];
                else if (Data.Name[i] == "object.length")
                    Data.Object = new Object[int.Parse(Data.Value[i])];
                else if (Data.Name[i] == "objhrc.length")
                    Data.ObjectHRC = new ObjectHRC[int.Parse(Data.Value[i])];
                else if (Data.Name[i] == "object_list.length")
                    Data.ObjectList = new string[int.Parse(Data.Value[i])];
                else if (Data.Name[i] == "objhrc_list.length")
                    Data.ObjectHRCList = new string[int.Parse(Data.Value[i])];
                else if (Data.Name[i] == "play_control.begin")
                    Data.PlayControl.Begin = int.Parse(Data.Value[i]);
                else if (Data.Name[i] == "play_control.fps")
                    Data.PlayControl.FPS = int.Parse(Data.Value[i]);
                else if (Data.Name[i] == "play_control.offset")
                    Data.PlayControl.Offset = int.Parse(Data.Value[i]);
                else if (Data.Name[i] == "play_control.size")
                    Data.PlayControl.Size = int.Parse(Data.Value[i]);
                else if (Data.Name[i] == "point.length")
                    Data.Point = new ModelTransform[int.Parse(Data.Value[i])];
                else if (Data.Name[i].StartsWith("camera_auxiliary") && !Data.Camera.Auxiliary.Boolean)
                    Data.Camera.Auxiliary.Boolean = true;
                else if (Data.Name[i].StartsWith("post_process") && !Data.PostProcess.Boolean)
                    Data.PostProcess.Boolean = true;
            }

            if (Data.Camera.Auxiliary.Boolean)
            {
                name = "camera_auxiliary.";
                KeyReader(name + "auto_exposure.", ref Data.Camera.Auxiliary.AutoExposure, 0x3002, true);
                KeyReader(name + "exposure.", ref Data.Camera.Auxiliary.Exposure, 0x3002, true);
                KeyReader(name + "gamma.", ref Data.Camera.Auxiliary.Gamma, 0x3002, true);
                KeyReader(name + "gamma_rate.", ref Data.Camera.Auxiliary.GammaRate, 0x3002, true);
                KeyReader(name + "saturate.", ref Data.Camera.Auxiliary.Saturate, 0x3002, true);
                KeyReader(name + "auto_exposure.", ref Data.Camera.Auxiliary.AutoExposure, 0x3002, true);
            }

            for (int i0 = 0; i0 < Data.Camera.Root.Length; i0++)
            {
                name = "camera_root." + i0 + Dot;
                nameInt = name + "interest.";
                nameView = name + "view_point.";

                Data.Camera.Root[i0] = new CameraRoot
                { Interest = new Interest { MT = new ModelTransform { BinOffset = -1 } },
                    MT = new ModelTransform { BinOffset = -1 }, Region = GetListRegion("camera_root." + i0),
                    ViewPoint = new ViewPoint { Aspect = -1, CameraApertureH = -1, CameraApertureW = -1,
                        FocalLength = new Key { BinOffset = -1 }, FOV = new Key { BinOffset = -1 },
                        FOVHorizontal = -1, MT = new ModelTransform { BinOffset = -1 },
                        Roll = new Key { BinOffset = -1 } } };

                for (int Region = 0; Region < Data.Camera.Root[i0].Region.Count; Region++)
                    for (int i = Data.Camera.Root[i0].Region[Region].Start;
                        i < Data.Camera.Root[i0].Region[Region].End; i++)
                    {
                        CheckString(name + MTBO, i, ref Data.Camera.Root[i0].MT.BinOffset);
                        CheckString(nameInt + MTBO, i, ref Data.Camera.Root[i0].Interest.MT.BinOffset);
                        CheckString(nameView + MTBO, i, ref Data.Camera.Root[i0].ViewPoint.MT.BinOffset);
                        CheckString(nameView + "aspect", i, ref Data.Camera.Root[i0].ViewPoint.Aspect);
                        CheckString(nameView + "camera_aperture_h", i,
                            ref Data.Camera.Root[i0].ViewPoint.CameraApertureH);
                        CheckString(nameView + "camera_aperture_w", i,
                            ref Data.Camera.Root[i0].ViewPoint.CameraApertureW);
                        CheckString(nameView + "focal_length." + BO, i,
                            ref Data.Camera.Root[i0].ViewPoint.FocalLength.BinOffset);
                        CheckString(nameView + "fov." + BO, i,
                            ref Data.Camera.Root[i0].ViewPoint.FOV.BinOffset);
                        CheckString(nameView + "fov_is_horizontal", i,
                            ref Data.Camera.Root[i0].ViewPoint.FOVHorizontal);
                        CheckString(nameView + "roll." + BO, i,
                            ref Data.Camera.Root[i0].ViewPoint.Roll.BinOffset);
                    }

                KeyReader(name, ref Data.Camera.Root[i0].MT, 0x3002);
                KeyReader(nameInt, ref Data.Camera.Root[i0].Interest.MT, 0x3002);
                KeyReader(nameView, ref Data.Camera.Root[i0].ViewPoint.MT, 0x3002);
                KeyReader(nameView + "focal_length.", ref Data.Camera.Root[i0].ViewPoint.FocalLength, 0x3002);
                KeyReader(nameView + "fov.", ref Data.Camera.Root[i0].ViewPoint.FOV, 0x3002);
                KeyReader(nameView + "roll.", ref Data.Camera.Root[i0].ViewPoint.Roll, 0x3002);
            }

            for (int i0 = 0; i0 < Data.Chara.Length; i0++)
            {
                Data.Chara[i0] = new ModelTransform { BinOffset = -1 };
                Data.Chara[i0].Region = GetListRegion("chara." + i0);
                name = "chara." + i0 + Dot;

                for (int Region = 0; Region < Data.Chara[i0].Region.Count; Region++)
                    for (int i = Data.Chara[i0].Region[Region].Start;
                        i < Data.Chara[i0].Region[Region].End; i++)
                        CheckString(name + MTBO, i, ref Data.Chara[i0].BinOffset);
                KeyReader(name, ref Data.Chara[i0], 0x3002);
            }

            for (int i0 = 0; i0 < Data.Curve.Length; i0++)
            {
                Data.Curve[i0].CV = new Key { BinOffset = -1 };
                Data.Curve[i0].Region = GetListRegion("curve." + i0);
                name = "curve." + i0 + Dot;

                for (int Region = 0; Region < Data.Curve[i0].Region.Count; Region++)
                    for (int i = Data.Curve[i0].Region[Region].Start; i < Data.Curve[i0].Region[Region].End; i++)
                    {
                        CheckString(name + "cv." + BO, i, ref Data.Curve[i0].CV.BinOffset);
                        CheckString(name + "name", i, ref Data.Curve[i0].Name);
                    }

                KeyReader(name + "cv.", ref Data.Curve[i0].CV, 0x3002);
            }

            if (Data.DOF.Name != null)
            {
                Data.DOF = new ModelTransform { BinOffset = -1 };
                Data.DOF.Region = GetListRegion("dof");
                name = "dof.";

                for (int Region = 0; Region < Data.DOF.Region.Count; Region++)
                    for (int i = Data.DOF.Region[Region].Start;
                        i < Data.DOF.Region[Region].End; i++)
                        CheckString(name + MTBO, i, ref Data.DOF.BinOffset);

                KeyReader(name, ref Data.DOF, 0x3002);
            }

            for (int i0 = 0; i0 < Data.Fog.Length; i0++)
            {
                Data.Fog[i0].Density = new Key { BinOffset = -1 };
                Data.Fog[i0].End = new Key { BinOffset = -1 };
                Data.Fog[i0].Region = GetListRegion("fog." + i0);
                Data.Fog[i0].Start = new Key { BinOffset = -1 };
                name = "fog." + i0 + Dot;

                for (int Region = 0; Region < Data.Fog[i0].Region.Count; Region++)
                    for (int i = Data.Fog[i0].Region[Region].Start;
                        i < Data.Fog[i0].Region[Region].End; i++)
                    {
                        CheckString(name + "id", i, ref Data.Fog[i0].Id);
                        CheckString(name + "density." + BO, i, ref Data.Fog[i0].Density.BinOffset);
                        CheckString(name + "end." + BO, i, ref Data.Fog[i0].End.BinOffset);
                        CheckString(name + "start." + BO, i, ref Data.Fog[i0].Start.BinOffset);
                    }

                KeyReader(name + "Diffuse.", ref Data.Fog[i0].Diffuse, 0x3002);
                KeyReader(name + "density.", ref Data.Fog[i0].Density, 0x3002);
                KeyReader(name + "end.", ref Data.Fog[i0].End, 0x3002);
                KeyReader(name + "start.", ref Data.Fog[i0].Start, 0x3002);
            }

            for (int i0 = 0; i0 < Data.Light.Length; i0++)
            {
                Data.Light[i0].Position = new ModelTransform { BinOffset = -1 };
                Data.Light[i0].Region = GetListRegion("light." + i0);
                Data.Light[i0].SpotDirection = new ModelTransform { BinOffset = -1 };
                name = "light." + i0 + Dot;

                for (int Region = 0; Region < Data.Light[i0].Region.Count; Region++)
                    for (int i = Data.Light[i0].Region[Region].Start;
                        i < Data.Light[i0].Region[Region].End; i++)
                    {
                        CheckString(name + "id", i, ref Data.Light[i0].Id);
                        CheckString(name + "name", i, ref Data.Light[i0].Name);
                        CheckString(name + "position." + MTBO, i, ref Data.Light[i0].Position.BinOffset);
                        CheckString(name + "spot_direction." + MTBO, i, ref Data.Light[i0].SpotDirection.BinOffset);
                        CheckString(name + "type", i, ref Data.Light[i0].Type);
                    }

                KeyReader(name + "Ambient.", ref Data.Light[i0].Ambient, 0x3002);
                KeyReader(name + "Diffuse.", ref Data.Light[i0].Diffuse, 0x3002);
                KeyReader(name + "Incandescence.", ref Data.Light[i0].Incandescence, 0x3002);
                KeyReader(name + "Specular.", ref Data.Light[i0].Specular, 0x3002);
                KeyReader(name + "position.", ref Data.Light[i0].Position, 0x3002);
                KeyReader(name + "spot_direction.", ref Data.Light[i0].SpotDirection, 0x3002);
            }

            for (int i0 = 0; i0 < Data.Event.Length; i0++)
            {
                Data.Event[i0].Region = GetListRegion("event." + i0);
                name = "event." + i0 + Dot;

                for (int Region = 0; Region < Data.Event[i0].Region.Count; Region++)
                    for (int i = Data.Curve[i0].Region[Region].Start; i < Data.Event[i0].Region[Region].End; i++)
                    {
                        CheckString(name + "begin", i, ref Data.Event[i0].Begin);
                        CheckString(name + "clip_begin", i, ref Data.Event[i0].ClipBegin);
                        CheckString(name + "clip_en", i, ref Data.Event[i0].ClipEnd);
                        CheckString(name + "end", i, ref Data.Event[i0].End);
                        CheckString(name + "name", i, ref Data.Event[i0].Name);
                        CheckString(name + "param1", i, ref Data.Event[i0].Param1);
                        CheckString(name + "ref", i, ref Data.Event[i0].Ref);
                        CheckString(name + "time_ref_scale", i, ref Data.Event[i0].TimeRefScale);
                        CheckString(name + "type", i, ref Data.Event[i0].Type);
                    }
            }

            for (int i0 = 0; i0 < Data.MObjectHRC.Length; i0++)
            {
                Data.MObjectHRC[i0].Region = GetListRegion("m_objhrc." + i0);
                name = "m_objhrc." + i0 + Dot;

                for (int Region = 0; Region < Data.MObjectHRC[i0].Region.Count; Region++)
                    for (int i = Data.MObjectHRC[i0].Region[Region].Start;
                        i < Data.MObjectHRC[i0].Region[Region].End; i++)
                        if (Data.Name[i] == name + "instance.length")
                            Data.MObjectHRC[i0].Node = new Node[int.Parse(Data.Value[i])];
                        else if (Data.Name[i] == name + "name")
                            Data.MObjectHRC[i0].Name = Data.Value[i];
                        else if (Data.Name[i] == name + "node.length")
                            Data.MObjectHRC[i0].Node = new Node[int.Parse(Data.Value[i])];

                if (Data.MObjectHRC[i0].Instance != null)
                    for (int i1 = 0; i1 < Data.MObjectHRC[i0].Instance.Length; i1++)
                    {
                        name = "m_objhrc." + i0 + ".instance." + i1 + Dot;
                        Data.MObjectHRC[i0].Instance[i1] = new ModelTransform { BinOffset = -1 };
                        for (int Region = 0; Region < Data.MObjectHRC[i0].Region.Count; Region++)
                            for (int i = Data.MObjectHRC[i0].Region[Region].Start;
                                i < Data.MObjectHRC[i0].Region[Region].End; i++)
                                if (Data.Name[i] == name + MTBO)
                                    Data.MObjectHRC[i0].Instance[i1].BinOffset = int.Parse(Data.Value[i]);
                                else if (Data.Name[i] == name + "name")
                                    Data.MObjectHRC[i0].Instance[i1].Name = Data.Value[i];
                                else if (Data.Name[i] == name + "uid_name")
                                    Data.MObjectHRC[i0].Instance[i1].UIDName = Data.Value[i];

                        KeyReader(name, ref Data.MObjectHRC[i0].Instance[i1], 0x3002);
                    }

                if (Data.MObjectHRC[i0].Node != null)
                    for (int i1 = 0; i1 < Data.MObjectHRC[i0].Node.Length; i1++)
                    {
                        name = "m_objhrc." + i0 + ".node." + i1 + Dot;
                        Data.MObjectHRC[i0].Node[i1].MT = new ModelTransform { BinOffset = -1 };
                        for (int Region = 0; Region < Data.MObjectHRC[i0].Region.Count; Region++)
                            for (int i = Data.MObjectHRC[i0].Region[Region].Start;
                                i < Data.MObjectHRC[i0].Region[Region].End; i++)
                            {
                                CheckString(name + MTBO, i, ref Data.MObjectHRC[i0].Node[i1].MT.BinOffset);
                                CheckString(name + "name", i, ref Data.MObjectHRC[i0].Node[i1].Name);
                                CheckString(name + "parent", i, ref Data.MObjectHRC[i0].Node[i1].Parent);
                            }

                        KeyReader(name, ref Data.MObjectHRC[i0].Node[i1].MT, 0x3002);
                    }
            }

            for (int i0 = 0; i0 < Data.MObjectHRCList.Length; i0++)
            {
                name = "m_objhrc_list." + i0;
                for (int i = 0; i < Data.Name.Count; i++)
                    if (CheckString(name, i, ref Data.MObjectHRCList[i0]))
                        break;
            }

            for (int i0 = 0; i0 < Data.Motion.Length; i0++)
            {
                name = "motion." + i0 + ".name";
                for (int i = 0; i < Data.Name.Count; i++)
                    if (CheckString(name, i, ref Data.Motion[i0]))
                        break;
            }

            for (int i0 = 0; i0 < Data.Object.Length; i0++)
            {
                Data.Object[i0].Morph = "";
                Data.Object[i0].MorphOffset = -1;
                Data.Object[i0].MT = new ModelTransform { BinOffset = -1 };
                Data.Object[i0].Region = GetListRegion("object." + i0);
                name = "object." + i0 + Dot;

                for (int Region = 0; Region < Data.Object[i0].Region.Count; Region++)
                {
                    for (int i = Data.Object[i0].Region[Region].Start; i < Data.Object[i0].Region[Region].End; i++)
                    {
                        CheckString(name + MTBO, i, ref Data.Object[i0].MT.BinOffset);
                        CheckString(name + "morph", i, ref Data.Object[i0].Morph);
                        CheckString(name + "morph_offset", i, ref Data.Object[i0].MorphOffset);
                        CheckString(name + "name", i, ref Data.Object[i0].MT.Name);
                        CheckString(name + "parent_name", i, ref Data.Object[i0].ParentName);
                        CheckString(name + "uid_name", i, ref Data.Object[i0].MT.UIDName);

                        if (Data.Name[i] == name + "tex_pat.length")
                            Data.Object[i0].TexPat = new TexturePattern[int.Parse(Data.Value[i])];
                        else if (Data.Name[i] == name + "tex_transform.length")
                            Data.Object[i0].TexTrans = new TextureTransform[int.Parse(Data.Value[i])];
                    }

                    if (Data.Object[i0].TexPat != null)
                        for (int i1 = 0; i1 < Data.Object[i0].TexPat.Length; i1++)
                        {
                            name = "object." + i0 + ".tex_pat." + i1 + Dot;
                            for (int i = Data.Object[i0].Region[Region].Start; i < Data.Object[i0].Region[Region].End; i++)
                            {
                                CheckString(name + "name", i, ref Data.Object[i0].TexPat[i1].Name);
                                CheckString(name + "pat", i, ref Data.Object[i0].TexPat[i1].Pat);
                                CheckString(name + "pat_offset", i, ref Data.Object[i0].TexPat[i1].PatOffset);
                            }
                        }

                    if (Data.Object[i0].TexTrans != null)
                        for (int i1 = 0; i1 < Data.Object[i0].TexTrans.Length; i1++)
                        {
                            name = "object." + i0 + ".tex_transform." + i1 + Dot;
                            for (int i = Data.Object[i0].Region[Region].Start; i < Data.Object[i0].Region[Region].End; i++)
                                CheckString(name + "name", i, ref Data.Object[i0].TexTrans[i1].Name);
                        }
                }

                name = "object." + i0 + Dot;
                KeyReader(name, ref Data.Object[i0].MT, 0x3002);

                if (Data.Object[i0].TexTrans != null)
                    for (int i1 = 0; i1 < Data.Object[i0].TexTrans.Length; i1++)
                    {
                        name = "object." + i0 + ".tex_transform." + i1 + Dot;
                        KeyReader(name + "coverageU.", ref Data.Object[i0].TexTrans[i1].CoverageU, 0x3002, true);
                        KeyReader(name + "coverageV.", ref Data.Object[i0].TexTrans[i1].CoverageV, 0x3002, true);
                        KeyReader(name + "offsetU.", ref Data.Object[i0].TexTrans[i1].OffsetU, 0x3002, true);
                        KeyReader(name + "offsetV.", ref Data.Object[i0].TexTrans[i1].OffsetV, 0x3002, true);
                        KeyReader(name + "repeatU.", ref Data.Object[i0].TexTrans[i1].RepeatU, 0x3002, true);
                        KeyReader(name + "repeatV.", ref Data.Object[i0].TexTrans[i1].RepeatV, 0x3002, true);
                        KeyReader(name + "rotate.", ref Data.Object[i0].TexTrans[i1].Rotate, 0x3002, true);
                        KeyReader(name + "rotateFrame.", ref Data.Object[i0].TexTrans[i1].RotateFrame, 0x3002, true);
                        KeyReader(name + "translateFrameU.", ref Data.Object[i0].TexTrans[i1].TranslateFrameU, 0x3002, true);
                        KeyReader(name + "translateFrameV.", ref Data.Object[i0].TexTrans[i1].TranslateFrameV, 0x3002, true);
                    }
            }


            for (int i0 = 0; i0 < Data.ObjectHRC.Length; i0++)
            {
                Data.ObjectHRC[i0].Name = "";
                Data.ObjectHRC[i0].UIDName = "";
                Data.ObjectHRC[i0].Region = GetListRegion("objhrc." + i0);
                name = "objhrc." + i0 + Dot;

                for (int Region = 0; Region < Data.ObjectHRC[i0].Region.Count; Region++)
                    for (int i = Data.ObjectHRC[i0].Region[Region].Start; i < Data.ObjectHRC[i0].Region[Region].End; i++)
                    {
                        CheckString(name + "name", i, ref Data.ObjectHRC[i0].Name);
                        CheckString(name + "shadow", i, ref Data.ObjectHRC[i0].Shadow);
                        CheckString(name + "uid_name", i, ref Data.ObjectHRC[i0].UIDName);
                        if (Data.Name[i] == name + "node.length")
                            Data.ObjectHRC[i0].Node = new Node[int.Parse(Data.Value[i])];
                    }

                if (Data.ObjectHRC[i0].Node != null)
                    for (int i1 = 0; i1 < Data.ObjectHRC[i0].Node.Length; i1++)
                    {
                        Data.ObjectHRC[i0].Node[i1].MT = new ModelTransform { BinOffset = -1 };
                        name = "objhrc." + i0 + ".node." + i1 + Dot;

                        for (int Region = 0; Region < Data.ObjectHRC[i0].Region.Count; Region++)
                            for (int i = Data.ObjectHRC[i0].Region[Region].Start; i < Data.ObjectHRC[i0].Region[Region].End; i++)
                            {
                                CheckString(name + MTBO, i, ref Data.ObjectHRC[i0].Node[i1].MT.BinOffset);
                                CheckString(name + "name", i, ref Data.ObjectHRC[i0].Node[i1].Name);
                                CheckString(name + "parent", i, ref Data.ObjectHRC[i0].Node[i1].Parent);
                            }

                        KeyReader(name, ref Data.ObjectHRC[i0].Node[i1].MT, 0x3002);
                    }
            }


            for (int i0 = 0; i0 < Data.ObjectHRCList.Length; i0++)
            {
                Data.ObjectHRCList[i0] = "";
                name = "objhrc_list." + i0;
                ;
                for (int i = 0; i < Data.Name.Count; i++)
                    if (CheckString("objhrc_list." + i0, i, ref Data.ObjectHRCList[i0]))
                        break;
            }


            for (int i0 = 0; i0 < Data.ObjectList.Length; i0++)
            {
                Data.ObjectList[i0] = "";
                name = "object_list." + i0;

                for (int i = 0; i < Data.Name.Count; i++)
                    if (CheckString(name, i, ref Data.ObjectList[i0]))
                        break;
            }

            for (int i0 = 0; i0 < Data.Point.Length; i0++)
            {
                Data.Point[i0] = new ModelTransform { BinOffset = -1 };
                Data.Point[i0].Region = GetListRegion("point." + i0);
                name = "point." + i0 + Dot;

                for (int Region = 0; Region < Data.Point[i0].Region.Count; Region++)
                    for (int i = Data.Point[i0].Region[Region].Start; i < Data.Point[i0].Region[Region].End; i++)
                        CheckString(name + MTBO, i, ref Data.Point[i0].BinOffset);

                KeyReader(name, ref Data.Point[i0], 0x3002);
            }

            if (Data.PostProcess.Boolean)
            {
                name = "post_process.";

                Data.PostProcess.Boolean = true;
                KeyReader(name + "Ambient.", ref Data.PostProcess.Ambient, 0x3002);
                KeyReader(name + "Diffuse.", ref Data.PostProcess.Diffuse, 0x3002);
                KeyReader(name + "Specular.", ref Data.PostProcess.Specular, 0x3002);
                KeyReader(name + "lens_flare.", ref Data.PostProcess.LensFlare, 0x3002, true);
                KeyReader(name + "lens_ghost.", ref Data.PostProcess.LensGhost, 0x3002, true);
                KeyReader(name + "lens_shaft.", ref Data.PostProcess.LensShaft, 0x3002, true);
            }
        }

        static void A3DCReader()
        {
            if (Data.Camera.Auxiliary.Boolean)
            {
                KeyReader(ref Data.Camera.Auxiliary.AutoExposure, 0x3002);
                KeyReader(ref Data.Camera.Auxiliary.Exposure, 0x3002);
                KeyReader(ref Data.Camera.Auxiliary.Gamma, 0x3002);
                KeyReader(ref Data.Camera.Auxiliary.GammaRate, 0x3002);
                KeyReader(ref Data.Camera.Auxiliary.Saturate, 0x3002);
            }

            for (int i0 = 0; i0 < Data.Camera.Root.Length; i0++)
            {
                KeyReader(ref Data.Camera.Root[i0].MT, 0x3002);
                KeyReader(ref Data.Camera.Root[i0].Interest.MT, 0x3002);
                KeyReader(ref Data.Camera.Root[i0].ViewPoint.MT, 0x3002);
                KeyReader(ref Data.Camera.Root[i0].ViewPoint.FocalLength, 0x3002);
                KeyReader(ref Data.Camera.Root[i0].ViewPoint.FOV, 0x3002);
                KeyReader(ref Data.Camera.Root[i0].ViewPoint.Roll, 0x3002);
            }
            
            for (int i0 = 0; i0 < Data.Chara.Length; i0++)
                KeyReader(ref Data.Chara[i0], 0x3002);

            for (int i0 = 0; i0 < Data.Curve.Length; i0++)
                KeyReader(ref Data.Curve[i0].CV, 0x3002);

            if (Data.DOF.Name != null)
                KeyReader(ref Data.DOF, 0x3002);

            for (int i0 = 0; i0 < Data.Fog.Length; i0++)
            {
                KeyReader(ref Data.Fog[i0].Density, 0x3002);
                KeyReader(ref Data.Fog[i0].Diffuse, 0x3002);
                KeyReader(ref Data.Fog[i0].End, 0x3002);
                KeyReader(ref Data.Fog[i0].Start, 0x3002);
            }

            for (int i0 = 0; i0 < Data.Light.Length; i0++)
            {
                KeyReader(ref Data.Light[i0].Ambient, 0x3002);
                KeyReader(ref Data.Light[i0].Diffuse, 0x3002);
                KeyReader(ref Data.Light[i0].Incandescence, 0x3002);
                KeyReader(ref Data.Light[i0].Specular, 0x3002);
                KeyReader(ref Data.Light[i0].Position, 0x3002);
                KeyReader(ref Data.Light[i0].SpotDirection, 0x3002);
            }

            for (int i0 = 0; i0 < Data.MObjectHRC.Length; i0++)
                if (Data.MObjectHRC[i0].Node != null)
                    for (int i1 = 0; i1 < Data.MObjectHRC[i0].Node.Length; i1++)
                        KeyReader(ref Data.MObjectHRC[i0].Node[i1].MT, 0x3002);

            for (int i0 = 0; i0 < Data.Object.Length; i0++)
            {
                KeyReader(ref Data.Object[i0].MT, 0x3002);
                if (Data.Object[i0].TexTrans != null)
                    for (int i1 = 0; i1 < Data.Object[i0].TexTrans.Length; i1++)
                    {
                        KeyReader(ref Data.Object[i0].TexTrans[i1].CoverageU, 0x3002);
                        KeyReader(ref Data.Object[i0].TexTrans[i1].CoverageV, 0x3002);
                        KeyReader(ref Data.Object[i0].TexTrans[i1].OffsetU, 0x3002);
                        KeyReader(ref Data.Object[i0].TexTrans[i1].OffsetV, 0x3002);
                        KeyReader(ref Data.Object[i0].TexTrans[i1].RepeatU, 0x3002);
                        KeyReader(ref Data.Object[i0].TexTrans[i1].RepeatV, 0x3002);
                        KeyReader(ref Data.Object[i0].TexTrans[i1].Rotate, 0x3002);
                        KeyReader(ref Data.Object[i0].TexTrans[i1].RotateFrame, 0x3002);
                        KeyReader(ref Data.Object[i0].TexTrans[i1].TranslateFrameU, 0x3002);
                        KeyReader(ref Data.Object[i0].TexTrans[i1].TranslateFrameV, 0x3002);
                    }
            }

            for (int i0 = 0; i0 < Data.ObjectHRC.Length; i0++)
                if (Data.ObjectHRC[i0].Node != null)
                    for (int i1 = 0; i1 < Data.ObjectHRC[i0].Node.Length; i1++)
                        KeyReader(ref Data.ObjectHRC[i0].Node[i1].MT, 0x3002);


            for (int i0 = 0; i0 < Data.Point.Length; i0++)
                KeyReader(ref Data.Point[i0], 0x3002);

            if (Data.PostProcess.Boolean)
            {
                KeyReader(ref Data.PostProcess.Ambient, 0x3002);
                KeyReader(ref Data.PostProcess.Diffuse, 0x3002);
                KeyReader(ref Data.PostProcess.Specular, 0x3002);
                KeyReader(ref Data.PostProcess.LensFlare, 0x3002);
                KeyReader(ref Data.PostProcess.LensGhost, 0x3002);
                KeyReader(ref Data.PostProcess.LensShaft, 0x3002);
            }
        }

        static void KeyReader(string Template, ref ModelTransform MT, int Type)
        {
            MT.Region = GetListRegion(Template);
            for (int region = 0; region < MT.Region.Count; region++)
                for (int i = MT.Region[region].Start; i < MT.Region[region].End; i++)
                    if (Data.Name[i] == Template + "name")
                        MT.Name = Data.Value[i];
                    else if (Data.Name[i] == Template + "rot.x.type")
                        KeyReader(Template + "rot.", ref MT.Rot, Type);
                    else if (Data.Name[i] == Template + "scale.x.type")
                        KeyReader(Template + "scale.", ref MT.Scale, Type);
                    else if (Data.Name[i] == Template + "trans.x.type")
                        KeyReader(Template + "trans.", ref MT.Trans, Type);
                    else if (Data.Name[i] == Template + "uid_name")
                        MT.UIDName = Data.Value[i];
                    else if (Data.Name[i] == Template + "visibility.type")
                        KeyReader(Template + "visibility.", ref MT.Visibility, Type);
        }

        static void KeyReader(string Template, ref RGBAKey k, int Type)
        {
            k.Region = GetListRegion(Template.Remove(Template.Length - 1));
            for (int region = 0; region < k.Region.Count; region++)
                for (int i = k.Region[region].Start; i < k.Region[region].End; i++)
                    CheckString(Template.Remove(Template.Length - 1), i, ref k.Boolean);
            
            KeyReader(Template + "a.", ref k.A, Type, true);
            KeyReader(Template + "b.", ref k.B, Type, true);
            KeyReader(Template + "g.", ref k.G, Type, true);
            KeyReader(Template + "r.", ref k.R, Type, true);
            k.Alpha = k.A.Boolean;
        }

        static void KeyReader(string Template, ref Vector3Key k, int Type)
        {
            KeyReader(Template + "x.", ref k.X, Type);
            KeyReader(Template + "y.", ref k.Y, Type);
            KeyReader(Template + "z.", ref k.Z, Type);
        }

        static void KeyReader(string Template, ref Key k, int Type, bool New)
        {
            k = new Key { BinOffset = -1, Region = GetListRegion(Template.Remove(Template.Length - 1)) };
            for (int Region = 0; Region < k.Region.Count; Region++)
                for (int i = k.Region[Region].Start; i < k.Region[Region].End; i++)
                {
                    CheckString(Template.Remove(Template.Length - 1), i, ref k.Boolean);
                    if (CheckString(Template + BO, i, ref k.BinOffset))
                        k.Boolean = true;
                    else if (Data.Name[i] == Template + "type")
                        KeyReader(Template, ref k, Type, int.Parse(Data.Value[i]));
                }
        }

        static void KeyReader(string Template, ref Key k, int Type)
        {
            k.Region = GetListRegion(Template);
            for (int i = 0; i < Data.Name.Count; i++)
                if (Data.Name[i] == Template + "type")
                    KeyReader(Template, ref k, Type, int.Parse(Data.Value[i]));
        }

        static void KeyReader(string Template, ref Key k, int Type, int type)
        {
            k.Boolean = true;
            k.EPTypePost = -1;
            k.EPTypePre = -1;
            k.Length = -1;
            k.Max = -1;
            k.Type = type;
            k.Value = -1;
            k.RawData = new RawData { KeyType = -1 };
            k.Trans = null;
            if (Type == 0x3002 && k.Type == 0x0002)
                k.Type = 0x3002;
            else if (Type == 0x0003 && k.Type == 0x0002)
                k.Type = 0x0003;
            else if ((int)Type == 4)
                k.Type = 0x0003;

            if (k.Region == null)
                k.Region = GetListRegion(Template);

            if (k.Type == 0x0000 || k.Type == 0x0001)
            {
                if (k.Type > 0)
                    for (int Region = 0; Region < k.Region.Count; Region++)
                        for (int i = k.Region[Region].Start; i < k.Region[Region].End; i++)
                            if (Data.Name[i] == Template + "value")
                            {
                                k.Value = System.ToDouble(Data.Value[i]);
                                break;
                            }
            }
            else
            {
                for (int Region = 0; Region < k.Region.Count; Region++)
                    for (int i = k.Region[Region].Start; i < k.Region[Region].End; i++)
                        if (Data.Name[i] == Template + "ep_type_post")
                            k.EPTypePost = int.Parse(Data.Value[i]);
                        else if (Data.Name[i] == Template + "ep_type_pre")
                            k.EPTypePre = int.Parse(Data.Value[i]);
                        else if (Data.Name[i] == Template + "key.length")
                            k.Length = int.Parse(Data.Value[i]);
                        else if (Data.Name[i] == Template + "max")
                            k.Max = int.Parse(Data.Value[i]);
                        else if (Data.Name[i] == Template + "raw_data_key_type")
                            k.RawData.KeyType = int.Parse(Data.Value[i]);

                if (k.Length != -1)
                {
                    k.Trans = new Transform[k.Length];
                    for (int Region = 0; Region < k.Region.Count; Region++)
                        for (int i1 = k.Region[Region].Start; i1 < k.Region[Region].End; i1++)
                            for (int i0 = 0; i0 < k.Length; i0++)
                                if (Data.Name[i1] == Template + "key." + i0 + ".data")
                                {
                                    dataArray = Data.Value[i1].Replace("(", "").Replace(")", "").Split(',');
                                    k.Trans[i0].Frame = System.ToDouble(dataArray[0]);
                                    if (dataArray.Length > 1)
                                        k.Trans[i0].Value1 = System.ToDouble(dataArray[1]);
                                    if (dataArray.Length > 2)
                                        k.Trans[i0].Value2 = System.ToDouble(dataArray[2]);
                                    if (dataArray.Length > 3)
                                        k.Trans[i0].Value3 = System.ToDouble(dataArray[3]);
                                    k.Trans[i0].Type = dataArray.Length - 1;
                                    break;
                                }
                }
                else if (k.RawData.KeyType != -1)
                {
                    for (int Region = 0; Region < k.Region.Count; Region++)
                        for (int i = k.Region[Region].Start; i < k.Region[Region].End; i++)
                            if (Data.Name[i] == Template + "raw_data.value_type")
                                k.RawData.ValueType = Data.Value[i];
                            else if (Data.Name[i] == Template + "raw_data.value_list")
                                k.RawData.ValueList = Data.Value[i].Split(',');
                            else if (Data.Name[i] == Template + "raw_data.value_list_size")
                                k.RawData.ValueListSize = int.Parse(Data.Value[i]);

                    int DataSize = 0;
                    if (k.RawData.KeyType > 2)
                        DataSize = 4;
                    else if (k.RawData.KeyType > 1)
                        DataSize = 3;
                    else if (k.RawData.KeyType > 0)
                        DataSize = 2;
                    else
                        DataSize = 1;

                    k.Length = k.RawData.ValueListSize / DataSize;
                    k.Trans = new Transform[k.Length];
                    for (int i = 0; i < k.Length; i++)
                    {
                        k.Trans[i].Type = k.RawData.KeyType;
                        k.Trans[i].Frame = System.ToDouble(k.RawData.ValueList[i * DataSize + 0]);
                        if (k.Trans[i].Type > 0)
                            k.Trans[i].Value1 = System.ToDouble(k.RawData.ValueList[i * DataSize + 1]);
                        if (k.Trans[i].Type > 1)
                            k.Trans[i].Value2 = System.ToDouble(k.RawData.ValueList[i * DataSize + 2]);
                        if (k.Trans[i].Type > 2)
                            k.Trans[i].Value3 = System.ToDouble(k.RawData.ValueList[i * DataSize + 3]);
                    }
                }

                double kFrameMax = 0;
                for (int i = 0; i < k.Length; i++)
                {
                    if (kFrameMax < k.Trans[i].Frame)
                        kFrameMax = k.Trans[i].Frame;
                }

                if (kFrameMax > k.Max && kFrameMax < Data.PlayControl.Size)
                    k.Max = Data.PlayControl.Size;
                else if (kFrameMax > k.Max && kFrameMax > Data.PlayControl.Size)
                    k.Max = Data.PlayControl.Size = (int)Math.Ceiling(kFrameMax + 1);
            }
        }
        
        static void KeyWriter(string Template, ref ModelTransform MT, byte Flags, bool A3DC)
        {
            if (A3DC && !MT.Writed && ((Flags >> 6) & 0b1) == 1)
            {
                System.Write(Template + MTBO + "=", MT.BinOffset.ToString().ToLower());
                MT.Writed = true;
            }
            if (((Flags >> 5) & 0b1) == 1)
                System.Write(Template + "name=", MT.Name);
            if (!A3DC)
            {
                if (((Flags >> 3) & 0b1) == 1)
                    KeyWriter(Template + "rot.", MT.Rot);
                if (((Flags >> 2) & 0b1) == 1)
                    KeyWriter(Template + "scale.", MT.Scale);
                if (((Flags >> 1) & 0b1) == 1)
                    KeyWriter(Template + "trans.", MT.Trans);
            }
            if (((Flags >> 4) & 0b1) == 1)
                System.Write(Template + "uid_name=", MT.UIDName);
            if (!A3DC)
                if ((Flags & 0b1) == 1)
                    KeyWriter(Template + "visibility.", MT.Visibility);
        }

        static void KeyWriter(string Template, ModelTransform MT, byte Flags)
        {
            if (((Flags >> 5) & 0b1) == 1)
                System.Write(Template + "name=", MT.Name);
            if (((Flags >> 3) & 0b1) == 1)
                KeyWriter(Template + "rot.", MT.Rot);
            if (((Flags >> 2) & 0b1) == 1)
                KeyWriter(Template + "scale.", MT.Scale);
            if (((Flags >> 1) & 0b1) == 1)
                KeyWriter(Template + "trans.", MT.Trans);
            if (((Flags >> 4) & 0b1) == 1)
                System.Write(Template + "uid_name=", MT.UIDName);
            if ((Flags & 0b1) == 1)
                KeyWriter(Template + "visibility.", MT.Visibility);
        }

        static void KeyWriter(string Template, RGBAKey k, bool A3DC, string Data)
        {
            if (k.Boolean)
            {
                System.Write(Template + Data + "=", k.Boolean.ToString().ToLower());
                if (k.Alpha)
                    KeyWriter(Template + Data + ".a.", k.A, A3DC);
                KeyWriter(Template + Data + ".b.", k.B, A3DC);
                KeyWriter(Template + Data + ".g.", k.G, A3DC);
                KeyWriter(Template + Data + ".r.", k.R, A3DC);
            }
        }

        static void KeyWriter(string Template, Vector3Key k)
        {
            KeyWriter(Template + "x.", k.X);
            KeyWriter(Template + "y.", k.Y);
            KeyWriter(Template + "z.", k.Z);
        }

        static void KeyWriter(string Template, Key k, bool A3DC, string Data)
        {
            if (k.Boolean)
            {
                System.Write(Template + Data + "=", k.Boolean.ToString().ToLower());
                KeyWriter(Template + Data + Dot, k, A3DC);
            }
        }
        
        static void KeyWriter(string Template, Key k, bool A3DC)
        {
            if (k.Boolean)
            {
                if (A3DC)
                    System.Write(Template + BO + "=", k.BinOffset.ToString().ToLower());
                else
                    KeyWriter(Template, k);
            }
        }

        static void KeyWriter(string Template, Key k)
        {
            if (k.Boolean)
            {
                if (k.Trans != null)
                {
                    int[] SO = SortWriter(k.Trans.Length);
                    if (k.EPTypePost != -1)
                        System.Write(Template + "ep_type_post=", k.EPTypePost.ToString());
                    if (k.EPTypePre != -1)
                        System.Write(Template + "ep_type_pre=", k.EPTypePre.ToString());
                    if (k.Length != -1)
                    {
                        for (int i = 0; i < k.Trans.Length; i++)
                            KeyWriter(Template, k, SO[i]);
                        System.Write(Template + "key.length=", k.Length.ToString());
                    }
                    if (k.Max != -1)
                        System.Write(Template + "max=", (k.Max - Data.PlayControl.Offset).ToString());
                    if (k.Type == 0x3002)
                        System.Write(Template + "type=3\n");
                    else
                    {
                        if ((int)k.Type <= 3)
                            System.Write(Template + "type=", ((int)k.Type).ToString());
                        else
                            System.Write(Template + "type=", "2");
                    }
                }
                else
                {
                    if (k.Type == 0x0000)
                        System.Write(Template + "type=0\n");
                    else
                    {
                        System.Write(Template + "type=1\n");
                        System.Write(Template + "value=", System.ToString(k.Value, 15).ToString());
                    }
                }
            }
        }

        static void KeyWriter(string Template, Key k, int i)
        {
            Transform trans = k.Trans[i];

            string Temp = Template + "key." + i;
            string Frame = System.ToString(trans.Frame - Data.PlayControl.Offset);
            string V1 = System.ToString(trans.Value1, 9);
            string V2 = System.ToString(trans.Value2, 9);
            string V3 = System.ToString(trans.Value3, 9);
            if (trans.Type == 3)
                System.Write(Temp + ".data=(" + Frame + "," + V1 + "," + V2 + "," + V3 + ")\n");
            else if (trans.Type == 2)
                System.Write(Temp + ".data=(" + Frame + "," + V1 + "," + V2 + ")\n");
            else if (trans.Type == 1)
                System.Write(Temp + ".data=(" + Frame + "," + V1 + ")\n");
            else
                System.Write(Temp + ".data=" + Frame + "\n");
            System.Write(Temp + ".type=" + trans.Type + "\n");
        }
        
        static void KeyReader(ref ModelTransform MT, int Type)
        {
            if (MT.BinOffset > -1)
            {
                System.reader.Seek(Offset + MT.BinOffset, 0);
                KeyReader(out MT.Scale, Type);
                KeyReader(out MT.Rot, Type);
                KeyReader(out MT.Trans, Type);
                MT.Visibility = new Key { BinOffset = System.ReadInt32() };
                KeyReader(ref MT.Visibility, Type);
            }
        }

        static void KeyReader(ref RGBAKey k, int Type)
        {
            long CurrentOffset = System.reader.Position;
            KeyReader(ref k.R, Type);
            KeyReader(ref k.G, Type);
            KeyReader(ref k.B, Type);
            if (k.Alpha)
                KeyReader(ref k.A, Type);
            System.reader.Seek(CurrentOffset, 0);
        }

        static void KeyReader(out Vector3Key k, int Type)
        {
            k = new Vector3Key();
            k.X.BinOffset = System.ReadInt32();
            k.Y.BinOffset = System.ReadInt32();
            k.Z.BinOffset = System.ReadInt32();
            long CurrentOffset = System.reader.Position;
            KeyReader(ref k.X, Type);
            KeyReader(ref k.Y, Type);
            KeyReader(ref k.Z, Type);
            System.reader.Seek(CurrentOffset, 0);
        }

        static void KeyReader(ref Key k, int Type)
        {
            k.Boolean = true;
            k.EPTypePost = -1;
            k.EPTypePre = -1;
            k.Length = -1;
            k.Max = -1;
            k.Type = -1;
            k.Value = -1;
            k.RawData = new RawData { KeyType = -1 };
            k.Trans = null;
            if (k.BinOffset != -1)
            {
                System.reader.Seek(Offset + k.BinOffset, 0);
                k.Boolean = true;
                k.Type = System.ReadInt32();

                if (Type == 0x2003 && k.Type == 0x0002 && Data._.CompressF16 == 1)
                    k.Type = 0x2003;
                else if (Type == 0x2003 && k.Type == 0x0003 && Data._.CompressF16 == 1)
                    k.Type = 0x2003;

                k.Value = System.ReadSingle();
                if (k.Type != 0x0000 && k.Type != 0x0001)
                {
                    k.Max = System.ReadSingle();
                    k.Length = System.ReadInt32();
                    k.Trans = new Transform[k.Length];
                    for (int i = 0; i < k.Length; i++)
                    {
                        k.Trans[i].Type = 3;
                        if (k.Type == 0x2003)
                        {
                            k.Trans[i].Frame = System.ReadUInt16();
                            k.Trans[i].Value1 = System.ReadHalf();
                        }
                        else
                        {
                            k.Trans[i].Frame = System.ReadSingle();
                            k.Trans[i].Value1 = System.ReadSingle();
                        }
                        if (k.Type == 0x2003 && Data._.CompressF16 == 2)
                        {
                            k.Trans[i].Value2 = System.ReadHalf();
                            k.Trans[i].Value3 = System.ReadHalf();
                        }
                        else
                        {
                            k.Trans[i].Value2 = System.ReadSingle();
                            k.Trans[i].Value3 = System.ReadSingle();
                        }

                        if (k.Trans[i].Value3 == 0)
                        {
                            k.Trans[i].Type = 2;
                            if (k.Trans[i].Value2 == 0)
                            {
                                k.Trans[i].Type = 1;
                                if (k.Trans[i].Value3 == 0 && k.Trans[i].Value2 == 0 && k.Trans[i].Value1 == 0)
                                    k.Trans[i].Type = 0;
                            }
                        }
                    }

                    double kFrameMax = 0;
                    for (int i = 0; i < k.Length; i++)
                    {
                        if (kFrameMax < k.Trans[i].Frame)
                            kFrameMax = k.Trans[i].Frame;
                    }

                    if (kFrameMax > k.Max && kFrameMax < Data.PlayControl.Size)
                        k.Max = Data.PlayControl.Size;
                    else if (kFrameMax > k.Max && kFrameMax > Data.PlayControl.Size)
                        k.Max = Data.PlayControl.Size = (int)Math.Ceiling(kFrameMax + 1);
                }
            }
        }

        static void KeyWriter(ref ModelTransform MT)
        {
            KeyWriter(ref MT.Scale);
            KeyWriter(ref MT.Rot);
            KeyWriter(ref MT.Trans);
            KeyWriter(ref MT.Visibility);
        }

        static void KeyWriter(ref RGBAKey k)
        {
            if (k.Boolean)
            {
                KeyWriter(ref k.R);
                KeyWriter(ref k.G);
                KeyWriter(ref k.B);
                if (k.Alpha)
                    KeyWriter(ref k.A);
            }
        }

        static void KeyWriter(ref Vector3Key k)
        {
            KeyWriter(ref k.X);
            KeyWriter(ref k.Y);
            KeyWriter(ref k.Z);
        }

        static void KeyWriter(ref Key k)
        {
            if (k.Trans != null && k.Boolean)
            {
                k.BinOffset = (int)System.writer.Position - Offset;
                if (k.Type <= 3)
                    System.Write((int)k.Type);
                else
                    System.Write(0x0002);
                System.Write(0x00);
                System.Write(System.ToSingle(k.Max));
                System.Write(k.Trans.Length);
                for (int i = 0; i < k.Trans.Length; i++)
                {
                    System.Write(System.ToSingle(k.Trans[i].Frame));
                    System.Write(System.ToSingle(k.Trans[i].Value1));
                    System.Write(System.ToSingle(k.Trans[i].Value2));
                    System.Write(System.ToSingle(k.Trans[i].Value3));
                }
            }
            else if (k.Boolean)
            {
                if (UsedValues.Value.Contains(k.Value) && A3DCOpt)
                {
                    for (int i = 0; i < UsedValues.Value.Count; i++)
                        if (UsedValues.Value[i] == k.Value)
                            k.BinOffset = UsedValues.BinOffset[i];
                }
                else
                {
                    k.BinOffset = (int)System.writer.Position - Offset;
                    if (k.Type != 0x00 || k.Value != 0)
                    {
                        System.Write(0x01);
                        System.Write(System.ToSingle(k.Value));
                    }
                    else
                    {
                        System.Write(0x00);
                        System.Write(0x00);
                    }
                    if (A3DCOpt)
                    {
                        UsedValues.BinOffset.Add(k.BinOffset);
                        UsedValues.Value.Add(k.Value);
                    }
                }
            }
        }

        static void KeyOffsetWriter(ref ModelTransform MT, bool ReturnToOffset)
        {
            if (ReturnToOffset)
                System.writer.Seek(MT.BinOffset + Offset, 0);

            MT.BinOffset = (int)System.writer.Position - Offset;
            System.Write(MT.Scale.X.BinOffset);
            System.Write(MT.Scale.Y.BinOffset);
            System.Write(MT.Scale.Z.BinOffset);
            System.Write(MT.Rot.X.BinOffset);
            System.Write(MT.Rot.Y.BinOffset);
            System.Write(MT.Rot.Z.BinOffset);
            System.Write(MT.Trans.X.BinOffset);
            System.Write(MT.Trans.Y.BinOffset);
            System.Write(MT.Trans.Z.BinOffset);
            System.Write(MT.Visibility.BinOffset);
            System.writer.Seek(System.Align(System.writer.Position, 0x10), 0);
        }
        
        static void XMLReader(string file)
        {
            name = "";
            Offset = 0;
            nameInt = "";
            nameView = "";
            dataArray = new string[4];

            Data = new A3DA
            {
                _ = new _(),
                Camera = new Camera { Auxiliary = new CameraAuxiliary(), Root = new CameraRoot[0] },
                Chara = new ModelTransform[0],
                Curve = new Curve[0],
                Light = new Light[0],
                DOF = new ModelTransform(),
                Event = new Event[0],
                Fog = new Fog[0],
                MObjectHRC = new MObjectHRC[0],
                MObjectHRCList = new string[0],
                Motion = new string[0],
                Name = new List<string>(),
                Object = new Object[0],
                ObjectList = new string[0],
                ObjectHRC = new ObjectHRC[0],
                ObjectHRCList = new string[0],
                Point = new ModelTransform[0],
                PlayControl = new PlayControl { Begin = 0, FPS = 0, Offset = 0, Size = 0 },
                PostProcess = new PostProcess(),
                Value = new List<string>(),
                Head = new Header()
            };

            System.doc = new XmlDocument();
            System.doc.Load(file + ".xml");
            System.XMLCompact = true;

            XmlElement A3D = System.doc.DocumentElement;
            if (A3D.Name == "A3D")
            {
                foreach (XmlAttribute Entry in A3D.Attributes)
                    if (Entry.Name == "Signature")
                        Data.Header.Signature = BitConverter.ToInt32(Encoding.ASCII.GetBytes(Entry.Value), 0);

                foreach (XmlNode Child0 in A3D.ChildNodes)
                {
                    if (Child0.Name == "_")
                    {
                        foreach (XmlAttribute Entry in Child0.Attributes)
                        {
                            if (Data.Header.Signature == 0x5F5F5F43)
                                System.XMLReader(Entry, ref Data._.CompressF16, "CompressF16");
                            System.XMLReader(Entry, ref Data._.ConverterVersion, "ConverterVersion");
                            System.XMLReader(Entry, ref Data._.FileName, "FileName");
                            System.XMLReader(Entry, ref Data._.PropertyVersion, "PropertyVersion");
                        }
                    }
                    else if (Child0.Name == "Camera")
                    {
                        foreach (XmlNode Camera in Child0.ChildNodes)
                        {
                            if (Camera.Name == "Auxiliary")
                            {
                                Data.Camera.Auxiliary.Boolean = true;
                                foreach (XmlNode Auxiliary in Camera.ChildNodes)
                                {
                                    KeyReader(Auxiliary, ref Data.Camera.Auxiliary.AutoExposure, "AutoExposure");
                                    KeyReader(Auxiliary, ref Data.Camera.Auxiliary.Exposure, "Exposure");
                                    KeyReader(Auxiliary, ref Data.Camera.Auxiliary.Gamma, "Gamma");
                                    KeyReader(Auxiliary, ref Data.Camera.Auxiliary.GammaRate, "GammaRate");
                                    KeyReader(Auxiliary, ref Data.Camera.Auxiliary.Saturate, "Saturate");
                                }
                            }
                            else if (Camera.Name == "Root")
                            {
                                foreach (XmlAttribute Entry in Camera.Attributes)
                                    if (Entry.Name == "Length")
                                        Data.Camera.Root = new CameraRoot[int.Parse(Entry.Value)];

                                int i0 = 0;
                                foreach (XmlNode Root in Camera.ChildNodes)
                                    if (Root.Name == "RootEntry")
                                    {
                                        Data.Camera.Root[i0] = new CameraRoot
                                        {
                                            Interest = new Interest
                                            { MT = new ModelTransform() },
                                            MT = new ModelTransform(),
                                            ViewPoint = new ViewPoint
                                            {
                                                Aspect = -1,
                                                CameraApertureH = -1,
                                                CameraApertureW = -1,
                                                FocalLength = new Key { BinOffset = -1 },
                                                FOV = new Key { BinOffset = -1 },
                                                FOVHorizontal = -1,
                                                MT = new ModelTransform { BinOffset = -1 },
                                                Roll = new Key { BinOffset = -1 }
                                            }
                                        };

                                        KeyReader(Root, ref Data.Camera.Root[i0].MT);
                                        foreach (XmlNode CameraRoot in Root.ChildNodes)
                                        {
                                            if (CameraRoot.Name == "Interest")
                                                KeyReader(CameraRoot, ref Data.Camera.Root[i0].Interest.MT);
                                            else if (CameraRoot.Name == "ViewPoint")
                                            {
                                                foreach (XmlAttribute Entry in CameraRoot.Attributes)
                                                {
                                                    System.XMLReader(Entry, ref Data.Camera.Root[i0].
                                                        ViewPoint.Aspect, "Aspect");
                                                    System.XMLReader(Entry, ref Data.Camera.Root[i0].
                                                        ViewPoint.CameraApertureH, "CameraApertureH");
                                                    System.XMLReader(Entry, ref Data.Camera.Root[i0].
                                                        ViewPoint.CameraApertureW, "CameraApertureW");
                                                    System.XMLReader(Entry, ref Data.Camera.Root[i0].
                                                        ViewPoint.FOVHorizontal, "FOVHorizontal");
                                                }

                                                foreach (XmlNode ViewPoint in CameraRoot.ChildNodes)
                                                {
                                                    KeyReader(ViewPoint, ref Data.Camera.Root[i0].
                                                        ViewPoint.FocalLength, "FocalLength");
                                                    KeyReader(ViewPoint, ref Data.Camera.Root[i0].
                                                        ViewPoint.FOV, "FOV");
                                                    KeyReader(ViewPoint, ref Data.Camera.Root[i0].
                                                        ViewPoint.Roll, "Roll");
                                                }
                                                KeyReader(CameraRoot, ref Data.Camera.Root[i0].ViewPoint.MT);
                                            }
                                        }
                                        i0++;
                                    }
                            }
                        }
                    }
                    else if (Child0.Name == "Charas")
                    {
                        foreach (XmlAttribute Entry in Child0.Attributes)
                            if (Entry.Name == "Length")
                                Data.Chara = new ModelTransform[int.Parse(Entry.Value)];

                        int i0 = 0;
                        foreach (XmlNode Charas in Child0.ChildNodes)
                            KeyReader(Charas, ref Data.Chara[i0], "Chara", ref i0);
                    }
                    else if (Child0.Name == "Curves")
                    {
                        foreach (XmlAttribute Entry in Child0.Attributes)
                            if (Entry.Name == "Length")
                                Data.Curve = new Curve[int.Parse(Entry.Value)];

                        int i0 = 0;
                        foreach (XmlNode Curves in Child0.ChildNodes)
                        {
                            if (Curves.Name == "Curve")
                            {
                                foreach (XmlAttribute Entry in Curves.Attributes)
                                    System.XMLReader(Entry, ref Data.Curve[i0].Name, "Name");

                                foreach (XmlNode Curve in Curves.ChildNodes)
                                    KeyReader(Curve, ref Data.Curve[i0].CV, "CV");
                                i0++;
                            }
                        }
                    }
                    else if (Child0.Name == "DOF")
                        KeyReader(Child0, ref Data.DOF);
                    else if (Child0.Name == "Fogs")
                    {
                        foreach (XmlAttribute Entry in Child0.Attributes)
                            if (Entry.Name == "Length")
                                Data.Fog = new Fog[int.Parse(Entry.Value)];

                        int i0 = 0;
                        foreach (XmlNode Fogs in Child0.ChildNodes)
                        {
                            if (Fogs.Name == "Fog")
                            {
                                foreach (XmlAttribute Entry in Fogs.Attributes)
                                    System.XMLReader(Entry, ref Data.Fog[i0].Id, "Id");
                                foreach (XmlNode Fog in Fogs.ChildNodes)
                                {
                                    KeyReader(Fog, ref Data.Fog[i0].Diffuse, "Diffuse");
                                    KeyReader(Fog, ref Data.Fog[i0].Density, "Density");
                                    KeyReader(Fog, ref Data.Fog[i0].End, "End");
                                    KeyReader(Fog, ref Data.Fog[i0].Start, "Start");
                                }
                                i0++;
                            }
                        }
                    }
                    else if (Child0.Name == "Lights")
                    {
                        foreach (XmlAttribute Entry in Child0.Attributes)
                            if (Entry.Name == "Length")
                                Data.Light = new Light[int.Parse(Entry.Value)];

                        int i0 = 0;
                        foreach (XmlNode Lights in Child0.ChildNodes)
                        {
                            if (Lights.Name == "Light")
                            {
                                foreach (XmlAttribute Entry in Lights.Attributes)
                                {
                                    System.XMLReader(Entry, ref Data.Light[i0].Id, "Id");
                                    System.XMLReader(Entry, ref Data.Light[i0].Name, "Name");
                                    System.XMLReader(Entry, ref Data.Light[i0].Type, "Type");
                                }
                                foreach (XmlNode Light in Lights.ChildNodes)
                                {
                                    KeyReader(Light, ref Data.Light[i0].Ambient, "Ambient");
                                    KeyReader(Light, ref Data.Light[i0].Diffuse, "Diffuse");
                                    KeyReader(Light, ref Data.Light[i0].Incandescence, "Incandescence");
                                    KeyReader(Light, ref Data.Light[i0].Position, "Position");
                                    KeyReader(Light, ref Data.Light[i0].Specular, "Specular");
                                    KeyReader(Light, ref Data.Light[i0].SpotDirection, "SpotDirection");
                                }
                                i0++;
                            }
                        }
                    }
                    else if (Child0.Name == "MObjectsHRC")
                    {
                        foreach (XmlAttribute Entry in Child0.Attributes)
                            if (Entry.Name == "Length")
                                Data.MObjectHRC = new MObjectHRC[int.Parse(Entry.Value)];

                        int i0 = 0;
                        foreach (XmlNode MObjectsHRC in Child0.ChildNodes)
                        {
                            if (MObjectsHRC.Name == "MObjectHRC")
                            {
                                foreach (XmlAttribute Entry in MObjectsHRC.Attributes)
                                    System.XMLReader(Entry, ref Data.MObjectHRC[i0].Name, "Name");
                                foreach (XmlNode MObjectHRC in MObjectsHRC.ChildNodes)
                                {
                                    if (MObjectHRC.Name == "Instances")
                                    {
                                        foreach (XmlAttribute Entry in MObjectHRC.Attributes)
                                            if (Entry.Name == "Length")
                                                Data.MObjectHRC[i0].Instance = new ModelTransform[int.Parse(Entry.Value)];

                                        int i1 = 0;
                                        foreach (XmlNode Instances in MObjectHRC.ChildNodes)
                                            KeyReader(Instances, ref Data.MObjectHRC[i0].Instance[i1], "Instance", ref i1);
                                    }
                                    else if (MObjectHRC.Name == "Nodes")
                                    {
                                        foreach (XmlAttribute Entry in MObjectHRC.Attributes)
                                            if (Entry.Name == "Length")
                                                Data.MObjectHRC[i0].Node = new Node[int.Parse(Entry.Value)];

                                        int i1 = 0;
                                        foreach (XmlNode Nodes in MObjectHRC.ChildNodes)
                                        {
                                            if (Nodes.Name == "Node")
                                            {
                                                foreach (XmlAttribute Entry in Nodes.Attributes)
                                                {
                                                    System.XMLReader(Entry, ref Data.MObjectHRC[i0].Node[i1].Name, "Name");
                                                    System.XMLReader(Entry, ref Data.MObjectHRC[i0].Node[i1].Parent, "Parent");
                                                }

                                                KeyReader(Nodes, ref Data.MObjectHRC[i0].Node[i1].MT);
                                                i1++;
                                            }
                                        }
                                    }
                                }
                                i0++;
                            }
                        }
                    }
                    else if (Child0.Name == "MObjectHRCList")
                    {
                        foreach (XmlAttribute Entry in Child0.Attributes)
                            if (Entry.Name == "Length")
                                Data.MObjectHRCList = new string[int.Parse(Entry.Value)];

                        int i0 = 0;
                        foreach (XmlNode MObjectList in Child0.ChildNodes)
                        {
                            if (MObjectList.Name == "String")
                            {
                                foreach (XmlAttribute Entry in MObjectList.Attributes)
                                    System.XMLReader(Entry, ref Data.MObjectHRCList[i0], "Name");
                                i0++;
                            }
                        }
                    }
                    else if (Child0.Name == "Motion")
                    {
                        foreach (XmlAttribute Entry in Child0.Attributes)
                            if (Entry.Name == "Length")
                                Data.Motion = new string[int.Parse(Entry.Value)];

                        int i0 = 0;
                        foreach (XmlNode Motion in Child0.ChildNodes)
                        {
                            if (Motion.Name == "String")
                            {
                                foreach (XmlAttribute Entry in Motion.Attributes)
                                    System.XMLReader(Entry, ref Data.Motion[i0], "Name");
                                i0++;
                            }
                        }
                    }
                    else if (Child0.Name == "Objects")
                    {
                        foreach (XmlAttribute Entry in Child0.Attributes)
                            if (Entry.Name == "Length")
                                Data.Object = new Object[int.Parse(Entry.Value)];

                        int i0 = 0;
                        foreach (XmlNode Objects in Child0.ChildNodes)
                        {
                            Data.Object[i0] = new Object { MT = new ModelTransform() };
                            if (Objects.Name == "Object")
                            {
                                foreach (XmlAttribute Entry in Objects.Attributes)
                                {
                                    System.XMLReader(Entry, ref Data.Object[i0].Morph, "Morph");
                                    System.XMLReader(Entry, ref Data.Object[i0].MorphOffset, "MorphOffset");
                                    System.XMLReader(Entry, ref Data.Object[i0].MT.Name, "Name");
                                    System.XMLReader(Entry, ref Data.Object[i0].ParentName, "ParentName");
                                    System.XMLReader(Entry, ref Data.Object[i0].MT.UIDName, "UIDName");
                                }

                                foreach (XmlNode Object in Objects.ChildNodes)
                                {
                                    if (Object.Name == "TexTransforms")
                                    {
                                        foreach (XmlAttribute Entry in Object.Attributes)
                                            if (Entry.Name == "Length")
                                                Data.Object[i0].TexTrans = new TextureTransform[int.Parse(Entry.Value)];

                                        int i1 = 0;
                                        foreach (XmlNode TexTransforms in Object.ChildNodes)
                                            if (TexTransforms.Name == "TexTransform")
                                            {
                                                TextureTransform TexTrans = new TextureTransform();
                                                foreach (XmlAttribute Entry in TexTransforms.Attributes)
                                                    System.XMLReader(Entry, ref TexTrans.Name, "Name");

                                                foreach (XmlNode TexTransform in TexTransforms.ChildNodes)
                                                {
                                                    KeyReader(TexTransform, ref TexTrans.CoverageU, "CoverageU");
                                                    KeyReader(TexTransform, ref TexTrans.CoverageV, "CoverageV");
                                                    KeyReader(TexTransform, ref TexTrans.OffsetU, "OffsetU");
                                                    KeyReader(TexTransform, ref TexTrans.OffsetV, "OffsetV");
                                                    KeyReader(TexTransform, ref TexTrans.RepeatU, "RepeatU");
                                                    KeyReader(TexTransform, ref TexTrans.RepeatV, "RepeatV");
                                                    KeyReader(TexTransform, ref TexTrans.Rotate, "Rotate");
                                                    KeyReader(TexTransform, ref TexTrans.RotateFrame, "RotateFrame");
                                                    KeyReader(TexTransform, ref TexTrans.TranslateFrameU, "TranslateFrameU");
                                                    KeyReader(TexTransform, ref TexTrans.TranslateFrameV, "TranslateFrameV");
                                                }
                                                Data.Object[i0].TexTrans[i1] = TexTrans;
                                                i1++;
                                            }
                                    }
                                    if (Object.Name == "TexPats")
                                    {
                                        foreach (XmlAttribute Entry in Object.Attributes)
                                            if (Entry.Name == "Length")
                                                Data.Object[i0].TexPat = new TexturePattern[int.Parse(Entry.Value)];

                                        int i1 = 0;
                                        foreach (XmlNode TexPats in Object.ChildNodes)
                                            if (TexPats.Name == "TexPat")
                                            {
                                                foreach (XmlAttribute Entry in TexPats.Attributes)
                                                {
                                                    System.XMLReader(Entry, ref Data.Object[i0].TexPat[i1].Name, "Name");
                                                    System.XMLReader(Entry, ref Data.Object[i0].TexPat[i1].Pat, "Pat");
                                                    System.XMLReader(Entry, ref Data.Object[i0].
                                                        TexPat[i1].PatOffset, "PatOffset");
                                                }
                                                i1++;
                                            }
                                    }
                                }
                                KeyReader(Objects, ref Data.Object[i0].MT);
                                i0++;
                            }
                        }
                    }
                    else if (Child0.Name == "ObjectsHRC")
                    {
                        foreach (XmlAttribute Entry in Child0.Attributes)
                            if (Entry.Name == "Length")
                                Data.ObjectHRC = new ObjectHRC[int.Parse(Entry.Value)];

                        int i0 = 0;
                        foreach (XmlNode ObjectsHRC in Child0.ChildNodes)
                        {
                            if (ObjectsHRC.Name == "ObjectHRC")
                            {
                                foreach (XmlAttribute Entry in ObjectsHRC.Attributes)
                                {
                                    System.XMLReader(Entry, ref Data.ObjectHRC[i0].Name, "Name");
                                    System.XMLReader(Entry, ref Data.ObjectHRC[i0].Shadow, "Shadow");
                                    System.XMLReader(Entry, ref Data.ObjectHRC[i0].UIDName, "UIDName");
                                }

                                foreach (XmlNode ObjectHRC in ObjectsHRC.ChildNodes)
                                {
                                    if (ObjectHRC.Name == "Nodes")
                                    {
                                        foreach (XmlAttribute Entry in ObjectHRC.Attributes)
                                            if (Entry.Name == "Length")
                                                Data.ObjectHRC[i0].Node = new Node[int.Parse(Entry.Value)];

                                        int i1 = 0;
                                        foreach (XmlNode Nodes in ObjectHRC.ChildNodes)
                                        {
                                            if (Nodes.Name == "Node")
                                            {
                                                foreach (XmlAttribute Entry in Nodes.Attributes)
                                                {
                                                    System.XMLReader(Entry, ref Data.ObjectHRC[i0].Node[i1].Name, "Name");
                                                    System.XMLReader(Entry, ref Data.ObjectHRC[i0].Node[i1].Parent, "Parent");
                                                }
                                                KeyReader(Nodes, ref Data.ObjectHRC[i0].Node[i1].MT);
                                                i1++;
                                            }
                                        }
                                    }
                                }
                                i0++;
                            }
                        }
                    }
                    else if (Child0.Name == "ObjectHRCList")
                    {
                        foreach (XmlAttribute Entry in Child0.Attributes)
                            if (Entry.Name == "Length")
                                Data.ObjectHRCList = new string[int.Parse(Entry.Value)];

                        int i0 = 0;
                        foreach (XmlNode ObjectList in Child0.ChildNodes)
                        {
                            if (ObjectList.Name == "String")
                            {
                                foreach (XmlAttribute Entry in ObjectList.Attributes)
                                    System.XMLReader(Entry, ref Data.ObjectHRCList[i0], "Name");
                                i0++;
                            }
                        }
                    }
                    else if (Child0.Name == "ObjectList")
                    {
                        foreach (XmlAttribute Entry in Child0.Attributes)
                            if (Entry.Name == "Length")
                                Data.ObjectList = new string[int.Parse(Entry.Value)];

                        int i0 = 0;
                        foreach (XmlNode ObjectList in Child0.ChildNodes)
                        {
                            if (ObjectList.Name == "String")
                            {
                                foreach (XmlAttribute Entry in ObjectList.Attributes)
                                    System.XMLReader(Entry, ref Data.ObjectList[i0], "Name");
                                i0++;
                            }
                        }
                    }
                    else if (Child0.Name == "PlayControl")
                    {
                        Data.PlayControl = new PlayControl();
                        foreach (XmlAttribute Entry in Child0.Attributes)
                        {
                            System.XMLReader(Entry, ref Data.PlayControl.Begin, "Begin");
                            System.XMLReader(Entry, ref Data.PlayControl.FPS, "FPS");
                            System.XMLReader(Entry, ref Data.PlayControl.Offset, "Offset");
                            System.XMLReader(Entry, ref Data.PlayControl.Size, "Size");
                        }
                    }
                    else if (Child0.Name == "Point")
                    {
                        foreach (XmlAttribute Entry in Child0.Attributes)
                            if (Entry.Name == "Length")
                                Data.Point = new ModelTransform[int.Parse(Entry.Value)];

                        int i0 = 0;
                        foreach (XmlNode Points in Child0.ChildNodes)
                            if (Points.Name == "Point")
                            {
                                KeyReader(Points, ref Data.Point[i0]);
                                i0++;
                            }
                    }
                    else if (Child0.Name == "PostProcess")
                    {
                        Data.PostProcess = new PostProcess { Boolean = true };
                        foreach (XmlNode Light in Child0.ChildNodes)
                        {
                            KeyReader(Light, ref Data.PostProcess.Ambient, "Ambient");
                            KeyReader(Light, ref Data.PostProcess.Diffuse, "Diffuse");
                            KeyReader(Light, ref Data.PostProcess.LensFlare, "LensFlare");
                            KeyReader(Light, ref Data.PostProcess.LensGhost, "LensGhost");
                            KeyReader(Light, ref Data.PostProcess.LensShaft, "LensShaft");
                            KeyReader(Light, ref Data.PostProcess.Specular, "Specular");
                        }
                    }
                }
            }
            System.doc = null;
        }

        static void XMLWriter(string file)
        {
            System.doc = new XmlDocument();
            if (File.Exists(file + ".xml"))
                File.Delete(file + ".xml");
            System.doc.InsertBefore(System.doc.CreateXmlDeclaration("1.0",
                "utf-8", null), System.doc.DocumentElement);
            XmlElement A3D = System.doc.CreateElement("A3D");
            System.XMLCompact = true;

            System.XMLWriter(ref A3D, Encoding.ASCII.GetString(BitConverter.
                GetBytes(Data.Header.Signature)), "Signature");

            XmlElement _ = System.doc.CreateElement("_");
            if (Data.Header.Signature == 0x5F5F5F43)
                System.XMLWriter(ref _, Data._.CompressF16.ToString().ToUpper(), "CompressF16");
            System.XMLWriter(ref _, Data._.ConverterVersion, "ConverterVersion");
            System.XMLWriter(ref _, Data._.FileName, "FileName");
            System.XMLWriter(ref _, Data._.PropertyVersion, "PropertyVersion");
            A3D.AppendChild(_);

            if (Data.Camera.Auxiliary.Boolean || Data.Camera.Root.Length != 0)
            {
                XmlElement Camera = System.doc.CreateElement("Camera");
                if (Data.Camera.Auxiliary.Boolean)
                {
                    XmlElement Auxiliary = System.doc.CreateElement("Auxiliary");
                    KeyWriter(ref Auxiliary, Data.Camera.Auxiliary.AutoExposure, "AutoExposure");
                    KeyWriter(ref Auxiliary, Data.Camera.Auxiliary.Exposure, "Exposure");
                    KeyWriter(ref Auxiliary, Data.Camera.Auxiliary.Gamma, "Gamma");
                    KeyWriter(ref Auxiliary, Data.Camera.Auxiliary.GammaRate, "GammaRate");
                    KeyWriter(ref Auxiliary, Data.Camera.Auxiliary.Saturate, "Saturate");
                    Camera.AppendChild(Auxiliary);
                }

                if (Data.Camera.Root.Length != 0)
                {
                    XmlElement Root = System.doc.CreateElement("Root");
                    System.XMLWriter(ref Root, Data.Camera.Root.Length, "Length");
                    for (int i = 0; i < Data.Camera.Root.Length; i++)
                    {
                        XmlElement RootEntry = System.doc.CreateElement("RootEntry");
                        KeyWriter(ref RootEntry, Data.Camera.Root[i].MT);
                        KeyWriter(ref RootEntry, Data.Camera.Root[i].Interest.MT, "Interest");
                        
                        XmlElement ViewPoint = System.doc.CreateElement("ViewPoint");
                        if (Data.Camera.Root[i].ViewPoint.Aspect != -1)
                            System.XMLWriter(ref ViewPoint, Data.Camera.Root[i].ViewPoint.Aspect, "Aspect");
                        if (Data.Camera.Root[i].ViewPoint.CameraApertureH != -1)
                            System.XMLWriter(ref ViewPoint, Data.Camera.Root[i].ViewPoint.CameraApertureH, "CameraApertureH");
                        if (Data.Camera.Root[i].ViewPoint.CameraApertureW != -1)
                            System.XMLWriter(ref ViewPoint, Data.Camera.Root[i].ViewPoint.CameraApertureW, "CameraApertureW");
                        KeyWriter(ref ViewPoint, Data.Camera.Root[i].ViewPoint.FocalLength, "FocalLength");
                        KeyWriter(ref ViewPoint, Data.Camera.Root[i].ViewPoint.FOV, "FOV");
                        System.XMLWriter(ref ViewPoint, Data.Camera.Root[i].ViewPoint.FOVHorizontal, "FOVHorizontal");
                        KeyWriter(ref ViewPoint, Data.Camera.Root[i].ViewPoint.Roll, "Roll");
                        KeyWriter(ref ViewPoint, Data.Camera.Root[i].ViewPoint.MT);
                        RootEntry.AppendChild(ViewPoint);
                        Root.AppendChild(RootEntry);
                    }
                    Camera.AppendChild(Root);
                }
                A3D.AppendChild(Camera);
            }

            if (Data.Chara.Length != 0)
            {
                XmlElement Charas = System.doc.CreateElement("Charas");
                System.XMLWriter(ref Charas, Data.Curve.Length, "Length");
                for (int i = 0; i < Data.Curve.Length; i++)
                    KeyWriter(ref Charas, Data.Chara[i], "Chara");
                A3D.AppendChild(Charas);
            }

            if (Data.Curve.Length != 0)
            {
                XmlElement Curves = System.doc.CreateElement("Curves");
                System.XMLWriter(ref Curves, Data.Curve.Length, "Length");
                for (int i = 0; i < Data.Curve.Length; i++)
                {
                    XmlElement Curve = System.doc.CreateElement("Curve");
                    System.XMLWriter(ref Curve, Data.Curve[i].Name, "Name");
                    KeyWriter(ref Curve, Data.Curve[i].CV, "CV");
                    Curves.AppendChild(Curve);
                }
                A3D.AppendChild(Curves);
            }

            if (Data.DOF.Name != null)
                KeyWriter(ref A3D, Data.DOF, "DOF");

            if (Data.Event.Length != 0)
            {
                XmlElement Events = System.doc.CreateElement("Events");
                for (int i0 = 0; i0 < Data.Event.Length; i0++)
                {
                    System.Write("event." + i0 + ".begin", System.ToString(Data.Event[i0].Begin));
                    System.Write("event." + i0 + ".clip_begin", System.ToString(Data.Event[i0].ClipBegin));
                    System.Write("event." + i0 + ".clip_en", System.ToString(Data.Event[i0].ClipEnd));
                    System.Write("event." + i0 + ".end", System.ToString(Data.Event[i0].End));
                    System.Write("event." + i0 + ".name", Data.Event[i0].Name);
                    System.Write("event." + i0 + ".param1", Data.Event[i0].Param1);
                    System.Write("event." + i0 + ".ref", Data.Event[i0].Ref);
                    System.Write("event." + i0 + ".time_ref_scale", System.ToString(Data.Event[i0].TimeRefScale));
                    System.Write("event." + i0 + ".type=", Data.Event[i0].Type.ToString());
                }
                A3D.AppendChild(Events);
            }

            if (Data.Fog.Length != 0)
            {
                XmlElement Fogs = System.doc.CreateElement("Fogs");
                System.XMLWriter(ref Fogs, Data.Fog.Length, "Length");
                for (int i = 0; i < Data.Fog.Length; i++)
                {
                    XmlElement Fog = System.doc.CreateElement("Fog");
                    KeyWriter(ref Fog, Data.Fog[i].Diffuse, "Diffuse");
                    KeyWriter(ref Fog, Data.Fog[i].Density, "Density");
                    KeyWriter(ref Fog, Data.Fog[i].End, "End");
                    System.XMLWriter(ref Fog, Data.Fog[i].Id, "Id");
                    KeyWriter(ref Fog, Data.Fog[i].Start, "Start");
                    Fogs.AppendChild(Fog);
                }
                A3D.AppendChild(Fogs);
            }

            if (Data.Light.Length != 0)
            {
                XmlElement Lights = System.doc.CreateElement("Lights");
                System.XMLWriter(ref Lights, Data.Light.Length, "Length");
                for (int i = 0; i < Data.Light.Length; i++)
                {
                    XmlElement Light = System.doc.CreateElement("Light");
                    KeyWriter(ref Light, Data.Light[i].Ambient, "Ambient");
                    KeyWriter(ref Light, Data.Light[i].Diffuse, "Diffuse");
                    System.XMLWriter(ref Light, Data.Light[i].Id, "Id");
                    KeyWriter(ref Light, Data.Light[i].Incandescence, "Incandescence");
                    System.XMLWriter(ref Light, Data.Light[i].Name, "Name");
                    KeyWriter(ref Light, Data.Light[i].Position, "Position");
                    KeyWriter(ref Light, Data.Light[i].Specular, "Specular");
                    KeyWriter(ref Light, Data.Light[i].SpotDirection, "SpotDirection");
                    System.XMLWriter(ref Light, Data.Light[i].Type, "Type");
                    Lights.AppendChild(Light);
                }
                A3D.AppendChild(Lights);
            }

            if (Data.MObjectHRC.Length != 0)
            {
                XmlElement MObjectsHRC = System.doc.CreateElement("MObjectsHRC");
                System.XMLWriter(ref MObjectsHRC, Data.MObjectHRC.Length, "Length");
                for (int i0 = 0; i0 < Data.MObjectHRC.Length; i0++)
                {
                    XmlElement MObjectHRC = System.doc.CreateElement("MObjectHRC");
                    System.XMLWriter(ref MObjectHRC, Data.MObjectHRC[i0].Name, "Name");
                    if (Data.MObjectHRC[i0].Instance != null)
                    {
                        XmlElement Instances = System.doc.CreateElement("Instances");
                        System.XMLWriter(ref Instances, Data.MObjectHRC[i0].Instance.Length, "Length");
                        for (int i1 = 0; i1 < Data.MObjectHRC[i0].Instance.Length; i1++)
                        {
                            XmlElement Instance = System.doc.CreateElement("Instance");
                            KeyWriter(ref Instance, Data.MObjectHRC[i0].Instance[i1]);
                            Instances.AppendChild(Instance);
                        }
                        MObjectHRC.AppendChild(Instances);
                    }

                    if (Data.MObjectHRC[i0].Node != null)
                    {
                        XmlElement Nodes = System.doc.CreateElement("Nodes");
                        System.XMLWriter(ref Nodes, Data.MObjectHRC[i0].Node.Length, "Length");
                        for (int i1 = 0; i1 < Data.MObjectHRC[i0].Node.Length; i1++)
                        {
                            XmlElement Node = System.doc.CreateElement("Node");
                            System.XMLWriter(ref Node, Data.MObjectHRC[i0].Node[i1].Name, "Name");
                            System.XMLWriter(ref Node, Data.MObjectHRC[i0].Node[i1].Parent, "Parent");
                            KeyWriter(ref Node, Data.MObjectHRC[i0].Node[i1].MT);
                            Nodes.AppendChild(Node);
                        }
                        MObjectHRC.AppendChild(Nodes);
                    }
                    MObjectsHRC.AppendChild(MObjectHRC);
                }
                A3D.AppendChild(MObjectsHRC);
            }

            if (Data.MObjectHRCList.Length != 0)
            {
                XmlElement MObjectHRCList = System.doc.CreateElement("MObjectHRCList");
                System.XMLWriter(ref MObjectHRCList, Data.MObjectHRCList.Length, "Length");
                for (int i = 0; i < Data.MObjectHRCList.Length; i++)
                {
                    XmlElement String = System.doc.CreateElement("String");
                    System.XMLWriter(ref String, Data.MObjectHRCList[i], "Name");
                    MObjectHRCList.AppendChild(String);
                }
                A3D.AppendChild(MObjectHRCList);
            }

            if (Data.Motion.Length != 0)
            {
                XmlElement Motion = System.doc.CreateElement("Motion");
                System.XMLWriter(ref Motion, Data.Motion.Length, "Length");
                for (int i = 0; i < Data.Motion.Length; i++)
                {
                    XmlElement String = System.doc.CreateElement("String");
                    System.XMLWriter(ref String, Data.Motion[i], "Name");
                    Motion.AppendChild(String);
                }
                A3D.AppendChild(Motion);
            }

            if (Data.Object.Length != 0)
            {
                XmlElement Objects = System.doc.CreateElement("Objects");
                System.XMLWriter(ref Objects, Data.Object.Length, "Length");
                for (int i0 = 0; i0 < Data.Object.Length; i0++)
                {
                    XmlElement Object = System.doc.CreateElement("Object");
                    if (Data.Object[i0].Morph != null)
                    {
                        System.XMLWriter(ref Object, Data.Object[i0].Morph, "Morph");
                        System.XMLWriter(ref Object, Data.Object[i0].MorphOffset, "MorphOffset");
                    }
                    System.XMLWriter(ref Object, Data.Object[i0].MT.Name, "Name");
                    System.XMLWriter(ref Object, Data.Object[i0].ParentName, "ParentName");
                    if (Data.Object[i0].TexPat != null)
                    {
                        XmlElement TexPats = System.doc.CreateElement("TexPats");
                        System.XMLWriter(ref TexPats, Data.Object[i0].TexPat.Length, "Length");
                        for (int i1 = 0; i1 < Data.Object[i0].TexPat.Length; i1++)
                        {
                            XmlElement TexPat = System.doc.CreateElement("TexPat");
                            System.XMLWriter(ref TexPat, Data.Object[i0].TexPat[i1].Name, "Name");
                            System.XMLWriter(ref TexPat, Data.Object[i0].TexPat[i1].Pat, "Pat");
                            System.XMLWriter(ref TexPat, Data.Object[i0].TexPat[i1].PatOffset, "PatOffset");
                            TexPats.AppendChild(TexPat);
                        }
                        Object.AppendChild(TexPats);
                    }
                    if (Data.Object[i0].TexTrans != null)
                    {
                        XmlElement TexTransforms = System.doc.CreateElement("TexTransforms");
                        System.XMLWriter(ref TexTransforms, Data.Object[i0].TexTrans.Length, "Length");
                        for (int i1 = 0; i1 < Data.Object[i0].TexTrans.Length; i1++)
                        {
                            XmlElement TexTransform = System.doc.CreateElement("TexTransform");
                            KeyWriter(ref TexTransform, Data.Object[i0].TexTrans[i1].CoverageU, "CoverageU");
                            KeyWriter(ref TexTransform, Data.Object[i0].TexTrans[i1].CoverageV, "CoverageV");
                            System.XMLWriter(ref TexTransform, Data.Object[i0].TexTrans[i1].Name, "Name");
                            KeyWriter(ref TexTransform, Data.Object[i0].TexTrans[i1].OffsetU, "OffsetU");
                            KeyWriter(ref TexTransform, Data.Object[i0].TexTrans[i1].OffsetV, "OffsetV");
                            KeyWriter(ref TexTransform, Data.Object[i0].TexTrans[i1].RepeatU, "RepeatU");
                            KeyWriter(ref TexTransform, Data.Object[i0].TexTrans[i1].RepeatV, "RepeatV");
                            KeyWriter(ref TexTransform, Data.Object[i0].TexTrans[i1].Rotate, "Rotate");
                            KeyWriter(ref TexTransform, Data.Object[i0].TexTrans[i1].RotateFrame, "RotateFrame");
                            KeyWriter(ref TexTransform, Data.Object[i0].TexTrans[i1].TranslateFrameU, "TranslateFrameU");
                            KeyWriter(ref TexTransform, Data.Object[i0].TexTrans[i1].TranslateFrameV, "TranslateFrameV");
                            TexTransforms.AppendChild(TexTransform);
                        }
                        Object.AppendChild(TexTransforms);
                    }
                    System.XMLWriter(ref Object, Data.Object[i0].MT.UIDName, "UIDName");
                    KeyWriter(ref Object, Data.Object[i0].MT);
                    Objects.AppendChild(Object);
                }
                A3D.AppendChild(Objects);
            }

            if (Data.ObjectHRC.Length != 0)
            {
                XmlElement ObjectsHRC = System.doc.CreateElement("ObjectsHRC");
                System.XMLWriter(ref ObjectsHRC, Data.ObjectHRC.Length, "Length");
                for (int i0 = 0; i0 < Data.ObjectHRC.Length; i0++)
                {
                    XmlElement ObjectHRC = System.doc.CreateElement("ObjectHRC");
                    System.XMLWriter(ref ObjectHRC, Data.ObjectHRC[i0].Name, "Name");
                    if (Data.ObjectHRC[i0].Shadow != 0)
                        System.XMLWriter(ref ObjectHRC, Data.ObjectHRC[i0].Shadow, "Shadow");
                    System.XMLWriter(ref ObjectHRC, Data.ObjectHRC[i0].UIDName, "UIDName");
                    if (Data.ObjectHRC[i0].Node != null)
                    {
                        XmlElement Nodes = System.doc.CreateElement("Nodes");
                        System.XMLWriter(ref Nodes, Data.ObjectHRC[i0].Node.Length, "Length");
                        for (int i1 = 0; i1 < Data.ObjectHRC[i0].Node.Length; i1++)
                        {
                            XmlElement Node = System.doc.CreateElement("Node");
                            System.XMLWriter(ref Node, Data.ObjectHRC[i0].Node[i1].Name, "Name");
                            System.XMLWriter(ref Node, Data.ObjectHRC[i0].Node[i1].Parent, "Parent");
                            KeyWriter(ref Node, Data.ObjectHRC[i0].Node[i1].MT);
                            Nodes.AppendChild(Node);
                        }
                        ObjectHRC.AppendChild(Nodes);
                    }
                    ObjectsHRC.AppendChild(ObjectHRC);
                }
                A3D.AppendChild(ObjectsHRC);
            }

            if (Data.ObjectHRCList.Length != 0)
            {
                XmlElement ObjectHRCList = System.doc.CreateElement("ObjectHRCList");
                System.XMLWriter(ref ObjectHRCList, Data.ObjectHRCList.Length, "Length");
                for (int i = 0; i < Data.ObjectHRCList.Length; i++)
                {
                    XmlElement String = System.doc.CreateElement("String");
                    System.XMLWriter(ref String, Data.ObjectHRCList[i], "Name");
                    ObjectHRCList.AppendChild(String);
                }
                A3D.AppendChild(ObjectHRCList);
            }

            if (Data.ObjectList.Length != 0)
            {
                XmlElement ObjectList = System.doc.CreateElement("ObjectList");
                System.XMLWriter(ref ObjectList, Data.ObjectList.Length, "Length");
                for (int i = 0; i < Data.ObjectList.Length; i++)
                {
                    XmlElement String = System.doc.CreateElement("String");
                    System.XMLWriter(ref String, Data.ObjectList[i], "Name");
                    ObjectList.AppendChild(String);
                }
                A3D.AppendChild(ObjectList);
            }

            XmlElement PlayControl = System.doc.CreateElement("PlayControl");
            System.XMLWriter(ref PlayControl, Data.PlayControl.Begin, "Begin");
            System.XMLWriter(ref PlayControl, Data.PlayControl.FPS, "FPS");
            System.XMLWriter(ref PlayControl, Data.PlayControl.Offset, "Offset");
            System.XMLWriter(ref PlayControl, Data.PlayControl.Size, "Size");
            A3D.AppendChild(PlayControl);

            if (Data.Point.Length != 0)
            {
                XmlElement Points = System.doc.CreateElement("Points");
                System.XMLWriter(ref Points, Data.Curve.Length, "Length");
                for (int i = 0; i < Data.Point.Length; i++)
                    KeyWriter(ref Points, Data.Point[i], "Point");
                A3D.AppendChild(Points);
            }

            XmlElement PostProcess = System.doc.CreateElement("PostProcess");
            KeyWriter(ref PostProcess, Data.PostProcess.Ambient, "Ambient");
            KeyWriter(ref PostProcess, Data.PostProcess.Diffuse, "Diffuse");
            KeyWriter(ref PostProcess, Data.PostProcess.LensFlare, "LensFlare");
            KeyWriter(ref PostProcess, Data.PostProcess.LensGhost, "LensGhost");
            KeyWriter(ref PostProcess, Data.PostProcess.LensShaft, "LensShaft");
            KeyWriter(ref PostProcess, Data.PostProcess.Specular, "Specular");
            A3D.AppendChild(PostProcess);

            System.doc.AppendChild(A3D);

            XmlWriterSettings settings = new XmlWriterSettings
            {
                Encoding = Encoding.UTF8,
                NewLineChars = "\n",
                Indent = true,
                IndentChars = "\t"
            };

            XmlWriter writer = XmlWriter.Create(file + ".xml", settings);
            System.doc.Save(writer);
            writer.Close();
            System.doc = null;
        }

        static void KeyReader(XmlNode element, ref ModelTransform MT, string name, ref int i)
        {
            if (element.Name == name)
            {
                KeyReader(element, ref MT);
                i++;
            }
        }

        static void KeyReader(XmlNode element, ref ModelTransform MT, string name)
        {
            if (element.Name == name)
                KeyReader(element, ref MT);
        }
        
        static void KeyReader(XmlNode element, ref ModelTransform MT)
        {
            foreach (XmlAttribute Entry in element.Attributes)
            {
                System.XMLReader(Entry, ref MT.Name, "Name");
                System.XMLReader(Entry, ref MT.UIDName, "UIDName");
            }

            foreach (XmlNode Object in element.ChildNodes)
            {
                KeyReader(Object, ref MT.Rot, "Rot");
                KeyReader(Object, ref MT.Scale, "Scale");
                KeyReader(Object, ref MT.Trans, "Trans");
                KeyReader(Object, ref MT.Visibility, "Visibility");
            }
        }

        static void KeyReader(XmlNode element, ref RGBAKey k, string name)
        {
            if (element.Name == name)
                KeyReader(element, out k);
        }

        static void KeyReader(XmlNode element, out RGBAKey k)
        {
            k = new RGBAKey { Boolean = true };
            foreach (XmlAttribute Entry in element.Attributes)
                System.XMLReader(Entry, ref k.Alpha, "Alpha");

            foreach (XmlNode Child in element.ChildNodes)
            {
                KeyReader(Child, ref k.R, "R");
                KeyReader(Child, ref k.G, "G");
                KeyReader(Child, ref k.B, "B");
                KeyReader(Child, ref k.A, "A");
            }
        }

        static void KeyReader(XmlNode element, ref Vector3Key k, string name)
        {
            if (element.Name == name)
                KeyReader(element, out k);
        }

        static void KeyReader(XmlNode element, out Vector3Key k)
        {
            k = new Vector3Key();
            foreach (XmlNode Child in element.ChildNodes)
            {
                KeyReader(Child, ref k.X, "X");
                KeyReader(Child, ref k.Y, "Y");
                KeyReader(Child, ref k.Z, "Z");
            }
        }

        static void KeyReader(XmlNode element, ref Key k, string name)
        {
            if (element.Name == name)
                KeyReader(element, out k);
        }

        static void KeyReader(XmlNode element, out Key k)
        {
            k = new Key { Boolean = true, EPTypePost = -1, EPTypePre = -1, Length = -1, Max = -1,
                Type = 0x0000, Value = -1, RawData = new RawData { KeyType = -1 }, Trans = null };
            foreach (XmlAttribute Entry in element.Attributes)
            {
                System.XMLReader(Entry, ref k.EPTypePost, "EPTypePost");
                System.XMLReader(Entry, ref k.EPTypePre, "EPTypePre");
                System.XMLReader(Entry, ref k.Length, "Length");
                System.XMLReader(Entry, ref k.Max, "Max");
                System.XMLReader(Entry, ref k.Type, "Type");
                System.XMLReader(Entry, ref k.Value, "Value");
                System.XMLReader(Entry, ref k.EPTypePost, "Post");
                System.XMLReader(Entry, ref k.EPTypePre, "Pre");
                System.XMLReader(Entry, ref k.Length, "L");
                System.XMLReader(Entry, ref k.Max, "M");
                System.XMLReader(Entry, ref k.Type, "T");
                System.XMLReader(Entry, ref k.Value, "V");
            }

            if (k.Type != 0x0000 && k.Type != 0x0001)
            {
                k.Trans = new Transform[k.Length];
                int i = 0;

                foreach (XmlNode Child in element.ChildNodes)
                {
                    foreach (XmlAttribute Entry in Child.Attributes)
                    {
                        System.XMLReader(Entry, ref k.Trans[i].Frame, "Frame");
                        System.XMLReader(Entry, ref k.Trans[i].Value1, "Value1");
                        System.XMLReader(Entry, ref k.Trans[i].Value2, "Value2");
                        System.XMLReader(Entry, ref k.Trans[i].Value3, "Value3");
                        System.XMLReader(Entry, ref k.Trans[i].Type, "Type");
                        System.XMLReader(Entry, ref k.Trans[i].Frame, "F");
                        System.XMLReader(Entry, ref k.Trans[i].Value1, "V1");
                        System.XMLReader(Entry, ref k.Trans[i].Value2, "V2");
                        System.XMLReader(Entry, ref k.Trans[i].Value3, "V3");
                        System.XMLReader(Entry, ref k.Trans[i].Type, "T");
                    }

                    i++;
                    if (i == k.Length)
                        break;
                }

                double kFrameMax = 0;
                for (i = 0; i < k.Length; i++)
                {
                    if (kFrameMax < k.Trans[i].Frame)
                        kFrameMax = k.Trans[i].Frame;
                }

                if (kFrameMax > k.Max && kFrameMax < Data.PlayControl.Size)
                    k.Max = Data.PlayControl.Size;
                else if (kFrameMax > k.Max && kFrameMax > Data.PlayControl.Size)
                    k.Max = Data.PlayControl.Size = (int)Math.Ceiling(kFrameMax + 1);
            }
            else if (k.Type == 0x0000)
                k.Value = 0;
        }

        static void KeyWriter(ref XmlElement element, ModelTransform MT, string name)
        {
            XmlElement Child = System.doc.CreateElement(name);
            KeyWriter(ref Child, MT);
            element.AppendChild(Child);
        }

        static void KeyWriter(ref XmlElement element, ModelTransform MT)
        {
            System.XMLWriter(ref element, MT.Name, "Name");
            System.XMLWriter(ref element, MT.UIDName, "UIDName");
            KeyWriter(ref element, MT.Rot, "Rot");
            KeyWriter(ref element, MT.Scale, "Scale");
            KeyWriter(ref element, MT.Trans, "Trans");
            KeyWriter(ref element, MT.Visibility, "Visibility");
        }

        static void KeyWriter(ref XmlElement element, RGBAKey k, string name)
        {
            if (k.Boolean)
            {
                XmlElement Child = System.doc.CreateElement(name);
                if (k.Alpha)
                    System.XMLWriter(ref Child, k.Alpha, "Alpha");
                KeyWriter(ref Child, k.R, "R");
                KeyWriter(ref Child, k.G, "G");
                KeyWriter(ref Child, k.B, "B");
                if (k.Alpha)
                    KeyWriter(ref Child, k.A, "A");
                element.AppendChild(Child);
            }
        }

        static void KeyWriter(ref XmlElement element, Vector3Key k, string name)
        {
            XmlElement Child = System.doc.CreateElement(name);
            KeyWriter(ref Child, k.X, "X");
            KeyWriter(ref Child, k.Y, "Y");
            KeyWriter(ref Child, k.Z, "Z");
            element.AppendChild(Child);
        }

        static void KeyWriter(ref XmlElement element, Key k, string name)
        {
            if (k.Boolean && k.Type != -1)
            {
                XmlElement Keys = System.doc.CreateElement(name);
                System.XMLWriter(ref Keys, k.Type.ToString(), "T");
                if (k.Trans != null)
                {
                    if (k.EPTypePost != -1)
                        System.XMLWriter(ref Keys, k.EPTypePost, "Post");
                    if (k.EPTypePre != -1)
                        System.XMLWriter(ref Keys, k.EPTypePre, "Pre");
                    if (k.Length != -1)
                        System.XMLWriter(ref Keys, k.Length, "L");
                    if (k.Max != -1)
                        System.XMLWriter(ref Keys, k.Max, "M");
                    for (int i = 0; i < k.Trans.Length; i++)
                    {
                        int Type = k.Trans[i].Type;
                        if (k.Trans[i].Value3 == 0 && Type > 2)
                            Type = 2;
                        if (k.Trans[i].Value2 == 0 && Type > 1)
                            Type = 1;
                        if (k.Trans[i].Value1 == 0 && Type > 0)
                            Type = 0;
                        XmlElement Key = System.doc.CreateElement("Key");
                        System.XMLWriter(ref Key, Type, "T");
                        System.XMLWriter(ref Key, k.Trans[i].Frame, "F");
                        if (Type > 0)
                            System.XMLWriter(ref Key, k.Trans[i].Value1, "V1");
                        if (Type > 1)
                            System.XMLWriter(ref Key, k.Trans[i].Value2, "V2");
                        if (Type > 2)
                            System.XMLWriter(ref Key, k.Trans[i].Value3, "V3");
                        Keys.AppendChild(Key);
                    }
                }
                else if (k.Value != 0)
                    System.XMLWriter(ref Keys, k.Value, "V");
                element.AppendChild(Keys);
            }
        }

        static bool CheckString(string Template, int i, ref bool Data)
        { bool Check = A3D.Data.Name[i] == Template; if (Check) Data = bool.Parse(A3D.Data.Value[i]); return Check; }

        static bool CheckString(string Template, int i, ref int Data)
        { bool Check = A3D.Data.Name[i] == Template; if (Check) Data = int.Parse(A3D.Data.Value[i]); return Check; }

        static bool CheckString(string Template, int i, ref double Data)
        { bool Check = A3D.Data.Name[i] == Template; if (Check) Data = System.ToDouble(A3D.Data.Value[i]); return Check; }

        static bool CheckString(string Template, int i, ref string Data)
        { bool Check = A3D.Data.Name[i] == Template; if (Check) Data = A3D.Data.Value[i]; return Check; }

        static Vector3 ReadVector3Int()
        { return new Vector3 { X = System.ReadInt32(), Y = System.ReadInt32(), Z = System.ReadInt32() }; }

        static int[] SortWriter(int Length)
        {
            List<string> A = new List<string>();
            for (int i = 0; i < Length; i++)
                A.Add(i.ToString());
            A.Sort();
            int[] B = new int[Length];
            for (int i = 0; i < Length; i++)
                B[i] = int.Parse(A[i]);
            return B;
        }

        static List<ListRegion> GetListRegion(string Name)
        {
            List<ListRegion> Region = new List<ListRegion>();

            int Start = 0;
            int End = 0;
            int LastValue = 0;
            bool HasStart = false;
            for (int i = 0; i < Data.Name.Count; i++)
            {
                if (Data.Name[i].StartsWith(Name))
                {
                    if (!HasStart)
                    {
                        LastValue = Start = i;
                        HasStart = true;
                    }
                    else if (LastValue != i)
                    {
                        HasStart = false;
                        Region.Add(new ListRegion { End = End, Start = Start });
                    }
                    LastValue = End = i + 1;
                }

            }
            Region.Add(new ListRegion { End = End, Start = Start });
            return Region;
        }

        public struct ListRegion
        {
            public int Start;
            public int End;
        }

        public struct _
        {
            public int CompressF16;
            public string FileName;
            public string PropertyVersion;
            public string ConverterVersion;
        }
#endif
        public struct A3DA
        {
#if !A3DAList
            public string[] Motion;
            public string[] ObjectList;
            public string[] ObjectHRCList;
            public string[] MObjectHRCList;
            public _ _;
#endif
            public List<string> Name;
            public List<string> Value;
            public Header Head;
#if !A3DAList
            public Fog[] Fog;
            public Curve[] Curve;
            public Event[] Event;
            public Light[] Light;
            public Camera Camera;
            public Object[] Object;
            public ObjectHRC[] ObjectHRC;
            public MObjectHRC[] MObjectHRC;
            public PlayControl PlayControl;
            public PostProcess PostProcess;
            public ModelTransform DOF;
            public ModelTransform[] Chara;
            public ModelTransform[] Point;
#endif
            public System.Header Header;
        }
#if !A3DAList
        public struct Camera
        {
            public CameraAuxiliary Auxiliary;
            public CameraRoot[] Root;
        }

        public struct CameraAuxiliary
        {
            public bool Boolean;
            public Key Gamma;
            public Key Exposure;
            public Key Saturate;
            public Key GammaRate;
            public Key AutoExposure;
        }

        public struct CameraRoot
        {
            public Interest Interest;
            public ViewPoint ViewPoint;
            public ModelTransform MT;
            public List<ListRegion> Region;
        }

        public struct Curve
        {
            public string Name;
            public Key CV;
            public List<ListRegion> Region;
        }
        
        public struct Event
        {
            public int Type;
            public double End;
            public double Begin;
            public double ClipEnd;
            public double ClipBegin;
            public double TimeRefScale;
            public string Name;
            public string Param1;
            public string Ref;
            public List<ListRegion> Region;
        }

        public struct Fog
        {
            public int Id;
            public Key End;
            public Key Start;
            public Key Density;
            public RGBAKey Diffuse;
            public List<ListRegion> Region;
        }
#endif
        public struct Header
        {
            public int Count;
            public int BinaryLength;
            public int BinaryOffset;
            public int HeaderOffset;
            public int StringLength;
            public int StringOffset;
        }
#if !A3DAList
        public struct Interest
        {
            public ModelTransform MT;
        }

        public struct Key
        {
            public int Type;
            public int Length;
            public int BinOffset;
            public bool Boolean;
            public double Max;
            public double Value;
            public double EPTypePost;
            public double EPTypePre;
            public RawData RawData;
            public Transform[] Trans;
            public List<ListRegion> Region;
        }

        public struct Light
        {
            public int Id;
            public string Name;
            public string Type;
            public RGBAKey Ambient;
            public RGBAKey Diffuse;
            public RGBAKey Specular;
            public RGBAKey Incandescence;
            public ModelTransform Position;
            public ModelTransform SpotDirection;
            public List<ListRegion> Region;
        }

        public struct RawData
        {
            public int KeyType;
            public int ValueListSize;
            public string ValueType;
            public string[] ValueList;
        }

        public struct MObjectHRC
        {
            public string Name;
            public Node[] Node;
            public ModelTransform[] Instance;
            public List<ListRegion> Region;
        }

        public struct ModelTransform
        {
            public int BinOffset;
            public bool Writed;
            public string Name;
            public string UIDName;
            public Key Visibility;
            public Vector3Key Rot;
            public Vector3Key Scale;
            public Vector3Key Trans;
            public List<ListRegion> Region;
        }
        
        public struct Node
        {
            public int Parent;
            public string Name;
            public ModelTransform MT;
        }

        public struct Object
        {
            public int MorphOffset;
            public string Morph;
            public string ParentName;
            public ModelTransform MT;
            public TexturePattern[] TexPat;
            public List<ListRegion> Region;
            public TextureTransform[] TexTrans;
        }

        public struct ObjectHRC
        {
            public double Shadow;
            public string Name;
            public string UIDName;
            public Node[] Node;
            public List<ListRegion> Region;
        }

        public struct PlayControl
        {
            public double Begin;
            public double FPS;
            public double Offset;
            public double Size;
        }

        public struct PostProcess
        {
            public bool Boolean;
            public Key LensFlare;
            public Key LensGhost;
            public Key LensShaft;
            public RGBAKey Ambient;
            public RGBAKey Diffuse;
            public RGBAKey Specular;
        }

        public struct RGBAKey
        {
            public bool Boolean;
            public bool Alpha;
            public Key R;
            public Key G;
            public Key B;
            public Key A;
            public List<ListRegion> Region;
        }

        public struct TexturePattern
        {
            public int PatOffset;
            public string Pat;
            public string Name;
        }

        public struct TextureTransform
        {
            public string Name;
            public Key CoverageU;
            public Key CoverageV;
            public Key OffsetU;
            public Key OffsetV;
            public Key RepeatU;
            public Key RepeatV;
            public Key Rotate;
            public Key RotateFrame;
            public Key TranslateFrameU;
            public Key TranslateFrameV;
        }

        public struct Transform
        {
            public int Type;
            public double Frame;
            public double Value1;
            public double Value2;
            public double Value3;
        }

        public struct Values
        {
            public List<int> BinOffset;
            public List<double> Value;
        }

        public struct Vector3
        {
            public double X;
            public double Y;
            public double Z;
        }

        public struct Vector3Key
        {
            public Key X;
            public Key Y;
            public Key Z;
        }
        
        public struct ViewPoint
        {
            public double Aspect;
            public double FOVHorizontal;
            public double CameraApertureH;
            public double CameraApertureW;
            public Key FOV;
            public Key Roll;
            public Key FocalLength;
            public ModelTransform MT;
        }
#endif
    }
}
