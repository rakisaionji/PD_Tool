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
    public class A3DA
    {
        private static readonly bool A3DCOpt = true;
        private static readonly string d = ".";
        private static readonly string BO = "bin_offset";
        private static readonly string MTBO = "model_transform.bin_offset";

        private int SOi0;
        private int SOi1;
        private int[] SO0;
        private int[] SO1;
        private string name;
        private string nameInt;
        private string nameView;

        private static int Offset;
        private static bool Base64;
        private static string value;
        private static string[] dataArray;
        private static Values UsedValues;
        private static Dictionary<string, object> Dict;

        public static A3DAData Data;
        public static KKtIO IO;

        public A3DA() { Data = new A3DAData(); Dict = new Dictionary<string, object>();
            IO = new KKtIO(); UsedValues = new Values(); }

        public int A3DAReader(string file, string ext)
        {
            name = "";
            Offset = 0;
            nameInt = "";
            nameView = "";
            dataArray = new string[4];
            Dict = new Dictionary<string, object>();
            Data = new A3DAData();

            IO = KKtIO.OpenReader(file + ext);
            IO.Format = KKtMain.Format.F;
            Data.Header.Signature = IO.ReadInt32();
            if (Data.Header.Signature == 0x41443341)
                Data.Header = IO.ReadHeader(true);
            else if (Data.Header.Signature != 0x44334123)
                return 0;

            Offset = (int)IO.Position - 4;
            Data.Header.Signature = IO.ReadInt32();

            if (Data.Header.Signature == 0x5F5F5F41)
                IO.Seek(0x10, 0);
            else if (Data.Header.Signature == 0x5F5F5F43)
            {
                IO.Seek(Offset + 0x10, 0);
                IO.ReadInt32();
                IO.ReadInt32();
                Data.Head.HeaderOffset = IO.ReadInt32(true, true);

                IO.Seek(Offset + Data.Head.HeaderOffset, 0);
                if (IO.ReadInt32() != 0x50)
                    return 0;
                Data.Head.StringOffset = IO.ReadInt32(true, true);
                Data.Head.StringLength = IO.ReadInt32(true, true);
                Data.Head.Count = IO.ReadInt32(true, true);
                if (IO.ReadInt32() != 0x4C42)
                    return 0;
                Data.Head.BinaryOffset = IO.ReadInt32(true, true);
                Data.Head.BinaryLength = IO.ReadInt32(true, true);

                IO.Seek(Offset + Data.Head.StringOffset, 0);
            }
            else
                return 0;

            string[] STRData = null;
            if (Data.Header.Signature == 0x5F5F5F43)
                STRData = IO.ReadString(Data.Head.StringLength).Split('\n');
            else
                STRData = IO.ReadString(IO.Length - 0x10      ).Split('\n');

            for (int i = 0; i < STRData.Length; i++)
            {
                dataArray = STRData[i].Split('=');
                if (dataArray.Length == 2)
                    KKtMain.GetDictionary(ref Dict, dataArray[0], dataArray[1]);
            }

            A3DAReader();

            if (Data.Header.Signature == 0x5F5F5F43)
            {
                IO.Seek(Offset + Data.Head.BinaryOffset, 0);
                Offset = (int)IO.Position;
                A3DCReader();
            }

            IO.Close();

            name = "";
            Offset = 0;
            nameInt = "";
            nameView = "";
            dataArray = null;
            Dict = null;
            return 1;
        }

        public void A3DAWriter(string file, bool A3DC)
        {
            DateTime date = DateTime.Now;
            ref A3DAData D = ref Data;
            if (A3DC && D._.CompressF16 != 0)
                IO.Write("", "#-compress_f16");
            if (!A3DC)
                IO.Write("#A3DA__________\n");
            IO.Write("#", DateTime.UtcNow.ToString("ddd MMM dd HH:mm:ss yyyy",
                System.Globalization.CultureInfo.InvariantCulture));
            if (A3DC && D._.CompressF16 != 0)
                IO.Write("_.compress_f16=", D._.CompressF16.ToString());

            IO.Write("_.converter.version=", D._.ConverterVersion.ToString());
            IO.Write("_.file_name=", D._.FileName.ToString());
            IO.Write("_.property.version=", D._.PropertyVersion.ToString());

            name = "camera_auxiliary.";

            if (D.Camera.Auxiliary.Boolean)
            {
                D.Camera.Auxiliary.AutoExposure.Write(name, A3DC, "auto_exposure");
                D.Camera.Auxiliary.    Exposure.Write(name, A3DC,      "exposure");
                D.Camera.Auxiliary.Gamma       .Write(name, A3DC, "gamma"        );
                D.Camera.Auxiliary.GammaRate   .Write(name, A3DC, "gamma_rate"   );
                D.Camera.Auxiliary.Saturate    .Write(name, A3DC, "saturate"     );
            }

            if (D.Camera.Root.Length != 0)
            {
                SO0 = SortWriter(D.Camera.Root.Length);
                SOi0 = 0;
                for (int i0 = 0; i0 < D.Camera.Root.Length; i0++)
                {
                    SOi0 = SO0[i0];
                    name     = "camera_root." + SOi0 + d;
                    nameInt  = "camera_root." + SOi0 + ".interest.";
                    nameView = "camera_root." + SOi0 + ".view_point.";

                    D.Camera.Root[SOi0].Interest.Write(nameInt, 0b1001111, A3DC);
                    D.Camera.Root[SOi0].MT      .Write(name   , 0b1001110, A3DC);
                    IO.Write(nameView + "aspect=",
                        KKtMain.ToString(D.Camera.Root[i0].ViewPoint.Aspect, 5));
                    if (D.Camera.Root[i0].ViewPoint.CameraApertureH != -1)
                        IO.Write(nameView + "camera_aperture_h=",
                            KKtMain.ToString(D.Camera.Root[i0].ViewPoint.CameraApertureH, 5));
                    if (D.Camera.Root[i0].ViewPoint.CameraApertureW != -1)
                        IO.Write(nameView + "camera_aperture_w=",
                            KKtMain.ToString(D.Camera.Root[i0].ViewPoint.CameraApertureW, 5));
                    D.Camera.Root[SOi0].ViewPoint.FocalLength.Write(nameView + "focal_length.", A3DC);
                    D.Camera.Root[SOi0].ViewPoint.FOV        .Write(nameView + "fov."         , A3DC);
                    IO.Write(nameView + "fov_is_horizontal=",
                        D.Camera.Root[i0].ViewPoint.FOVHorizontal.ToString());
                    D.Camera.Root[SOi0].ViewPoint.MT  .Write(nameView, 0b1000000, A3DC);
                    D.Camera.Root[SOi0].ViewPoint.Roll.Write(nameView + "roll." , A3DC);
                    D.Camera.Root[SOi0].ViewPoint.MT  .Write(nameView, 0b0001111, A3DC);
                    D.Camera.Root[SOi0].          MT  .Write(name    , 0b0000001, A3DC);
                }
                IO.Write("camera_root.length=", D.Camera.Root.Length.ToString());
            }

            if (D.Chara.Length != 0)
            {
                SO0 = SortWriter(D.Chara.Length);
                name = "chara.";

                for (int i0 = 0; i0 < D.Chara.Length; i0++)
                    D.Chara[SO0[i0]].Write(name + SO0[i0] + d, 0b1001111, A3DC);
                IO.Write(name + "length=", D.Chara.Length.ToString());
            }

            if (D.Curve.Length != 0)
            {
                SO0 = SortWriter(D.Curve.Length);
                SOi0 = 0;

                for (int i0 = 0; i0 < D.Curve.Length; i0++)
                {
                    SOi0 = SO0[i0];
                    name = "curve." + SOi0 + d;

                    D.Curve[SOi0].CV.Write(name + "cv.", A3DC);
                    IO.Write(name + "name=", D.Curve[SOi0].Name);
                }
                IO.Write("curve.length=", D.Curve.Length.ToString());
            }

            if (D.DOF.Name != null)
                D.DOF.Write("dof.", 0b1111111, A3DC);

            if (D.Event.Length != 0)
            {
                SO0 = SortWriter(D.Event.Length);
                SOi0 = 0;
                for (int i0 = 0; i0 < D.Event.Length; i0++)
                {
                    SOi0 = SO0[i0];
                    name = "event." + SOi0 + d;

                    IO.Write(name + "begin=", KKtMain.ToString(D.Event[SOi0].Begin));
                    IO.Write(name + "clip_begin=", KKtMain.ToString(D.Event[SOi0].ClipBegin));
                    IO.Write(name + "clip_en=", KKtMain.ToString(D.Event[SOi0].ClipEnd));
                    IO.Write(name + "end=", KKtMain.ToString(D.Event[SOi0].End));
                    IO.Write(name + "name=", D.Event[SOi0].Name);
                    IO.Write(name + "param1=", D.Event[SOi0].Param1);
                    IO.Write(name + "ref=", D.Event[SOi0].Ref);
                    IO.Write(name + "time_ref_scale=", KKtMain.ToString(D.Event[SOi0].TimeRefScale));
                    IO.Write(name + "type=", D.Event[SOi0].Type.ToString());
                }
            }

            if (D.Fog.Length != 0)
            {
                SO0 = SortWriter(D.Fog.Length);
                SOi0 = 0;
                for (int i0 = 0; i0 < D.Fog.Length; i0++)
                {
                    SOi0 = SO0[i0];
                    name = "fog." + SOi0 + d;

                    D.Fog[SOi0].Density.Write(name, A3DC, "density");
                    D.Fog[SOi0].Diffuse.Write(name, A3DC, "Diffuse");
                    D.Fog[SOi0].End    .Write(name, A3DC, "end"    );
                    IO.Write(name + "id=", D.Fog[SOi0].Id.ToString());
                    D.Fog[SOi0].Start  .Write(name, A3DC, "start");
                }
                IO.Write("fog.length=", D.Fog.Length.ToString());
            }

            if (D.Light.Length != 0)
            {
                SO0 = SortWriter(D.Light.Length);
                SOi0 = 0;
                for (int i0 = 0; i0 < D.Light.Length; i0++)
                {
                    SOi0 = SO0[i0];
                    name = "light." + SOi0 + d;

                    D.Light[SOi0].Ambient      .Write(name, A3DC, "Ambient"      );
                    D.Light[SOi0].Diffuse      .Write(name, A3DC, "Diffuse"      );
                    D.Light[SOi0].Incandescence.Write(name, A3DC, "Incandescence");
                    D.Light[SOi0].Specular     .Write(name, A3DC, "Specular"     );
                    IO.Write (name + "id="            , D.Light[SOi0].Id.ToString());
                    IO.Write (name + "name="          , D.Light[SOi0].Name);
                    D.Light[SOi0].Position     .Write(name + "position."      , 0b1001111, A3DC);
                    D.Light[SOi0].SpotDirection.Write(name + "spot_direction.", 0b1001111, A3DC);
                    IO.Write (name + "type="          , D.Light[SOi0].Type);
                }
                IO.Write("light.length=", D.Light.Length.ToString());
            }

            if (D.MObjectHRC.Length != 0)
            {
                SO0 = SortWriter(D.MObjectHRC.Length);
                SOi0 = 0;
                for (int i0 = 0; i0 < D.MObjectHRC.Length; i0++)
                {
                    SOi0 = SO0[i0];
                    name = "m_objhrc." + SOi0 + d;

                    if (D.MObjectHRC[SOi0].Instance != null)
                    {
                        name = "m_objhrc." + SOi0 + ".instance.";
                        int[] SO1 = SortWriter(D.MObjectHRC[i0].Instance.Length);
                        int SOi1 = 0;
                        for (int i1 = 0; i1 < D.MObjectHRC[i0].Instance.Length; i1++)
                        {
                            SOi1 = SO1[i1];
                            D.MObjectHRC[SOi0].Instance[SOi1].Write(name + SOi1 + d, 0b1111111, A3DC);
                        }
                        IO.Write(name + "length=",
                            D.MObjectHRC[SOi0].Instance.Length.ToString());
                    }
                    IO.Write(name + "name=", D.MObjectHRC[SOi0].Name);
                    if (D.MObjectHRC[SOi0].Node != null)
                    {
                        name = "m_objhrc." + SOi0 + ".node.";
                        int[] SO1 = SortWriter(D.MObjectHRC[i0].Node.Length);
                        int SOi1 = 0;
                        for (int i1 = 0; i1 < D.MObjectHRC[i0].Node.Length; i1++)
                        {
                            SOi1 = SO1[i1];
                            IO.Write(name + SOi1 + ".name=",
                                D.MObjectHRC[SOi0].Node[SOi1].Name);
                            IO.Write(name + SOi1 + ".parent=",
                                D.MObjectHRC[SOi0].Node[SOi1].Parent.ToString());
                            D.MObjectHRC[SOi0].Node[SOi1].MT.Write(name + SOi1 + d, 0b1001111, A3DC);

                        }
                        IO.Write(name + "length=", D.
                            MObjectHRC[SOi0].Node.Length.ToString());
                    }
                }
                IO.Write("m_objhrc.length=", D.MObjectHRC.Length.ToString());
            }

            if (D.MObjectHRCList.Length != 0)
            {
                SO0 = SortWriter(D.MObjectHRCList.Length);
                name = "m_objhrc_list.";
                for (int i0 = 0; i0 < D.MObjectHRCList.Length; i0++)
                    IO.Write(name + SO0[i0] + "=",
                        D.MObjectHRCList[SO0[i0]]);
                IO.Write(name + "length=", D.MObjectHRCList.Length.ToString());
            }

            if (D.Motion.Length != 0)
            {
                SO0 = SortWriter(D.Motion.Length);
                name = "motion.";
                for (int i0 = 0; i0 < D.Motion.Length; i0++)
                    IO.Write(name + SO0[i0] + ".name=",
                        D.Motion[SO0[i0]]);
                IO.Write(name + "length=", D.Motion.Length.ToString());
            }

            if (D.Object.Length != 0)
            {
                SO0 = SortWriter(D.Object.Length);
                SOi0 = 0;
                for (int i0 = 0; i0 < D.Object.Length; i0++)
                {
                    SOi0 = SO0[i0];
                    name = "object." + SOi0 + d;
                    
                    D.Object[SOi0].MT.Write(name, 0b1000000, A3DC);
                    if (D.Object[SOi0].Morph != null)
                    {
                        IO.Write(name + "morph=", D.Object[SOi0].Morph);
                        IO.Write(name + "morph_offset=",
                            D.Object[SOi0].MorphOffset.ToString());
                    }
                    D.Object[SOi0].MT.Write(name, 0b0100000, A3DC);
                    IO.Write(name + "parent_name=", D.Object[SOi0].ParentName);
                    D.Object[SOi0].MT.Write(name, 0b0001100, A3DC);

                    if (D.Object[SOi0].TP != null)
                    {
                        nameView = name + "tex_pat.";

                        SO1 = SortWriter(D.Object[SOi0].TP.Length);
                        SOi1 = 0;
                        for (int i1 = 0; i1 < D.Object[SOi0].TP.Length; i1++)
                        {
                            SOi1 = SO1[i1];
                            ref TexturePattern TP = ref D.Object[SOi0].TP[SOi1];
                            IO.Write(nameView + SOi1 + ".name=", TP.Name);
                            IO.Write(nameView + SOi1 + ".pat=", TP.Pat);
                            IO.Write(nameView + SOi1 + ".pat_offset=", TP.PatOffset.ToString());
                        }
                        IO.Write(nameView + "length=" + Data.Object[SOi0].TP.Length + "\n");
                    }

                    if (D.Object[SOi0].TT != null)
                    {
                        SO1 = SortWriter(D.Object[SOi0].TT.Length);
                        SOi1 = 0;
                        nameView = name + "tex_transform.";
                        for (int i1 = 0; i1 < D.Object[SOi0].TT.Length; i1++)
                        {
                            SOi1 = SO1[i1];
                            ref TextureTransform TT = ref D.Object[SOi0].TT[SOi1];
                            IO.Write   (nameView + SOi1 + ".name=" , TT.Name);
                            TT.C .Write(nameView + SOi1 + d, A3DC  , "coverage"      );
                            TT.O .Write(nameView + SOi1 + d, A3DC  , "offset"        );
                            TT.R .Write(nameView + SOi1 + d, A3DC  , "repeat"        );
                            TT.Ro.Write(nameView + SOi1 + d, A3DC  , "rotate"        );
                            TT.RF.Write(nameView + SOi1 + d, A3DC  , "rotateFrame"   );
                            TT.TF.Write(nameView + SOi1 + d, A3DC  , "translateFrame");
                        }
                        IO.Write(nameView + "length=" + D.Object[SOi0].TT.Length + "\n");
                    }

                    D.Object[SOi0].MT.Write(name, 0b10011, A3DC);
                }
                IO.Write("object.length=", D.Object.Length.ToString());
            }

            if (D.ObjectList.Length != 0)
            {
                SO0 = SortWriter(D.ObjectList.Length);
                for (int i0 = 0; i0 < D.ObjectList.Length; i0++)
                    IO.Write("object_list." + SO0[i0] + "=",
                        D.ObjectList[SO0[i0]].ToString());
                IO.Write("object_list.length=", D.ObjectList.Length.ToString());
            }

            if (D.ObjectHRC.Length != 0)
            {
                SO0 = SortWriter(D.ObjectHRC.Length);
                SOi0 = 0;
                for (int i0 = 0; i0 < D.ObjectHRC.Length; i0++)
                {
                    SOi0 = SO0[i0];
                    IO.Write("objhrc." + SOi0 + ".name=", D.ObjectHRC[SOi0].Name);
                    if (D.ObjectHRC[SOi0].Node != null)
                    {
                        SO1 = SortWriter(D.ObjectHRC[i0].Node.Length);
                        SOi1 = 0;
                        name = "objhrc." + SOi0 + ".node.";
                        for (int i1 = 0; i1 < D.ObjectHRC[i0].Node.Length; i1++)
                        {
                            SOi1 = SO1[i1];
                            D.ObjectHRC[SOi0].Node[SOi1].MT.Write(name + SOi1 + d, 0b1000000, A3DC);
                            IO.Write(name + SOi1 + ".name=", D.ObjectHRC[SOi0].Node[SOi1].Name);
                            IO.Write(name + SOi1 + ".parent=", D.ObjectHRC[SOi0].Node[SOi1].Parent.ToString());
                            D.ObjectHRC[SOi0].Node[SOi1].MT.Write(name + SOi1 + d, 0b0001111, A3DC);
                        }
                        IO.Write(name + "length=",
                            D.ObjectHRC[SOi0].Node.Length.ToString());
                    }
                    name = "objhrc." + SOi0 + d;

                    if (D.ObjectHRC[SOi0].Shadow != 0)
                        IO.Write(name + "shadow=", D.ObjectHRC[SOi0].Shadow.ToString());
                    IO.Write(name + "uid_name=", D.ObjectHRC[SOi0].UIDName);
                }
                IO.Write("objhrc.length=", D.ObjectHRC.Length.ToString());
            }

            if (D.ObjectHRCList.Length != 0)
            {
                SO0 = SortWriter(D.ObjectHRCList.Length);
                for (int i0 = 0; i0 < D.ObjectHRCList.Length; i0++)
                    IO.Write("objhrc_list." + SO0[i0] + "=", D.ObjectHRCList[SO0[i0]]);
                IO.Write("objhrc_list.length=", D.ObjectHRCList.Length.ToString());
            }

            IO.Write("play_control.begin=" , D.PlayControl.Begin.ToString());
            IO.Write("play_control.fps="   , D.PlayControl.FPS.ToString());
            if (D._.CompressF16 != 0)
                IO.Write("play_control.offset=", D.PlayControl.Offset.ToString());
            IO.Write("play_control.size="  , (D.PlayControl.Size - D.PlayControl.Offset).ToString());

            if (D.PostProcess.Boolean)
            {
                name = "post_process.";
                D.PostProcess.Ambient  .Write(name, A3DC, "Ambient"   );
                D.PostProcess.Diffuse  .Write(name, A3DC, "Diffuse"   );
                D.PostProcess.Specular .Write(name, A3DC, "Specular"  );
                D.PostProcess.LensFlare.Write(name, A3DC, "lens_flare");
                D.PostProcess.LensGhost.Write(name, A3DC, "lens_ghost");
                D.PostProcess.LensShaft.Write(name, A3DC, "lens_shaft");
            }

            SO0 = SortWriter(D.Point.Length);
            if (D.Point.Length != 0)
            {
                for (int i0 = 0; i0 < D.Point.Length; i0++)
                    D.Point[SO0[i0]].Write("point." + SO0[i0] + d, 0b1111, A3DC);
                IO.Write("point.length=", D.Point.Length.ToString());
            }

            if (!A3DC)
                IO.Close();
        }

        public void A3DCWriter(string file)
        {
            if (A3DCOpt)
                UsedValues = new Values { BinOffset = new List<int>(), Value = new List<double>() };
            DateTime date = DateTime.Now;

            ref A3DAData D = ref Data;
            if (D._.CompressF16 != 0)
            {
                int a = int.Parse(D._.ConverterVersion);
                int b = BitConverter.ToInt32(KKtText.ToUTF8(D._.FileName), 0);
                int c = int.Parse(D._.PropertyVersion);
                int d = (int)((long)a * b * c);

                IO.Write(KKtText.ToASCII("A3DA"));
                IO.Write(0x00);
                IO.Write(0x40);
                IO.Write(0x10000000);
                IO.Write((long)0x00);
                IO.Write((long)0x00);
                IO.Write(d);
                IO.Write(0x00);
                IO.Write((long)0x00);
                IO.Write(0x01131010);
                IO.Write(0x00);
                IO.Write(0x00);
                IO.Write(0x00);
            }

            int A3DCStart = (int)IO.Position;
            IO.Seek(0x40, SeekOrigin.Current);

            Offset = (int)IO.Position;

            IO.Close();
            IO = new KKtIO(new MemoryStream());

            for (byte i = 0; i < 2; i++)
            {
                bool ReturnToOffset = i == 1;
                if (ReturnToOffset)
                {
                    if (D.Camera.Auxiliary.Boolean)
                    {
                        D.Camera.Auxiliary.AutoExposure.Write();
                        D.Camera.Auxiliary.    Exposure.Write();
                        D.Camera.Auxiliary.Gamma       .Write();
                        D.Camera.Auxiliary.GammaRate   .Write();
                        D.Camera.Auxiliary.Saturate    .Write();
                    }

                    for (int i0 = 0; i0 < D.Camera.Root.Length; i0++)
                    {
                        D.Camera.Root[i0]          .MT         .Write();
                        D.Camera.Root[i0].ViewPoint.MT         .Write();
                        D.Camera.Root[i0].Interest             .Write();
                        D.Camera.Root[i0].ViewPoint.FOV        .Write();
                        D.Camera.Root[i0].ViewPoint.Roll       .Write();
                        D.Camera.Root[i0].ViewPoint.FocalLength.Write();
                    }

                    for (int i0 = 0; i0 < D.Chara.Length; i0++)
                        D.Chara[i0].Write();

                    for (int i0 = 0; i0 < D.Curve.Length; i0++)
                        D.Curve[i0].CV.Write();

                    if (D.DOF.Name != null)
                        D.DOF.Write();

                    for (int i0 = 0; i0 < D.Fog.Length; i0++)
                    {
                        D.Fog[i0].Density.Write();
                        D.Fog[i0].Diffuse.Write();
                        D.Fog[i0].End    .Write();
                        D.Fog[i0].Start  .Write();
                    }

                    for (int i0 = 0; i0 < D.Light.Length; i0++)
                    {
                        D.Light[i0].Ambient      .Write();
                        D.Light[i0].Diffuse      .Write();
                        D.Light[i0].Incandescence.Write();
                        D.Light[i0].Specular     .Write();
                        D.Light[i0].Position     .Write();
                        D.Light[i0].SpotDirection.Write();
                    }

                    for (int i0 = 0; i0 < D.MObjectHRC.Length; i0++)
                    {
                        if (D.MObjectHRC[i0].Instance != null)
                            for (int i1 = 0; i1 < D.MObjectHRC[i0].Instance.Length; i1++)
                                D.MObjectHRC[i0].Instance[i1].Write();
                        if (D.MObjectHRC[i0].Node != null)
                            for (int i1 = 0; i1 < D.MObjectHRC[i0].Node.Length; i1++)
                                D.MObjectHRC[i0].Node[i1].MT.Write();
                    }

                    for (int i0 = 0; i0 < D.Object.Length; i0++)
                    {
                        D.Object[i0].MT.Write();
                        if (D.Object[i0].TT != null)
                            for (int i1 = 0; i1 < D.Object[i0].TT.Length; i1++)
                            {
                                D.Object[i0].TT[i1].C .Write();
                                D.Object[i0].TT[i1].O .Write();
                                D.Object[i0].TT[i1].R .Write();
                                D.Object[i0].TT[i1].Ro.Write();
                                D.Object[i0].TT[i1].RF.Write();
                                D.Object[i0].TT[i1].TF.Write();
                            }
                    }

                    for (int i0 = 0; i0 < D.ObjectHRC.Length; i0++)
                        if (D.ObjectHRC[i0].Node != null)
                            for (int i1 = 0; i1 < D.ObjectHRC[i0].Node.Length; i1++)
                                D.ObjectHRC[i0].Node[i1].MT.Write();

                    for (int i0 = 0; i0 < D.Point.Length; i0++)
                        D.Point[i0].Write();

                    if (D.PostProcess.Boolean)
                    {
                        D.PostProcess.Ambient  .Write();
                        D.PostProcess.Diffuse  .Write();
                        D.PostProcess.Specular .Write();
                        D.PostProcess.LensFlare.Write();
                        D.PostProcess.LensGhost.Write();
                        D.PostProcess.LensShaft.Write();
                    }
                }
                IO.Seek(0x00, 0);

                for (int i0 = 0; i0 < D.Camera.Root.Length; i0++)
                {
                    D.Camera.Root[i0].Interest    .WriteOffset(ReturnToOffset);
                    D.Camera.Root[i0].MT          .WriteOffset(ReturnToOffset);
                    D.Camera.Root[i0].ViewPoint.MT.WriteOffset(ReturnToOffset);
                }

                if (D.DOF.Name != null)
                    D.DOF.WriteOffset(ReturnToOffset);

                for (int i0 = 0; i0 < D.Light.Length; i0++)
                {
                    D.Light[i0].     Position.WriteOffset(ReturnToOffset);
                    D.Light[i0].SpotDirection.WriteOffset(ReturnToOffset);
                }

                for (int i0 = 0; i0 < D.MObjectHRC.Length; i0++)
                    if (D.MObjectHRC[i0].Node != null)
                        for (int i1 = 0; i1 < D.MObjectHRC[i0].Node.Length; i1++)
                            D.MObjectHRC[i0].Node[i1].MT.WriteOffset(ReturnToOffset);

                for (int i0 = 0; i0 < D.Object.Length; i0++)
                    D.Object[i0].MT.WriteOffset(ReturnToOffset);

                for (int i0 = 0; i0 < D.ObjectHRC.Length; i0++)
                    if (D.ObjectHRC[i0].Node != null)
                        for (int i1 = 0; i1 < D.ObjectHRC[i0].Node.Length; i1++)
                            D.ObjectHRC[i0].Node[i1].MT.WriteOffset(ReturnToOffset);

                if (!ReturnToOffset)
                    IO.Align(0x10, true);
            }

            byte[] A3DC = IO.ToArray(true);

            IO = new KKtIO(new MemoryStream());
            A3DAWriter(file, true);
            byte[] A3DA = IO.ToArray(true);

            IO = KKtIO.OpenWriter(file + ".a3da");
            IO.Seek(Offset, 0);

            int A3DAStart = (int)IO.Position;
            IO.Write(A3DA);
            int A3DAEnd = (int)IO.Position;
            IO.Align(0x20, true);

            int BinaryStart = (int)IO.Position;
            IO.Write(A3DC);
            int BinaryEnd = (int)IO.Position;
            IO.Align(0x10, true);

            int A3DCEnd = (int)IO.Position;

            if (D._.CompressF16 != 0)
            {
                IO.Align(0x10);
                A3DCEnd = (int)IO.Position;
                IO.WriteEOFC(0);
                IO.Seek(0x04, 0);
                IO.Write(A3DCEnd - A3DCStart);
                IO.Seek(0x14, 0);
                IO.Write(A3DCEnd - A3DCStart);
            }

            IO.Seek(A3DCStart, 0);
            IO.Write("#A3D", "C__________");
            IO.Write(0x2000);
            IO.Write(0x00);
            IO.Write(0x20, true, true);
            IO.Write(0x10000200);
            IO.Write(0x50);
            IO.Write(          A3DAStart - A3DCStart, true, true);
            IO.Write(A3DAEnd - A3DAStart            , true, true);
            IO.Write(0x01, true, true);
            IO.Write(0x4C42);
            IO.Write(            BinaryStart - A3DCStart, true, true);
            IO.Write(BinaryEnd - BinaryStart            , true, true);
            IO.Write(0x20, true, true);
            if (D._.CompressF16 != 0)
            {
                IO.Seek(A3DCEnd, 0);
                IO.WriteEOFC(0);
            }

            IO.Close();
        }

        void A3DAReader()
        {
            ref A3DAData D = ref Data;
            if (KKtMain.FindValue(Dict, "_.compress_f16", ref value))
                D._.CompressF16 = int.Parse(value);
            if (KKtMain.FindValue(Dict, "_.converter.version", ref value))
                D._.ConverterVersion = value;
            if (KKtMain.FindValue(Dict, "_.file_name", ref value))
                D._.FileName = value;
            if (KKtMain.FindValue(Dict, "_.property.version", ref value))
                D._.PropertyVersion = value;
            if (KKtMain.FindValue(Dict, "camera_root.length", ref value))
                D.Camera.Root = new CameraRoot[int.Parse(value)];
            if (KKtMain.FindValue(Dict, "chara.length", ref value))
                D.Chara = new ModelTransform[int.Parse(value)];
            if (KKtMain.FindValue(Dict, "curve.length", ref value))
                D.Curve = new Curve[int.Parse(value)];
            if (KKtMain.FindValue(Dict, "dof.name", ref value))
                D.DOF.Name = value;
            if (KKtMain.FindValue(Dict, "event.length", ref value))
                D.Event = new Event[int.Parse(value)];
            if (KKtMain.FindValue(Dict, "fog.length", ref value))
                D.Fog = new Fog[int.Parse(value)];
            if (KKtMain.FindValue(Dict, "light.length", ref value))
                D.Light = new Light[int.Parse(value)];
            if (KKtMain.FindValue(Dict, "m_objhrc.length", ref value))
                D.MObjectHRC = new MObjectHRC[int.Parse(value)];
            if (KKtMain.FindValue(Dict, "m_objhrc_list.length", ref value))
                D.MObjectHRCList = new string[int.Parse(value)];
            if (KKtMain.FindValue(Dict, "motion.length", ref value))
                D.Motion = new string[int.Parse(value)];
            if (KKtMain.FindValue(Dict, "object.length", ref value))
                D.Object = new Object[int.Parse(value)];
            if (KKtMain.FindValue(Dict, "objhrc.length", ref value))
                D.ObjectHRC = new ObjectHRC[int.Parse(value)];
            if (KKtMain.FindValue(Dict, "object_list.length", ref value))
                D.ObjectList = new string[int.Parse(value)];
            if (KKtMain.FindValue(Dict, "objhrc_list.length", ref value))
                D.ObjectHRCList = new string[int.Parse(value)];
            if (KKtMain.FindValue(Dict, "play_control.begin", ref value))
                D.PlayControl.Begin = int.Parse(value);
            if (KKtMain.FindValue(Dict, "play_control.fps", ref value))
                D.PlayControl.FPS = KKtMain.ToDouble(value);
            if (KKtMain.FindValue(Dict, "play_control.offset", ref value))
                D.PlayControl.Offset = int.Parse(value);
            if (KKtMain.FindValue(Dict, "play_control.size", ref value))
                D.PlayControl.Size = int.Parse(value);
            if (KKtMain.FindValue(Dict, "point.length", ref value))
                D.Point = new ModelTransform[int.Parse(value)];
            if (KKtMain.StartsWith(Dict, "camera_auxiliary") && !D.Camera.Auxiliary.Boolean)
                D.Camera.Auxiliary.Boolean = true;
            if (KKtMain.StartsWith(Dict, "post_process") && !D.PostProcess.Boolean)
                D.PostProcess.Boolean = true;

            if (D.Camera.Auxiliary.Boolean)
            {
                name = "camera_auxiliary.";

                D.Camera.Auxiliary.AutoExposure.Read(name + "auto_exposure.", 0x3002);
                D.Camera.Auxiliary.    Exposure.Read(name +      "exposure.", 0x3002);
                D.Camera.Auxiliary.Gamma       .Read(name + "gamma."        , 0x3002);
                D.Camera.Auxiliary.GammaRate   .Read(name + "gamma_rate."   , 0x3002);
                D.Camera.Auxiliary.Saturate    .Read(name + "saturate."     , 0x3002);
            }

            for (int i0 = 0; i0 < D.Camera.Root.Length; i0++)
            {
                name = "camera_root." + i0 + d;
                nameInt = name + "interest.";
                nameView = name + "view_point.";

                ref CameraRoot CameraRoot = ref D.Camera.Root[i0];

                CameraRoot = new CameraRoot { Interest = new ModelTransform(),
                    MT = new ModelTransform(), ViewPoint = new ViewPoint {
                        Aspect = -1, CameraApertureH = -1, CameraApertureW = -1,
                        FocalLength = new Key(), FOV = new Key(), FOVHorizontal = -1,
                        MT = new ModelTransform(), Roll = new Key() } };

                ref ViewPoint ViewPoint = ref CameraRoot.ViewPoint;
                
                KKtMain.FindValue(Dict, name     + MTBO                , ref CameraRoot          .MT.BinOffset);
                KKtMain.FindValue(Dict, nameInt  + MTBO                , ref CameraRoot.Interest    .BinOffset);
                KKtMain.FindValue(Dict, nameView + MTBO                , ref CameraRoot.ViewPoint.MT.BinOffset);
                KKtMain.FindValue(Dict, nameView + "aspect"            , ref ViewPoint.Aspect                 );
                KKtMain.FindValue(Dict, nameView + "camera_aperture_h" , ref ViewPoint.CameraApertureH        );
                KKtMain.FindValue(Dict, nameView + "camera_aperture_w" , ref ViewPoint.CameraApertureW        );
                KKtMain.FindValue(Dict, nameView + "focal_length." + BO, ref ViewPoint.FocalLength  .BinOffset);
                KKtMain.FindValue(Dict, nameView + "fov."          + BO, ref ViewPoint.FOV          .BinOffset);
                KKtMain.FindValue(Dict, nameView + "fov_is_horizontal" , ref ViewPoint.FOVHorizontal          );
                KKtMain.FindValue(Dict, nameView + "roll."         + BO, ref ViewPoint.Roll         .BinOffset);

                CameraRoot          .MT         .Read(name                      , 0x3002);
                CameraRoot.Interest             .Read(nameInt                   , 0x3002);
                CameraRoot.ViewPoint.MT         .Read(nameView                  , 0x3002);
                CameraRoot.ViewPoint.FocalLength.Read(nameView + "focal_length.", 0x3002);
                CameraRoot.ViewPoint.FOV        .Read(nameView + "fov."         , 0x3002);
                CameraRoot.ViewPoint.Roll       .Read(nameView + "roll."        , 0x3002);
            }

            for (int i0 = 0; i0 < D.Chara.Length; i0++)
            {
                D.Chara[i0] = new ModelTransform();
                name = "chara." + i0 + d;

                KKtMain.FindValue(Dict, name + MTBO, ref D.Chara[i0].BinOffset);

                D.Chara[i0].Read(name, 0x3002);
            }

            for (int i0 = 0; i0 < D.Curve.Length; i0++)
            {
                D.Curve[i0] = new Curve();
                name = "curve." + i0 + d;

                KKtMain.FindValue(Dict, name + "cv." + BO, ref D.Curve[i0].CV.BinOffset);
                KKtMain.FindValue(Dict, name + "name", ref D.Curve[i0].Name);

                D.Curve[i0].CV.Read(name + "cv.", 0x3002);
            }

            if (D.DOF.Name != null)
            {
                D.DOF = new ModelTransform();
                name = "dof.";

                KKtMain.FindValue(Dict, name + MTBO, ref D.DOF.BinOffset);

                D.DOF.Read(name, 0x3002);
            }

            for (int i0 = 0; i0 < D.Fog.Length; i0++)
            {
                D.Fog[i0] = new Fog();
                name = "fog." + i0 + d;

                KKtMain.FindValue(Dict, name + "id", ref D.Fog[i0].Id);
                KKtMain.FindValue(Dict, name + "density." + BO, ref D.Fog[i0].Density.BinOffset);
                KKtMain.FindValue(Dict, name + "end." + BO, ref D.Fog[i0].End.BinOffset);
                KKtMain.FindValue(Dict, name + "start." + BO, ref D.Fog[i0].Start.BinOffset);
                
                D.Fog[i0].Density.Read(name + "density.", 0x3002);
                D.Fog[i0].Diffuse.Read(name + "Diffuse.", 0x3002);
                D.Fog[i0].End    .Read(name + "end."    , 0x3002);
                D.Fog[i0].Start  .Read(name + "start."  , 0x3002);
            }

            for (int i0 = 0; i0 < D.Light.Length; i0++)
            {
                D.Light[i0] = new Light();
                name = "light." + i0 + d;

                KKtMain.FindValue(Dict, name + "id", ref D.Light[i0].Id);
                KKtMain.FindValue(Dict, name + "name", ref D.Light[i0].Name);
                KKtMain.FindValue(Dict, name + "position." + MTBO,
                    ref D.Light[i0].Position.BinOffset);
                KKtMain.FindValue(Dict, name + "spot_direction." + MTBO,
                    ref D.Light[i0].SpotDirection.BinOffset);
                KKtMain.FindValue(Dict, name + "type", ref D.Light[i0].Type);

                D.Light[i0].Ambient      .Read(name + "Ambient."       , 0x3002);
                D.Light[i0].Diffuse      .Read(name + "Diffuse."       , 0x3002);
                D.Light[i0].Incandescence.Read(name + "Incandescence." , 0x3002);
                D.Light[i0].Specular     .Read(name + "Specular."      , 0x3002);
                D.Light[i0].Position     .Read(name + "position."      , 0x3002);
                D.Light[i0].SpotDirection.Read(name + "spot_direction.", 0x3002);
            }

            for (int i0 = 0; i0 < D.Event.Length; i0++)
            {
                name = "event." + i0 + d;

                KKtMain.FindValue(Dict, name + "begin", ref D.Event[i0].Begin);
                KKtMain.FindValue(Dict, name + "clip_begin", ref D.Event[i0].ClipBegin);
                KKtMain.FindValue(Dict, name + "clip_en", ref D.Event[i0].ClipEnd);
                KKtMain.FindValue(Dict, name + "end", ref D.Event[i0].End);
                KKtMain.FindValue(Dict, name + "name", ref D.Event[i0].Name);
                KKtMain.FindValue(Dict, name + "param1", ref D.Event[i0].Param1);
                KKtMain.FindValue(Dict, name + "ref", ref D.Event[i0].Ref);
                KKtMain.FindValue(Dict, name + "time_ref_scale", ref D.Event[i0].TimeRefScale);
                KKtMain.FindValue(Dict, name + "type", ref D.Event[i0].Type);
            }

            for (int i0 = 0; i0 < D.MObjectHRC.Length; i0++)
            {
                D.MObjectHRC[i0] = new MObjectHRC();
                name = "m_objhrc." + i0 + d;

                if (KKtMain.FindValue(Dict, name + "instance.length", ref value))
                    D.MObjectHRC[i0].Instance = new ModelTransform[int.Parse(value)];
                if (KKtMain.FindValue(Dict, name + "node.length", ref value))
                    D.MObjectHRC[i0].Node = new Node[int.Parse(value)];
                KKtMain.FindValue(Dict, name + "name", ref D.MObjectHRC[i0].Name);

                if (D.MObjectHRC[i0].Instance != null)
                    for (int i1 = 0; i1 < D.MObjectHRC[i0].Instance.Length; i1++)
                    {
                        name = "m_objhrc." + i0 + ".instance." + i1 + d;
                        D.MObjectHRC[i0].Instance[i1] = new ModelTransform();
                        D.MObjectHRC[i0].Instance[i1].Read(name, 0x3002);
                    }

                if (D.MObjectHRC[i0].Node != null)
                    for (int i1 = 0; i1 < D.MObjectHRC[i0].Node.Length; i1++)
                    {
                        D.MObjectHRC[i0].Node[i1] = new Node();
                        name = "m_objhrc." + i0 + ".node." + i1 + d;
                        D.MObjectHRC[i0].Node[i1].MT = new ModelTransform();
                        KKtMain.FindValue(Dict, name + "parent", ref D.MObjectHRC[i0].Node[i1].Parent);

                        D.MObjectHRC[i0].Node[i1].MT.Read(name, 0x3002);
                    }
            }

            for (int i0 = 0; i0 < D.MObjectHRCList.Length; i0++)
            {
                D.MObjectHRCList[i0] = "";
                name = "m_objhrc_list." + i0;

                KKtMain.FindValue(Dict, name, ref D.MObjectHRCList[i0]);
            }

            for (int i0 = 0; i0 < D.Motion.Length; i0++)
            {
                name = "motion." + i0 + ".name";

                KKtMain.FindValue(Dict, name, ref D.Motion[i0]);
            }

            for (int i0 = 0; i0 < D.Object.Length; i0++)
            {
                D.Object[i0] = new Object();
                name = "object." + i0 + d;

                KKtMain.FindValue(Dict, name + MTBO, ref D.Object[i0].MT.BinOffset);
                KKtMain.FindValue(Dict, name + "morph", ref D.Object[i0].Morph);
                KKtMain.FindValue(Dict, name + "morph_offset", ref D.Object[i0].MorphOffset);
                KKtMain.FindValue(Dict, name + "parent_name", ref D.Object[i0].ParentName);

                if (KKtMain.FindValue(Dict, name + "tex_pat.length", ref value))
                    D.Object[i0].TP = new TexturePattern[int.Parse(value)];
                if (KKtMain.FindValue(Dict, name + "tex_transform.length", ref value))
                    D.Object[i0].TT = new TextureTransform[int.Parse(value)];

                if (D.Object[i0].TP != null)
                    for (int i1 = 0; i1 < D.Object[i0].TP.Length; i1++)
                    {
                        D.Object[i0].TP[i1] = new TexturePattern();
                        name = "object." + i0 + ".tex_pat." + i1 + d;
                        KKtMain.FindValue(Dict, name + "name", ref D.Object[i0].TP[i1].Name);
                        KKtMain.FindValue(Dict, name + "pat", ref D.Object[i0].TP[i1].Pat);
                        KKtMain.FindValue(Dict, name + "pat_offset", ref D.Object[i0].TP[i1].PatOffset);
                    }

                if (D.Object[i0].TT != null)
                    for (int i1 = 0; i1 < D.Object[i0].TT.Length; i1++)
                    {
                        ref TextureTransform TT = ref D.Object[i0].TT[i1];
                        TT = new TextureTransform();
                        name = "object." + i0 + ".tex_transform." + i1 + d;
                        KKtMain.FindValue(Dict, name + "name", ref TT.Name);

                        TT.C. Read(name + "coverage"      , 0x3002);
                        TT.O. Read(name + "offset"        , 0x3002);
                        TT.R. Read(name + "repeat"        , 0x3002);
                        TT.Ro.Read(name + "rotate."       , 0x3002);
                        TT.RF.Read(name + "rotateFrame."  , 0x3002);
                        TT.TF.Read(name + "translateFrame", 0x3002);
                    }

                name = "object." + i0 + d;
                D.Object[i0].MT.Read(name, 0x3002);
            }


            for (int i0 = 0; i0 < D.ObjectHRC.Length; i0++)
            {
                D.ObjectHRC[i0] = new ObjectHRC();
                name = "objhrc." + i0 + d;

                KKtMain.FindValue(Dict, name + "shadow"  , ref D.ObjectHRC[i0].Shadow );
                KKtMain.FindValue(Dict, name + "name"    , ref D.ObjectHRC[i0].Name   );
                KKtMain.FindValue(Dict, name + "uid_name", ref D.ObjectHRC[i0].UIDName);
                if (KKtMain.FindValue(Dict, name + "node.length", ref value))
                    D.ObjectHRC[i0].Node = new Node[int.Parse(value)];

                if (D.ObjectHRC[i0].Node != null)
                    for (int i1 = 0; i1 < D.ObjectHRC[i0].Node.Length; i1++)
                    {
                        D.ObjectHRC[i0].Node[i1] = new Node();
                        name = "objhrc." + i0 + ".node." + i1 + d;

                        KKtMain.FindValue(Dict, name + "parent", ref D.ObjectHRC[i0].Node[i1].Parent);

                        D.ObjectHRC[i0].Node[i1].MT.Read(name, 0x3002);
                    }
            }


            for (int i0 = 0; i0 < D.ObjectHRCList.Length; i0++)
            {
                D.ObjectHRCList[i0] = "";
                name = "objhrc_list." + i0;

                KKtMain.FindValue(Dict, name, ref D.ObjectHRCList[i0]);
            }


            for (int i0 = 0; i0 < D.ObjectList.Length; i0++)
            {
                D.ObjectList[i0] = "";
                name = "object_list." + i0;

                KKtMain.FindValue(Dict, name, ref D.ObjectList[i0]);
            }

            for (int i0 = 0; i0 < D.Point.Length; i0++)
            {
                D.Point[i0] = new ModelTransform();
                name = "point." + i0 + d;

                KKtMain.FindValue(Dict, name + MTBO, ref D.Point[i0].BinOffset);
                D.Point[i0].Read(name, 0x3002);
            }

            if (D.PostProcess.Boolean)
            {
                name = "post_process.";
                D.PostProcess = new PostProcess();

                D.PostProcess.Ambient  .Read(name + "Ambient."   , 0x3002);
                D.PostProcess.Diffuse  .Read(name + "Diffuse."   , 0x3002);
                D.PostProcess.Specular .Read(name + "Specular."  , 0x3002);
                D.PostProcess.LensFlare.Read(name + "lens_flare.", 0x3002);
                D.PostProcess.LensGhost.Read(name + "lens_ghost.", 0x3002);
                D.PostProcess.LensShaft.Read(name + "lens_shaft.", 0x3002);
            }
        }

        void A3DCReader()
        {
            if (Data.Camera.Auxiliary.Boolean)
            {
                Data.Camera.Auxiliary.AutoExposure.Read();
                Data.Camera.Auxiliary.    Exposure.Read();
                Data.Camera.Auxiliary.Gamma       .Read();
                Data.Camera.Auxiliary.GammaRate   .Read();
                Data.Camera.Auxiliary.Saturate    .Read();
            }

            for (int i0 = 0; i0 < Data.Camera.Root.Length; i0++)
            {
                Data.Camera.Root[i0]          .MT         .Read();
                Data.Camera.Root[i0].Interest             .Read();
                Data.Camera.Root[i0].ViewPoint.MT         .Read();
                Data.Camera.Root[i0].ViewPoint.FocalLength.Read();
                Data.Camera.Root[i0].ViewPoint.FOV        .Read();
                Data.Camera.Root[i0].ViewPoint.Roll       .Read();
            }

            for (int i0 = 0; i0 < Data.Chara.Length; i0++)
                Data.Chara[i0].Read();

            for (int i0 = 0; i0 < Data.Curve.Length; i0++)
                Data.Curve[i0].CV.Read();

            if (Data.DOF.Name != null)
                Data.DOF.Read();

            for (int i0 = 0; i0 < Data.Fog.Length; i0++)
            {
                Data.Fog[i0].Density.Read();
                Data.Fog[i0].Diffuse.Read();
                Data.Fog[i0].End    .Read();
                Data.Fog[i0].Start  .Read();
            }

            for (int i0 = 0; i0 < Data.Light.Length; i0++)
            {
                Data.Light[i0].Ambient      .Read();
                Data.Light[i0].Diffuse      .Read();
                Data.Light[i0].Incandescence.Read();
                Data.Light[i0].Specular     .Read();
                Data.Light[i0].Position     .Read();
                Data.Light[i0].SpotDirection.Read();
            }

            for (int i0 = 0; i0 < Data.MObjectHRC.Length; i0++)
                if (Data.MObjectHRC[i0].Node != null)
                    for (int i1 = 0; i1 < Data.MObjectHRC[i0].Node.Length; i1++)
                        Data.MObjectHRC[i0].Node[i1].MT.Read();

            for (int i0 = 0; i0 < Data.Object.Length; i0++)
            {
                Data.Object[i0].MT.Read();
                if (Data.Object[i0].TT != null)
                    for (int i1 = 0; i1 < Data.Object[i0].TT.Length; i1++)
                    {
                        Data.Object[i0].TT[i1].C .Read();
                        Data.Object[i0].TT[i1].O .Read();
                        Data.Object[i0].TT[i1].R .Read();
                        Data.Object[i0].TT[i1].Ro.Read();
                        Data.Object[i0].TT[i1].RF.Read();
                        Data.Object[i0].TT[i1].TF.Read();
                    }
            }

            for (int i0 = 0; i0 < Data.ObjectHRC.Length; i0++)
                if (Data.ObjectHRC[i0].Node != null)
                    for (int i1 = 0; i1 < Data.ObjectHRC[i0].Node.Length; i1++)
                        Data.ObjectHRC[i0].Node[i1].MT.Read();


            for (int i0 = 0; i0 < Data.Point.Length; i0++)
                Data.Point[i0].Read();

            if (Data.PostProcess.Boolean)
            {
                Data.PostProcess.Ambient  .Read();
                Data.PostProcess.Diffuse  .Read();
                Data.PostProcess.Specular .Read();
                Data.PostProcess.LensFlare.Read();
                Data.PostProcess.LensGhost.Read();
                Data.PostProcess.LensShaft.Read();
            }
        }

        public void XMLReader(string file)
        {
            name = "";
            Offset = 0;
            nameInt = "";
            nameView = "";
            dataArray = new string[4];
            Dict = new Dictionary<string, object>();

            ref A3DAData D = ref Data;
            D = new A3DAData();

            KKtXml Xml = new KKtXml();
            Xml.OpenXml(file + ".xml", true);
            foreach (XElement A3D in Xml.doc.Elements("A3D"))
            {
                foreach (XAttribute Entry in A3D.Attributes())
                    if (Entry.Name == "Base64")
                        Base64 = bool.Parse(Entry.Value);
                    else if (Entry.Name == "Signature")
                        D.Header.Signature = BitConverter.ToInt32(KKtText.ToASCII(Entry.Value), 0);

                foreach (XElement Child0 in A3D.Elements())
                {
                    if (Child0.Name == "_")
                    {
                        foreach (XAttribute Entry in Child0.Attributes())
                        {
                            if (D.Header.Signature == 0x5F5F5F43)
                                Xml.Reader(Entry, ref D._.CompressF16, "CompressF16");
                            Xml.Reader(Entry, ref D._.ConverterVersion, "ConverterVersion");
                            Xml.Reader(Entry, ref D._.FileName, "FileName");
                            Xml.Reader(Entry, ref D._.PropertyVersion, "PropertyVersion");
                        }
                    }
                    else if (Child0.Name == "Camera")
                    {
                        foreach (XElement Camera in Child0.Elements())
                        {
                            if (Camera.Name == "Auxiliary")
                            {
                                D.Camera.Auxiliary.Boolean = true;
                                foreach (XElement Auxiliary in Camera.Elements())
                                {
                                    D.Camera.Auxiliary.AutoExposure.Read(ref Xml, Auxiliary, "AutoExposure");
                                    D.Camera.Auxiliary.    Exposure.Read(ref Xml, Auxiliary,     "Exposure");
                                    D.Camera.Auxiliary.Gamma       .Read(ref Xml, Auxiliary, "Gamma"       );
                                    D.Camera.Auxiliary.GammaRate   .Read(ref Xml, Auxiliary, "GammaRate"   );
                                    D.Camera.Auxiliary.Saturate    .Read(ref Xml, Auxiliary, "Saturate"    );
                                }
                            }
                            else if (Camera.Name == "Root")
                            {
                                foreach (XAttribute Entry in Camera.Attributes())
                                    if (Entry.Name == "Length")
                                        D.Camera.Root = new CameraRoot[int.Parse(Entry.Value)];

                                int i0 = 0;
                                foreach (XElement Root in Camera.Elements())
                                    if (Root.Name == "RootEntry")
                                    {
                                        D.Camera.Root[i0] = new CameraRoot();

                                        D.Camera.Root[i0].MT.Read(ref Xml, Root);
                                        foreach (XElement CameraRoot in Root.Elements())
                                        {
                                            if (CameraRoot.Name == "Interest")
                                                D.Camera.Root[i0].Interest.Read(ref Xml, CameraRoot);
                                            else if (CameraRoot.Name == "ViewPoint")
                                            {
                                                foreach (XAttribute Entry in CameraRoot.Attributes())
                                                {
                                                    Xml.Reader(Entry, ref D.Camera.Root[i0].
                                                        ViewPoint.Aspect, "Aspect");
                                                    Xml.Reader(Entry, ref D.Camera.Root[i0].
                                                        ViewPoint.CameraApertureH, "CameraApertureH");
                                                    Xml.Reader(Entry, ref D.Camera.Root[i0].
                                                        ViewPoint.CameraApertureW, "CameraApertureW");
                                                    Xml.Reader(Entry, ref D.Camera.Root[i0].
                                                        ViewPoint.FOVHorizontal, "FOVHorizontal");
                                                }

                                                foreach (XElement ViewPoint in CameraRoot.Elements())
                                                {
                                                    D.Camera.Root[i0].ViewPoint.
                                                        FocalLength.Read(ref Xml, ViewPoint, "FocalLength");
                                                    D.Camera.Root[i0].ViewPoint.
                                                        FOV        .Read(ref Xml, ViewPoint, "FOV"        );
                                                    D.Camera.Root[i0].ViewPoint.
                                                        Roll       .Read(ref Xml, ViewPoint, "Roll"       );
                                                }
                                                D.Camera.Root[i0].ViewPoint.MT.Read(ref Xml, CameraRoot);
                                            }
                                        }
                                        i0++;
                                    }
                            }
                        }
                    }
                    else if (Child0.Name == "Charas")
                    {
                        foreach (XAttribute Entry in Child0.Attributes())
                            if (Entry.Name == "Length")
                                D.Chara = new ModelTransform[int.Parse(Entry.Value)];

                        int i0 = 0;
                        foreach (XElement Charas in Child0.Elements())
                            if (D.Chara[i0].Read(ref Xml, Charas, "Chara"))
                                i0++;
                    }
                    else if (Child0.Name == "Curves")
                    {
                        foreach (XAttribute Entry in Child0.Attributes())
                            if (Entry.Name == "Length")
                                D.Curve = new Curve[int.Parse(Entry.Value)];

                        int i0 = 0;
                        foreach (XElement Curves in Child0.Elements())
                        {
                            D.Curve[i0] = new Curve();
                            if (Curves.Name == "Curve")
                            {
                                foreach (XAttribute Entry in Curves.Attributes())
                                    Xml.Reader(Entry, ref D.Curve[i0].Name, "Name");

                                foreach (XElement Curve in Curves.Elements())
                                    D.Curve[i0].CV.Read(ref Xml, Curve, "CV");
                                i0++;
                            }
                        }
                    }
                    else if (Child0.Name == "DOF")
                        D.DOF.Read(ref Xml, Child0);
                    else if (Child0.Name == "Events")
                    {
                        foreach (XAttribute Entry in Child0.Attributes())
                            if (Entry.Name == "Length")
                                D.Event = new Event[int.Parse(Entry.Value)];

                        int i0 = 0;
                        foreach (XElement Events in Child0.Elements())
                        {
                            if (Events.Name == "Event")
                            {
                                foreach (XAttribute Entry in Events.Attributes())
                                {
                                    Xml.Reader(Entry, ref D.Event[i0].Begin, "Begin");
                                    Xml.Reader(Entry, ref D.Event[i0].ClipBegin, "ClipBegin");
                                    Xml.Reader(Entry, ref D.Event[i0].ClipEnd, "ClipEnd");
                                    Xml.Reader(Entry, ref D.Event[i0].End, "End");
                                    Xml.Reader(Entry, ref D.Event[i0].Name, "Name");
                                    Xml.Reader(Entry, ref D.Event[i0].Param1, "Param1");
                                    Xml.Reader(Entry, ref D.Event[i0].Ref, "Ref");
                                    Xml.Reader(Entry, ref D.Event[i0].TimeRefScale, "TimeRefScale");
                                    Xml.Reader(Entry, ref D.Event[i0].Type, "Type");
                                }
                                i0++;
                            }
                        }
                    }
                    else if (Child0.Name == "Fogs")
                    {
                        foreach (XAttribute Entry in Child0.Attributes())
                            if (Entry.Name == "Length")
                                D.Fog = new Fog[int.Parse(Entry.Value)];

                        int i0 = 0;
                        foreach (XElement Fogs in Child0.Elements())
                        {
                            if (Fogs.Name == "Fog")
                            {
                                foreach (XAttribute Entry in Fogs.Attributes())
                                    Xml.Reader(Entry, ref D.Fog[i0].Id, "Id");
                                foreach (XElement Fog in Fogs.Elements())
                                {
                                    D.Fog[i0].Diffuse.Read(ref Xml, Fog, "Diffuse");
                                    D.Fog[i0].Density.Read(ref Xml, Fog, "Density");
                                    D.Fog[i0].End    .Read(ref Xml, Fog, "End"    );
                                    D.Fog[i0].Start  .Read(ref Xml, Fog, "Start"  );
                                }
                                i0++;
                            }
                        }
                    }
                    else if (Child0.Name == "Lights")
                    {
                        foreach (XAttribute Entry in Child0.Attributes())
                            if (Entry.Name == "Length")
                                D.Light = new Light[int.Parse(Entry.Value)];

                        int i0 = 0;
                        foreach (XElement Lights in Child0.Elements())
                        {
                            if (Lights.Name == "Light")
                            {
                                foreach (XAttribute Entry in Lights.Attributes())
                                {
                                    Xml.Reader(Entry, ref D.Light[i0].Id, "Id");
                                    Xml.Reader(Entry, ref D.Light[i0].Name, "Name");
                                    Xml.Reader(Entry, ref D.Light[i0].Type, "Type");
                                }
                                foreach (XElement Light in Lights.Elements())
                                {
                                    D.Light[i0].Ambient      .Read(ref Xml, Light, "Ambient"      );
                                    D.Light[i0].Diffuse      .Read(ref Xml, Light, "Diffuse"      );
                                    D.Light[i0].Incandescence.Read(ref Xml, Light, "Incandescence");
                                    D.Light[i0].Position     .Read(ref Xml, Light, "Position"     );
                                    D.Light[i0].Specular     .Read(ref Xml, Light, "Specular"     );
                                    D.Light[i0].SpotDirection.Read(ref Xml, Light, "SpotDirection");
                                }
                                i0++;
                            }
                        }
                    }
                    else if (Child0.Name == "MObjectsHRC")
                    {
                        foreach (XAttribute Entry in Child0.Attributes())
                            if (Entry.Name == "Length")
                                D.MObjectHRC = new MObjectHRC[int.Parse(Entry.Value)];

                        int i0 = 0;
                        foreach (XElement MObjectsHRC in Child0.Elements())
                        {
                            if (MObjectsHRC.Name == "MObjectHRC")
                            {
                                foreach (XAttribute Entry in MObjectsHRC.Attributes())
                                    Xml.Reader(Entry, ref D.MObjectHRC[i0].Name, "Name");
                                foreach (XElement MObjectHRC in MObjectsHRC.Elements())
                                {
                                    if (MObjectHRC.Name == "Instances")
                                    {
                                        foreach (XAttribute Entry in MObjectHRC.Attributes())
                                            if (Entry.Name == "Length")
                                                D.MObjectHRC[i0].Instance =
                                                    new ModelTransform[int.Parse(Entry.Value)];

                                        int i1 = 0;
                                        foreach (XElement Instances in MObjectHRC.Elements())
                                            if (D.MObjectHRC[i0].Instance[i1].Read(ref Xml, Instances, "Instance"))
                                                i1++;
                                    }
                                    else if (MObjectHRC.Name == "Nodes")
                                    {
                                        foreach (XAttribute Entry in MObjectHRC.Attributes())
                                            if (Entry.Name == "Length")
                                                D.MObjectHRC[i0].Node = new Node[int.Parse(Entry.Value)];

                                        int i1 = 0;
                                        foreach (XElement Nodes in MObjectHRC.Elements())
                                        {
                                            if (Nodes.Name == "Node")
                                            {
                                                foreach (XAttribute Entry in Nodes.Attributes())
                                                {
                                                    Xml.Reader(Entry, ref D.
                                                        MObjectHRC[i0].Node[i1].Name, "Name");
                                                    Xml.Reader(Entry, ref D.
                                                        MObjectHRC[i0].Node[i1].Parent, "Parent");
                                                }

                                                D.MObjectHRC[i0].Node[i1].MT.Read(ref Xml, Nodes);
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
                        foreach (XAttribute Entry in Child0.Attributes())
                            if (Entry.Name == "Length")
                                D.MObjectHRCList = new string[int.Parse(Entry.Value)];

                        int i0 = 0;
                        foreach (XElement MObjectList in Child0.Elements())
                        {
                            if (MObjectList.Name == "String")
                            {
                                foreach (XAttribute Entry in MObjectList.Attributes())
                                    Xml.Reader(Entry, ref D.MObjectHRCList[i0], "Name");
                                i0++;
                            }
                        }
                    }
                    else if (Child0.Name == "Motion")
                    {
                        foreach (XAttribute Entry in Child0.Attributes())
                            if (Entry.Name == "Length")
                                D.Motion = new string[int.Parse(Entry.Value)];

                        int i0 = 0;
                        foreach (XElement Motion in Child0.Elements())
                        {
                            if (Motion.Name == "String")
                            {
                                foreach (XAttribute Entry in Motion.Attributes())
                                    Xml.Reader(Entry, ref D.Motion[i0], "Name");
                                i0++;
                            }
                        }
                    }
                    else if (Child0.Name == "Objects")
                    {
                        foreach (XAttribute Entry in Child0.Attributes())
                            if (Entry.Name == "Length")
                                D.Object = new Object[int.Parse(Entry.Value)];

                        int i0 = 0;
                        foreach (XElement Objects in Child0.Elements())
                        {
                            D.Object[i0] = new Object();
                            if (Objects.Name == "Object")
                            {
                                foreach (XAttribute Entry in Objects.Attributes())
                                {
                                    Xml.Reader(Entry, ref D.Object[i0].Morph, "Morph");
                                    Xml.Reader(Entry, ref D.Object[i0].MorphOffset, "MorphOffset");
                                    Xml.Reader(Entry, ref D.Object[i0].ParentName, "ParentName");
                                }

                                foreach (XElement Object in Objects.Elements())
                                {
                                    if (Object.Name == "TexTransforms" || Object.Name == "TTs")
                                    {
                                        foreach (XAttribute Entry in Object.Attributes())
                                            if (Entry.Name == "Length")
                                                D.Object[i0].TT = new TextureTransform[int.Parse(Entry.Value)];

                                        int i1 = 0;
                                        foreach (XElement TexTransforms in Object.Elements())
                                            if (TexTransforms.Name == "TexTransform" || TexTransforms.Name == "TT")
                                            {
                                                ref TextureTransform TT = ref D.Object[i0].TT[i1];
                                                TT = new TextureTransform();
                                                foreach (XAttribute Entry in TexTransforms.Attributes())
                                                    Xml.Reader(Entry, ref TT.Name, "Name");

                                                foreach (XElement TexTrans in TexTransforms.Elements())
                                                {
                                                    TT.C .Read(ref Xml, TexTrans, "C" , "Coverage"      );
                                                    TT.O .Read(ref Xml, TexTrans, "O" , "Offset"        );
                                                    TT.R .Read(ref Xml, TexTrans, "R" , "Repeat"        );
                                                    TT.Ro.Read(ref Xml, TexTrans, "Ro", "Rotate"        );
                                                    TT.RF.Read(ref Xml, TexTrans, "RF", "RotateFrame"   );
                                                    TT.TF.Read(ref Xml, TexTrans, "TF", "TranslateFrame");
                                                }
                                                i1++;
                                            }
                                    }
                                    if (Object.Name == "TexPats" || Object.Name == "TPs")
                                    {
                                        foreach (XAttribute Entry in Object.Attributes())
                                            if (Entry.Name == "Length")
                                                D.Object[i0].TP = new TexturePattern[int.Parse(Entry.Value)];

                                        int i1 = 0;
                                        foreach (XElement TexPats in Object.Elements())
                                            if (TexPats.Name == "TexPat" || Object.Name == "TP")
                                            {
                                                foreach (XAttribute Entry in TexPats.Attributes())
                                                {
                                                    Xml.Reader(Entry, ref D.
                                                        Object[i0].TP[i1].Name, "Name");
                                                    Xml.Reader(Entry, ref D.
                                                        Object[i0].TP[i1].Pat, "Pat");
                                                    Xml.Reader(Entry, ref D.
                                                        Object[i0].TP[i1].PatOffset, "PatOffset");
                                                }
                                                i1++;
                                            }
                                    }
                                }
                                D.Object[i0].MT.Read(ref Xml, Objects);
                                i0++;
                            }
                        }
                    }
                    else if (Child0.Name == "ObjectsHRC")
                    {
                        foreach (XAttribute Entry in Child0.Attributes())
                            if (Entry.Name == "Length")
                                D.ObjectHRC = new ObjectHRC[int.Parse(Entry.Value)];

                        int i0 = 0;
                        foreach (XElement ObjectsHRC in Child0.Elements())
                        {
                            if (ObjectsHRC.Name == "ObjectHRC")
                            {
                                foreach (XAttribute Entry in ObjectsHRC.Attributes())
                                {
                                    Xml.Reader(Entry, ref D.ObjectHRC[i0].Name, "Name");
                                    Xml.Reader(Entry, ref D.ObjectHRC[i0].Shadow, "Shadow");
                                    Xml.Reader(Entry, ref D.ObjectHRC[i0].UIDName, "UIDName");
                                }

                                foreach (XElement ObjectHRC in ObjectsHRC.Elements())
                                {
                                    if (ObjectHRC.Name == "Nodes")
                                    {
                                        foreach (XAttribute Entry in ObjectHRC.Attributes())
                                            if (Entry.Name == "Length")
                                                D.ObjectHRC[i0].Node = new Node[int.Parse(Entry.Value)];

                                        int i1 = 0;
                                        foreach (XElement Nodes in ObjectHRC.Elements())
                                        {
                                            if (Nodes.Name == "Node")
                                            {
                                                D.ObjectHRC[i0].Node[i1] = new Node();
                                                foreach (XAttribute Entry in Nodes.Attributes())
                                                {
                                                    Xml.Reader(Entry, ref D.
                                                        ObjectHRC[i0].Node[i1].Name, "Name");
                                                    Xml.Reader(Entry, ref D.
                                                        ObjectHRC[i0].Node[i1].Parent, "Parent");
                                                }
                                                D.ObjectHRC[i0].Node[i1].MT.Read(ref Xml, Nodes);
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
                        foreach (XAttribute Entry in Child0.Attributes())
                            if (Entry.Name == "Length")
                                D.ObjectHRCList = new string[int.Parse(Entry.Value)];

                        int i0 = 0;
                        foreach (XElement ObjectList in Child0.Elements())
                        {
                            if (ObjectList.Name == "String")
                            {
                                foreach (XAttribute Entry in ObjectList.Attributes())
                                    Xml.Reader(Entry, ref D.ObjectHRCList[i0], "Name");
                                i0++;
                            }
                        }
                    }
                    else if (Child0.Name == "ObjectList")
                    {
                        foreach (XAttribute Entry in Child0.Attributes())
                            if (Entry.Name == "Length")
                                D.ObjectList = new string[int.Parse(Entry.Value)];

                        int i0 = 0;
                        foreach (XElement ObjectList in Child0.Elements())
                        {
                            if (ObjectList.Name == "String")
                            {
                                foreach (XAttribute Entry in ObjectList.Attributes())
                                    Xml.Reader(Entry, ref D.ObjectList[i0], "Name");
                                i0++;
                            }
                        }
                    }
                    else if (Child0.Name == "PlayControl")
                    {
                        D.PlayControl = new PlayControl();
                        foreach (XAttribute Entry in Child0.Attributes())
                        {
                            Xml.Reader(Entry, ref D.PlayControl.Begin, "Begin");
                            Xml.Reader(Entry, ref D.PlayControl.FPS, "FPS");
                            Xml.Reader(Entry, ref D.PlayControl.Offset, "Offset");
                            Xml.Reader(Entry, ref D.PlayControl.Size, "Size");
                        }
                    }
                    else if (Child0.Name == "Points")
                    {
                        foreach (XAttribute Entry in Child0.Attributes())
                            if (Entry.Name == "Length")
                                D.Point = new ModelTransform[int.Parse(Entry.Value)];

                        int i0 = 0;
                        foreach (XElement Points in Child0.Elements())
                            if (D.Point[i0].Read(ref Xml, Points, "Point"))
                                i0++;
                    }
                    else if (Child0.Name == "PostProcess")
                    {
                        D.PostProcess = new PostProcess { Boolean = true };
                        foreach (XElement PostProcess in Child0.Elements())
                        {
                            D.PostProcess.Ambient  .Read(ref Xml, PostProcess, "Ambient"  );
                            D.PostProcess.Diffuse  .Read(ref Xml, PostProcess, "Diffuse"  );
                            D.PostProcess.LensFlare.Read(ref Xml, PostProcess, "LensFlare");
                            D.PostProcess.LensGhost.Read(ref Xml, PostProcess, "LensGhost");
                            D.PostProcess.LensShaft.Read(ref Xml, PostProcess, "LensShaft");
                            D.PostProcess.Specular .Read(ref Xml, PostProcess, "Specular" );
                        }
                    }
                }
            }
            Xml = null;
        }

        public void XMLWriter(string file)
        {
            Base64 = true;
            ref A3DAData D = ref Data;
            KKtXml Xml = new KKtXml();
            XElement A3D = new XElement("A3D");
            Xml.Compact = true;

            if (Base64)
                Xml.Writer(A3D, KKtMain.ToTitleCase(Base64.ToString()), "Base64");
            Xml.Writer(A3D, KKtText.ToASCII(BitConverter.GetBytes(D.Header.Signature)), "Signature");

            XElement _ = new XElement("_");
            if (D.Header.Signature == 0x5F5F5F43)
                Xml.Writer(_, D._.CompressF16.ToString().ToUpper(), "CompressF16");
            Xml.Writer(_, D._.ConverterVersion, "ConverterVersion");
            Xml.Writer(_, D._.FileName, "FileName");
            Xml.Writer(_, D._.PropertyVersion, "PropertyVersion");
            A3D.Add(_);

            if (D.Camera.Auxiliary.Boolean || D.Camera.Root.Length != 0)
            {
                XElement Camera = new XElement("Camera");
                if (D.Camera.Auxiliary.Boolean)
                {
                    XElement Auxiliary = new XElement("Auxiliary");
                    D.Camera.Auxiliary.AutoExposure.Write(ref Xml, Auxiliary, "AutoExposure");
                    D.Camera.Auxiliary.    Exposure.Write(ref Xml, Auxiliary,     "Exposure");
                    D.Camera.Auxiliary.Gamma       .Write(ref Xml, Auxiliary, "Gamma"       );
                    D.Camera.Auxiliary.GammaRate   .Write(ref Xml, Auxiliary, "GammaRate"   );
                    D.Camera.Auxiliary.Saturate    .Write(ref Xml, Auxiliary, "Saturate"   );
                    Camera.Add(Auxiliary);
                }

                if (D.Camera.Root.Length != 0)
                {
                    XElement Root = new XElement("Root");
                    Xml.Writer(Root, D.Camera.Root.Length, "Length");
                    for (int i = 0; i < D.Camera.Root.Length; i++)
                    {
                        XElement RootEntry = new XElement("RootEntry");
                        D.Camera.Root[i].MT      .Write(ref Xml, RootEntry);
                        D.Camera.Root[i].Interest.Write(ref Xml, RootEntry, "Interest");

                        XElement ViewPoint = new XElement("ViewPoint");
                        if (D.Camera.Root[i].ViewPoint.Aspect != -1)
                            Xml.Writer(ViewPoint, D.Camera.Root[i].ViewPoint.Aspect         , "Aspect"         );
                        if (D.Camera.Root[i].ViewPoint.CameraApertureH != -1)
                            Xml.Writer(ViewPoint, D.Camera.Root[i].ViewPoint.CameraApertureH, "CameraApertureH");
                        if (D.Camera.Root[i].ViewPoint.CameraApertureW != -1)
                            Xml.Writer(ViewPoint, D.Camera.Root[i].ViewPoint.CameraApertureW, "CameraApertureW");
                        D.Camera.Root[i].ViewPoint.FocalLength.Write(ref Xml, ViewPoint, "FocalLength");
                        D.Camera.Root[i].ViewPoint.FOV        .Write(ref Xml, ViewPoint, "FOV"        );
                        if (D.Camera.Root[i].ViewPoint.FOVHorizontal != -1)
                            Xml.Writer(ViewPoint, D.Camera.Root[i].ViewPoint.FOVHorizontal, "FOVHorizontal");
                        D.Camera.Root[i].ViewPoint.Roll       .Write(ref Xml, ViewPoint, "Roll"       );
                        D.Camera.Root[i].ViewPoint.MT.Write(ref Xml, ViewPoint);
                        RootEntry.Add(ViewPoint);
                        Root.Add(RootEntry);
                    }
                    Camera.Add(Root);
                }
                A3D.Add(Camera);
            }

            if (D.Chara.Length != 0)
            {
                XElement Charas = new XElement("Charas");
                Xml.Writer(Charas, D.Curve.Length, "Length");
                for (int i = 0; i < D.Curve.Length; i++)
                    D.Chara[i].Write(ref Xml, Charas, "Chara");
                A3D.Add(Charas);
            }

            if (D.Curve.Length != 0)
            {
                XElement Curves = new XElement("Curves");
                Xml.Writer(Curves, D.Curve.Length, "Length");
                for (int i = 0; i < D.Curve.Length; i++)
                {
                    XElement Curve = new XElement("Curve");
                    Xml.Writer(Curve, D.Curve[i].Name, "Name");
                    D.Curve[i].CV.Write(ref Xml, Curve, "CV");
                    Curves.Add(Curve);
                }
                A3D.Add(Curves);
            }

            if (D.DOF.Name != null)
                D.DOF.Write(ref Xml, A3D, "DOF");

            if (D.Event.Length != 0)
            {
                XElement Events = new XElement("Events");
                Xml.Writer(Events, D.Event.Length, "Length");
                for (int i = 0; i < D.Event.Length; i++)
                {
                    XElement Event = new XElement("Event");
                    Xml.Writer(Event, D.Event[i].Begin, "Begin");
                    Xml.Writer(Event, D.Event[i].ClipBegin, "ClipBegin");
                    Xml.Writer(Event, D.Event[i].ClipEnd, "ClipEnd");
                    Xml.Writer(Event, D.Event[i].End, "End");
                    Xml.Writer(Event, D.Event[i].Name, "Name");
                    Xml.Writer(Event, D.Event[i].Param1, "Param1");
                    Xml.Writer(Event, D.Event[i].Ref, "Ref");
                    Xml.Writer(Event, D.Event[i].TimeRefScale, "TimeRefScale");
                    Xml.Writer(Event, D.Event[i].Type, "Type");
                    Events.Add(Event);
                }
                A3D.Add(Events);
            }

            if (D.Fog.Length != 0)
            {
                XElement Fogs = new XElement("Fogs");
                Xml.Writer(Fogs, D.Fog.Length, "Length");
                for (int i = 0; i < D.Fog.Length; i++)
                {
                    XElement Fog = new XElement("Fog");
                    D.Fog[i].Density.Write(ref Xml, Fog, "Density");
                    D.Fog[i].Diffuse.Write(ref Xml, Fog, "Diffuse");
                    D.Fog[i].End    .Write(ref Xml, Fog, "End"    );
                    Xml.Writer(Fog, D.Fog[i].Id, "Id");
                    D.Fog[i].Start  .Write(ref Xml, Fog, "Start"  );
                    Fogs.Add(Fog);
                }
                A3D.Add(Fogs);
            }

            if (D.Light.Length != 0)
            {
                XElement Lights = new XElement("Lights");
                Xml.Writer(Lights, D.Light.Length, "Length");
                for (int i = 0; i < D.Light.Length; i++)
                {
                    XElement Light = new XElement("Light");
                    D.Light[i].Ambient      .Write(ref Xml, Light, "Ambient"      );
                    D.Light[i].Diffuse      .Write(ref Xml, Light, "Diffuse"      );
                    Xml.Writer(Light, D.Light[i].Id, "Id");
                    D.Light[i].Incandescence.Write(ref Xml, Light, "Incandescence");
                    Xml.Writer(Light, D.Light[i].Name, "Name");
                    D.Light[i].Position     .Write(ref Xml, Light, "Position"     );
                    D.Light[i].Specular     .Write(ref Xml, Light, "Specular"     );
                    D.Light[i].SpotDirection.Write(ref Xml, Light, "SpotDirection");
                    Xml.Writer(Light, D.Light[i].Type, "Type");
                    Lights.Add(Light);
                }
                A3D.Add(Lights);
            }

            if (D.MObjectHRC.Length != 0)
            {
                XElement MObjectsHRC = new XElement("MObjectsHRC");
                Xml.Writer(MObjectsHRC, D.MObjectHRC.Length, "Length");
                for (int i0 = 0; i0 < D.MObjectHRC.Length; i0++)
                {
                    XElement MObjectHRC = new XElement("MObjectHRC");
                    Xml.Writer(MObjectHRC, D.MObjectHRC[i0].Name, "Name");
                    if (D.MObjectHRC[i0].Instance != null)
                    {
                        XElement Instances = new XElement("Instances");
                        Xml.Writer(Instances, D.MObjectHRC[i0].Instance.Length, "Length");
                        for (int i1 = 0; i1 < D.MObjectHRC[i0].Instance.Length; i1++)
                            D.MObjectHRC[i0].Instance[i1].Write(ref Xml, Instances, "Instance");
                        MObjectHRC.Add(Instances);
                    }

                    if (D.MObjectHRC[i0].Node != null)
                    {
                        XElement Nodes = new XElement("Nodes");
                        Xml.Writer(Nodes, D.MObjectHRC[i0].Node.Length, "Length");
                        for (int i1 = 0; i1 < D.MObjectHRC[i0].Node.Length; i1++)
                        {
                            XElement Node = new XElement("Node");
                            Xml.Writer(Node, D.MObjectHRC[i0].Node[i1].Name  , "Name"  );
                            Xml.Writer(Node, D.MObjectHRC[i0].Node[i1].Parent, "Parent");
                            D.MObjectHRC[i0].Node[i1].MT.Write(ref Xml, Node);
                            Nodes.Add(Node);
                        }
                        MObjectHRC.Add(Nodes);
                    }
                    MObjectsHRC.Add(MObjectHRC);
                }
                A3D.Add(MObjectsHRC);
            }

            if (D.MObjectHRCList.Length != 0)
            {
                XElement MObjectHRCList = new XElement("MObjectHRCList");
                Xml.Writer(MObjectHRCList, D.MObjectHRCList.Length, "Length");
                for (int i = 0; i < D.MObjectHRCList.Length; i++)
                {
                    XElement String = new XElement("String");
                    Xml.Writer(String, D.MObjectHRCList[i], "Name");
                    MObjectHRCList.Add(String);
                }
                A3D.Add(MObjectHRCList);
            }

            if (D.Motion.Length != 0)
            {
                XElement Motion = new XElement("Motion");
                Xml.Writer(Motion, D.Motion.Length, "Length");
                for (int i = 0; i < D.Motion.Length; i++)
                {
                    XElement String = new XElement("String");
                    Xml.Writer(String, D.Motion[i], "Name");
                    Motion.Add(String);
                }
                A3D.Add(Motion);
            }

            if (D.Object.Length != 0)
            {
                XElement Objects = new XElement("Objects");
                Xml.Writer(Objects, D.Object.Length, "Length");
                for (int i0 = 0; i0 < D.Object.Length; i0++)
                {
                    XElement Object = new XElement("Object");
                    if (D.Object[i0].Morph != null)
                    {
                        Xml.Writer(Object, D.Object[i0].Morph, "Morph");
                        if (D.Object[i0].MorphOffset != -1)
                            Xml.Writer(Object, D.Object[i0].MorphOffset, "MorphOffset");
                    }
                    Xml.Writer(Object, D.Object[i0].ParentName, "ParentName");
                    D.Object[i0].MT.Write(ref Xml, Object);
                    if (D.Object[i0].TP != null)
                    {
                        XElement TexPats = new XElement("TexPats");
                        Xml.Writer(TexPats, D.Object[i0].TP.Length, "Length");
                        for (int i1 = 0; i1 < D.Object[i0].TP.Length; i1++)
                        {
                            XElement TexPat = new XElement("TexPat");
                            Xml.Writer(TexPat, D.Object[i0].TP[i1].Name, "Name");
                            Xml.Writer(TexPat, D.Object[i0].TP[i1].Pat, "Pat");
                            Xml.Writer(TexPat, D.Object[i0].TP[i1].PatOffset, "PatOffset");
                            TexPats.Add(TexPat);
                        }
                        Object.Add(TexPats);
                    }
                    if (D.Object[i0].TT != null)
                    {
                        XElement TexTransforms = new XElement("TexTransforms");
                        Xml.Writer(TexTransforms, D.Object[i0].TT.Length, "Length");
                        for (int i1 = 0; i1 < D.Object[i0].TT.Length; i1++)
                        {
                            ref TextureTransform TT = ref D.Object[i0].TT[i1];
                            XElement TexTransform = new XElement("TexTransform");
                            Xml.Writer(TexTransform, TT.Name, "Name");
                            TT.C .Write(ref Xml, TexTransform, "C" );
                            TT.O .Write(ref Xml, TexTransform, "O" );
                            TT.R .Write(ref Xml, TexTransform, "R" );
                            TT.Ro.Write(ref Xml, TexTransform, "Ro");
                            TT.RF.Write(ref Xml, TexTransform, "RF");
                            TT.TF.Write(ref Xml, TexTransform, "TF");
                            TexTransforms.Add(TexTransform);
                        }
                        Object.Add(TexTransforms);
                    }
                    Objects.Add(Object);
                }
                A3D.Add(Objects);
            }

            if (D.ObjectHRC.Length != 0)
            {
                XElement ObjectsHRC = new XElement("ObjectsHRC");
                Xml.Writer(ObjectsHRC, D.ObjectHRC.Length, "Length");
                for (int i0 = 0; i0 < D.ObjectHRC.Length; i0++)
                {
                    XElement ObjectHRC = new XElement("ObjectHRC");
                    Xml.Writer(ObjectHRC, D.ObjectHRC[i0].Name, "Name");
                    if (D.ObjectHRC[i0].Shadow != 0)
                        Xml.Writer(ObjectHRC, D.ObjectHRC[i0].Shadow, "Shadow");
                    Xml.Writer(ObjectHRC, D.ObjectHRC[i0].UIDName, "UIDName");
                    if (D.ObjectHRC[i0].Node != null)
                    {
                        XElement Nodes = new XElement("Nodes");
                        Xml.Writer(Nodes, D.ObjectHRC[i0].Node.Length, "Length");
                        for (int i1 = 0; i1 < D.ObjectHRC[i0].Node.Length; i1++)
                        {
                            XElement Node = new XElement("Node");
                            Xml.Writer(Node, D.ObjectHRC[i0].Node[i1].Name, "Name");
                            Xml.Writer(Node, D.ObjectHRC[i0].Node[i1].Parent, "Parent");
                            D.ObjectHRC[i0].Node[i1].MT.Write(ref Xml, Node);
                            Nodes.Add(Node);
                        }
                        ObjectHRC.Add(Nodes);
                    }
                    ObjectsHRC.Add(ObjectHRC);
                }
                A3D.Add(ObjectsHRC);
            }

            if (D.ObjectHRCList.Length != 0)
            {
                XElement ObjectHRCList = new XElement("ObjectHRCList");
                Xml.Writer(ObjectHRCList, D.ObjectHRCList.Length, "Length");
                for (int i = 0; i < D.ObjectHRCList.Length; i++)
                {
                    XElement String = new XElement("String");
                    Xml.Writer(String, D.ObjectHRCList[i], "Name");
                    ObjectHRCList.Add(String);
                }
                A3D.Add(ObjectHRCList);
            }

            if (D.ObjectList.Length != 0)
            {
                XElement ObjectList = new XElement("ObjectList");
                Xml.Writer(ObjectList, D.ObjectList.Length, "Length");
                for (int i = 0; i < D.ObjectList.Length; i++)
                {
                    XElement String = new XElement("String");
                    Xml.Writer(String, D.ObjectList[i], "Name");
                    ObjectList.Add(String);
                }
                A3D.Add(ObjectList);
            }

            XElement PlayControl = new XElement("PlayControl");
            Xml.Writer(PlayControl, D.PlayControl.Begin, "Begin");
            Xml.Writer(PlayControl, D.PlayControl.FPS, "FPS");
            Xml.Writer(PlayControl, D.PlayControl.Offset, "Offset");
            Xml.Writer(PlayControl, D.PlayControl.Size, "Size");
            A3D.Add(PlayControl);

            if (D.Point.Length != 0)
            {
                XElement Points = new XElement("Points");
                Xml.Writer(Points, D.Curve.Length, "Length");
                for (int i = 0; i < D.Point.Length; i++)
                    D.Point[i].Write(ref Xml, Points, "Point");
                A3D.Add(Points);
            }

            if (D.PostProcess.Boolean)
            {
                XElement PostProcess = new XElement("PostProcess");
                D.PostProcess.Ambient  .Write(ref Xml, PostProcess, "Ambient"  );
                D.PostProcess.Diffuse  .Write(ref Xml, PostProcess, "Diffuse"  );
                D.PostProcess.LensFlare.Write(ref Xml, PostProcess, "LensFlare");
                D.PostProcess.LensGhost.Write(ref Xml, PostProcess, "LensGhost");
                D.PostProcess.LensShaft.Write(ref Xml, PostProcess, "LensShaft");
                D.PostProcess.Specular .Write(ref Xml, PostProcess, "Specular" );
                A3D.Add(PostProcess);
            }
            Xml.doc.Add(A3D);
            if (File.Exists(file + ".xml"))
                File.Delete(file + ".xml");
            Xml.SaveXml(file + ".xml");
        }

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

        public struct _
        {
            public int CompressF16;
            public string FileName;
            public string PropertyVersion;
            public string ConverterVersion;
        }

        public class A3DAData
        {
            public string[] Motion;
            public string[] ObjectList;
            public string[] ObjectHRCList;
            public string[] MObjectHRCList;
            public _ _;
            public Fog[] Fog;
            public Curve[] Curve;
            public Event[] Event;
            public Light[] Light;
            public Header Head;
            public Camera Camera;
            public Object[] Object;
            public ObjectHRC[] ObjectHRC;
            public MObjectHRC[] MObjectHRC;
            public PlayControl PlayControl;
            public PostProcess PostProcess;
            public KKtMain.Header Header;
            public ModelTransform DOF;
            public ModelTransform[] Chara;
            public ModelTransform[] Point;

            public A3DAData()
            {
                _ = new _();
                DOF = new ModelTransform();
                Fog = new Fog[0];
                Head = new Header();
                Chara = new ModelTransform[0];
                Curve = new Curve[0];
                Event = new Event[0];
                Light = new Light[0];
                Point = new ModelTransform[0];
                Camera = new Camera();
                Header = new KKtMain.Header();
                Motion = new string[0];
                Object = new Object[0];
                ObjectHRC = new ObjectHRC[0];
                ObjectList = new string[0];
                MObjectHRC = new MObjectHRC[0];
                PlayControl = new PlayControl();
                PostProcess = new PostProcess();
                ObjectHRCList = new string[0];
                MObjectHRCList = new string[0];
            }
        }

        public class Camera
        {
            public CameraAuxiliary Auxiliary;
            public CameraRoot[] Root;

            public Camera()
            { Auxiliary = new CameraAuxiliary(); Root = new CameraRoot[0]; }
        }

        public class CameraAuxiliary
        {
            public bool Boolean;
            public Key Gamma;
            public Key Exposure;
            public Key Saturate;
            public Key GammaRate;
            public Key AutoExposure;

            public CameraAuxiliary()
            { Boolean = false; Gamma = new Key(); Exposure = new Key();
                Saturate = new Key(); GammaRate = new Key(); AutoExposure = new Key(); }
        }

        public class CameraRoot
        {
            public ViewPoint ViewPoint;
            public ModelTransform MT;
            public ModelTransform Interest;

            public CameraRoot()
            { ViewPoint = new ViewPoint(); MT = new ModelTransform(); Interest = new ModelTransform(); }
        }

        public class Curve
        {
            public string Name;
            public Key CV;

            public Curve()
            { Name = null; CV = new Key(); }
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
        }

        public class Fog
        {
            public int Id;
            public Key End;
            public Key Start;
            public Key Density;
            public RGBAKey Diffuse;

            public Fog()
            { Id = -1; End = new Key(); Start = new Key(); Density = new Key(); Diffuse = new RGBAKey(); }
        }

        public struct Header
        {
            public int Count;
            public int BinaryLength;
            public int BinaryOffset;
            public int HeaderOffset;
            public int StringLength;
            public int StringOffset;
        }

        public class Light
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

            public Light() { Id = -1; Name = null; Type = null; Ambient = new RGBAKey();
                Diffuse = new RGBAKey(); Specular = new RGBAKey(); Incandescence = new RGBAKey();
                Position = new ModelTransform(); SpotDirection = new ModelTransform(); }
        }

        public class MObjectHRC
        {
            public string Name;
            public Node[] Node;
            public ModelTransform[] Instance;

            public MObjectHRC()
            { Name = null; Node = null; Instance = null; }
        }

        public class Node
        {
            public int Parent;
            public string Name;
            public ModelTransform MT;

            public Node()
            { Parent = -1; Name = null; MT = new ModelTransform(); }
        }

        public class Object
        {
            public int MorphOffset;
            public string Morph;
            public string ParentName;
            public ModelTransform MT;
            public TexturePattern[] TP; //TexPat
            public TextureTransform[] TT; //TexTrans

            public Object()
            { MorphOffset = -1; Morph = null; ParentName = null; MT = new ModelTransform(); TP = null; TT = null; }
        }

        public struct ObjectHRC
        {
            public double Shadow;
            public string Name;
            public string UIDName;
            public Node[] Node;
        }

        public class PlayControl
        {
            public double Begin;
            public double FPS;
            public double Offset;
            public double Size;

            public PlayControl() { Begin = -1; FPS = -1; Offset = -1; Size = -1; }
        }

        public class PostProcess
        {
            public bool Boolean;
            public Key LensFlare;
            public Key LensGhost;
            public Key LensShaft;
            public RGBAKey Ambient;
            public RGBAKey Diffuse;
            public RGBAKey Specular;

            public PostProcess()
            { Boolean = false; LensFlare = new Key(); LensGhost = new Key(); LensShaft = new Key();
                Ambient = new RGBAKey(); Diffuse = new RGBAKey(); Specular = new RGBAKey(); }
        }

        public struct TexturePattern
        {
            public int PatOffset;
            public string Pat;
            public string Name;
        }

        public class TextureTransform
        {
            public string Name;
            public KeyUV C ;  //Coverage
            public KeyUV O ;  //Offset
            public KeyUV R ;  //Repeat
            public Key   Ro; //Rotate
            public Key   RF; //RotateFrame
            public KeyUV TF; //TranslateFrameU

            public TextureTransform()
            { Name = null; C = new KeyUV(); O = new KeyUV(); R = new KeyUV();
                Ro = new Key(); RF = new Key(); TF = new KeyUV(); }
        }

        public class Values
        {
            public List<int> BinOffset;
            public List<double> Value;

            public Values()
            { BinOffset = new List<int>(); Value = new List<double>(); }
        }

        public class ViewPoint
        {
            public double Aspect;
            public double FOVHorizontal;
            public double CameraApertureH;
            public double CameraApertureW;
            public Key FOV;
            public Key Roll;
            public Key FocalLength;
            public ModelTransform MT;

            public ViewPoint()
            { Aspect = -1; CameraApertureH = -1; CameraApertureW = -1; FocalLength = new Key();
                FOV = new Key(); FOVHorizontal = -1; MT = new ModelTransform(); Roll = new Key(); }
        }

        public class ModelTransform
        {
            public int BinOffset;
            public bool Writed;
            public string Name;
            public string UIDName;
            public Key Visibility;
            public Vector3Key Rot;
            public Vector3Key Scale;
            public Vector3Key Trans;
            
            public ModelTransform() => NewModelTransform();

            public void NewModelTransform()
            {
                Rot        = new Vector3Key();
                Scale      = new Vector3Key();
                Trans      = new Vector3Key();
                Visibility = new        Key();
                BinOffset  = -1;
                   Name    = null;
                UIDName    = null;
                Writed     = false;
            }

            public void Read(string Temp, int Type)
            {
                KKtMain.FindValue(Dict, Temp + MTBO      , ref BinOffset);
                KKtMain.FindValue(Dict, Temp +     "name", ref    Name  );
                KKtMain.FindValue(Dict, Temp + "uid_name", ref UIDName  );
                
                Rot       .Read(Temp + "rot."       , Type);
                Scale     .Read(Temp + "scale."     , Type);
                Trans     .Read(Temp + "trans."     , Type);
                Visibility.Read(Temp + "visibility.", Type);
            }

            public void Write(string Temp, byte Flags, bool A3DC)
            {
                if (A3DC && !Writed && ((Flags >> 6) & 0b1) == 1)
                {
                    IO.Write(Temp + MTBO + "=", BinOffset.ToString().ToLower());
                    Writed = true;
                }
                if (((Flags >> 5) & 0b1) == 1)
                    IO.Write(Temp + "name=", Name);
                if (!A3DC)
                {
                    if (((Flags >> 3) & 0b1) == 1)
                        Rot.Write(Temp + "rot.");
                    if (((Flags >> 2) & 0b1) == 1)
                        Scale.Write(Temp + "scale.");
                    if (((Flags >> 1) & 0b1) == 1)
                        Trans.Write(Temp + "trans.");
                }
                if (((Flags >> 4) & 0b1) == 1)
                    IO.Write(Temp + "uid_name=", UIDName);
                if (!A3DC)
                    if ((Flags & 0b1) == 1)
                        Visibility.Write(Temp + "visibility.");
            }

            public void Write(string Temp, byte Flags)
            {
                if (((Flags >> 5) & 0b1) == 1)
                    IO.Write(Temp + "name=", Name);
                if (((Flags >> 3) & 0b1) == 1)
                    Rot.Write(Temp + "rot.");
                if (((Flags >> 2) & 0b1) == 1)
                    Scale.Write(Temp + "scale.");
                if (((Flags >> 1) & 0b1) == 1)
                    Trans.Write(Temp + "trans.");
                if (((Flags >> 4) & 0b1) == 1)
                    IO.Write(Temp + "uid_name=", UIDName);
                if ((Flags & 0b1) == 1)
                    Visibility.Write(Temp + "visibility.");
            }

            public void Read()
            {
                if (BinOffset > -1)
                {
                    IO.Seek(Offset + BinOffset, 0);
                    Scale.Read(false);
                    Rot  .Read( true);
                    Trans.Read(false);
                    Visibility = new Key { BinOffset = IO.ReadInt32() };
                    Visibility.Read();
                }
            }

            public void Write()
            { Scale.Write(false); Rot.Write(true); Trans.Write(false); Visibility.Write(); }

            public void WriteOffset(bool ReturnToOffset)
            {
                if (ReturnToOffset)
                    IO.Seek(BinOffset, 0);
                else
                    BinOffset = (int)IO.Position;

                Scale.WriteOffset();
                Rot.WriteOffset();
                Trans.WriteOffset();
                IO.Write(Visibility.BinOffset);
                IO.Align(0x10);
            }

            public bool Read(ref KKtXml Xml, XElement element, string name) =>
                Read(ref Xml, element, name, "");

            public bool Read(ref KKtXml Xml, XElement element, string name, string altname)
            {
                BinOffset = -1;
                Writed = false;
                bool HasMT = element.Name.ToString() == name || element.Name.ToString() == altname;
                if (HasMT)
                    Read(ref Xml, element);
                return HasMT;
            }

            public void Read(ref KKtXml Xml, XElement element)
            {
                foreach (XAttribute Entry in element.Attributes())
                {
                    Xml.Reader(Entry, ref    Name,    "Name");
                    Xml.Reader(Entry, ref UIDName, "UIDName");
                }

                foreach (XElement Object in element.Elements())
                {
                    Rot       .Read(ref Xml, Object, "Rot"       );
                    Scale     .Read(ref Xml, Object, "Scale"     );
                    Trans     .Read(ref Xml, Object, "Trans"     );
                    Visibility.Read(ref Xml, Object, "Visibility");
                }
            }

            public void Write(ref KKtXml Xml, XElement element, string name)
            {
                XElement Child = new XElement(name);
                Write(ref Xml, Child);
                element.Add(Child);
            }

            public void Write(ref KKtXml Xml, XElement element)
            {
                Xml.Writer(element,    Name,    "Name");
                Xml.Writer(element, UIDName, "UIDName");
                Rot       .Write(ref Xml, element, "Rot"       );
                Scale     .Write(ref Xml, element, "Scale"     );
                Trans     .Write(ref Xml, element, "Trans"     );
                Visibility.Write(ref Xml, element, "Visibility");
            }
        }

        public class RGBAKey
        {
            public bool Alpha;
            public bool Boolean;
            public Key R;
            public Key G;
            public Key B;
            public Key A;

            public RGBAKey()
            { Alpha = false; Boolean = false; R = new Key(); G = new Key(); B = new Key(); A = new Key(); }

            public void Read(string Temp, int Type)
            {
                KKtMain.FindValue(Dict, Temp.Remove(Temp.Length - 1), ref Boolean);

                A.Read(Temp + "a.", Type);
                B.Read(Temp + "b.", Type);
                G.Read(Temp + "g.", Type);
                R.Read(Temp + "r.", Type);

                Alpha = A.Boolean;
            }

            public void Write(string Temp, bool A3DC, string Data)
            {
                if (Boolean)
                {
                    IO.Write(Temp + Data + "=", Boolean.ToString().ToLower());
                    if (Alpha)
                        A.Write(Temp + Data + ".a.", A3DC);
                    B.Write(Temp + Data + ".b.", A3DC);
                    G.Write(Temp + Data + ".g.", A3DC);
                    R.Write(Temp + Data + ".r.", A3DC);
                }
            }

            public void Read()
            {
                long CurrentOffset = IO.Position;
                R.Read();
                G.Read();
                B.Read();
                if (Alpha)
                    A.Read();
                IO.Seek(CurrentOffset, 0);
            }

            public void Write()
            { if (Boolean) { R.Write(); G.Write(); B.Write(); if (Alpha) A.Write(); } }

            public void Read(ref KKtXml Xml, XElement element, string name) =>
                Read(ref Xml, element, name, "");

            public void Read(ref KKtXml Xml, XElement element, string name, string altname)
            { if (element.Name.ToString() == name || element.Name.ToString() == altname)
                Read(ref Xml, element); }

            public void Read(ref KKtXml Xml, XElement element)
            {
                Boolean = true;

                foreach (XAttribute Entry in element.Attributes())
                    Xml.Reader(Entry, ref Alpha, "Alpha");

                foreach (XElement Child in element.Elements())
                {   R.Read(ref Xml, Child, "R"); G.Read(ref Xml, Child, "G");
                    B.Read(ref Xml, Child, "B"); A.Read(ref Xml, Child, "A"); }
            }

            public void Write(ref KKtXml Xml, XElement element, string name)
            {
                if (Boolean)
                {
                    XElement Child = new XElement(name);
                    if (Alpha)
                        Xml.Writer(Child, Alpha, "Alpha");
                    R.Write(ref Xml, Child, "R");
                    G.Write(ref Xml, Child, "G");
                    B.Write(ref Xml, Child, "B");
                    if (Alpha)
                        A.Write(ref Xml, Child, "A");
                    element.Add(Child);
                }
            }
        }

        public class Vector3Key
        {
            public Key X;
            public Key Y;
            public Key Z;

            public Vector3Key()
            { X = new Key(); Y = new Key(); Z = new Key(); }

            public void Read(string Temp, int Type)
            { X.Read(Temp + "x.", Type); Y.Read(Temp + "y.", Type); Z.Read(Temp + "z.", Type); }

            public void Write(string Temp)
            { X.Write(Temp + "x."); Y.Write(Temp + "y."); Z.Write(Temp + "z."); }

            public void Read(bool CompressF16)
            {
                X.BinOffset = IO.ReadInt32();
                Y.BinOffset = IO.ReadInt32();
                Z.BinOffset = IO.ReadInt32();
                long CurrentOffset = IO.Position;
                X.Read(CompressF16);
                Y.Read(CompressF16);
                Z.Read(CompressF16);
                IO.Seek(CurrentOffset, 0);
            }

            public void Write(bool CompressF16)
            { X.Write(CompressF16); Y.Write(CompressF16); Z.Write(CompressF16); }
            
            public void WriteOffset()
            {
                IO.Write(X.BinOffset);
                IO.Write(Y.BinOffset);
                IO.Write(Z.BinOffset);
            }

            public void Read(ref KKtXml Xml, XElement element, string name) =>
                Read(ref Xml, element, name, ""); 
            
            public void Read(ref KKtXml Xml, XElement element, string name, string altname)
            { if (element.Name.ToString() == name || element.Name.ToString() == altname)
                Read(ref Xml, element); }

            public void Read(ref KKtXml Xml, XElement element)
            {
                foreach (XElement Child in element.Elements())
                {
                    X.Read(ref Xml, Child, "X");
                    Y.Read(ref Xml, Child, "Y");
                    Z.Read(ref Xml, Child, "Z");
                }
            }

            public void Write(ref KKtXml Xml, XElement element, string name)
            {
                XElement Child = new XElement(name);
                X.Write(ref Xml, Child, "X");
                Y.Write(ref Xml, Child, "Y");
                Z.Write(ref Xml, Child, "Z");
                element.Add(Child);
            }
        }

        public class KeyUV
        {
            public Key U;
            public Key V;

            private readonly string u = "U";
            private readonly string v = "V";

            public KeyUV()
            { U = new Key(); V = new Key(); }

            public void Read(string Temp, int Type)
            { U.Read(Temp + u + d, Type); V.Read(Temp + v + d, Type); }

            public void Write(string Temp, bool A3DC, string Data)
            { U.Write(Temp, A3DC, Data + u); V.Write(Temp, A3DC, Data + v); }

            public void Read()
            { U.Read(); V.Read(); }

            public void Write()
            { U.Write(); V.Write(); }

            public void Read(ref KKtXml Xml, XElement element, string name)
            { Read(ref Xml, element, name, ""); }

            public void Read(ref KKtXml Xml, XElement element, string name, string altname)
            {
                if (element.Name.ToString() == name + u || element.Name.ToString() == altname + u)
                    U.Read(ref Xml, element);
                if (element.Name.ToString() == name + v || element.Name.ToString() == altname + v)
                    V.Read(ref Xml, element);
            }

            public void Write(ref KKtXml Xml, XElement element, string name)
            { U.Write(ref Xml, element, name + u); V.Write(ref Xml, element, name + v); }
        }

        public class Key
        {
            public int Type;
            public int Length;
            public int BinOffset;
            public bool Boolean;
            public bool RoundFrame;
            public double Max;
            public double Value;
            public double EPTypePost;
            public double EPTypePre;
            public RawD RawData;
            public Transform[] Trans;

            public Key() => NewKey();

            public void NewKey()
            { BinOffset = -1; Boolean = false; EPTypePost = -1; EPTypePre = -1; Length = -1; Max = -1;
                RawData = new RawD(); RoundFrame = false; Trans = null; Type = 0; Value = -1;}

            public class RawD
            {
                public int KeyType;
                public int ValueListSize;
                public string ValueType;
                public string ValueListString;
                public string[] ValueList;

                public RawD()
                { KeyType = -1; ValueListSize = -1; ValueType = null; ValueListString = null; ValueList = null; }
            }

            public struct Transform
            {
                public int Type;
                public double Frame;
                public double Value1;
                public double Value2;
                public double Value3;
            }

            public void Read(string Temp, int Type)
            {
                KKtMain.FindValue(Dict, Temp.Remove(Temp.Length - 1), ref Boolean);
                if (KKtMain.FindValue(Dict, Temp + BO, ref BinOffset))
                    Boolean = true;
                else if (KKtMain.FindValue(Dict, (Temp + "type").Split('.'), out string type))
                {
                    Boolean = true;
                    Read(Temp, Type, int.Parse(type));
                }
            }

            public void Read(string Temp, int Type, int type)
            {
                this.Type = type;
                if (Type == 0x3002 && this.Type == 0x0002)
                    this.Type = 0x3002;
                else if (Type == 0x0003 && this.Type == 0x0002)
                    this.Type = 0x0003;
                else if (this.Type == 4)
                    this.Type = 0x0003;

                if (Boolean)
                {
                    if (this.Type == 0x0000 || this.Type == 0x0001)
                        KKtMain.FindValue(Dict, Temp + "value", ref Value);
                    else
                    {
                        KKtMain.FindValue(Dict, Temp + "ep_type_post", ref EPTypePost);
                        KKtMain.FindValue(Dict, Temp + "ep_type_pre", ref EPTypePre);
                        KKtMain.FindValue(Dict, Temp + "key.length", ref Length);
                        KKtMain.FindValue(Dict, Temp + "max", ref Max);
                        KKtMain.FindValue(Dict, Temp + "raw_data_key_type", ref RawData.KeyType);

                        if (Length != -1)
                        {
                            Trans = new Transform[Length];
                            for (int i0 = 0; i0 < Length; i0++)
                                if (KKtMain.FindValue(Dict, Temp + "key." + i0 + ".data", ref value))
                                {
                                    dataArray = value.Replace("(", "").Replace(")", "").Split(',');
                                    Trans[i0].Frame = KKtMain.ToDouble(dataArray[0]);
                                    if (dataArray.Length > 1)
                                        Trans[i0].Value1 = KKtMain.ToDouble(dataArray[1]);
                                    if (dataArray.Length > 2)
                                        Trans[i0].Value2 = KKtMain.ToDouble(dataArray[2]);
                                    if (dataArray.Length > 3)
                                        Trans[i0].Value3 = KKtMain.ToDouble(dataArray[3]);
                                    Trans[i0].Type = dataArray.Length - 1;
                                }
                        }
                        else if (RawData.KeyType != -1)
                        {
                            KKtMain.FindValue(Dict, Temp + "raw_data.value_type", ref RawData.ValueType);
                            if (KKtMain.FindValue(Dict, Temp + "raw_data.value_list", ref RawData.ValueListString))
                                RawData.ValueList = RawData.ValueListString.Split(',');
                            KKtMain.FindValue(Dict, Temp + "raw_data.value_list_size",
                                ref RawData.ValueListSize);

                            int DataSize = 0;
                            if (RawData.KeyType > 2)
                                DataSize = 4;
                            else if (RawData.KeyType > 1)
                                DataSize = 3;
                            else if (RawData.KeyType > 0)
                                DataSize = 2;
                            else
                                DataSize = 1;

                            Length = RawData.ValueListSize / DataSize;
                            Trans = new Transform[Length];
                            for (int i = 0; i < Length; i++)
                            {
                                Trans[i].Type = RawData.KeyType;
                                Trans[i].Frame = KKtMain.ToDouble(RawData.ValueList[i * DataSize + 0]);
                                if (Trans[i].Type > 0)
                                    Trans[i].Value1 = KKtMain.ToDouble(RawData.ValueList[i * DataSize + 1]);
                                if (Trans[i].Type > 1)
                                    Trans[i].Value2 = KKtMain.ToDouble(RawData.ValueList[i * DataSize + 2]);
                                if (Trans[i].Type > 2)
                                    Trans[i].Value3 = KKtMain.ToDouble(RawData.ValueList[i * DataSize + 3]);
                            }
                            RawData = new RawD();
                        }
                    }
                }
            }

            public void Write(string Temp, bool A3DC, string Data)
            { if (Boolean) { IO.Write(Temp + Data + "=",
                Boolean.ToString().ToLower()); Write(Temp + Data + d, A3DC); } }

            public void Write(string Temp, bool A3DC)
            { if (Boolean) { if (A3DC) IO.Write(Temp + BO + "=",
                BinOffset.ToString().ToLower()); else Write(Temp); } }

            public void Write(string Temp)
            {
                if (Boolean)
                    if (Trans != null)
                    {
                        int[] SO = SortWriter(Trans.Length);
                        if (EPTypePost != -1)
                            IO.Write(Temp + "ep_type_post=", EPTypePost.ToString());
                        if (EPTypePre != -1)
                            IO.Write(Temp + "ep_type_pre=", EPTypePre.ToString());
                        if (Length > 0 && Length < 1000)
                        {
                            for (int i = 0; i < Trans.Length; i++)
                                Write(Temp, SO[i]);
                            IO.Write(Temp + "key.length=", Length.ToString());
                        }
                        if (Max != -1)
                            IO.Write(Temp + "max=", Max.ToString());
                        if (Length != -1 && Length >= 1000)
                        {
                            RawData = new RawD { ValueType = "float" };
                            for (int i = 0; i < Trans.Length; i++)
                                if (RawData.KeyType < Trans[i].Type)
                                    RawData.KeyType = Trans[i].Type;
                            RawData.ValueListSize = Trans.Length * (RawData.KeyType + 1);
                            IO.Write(Temp + "raw_data.value_list=");
                            for (int i = 0; i < Trans.Length; i++)
                            {
                                IO.Write(KKtMain.ToString(Trans[i].Frame).ToString());
                                if (RawData.KeyType > 0)
                                    IO.Write("," + KKtMain.ToString(Trans[i].Value1).ToString());
                                if (RawData.KeyType > 1)
                                    IO.Write("," + KKtMain.ToString(Trans[i].Value2).ToString());
                                if (RawData.KeyType > 2)
                                    IO.Write("," + KKtMain.ToString(Trans[i].Value3).ToString());
                                if (i < Trans.Length - 1)
                                    IO.Write(',');
                            }
                            IO.Write('\n');
                            IO.Write(Temp + "raw_data.value_list_size=", RawData.ValueListSize.ToString());
                            IO.Write(Temp + "raw_data.value_type=", RawData.ValueType);
                            IO.Write(Temp + "raw_data_key_type=", RawData.KeyType.ToString());
                            RawData = new RawD();
                        }
                        if (Type == 0x3002)
                            IO.Write(Temp + "type=2\n");
                        else
                        {
                            if (Type <= 0x0003)
                                IO.Write(Temp + "type=", Type.ToString());
                            else
                                IO.Write(Temp + "type=", "2");
                        }
                    }
                    else
                    {
                        if (Type == 0x0000)
                            IO.Write(Temp + "type=0\n");
                        else
                        {
                            IO.Write(Temp + "type=1\n");
                            IO.Write(Temp + "value=", KKtMain.ToString(Value).ToString());
                        }
                    }
            }

            public void Write(string Temp, int i)
            {
                Temp += "key." + i;
                string Frame = KKtMain.ToString(Trans[i].Frame);
                string V1 = KKtMain.ToString(Trans[i].Value1);
                string V2 = KKtMain.ToString(Trans[i].Value2);
                string V3 = KKtMain.ToString(Trans[i].Value3);
                if (Trans[i].Type == 3)
                    IO.Write(Temp + ".data=(" + Frame + "," + V1 + "," + V2 + "," + V3 + ")\n");
                else if (Trans[i].Type == 2)
                    IO.Write(Temp + ".data=(" + Frame + "," + V1 + "," + V2 + ")\n");
                else if (Trans[i].Type == 1)
                    IO.Write(Temp + ".data=(" + Frame + "," + V1 + ")\n");
                else
                    IO.Write(Temp + ".data=" + Frame + "\n");
                IO.Write(Temp + ".type=" + Trans[i].Type + "\n");
            }

            public void Read() => Read(false);

            public void Read(bool CompressF16)
            {
                if (BinOffset != -1)
                {
                    IO.Seek(Offset + BinOffset, 0);
                    Boolean = true;
                    Type = IO.ReadInt32();
                    
                    Value = IO.ReadSingle();
                    if (Type != 0x0000 && Type != 0x0001)
                    {
                        Max = IO.ReadSingle();
                        Length = IO.ReadInt32();
                        Trans = new Transform[Length];
                        for (int i = 0; i < Length; i++)
                        {
                            Trans[i].Type = 3;
                            if (CompressF16)
                            {
                                Trans[i].Frame = IO.ReadUInt16();
                                Trans[i].Value1 = IO.ReadHalf();
                            }
                            else
                            {
                                Trans[i].Frame = IO.ReadSingle();
                                Trans[i].Value1 = IO.ReadSingle();
                            }

                            if (CompressF16 && Data._.CompressF16 == 2)
                            {
                                Trans[i].Value2 = IO.ReadHalf();
                                Trans[i].Value3 = IO.ReadHalf();
                            }
                            else
                            {
                                Trans[i].Value2 = IO.ReadSingle();
                                Trans[i].Value3 = IO.ReadSingle();
                            }

                            if (Trans[i].Value3 == 0)
                            {
                                Trans[i].Type--;
                                if (Trans[i].Value2 == 0)
                                {
                                    Trans[i].Type--;
                                    if (Trans[i].Value3 == 0 && Trans[i].Value2 == 0 && Trans[i].Value1 == 0)
                                        Trans[i].Type--;
                                }
                            }
                        }
                    }
                }
            }

            public void Write() => Write(false);

            public void Write(bool CompressF16)
            {
                if (Trans != null && Boolean)
                {
                    BinOffset = (int)IO.Position;
                    IO.Write(Type);
                    IO.Write(0x00);
                    IO.Write((float)Max);
                    IO.Write(Trans.Length);
                    for (int i = 0; i < Trans.Length; i++)
                    {
                        if (CompressF16)
                        {
                            IO.Write((ushort)       Trans[i].Frame  );
                            IO.Write(KKtMain.ToHalf(Trans[i].Value1));
                        }
                        else
                        {
                            IO.Write((float)Trans[i].Frame );
                            IO.Write((float)Trans[i].Value1);
                        }
                        if (CompressF16 && Data._.CompressF16 == 2)
                        {
                            IO.Write(KKtMain.ToHalf(Trans[i].Value2));
                            IO.Write(KKtMain.ToHalf(Trans[i].Value3));
                        }
                        else
                        {
                            IO.Write((float)Trans[i].Value2);
                            IO.Write((float)Trans[i].Value3);
                        }
                    }
                }
                else if (Boolean)
                {
                    if (UsedValues.Value.Contains(Value) && A3DCOpt)
                    {
                        for (int i = 0; i < UsedValues.Value.Count; i++)
                            if (UsedValues.Value[i] == Value)
                                BinOffset = UsedValues.BinOffset[i];
                    }
                    else
                    {
                        BinOffset = (int)IO.Position;
                        if (Type != 0x00 || Value != 0)
                        {
                            IO.Write(0x01);
                            IO.Write((float)Value);
                        }
                        else
                        {
                            IO.Write((long)0x00);
                        }
                        if (A3DCOpt)
                        {
                            UsedValues.BinOffset.Add(BinOffset);
                            UsedValues.Value.Add(Value);
                        }
                    }
                }
            }

            public void Read(ref KKtXml Xml, XElement element, string name) => Read(ref Xml, element, name, "");

            public void Read(ref KKtXml Xml, XElement element, string name, string altname)
            { if (element.Name.ToString() == name || element.Name.ToString() == altname)
              Read(ref Xml, element); }

            public void Read(ref KKtXml Xml, XElement element)
            {
                NewKey();
                Boolean = true;

                foreach (XAttribute Entry in element.Attributes())
                {
                    Xml.Reader(Entry, ref EPTypePost, "Post");
                    Xml.Reader(Entry, ref EPTypePre , "Pre" );
                    Xml.Reader(Entry, ref Length    , "L"   );
                    Xml.Reader(Entry, ref Max       , "M"   );
                    Xml.Reader(Entry, ref Type      , "T"   );
                    Xml.Reader(Entry, ref Value     , "V"   , Base64);
                    Xml.Reader(Entry, ref RoundFrame, "RF"  );
                }

                if (Type != 0x0000 && Type != 0x0001)
                {
                    Trans = new Transform[Length];
                    int i = 0;

                    foreach (XElement Child in element.Elements())
                    {
                        if (Child.Name == "Key" || Child.Name == "K")
                            foreach (XAttribute Entry in Child.Attributes())
                            {
                                Xml.Reader(Entry, ref Trans[i].Frame , "F" );
                                Xml.Reader(Entry, ref Trans[i].Value1, "V1", Base64);
                                Xml.Reader(Entry, ref Trans[i].Value2, "V2", Base64);
                                Xml.Reader(Entry, ref Trans[i].Value3, "V3", Base64);
                                Xml.Reader(Entry, ref Trans[i].Type  , "T" );

                                if (RoundFrame)
                                    KKtMain.FloorCeiling(ref Trans[i].Frame);
                            }

                        i++;
                        if (i == Length)
                            break;
                    }
                }
                else if (Type == 0x0000)
                    Value = 0;

                RawData = new RawD();
                foreach (XAttribute Entry in element.Attributes())
                {
                    Xml.Reader(Entry, ref RawData.KeyType, "KeyType");
                    Xml.Reader(Entry, ref RawData.ValueType, "ValueType");
                    Xml.Reader(Entry, ref RawData.ValueListSize, "ValueListSize");
                    Xml.Reader(Entry, ref RawData.ValueList, "ValueList", ',');
                    Xml.Reader(Entry, ref RawData.KeyType, "KT");
                    Xml.Reader(Entry, ref RawData.ValueType, "VT");
                    Xml.Reader(Entry, ref RawData.ValueListSize, "VLS");
                    Xml.Reader(Entry, ref RawData.ValueList, "VL", ',');
                }

                if (RawData.ValueListSize > 0)
                {
                    int DataSize = 0;
                    if (RawData.KeyType > 2)
                        DataSize = 4;
                    else if (RawData.KeyType > 1)
                        DataSize = 3;
                    else if (RawData.KeyType > 0)
                        DataSize = 2;
                    else
                        DataSize = 1;

                    Length = RawData.ValueListSize / DataSize;
                    Trans = new Transform[Length];
                    for (int i1 = 0; i1 < Length; i1++)
                    {
                        Trans[i1].Type = RawData.KeyType;
                        Trans[i1].Frame = KKtMain.ToDouble(RawData.ValueList[i1 * DataSize + 0]);
                        if (Trans[i1].Type > 0)
                            Trans[i1].Value1 = KKtMain.ToDouble(RawData.ValueList[i1 * DataSize + 1]);
                        if (Trans[i1].Type > 1)
                            Trans[i1].Value2 = KKtMain.ToDouble(RawData.ValueList[i1 * DataSize + 2]);
                        if (Trans[i1].Type > 2)
                            Trans[i1].Value3 = KKtMain.ToDouble(RawData.ValueList[i1 * DataSize + 3]);
                    }
                    RawData = new RawD();
                }
            }

            public void Write(ref KKtXml Xml, XElement element, string name)
            {
                if (Boolean && Type != -1)
                {
                    XElement Keys = new XElement(name);
                    Xml.Writer(Keys, Type, "T");
                    if (Trans != null)
                    {
                        if (EPTypePost != -1)
                            Xml.Writer(Keys, EPTypePost, "Post");
                        if (EPTypePre != -1)
                            Xml.Writer(Keys, EPTypePre, "Pre");
                        if (Length != -1)
                            Xml.Writer(Keys, Length, "L");
                        if (Max != -1)
                            Xml.Writer(Keys, Max, "M");
                        int Type = 0;
                        if (Length < 1000)
                            for (int i = 0; i < Trans.Length; i++)
                            {
                                Type = Trans[i].Type;
                                if (Trans[i].Value3 == 0 && Type > 2)
                                    Type = 2;
                                if (Trans[i].Value2 == 0 && Type > 1)
                                    Type = 1;
                                if (Trans[i].Value1 == 0 && Type > 0)
                                    Type = 0;
                                XElement Key = new XElement("K");
                                Xml.Writer(Key, Type, "T");
                                Xml.Writer(Key, Trans[i].Frame, "F");
                                if (Type > 0)
                                    Xml.Writer(Key, Trans[i].Value1, "V1", Base64);
                                if (Type > 1)
                                    Xml.Writer(Key, Trans[i].Value2, "V2", Base64);
                                if (Type > 2)
                                    Xml.Writer(Key, Trans[i].Value3, "V3", Base64);
                                Keys.Add(Key);
                            }
                        else
                        {
                            RawData = new RawD { ValueType = "float" };
                            for (int i = 0; i < Trans.Length; i++)
                                if (RawData.KeyType < Trans[i].Type)
                                    RawData.KeyType = Trans[i].Type;
                            RawData.ValueListSize = Trans.Length * (RawData.KeyType + 1);
                            Xml.Writer(Keys, RawData.KeyType.ToString(), "KT");
                            Xml.Writer(Keys, RawData.ValueType, "VT");
                            Xml.Writer(Keys, RawData.ValueListSize.ToString(), "VLS");

                            for (int i = 0; i < Trans.Length; i++)
                            {
                                RawData.ValueListString += KKtMain.ToString(Trans[i].Frame);
                                if (RawData.KeyType > 0)
                                    RawData.ValueListString += "," + KKtMain.ToString(Trans[i].Value1, Base64);
                                if (RawData.KeyType > 1)
                                    RawData.ValueListString += "," + KKtMain.ToString(Trans[i].Value2, Base64);
                                if (RawData.KeyType > 2)
                                    RawData.ValueListString += "," + KKtMain.ToString(Trans[i].Value3, Base64);
                                if (i < Trans.Length - 1)
                                    RawData.ValueListString += ',';
                            }
                            Xml.Writer(Keys, RawData.ValueListString, "VL");
                            RawData = new RawD();
                        }
                    }
                    else if (Value != 0 && Type > 0)
                        Xml.Writer(Keys, Value, "V", Base64);
                    element.Add(Keys);
                }
            }
        }
    }
}
