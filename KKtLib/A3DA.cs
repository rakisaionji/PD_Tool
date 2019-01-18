using System;
using System.IO;
using System.Xml.Linq;
using System.Collections.Generic;
using KKtIO = KKtLib.IO.KKtIO;
using KKtXml = KKtLib.Xml.KKtXml;
using KKtMain = KKtLib.Main;
using KKtText = KKtLib.Text;

namespace KKtLib.A3DA
{
    public class A3DA
    {
        private const bool A3DCOpt = true;
        private const string d = ".";
        private const string BO = "bin_offset";
        private const string MTBO = "model_transform.bin_offset";

        private int SOi0;
        private int SOi1;
        private int Offset;
        private int[] SO0;
        private int[] SO1;
        private bool A3DC;
        private bool Base64;
        private string name;
        private string nameInt;
        private string nameView;
        private string value;
        private string[] dataArray;
        private Values UsedValues;
        private Dictionary<string, object> Dict;

        public KKtIO IO;
        public KKtXml Xml;
        public A3DAData Data;

        public A3DA() { A3DC = false; Data = new A3DAData(); Dict = new Dictionary<string, object>();
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

        private void A3DAReader()
        {
            int i0 = 0;
            int i1 = 0;

            if (KKtMain.FindValue(Dict, "_.compress_f16", ref value))
                Data._.CompressF16 = int.Parse(value);
            if (KKtMain.FindValue(Dict, "_.converter.version", ref value))
                Data._.ConverterVersion = value;
            if (KKtMain.FindValue(Dict, "_.file_name", ref value))
                Data._.FileName = value;
            if (KKtMain.FindValue(Dict, "_.property.version", ref value))
                Data._.PropertyVersion = value;
            if (KKtMain.FindValue(Dict, "camera_root.length", ref value))
                Data.Camera.Root = new CameraRoot[int.Parse(value)];
            if (KKtMain.FindValue(Dict, "chara.length", ref value))
                Data.Chara = new ModelTransform[int.Parse(value)];
            if (KKtMain.FindValue(Dict, "curve.length", ref value))
                Data.Curve = new Curve[int.Parse(value)];
            if (KKtMain.FindValue(Dict, "dof.name", ref value))
                Data.DOF.Name = value;
            if (KKtMain.FindValue(Dict, "event.length", ref value))
                Data.Event = new Event[int.Parse(value)];
            if (KKtMain.FindValue(Dict, "fog.length", ref value))
                Data.Fog = new Fog[int.Parse(value)];
            if (KKtMain.FindValue(Dict, "light.length", ref value))
                Data.Light = new Light[int.Parse(value)];
            if (KKtMain.FindValue(Dict, "m_objhrc.length", ref value))
                Data.MObjectHRC = new MObjectHRC[int.Parse(value)];
            if (KKtMain.FindValue(Dict, "m_objhrc_list.length", ref value))
                Data.MObjectHRCList = new string[int.Parse(value)];
            if (KKtMain.FindValue(Dict, "motion.length", ref value))
                Data.Motion = new string[int.Parse(value)];
            if (KKtMain.FindValue(Dict, "object.length", ref value))
                Data.Object = new Object[int.Parse(value)];
            if (KKtMain.FindValue(Dict, "objhrc.length", ref value))
                Data.ObjectHRC = new ObjectHRC[int.Parse(value)];
            if (KKtMain.FindValue(Dict, "object_list.length", ref value))
                Data.ObjectList = new string[int.Parse(value)];
            if (KKtMain.FindValue(Dict, "objhrc_list.length", ref value))
                Data.ObjectHRCList = new string[int.Parse(value)];
            if (KKtMain.FindValue(Dict, "play_control.begin", ref value))
                Data.PlayControl.Begin = KKtMain.ToDouble(value);
            if (KKtMain.FindValue(Dict, "play_control.div", ref value))
                Data.PlayControl.FPS = KKtMain.ToDouble(value);
            if (KKtMain.FindValue(Dict, "play_control.fps", ref value))
                Data.PlayControl.FPS = KKtMain.ToDouble(value);
            if (KKtMain.FindValue(Dict, "play_control.offset", ref value))
                Data.PlayControl.Offset = KKtMain.ToDouble(value);
            if (KKtMain.FindValue(Dict, "play_control.size", ref value))
                Data.PlayControl.Size = KKtMain.ToDouble(value);
            if (KKtMain.FindValue(Dict, "point.length", ref value))
                Data.Point = new ModelTransform[int.Parse(value)];
            if (KKtMain.StartsWith(Dict, "camera_auxiliary"))
                Data.Camera.Auxiliary.Boolean = true;
            if (KKtMain.StartsWith(Dict, "post_process"))
                Data.PostProcess.Boolean = true;

            if (Data.Camera.Auxiliary.Boolean)
            {
                name = "camera_auxiliary.";

                Read(ref Data.Camera.Auxiliary.AutoExposure, name + "auto_exposure.");
                Read(ref Data.Camera.Auxiliary.    Exposure, name +      "exposure.");
                Read(ref Data.Camera.Auxiliary.Gamma       , name + "gamma."        );
                Read(ref Data.Camera.Auxiliary.GammaRate   , name + "gamma_rate."   );
                Read(ref Data.Camera.Auxiliary.Saturate    , name + "saturate."     );
            }

            for (i0 = 0; i0 < Data.Camera.Root.Length; i0++)
            {
                name = "camera_root." + i0 + d;
                nameInt = name + "interest.";
                nameView = name + "view_point.";

                Data.Camera.Root[i0] = new CameraRoot();
                
                KKtMain.FindValue(Dict, nameView + "aspect"            ,
                    ref Data.Camera.Root[i0].ViewPoint.Aspect                 );
                KKtMain.FindValue(Dict, nameView + "camera_aperture_h" ,
                    ref Data.Camera.Root[i0].ViewPoint.CameraApertureH        );
                KKtMain.FindValue(Dict, nameView + "camera_aperture_w" ,
                    ref Data.Camera.Root[i0].ViewPoint.CameraApertureW        );
                KKtMain.FindValue(Dict, nameView + "fov_is_horizontal" ,
                    ref Data.Camera.Root[i0].ViewPoint.FOVHorizontal          );

                Read(ref Data.Camera.Root[i0].MT                   , name                      );
                Read(ref Data.Camera.Root[i0].Interest             , nameInt                   );
                Read(ref Data.Camera.Root[i0].ViewPoint.MT         , nameView                  );
                Read(ref Data.Camera.Root[i0].ViewPoint.FocalLength, nameView + "focal_length.");
                Read(ref Data.Camera.Root[i0].ViewPoint.FOV        , nameView + "fov."         );
                Read(ref  Data.Camera.Root[i0].ViewPoint.Roll       , nameView + "roll."        );
            }

            for (i0 = 0; i0 < Data.Chara.Length; i0++)
            {
                Data.Chara[i0] = new ModelTransform();
                Read(ref Data.Chara[i0], "chara." + i0 + d);
            }

            for (i0 = 0; i0 < Data.Curve.Length; i0++)
            {
                Data.Curve[i0] = new Curve();
                name = "curve." + i0 + d;
                
                KKtMain.FindValue(Dict, name + "name", ref Data.Curve[i0].Name);
                Read(ref Data.Curve[i0].CV, name + "cv.");
            }

            if (Data.DOF.Name != null)
            {
                Data.DOF = new ModelTransform();
                Read(ref Data.DOF, "dof.");
            }

            for (i0 = 0; i0 < Data.Fog.Length; i0++)
            {
                Data.Fog[i0] = new Fog();
                name = "fog." + i0 + d;

                KKtMain.FindValue(Dict, name + "id", ref Data.Fog[i0].Id);
                Read(ref Data.Fog[i0].Density, name + "density.");
                Read(ref Data.Fog[i0].Diffuse, name + "Diffuse.");
                Read(ref Data.Fog[i0].End    , name + "end."    );
                Read(ref Data.Fog[i0].Start  ,name + "start."  );
            }

            for (i0 = 0; i0 < Data.Light.Length; i0++)
            {
                Data.Light[i0] = new Light();
                name = "light." + i0 + d;

                KKtMain.FindValue(Dict, name + "id", ref Data.Light[i0].Id);
                KKtMain.FindValue(Dict, name + "name", ref Data.Light[i0].Name);
                KKtMain.FindValue(Dict, name + "type", ref Data.Light[i0].Type);

                Read(ref Data.Light[i0].Ambient      , name + "Ambient."       );
                Read(ref Data.Light[i0].Diffuse      , name + "Diffuse."       );
                Read(ref Data.Light[i0].Incandescence, name + "Incandescence." );
                Read(ref Data.Light[i0].Specular     , name + "Specular."      );
                Read(ref Data.Light[i0].Position     , name + "position."      );
                Read(ref Data.Light[i0].SpotDirection, name + "spot_direction.");
            }

            for (i0 = 0; i0 < Data.Event.Length; i0++)
            {
                name = "event." + i0 + d;

                KKtMain.FindValue(Dict, name + "begin", ref Data.Event[i0].Begin);
                KKtMain.FindValue(Dict, name + "clip_begin", ref Data.Event[i0].ClipBegin);
                KKtMain.FindValue(Dict, name + "clip_en", ref Data.Event[i0].ClipEnd);
                KKtMain.FindValue(Dict, name + "end", ref Data.Event[i0].End);
                KKtMain.FindValue(Dict, name + "name", ref Data.Event[i0].Name);
                KKtMain.FindValue(Dict, name + "param1", ref Data.Event[i0].Param1);
                KKtMain.FindValue(Dict, name + "ref", ref Data.Event[i0].Ref);
                KKtMain.FindValue(Dict, name + "time_ref_scale", ref Data.Event[i0].TimeRefScale);
                KKtMain.FindValue(Dict, name + "type", ref Data.Event[i0].Type);
            }

            for (i0 = 0; i0 < Data.MObjectHRC.Length; i0++)
            {
                Data.MObjectHRC[i0] = new MObjectHRC();
                name = "m_objhrc." + i0 + d;

                if (KKtMain.FindValue(Dict, name + "instance.length", ref value))
                    Data.MObjectHRC[i0].Instance = new ModelTransform[int.Parse(value)];
                if (KKtMain.FindValue(Dict, name + "node.length", ref value))
                    Data.MObjectHRC[i0].Node = new Node[int.Parse(value)];
                KKtMain.FindValue(Dict, name + "name", ref Data.MObjectHRC[i0].Name);

                if (Data.MObjectHRC[i0].Instance != null)
                    for (i1 = 0; i1 < Data.MObjectHRC[i0].Instance.Length; i1++)
                    {
                        name = "m_objhrc." + i0 + ".instance." + i1 + d;
                        Data.MObjectHRC[i0].Instance[i1] = new ModelTransform();
                        Read(ref Data.MObjectHRC[i0].Instance[i1], name);
                    }

                if (Data.MObjectHRC[i0].Node != null)
                    for (i1 = 0; i1 < Data.MObjectHRC[i0].Node.Length; i1++)
                    {
                        Data.MObjectHRC[i0].Node[i1] = new Node();
                        name = "m_objhrc." + i0 + ".node." + i1 + d;
                        Data.MObjectHRC[i0].Node[i1].MT = new ModelTransform();
                        KKtMain.FindValue(Dict, name + "parent", ref Data.MObjectHRC[i0].Node[i1].Parent);

                        Read(ref Data.MObjectHRC[i0].Node[i1].MT, name);
                    }
            }

            for (i0 = 0; i0 < Data.MObjectHRCList.Length; i0++)
                KKtMain.FindValue(Dict, "m_objhrc_list." + i0, ref Data.MObjectHRCList[i0]);

            for (i0 = 0; i0 < Data.Motion.Length; i0++)
                KKtMain.FindValue(Dict, "motion." + i0 + ".name", ref Data.Motion[i0]);

            for (i0 = 0; i0 < Data.Object.Length; i0++)
            {
                Data.Object[i0] = new Object();
                name = "object." + i0 + d;
                
                KKtMain.FindValue(Dict, name + "morph", ref Data.Object[i0].Morph);
                KKtMain.FindValue(Dict, name + "morph_offset", ref Data.Object[i0].MorphOffset);
                KKtMain.FindValue(Dict, name + "parent_name", ref Data.Object[i0].ParentName);

                if (KKtMain.FindValue(Dict, name + "tex_pat.length", ref value))
                    Data.Object[i0].TP = new TexturePattern[int.Parse(value)];
                if (KKtMain.FindValue(Dict, name + "tex_transform.length", ref value))
                    Data.Object[i0].TT = new TextureTransform[int.Parse(value)];

                if (Data.Object[i0].TP != null)
                    for (i1 = 0; i1 < Data.Object[i0].TP.Length; i1++)
                    {
                        Data.Object[i0].TP[i1] = new TexturePattern();
                        name = "object." + i0 + ".tex_pat." + i1 + d;
                        KKtMain.FindValue(Dict, name + "name"      , ref Data.Object[i0].TP[i1].Name     );
                        KKtMain.FindValue(Dict, name + "pat"       , ref Data.Object[i0].TP[i1].Pat      );
                        KKtMain.FindValue(Dict, name + "pat_offset", ref Data.Object[i0].TP[i1].PatOffset);
                    }

                if (Data.Object[i0].TT != null)
                    for (i1 = 0; i1 < Data.Object[i0].TT.Length; i1++)
                    {
                        name = "object." + i0 + ".tex_transform." + i1 + d;
                        Data.Object[i0].TT[i1] = new TextureTransform();

                        KKtMain.FindValue(Dict, name + "name", ref Data.Object[i0].TT[i1].Name);
                        Read(ref Data.Object[i0].TT[i1].C , name + "coverage"      );
                        Read(ref Data.Object[i0].TT[i1].O , name + "offset"        );
                        Read(ref Data.Object[i0].TT[i1].R , name + "repeat"        );
                        Read(ref Data.Object[i0].TT[i1].Ro, name + "rotate."       );
                        Read(ref Data.Object[i0].TT[i1].RF, name + "rotateFrame."  );
                        Read(ref Data.Object[i0].TT[i1].TF, name + "translateFrame");
                    }

                name = "object." + i0 + d;
                Read(ref Data.Object[i0].MT, name);
            }


            for (i0 = 0; i0 < Data.ObjectHRC.Length; i0++)
            {
                Data.ObjectHRC[i0] = new ObjectHRC();
                name = "objhrc." + i0 + d;

                KKtMain.FindValue(Dict, name + "shadow"  , ref Data.ObjectHRC[i0].Shadow );
                KKtMain.FindValue(Dict, name + "name"    , ref Data.ObjectHRC[i0].Name   );
                KKtMain.FindValue(Dict, name + "uid_name", ref Data.ObjectHRC[i0].UIDName);
                if (KKtMain.FindValue(Dict, name + "node.length", ref value))
                    Data.ObjectHRC[i0].Node = new Node[int.Parse(value)];

                if (Data.ObjectHRC[i0].Node != null)
                    for (i1 = 0; i1 < Data.ObjectHRC[i0].Node.Length; i1++)
                    {
                        Data.ObjectHRC[i0].Node[i1] = new Node();
                        name = "objhrc." + i0 + ".node." + i1 + d;

                        KKtMain.FindValue(Dict, name + "parent", ref Data.ObjectHRC[i0].Node[i1].Parent);

                        Read(ref Data.ObjectHRC[i0].Node[i1].MT, name);
                    }
            }


            for (i0 = 0; i0 < Data.ObjectHRCList.Length; i0++)
            {
                Data.ObjectHRCList[i0] = "";
                KKtMain.FindValue(Dict, "objhrc_list." + i0, ref Data.ObjectHRCList[i0]);
            }


            for (i0 = 0; i0 < Data.ObjectList.Length; i0++)
            {
                Data.ObjectList[i0] = "";
                KKtMain.FindValue(Dict, "object_list." + i0, ref Data.ObjectList[i0]);
            }

            for (i0 = 0; i0 < Data.Point.Length; i0++)
            {
                Data.Point[i0] = new ModelTransform();
                Read(ref Data.Point[i0], "point." + i0 + d);
            }

            if (Data.PostProcess.Boolean)
            {
                name = "post_process.";
                Data.PostProcess = new PostProcess();

                Read(ref Data.PostProcess.Ambient  , name + "Ambient."   );
                Read(ref Data.PostProcess.Diffuse  , name + "Diffuse."   );
                Read(ref Data.PostProcess.Specular , name + "Specular."  );
                Read(ref Data.PostProcess.LensFlare, name + "lens_flare.");
                Read(ref Data.PostProcess.LensGhost, name + "lens_ghost.");
                Read(ref Data.PostProcess.LensShaft, name + "lens_shaft.");
            }
        }
        
        public void A3DAWriter(string file)
        {
            int i0 = 0;
            int i1 = 0;
            DateTime date = DateTime.Now;
            if (A3DC && Data._.CompressF16 != 0)
                IO.Write("", "#-compress_f16");
            if (!A3DC)
                IO.Write("#A3DA__________\n");
            IO.Write("#", DateTime.UtcNow.ToString("ddd MMM dd HH:mm:ss yyyy",
                System.Globalization.CultureInfo.InvariantCulture));
            if (A3DC && Data._.CompressF16 != 0)
                IO.Write("_.compress_f16=", Data._.CompressF16);

            IO.Write("_.converter.version=", Data._.ConverterVersion);
            IO.Write("_.file_name=", Data._.FileName);
            IO.Write("_.property.version=", Data._.PropertyVersion);

            name = "camera_auxiliary.";

            if (Data.Camera.Auxiliary.Boolean)
            {
                Write(ref Data.Camera.Auxiliary.AutoExposure, name, "auto_exposure");
                Write(ref Data.Camera.Auxiliary.    Exposure, name, "exposure"     );
                Write(ref Data.Camera.Auxiliary.Gamma       , name, "gamma"        );
                Write(ref Data.Camera.Auxiliary.GammaRate   , name, "gamma_rate"   );
                Write(ref Data.Camera.Auxiliary.Saturate    , name, "saturate"     );
            }

            if (Data.Camera.Root.Length != 0)
            {
                SO0 = SortWriter(Data.Camera.Root.Length);
                SOi0 = 0;
                for (i0 = 0; i0 < Data.Camera.Root.Length; i0++)
                {
                    SOi0 = SO0[i0];
                    name = "camera_root." + SOi0 + d;
                    nameInt = "camera_root." + SOi0 + ".interest.";
                    nameView = "camera_root." + SOi0 + ".view_point.";

                    Write(ref Data.Camera.Root[SOi0].Interest, nameInt, 0b1001111);
                    Write(ref Data.Camera.Root[SOi0].MT, name, 0b1001110);
                    IO.Write(nameView + "aspect=", Data.Camera.Root[i0].ViewPoint.Aspect);
                    if (Data.Camera.Root[i0].ViewPoint.CameraApertureH != -1)
                        IO.Write(nameView + "camera_aperture_h=",
                            Data.Camera.Root[i0].ViewPoint.CameraApertureH);
                    if (Data.Camera.Root[i0].ViewPoint.CameraApertureW != -1)
                        IO.Write(nameView + "camera_aperture_w=",
                            Data.Camera.Root[i0].ViewPoint.CameraApertureW);
                    Write(ref Data.Camera.Root[SOi0].ViewPoint.FocalLength, nameView + "focal_length.");
                    Write(ref Data.Camera.Root[SOi0].ViewPoint.FOV, nameView + "fov.");
                    IO.Write(nameView + "fov_is_horizontal=",
                        Data.Camera.Root[i0].ViewPoint.FOVHorizontal);
                    Write(ref Data.Camera.Root[SOi0].ViewPoint.MT, nameView, 0b1000000);
                    Write(ref Data.Camera.Root[SOi0].ViewPoint.Roll, nameView + "roll.");
                    Write(ref Data.Camera.Root[SOi0].ViewPoint.MT, nameView, 0b0001111);
                    Write(ref Data.Camera.Root[SOi0].MT, name, 0b0000001);
                }
                IO.Write("camera_root.length=", Data.Camera.Root.Length);
            }

            if (Data.Chara.Length != 0)
            {
                SO0 = SortWriter(Data.Chara.Length);
                name = "chara.";

                for (i0 = 0; i0 < Data.Chara.Length; i0++)
                    Write(ref Data.Chara[SO0[i0]], name + SO0[i0] + d, 0b1001111);
                IO.Write(name + "length=", Data.Chara.Length);
            }

            if (Data.Curve.Length != 0)
            {
                SO0 = SortWriter(Data.Curve.Length);
                SOi0 = 0;

                for (i0 = 0; i0 < Data.Curve.Length; i0++)
                {
                    SOi0 = SO0[i0];
                    name = "curve." + SOi0 + d;

                    Write(ref Data.Curve[SOi0].CV, name + "cv.");
                    IO.Write(name + "name=", Data.Curve[SOi0].Name);
                }
                IO.Write("curve.length=", Data.Curve.Length);
            }

            if (Data.DOF.Name != null)
                Write(ref Data.DOF, "dof.", 0b1111111);

            if (Data.Event.Length != 0)
            {
                SO0 = SortWriter(Data.Event.Length);
                SOi0 = 0;
                for (i0 = 0; i0 < Data.Event.Length; i0++)
                {
                    SOi0 = SO0[i0];
                    name = "event." + SOi0 + d;

                    IO.Write(name + "begin=", Data.Event[SOi0].Begin);
                    IO.Write(name + "clip_begin=", Data.Event[SOi0].ClipBegin);
                    IO.Write(name + "clip_en=", Data.Event[SOi0].ClipEnd);
                    IO.Write(name + "end=", Data.Event[SOi0].End);
                    IO.Write(name + "name=", Data.Event[SOi0].Name);
                    IO.Write(name + "param1=", Data.Event[SOi0].Param1);
                    IO.Write(name + "ref=", Data.Event[SOi0].Ref);
                    IO.Write(name + "time_ref_scale=", Data.Event[SOi0].TimeRefScale);
                    IO.Write(name + "type=", Data.Event[SOi0].Type);
                }
            }

            if (Data.Fog.Length != 0)
            {
                SO0 = SortWriter(Data.Fog.Length);
                SOi0 = 0;
                for (i0 = 0; i0 < Data.Fog.Length; i0++)
                {
                    SOi0 = SO0[i0];
                    name = "fog." + SOi0 + d;

                    Write(ref Data.Fog[SOi0].Diffuse, name, "Diffuse");
                    Write(ref Data.Fog[SOi0].Density, name, "density");
                    Write(ref Data.Fog[SOi0].End    , name, "end"    );
                    IO.Write(name + "id=", Data.Fog[SOi0].Id);
                    Write(ref Data.Fog[SOi0].Start  , name, "start"  );
                }
                IO.Write("fog.length=", Data.Fog.Length);
            }

            if (Data.Light.Length != 0)
            {
                SO0 = SortWriter(Data.Light.Length);
                SOi0 = 0;
                for (i0 = 0; i0 < Data.Light.Length; i0++)
                {
                    SOi0 = SO0[i0];
                    name = "light." + SOi0 + d;

                    Write(ref Data.Light[SOi0].Ambient      , name, "Ambient"      );
                    Write(ref Data.Light[SOi0].Diffuse      , name, "Diffuse"      );
                    Write(ref Data.Light[SOi0].Incandescence, name, "Incandescence");
                    Write(ref Data.Light[SOi0].Specular     , name, "Specular"     );
                    IO.Write(name + "id=", Data.Light[SOi0].Id);
                    IO.Write(name + "name=", Data.Light[SOi0].Name);
                    Write(ref Data.Light[SOi0].Position     , name + "position."      , 0b1001111);
                    Write(ref Data.Light[SOi0].SpotDirection, name + "spot_direction.", 0b1001111);
                    IO.Write(name + "type=", Data.Light[SOi0].Type);
                }
                IO.Write("light.length=", Data.Light.Length);
            }

            if (Data.MObjectHRC.Length != 0)
            {
                SO0 = SortWriter(Data.MObjectHRC.Length);
                SOi0 = 0;
                for (i0 = 0; i0 < Data.MObjectHRC.Length; i0++)
                {
                    SOi0 = SO0[i0];
                    name = "m_objhrc." + SOi0 + d;

                    if (Data.MObjectHRC[SOi0].Instance != null)
                    {
                        name = "m_objhrc." + SOi0 + ".instance.";
                        int[] SO1 = SortWriter(Data.MObjectHRC[i0].Instance.Length);
                        int SOi1 = 0;
                        for (i1 = 0; i1 < Data.MObjectHRC[i0].Instance.Length; i1++)
                        {
                            SOi1 = SO1[i1];
                            Write(ref Data.MObjectHRC[SOi0].Instance[SOi1], name + SOi1 + d, 0b1111111);
                        }
                        IO.Write(name + "length=",
                            Data.MObjectHRC[SOi0].Instance.Length);
                    }
                    IO.Write(name + "name=", Data.MObjectHRC[SOi0].Name);
                    if (Data.MObjectHRC[SOi0].Node != null)
                    {
                        name = "m_objhrc." + SOi0 + ".node.";
                        int[] SO1 = SortWriter(Data.MObjectHRC[i0].Node.Length);
                        int SOi1 = 0;
                        for (i1 = 0; i1 < Data.MObjectHRC[i0].Node.Length; i1++)
                        {
                            SOi1 = SO1[i1];
                            IO.Write(name + SOi1 + ".name=",
                                Data.MObjectHRC[SOi0].Node[SOi1].Name);
                            IO.Write(name + SOi1 + ".parent=",
                                Data.MObjectHRC[SOi0].Node[SOi1].Parent);
                            Write(ref Data.MObjectHRC[SOi0].Node[SOi1].MT, name + SOi1 + d, 0b1001111);

                        }
                        IO.Write(name + "length=", Data.
                            MObjectHRC[SOi0].Node.Length);
                    }
                }
                IO.Write("m_objhrc.length=", Data.MObjectHRC.Length);
            }

            if (Data.MObjectHRCList.Length != 0)
            {
                SO0 = SortWriter(Data.MObjectHRCList.Length);
                name = "m_objhrc_list.";
                for (i0 = 0; i0 < Data.MObjectHRCList.Length; i0++)
                    IO.Write(name + SO0[i0] + "=",
                        Data.MObjectHRCList[SO0[i0]]);
                IO.Write(name + "length=", Data.MObjectHRCList.Length);
            }

            if (Data.Motion.Length != 0)
            {
                SO0 = SortWriter(Data.Motion.Length);
                name = "motion.";
                for (i0 = 0; i0 < Data.Motion.Length; i0++)
                    IO.Write(name + SO0[i0] + ".name=",
                        Data.Motion[SO0[i0]]);
                IO.Write(name + "length=", Data.Motion.Length);
            }

            if (Data.Object.Length != 0)
            {
                SO0 = SortWriter(Data.Object.Length);
                SOi0 = 0;
                for (i0 = 0; i0 < Data.Object.Length; i0++)
                {
                    SOi0 = SO0[i0];
                    name = "object." + SOi0 + d;

                    Write(ref Data.Object[SOi0].MT, name, 0b1000000);
                    if (Data.Object[SOi0].Morph != null)
                    {
                        IO.Write(name + "morph=", Data.Object[SOi0].Morph);
                        IO.Write(name + "morph_offset=",
                            Data.Object[SOi0].MorphOffset);
                    }
                    Write(ref Data.Object[SOi0].MT, name, 0b0100000);
                    IO.Write(name + "parent_name=", Data.Object[SOi0].ParentName);
                    Write(ref Data.Object[SOi0].MT, name, 0b0001100);

                    if (Data.Object[SOi0].TP != null)
                    {
                        nameView = name + "tex_pat.";

                        SO1 = SortWriter(Data.Object[SOi0].TP.Length);
                        SOi1 = 0;
                        for (i1 = 0; i1 < Data.Object[SOi0].TP.Length; i1++)
                        {
                            SOi1 = SO1[i1];
                            IO.Write(nameView + SOi1 + ".name=",
                                Data.Object[SOi0].TP[SOi1].Name);
                            IO.Write(nameView + SOi1 + ".pat=",
                                Data.Object[SOi0].TP[SOi1].Pat);
                            IO.Write(nameView + SOi1 + ".pat_offset=",
                                Data.Object[SOi0].TP[SOi1].PatOffset);
                        }
                        IO.Write(nameView + "length=" + Data.Object[SOi0].TP.Length + "\n");
                    }

                    if (Data.Object[SOi0].TT != null)
                    {
                        SO1 = SortWriter(Data.Object[SOi0].TT.Length);
                        SOi1 = 0;
                        nameView = name + "tex_transform.";
                        for (i1 = 0; i1 < Data.Object[SOi0].TT.Length; i1++)
                        {
                            SOi1 = SO1[i1];
                            IO.Write(nameView + SOi1 + ".name=", Data.Object[SOi0].TT[SOi1].Name);
                            Write(ref Data.Object[SOi0].TT[SOi1].C , nameView + SOi1 + d, "coverage");
                            Write(ref Data.Object[SOi0].TT[SOi1].O , nameView + SOi1 + d, "offset");
                            Write(ref Data.Object[SOi0].TT[SOi1].R , nameView + SOi1 + d, "repeat");
                            Write(ref Data.Object[SOi0].TT[SOi1].Ro, nameView + SOi1 + d, "rotate");
                            Write(ref Data.Object[SOi0].TT[SOi1].RF, nameView + SOi1 + d, "rotateFrame");
                            Write(ref Data.Object[SOi0].TT[SOi1].TF, nameView + SOi1 + d, "translateFrame");
                        }
                        IO.Write(nameView + "length=" + Data.Object[SOi0].TT.Length + "\n");
                    }

                    Write(ref Data.Object[SOi0].MT, name, 0b10011);
                }
                IO.Write("object.length=", Data.Object.Length);
            }

            if (Data.ObjectList.Length != 0)
            {
                SO0 = SortWriter(Data.ObjectList.Length);
                for (i0 = 0; i0 < Data.ObjectList.Length; i0++)
                    IO.Write("object_list." + SO0[i0] + "=",
                        Data.ObjectList[SO0[i0]]);
                IO.Write("object_list.length=", Data.ObjectList.Length);
            }

            if (Data.ObjectHRC.Length != 0)
            {
                SO0 = SortWriter(Data.ObjectHRC.Length);
                SOi0 = 0;
                for (i0 = 0; i0 < Data.ObjectHRC.Length; i0++)
                {
                    SOi0 = SO0[i0];
                    IO.Write("objhrc." + SOi0 + ".name=", Data.ObjectHRC[SOi0].Name);
                    if (Data.ObjectHRC[SOi0].Node != null)
                    {
                        SO1 = SortWriter(Data.ObjectHRC[i0].Node.Length);
                        SOi1 = 0;
                        name = "objhrc." + SOi0 + ".node.";
                        for (i1 = 0; i1 < Data.ObjectHRC[i0].Node.Length; i1++)
                        {
                            SOi1 = SO1[i1];
                            Write(ref Data.ObjectHRC[SOi0].Node[SOi1].MT, name + SOi1 + d, 0b1000000);
                            IO.Write(name + SOi1 + ".name=",
                                Data.ObjectHRC[SOi0].Node[SOi1].Name);
                            IO.Write(name + SOi1 + ".parent=",
                                Data.ObjectHRC[SOi0].Node[SOi1].Parent);
                            Write(ref Data.ObjectHRC[SOi0].Node[SOi1].MT, name + SOi1 + d, 0b0001111);
                        }
                        IO.Write(name + "length=",
                            Data.ObjectHRC[SOi0].Node.Length);
                    }
                    name = "objhrc." + SOi0 + d;

                    if (Data.ObjectHRC[SOi0].Shadow != 0)
                        IO.Write(name + "shadow=", Data.ObjectHRC[SOi0].Shadow);
                    IO.Write(name + "uid_name=", Data.ObjectHRC[SOi0].UIDName);
                }
                IO.Write("objhrc.length=", Data.ObjectHRC.Length);
            }

            if (Data.ObjectHRCList.Length != 0)
            {
                SO0 = SortWriter(Data.ObjectHRCList.Length);
                for (i0 = 0; i0 < Data.ObjectHRCList.Length; i0++)
                    IO.Write("objhrc_list." + SO0[i0] + "=", Data.ObjectHRCList[SO0[i0]]);
                IO.Write("objhrc_list.length=", Data.ObjectHRCList.Length);
            }

            IO.Write("play_control.begin=", Data.PlayControl.Begin);
            if (Data._.CompressF16 != 0 || (Data.PlayControl.Offset > -1 && Data.Header.Signature == 0x5F5F5F43))
                IO.Write("play_control.div=", Data.PlayControl.Div);
            IO.Write("play_control.fps=", Data.PlayControl.FPS);
            if (Data._.CompressF16 != 0 || (Data.PlayControl.Offset > -1 && Data.Header.Signature == 0x5F5F5F43))
                IO.Write("play_control.offset=", Data.PlayControl.Offset);
            IO.Write("play_control.size=", (Data.PlayControl.Size - Data.PlayControl.Offset));

            if (Data.PostProcess.Boolean)
            {
                name = "post_process.";
                Write(ref Data.PostProcess.Ambient  , name, "Ambient");
                Write(ref Data.PostProcess.Diffuse  , name, "Diffuse");
                Write(ref Data.PostProcess.Specular , name, "Specular");
                Write(ref Data.PostProcess.LensFlare, name, "lens_flare");
                Write(ref Data.PostProcess.LensGhost, name, "lens_ghost");
                Write(ref Data.PostProcess.LensShaft, name, "lens_shaft");
            }

            SO0 = SortWriter(Data.Point.Length);
            if (Data.Point.Length != 0)
            {
                for (i0 = 0; i0 < Data.Point.Length; i0++)
                    Write(ref Data.Point[SO0[i0]], "point." + SO0[i0] + d, 0b1111);
                IO.Write("point.length=", Data.Point.Length);
            }

            if (!A3DC)
                IO.Close();
        }
        
        public void Read(ref ModelTransform MT, string Temp)
        {
            KKtMain.FindValue(Dict, Temp + MTBO, ref MT.BinOffset);
            KKtMain.FindValue(Dict, Temp +     "name", ref    MT.Name);
            KKtMain.FindValue(Dict, Temp + "uid_name", ref MT.UIDName);

            Read(ref MT.Rot       , Temp + "rot."       );
            Read(ref MT.Scale     , Temp + "scale."     );
            Read(ref MT.Trans     , Temp + "trans."     );
            Read(ref MT.Visibility, Temp + "visibility.");
        }

        public void Read(ref RGBAKey RGBA, string Temp)
        {
            KKtMain.FindValue(Dict, Temp.Remove(Temp.Length - 1), ref RGBA.Boolean);

            Read(ref RGBA.A, Temp + "a.");
            Read(ref RGBA.B, Temp + "b.");
            Read(ref RGBA.G, Temp + "g.");
            Read(ref RGBA.R, Temp + "r.");

            RGBA.Alpha = RGBA.A.Boolean;
        }
        
        public void Read(ref Vector3Key Key, string Temp)
        { Read(ref Key.X, Temp + "x."); Read(ref Key.Y, Temp + "y."); Read(ref Key.Z, Temp + "z."); }

        public void Read(ref KeyUV UV, string Temp)
        { Read(ref UV.U, Temp + "U" + d); Read(ref UV.V, Temp + "V" + d); }

        public void Read(ref Key Key, string Temp)
        {
            KKtMain.FindValue(Dict, Temp.Remove(Temp.Length - 1), ref Key.Boolean);
            if (KKtMain.FindValue(Dict, Temp + BO, ref Key.BinOffset))
                Key.Boolean = true;
            else if (KKtMain.FindValue(Dict, Temp + "type", ref Key.Type))
                Key.Boolean = true;

            int i = 0;
            int i0 = 0;
            if (Key.Boolean)
            {
                if (Key.Type == 0x0000 || Key.Type == 0x0001)
                    KKtMain.FindValue(Dict, Temp + "value", ref Key.Value);
                else
                {
                    KKtMain.FindValue(Dict, Temp + "ep_type_post", ref Key.EPTypePost);
                    KKtMain.FindValue(Dict, Temp + "ep_type_pre", ref Key.EPTypePre);
                    KKtMain.FindValue(Dict, Temp + "key.length", ref Key.Length);
                    KKtMain.FindValue(Dict, Temp + "max", ref Key.Max);
                    KKtMain.FindValue(Dict, Temp + "raw_data_key_type", ref Key.RawData.KeyType);

                    if (Key.Length != -1)
                    {
                        Key.Trans = new Key.Transform[Key.Length];
                        for (i0 = 0; i0 < Key.Length; i0++)
                            if (KKtMain.FindValue(Dict, Temp + "key." + i0 + ".data", ref value))
                            {
                                dataArray = value.Replace("(", "").Replace(")", "").Split(',');
                                Key.Trans[i0].Frame = KKtMain.ToDouble(dataArray[0]);
                                if (dataArray.Length > 1)
                                {
                                    Key.Trans[i0].Value1 = KKtMain.ToDouble(dataArray[1]);
                                    if (dataArray.Length > 2)
                                    {
                                        Key.Trans[i0].Value2 = KKtMain.ToDouble(dataArray[2]);
                                        if (dataArray.Length > 3)
                                            Key.Trans[i0].Value3 = KKtMain.ToDouble(dataArray[3]);
                                    }
                                }
                                Key.Trans[i0].Type = dataArray.Length - 1;
                            }
                    }
                    else if (Key.RawData.KeyType != -1)
                    {
                        KKtMain.FindValue(Dict, Temp + "raw_data.value_type",
                            ref Key.RawData.ValueType);
                        if (KKtMain.FindValue(Dict, Temp + "raw_data.value_list",
                            ref Key.RawData.ValueListString))
                            Key.RawData.ValueList = Key.RawData.ValueListString.Split(',');
                        KKtMain.FindValue(Dict, Temp + "raw_data.value_list_size",
                            ref Key.RawData.ValueListSize);

                        int DataSize = Key.RawData.KeyType + 1;
                        Key.Length = Key.RawData.ValueListSize / DataSize;
                        Key.Trans = new Key.Transform[Key.Length];
                        for (i = 0; i < Key.Length; i++)
                        {
                            Key.Trans[i].Type = Key.RawData.KeyType;
                            Key.Trans[i].Frame = KKtMain.ToDouble(Key.RawData.ValueList[i * DataSize + 0]);
                            if (Key.Trans[i].Type > 0)
                            {
                                Key.Trans[i].Value1 = KKtMain.
                                    ToDouble(Key.RawData.ValueList[i * DataSize + 1]);
                                if (Key.Trans[i].Type > 1)
                                {
                                    Key.Trans[i].Value2 = KKtMain.
                                        ToDouble(Key.RawData.ValueList[i * DataSize + 2]);
                                    if (Key.Trans[i].Type > 2)
                                        Key.Trans[i].Value3 = KKtMain.
                                            ToDouble(Key.RawData.ValueList[i * DataSize + 3]);
                                }
                            }
                        }
                        Key.RawData = new Key.RawD();
                    }
                }
            }
        }

        public void Write(ref ModelTransform MT, string Temp, byte Flags)
        {
            if (A3DC && !MT.Writed && ((Flags & 0x40) >> 6) == 1)
            {
                IO.Write(Temp + MTBO + "=", MT.BinOffset);
                MT.Writed = true;
            }

            if (((Flags & 0b0100000) >> 5) == 1)
                IO.Write(Temp + "name=", MT.Name);
            if (((Flags & 0b0001000) >> 3) == 1)
                Write(ref MT.Rot  , Temp + "rot."  );
            if (((Flags & 0b0000100) >> 2) == 1)
                Write(ref MT.Scale, Temp + "scale.");
            if (((Flags & 0b0000010) >> 1) == 1)
                Write(ref MT.Trans, Temp + "trans.");
            if (((Flags & 0b0010000) >> 4) == 1)
                IO.Write(Temp + "uid_name=", MT.UIDName);
            if ( (Flags & 0b0000001)       == 1 && !A3DC)
                Write(ref MT.Visibility, Temp + "visibility.");
        }

        public void Write(ref RGBAKey RGBA, string Temp, string Data)
        {
            if (RGBA.Boolean)
            {
                IO.Write(Temp + Data + "=", RGBA.Boolean);
                if (RGBA.Alpha)
                    Write(ref RGBA.A, Temp + Data + ".a.");
                Write(ref RGBA.B, Temp + Data + ".b.");
                Write(ref RGBA.G, Temp + Data + ".g.");
                Write(ref RGBA.R, Temp + Data + ".r.");
            }
        }

        public void Write(ref Vector3Key Key, string Temp)
        { Write(ref Key.X, Temp + "x."); Write(ref Key.Y, Temp + "y."); Write(ref Key.Z, Temp + "z."); }

        public void Write(ref KeyUV UV, string Temp, string Data)
        { Write(ref UV.U, Temp, Data + "U"); Write(ref UV.V, Temp, Data + "V"); }

        public void Write(ref Key Key, string Temp, string Data)
        {
            if (Key.Boolean)
            {
                IO.Write(Temp + Data + "=", Key.Boolean);
                Write(ref Key, Temp + Data + d);
            }
        }

        public void Write(ref Key Key, string Temp)
        {
            if (Key.Boolean)
            {
                if (A3DC)
                    IO.Write(Temp + BO + "=", Key.BinOffset);
                else
                {
                    int i = 0;
                    if (Key.Trans != null)
                    {
                        int[] SO = SortWriter(Key.Trans.Length);
                        if (Key.EPTypePost != -1)
                            IO.Write(Temp + "ep_type_post=", Key.EPTypePost);
                        if (Key.EPTypePre != -1)
                            IO.Write(Temp + "ep_type_pre=", Key.EPTypePre);
                        if (Key.Length < 1000)
                        {
                            for (i = 0; i < Key.Trans.Length; i++)
                            {
                                IO.Write(Temp + "key." + SO[i] + ".data=");
                                if (Key.Trans[SO[i]].Type > 0)
                                    IO.Write("(");
                                IO.Write(KKtMain.ToString(Key.Trans[SO[i]].Frame));
                                if (Key.Trans[SO[i]].Type > 0)
                                {
                                    IO.Write("," + KKtMain.ToString(Key.Trans[SO[i]].Value1));
                                    if (Key.Trans[SO[i]].Type > 1)
                                    {
                                        IO.Write("," + KKtMain.ToString(Key.Trans[SO[i]].Value2));
                                        if (Key.Trans[SO[i]].Type > 2)
                                            IO.Write("," + KKtMain.ToString(Key.Trans[SO[i]].Value3));
                                    }
                                    IO.Write(")");
                                }
                                IO.Write("\n");
                            }
                            IO.Write(Temp + "key.length=", Key.Length);
                        }
                        if (Key.Max != -1)
                            IO.Write(Temp + "max=", Key.Max);
                        if (Key.Length >= 1000)
                        {
                            Key.RawData = new Key.RawD();
                            for (i = 0; i < Key.Trans.Length; i++)
                                if (Key.RawData.KeyType < Key.Trans[i].Type)
                                    Key.RawData.KeyType = Key.Trans[i].Type;
                            Key.RawData.ValueListSize = Key.Trans.Length * (Key.RawData.KeyType + 1);
                            IO.Write(Temp + "raw_data.value_list=");
                            for (i = 0; i < Key.Trans.Length; i++)
                            {
                                IO.Write(KKtMain.ToString(Key.Trans[i].Frame));
                                if (Key.RawData.KeyType > 0)
                                {
                                    IO.Write("," + KKtMain.ToString(Key.Trans[i].Value1));
                                    if (Key.RawData.KeyType > 1)
                                    {
                                        IO.Write("," + KKtMain.ToString(Key.Trans[i].Value2));
                                        if (Key.RawData.KeyType > 2)
                                            IO.Write("," + KKtMain.ToString(Key.Trans[i].Value3));
                                    }
                                }
                                if (i < Key.Trans.Length - 1)
                                    IO.Write(',');
                            }
                            IO.Write('\n');
                            IO.Write(Temp + "raw_data.value_list_size=", Key.RawData.ValueListSize);
                            IO.Write(Temp + "raw_data.value_type=", Key.RawData.ValueType);
                            IO.Write(Temp + "raw_data_key_type=", Key.RawData.KeyType);
                            Key.RawData = new Key.RawD();
                        }
                        if (Key.Type == 0x3002)
                            IO.Write(Temp + "type=2\n");
                        else
                        {
                            if (Key.Type <= 0x0003)
                                IO.Write(Temp + "type=", Key.Type);
                            else
                                IO.Write(Temp + "type=", "2");
                        }
                    }
                    else
                    {
                        if (Key.Type == 0x0000)
                            IO.Write(Temp + "type=0\n");
                        else
                        {
                            IO.Write(Temp + "type=1\n");
                            IO.Write(Temp + "value=", KKtMain.ToString(Key.Value));
                        }
                    }
                }
            }
        }

        private void A3DCReader()
        {
            int i0 = 0;
            int i1 = 0;

            if (Data.Camera.Auxiliary.Boolean)
            {
                Read(ref Data.Camera.Auxiliary.AutoExposure);
                Read(ref Data.Camera.Auxiliary.    Exposure);
                Read(ref Data.Camera.Auxiliary.Gamma       );
                Read(ref Data.Camera.Auxiliary.GammaRate   );
                Read(ref Data.Camera.Auxiliary.Saturate    );
            }

            for (i0 = 0; i0 < Data.Camera.Root.Length; i0++)
            {
                Read(ref Data.Camera.Root[i0]          .MT         );
                Read(ref Data.Camera.Root[i0].Interest             );
                Read(ref Data.Camera.Root[i0].ViewPoint.MT         );
                Read(ref Data.Camera.Root[i0].ViewPoint.FocalLength);
                Read(ref Data.Camera.Root[i0].ViewPoint.FOV        );
                Read(ref Data.Camera.Root[i0].ViewPoint.Roll       );
            }

            for (i0 = 0; i0 < Data.Chara.Length; i0++)
                Read(ref Data.Chara[i0]);

            for (i0 = 0; i0 < Data.Curve.Length; i0++)
                Read(ref Data.Curve[i0].CV);

            if (Data.DOF.Name != null)
                Read(ref Data.DOF);

            for (i0 = 0; i0 < Data.Fog.Length; i0++)
            {
                Read(ref Data.Fog[i0].Density);
                Read(ref Data.Fog[i0].Diffuse);
                Read(ref Data.Fog[i0].End    );
                Read(ref Data.Fog[i0].Start  );
            }

            for (i0 = 0; i0 < Data.Light.Length; i0++)
            {
                Read(ref Data.Light[i0].Ambient      );
                Read(ref Data.Light[i0].Diffuse      );
                Read(ref Data.Light[i0].Incandescence);
                Read(ref Data.Light[i0].Position     );
                Read(ref Data.Light[i0].Specular     );
                Read(ref Data.Light[i0].SpotDirection);
            }

            for (i0 = 0; i0 < Data.MObjectHRC.Length; i0++)
                if (Data.MObjectHRC[i0].Node != null)
                    for (i1 = 0; i1 < Data.MObjectHRC[i0].Node.Length; i1++)
                        Read(ref Data.MObjectHRC[i0].Node[i1].MT);

            for (i0 = 0; i0 < Data.Object.Length; i0++)
            {
                Read(ref Data.Object[i0].MT);
                if (Data.Object[i0].TT == null)
                    continue;

                for (i1 = 0; i1 < Data.Object[i0].TT.Length; i1++)
                {
                    Read(ref Data.Object[i0].TT[i1].C );
                    Read(ref Data.Object[i0].TT[i1].O );
                    Read(ref Data.Object[i0].TT[i1].R );
                    Read(ref Data.Object[i0].TT[i1].Ro);
                    Read(ref Data.Object[i0].TT[i1].RF);
                    Read(ref Data.Object[i0].TT[i1].TF);
                }
            }

            for (i0 = 0; i0 < Data.ObjectHRC.Length; i0++)
            {
                if (Data.ObjectHRC[i0].Node == null)
                    continue;

                for (i1 = 0; i1 < Data.ObjectHRC[i0].Node.Length; i1++)
                    Read(ref Data.ObjectHRC[i0].Node[i1].MT);
            }


            for (i0 = 0; i0 < Data.Point.Length; i0++)
                Read(ref Data.Point[i0]);

            if (Data.PostProcess.Boolean)
            {
                Read(ref Data.PostProcess.Ambient  );
                Read(ref Data.PostProcess.Diffuse  );
                Read(ref Data.PostProcess.Specular );
                Read(ref Data.PostProcess.LensFlare);
                Read(ref Data.PostProcess.LensGhost);
                Read(ref Data.PostProcess.LensShaft);
            }
        }

        public void A3DCWriter(string file)
        {
            A3DC = true;

            int i0 = 0;
            int i1 = 0;
            if (A3DCOpt)
                UsedValues = new Values { BinOffset = new List<int>(), Value = new List<double>() };
            DateTime date = DateTime.Now;
            Data.Head = new Header();

            if (Data._.CompressF16 != 0)
            {
                int a = int.Parse(Data._.ConverterVersion);
                int b = BitConverter.ToInt32(KKtText.ToUTF8(Data._.FileName), 0);
                int c = int.Parse(Data._.PropertyVersion);
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
                    if (Data.Camera.Auxiliary.Boolean)
                    {
                        Write(ref Data.Camera.Auxiliary.AutoExposure);
                        Write(ref Data.Camera.Auxiliary.Exposure    );
                        Write(ref Data.Camera.Auxiliary.Gamma       );
                        Write(ref Data.Camera.Auxiliary.GammaRate   );
                        Write(ref Data.Camera.Auxiliary.Saturate    );
                    }

                    for (i0 = 0; i0 < Data.Camera.Root.Length; i0++)
                    {
                        Write(ref Data.Camera.Root[i0]          .MT         );
                        Write(ref Data.Camera.Root[i0].ViewPoint.MT         );
                        Write(ref Data.Camera.Root[i0].Interest             );
                        Write(ref Data.Camera.Root[i0].ViewPoint.FOV        );
                        Write(ref Data.Camera.Root[i0].ViewPoint.Roll       );
                        Write(ref Data.Camera.Root[i0].ViewPoint.FocalLength);
                    }

                    for (i0 = 0; i0 < Data.Chara.Length; i0++)
                        Write(ref Data.Chara[i0]);

                    for (i0 = 0; i0 < Data.Curve.Length; i0++)
                        Write(ref Data.Curve[i0].CV);

                    if (Data.DOF.Name != null)
                        Write(ref Data.DOF);

                    for (i0 = 0; i0 < Data.Fog.Length; i0++)
                    {
                        Write(ref Data.Fog[i0].Density);
                        Write(ref Data.Fog[i0].Diffuse);
                        Write(ref Data.Fog[i0].End    );
                        Write(ref Data.Fog[i0].Start  );
                    }

                    for (i0 = 0; i0 < Data.Light.Length; i0++)
                    {
                        Write(ref Data.Light[i0].Ambient      );
                        Write(ref Data.Light[i0].Diffuse      );
                        Write(ref Data.Light[i0].Incandescence);
                        Write(ref Data.Light[i0].Specular     );
                        Write(ref Data.Light[i0].Position     );
                        Write(ref Data.Light[i0].SpotDirection);
                    }

                    for (i0 = 0; i0 < Data.MObjectHRC.Length; i0++)
                    {
                        if (Data.MObjectHRC[i0].Instance != null)
                            for (i1 = 0; i1 < Data.MObjectHRC[i0].Instance.Length; i1++)
                                Write(ref Data.MObjectHRC[i0].Instance[i1]);

                        if (Data.MObjectHRC[i0].Node != null)
                            for (i1 = 0; i1 < Data.MObjectHRC[i0].Node.Length; i1++)
                                Write(ref Data.MObjectHRC[i0].Node[i1].MT);
                    }

                    for (i0 = 0; i0 < Data.Object.Length; i0++)
                    {
                        Write(ref Data.Object[i0].MT);
                        if (Data.Object[i0].TT != null)
                            for (i1 = 0; i1 < Data.Object[i0].TT.Length; i1++)
                            {
                                Write(ref Data.Object[i0].TT[i1].C );
                                Write(ref Data.Object[i0].TT[i1].O );
                                Write(ref Data.Object[i0].TT[i1].R );
                                Write(ref Data.Object[i0].TT[i1].Ro);
                                Write(ref Data.Object[i0].TT[i1].RF);
                                Write(ref Data.Object[i0].TT[i1].TF);
                            }
                    }

                    for (i0 = 0; i0 < Data.ObjectHRC.Length; i0++)
                    {
                        if (Data.ObjectHRC[i0].Node == null)
                            continue;

                        for (i1 = 0; i1 < Data.ObjectHRC[i0].Node.Length; i1++)
                            Write(ref Data.ObjectHRC[i0].Node[i1].MT);
                    }

                    for (i0 = 0; i0 < Data.Point.Length; i0++)
                        Write(ref Data.Point[i0]);

                    if (Data.PostProcess.Boolean)
                    {
                        Write(ref Data.PostProcess.Ambient  );
                        Write(ref Data.PostProcess.Diffuse  );
                        Write(ref Data.PostProcess.Specular );
                        Write(ref Data.PostProcess.LensFlare);
                        Write(ref Data.PostProcess.LensGhost);
                        Write(ref Data.PostProcess.LensShaft);
                    }
                }
                IO.Seek(0x00, 0);

                for (i0 = 0; i0 < Data.Camera.Root.Length; i0++)
                {
                    WriteOffset(ref Data.Camera.Root[i0].Interest    , ReturnToOffset);
                    WriteOffset(ref Data.Camera.Root[i0]          .MT, ReturnToOffset);
                    WriteOffset(ref Data.Camera.Root[i0].ViewPoint.MT, ReturnToOffset);
                }

                if (Data.DOF.Name != null)
                    WriteOffset(ref Data.DOF, ReturnToOffset);

                for (i0 = 0; i0 < Data.Light.Length; i0++)
                {
                    WriteOffset(ref Data.Light[i0].Position     , ReturnToOffset);
                    WriteOffset(ref Data.Light[i0].SpotDirection, ReturnToOffset);
                }

                for (i0 = 0; i0 < Data.MObjectHRC.Length; i0++)
                {
                    if (Data.MObjectHRC[i0].Node == null)
                        continue;

                    for (i1 = 0; i1 < Data.MObjectHRC[i0].Node.Length; i1++)
                        WriteOffset(ref Data.MObjectHRC[i0].Node[i1].MT, ReturnToOffset);
                }

                for (i0 = 0; i0 < Data.Object.Length; i0++)
                    WriteOffset(ref Data.Object[i0].MT, ReturnToOffset);


                for (i0 = 0; i0 < Data.MObjectHRC.Length; i0++)
                {
                    if (Data.ObjectHRC[i0].Node == null)
                        continue;

                    for (i1 = 0; i1 < Data.ObjectHRC[i0].Node.Length; i1++)
                        WriteOffset(ref Data.ObjectHRC[i0].Node[i1].MT, ReturnToOffset);
                }

                if (!ReturnToOffset)
                    IO.Align(0x10, true);
            }

            byte[] A3DCData = IO.ToArray(true);

            IO = new KKtIO(new MemoryStream());
            A3DAWriter(file);
            byte[] A3DAData = IO.ToArray(true);

            IO = KKtIO.OpenWriter(file + ".a3da");
            IO.Seek(Offset, 0);

            Data.Head.StringOffset = (int)(IO.Position - A3DCStart);
            IO.Write(A3DAData);
            Data.Head.StringLength = (int)(IO.Position - A3DCStart - Data.Head.StringOffset);
            IO.Align(0x20, true);

            Data.Head.BinaryOffset = (int)(IO.Position - A3DCStart);
            IO.Write(A3DCData);
            Data.Head.BinaryLength = (int)(IO.Position - A3DCStart - Data.Head.BinaryOffset);
            IO.Align(0x10, true);

            int A3DCEnd = (int)IO.Position;

            if (Data._.CompressF16 != 0)
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
            IO.Write(Data.Head.StringOffset, true, true);
            IO.Write(Data.Head.StringLength, true, true);
            IO.Write(0x01, true, true);
            IO.Write(0x4C42);
            IO.Write(Data.Head.BinaryOffset, true, true);
            IO.Write(Data.Head.BinaryLength, true, true);
            IO.Write(0x20, true, true);
            if (Data._.CompressF16 != 0)
            {
                IO.Seek(A3DCEnd, 0);
                IO.WriteEOFC(0);
            }

            IO.Close();
        }

        public void Read(ref ModelTransform MT)
        {
            if (MT.BinOffset > -1)
            {
                IO.Seek(Offset + MT.BinOffset, 0);
                Read(ref MT.Scale     );
                Read(ref MT.Rot       );
                Read(ref MT.Trans     );
                Read(ref MT.Visibility);
            }
        }

        public void Read(ref RGBAKey RGBA)
        {
            long CurrentOffset = IO.Position;
            Read(ref RGBA.R);
            Read(ref RGBA.G);
            Read(ref RGBA.B);
            if (RGBA.Alpha)
                Read(ref RGBA.A);
            RGBA.Boolean = RGBA.R.Boolean || RGBA.G.Boolean || RGBA.B.Boolean || RGBA.A.Boolean;
            RGBA.Alpha = RGBA.A.Boolean;
            IO.Seek(CurrentOffset, 0);
        }

        public void Read(ref Vector3Key Key)
        {
            Key.X.BinOffset = IO.ReadInt32();
            Key.Y.BinOffset = IO.ReadInt32();
            Key.Z.BinOffset = IO.ReadInt32();
            long CurrentOffset = IO.Position;
            Read(ref Key.X);
            Read(ref Key.Y);
            Read(ref Key.Z);
            IO.Seek(CurrentOffset, 0);
        }

        public void Read(ref KeyUV UV)
        { Read(ref UV.U); Read(ref UV.V); }

        public void Read(ref Key Key) => Read(ref Key, false);

        public void Read(ref Key Key, bool CompressF16)
        {
            if (Key.BinOffset > -1)
            {
                IO.Seek(Offset + Key.BinOffset, 0);
                Key.Boolean = true;
                Key.Type = IO.ReadInt32();

                Key.Value = IO.ReadSingle();
                if (Key.Type != 0x0000 && Key.Type != 0x0001)
                {
                    Key.Max = IO.ReadSingle();
                    Key.Length = IO.ReadInt32();
                    Key.Trans = new Key.Transform[Key.Length];
                    for (int i = 0; i < Key.Length; i++)
                    {
                        Key.Trans[i].Type = 3;
                        if (CompressF16 && Data._.CompressF16 > 0)
                        {
                            Key.Trans[i].Frame = IO.ReadUInt16();
                            Key.Trans[i].Value1 = IO.ReadHalf();
                        }
                        else
                        {
                            Key.Trans[i].Frame = IO.ReadSingle();
                            Key.Trans[i].Value1 = IO.ReadSingle();
                        }

                        if (CompressF16 && Data._.CompressF16 == 2)
                        {
                            Key.Trans[i].Value2 = IO.ReadHalf();
                            Key.Trans[i].Value3 = IO.ReadHalf();
                        }
                        else
                        {
                            Key.Trans[i].Value2 = IO.ReadSingle();
                            Key.Trans[i].Value3 = IO.ReadSingle();
                        }

                        if (Key.Trans[i].Value3 == 0)
                        {
                            Key.Trans[i].Type--;
                            if (Key.Trans[i].Value2 == 0)
                            {
                                Key.Trans[i].Type--;
                                if (Key.Trans[i].Value1 == 0)
                                    Key.Trans[i].Type--;
                            }
                        }
                    }
                }
            }
        }

        public void Write(ref ModelTransform MT)
        { Write(ref MT.Scale); Write(ref MT.Rot, true); Write(ref MT.Trans); Write(ref MT.Visibility); }

        public void Write(ref RGBAKey RGBA)
        {
            if (RGBA.Boolean)
            {
                Write(ref RGBA.R); Write(ref RGBA.G);
                Write(ref RGBA.B); if (RGBA.Alpha) Write(ref RGBA.A);
            }
        }

        public void Write(ref Vector3Key Key) => Write(ref Key, false);

        public void Write(ref Vector3Key Key, bool CompressF16)
        { Write(ref Key.X, CompressF16); Write(ref Key.Y, CompressF16); Write(ref Key.Z, CompressF16); }
        
        public void Write(ref KeyUV UV)
        { Write(ref UV.U); Write(ref UV.V); }

        public void Write(ref Key Key) => Write(ref Key, false);

        public void Write(ref Key Key, bool CompressF16)
        {
            int i = 0;
            if (Key.Trans != null && Key.Boolean)
            {
                Key.BinOffset = (int)IO.Position;
                IO.Write(Key.Type);
                IO.Write(0x00);
                IO.Write((float)Key.Max);
                IO.Write(Key.Trans.Length);
                for (i = 0; i < Key.Trans.Length; i++)
                {
                    if (CompressF16 && Data._.CompressF16 > 0)
                    {
                        IO.Write((ushort)Key.Trans[i].Frame);
                        IO.Write(KKtMain.ToHalf(Key.Trans[i].Value1));
                    }
                    else
                    {
                        IO.Write((float)Key.Trans[i].Frame);
                        IO.Write((float)Key.Trans[i].Value1);
                    }
                    if (CompressF16 && Data._.CompressF16 == 2)
                    {
                        IO.Write(KKtMain.ToHalf(Key.Trans[i].Value2));
                        IO.Write(KKtMain.ToHalf(Key.Trans[i].Value3));
                    }
                    else
                    {
                        IO.Write((float)Key.Trans[i].Value2);
                        IO.Write((float)Key.Trans[i].Value3);
                    }
                }
            }
            else if (Key.Boolean)
            {
                if (UsedValues.Value.Contains(Key.Value) && A3DCOpt)
                {
                    for (i = 0; i < UsedValues.Value.Count; i++)
                        if (UsedValues.Value[i] == Key.Value)
                            Key.BinOffset = UsedValues.BinOffset[i];
                }
                else
                {
                    Key.BinOffset = (int)IO.Position;
                    if (Key.Type != 0x00 || Key.Value != 0)
                    {
                        IO.Write(0x01);
                        IO.Write((float)Key.Value);
                    }
                    else
                    {
                        IO.Write((long)0x00);
                    }
                    if (A3DCOpt)
                    {
                        UsedValues.BinOffset.Add(Key.BinOffset);
                        UsedValues.Value.Add(Key.Value);
                    }
                }
            }
        }

        public void WriteOffset(ref ModelTransform MT, bool ReturnToOffset)
        {
            if (ReturnToOffset)
                IO.Seek(MT.BinOffset, 0);
            else
                MT.BinOffset = (int)IO.Position;

            WriteOffset(ref MT.Scale);
            WriteOffset(ref MT.Rot  );
            WriteOffset(ref MT.Trans);
            IO.Write(MT.Visibility.BinOffset);
            IO.Align(0x10);
        }

        public void WriteOffset(ref Vector3Key Key)
        {
            IO.Write(Key.X.BinOffset);
            IO.Write(Key.Y.BinOffset);
            IO.Write(Key.Z.BinOffset);
        }

        public void XMLReader(string file)
        {
            name = "";
            Offset = 0;
            nameInt = "";
            nameView = "";
            dataArray = new string[4];
            Dict = new Dictionary<string, object>();
            Data = new A3DAData();

            int i0 = 0;
            int i1 = 0;
            Xml = new KKtXml();
            Xml.OpenXml(file + ".xml", true);
            foreach (XElement A3D in Xml.doc.Elements("A3D"))
            {
                foreach (XAttribute Entry in A3D.Attributes())
                    if (Entry.Name == "Base64")
                        Base64 = bool.Parse(Entry.Value);
                    else if (Entry.Name == "Signature")
                        Data.Header.Signature = BitConverter.ToInt32(KKtText.ToASCII(Entry.Value), 0);

                foreach (XElement Child0 in A3D.Elements())
                {
                    if (Child0.Name == "_")
                    {
                        foreach (XAttribute Entry in Child0.Attributes())
                        {
                            if (Data.Header.Signature == 0x5F5F5F43)
                                Xml.Reader(Entry, ref Data._.CompressF16, "CompressF16");
                            Xml.Reader(Entry, ref Data._.ConverterVersion, "ConverterVersion");
                            Xml.Reader(Entry, ref Data._.FileName, "FileName");
                            Xml.Reader(Entry, ref Data._.PropertyVersion, "PropertyVersion");
                        }
                    }
                    else if (Child0.Name == "Camera")
                    {
                        foreach (XElement Camera in Child0.Elements())
                        {
                            if (Camera.Name == "Auxiliary")
                            {
                                Data.Camera.Auxiliary.Boolean = true;
                                foreach (XElement Auxiliary in Camera.Elements())
                                {
                                    Read(ref Data.Camera.Auxiliary.AutoExposure, Auxiliary, "AutoExposure");
                                    Read(ref Data.Camera.Auxiliary.    Exposure, Auxiliary,     "Exposure");
                                    Read(ref Data.Camera.Auxiliary.Gamma       , Auxiliary, "Gamma"       );
                                    Read(ref Data.Camera.Auxiliary.GammaRate   , Auxiliary, "GammaRate"   );
                                    Read(ref Data.Camera.Auxiliary.Saturate    , Auxiliary, "Saturate"    );
                                }
                            }
                            else if (Camera.Name == "Root")
                            {
                                foreach (XAttribute Entry in Camera.Attributes())
                                    if (Entry.Name == "Length")
                                        Data.Camera.Root = new CameraRoot[int.Parse(Entry.Value)];

                                i0 = 0;
                                foreach (XElement Root in Camera.Elements())
                                {
                                    if (Root.Name == "RootEntry")
                                        Data.Camera.Root[i0] = new CameraRoot();

                                    Read(ref Data.Camera.Root[i0].MT, Root);
                                    foreach (XElement CameraRoot in Root.Elements())
                                    {
                                        if (CameraRoot.Name == "Interest")
                                            Read(ref Data.Camera.Root[i0].Interest, CameraRoot);
                                        else if (CameraRoot.Name == "ViewPoint")
                                        {
                                            foreach (XAttribute Entry in CameraRoot.Attributes())
                                            {
                                                Xml.Reader(Entry, ref Data.Camera.Root[i0].
                                                    ViewPoint.Aspect         , "Aspect"         );
                                                Xml.Reader(Entry, ref Data.Camera.Root[i0].
                                                    ViewPoint.CameraApertureH, "CameraApertureH");
                                                Xml.Reader(Entry, ref Data.Camera.Root[i0].
                                                    ViewPoint.CameraApertureW, "CameraApertureW");
                                                Xml.Reader(Entry, ref Data.Camera.Root[i0].
                                                    ViewPoint.FOVHorizontal  , "FOVHorizontal"  );
                                            }

                                            foreach (XElement ViewPoint in CameraRoot.Elements())
                                            {
                                                Read(ref Data.Camera.Root[i0].ViewPoint.
                                                    FocalLength, ViewPoint, "FocalLength");
                                                Read(ref Data.Camera.Root[i0].ViewPoint.
                                                    FOV        , ViewPoint, "FOV"        );
                                                Read(ref Data.Camera.Root[i0].ViewPoint.
                                                    Roll       , ViewPoint, "Roll"       );
                                            }

                                            Read(ref Data.Camera.Root[i0].ViewPoint.MT, CameraRoot);
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
                                Data.Chara = new ModelTransform[int.Parse(Entry.Value)];

                        i0 = 0;
                        foreach (XElement Charas in Child0.Elements())
                            if (Read(ref Data.Chara[i0], Charas, "Chara"))
                                i0++;
                    }
                    else if (Child0.Name == "Curves")
                    {
                        foreach (XAttribute Entry in Child0.Attributes())
                            if (Entry.Name == "Length")
                                Data.Curve = new Curve[int.Parse(Entry.Value)];

                        i0 = 0;
                        foreach (XElement Curves in Child0.Elements())
                            if (Curves.Name == "Curve")
                            {
                                Data.Curve[i0] = new Curve();
                                foreach (XAttribute Entry in Curves.Attributes())
                                    Xml.Reader(Entry, ref Data.Curve[i0].Name, "Name");

                                foreach (XElement Curve in Curves.Elements())
                                    Read(ref Data.Curve[i0].CV, Curve, "CV");
                                i0++;
                            }
                    }
                    else if (Child0.Name == "DOF")
                        Read(ref Data.DOF, Child0);
                    else if (Child0.Name == "Events")
                    {
                        foreach (XAttribute Entry in Child0.Attributes())
                            if (Entry.Name == "Length")
                                Data.Event = new Event[int.Parse(Entry.Value)];

                        i0 = 0;
                        foreach (XElement Events in Child0.Elements())
                        {
                            if (Events.Name != "Event")
                                continue;

                            foreach (XAttribute Entry in Events.Attributes())
                            {
                                Xml.Reader(Entry, ref Data.Event[i0].Begin       , "Begin"       );
                                Xml.Reader(Entry, ref Data.Event[i0].ClipBegin   , "ClipBegin"   );
                                Xml.Reader(Entry, ref Data.Event[i0].ClipEnd     , "ClipEnd"     );
                                Xml.Reader(Entry, ref Data.Event[i0].End         , "End"         );
                                Xml.Reader(Entry, ref Data.Event[i0].Name        , "Name"        );
                                Xml.Reader(Entry, ref Data.Event[i0].Param1      , "Param1"      );
                                Xml.Reader(Entry, ref Data.Event[i0].Ref         , "Ref"         );
                                Xml.Reader(Entry, ref Data.Event[i0].TimeRefScale, "TimeRefScale");
                                Xml.Reader(Entry, ref Data.Event[i0].Type        , "Type"        );
                            }
                            i0++;
                        }
                    }
                    else if (Child0.Name == "Fogs")
                    {
                        foreach (XAttribute Entry in Child0.Attributes())
                            if (Entry.Name == "Length")
                                Data.Fog = new Fog[int.Parse(Entry.Value)];

                        i0 = 0;
                        foreach (XElement Fogs in Child0.Elements())
                        {
                            if (Fogs.Name != "Fog")
                                continue;

                            Data.Fog[i0] = new Fog();
                            foreach (XAttribute Entry in Fogs.Attributes())
                                Xml.Reader(Entry, ref Data.Fog[i0].Id, "Id");
                            foreach (XElement Fog in Fogs.Elements())
                            {
                                Read(ref Data.Fog[i0].Diffuse, Fog, "Diffuse");
                                Read(ref Data.Fog[i0].Density, Fog, "Density");
                                Read(ref Data.Fog[i0].End    , Fog, "End"    );
                                Read(ref Data.Fog[i0].Start  , Fog, "Start"  );

                            }
                            i0++;
                        }
                    }
                    else if (Child0.Name == "Lights")
                    {
                        foreach (XAttribute Entry in Child0.Attributes())
                            if (Entry.Name == "Length")
                                Data.Light = new Light[int.Parse(Entry.Value)];

                        i0 = 0;
                        foreach (XElement Lights in Child0.Elements())
                        {
                            if (Lights.Name != "Light")
                                continue;

                            Data.Light[i0] = new Light();
                            foreach (XAttribute Entry in Lights.Attributes())
                            {
                                Xml.Reader(Entry, ref Data.Light[i0].Id, "Id");
                                Xml.Reader(Entry, ref Data.Light[i0].Name, "Name");
                                Xml.Reader(Entry, ref Data.Light[i0].Type, "Type");
                            }

                            foreach (XElement Light in Lights.Elements())
                            {
                                Read(ref Data.Light[i0].Ambient      , Light, "Ambient"      );
                                Read(ref Data.Light[i0].Diffuse      , Light, "Diffuse"      );
                                Read(ref Data.Light[i0].Incandescence, Light, "Incandescence");
                                Read(ref Data.Light[i0].Position     , Light, "Position"     );
                                Read(ref Data.Light[i0].Specular     , Light, "Specular"     );
                                Read(ref Data.Light[i0].SpotDirection, Light, "SpotDirection");
                            }
                            i0++;
                        }
                    }
                    else if (Child0.Name == "MObjectsHRC")
                    {
                        foreach (XAttribute Entry in Child0.Attributes())
                            if (Entry.Name == "Length")
                                Data.MObjectHRC = new MObjectHRC[int.Parse(Entry.Value)];

                        i0 = 0;
                        foreach (XElement MObjectsHRC in Child0.Elements())
                        {
                            if (MObjectsHRC.Name == "MObjectHRC")
                                continue;

                            Data.MObjectHRC[i0] = new MObjectHRC();
                            foreach (XAttribute Entry in MObjectsHRC.Attributes())
                                Xml.Reader(Entry, ref Data.MObjectHRC[i0].Name, "Name");
                            foreach (XElement MObjectHRC in MObjectsHRC.Elements())
                            {
                                if (MObjectHRC.Name == "Instances")
                                {
                                    foreach (XAttribute Entry in MObjectHRC.Attributes())
                                        if (Entry.Name == "Length")
                                            Data.MObjectHRC[i0].Instance =
                                                new ModelTransform[int.Parse(Entry.Value)];

                                    i1 = 0;
                                    foreach (XElement Instances in MObjectHRC.Elements())
                                        if (Read(ref Data.MObjectHRC[i0].Instance[i1], Instances, "Instance"))
                                            i1++;
                                }
                                else if (MObjectHRC.Name == "Nodes")
                                {
                                    foreach (XAttribute Entry in MObjectHRC.Attributes())
                                        if (Entry.Name == "Length")
                                            Data.MObjectHRC[i0].Node =
                                                new Node[int.Parse(Entry.Value)];

                                    i1 = 0;
                                    foreach (XElement Nodes in MObjectHRC.Elements())
                                    {
                                        if (Nodes.Name == "Node")
                                            continue;

                                        foreach (XAttribute Entry in Nodes.Attributes())
                                        {
                                            Xml.Reader(Entry, ref Data.MObjectHRC[i0].Node[i1].Name, "Name");
                                            Xml.Reader(Entry, ref Data.MObjectHRC[i0].Node[i1].Parent, "Parent");
                                        }

                                        Read(ref Data.MObjectHRC[i0].Node[i1].MT, Nodes);
                                        i1++;
                                    }
                                }
                            }

                            i0++;
                        }
                    }
                    else if (Child0.Name == "MObjectHRCList")
                    {
                        foreach (XAttribute Entry in Child0.Attributes())
                            if (Entry.Name == "Length")
                                Data.MObjectHRCList = new string[int.Parse(Entry.Value)];

                        i0 = 0;
                        foreach (XElement MObjectList in Child0.Elements())
                            if (MObjectList.Name == "String")
                            {
                                foreach (XAttribute Entry in MObjectList.Attributes())
                                    Xml.Reader(Entry, ref Data.MObjectHRCList[i0], "Name");
                                i0++;
                            }
                    }
                    else if (Child0.Name == "Motion")
                    {
                        foreach (XAttribute Entry in Child0.Attributes())
                            if (Entry.Name == "Length")
                                Data.Motion = new string[int.Parse(Entry.Value)];

                        i0 = 0;
                        foreach (XElement Motion in Child0.Elements())
                            if (Motion.Name == "String")
                            {
                                foreach (XAttribute Entry in Motion.Attributes())
                                    Xml.Reader(Entry, ref Data.Motion[i0], "Name");
                                i0++;
                            }
                    }
                    else if (Child0.Name == "Objects")
                    {
                        foreach (XAttribute Entry in Child0.Attributes())
                            if (Entry.Name == "Length")
                                Data.Object = new Object[int.Parse(Entry.Value)];

                        i0 = 0;
                        foreach (XElement Objects in Child0.Elements())
                        {
                            if (Objects.Name != "Object")
                                continue;

                            Data.Object[i0] = new Object();
                            foreach (XAttribute Entry in Objects.Attributes())
                            {
                                Xml.Reader(Entry, ref Data.Object[i0].Morph, "Morph");
                                Xml.Reader(Entry, ref Data.Object[i0].MorphOffset, "MorphOffset");
                                Xml.Reader(Entry, ref Data.Object[i0].ParentName, "ParentName");
                            }

                            foreach (XElement Object in Objects.Elements())
                            {
                                if (Object.Name == "TexTransforms" || Object.Name == "TTs")
                                {
                                    foreach (XAttribute Entry in Object.Attributes())
                                        if (Entry.Name == "Length")
                                            Data.Object[i0].TT = new TextureTransform[int.Parse(Entry.Value)];

                                    i1 = 0;
                                    foreach (XElement TexTransforms in Object.Elements())
                                    {
                                        if (TexTransforms.Name != "TexTransform" && TexTransforms.Name != "TT")
                                            continue;

                                        Data.Object[i0].TT[i1] = new TextureTransform();
                                        foreach (XAttribute Entry in TexTransforms.Attributes())
                                            Xml.Reader(Entry, ref Data.Object[i0].TT[i1].Name, "Name");

                                        foreach (XElement TexTrans in TexTransforms.Elements())
                                        {
                                            Read(ref Data.Object[i0].TT[i1].C , TexTrans, "C" );
                                            Read(ref Data.Object[i0].TT[i1].O , TexTrans, "O" );
                                            Read(ref Data.Object[i0].TT[i1].R , TexTrans, "R" );
                                            Read(ref Data.Object[i0].TT[i1].Ro, TexTrans, "Ro");
                                            Read(ref Data.Object[i0].TT[i1].RF, TexTrans, "RF");
                                            Read(ref Data.Object[i0].TT[i1].TF, TexTrans, "TF");
                                        }
                                        i1++;
                                    }
                                }

                                if (Object.Name != "TexPats" && Object.Name != "TPs")
                                    continue;

                                foreach (XAttribute Entry in Object.Attributes())
                                    if (Entry.Name == "Length")
                                        Data.Object[i0].TP = new TexturePattern[int.Parse(Entry.Value)];

                                i1 = 0;
                                foreach (XElement TexPats in Object.Elements())
                                {
                                    if (TexPats.Name != "TexPat" && Object.Name != "TP")
                                        continue;

                                    foreach (XAttribute Entry in TexPats.Attributes())
                                    {
                                        Data.Object[i0].TP[i1] = new TexturePattern();
                                        Xml.Reader(Entry, ref Data.Object[i0].TP[i1].Name     , "Name"     );
                                        Xml.Reader(Entry, ref Data.Object[i0].TP[i1].Pat      , "Pat"      );
                                        Xml.Reader(Entry, ref Data.Object[i0].TP[i1].PatOffset, "PatOffset");
                                    }

                                    i1++;
                                }
                            }

                            Read(ref Data.Object[i0].MT, Objects);
                            i0++;
                        }
                    }
                    else if (Child0.Name == "ObjectsHRC")
                    {
                        foreach (XAttribute Entry in Child0.Attributes())
                            if (Entry.Name == "Length")
                                Data.ObjectHRC = new ObjectHRC[int.Parse(Entry.Value)];

                        i0 = 0;
                        foreach (XElement ObjectsHRC in Child0.Elements())
                            if (ObjectsHRC.Name == "ObjectHRC")
                            {
                                Data.ObjectHRC[i0] = new ObjectHRC();
                                foreach (XAttribute Entry in ObjectsHRC.Attributes())
                                {
                                    Xml.Reader(Entry, ref Data.ObjectHRC[i0].Name   , "Name"   );
                                    Xml.Reader(Entry, ref Data.ObjectHRC[i0].Shadow , "Shadow" );
                                    Xml.Reader(Entry, ref Data.ObjectHRC[i0].UIDName, "UIDName");
                                }

                                foreach (XElement ObjectHRC in ObjectsHRC.Elements())
                                {
                                    if (ObjectHRC.Name != "Nodes")
                                        continue;

                                    foreach (XAttribute Entry in ObjectHRC.Attributes())
                                        if (Entry.Name == "Length")
                                            Data.ObjectHRC[i0].Node = new Node[int.Parse(Entry.Value)];

                                    i1 = 0;
                                    foreach (XElement Nodes in ObjectHRC.Elements())
                                    {
                                        if (Nodes.Name != "Node")
                                            continue;

                                        Data.ObjectHRC[i0].Node[i1] = new Node();

                                        foreach (XAttribute Entry in Nodes.Attributes())
                                        {
                                            Xml.Reader(Entry, ref Data.ObjectHRC[i0].Node[i1].Name  , "Name"  );
                                            Xml.Reader(Entry, ref Data.ObjectHRC[i0].Node[i1].Parent, "Parent");
                                        }
                                        Read(ref Data.ObjectHRC[i0].Node[i1].MT, Nodes);
                                        i1++;
                                    }
                                }
                                i0++;
                            }
                    }
                    else if (Child0.Name == "ObjectHRCList")
                    {
                        foreach (XAttribute Entry in Child0.Attributes())
                            if (Entry.Name == "Length")
                                Data.ObjectHRCList = new string[int.Parse(Entry.Value)];

                        i0 = 0;
                        foreach (XElement ObjectList in Child0.Elements())
                            if (ObjectList.Name == "String")
                            {
                                foreach (XAttribute Entry in ObjectList.Attributes())
                                    Xml.Reader(Entry, ref Data.ObjectHRCList[i0], "Name");
                                i0++;
                            }
                    }
                    else if (Child0.Name == "ObjectList")
                    {
                        foreach (XAttribute Entry in Child0.Attributes())
                            if (Entry.Name == "Length")
                                Data.ObjectList = new string[int.Parse(Entry.Value)];

                        i0 = 0;
                        foreach (XElement ObjectList in Child0.Elements())
                            if (ObjectList.Name == "String")
                            {
                                foreach (XAttribute Entry in ObjectList.Attributes())
                                    Xml.Reader(Entry, ref Data.ObjectList[i0], "Name");
                                i0++;
                            }
                    }
                    else if (Child0.Name == "PlayControl")
                    {
                        Data.PlayControl = new PlayControl();
                        foreach (XAttribute Entry in Child0.Attributes())
                        {
                            Xml.Reader(Entry, ref Data.PlayControl.Begin , "Begin" );
                            Xml.Reader(Entry, ref Data.PlayControl.Div   , "Div"   );
                            Xml.Reader(Entry, ref Data.PlayControl.FPS   , "FPS"   );
                            Xml.Reader(Entry, ref Data.PlayControl.Offset, "Offset");
                            Xml.Reader(Entry, ref Data.PlayControl.Size  , "Size"  );
                        }
                    }
                    else if (Child0.Name == "Points")
                    {
                        foreach (XAttribute Entry in Child0.Attributes())
                            if (Entry.Name == "Length")
                                Data.Point = new ModelTransform[int.Parse(Entry.Value)];

                        i0 = 0;
                        foreach (XElement Points in Child0.Elements())
                            if (Read(ref Data.Point[i0], Points, "Point"))
                                i0++;
                    }
                    else if (Child0.Name == "PostProcess")
                    {
                        Data.PostProcess = new PostProcess { Boolean = true };
                        foreach (XElement PostProcess in Child0.Elements())
                        {
                            Read(ref Data.PostProcess.Ambient  , PostProcess, "Ambient"  );
                            Read(ref Data.PostProcess.Diffuse  , PostProcess, "Diffuse"  );
                            Read(ref Data.PostProcess.LensFlare, PostProcess, "LensFlare");
                            Read(ref Data.PostProcess.LensGhost, PostProcess, "LensGhost");
                            Read(ref Data.PostProcess.LensShaft, PostProcess, "LensShaft");
                            Read(ref Data.PostProcess.Specular , PostProcess, "Specular" );
                        }
                    }
                }
            }
            Xml = null;
        }

        public void XMLWriter(string file)
        {
            int i  = 0;
            int i0 = 0;
            int i1 = 0;
            Base64 = true;
            XElement A3D = new XElement("A3D");
            Xml = new KKtXml { Compact = true };

            if (Base64)
                Xml.Writer(A3D, Base64, "Base64");
            Xml.Writer(A3D, KKtText.ToASCII(BitConverter.GetBytes(Data.Header.Signature)), "Signature");

            XElement _ = new XElement("_");
            if (Data.Header.Signature == 0x5F5F5F43)
                Xml.Writer(_, Data._.CompressF16.ToString().ToUpper(), "CompressF16");
            Xml.Writer(_, Data._.ConverterVersion, "ConverterVersion");
            Xml.Writer(_, Data._.FileName, "FileName");
            Xml.Writer(_, Data._.PropertyVersion, "PropertyVersion");
            A3D.Add(_);

            if (Data.Camera.Auxiliary.Boolean || Data.Camera.Root.Length != 0)
            {
                XElement Camera = new XElement("Camera");
                if (Data.Camera.Auxiliary.Boolean)
                {
                    XElement Auxiliary = new XElement("Auxiliary");
                    Write(ref Data.Camera.Auxiliary.AutoExposure, Auxiliary, "AutoExposure");
                    Write(ref Data.Camera.Auxiliary.    Exposure, Auxiliary,     "Exposure");
                    Write(ref Data.Camera.Auxiliary.Gamma       , Auxiliary, "Gamma"       );
                    Write(ref Data.Camera.Auxiliary.GammaRate   , Auxiliary, "GammaRate"   );
                    Write(ref Data.Camera.Auxiliary.Saturate    , Auxiliary, "Saturate"    );
                    Camera.Add(Auxiliary);
                }

                if (Data.Camera.Root.Length != 0)
                {
                    XElement Root = new XElement("Root");
                    Xml.Writer(Root, Data.Camera.Root.Length, "Length");
                    for (i = 0; i < Data.Camera.Root.Length; i++)
                    {
                        XElement RootEntry = new XElement("RootEntry");
                        Write(ref Data.Camera.Root[i].MT      , RootEntry);
                        Write(ref Data.Camera.Root[i].Interest, RootEntry, "Interest");

                        XElement ViewPoint = new XElement("ViewPoint");
                        if (Data.Camera.Root[i].ViewPoint.Aspect != -1)
                            Xml.Writer(ViewPoint, Data.Camera.Root[i].
                                ViewPoint.Aspect         , "Aspect"         );
                        if (Data.Camera.Root[i].ViewPoint.CameraApertureH != -1)
                            Xml.Writer(ViewPoint, Data.Camera.Root[i].
                                ViewPoint.CameraApertureH, "CameraApertureH");
                        if (Data.Camera.Root[i].ViewPoint.CameraApertureW != -1)
                            Xml.Writer(ViewPoint, Data.Camera.Root[i].
                                ViewPoint.CameraApertureW, "CameraApertureW");
                        Write(ref Data.Camera.Root[i].ViewPoint.FocalLength, ViewPoint, "FocalLength");
                        Write(ref Data.Camera.Root[i].ViewPoint.FOV        , ViewPoint, "FOV"        );
                        if (Data.Camera.Root[i].ViewPoint.FOVHorizontal != -1)
                            Xml.Writer(ViewPoint, Data.Camera.Root[i].
                                ViewPoint.FOVHorizontal, "FOVHorizontal");
                        Write(ref Data.Camera.Root[i].ViewPoint.Roll       , ViewPoint, "Roll"       );
                        Write(ref Data.Camera.Root[i].ViewPoint.MT         , ViewPoint);
                        RootEntry.Add(ViewPoint);
                        Root.Add(RootEntry);
                    }
                    Camera.Add(Root);
                }
                A3D.Add(Camera);
            }

            if (Data.Chara.Length != 0)
            {
                XElement Charas = new XElement("Charas");
                Xml.Writer(Charas, Data.Curve.Length, "Length");
                for (i = 0; i < Data.Curve.Length; i++)
                    Write(ref Data.Chara[i], Charas, "Chara");
                A3D.Add(Charas);
            }

            if (Data.Curve.Length != 0)
            {
                XElement Curves = new XElement("Curves");
                Xml.Writer(Curves, Data.Curve.Length, "Length");
                for (i = 0; i < Data.Curve.Length; i++)
                {
                    XElement Curve = new XElement("Curve");
                    Xml.Writer(Curve, Data.Curve[i].Name, "Name");
                    Write(ref Data.Curve[i].CV, Curve, "CV");
                    Curves.Add(Curve);
                }
                A3D.Add(Curves);
            }

            if (Data.DOF.Name != null)
                Write(ref Data.DOF, A3D, "DOF");

            if (Data.Event.Length != 0)
            {
                XElement Events = new XElement("Events");
                Xml.Writer(Events, Data.Event.Length, "Length");
                for (i = 0; i < Data.Event.Length; i++)
                {
                    XElement Event = new XElement("Event");
                    Xml.Writer(Event, Data.Event[i].Begin, "Begin");
                    Xml.Writer(Event, Data.Event[i].ClipBegin, "ClipBegin");
                    Xml.Writer(Event, Data.Event[i].ClipEnd, "ClipEnd");
                    Xml.Writer(Event, Data.Event[i].End, "End");
                    Xml.Writer(Event, Data.Event[i].Name, "Name");
                    Xml.Writer(Event, Data.Event[i].Param1, "Param1");
                    Xml.Writer(Event, Data.Event[i].Ref, "Ref");
                    Xml.Writer(Event, Data.Event[i].TimeRefScale, "TimeRefScale");
                    Xml.Writer(Event, Data.Event[i].Type, "Type");
                    Events.Add(Event);
                }
                A3D.Add(Events);
            }

            if (Data.Fog.Length != 0)
            {
                XElement Fogs = new XElement("Fogs");
                Xml.Writer(Fogs, Data.Fog.Length, "Length");
                for (i = 0; i < Data.Fog.Length; i++)
                {
                    XElement Fog = new XElement("Fog");
                    Write(ref Data.Fog[i].Density, Fog, "Density");
                    Write(ref Data.Fog[i].Diffuse, Fog, "Diffuse");
                    Write(ref Data.Fog[i].End    , Fog, "End"    );
                    Xml.Writer(Fog, Data.Fog[i].Id, "Id");
                    Write(ref Data.Fog[i].Start  , Fog, "Start"  );
                    Fogs.Add(Fog);
                }
                A3D.Add(Fogs);
            }

            if (Data.Light.Length != 0)
            {
                XElement Lights = new XElement("Lights");
                Xml.Writer(Lights, Data.Light.Length, "Length");
                for (i = 0; i < Data.Light.Length; i++)
                {
                    XElement Light = new XElement("Light");
                    Write(ref Data.Light[i].Ambient      , Light, "Ambient"      );
                    Write(ref Data.Light[i].Diffuse      , Light, "Diffuse"      );
                    Xml.Writer(Light, Data.Light[i].Id  , "Id"  );
                    Write(ref Data.Light[i].Incandescence, Light, "Incandescence");
                    Xml.Writer(Light, Data.Light[i].Name, "Name");
                    Write(ref Data.Light[i].Position     , Light, "Position"     );
                    Write(ref Data.Light[i].Specular     , Light, "Specular"     );
                    Write(ref Data.Light[i].SpotDirection, Light, "SpotDirection");
                    Xml.Writer(Light, Data.Light[i].Type, "Type");
                    Lights.Add(Light);
                }
                A3D.Add(Lights);
            }

            if (Data.MObjectHRC.Length != 0)
            {
                XElement MObjectsHRC = new XElement("MObjectsHRC");
                Xml.Writer(MObjectsHRC, Data.MObjectHRC.Length, "Length");
                for (i0 = 0; i0 < Data.MObjectHRC.Length; i0++)
                {
                    XElement MObjectHRC = new XElement("MObjectHRC");
                    Xml.Writer(MObjectHRC, Data.MObjectHRC[i0].Name, "Name");
                    if (Data.MObjectHRC[i0].Instance != null)
                    {
                        XElement Instances = new XElement("Instances");
                        Xml.Writer(Instances, Data.MObjectHRC[i0].Instance.Length, "Length");
                        for (i1 = 0; i1 < Data.MObjectHRC[i0].Instance.Length; i1++)
                            Write(ref Data.MObjectHRC[i0].Instance[i1], Instances, "Instance");
                        MObjectHRC.Add(Instances);
                    }

                    if (Data.MObjectHRC[i0].Node != null)
                    {
                        XElement Nodes = new XElement("Nodes");
                        Xml.Writer(Nodes, Data.MObjectHRC[i0].Node.Length, "Length");
                        for (i1 = 0; i1 < Data.MObjectHRC[i0].Node.Length; i1++)
                        {
                            XElement Node = new XElement("Node");
                            Xml.Writer(Node, Data.MObjectHRC[i0].Node[i1].Name  , "Name"  );
                            Xml.Writer(Node, Data.MObjectHRC[i0].Node[i1].Parent, "Parent");
                            Write(ref Data.MObjectHRC[i0].Node[i1].MT, Node);
                            Nodes.Add(Node);
                        }
                        MObjectHRC.Add(Nodes);
                    }
                    MObjectsHRC.Add(MObjectHRC);
                }
                A3D.Add(MObjectsHRC);
            }

            if (Data.MObjectHRCList.Length != 0)
            {
                XElement MObjectHRCList = new XElement("MObjectHRCList");
                Xml.Writer(MObjectHRCList, Data.MObjectHRCList.Length, "Length");
                for (i = 0; i < Data.MObjectHRCList.Length; i++)
                {
                    XElement String = new XElement("String");
                    Xml.Writer(String, Data.MObjectHRCList[i], "Name");
                    MObjectHRCList.Add(String);
                }
                A3D.Add(MObjectHRCList);
            }

            if (Data.Motion.Length != 0)
            {
                XElement Motion = new XElement("Motion");
                Xml.Writer(Motion, Data.Motion.Length, "Length");
                for (i = 0; i < Data.Motion.Length; i++)
                {
                    XElement String = new XElement("String");
                    Xml.Writer(String, Data.Motion[i], "Name");
                    Motion.Add(String);
                }
                A3D.Add(Motion);
            }

            if (Data.Object.Length != 0)
            {
                XElement Objects = new XElement("Objects");
                Xml.Writer(Objects, Data.Object.Length, "Length");
                for (i0 = 0; i0 < Data.Object.Length; i0++)
                {
                    XElement Object = new XElement("Object");
                    if (Data.Object[i0].Morph != null)
                    {
                        Xml.Writer(Object, Data.Object[i0].Morph, "Morph");
                        if (Data.Object[i0].MorphOffset != -1)
                            Xml.Writer(Object, Data.Object[i0].MorphOffset, "MorphOffset");
                    }
                    Xml.Writer(Object, Data.Object[i0].ParentName, "ParentName");
                    Write(ref Data.Object[i0].MT, Object);
                    if (Data.Object[i0].TP != null)
                    {
                        XElement TexPats = new XElement("TexPats");
                        Xml.Writer(TexPats, Data.Object[i0].TP.Length, "Length");
                        for (i1 = 0; i1 < Data.Object[i0].TP.Length; i1++)
                        {
                            XElement TexPat = new XElement("TexPat");
                            Xml.Writer(TexPat, Data.Object[i0].TP[i1].Name     , "Name"     );
                            Xml.Writer(TexPat, Data.Object[i0].TP[i1].Pat      , "Pat"      );
                            Xml.Writer(TexPat, Data.Object[i0].TP[i1].PatOffset, "PatOffset");
                            TexPats.Add(TexPat);
                        }
                        Object.Add(TexPats);
                    }
                    if (Data.Object[i0].TT != null)
                    {
                        XElement TexTransforms = new XElement("TexTransforms");
                        Xml.Writer(TexTransforms, Data.Object[i0].TT.Length, "Length");
                        for (i1 = 0; i1 < Data.Object[i0].TT.Length; i1++)
                        {
                            XElement TexTransform = new XElement("TexTransform");
                            Xml.Writer(TexTransform, Data.Object[i0].TT[i1].Name, "Name");
                            Write(ref Data.Object[i0].TT[i1].C , TexTransform, "C" );
                            Write(ref Data.Object[i0].TT[i1].O , TexTransform, "O" );
                            Write(ref Data.Object[i0].TT[i1].R , TexTransform, "R" );
                            Write(ref Data.Object[i0].TT[i1].Ro, TexTransform, "Ro");
                            Write(ref Data.Object[i0].TT[i1].RF, TexTransform, "RF");
                            Write(ref Data.Object[i0].TT[i1].TF, TexTransform, "TF");
                            TexTransforms.Add(TexTransform);
                        }
                        Object.Add(TexTransforms);
                    }
                    Objects.Add(Object);
                }
                A3D.Add(Objects);
            }

            if (Data.ObjectHRC.Length != 0)
            {
                XElement ObjectsHRC = new XElement("ObjectsHRC");
                Xml.Writer(ObjectsHRC, Data.ObjectHRC.Length, "Length");
                for (i0 = 0; i0 < Data.ObjectHRC.Length; i0++)
                {
                    XElement ObjectHRC = new XElement("ObjectHRC");
                    Xml.Writer(ObjectHRC, Data.ObjectHRC[i0].Name, "Name");
                    if (Data.ObjectHRC[i0].Shadow != 0)
                        Xml.Writer(ObjectHRC, Data.ObjectHRC[i0].Shadow, "Shadow");
                    Xml.Writer(ObjectHRC, Data.ObjectHRC[i0].UIDName, "UIDName");
                    if (Data.ObjectHRC[i0].Node != null)
                    {
                        XElement Nodes = new XElement("Nodes");
                        Xml.Writer(Nodes, Data.ObjectHRC[i0].Node.Length, "Length");
                        for (i1 = 0; i1 < Data.ObjectHRC[i0].Node.Length; i1++)
                        {
                            XElement Node = new XElement("Node");
                            Xml.Writer(Node, Data.ObjectHRC[i0].Node[i1].Name  , "Name"  );
                            Xml.Writer(Node, Data.ObjectHRC[i0].Node[i1].Parent, "Parent");
                            Write(ref Data.ObjectHRC[i0].Node[i1].MT, Node);
                            Nodes.Add(Node);
                        }
                        ObjectHRC.Add(Nodes);
                    }
                    ObjectsHRC.Add(ObjectHRC);
                }
                A3D.Add(ObjectsHRC);
            }

            if (Data.ObjectHRCList.Length != 0)
            {
                XElement ObjectHRCList = new XElement("ObjectHRCList");
                Xml.Writer(ObjectHRCList, Data.ObjectHRCList.Length, "Length");
                for (i = 0; i < Data.ObjectHRCList.Length; i++)
                {
                    XElement String = new XElement("String");
                    Xml.Writer(String, Data.ObjectHRCList[i], "Name");
                    ObjectHRCList.Add(String);
                }
                A3D.Add(ObjectHRCList);
            }

            if (Data.ObjectList.Length != 0)
            {
                XElement ObjectList = new XElement("ObjectList");
                Xml.Writer(ObjectList, Data.ObjectList.Length, "Length");
                for (i = 0; i < Data.ObjectList.Length; i++)
                {
                    XElement String = new XElement("String");
                    Xml.Writer(String, Data.ObjectList[i], "Name");
                    ObjectList.Add(String);
                }
                A3D.Add(ObjectList);
            }

            XElement PlayControl = new XElement("PlayControl");
            Xml.Writer(PlayControl, Data.PlayControl.Begin, "Begin");
            Xml.Writer(PlayControl, Data.PlayControl.FPS, "FPS");
            Xml.Writer(PlayControl, Data.PlayControl.Offset, "Offset");
            Xml.Writer(PlayControl, Data.PlayControl.Size, "Size");
            A3D.Add(PlayControl);

            if (Data.Point.Length != 0)
            {
                XElement Points = new XElement("Points");
                Xml.Writer(Points, Data.Curve.Length, "Length");
                for (i = 0; i < Data.Point.Length; i++)
                    Write(ref Data.Point[i], Points, "Point");
                A3D.Add(Points);
            }

            if (Data.PostProcess.Boolean)
            {
                XElement PostProcess = new XElement("PostProcess");
                Write(ref Data.PostProcess.Ambient  , PostProcess, "Ambient"  );
                Write(ref Data.PostProcess.Diffuse  , PostProcess, "Diffuse"  );
                Write(ref Data.PostProcess.LensFlare, PostProcess, "LensFlare");
                Write(ref Data.PostProcess.LensGhost, PostProcess, "LensGhost");
                Write(ref Data.PostProcess.LensShaft, PostProcess, "LensShaft");
                Write(ref Data.PostProcess.Specular , PostProcess, "Specular" );
                A3D.Add(PostProcess);
            }
            Xml.doc.Add(A3D);
            if (File.Exists(file + ".xml"))
                File.Delete(file + ".xml");
            Xml.SaveXml(file + ".xml");
        }

        public bool Read(ref ModelTransform MT, XElement element, string name) =>
            Read(ref MT, element, name, "");

        public bool Read(ref ModelTransform MT, XElement element, string name, string altname)
        {
            bool HasMT = element.Name.ToString() == name || element.Name.ToString() == altname;
            if (HasMT)
                Read(ref MT, element);
            return HasMT;
        }

        public void Read(ref ModelTransform MT, XElement element)
        {
            foreach (XAttribute Entry in element.Attributes())
            {
                Xml.Reader(Entry, ref MT.   Name,    "Name");
                Xml.Reader(Entry, ref MT.UIDName, "UIDName");
            }

            foreach (XElement Object in element.Elements())
            {
                Read(ref MT.Rot       , Object, "Rot"       );
                Read(ref MT.Scale     , Object, "Scale"     );
                Read(ref MT.Trans     , Object, "Trans"     );
                Read(ref MT.Visibility, Object, "Visibility");
            }
        }

        public void Read(ref RGBAKey RGBA, XElement element, string name) =>
            Read(ref RGBA, element, name, "");

        public void Read(ref RGBAKey RGBA, XElement element, string name, string altname)
        { if (element.Name.ToString() == name || element.Name.ToString() == altname)
            Read(ref RGBA, element); }

        public void Read(ref RGBAKey RGBA, XElement element)
        {
            RGBA.Boolean = true;

            foreach (XAttribute Entry in element.Attributes())
                Xml.Reader(Entry, ref RGBA.Alpha, "Alpha");

            foreach (XElement Child in element.Elements())
            {
                Read(ref RGBA.R, Child, "R"); Read(ref RGBA.G, Child, "G");
                Read(ref RGBA.B, Child, "B"); Read(ref RGBA.A, Child, "A");
            }
        }

        public void Read(ref Vector3Key Key, XElement element, string name) =>
            Read(ref Key, element, name, "");

        public void Read(ref Vector3Key Key, XElement element, string name, string altname)
        {
            if (element.Name.ToString() == name || element.Name.ToString() == altname)
                Read(ref Key, element);
        }

        public void Read(ref Vector3Key Key, XElement element)
        {
            foreach (XElement Child in element.Elements())
            {
                Read(ref Key.X, Child, "X");
                Read(ref Key.Y, Child, "Y");
                Read(ref Key.Z, Child, "Z");
            }
        }

        public void Read(ref KeyUV UV, XElement element, string name) => Read(ref UV, element, name, "");

        public void Read(ref KeyUV UV, XElement element, string name, string altname)
        {
            if (element.Name.ToString() == name + "U" || element.Name.ToString() == altname + "U")
                Read(ref UV.U, element);
            if (element.Name.ToString() == name + "V" || element.Name.ToString() == altname + "V")
                Read(ref UV.V, element);
        }

        public void Read(ref Key Key, XElement element, string name) =>
            Read(ref Key, element, name, "");

        public void Read(ref Key Key, XElement element, string name, string altname)
        {
            if (element.Name.ToString() == name || element.Name.ToString() == altname)
                Read(ref Key, element);
        }

        public void Read(ref Key Key, XElement element)
        {
            int i = 0;

            Key.NewKey();
            Key.Boolean = true;

            foreach (XAttribute Entry in element.Attributes())
            {
                Xml.Reader(Entry, ref Key.EPTypePost, "Post");
                Xml.Reader(Entry, ref Key.EPTypePre, "Pre");
                Xml.Reader(Entry, ref Key.Length, "L");
                Xml.Reader(Entry, ref Key.Max, "M");
                Xml.Reader(Entry, ref Key.Type, "T");
                Xml.Reader(Entry, ref Key.Value, "V", Base64);
                Xml.Reader(Entry, ref Key.RoundFrame, "RF");
            }

            if (Key.Type != 0x0000 && Key.Type != 0x0001)
            {
                Key.Trans = new Key.Transform[Key.Length];

                foreach (XElement Child in element.Elements())
                {
                    if (Child.Name == "Key" || Child.Name == "K")
                        foreach (XAttribute Entry in Child.Attributes())
                        {
                            Xml.Reader(Entry, ref Key.Trans[i].Frame, "F");
                            Xml.Reader(Entry, ref Key.Trans[i].Value1, "V1", Base64);
                            Xml.Reader(Entry, ref Key.Trans[i].Value2, "V2", Base64);
                            Xml.Reader(Entry, ref Key.Trans[i].Value3, "V3", Base64);
                            Xml.Reader(Entry, ref Key.Trans[i].Type, "T");

                            if (Key.RoundFrame)
                                KKtMain.FloorCeiling(ref Key.Trans[i].Frame);
                        }

                    i++;
                    if (i == Key.Length)
                        break;
                }
            }
            else if (Key.Type == 0x0000)
                Key.Value = 0;

            Key.RawData = new Key.RawD();
            foreach (XAttribute Entry in element.Attributes())
            {
                Xml.Reader(Entry, ref Key.RawData.KeyType, "KeyType");
                Xml.Reader(Entry, ref Key.RawData.ValueType, "ValueType");
                Xml.Reader(Entry, ref Key.RawData.ValueListSize, "ValueListSize");
                Xml.Reader(Entry, ref Key.RawData.ValueList, "ValueList", ',');
                Xml.Reader(Entry, ref Key.RawData.KeyType, "KT");
                Xml.Reader(Entry, ref Key.RawData.ValueType, "VT");
                Xml.Reader(Entry, ref Key.RawData.ValueListSize, "VLS");
                Xml.Reader(Entry, ref Key.RawData.ValueList, "VL", ',');
            }

            if (Key.RawData.ValueListSize > 0)
            {
                int DataSize = 0;
                if (Key.RawData.KeyType > 2)
                    DataSize = 4;
                else if (Key.RawData.KeyType > 1)
                    DataSize = 3;
                else if (Key.RawData.KeyType > 0)
                    DataSize = 2;
                else
                    DataSize = 1;

                Key.Length = Key.RawData.ValueListSize / DataSize;
                Key.Trans = new Key.Transform[Key.Length];
                for (i = 0; i < Key.Length; i++)
                {
                    Key.Trans[i].Type = Key.RawData.KeyType;
                    Key.Trans[i].Frame = KKtMain.ToDouble(Key.RawData.ValueList[i * DataSize + 0]);

                    if (Base64)
                    {
                        if (Key.Trans[i].Type > 0)
                        {
                            Key.Trans[i].Value1 = BitConverter.ToDouble(KKtMain.
                                FromBase64(Key.RawData.ValueList[i * DataSize + 1]), 0);
                            if (Key.Trans[i].Type > 1)
                            {
                                Key.Trans[i].Value2 = BitConverter.ToDouble(KKtMain.
                                    FromBase64(Key.RawData.ValueList[i * DataSize + 2]), 0);
                                if (Key.Trans[i].Type > 2)
                                    Key.Trans[i].Value3 = BitConverter.ToDouble(KKtMain.
                                        FromBase64(Key.RawData.ValueList[i * DataSize + 3]), 0);
                            }
                        }
                    }
                    else
                    {
                        if (Key.Trans[i].Type > 0)
                        {
                            Key.Trans[i].Value1 = KKtMain.ToDouble(
                                Key.RawData.ValueList[i * DataSize + 1]);
                            if (Key.Trans[i].Type > 1)
                            {
                                Key.Trans[i].Value2 = KKtMain.ToDouble(
                                    Key.RawData.ValueList[i * DataSize + 2]);
                                if (Key.Trans[i].Type > 2)
                                    Key.Trans[i].Value3 = KKtMain.ToDouble(
                                      Key.RawData.ValueList[i * DataSize + 3]);
                            }
                        }
                    }
                }
                Key.RawData = new Key.RawD();
            }
        }

        public void Write(ref ModelTransform MT, XElement element, string name)
        {
            XElement Child = new XElement(name);
            Write(ref MT, Child);
            element.Add(Child);
        }

        public void Write(ref ModelTransform MT, XElement element)
        {
            Xml.Writer(element, MT.Name, "Name");
            Xml.Writer(element, MT.UIDName, "UIDName");

            Write(ref MT.Rot, element, "Rot");
            Write(ref MT.Scale, element, "Scale");
            Write(ref MT.Trans, element, "Trans");
            Write(ref MT.Visibility, element, "Visibility");
        }

        public void Write(ref RGBAKey RGBA, XElement element, string name)
        {
            if (!RGBA.Boolean)
                return;

            XElement Child = new XElement(name);
            if (RGBA.Alpha)
                Xml.Writer(Child, RGBA.Alpha, "Alpha");
            Write(ref RGBA.R, Child, "R");
            Write(ref RGBA.G, Child, "G");
            Write(ref RGBA.B, Child, "B");
            if (RGBA.Alpha)
                Write(ref RGBA.A, Child, "A");
            element.Add(Child);
        }

        public void Write(ref Vector3Key Key, XElement element, string name)
        {
            XElement Child = new XElement(name);
            Write(ref Key.X, Child, "X");
            Write(ref Key.Y, Child, "Y");
            Write(ref Key.Z, Child, "Z");
            element.Add(Child);
        }

        public void Write(ref KeyUV UV, XElement element, string name)
        { Write(ref UV.U, element, name + "U"); Write(ref UV.V, element, name + "V"); }

        public void Write(ref Key Key, XElement element, string name)
        {
            if (Key.Boolean && Key.Type != -1)
            {
                int i = 0;
                XElement Keys = new XElement(name);
                Xml.Writer(Keys, Key.Type, "T");
                if (Key.Trans != null)
                {
                    if (Key.EPTypePost != -1)
                        Xml.Writer(Keys, Key.EPTypePost, "Post");
                    if (Key.EPTypePre != -1)
                        Xml.Writer(Keys, Key.EPTypePre, "Pre");
                    if (Key.Length != -1)
                        Xml.Writer(Keys, Key.Length, "L");
                    if (Key.Max != -1)
                        Xml.Writer(Keys, Key.Max, "M");
                    int Type = 0;
                    if (Key.Length < 1000 || Data._.CompressF16 > 0)
                        for (i = 0; i < Key.Trans.Length; i++)
                        {
                            Type = Key.Trans[i].Type;
                            if (Key.Trans[i].Value3 == 0 && Type > 2)
                                Type = 2;
                            if (Key.Trans[i].Value2 == 0 && Type > 1)
                                Type = 1;
                            if (Key.Trans[i].Value1 == 0 && Type > 0)
                                Type = 0;
                            XElement K = new XElement("K");
                            Xml.Writer(K, Type, "T");
                            Xml.Writer(K, Key.Trans[i].Frame, "F");
                            if (Type > 0)
                            {
                                Xml.Writer(K, Key.Trans[i].Value1, "V1", Base64);
                                if (Type > 1)
                                {
                                    Xml.Writer(K, Key.Trans[i].Value2, "V2", Base64);
                                    if (Type > 2)
                                        Xml.Writer(K, Key.Trans[i].Value3, "V3", Base64);
                                }
                            }
                            Keys.Add(K);
                        }
                    else
                    {
                        Key.RawData = new Key.RawD();
                        for (i = 0; i < Key.Trans.Length; i++)
                            if (Key.RawData.KeyType < Key.Trans[i].Type)
                                Key.RawData.KeyType = Key.Trans[i].Type;
                        Key.RawData.ValueListSize = Key.Trans.Length * (Key.RawData.KeyType + 1);
                        Xml.Writer(Keys, Key.RawData.KeyType.ToString(), "KT");
                        Xml.Writer(Keys, Key.RawData.ValueType, "VT");
                        Xml.Writer(Keys, Key.RawData.ValueListSize.ToString(), "VLS");

                        for (i = 0; i < Key.Trans.Length; i++)
                        {
                            Key.RawData.ValueListString += KKtMain.ToString(Key.Trans[i].Frame);
                            if (Key.RawData.KeyType > 0)
                            {
                                Key.RawData.ValueListString += "," + KKtMain.
                                    ToString(Key.Trans[i].Value1, Base64);
                                if (Key.RawData.KeyType > 1)
                                {
                                    Key.RawData.ValueListString += "," + KKtMain.
                                        ToString(Key.Trans[i].Value2, Base64);
                                    if (Key.RawData.KeyType > 2)
                                        Key.RawData.ValueListString += "," + KKtMain.
                                            ToString(Key.Trans[i].Value3, Base64);
                                }
                            }
                            if (i < Key.Trans.Length - 1)
                                Key.RawData.ValueListString += ',';
                        }
                        Xml.Writer(Keys, Key.RawData.ValueListString, "VL");
                        Key.RawData = new Key.RawD();
                    }
                }
                else if (Key.Value != 0 && Key.Type > 0)
                    Xml.Writer(Keys, Key.Value, "V", Base64);
                element.Add(Keys);
            }
        }

        public int[] SortWriter(int Length)
        {
            int i = 0;
            List<string> A = new List<string>();
            for (i = 0; i < Length; i++)
                A.Add(i.ToString());
            A.Sort();
            int[] B = new int[Length];
            for (i = 0; i < Length; i++)
                B[i] = int.Parse(A[i]);
            return B;
        }
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

    public class Event
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

        public Event()
        { Type = -1; End = -1; Begin = -1; ClipEnd = -1; ClipBegin = -1;
            TimeRefScale = -1; Name = null; Param1 = null; Ref = null; }
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

    public class Header
    {
        public int Count;
        public int BinaryLength;
        public int BinaryOffset;
        public int HeaderOffset;
        public int StringLength;
        public int StringOffset;

        public Header()
        { Count = -1; BinaryLength = -1; BinaryOffset = -1;
            HeaderOffset = -1; StringLength = -1; StringOffset = -1; }
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

        public Light()
        { Id = -1; Name = null; Type = null; Ambient = new RGBAKey(); Diffuse = new RGBAKey();
            Specular = new RGBAKey(); Incandescence = new RGBAKey();
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
        { MorphOffset = -1; Morph = null; ParentName = null;
            MT = new ModelTransform(); TP = null; TT = null; }
    }

    public class ObjectHRC
    {
        public double Shadow;
        public string Name;
        public string UIDName;
        public Node[] Node;

        public ObjectHRC()
        { Shadow = -1; Name = null; UIDName = null; Node = null; }
    }

    public class PlayControl
    {
        public double Begin;
        public double Div;
        public double FPS;
        public double Offset;
        public double Size;

        public PlayControl()
        { Begin = -1; Div = -1; FPS = -1; Offset = -1; Size = -1; }
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

    public class TexturePattern
    {
        public int PatOffset;
        public string Pat;
        public string Name;

        public TexturePattern()
        { PatOffset = -1; Pat = null; Name = null; }
    }

    public class TextureTransform
    {
        public string Name;
        public KeyUV C;  //Coverage
        public KeyUV O;  //Offset
        public KeyUV R;  //Repeat
        public Key Ro; //Rotate
        public Key RF; //RotateFrame
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
            Rot = new Vector3Key();
            Scale = new Vector3Key();
            Trans = new Vector3Key();
            Visibility = new Key();
            BinOffset = -1;
            Name = null;
            UIDName = null;
            Writed = false;
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
    }

    public class Vector3Key
    {
        public Key X;
        public Key Y;
        public Key Z;

        public Vector3Key()
        { X = new Key(); Y = new Key(); Z = new Key(); }
    }

    public class KeyUV
    {
        public Key U;
        public Key V;

        public KeyUV()
        { U = new Key(); V = new Key(); }
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
            RawData = new RawD(); RoundFrame = false; Trans = null; Type = 0; Value = -1; }

        public class RawD
        {
            public int KeyType;
            public int ValueListSize;
            public string ValueType;
            public string ValueListString;
            public string[] ValueList;

            public RawD()
            { KeyType = -1; ValueListSize = -1; ValueType = "float";
                ValueListString = null; ValueList = null; }
        }

        public struct Transform
        {
            public int Type;
            public double Frame;
            public double Value1;
            public double Value2;
            public double Value3;
        }
    }
}
