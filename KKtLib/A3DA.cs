using System;
using System.IO;
using System.Collections.Generic;
using KKtIO = KKtLib.IO;

namespace KKtLib.A3DA
{
    public class A3DA
    {
        private const bool A3DCOpt = true;
        private const string d = ".";
        private const string BO = "bin_offset";
        private const string MTBO = "model_transform.bin_offset";

        private int SOi ;
        private int SOi0;
        private int SOi1;
        private int Offset;
        private int[] SO ;
        private int[] SO0;
        private int[] SO1;
        private string name;
        private string nameView;
        private string value;
        private string[] dataArray;
        private Values UsedValues;
        private Dictionary<string, object> Dict;

        public bool A3DC;
        public KKtIO IO;
        public A3DAData Data;

        public A3DA()
        { A3DC = false; Data = new A3DAData(); Dict = new Dictionary<string, object>();
            IO = new KKtIO(); UsedValues = new Values(); }

        public int A3DAReader(string file)
        {
            name = "";
            Offset = 0;
            nameView = "";
            dataArray = new string[4];
            Dict = new Dictionary<string, object>();
            Data = new A3DAData();

            IO = KKtIO.OpenReader(file + ".a3da");
            Data.Header.Format = IO.Format = Main.Format.F;
            Data.Header.Signature = IO.ReadInt32();
            if (Data.Header.Signature == 0x41443341)
                Data.Header = IO.ReadHeader(true);
            if (Data.Header.Signature != 0x44334123)
                return 0;

            Offset = IO.Position - 4;
            Data.Header.Signature = IO.ReadInt32();

            if (Data.Header.Signature == 0x5F5F5F41)
            {
                IO.Position = 0x10;
                Data.Header.Format = IO.Format = Main.Format.DT;
            }
            else if (Data.Header.Signature == 0x5F5F5F43)
            {
                IO.Position = Offset + 0x10;
                IO.ReadInt32();
                IO.ReadInt32();
                Data.Head.HeaderOffset = IO.ReadInt32Endian(true);

                IO.Position = Offset + (int)Data.Head.HeaderOffset;
                if (IO.ReadInt32() != 0x50)
                    return 0;
                Data.Head.StringOffset = IO.ReadInt32Endian(true);
                Data.Head.StringLength = IO.ReadInt32Endian(true);
                Data.Head.Count = IO.ReadInt32Endian(true);
                if (IO.ReadInt32() != 0x4C42)
                    return 0;
                Data.Head.BinaryOffset = IO.ReadInt32Endian(true);
                Data.Head.BinaryLength = IO.ReadInt32Endian(true);

                IO.Position = Offset + (int)Data.Head.StringOffset;
            }
            else
                return 0;

            if (Data.Header.Format < Main.Format.F || Data.Header.Format == Main.Format.FT)
                Data.Head.StringLength = IO.Length - 0x10;

            string[] STRData = IO.ReadString(Data.Head.StringLength).Split('\n');
            for (int i = 0; i < STRData.Length; i++)
            {
                dataArray = STRData[i].Split('=');
                if (dataArray.Length == 2)
                    Main.GetDictionary(ref Dict, dataArray[0], dataArray[1]);
            }

            A3DAReader();

            if (Data.Header.Format == Main.Format.F || Data.Header.Format > Main.Format.FT)
            {
                IO.Position = Offset + (int)Data.Head.BinaryOffset;
                Offset = IO.Position;
                A3DCReader();
            }

            IO.Close();

            name = "";
            Offset = 0;
            nameView = "";
            dataArray = null;
            Dict = null;
            return 1;
        }

        private void A3DAReader()
        {
            int i0 = 0;
            int i1 = 0;

            if (Main.FindValue(Dict, "_.compress_f16", ref value))
                Data._.CompressF16 = int.Parse(value);
            if (Main.FindValue(Dict, "_.converter.version", ref value))
                Data._.ConverterVersion = value;
            if (Main.FindValue(Dict, "_.file_name", ref value))
                Data._.FileName = value;
            if (Main.FindValue(Dict, "_.property.version", ref value))
                Data._.PropertyVersion = value;
            if (Main.FindValue(Dict, "ambient.length", ref value))
            {   Data.Ambient = new Ambient[int.Parse(value)]; Data.Header.Format = Main.Format.MGF; }
            if (Main.FindValue(Dict, "camera_root.length", ref value))
                Data.Camera.Root = new CameraRoot[int.Parse(value)];
            if (Main.FindValue(Dict, "chara.length", ref value))
                Data.Chara = new ModelTransform[int.Parse(value)];
            if (Main.FindValue(Dict, "curve.length", ref value))
                Data.Curve = new Curve[int.Parse(value)];
            if (Main.FindValue(Dict, "dof.name", ref value))
                Data.DOF.Name = value;
            if (Main.FindValue(Dict, "event.length", ref value))
                Data.Event = new Event[int.Parse(value)];
            if (Main.FindValue(Dict, "fog.length", ref value))
                Data.Fog = new Fog[int.Parse(value)];
            if (Main.FindValue(Dict, "light.length", ref value))
                Data.Light = new Light[int.Parse(value)];
            if (Main.FindValue(Dict, "m_objhrc.length", ref value))
                Data.MObjectHRC = new MObjectHRC[int.Parse(value)];
            if (Main.FindValue(Dict, "m_objhrc_list.length", ref value))
                Data.MObjectHRCList = new string[int.Parse(value)];
            if (Main.FindValue(Dict, "material_list.length", ref value))
            {   Data.MaterialList = new MaterialList[int.Parse(value)]; Data.Header.Format = Main.Format.X; }
            if (Main.FindValue(Dict, "motion.length", ref value))
                Data.Motion = new string[int.Parse(value)];
            if (Main.FindValue(Dict, "object.length", ref value))
                Data.Object = new Object[int.Parse(value)];
            if (Main.FindValue(Dict, "objhrc.length", ref value))
                Data.ObjectHRC = new ObjectHRC[int.Parse(value)];
            if (Main.FindValue(Dict, "object_list.length", ref value))
                Data.ObjectList = new string[int.Parse(value)];
            if (Main.FindValue(Dict, "objhrc_list.length", ref value))
                Data.ObjectHRCList = new string[int.Parse(value)];
            if (Main.FindValue(Dict, "play_control.begin", ref value))
                Data.PlayControl.Begin = Main.ToDouble(value);
            if (Main.FindValue(Dict, "play_control.div", ref value))
                Data.PlayControl.FPS = Main.ToDouble(value);
            if (Main.FindValue(Dict, "play_control.fps", ref value))
                Data.PlayControl.FPS = Main.ToDouble(value);
            if (Main.FindValue(Dict, "play_control.offset", ref value))
                Data.PlayControl.Offset = Main.ToDouble(value);
            if (Main.FindValue(Dict, "play_control.size", ref value))
                Data.PlayControl.Size = Main.ToDouble(value);
            if (Main.FindValue(Dict, "point.length", ref value))
                Data.Point = new ModelTransform[int.Parse(value)];
            if (Main.StartsWith(Dict, "camera_auxiliary"))
                Data.Camera.Auxiliary.Boolean = true;
            if (Main.StartsWith(Dict, "post_process"))
                Data.PostProcess.Boolean = true;
            
            for (i0 = 0; i0 < Data.Ambient.Length; i0++)
            {
                name = "ambient." + i0 + d;
                Data.Ambient[i0] = new Ambient();

                Main.FindValue(Dict, name + "name", ref Data.Ambient[i0].Name);
                ReadA3DA(ref Data.Ambient[i0].   LightDiffuse, name +    "light.Diffuse.");
                ReadA3DA(ref Data.Ambient[i0].RimLightDiffuse, name + "rimlight.Diffuse.");
            }

            if (Data.Camera.Auxiliary.Boolean)
            {
                name = "camera_auxiliary.";

                ReadA3DA(ref Data.Camera.Auxiliary.AutoExposure, name + "auto_exposure.");
                ReadA3DA(ref Data.Camera.Auxiliary.    Exposure, name +      "exposure.");
                ReadA3DA(ref Data.Camera.Auxiliary.Gamma       , name + "gamma."        );
                ReadA3DA(ref Data.Camera.Auxiliary.GammaRate   , name + "gamma_rate."   );
                ReadA3DA(ref Data.Camera.Auxiliary.Saturate    , name + "saturate."     );
            }

            for (i0 = 0; i0 < Data.Camera.Root.Length; i0++)
            {
                name = "camera_root." + i0 + d;
                nameView = name + "view_point.";
                Data.Camera.Root[i0] = new CameraRoot();  
                
                Main.FindValue(Dict, nameView + "aspect"           ,
                    ref Data.Camera.Root[i0].ViewPoint.Aspect         );
                Main.FindValue(Dict, nameView + "camera_aperture_h",
                    ref Data.Camera.Root[i0].ViewPoint.CameraApertureH);
                Main.FindValue(Dict, nameView + "camera_aperture_w",
                    ref Data.Camera.Root[i0].ViewPoint.CameraApertureW);
                Main.FindValue(Dict, nameView + "fov_is_horizontal",
                    ref Data.Camera.Root[i0].ViewPoint.FOVHorizontal  );
                ReadA3DA(ref Data.Camera.Root[i0].Interest             , name     + "interest."    );
                ReadA3DA(ref Data.Camera.Root[i0].          MT         , name                      );
                ReadA3DA(ref Data.Camera.Root[i0].ViewPoint.MT         , nameView                  );
                ReadA3DA(ref Data.Camera.Root[i0].ViewPoint.FocalLength, nameView + "focal_length.");
                ReadA3DA(ref Data.Camera.Root[i0].ViewPoint.FOV        , nameView + "fov."         );
                ReadA3DA(ref Data.Camera.Root[i0].ViewPoint.Roll       , nameView + "roll."        );
            }

            for (i0 = 0; i0 < Data.Chara.Length; i0++)
                ReadA3DA(ref Data.Chara[i0], "chara." + i0 + d);

            for (i0 = 0; i0 < Data.Curve.Length; i0++)
            {
                Data.Curve[i0] = new Curve();
                name = "curve." + i0 + d;
                
                Main.FindValue(Dict, name + "name", ref Data.Curve[i0].Name);
                ReadA3DA(ref Data.Curve[i0].CV, name + "cv.");
            }

            if (Data.DOF.Name != null)
            {
                Data.Header.Format = Main.Format.FT;
                Data.DOF.MT = new ModelTransform();
                ReadA3DA(ref Data.DOF.MT, "dof.");
            }

            for (i0 = 0; i0 < Data.Fog.Length; i0++)
            {
                Data.Fog[i0] = new Fog();
                name = "fog." + i0 + d;

                Main.FindValue(Dict, name + "id", ref Data.Fog[i0].Id);
                ReadA3DA(ref Data.Fog[i0].Density, name + "density.");
                ReadA3DA(ref Data.Fog[i0].Diffuse, name + "Diffuse.");
                ReadA3DA(ref Data.Fog[i0].End    , name + "end."    );
                ReadA3DA(ref Data.Fog[i0].Start  ,name + "start."  );
            }

            for (i0 = 0; i0 < Data.Light.Length; i0++)
            {
                Data.Light[i0] = new Light();
                name = "light." + i0 + d;

                Main.FindValue(Dict, name + "id", ref Data.Light[i0].Id);
                Main.FindValue(Dict, name + "name", ref Data.Light[i0].Name);
                Main.FindValue(Dict, name + "type", ref Data.Light[i0].Type);

                ReadA3DA(ref Data.Light[i0].Ambient      , name + "Ambient."       );
                ReadA3DA(ref Data.Light[i0].Diffuse      , name + "Diffuse."       );
                ReadA3DA(ref Data.Light[i0].Incandescence, name + "Incandescence." );
                ReadA3DA(ref Data.Light[i0].Specular     , name + "Specular."      );
                ReadA3DA(ref Data.Light[i0].Position     , name + "position."      );
                ReadA3DA(ref Data.Light[i0].SpotDirection, name + "spot_direction.");
            }

            for (i0 = 0; i0 < Data.Event.Length; i0++)
            {
                name = "event." + i0 + d;
                Data.Event[i0] = new Event();

                Main.FindValue(Dict, name + "begin", ref Data.Event[i0].Begin);
                Main.FindValue(Dict, name + "clip_begin", ref Data.Event[i0].ClipBegin);
                Main.FindValue(Dict, name + "clip_en", ref Data.Event[i0].ClipEnd);
                Main.FindValue(Dict, name + "end", ref Data.Event[i0].End);
                Main.FindValue(Dict, name + "name", ref Data.Event[i0].Name);
                Main.FindValue(Dict, name + "param1", ref Data.Event[i0].Param1);
                Main.FindValue(Dict, name + "ref", ref Data.Event[i0].Ref);
                Main.FindValue(Dict, name + "time_ref_scale", ref Data.Event[i0].TimeRefScale);
                Main.FindValue(Dict, name + "type", ref Data.Event[i0].Type);
            }

            for (i0 = 0; i0 < Data.MObjectHRC.Length; i0++)
            {
                Data.MObjectHRC[i0] = new MObjectHRC();
                name = "m_objhrc." + i0 + d;

                if (Main.FindValue(Dict, name + "instance.length", ref value))
                    Data.MObjectHRC[i0].Instance = new Instance[int.Parse(value)];
                if (Main.FindValue(Dict, name + "node.length", ref value))
                    Data.MObjectHRC[i0].Node = new Node[int.Parse(value)];

                Main.FindValue(Dict, name + "name", ref Data.MObjectHRC[i0].Name);
                ReadA3DA(ref Data.MObjectHRC[i0].MT, name);

                if (Data.MObjectHRC[i0].Instance != null)
                    for (i1 = 0; i1 < Data.MObjectHRC[i0].Instance.Length; i1++)
                    {
                        Data.MObjectHRC[i0].Instance[i1] = new Instance();

                        name = "m_objhrc." + i0 + ".instance." + i1 + d;
                        Main.FindValue(Dict, name +     "name", ref Data.MObjectHRC[i0].Instance[i1].   Name);
                        Main.FindValue(Dict, name + "shadow"  , ref Data.MObjectHRC[i0].Instance[i1].Shadow );
                        Main.FindValue(Dict, name + "uid_name", ref Data.MObjectHRC[i0].Instance[i1].UIDName);

                        ReadA3DA(ref Data.MObjectHRC[i0].Instance[i1].MT, name);
                    }

                if (Data.MObjectHRC[i0].Node != null)
                    for (i1 = 0; i1 < Data.MObjectHRC[i0].Node.Length; i1++)
                    {
                        Data.MObjectHRC[i0].Node[i1] = new Node();
                        name = "m_objhrc." + i0 + ".node." + i1 + d;
                        Data.MObjectHRC[i0].Node[i1].MT = new ModelTransform();
                        Main.FindValue(Dict, name + "name"  , ref Data.MObjectHRC[i0].Node[i1].Name  );
                        Main.FindValue(Dict, name + "parent", ref Data.MObjectHRC[i0].Node[i1].Parent);

                        ReadA3DA(ref Data.MObjectHRC[i0].Node[i1].MT, name);
                    }
            }

            for (i0 = 0; i0 < Data.MObjectHRCList.Length; i0++)
                Main.FindValue(Dict, "m_objhrc_list." + i0, ref Data.MObjectHRCList[i0]);

            for (i0 = 0; i0 < Data.MaterialList.Length; i0++)
            {
                Data.MaterialList[i0] = new MaterialList();

                Main.FindValue(Dict, "material_list." + i0 + ".hash_name",
                    ref Data.MaterialList[i0].HashName);
                Main.FindValue(Dict, "material_list." + i0 +      ".name",
                    ref Data.MaterialList[i0].    Name);

                ReadA3DA(ref Data.MaterialList[i0].BlendColor   , name + "blend_color."   );
                ReadA3DA(ref Data.MaterialList[i0].GlowIntensity, name + "glow_intensity.");
                ReadA3DA(ref Data.MaterialList[i0].Incandescence, name + "incandescence." );
            }

            for (i0 = 0; i0 < Data.Motion.Length; i0++)
                Main.FindValue(Dict, "motion." + i0 + ".name", ref Data.Motion[i0]);

            for (i0 = 0; i0 < Data.Object.Length; i0++)
            {
                Data.Object[i0] = new Object();
                name = "object." + i0 + d;

                Main.FindValue(Dict, name + "morph", ref Data.Object[i0].Morph);
                Main.FindValue(Dict, name + "morph_offset", ref Data.Object[i0].MorphOffset);
                Main.FindValue(Dict, name + "name", ref Data.Object[i0].Name);
                Main.FindValue(Dict, name + "parent_name", ref Data.Object[i0].ParentName);
                Main.FindValue(Dict, name + "uid_name", ref Data.Object[i0].UIDName);

                if (Main.FindValue(Dict, name + "tex_pat.length", ref value))
                    Data.Object[i0].TP = new TexturePattern[int.Parse(value)];
                if (Main.FindValue(Dict, name + "tex_transform.length", ref value))
                    Data.Object[i0].TT = new TextureTransform[int.Parse(value)];

                if (Data.Object[i0].TP != null)
                    for (i1 = 0; i1 < Data.Object[i0].TP.Length; i1++)
                    {
                        Data.Object[i0].TP[i1] = new TexturePattern();
                        name = "object." + i0 + ".tex_pat." + i1 + d;
                        Main.FindValue(Dict, name + "name"      , ref Data.Object[i0].TP[i1].Name     );
                        Main.FindValue(Dict, name + "pat"       , ref Data.Object[i0].TP[i1].Pat      );
                        Main.FindValue(Dict, name + "pat_offset", ref Data.Object[i0].TP[i1].PatOffset);
                    }

                if (Data.Object[i0].TT != null)
                    for (i1 = 0; i1 < Data.Object[i0].TT.Length; i1++)
                    {
                        name = "object." + i0 + ".tex_transform." + i1 + d;
                        Data.Object[i0].TT[i1] = new TextureTransform();

                        Main.FindValue(Dict, name + "name", ref Data.Object[i0].TT[i1].Name);
                        ReadA3DA(ref Data.Object[i0].TT[i1].C , name + "coverage"      );
                        ReadA3DA(ref Data.Object[i0].TT[i1].O , name + "offset"        );
                        ReadA3DA(ref Data.Object[i0].TT[i1].R , name + "repeat"        );
                        ReadA3DA(ref Data.Object[i0].TT[i1].Ro, name + "rotate."       );
                        ReadA3DA(ref Data.Object[i0].TT[i1].RF, name + "rotateFrame."  );
                        ReadA3DA(ref Data.Object[i0].TT[i1].TF, name + "translateFrame");
                    }

                name = "object." + i0 + d;
                ReadA3DA(ref Data.Object[i0].MT, name);
            }


            for (i0 = 0; i0 < Data.ObjectHRC.Length; i0++)
            {
                Data.ObjectHRC[i0] = new ObjectHRC();
                name = "objhrc." + i0 + d;

                Main.FindValue(Dict, name + "shadow"  , ref Data.ObjectHRC[i0].Shadow );
                Main.FindValue(Dict, name + "name"    , ref Data.ObjectHRC[i0].Name   );
                Main.FindValue(Dict, name + "uid_name", ref Data.ObjectHRC[i0].UIDName);
                if (Main.FindValue(Dict, name + "node.length", ref value))
                    Data.ObjectHRC[i0].Node = new Node[int.Parse(value)];

                if (Data.ObjectHRC[i0].Node != null)
                    for (i1 = 0; i1 < Data.ObjectHRC[i0].Node.Length; i1++)
                    {
                        Data.ObjectHRC[i0].Node[i1] = new Node();
                        name = "objhrc." + i0 + ".node." + i1 + d;

                        Main.FindValue(Dict, name + "name"  , ref Data.ObjectHRC[i0].Node[i1].Name  );
                        Main.FindValue(Dict, name + "parent", ref Data.ObjectHRC[i0].Node[i1].Parent);

                        ReadA3DA(ref Data.ObjectHRC[i0].Node[i1].MT, name);
                    }
            }


            for (i0 = 0; i0 < Data.ObjectHRCList.Length; i0++)
            {
                Data.ObjectHRCList[i0] = "";
                Main.FindValue(Dict, "objhrc_list." + i0, ref Data.ObjectHRCList[i0]);
            }


            for (i0 = 0; i0 < Data.ObjectList.Length; i0++)
            {
                Data.ObjectList[i0] = "";
                Main.FindValue(Dict, "object_list." + i0, ref Data.ObjectList[i0]);
            }

            for (i0 = 0; i0 < Data.Point.Length; i0++)
            {
                Data.Point[i0] = new ModelTransform();
                ReadA3DA(ref Data.Point[i0], "point." + i0 + d);
            }

            if (Data.PostProcess.Boolean)
            {
                name = "post_process.";

                ReadA3DA(ref Data.PostProcess.Ambient  , name + "Ambient."   );
                ReadA3DA(ref Data.PostProcess.Diffuse  , name + "Diffuse."   );
                ReadA3DA(ref Data.PostProcess.Specular , name + "Specular."  );
                ReadA3DA(ref Data.PostProcess.LensFlare, name + "lens_flare.");
                ReadA3DA(ref Data.PostProcess.LensGhost, name + "lens_ghost.");
                ReadA3DA(ref Data.PostProcess.LensShaft, name + "lens_shaft.");
            }
        }
        
        private void ReadA3DA(ref ModelTransform MT, string Temp)
        {
            MT = new ModelTransform();
            Main.FindValue(Dict, Temp + MTBO, ref MT.BinOffset);

            ReadA3DA(ref MT.Rot       , Temp + "rot."       );
            ReadA3DA(ref MT.Scale     , Temp + "scale."     );
            ReadA3DA(ref MT.Trans     , Temp + "trans."     );
            ReadA3DA(ref MT.Visibility, Temp + "visibility.");
        }

        private void ReadA3DA(ref RGBAKey RGBA, string Temp)
        {
            RGBA = new RGBAKey();
            Main.FindValue(Dict, Temp.Remove(Temp.Length - 1), ref RGBA.Boolean);

            ReadA3DA(ref RGBA.A, Temp + "a.");
            ReadA3DA(ref RGBA.B, Temp + "b.");
            ReadA3DA(ref RGBA.G, Temp + "g.");
            ReadA3DA(ref RGBA.R, Temp + "r.");
        }
        
        private void ReadA3DA(ref Vector3Key Key, string Temp)
        { Key = new Vector3Key(); ReadA3DA(ref Key.X, Temp + "x.");
            ReadA3DA(ref Key.Y, Temp + "y."); ReadA3DA(ref Key.Z, Temp + "z."); }

        private void ReadA3DA(ref KeyUV UV, string Temp)
        { UV = new KeyUV(); ReadA3DA(ref UV.U, Temp + "U" + d); ReadA3DA(ref UV.V, Temp + "V" + d); }

        private void ReadA3DA(ref Key Key, string Temp)
        {
            Key = new Key();
            Main.FindValue(Dict, Temp.Remove(Temp.Length - 1), ref Key.Boolean);
            if (Main.FindValue(Dict, Temp + BO, ref Key.BinOffset)) Key.Boolean = true;
            else if (Main.FindValue(Dict, Temp + "type", ref Key.Type)) Key.Boolean = true;

            if (!Key.Boolean || Key.Type == 0x0000) return;

            if (Key.Type == 0x0001) { Main.FindValue(Dict, Temp + "value", ref Key.Value); return; }

            int i = 0, i0 = 0;
            Main.FindValue(Dict, Temp + "ep_type_post"     , ref Key.EPTypePost     );
            Main.FindValue(Dict, Temp + "ep_type_pre"      , ref Key.EPTypePre      );
            Main.FindValue(Dict, Temp + "key.length"       , ref Key.Length         );
            Main.FindValue(Dict, Temp + "max"              , ref Key.Max            );
            Main.FindValue(Dict, Temp + "raw_data_key_type", ref Key.RawData.KeyType);

            if (Key.Length != null)
            {
                Key.Trans = new Key.Transform[(int)Key.Length];
                for (i0 = 0; i0 < Key.Length; i0++)
                    if (Main.FindValue(Dict, Temp + "key." + i0 + ".data", ref value))
                    {
                        dataArray = value.Replace("(", "").Replace(")", "").Split(',');
                        Key.Trans[i0].Frame = Main.ToDouble(dataArray[0]);
                        if (dataArray.Length > 1) { Key.Trans[i0].
                                Value1 = Main.ToDouble(dataArray[1]);
                            if (dataArray.Length > 2) { Key.Trans[i0].
                                    Value2 = Main.ToDouble(dataArray[2]);
                                if (dataArray.Length > 3) Key.Trans[i0].
                                        Value3 = Main.ToDouble(dataArray[3]); } }
                        Key.Trans[i0].Type = dataArray.Length - 1;
                    }
            }
            else if (Key.RawData.KeyType != null)
            {
                Main.FindValue(Dict, Temp + "raw_data.value_type",
                    ref Key.RawData.ValueType);
                if (Main.FindValue(Dict, Temp + "raw_data.value_list",
                    ref Key.RawData.ValueListString))
                    Key.RawData.ValueList = Key.RawData.ValueListString.Split(',');
                Main.FindValue(Dict, Temp + "raw_data.value_list_size",
                    ref Key.RawData.ValueListSize);

                int DataSize = (int)Key.RawData.KeyType + 1;
                Key.Length = Key.RawData.ValueListSize / DataSize;
                Key.Trans = new Key.Transform[(int)Key.Length];
                for (i = 0; i < Key.Length; i++)
                {
                    Key.Trans[i].Type = (int)Key.RawData.KeyType;
                    Key.Trans[i].Frame = Main.ToDouble(Key.RawData.ValueList[i * DataSize + 0]);
                    if (Key.Trans[i].Type > 0) { Key.Trans[i].Value1 = Main.
                            ToDouble(Key.RawData.ValueList[i * DataSize + 1]);
                        if (Key.Trans[i].Type > 1) { Key.Trans[i].Value2 = Main.
                                ToDouble(Key.RawData.ValueList[i * DataSize + 2]);
                            if (Key.Trans[i].Type > 2) Key.Trans[i].Value3 = Main.
                                    ToDouble(Key.RawData.ValueList[i * DataSize + 3]); } }
                }
                Key.RawData = new Key.RawD();
            }
        }

        public void A3DAWriter(string file)
        {
            int i0 = 0;
            int i1 = 0;
            DateTime date = DateTime.Now;
            if (A3DC && Data._.CompressF16 != null)
                if (Data._.CompressF16 != 0)
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
            
            if (Data.Ambient.Length != 0 && Data.Header.Format == Main.Format.MGF)
            {
                SO0 = SortWriter(Data.Ambient.Length);
                SOi0 = 0;
                for (i0 = 0; i0 < Data.Ambient.Length; i0++)
                {
                    SOi0 = SO0[i0];
                    name = "ambient." + SOi0 + d;

                    WriteA3DA(ref Data.Ambient[SOi0].   LightDiffuse, name,    "light.Diffuse");
                    IO.Write(name + "name=", Data.Ambient[SOi0].Name);
                    WriteA3DA(ref Data.Ambient[SOi0].RimLightDiffuse, name, "rimlight.Diffuse");
                }
                IO.Write("fog.length=", Data.Fog.Length);
            }

            if (Data.Camera.Auxiliary.Boolean)
            {
                name = "camera_auxiliary.";
                WriteA3DA(ref Data.Camera.Auxiliary.AutoExposure, name, "auto_exposure");
                WriteA3DA(ref Data.Camera.Auxiliary.    Exposure, name, "exposure"     );
                WriteA3DA(ref Data.Camera.Auxiliary.Gamma       , name, "gamma"        );
                WriteA3DA(ref Data.Camera.Auxiliary.GammaRate   , name, "gamma_rate"   );
                WriteA3DA(ref Data.Camera.Auxiliary.Saturate    , name, "saturate"     );
            }

            if (Data.Camera.Root.Length != 0)
            {
                SO0 = SortWriter(Data.Camera.Root.Length);
                SOi0 = 0;
                for (i0 = 0; i0 < Data.Camera.Root.Length; i0++)
                {
                    SOi0 = SO0[i0];
                    name = "camera_root." + SOi0 + d;
                    nameView = name + "view_point.";

                    WriteA3DA(ref Data.Camera.Root[SOi0].Interest, name + "interest.", 0b11111);
                    WriteA3DA(ref Data.Camera.Root[SOi0].MT, name, 0b11110);
                    IO.Write(nameView + "aspect=", Data.Camera.Root[i0].ViewPoint.Aspect);
                    if (Data.Camera.Root[i0].ViewPoint.CameraApertureH != null)
                        IO.Write(nameView + "camera_aperture_h=",
                            Data.Camera.Root[i0].ViewPoint.CameraApertureH);
                    if (Data.Camera.Root[i0].ViewPoint.CameraApertureW != null)
                        IO.Write(nameView + "camera_aperture_w=",
                            Data.Camera.Root[i0].ViewPoint.CameraApertureW);
                    WriteA3DA(ref Data.Camera.Root[SOi0].ViewPoint.FocalLength, nameView + "focal_length.");
                    WriteA3DA(ref Data.Camera.Root[SOi0].ViewPoint.FOV, nameView + "fov.");
                    IO.Write(nameView + "fov_is_horizontal=",
                        Data.Camera.Root[i0].ViewPoint.FOVHorizontal);
                    WriteA3DA(ref Data.Camera.Root[SOi0].ViewPoint.MT, nameView, 0b10000);
                    WriteA3DA(ref Data.Camera.Root[SOi0].ViewPoint.Roll, nameView + "roll.");
                    WriteA3DA(ref Data.Camera.Root[SOi0].ViewPoint.MT, nameView, 0b01111);
                    WriteA3DA(ref Data.Camera.Root[SOi0].MT, name, 0b00001);
                }
                IO.Write("camera_root.length=", Data.Camera.Root.Length);
            }

            if (Data.Chara.Length != 0)
            {
                SO0 = SortWriter(Data.Chara.Length);
                name = "chara.";

                for (i0 = 0; i0 < Data.Chara.Length; i0++)
                    WriteA3DA(ref Data.Chara[SO0[i0]], name + SO0[i0] + d, 0b11111);
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

                    WriteA3DA(ref Data.Curve[SOi0].CV, name + "cv.");
                    IO.Write(name + "name=", Data.Curve[SOi0].Name);
                }
                IO.Write("curve.length=", Data.Curve.Length);
            }

            if (Data.DOF.Name != null && Data.Header.Format == Main.Format.FT)
            {
                IO.Write("dof.name=", Data.DOF.Name);
                WriteA3DA(ref Data.DOF.MT, "dof.", 0b11111);
            }

            if (Data.Event.Length != 0)
            {
                SO0 = SortWriter(Data.Event.Length);
                SOi0 = 0;
                for (i0 = 0; i0 < Data.Event.Length; i0++)
                {
                    SOi0 = SO0[i0];
                    name = "event." + SOi0 + d;

                    IO.Write(name + "begin="         , Data.Event[SOi0].Begin       );
                    IO.Write(name + "clip_begin="    , Data.Event[SOi0].ClipBegin   );
                    IO.Write(name + "clip_en="       , Data.Event[SOi0].ClipEnd     );
                    IO.Write(name + "end="           , Data.Event[SOi0].End         );
                    IO.Write(name + "name="          , Data.Event[SOi0].Name        );
                    IO.Write(name + "param1="        , Data.Event[SOi0].Param1      );
                    IO.Write(name + "ref="           , Data.Event[SOi0].Ref         );
                    IO.Write(name + "time_ref_scale=", Data.Event[SOi0].TimeRefScale);
                    IO.Write(name + "type="          , Data.Event[SOi0].Type        );
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

                    WriteA3DA(ref Data.Fog[SOi0].Diffuse, name, "Diffuse");
                    WriteA3DA(ref Data.Fog[SOi0].Density, name, "density");
                    WriteA3DA(ref Data.Fog[SOi0].End    , name, "end"    );
                    IO.Write(name + "id=", Data.Fog[SOi0].Id);
                    WriteA3DA(ref Data.Fog[SOi0].Start  , name, "start"  );
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

                    WriteA3DA(ref Data.Light[SOi0].Ambient      , name, "Ambient"      );
                    WriteA3DA(ref Data.Light[SOi0].Diffuse      , name, "Diffuse"      );
                    WriteA3DA(ref Data.Light[SOi0].Incandescence, name, "Incandescence");
                    WriteA3DA(ref Data.Light[SOi0].Specular     , name, "Specular"     );
                    IO.Write(name + "id=", Data.Light[SOi0].Id);
                    IO.Write(name + "name=", Data.Light[SOi0].Name);
                    WriteA3DA(ref Data.Light[SOi0].Position     , name + "position."      , 0b11111);
                    WriteA3DA(ref Data.Light[SOi0].SpotDirection, name + "spot_direction.", 0b11111);
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
                        nameView = "m_objhrc." + SOi0 + ".instance.";
                        int[] SO1 = SortWriter(Data.MObjectHRC[i0].Instance.Length);
                        int SOi1 = 0;
                        for (i1 = 0; i1 < Data.MObjectHRC[i0].Instance.Length; i1++)
                        {
                            SOi1 = SO1[i1];
                            WriteA3DA(ref Data.MObjectHRC[SOi0].Instance[SOi1].MT, nameView + SOi1 + d, 0b10000);
                            IO.Write(nameView + SOi1 +     ".name=", Data.MObjectHRC[SOi0].Instance[SOi1].   Name);
                            WriteA3DA(ref Data.MObjectHRC[SOi0].Instance[SOi1].MT, nameView + SOi1 + d, 0b01100);
                            IO.Write(nameView + SOi1 + ".shadow="  , Data.MObjectHRC[SOi0].Instance[SOi1].Shadow );
                            WriteA3DA(ref Data.MObjectHRC[SOi0].Instance[SOi1].MT, nameView + SOi1 + d, 0b00010);
                            IO.Write(nameView + SOi1 + ".uid_name=", Data.MObjectHRC[SOi0].Instance[SOi1].UIDName);
                            WriteA3DA(ref Data.MObjectHRC[SOi0].Instance[SOi1].MT, nameView + SOi1 + d, 0b00001);
                        }
                        IO.Write(nameView + "length=", Data.MObjectHRC[SOi0].Instance.Length);
                    }
                    WriteA3DA(ref Data.MObjectHRC[SOi0].MT, name, 0b10000);
                    IO.Write(name + "name=", Data.MObjectHRC[SOi0].Name);
                    if (Data.MObjectHRC[SOi0].Node != null)
                    {
                        nameView = "m_objhrc." + SOi0 + ".node.";
                        int[] SO1 = SortWriter(Data.MObjectHRC[i0].Node.Length);
                        int SOi1 = 0;
                        for (i1 = 0; i1 < Data.MObjectHRC[i0].Node.Length; i1++)
                        {
                            SOi1 = SO1[i1];
                            WriteA3DA(ref Data.MObjectHRC[SOi0].Node[SOi1].MT, nameView + SOi1 + d, 0b10000);
                            IO.Write(nameView + SOi1 + ".name="  , Data.MObjectHRC[SOi0].Node[SOi1].Name  );
                            IO.Write(nameView + SOi1 + ".parent=", Data.MObjectHRC[SOi0].Node[SOi1].Parent);
                            WriteA3DA(ref Data.MObjectHRC[SOi0].Node[SOi1].MT, nameView + SOi1 + d, 0b01111);

                        }
                        IO.Write(nameView + "length=", Data. MObjectHRC[SOi0].Node.Length);
                    }
                    WriteA3DA(ref Data.MObjectHRC[SOi0].MT, name, 0b01111);
                }
                IO.Write("m_objhrc.length=", Data.MObjectHRC.Length);
            }

            if (Data.MObjectHRCList.Length != 0)
            {
                SO0 = SortWriter(Data.MObjectHRCList.Length);
                name = "m_objhrc_list.";
                for (i0 = 0; i0 < Data.MObjectHRCList.Length; i0++)
                    IO.Write(name + SO0[i0] + "=", Data.MObjectHRCList[SO0[i0]]);
                IO.Write(name + "length=", Data.MObjectHRCList.Length);
            }
            
            if (Data.MaterialList.Length != 0 && Data.Header.Format == Main.Format.X)
            {
                SO0 = SortWriter(Data.MaterialList.Length);
                name = "material_list.";
                for (i0 = 0; i0 < Data.MaterialList.Length; i0++)
                {
                    WriteA3DA(ref Data.MaterialList[SO0[i0]].BlendColor   , name + SO0[i0] + d,  "blend_color."  );
                    WriteA3DA(ref Data.MaterialList[SO0[i0]].GlowIntensity, name + SO0[i0] + d + "glow_intensity");
                    IO.Write(name + SO0[i0] + "hash_name=", Data.MaterialList[SO0[i0]].HashName);
                    WriteA3DA(ref Data.MaterialList[SO0[i0]].Incandescence, name + SO0[i0] + d,  "incandescence.");
                    IO.Write(name + SO0[i0] + "name=", Data.MaterialList[SO0[i0]].Name);
                }
                IO.Write(name + "length=", Data.MaterialList.Length);
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

                    WriteA3DA(ref Data.Object[SOi0].MT, name, 0b10000);
                    if (Data.Object[SOi0].Morph != null)
                    {
                        IO.Write(name + "morph="       , Data.Object[SOi0].Morph      );
                        IO.Write(name + "morph_offset=", Data.Object[SOi0].MorphOffset);
                    }
                    IO.Write(name + "name="       , Data.Object[SOi0].Name      );
                    IO.Write(name + "parent_name=", Data.Object[SOi0].ParentName);
                    WriteA3DA(ref Data.Object[SOi0].MT, name, 0b01100);

                    if (Data.Object[SOi0].TP != null)
                    {
                        nameView = name + "tex_pat.";

                        SO1 = SortWriter(Data.Object[SOi0].TP.Length);
                        SOi1 = 0;
                        for (i1 = 0; i1 < Data.Object[SOi0].TP.Length; i1++)
                        {
                            SOi1 = SO1[i1];
                            IO.Write(nameView + SOi1 + ".name="      , Data.Object[SOi0].TP[SOi1].Name     );
                            IO.Write(nameView + SOi1 + ".pat="       , Data.Object[SOi0].TP[SOi1].Pat      );
                            IO.Write(nameView + SOi1 + ".pat_offset=", Data.Object[SOi0].TP[SOi1].PatOffset);
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
                            WriteA3DA(ref Data.Object[SOi0].TT[SOi1].C , nameView + SOi1 + d, "coverage"      );
                            WriteA3DA(ref Data.Object[SOi0].TT[SOi1].O , nameView + SOi1 + d, "offset"        );
                            WriteA3DA(ref Data.Object[SOi0].TT[SOi1].R , nameView + SOi1 + d, "repeat"        );
                            WriteA3DA(ref Data.Object[SOi0].TT[SOi1].Ro, nameView + SOi1 + d, "rotate"        );
                            WriteA3DA(ref Data.Object[SOi0].TT[SOi1].RF, nameView + SOi1 + d, "rotateFrame"   );
                            WriteA3DA(ref Data.Object[SOi0].TT[SOi1].TF, nameView + SOi1 + d, "translateFrame");
                        }
                        IO.Write(nameView + "length=" + Data.Object[SOi0].TT.Length + "\n");
                    }

                    WriteA3DA(ref Data.Object[SOi0].MT, name, 0b00010);
                    IO.Write(name + "uid_name=", Data.Object[SOi0].UIDName);
                    WriteA3DA(ref Data.Object[SOi0].MT, name, 0b00001);
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
                            WriteA3DA(ref Data.ObjectHRC[SOi0].Node[SOi1].MT, name + SOi1 + d, 0b10000);
                            IO.Write(name + SOi1 + ".name="  , Data.ObjectHRC[SOi0].Node[SOi1].Name  );
                            IO.Write(name + SOi1 + ".parent=", Data.ObjectHRC[SOi0].Node[SOi1].Parent);
                            WriteA3DA(ref Data.ObjectHRC[SOi0].Node[SOi1].MT, name + SOi1 + d, 0b01111);
                        }
                        IO.Write(name + "length=",
                            Data.ObjectHRC[SOi0].Node.Length);
                    }
                    name = "objhrc." + SOi0 + d;

                    if (Data.ObjectHRC[SOi0].Shadow != null)
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
            if (Data.PlayControl.Div    != null && A3DC)
                IO.Write("play_control.div=", Data.PlayControl.Div);
            IO.Write("play_control.fps=", Data.PlayControl.FPS);
            if (Data.PlayControl.Offset != null)
            { if ( A3DC) { IO.Write("play_control.offset=", Data.PlayControl.Offset); 
                           IO.Write("play_control.size="  , Data.PlayControl.Size  ); }
              else IO.Write("play_control.size=", Data.PlayControl.Size + Data.PlayControl.Offset);
            }
            else   IO.Write("play_control.size=", Data.PlayControl.Size);

            if (Data.PostProcess.Boolean)
            {
                name = "post_process.";
                WriteA3DA(ref Data.PostProcess.Ambient  , name, "Ambient");
                WriteA3DA(ref Data.PostProcess.Diffuse  , name, "Diffuse");
                WriteA3DA(ref Data.PostProcess.Specular , name, "Specular");
                WriteA3DA(ref Data.PostProcess.LensFlare, name, "lens_flare");
                WriteA3DA(ref Data.PostProcess.LensGhost, name, "lens_ghost");
                WriteA3DA(ref Data.PostProcess.LensShaft, name, "lens_shaft");
            }

            SO0 = SortWriter(Data.Point.Length);
            if (Data.Point.Length != 0)
            {
                for (i0 = 0; i0 < Data.Point.Length; i0++)
                    WriteA3DA(ref Data.Point[SO0[i0]], "point." + SO0[i0] + d, 0b11111);
                IO.Write("point.length=", Data.Point.Length);
            }

            if (!A3DC)
                IO.Close();
        }
        
        private void WriteA3DA(ref ModelTransform MT, string Temp, byte Flags)
        {
            if (A3DC && !MT.Writed && ((Flags & 0b10000) >> 4) == 1)
            { IO.Write(Temp + MTBO + "=", MT.BinOffset); MT.Writed = true; }

            if (A3DC) return;
            
            if (((Flags & 0b01000) >> 3) == 1) WriteA3DA(ref MT.Rot       , Temp + "rot."       );
            if (((Flags & 0b00100) >> 2) == 1) WriteA3DA(ref MT.Scale     , Temp + "scale."     );
            if (((Flags & 0b00010) >> 1) == 1) WriteA3DA(ref MT.Trans     , Temp + "trans."     );
            if ( (Flags & 0b00001)       == 1) WriteA3DA(ref MT.Visibility, Temp + "visibility.");
        }

        private void WriteA3DA(ref RGBAKey RGBA, string Temp, string Data)
        {
            if (RGBA.Boolean)
            {
                IO.Write(Temp + Data + "=", ref RGBA.Boolean);
                if (RGBA.A.Boolean)
                    WriteA3DA(ref RGBA.A, Temp + Data + ".a.");
                WriteA3DA(ref RGBA.B, Temp + Data + ".b.");
                WriteA3DA(ref RGBA.G, Temp + Data + ".g.");
                WriteA3DA(ref RGBA.R, Temp + Data + ".r.");
            }
        }

        private void WriteA3DA(ref Vector3Key Key, string Temp)
        { WriteA3DA(ref Key.X, Temp + "x."); WriteA3DA(ref Key.Y,
            Temp + "y."); WriteA3DA(ref Key.Z, Temp + "z."); }

        private void WriteA3DA(ref KeyUV UV, string Temp, string Data)
        { WriteA3DA(ref UV.U, Temp, Data + "U"); WriteA3DA(ref UV.V, Temp, Data + "V"); }

        private void WriteA3DA(ref Key Key, string Temp, string Data)
        { if (Key.Boolean) { IO.Write(Temp + Data + "=", ref Key.Boolean);
                WriteA3DA(ref Key, Temp + Data + d); } }

        private void WriteA3DA(ref Key Key, string Temp)
        {
            if (!Key.Boolean) return;

            if (A3DC) { IO.Write(Temp + BO + "=", Key.BinOffset); return; }

            int i = 0;
            if (Key.Trans == null)
            {
                IO.Write(Temp + "type=", Key.Type);
                if (Key.Type != 0x0000) IO.Write(Temp + "value=", Key.Value);
                return;
            }

            SO = SortWriter(Key.Trans.Length);
            if (Key.EPTypePost != null)
                IO.Write(Temp + "ep_type_post=", Key.EPTypePost);
            if (Key.EPTypePre != null)
                IO.Write(Temp + "ep_type_pre=", Key.EPTypePre);
            if (Key.Length < 100)
            {
                for (i = 0; i < Key.Trans.Length; i++)
                {
                    SOi = SO[i];
                    IO.Write(Temp + "key." + SOi + ".data=");
                    if (Key.Trans[SOi].Type > 0) IO.Write("(");
                    IO.Write(Main.ToString((float)Key.Trans[SOi].Frame));
                    if (Key.Trans[SOi].Type > 0) { IO.Write("," + Main.ToString((float)Key.Trans[SOi].Value1));
                        if (Key.Trans[SOi].Type > 1) { IO.Write("," + Main.ToString((float)Key.Trans[SOi].Value2));
                            if (Key.Trans[SOi].Type > 2) IO.Write("," + Main.ToString((float)Key.Trans[SOi].Value3)); }
                        IO.Write(")"); }
                    IO.Write("\n");
                    IO.Write(Temp + "key." + SOi + ".type=", Key.Trans[SOi].Type);
                }
                IO.Write(Temp + "key.length=", Key.Length);
            }
            if (Key.Max != null)
                IO.Write(Temp + "max=", Key.Max);
            if (Key.Length >= 100)
            {
                Key.RawData = new Key.RawD();
                for (i = 0; i < Key.Trans.Length; i++)
                    if (Key.RawData.KeyType < Key.Trans[i].Type || Key.RawData.KeyType == null)
                        Key.RawData.KeyType = Key.Trans[i].Type;
                    else if (Key.RawData.KeyType == 3) break;
                Key.RawData.ValueListSize = Key.Trans.Length * (Key.RawData.KeyType + 1);
                IO.Write(Temp + "raw_data.value_list=");
                for (i = 0; i < Key.Trans.Length; i++)
                {
                    IO.Write(Main.ToString(Key.Trans[i].Frame));
                    if (Key.RawData.KeyType > 0) { IO.Write("," + Main.ToString(Key.Trans[i].Value1));
                        if (Key.RawData.KeyType > 1) { IO.Write("," + Main.ToString(Key.Trans[i].Value2));
                            if (Key.RawData.KeyType > 2) IO.Write("," + Main.ToString(Key.Trans[i].Value3)); } }
                    if (i < Key.Trans.Length - 1) IO.Write(',');
                }
                IO.Write('\n');
                IO.Write(Temp + "raw_data.value_list_size=", Key.RawData.ValueListSize);
                IO.Write(Temp + "raw_data.value_type=", Key.RawData.ValueType);
                IO.Write(Temp + "raw_data_key_type=", Key.RawData.KeyType);
                Key.RawData = new Key.RawD();
            }
            IO.Write(Temp + "type=", Key.Type & 0xFF);
        }        

        private void A3DCReader()
        {
            int i0 = 0;
            int i1 = 0;

            for (i0 = 0; i0 < Data.Ambient.Length; i0++)
            {
                ReadA3DC(ref Data.Ambient[i0].   LightDiffuse);
                ReadA3DC(ref Data.Ambient[i0].RimLightDiffuse);
            }

            if (Data.Camera.Auxiliary.Boolean)
            {
                ReadA3DC(ref Data.Camera.Auxiliary.AutoExposure);
                ReadA3DC(ref Data.Camera.Auxiliary.    Exposure);
                ReadA3DC(ref Data.Camera.Auxiliary.Gamma       );
                ReadA3DC(ref Data.Camera.Auxiliary.GammaRate   );
                ReadA3DC(ref Data.Camera.Auxiliary.Saturate    );
            }

            for (i0 = 0; i0 < Data.Camera.Root.Length; i0++)
            {
                ReadA3DC(ref Data.Camera.Root[i0]          .MT         );
                ReadA3DC(ref Data.Camera.Root[i0].Interest             );
                ReadA3DC(ref Data.Camera.Root[i0].ViewPoint.MT         );
                ReadA3DC(ref Data.Camera.Root[i0].ViewPoint.FocalLength);
                ReadA3DC(ref Data.Camera.Root[i0].ViewPoint.FOV        );
                ReadA3DC(ref Data.Camera.Root[i0].ViewPoint.Roll       );
            }

            for (i0 = 0; i0 < Data.Chara.Length; i0++)
                ReadA3DC(ref Data.Chara[i0]);

            for (i0 = 0; i0 < Data.Curve.Length; i0++)
                ReadA3DC(ref Data.Curve[i0].CV);

            if (Data.DOF.Name != null)
                ReadA3DC(ref Data.DOF.MT);

            for (i0 = 0; i0 < Data.Fog.Length; i0++)
            {
                ReadA3DC(ref Data.Fog[i0].Density);
                ReadA3DC(ref Data.Fog[i0].Diffuse);
                ReadA3DC(ref Data.Fog[i0].End    );
                ReadA3DC(ref Data.Fog[i0].Start  );
            }

            for (i0 = 0; i0 < Data.Light.Length; i0++)
            {
                ReadA3DC(ref Data.Light[i0].Ambient      );
                ReadA3DC(ref Data.Light[i0].Diffuse      );
                ReadA3DC(ref Data.Light[i0].Incandescence);
                ReadA3DC(ref Data.Light[i0].Position     );
                ReadA3DC(ref Data.Light[i0].Specular     );
                ReadA3DC(ref Data.Light[i0].SpotDirection);
            }

            for (i0 = 0; i0 < Data.MObjectHRC.Length; i0++)
            {
                ReadA3DC(ref Data.MObjectHRC[i0].MT);

                if (Data.MObjectHRC[i0].Instance != null)
                    for (i1 = 0; i1 < Data.MObjectHRC[i0].Instance.Length; i1++)
                        ReadA3DC(ref Data.MObjectHRC[i0].Instance[i1].MT);

                if (Data.MObjectHRC[i0].Node != null)
                    for (i1 = 0; i1 < Data.MObjectHRC[i0].Node.Length; i1++)
                        ReadA3DC(ref Data.MObjectHRC[i0].Node[i1].MT);
            }

            for (i0 = 0; i0 < Data.MaterialList.Length; i0++)
            {
                ReadA3DC(ref Data.MaterialList[i0].BlendColor   );
                ReadA3DC(ref Data.MaterialList[i0].GlowIntensity);
                ReadA3DC(ref Data.MaterialList[i0].Incandescence);
            }

            for (i0 = 0; i0 < Data.Object.Length; i0++)
            {
                ReadA3DC(ref Data.Object[i0].MT);
                if (Data.Object[i0].TT == null)
                    continue;

                for (i1 = 0; i1 < Data.Object[i0].TT.Length; i1++)
                {
                    ReadA3DC(ref Data.Object[i0].TT[i1].C );
                    ReadA3DC(ref Data.Object[i0].TT[i1].O );
                    ReadA3DC(ref Data.Object[i0].TT[i1].R );
                    ReadA3DC(ref Data.Object[i0].TT[i1].Ro);
                    ReadA3DC(ref Data.Object[i0].TT[i1].RF);
                    ReadA3DC(ref Data.Object[i0].TT[i1].TF);
                }
            }

            for (i0 = 0; i0 < Data.ObjectHRC.Length; i0++)
            {
                if (Data.ObjectHRC[i0].Node == null)
                    continue;

                for (i1 = 0; i1 < Data.ObjectHRC[i0].Node.Length; i1++)
                    ReadA3DC(ref Data.ObjectHRC[i0].Node[i1].MT);
            }


            for (i0 = 0; i0 < Data.Point.Length; i0++)
                ReadA3DC(ref Data.Point[i0]);

            if (Data.PostProcess.Boolean)
            {
                ReadA3DC(ref Data.PostProcess.Ambient  );
                ReadA3DC(ref Data.PostProcess.Diffuse  );
                ReadA3DC(ref Data.PostProcess.Specular );
                ReadA3DC(ref Data.PostProcess.LensFlare);
                ReadA3DC(ref Data.PostProcess.LensGhost);
                ReadA3DC(ref Data.PostProcess.LensShaft);
            }
        }

        private void ReadA3DC(ref ModelTransform MT)
        {
            if (MT.BinOffset != null)
            {
                IO.Position = Offset + (int)MT.BinOffset;
                ReadA3DC(ref MT.Scale     , false);
                ReadA3DC(ref MT.Rot       ,  true);
                ReadA3DC(ref MT.Trans     , false);
                MT.Visibility.BinOffset = IO.ReadInt32();
                ReadA3DC(ref MT.Visibility, false);
            }
        }

        private void ReadA3DC(ref RGBAKey RGBA)
        {
            int CurrentOffset = IO.Position;
            ReadA3DC(ref RGBA.R);
            ReadA3DC(ref RGBA.G);
            ReadA3DC(ref RGBA.B);
            ReadA3DC(ref RGBA.A);
            RGBA.Boolean = RGBA.R.Boolean || RGBA.G.Boolean || RGBA.B.Boolean || RGBA.A.Boolean;
            IO.Position = CurrentOffset;
        }

        private void ReadA3DC(ref Vector3Key Key, bool CompressF16)
        {
            Key.X.BinOffset = IO.ReadInt32();
            Key.Y.BinOffset = IO.ReadInt32();
            Key.Z.BinOffset = IO.ReadInt32();
            int CurrentOffset = IO.Position;
            ReadA3DC(ref Key.X, CompressF16);
            ReadA3DC(ref Key.Y, CompressF16);
            ReadA3DC(ref Key.Z, CompressF16);
            IO.Position = CurrentOffset;
        }

        private void ReadA3DC(ref KeyUV UV)
        { ReadA3DC(ref UV.U); ReadA3DC(ref UV.V); }

        private void ReadA3DC(ref Key Key) =>
            ReadA3DC(ref Key, false);

        public void ReadA3DC(ref Key Key, bool CompressF16)
        {
            if (Key.BinOffset == null || Key.BinOffset < 0)
                return;

            IO.Position = Offset + (int)Key.BinOffset;
            Key.Boolean = true;
            Key.Type = IO.ReadInt32();

            Key.Value = IO.ReadSingle();
            if (Key.Type == 0x0000 || Key.Type == 0x0001) return;

            Key.Max = IO.ReadSingle();
            Key.Length = IO.ReadInt32();
            Key.Trans = new Key.Transform[(int)Key.Length];
            for (int i = 0; i < Key.Length; i++)
            {
                Key.Trans[i].Type = 3;
                if (CompressF16 && Data._.CompressF16 > 0)
                { Key.Trans[i].Frame = IO.ReadUInt16(); Key.Trans[i].Value1 = (double)IO.ReadHalf(); }
                else
                { Key.Trans[i].Frame = IO.ReadSingle(); Key.Trans[i].Value1 = IO.ReadSingle(); }

                if (CompressF16 && Data._.CompressF16 == 2)
                { Key.Trans[i].Value2 = (double)IO.ReadHalf(); Key.Trans[i].Value3 = (double)IO.ReadHalf(); }
                else
                { Key.Trans[i].Value2 = IO.ReadSingle(); Key.Trans[i].Value3 = IO.ReadSingle(); }

                if (Key.Trans[i].Value3 == 0) { Key.Trans[i].Type--;
                    if (Key.Trans[i].Value2 == 0) { Key.Trans[i].Type--;
                        if (Key.Trans[i].Value1 == 0) Key.Trans[i].Type--; } }
            }
        }

        public void A3DCWriter(string file)
        {
            A3DC = true;

            int i0 = 0;
            int i1 = 0;
            if (A3DCOpt)
                UsedValues = new Values();

            if (Data.Header.Format > Main.Format.FT)
            {
                int a = int.Parse(Data._.ConverterVersion);
                int b = BitConverter.ToInt32(Text.ToUTF8(Data._.FileName), 0);
                int c = int.Parse(Data._.PropertyVersion);
                int d = (int)((long)a * b * c);

                IO.Write(Text.ToASCII("A3DA"));
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
            else Data._.CompressF16 = null;

            int A3DCStart = IO.Position;
            IO.Seek(0x40, SeekOrigin.Current);

            Offset = IO.Position;

            IO.Close();
            IO = new KKtIO(new MemoryStream());

            for (byte i = 0; i < 2; i++)
            {
                bool ReturnToOffset = i == 1;
                IO.Position = 0;

                for (i0 = 0; i0 < Data.Camera.Root.Length; i0++)
                {
                    WriteA3DCOffset(ref Data.Camera.Root[i0].Interest    , ReturnToOffset);
                    WriteA3DCOffset(ref Data.Camera.Root[i0].          MT, ReturnToOffset);
                    WriteA3DCOffset(ref Data.Camera.Root[i0].ViewPoint.MT, ReturnToOffset);
                }

                if (Data.DOF.Name != null)
                    WriteA3DCOffset(ref Data.DOF.MT, ReturnToOffset);

                for (i0 = 0; i0 < Data.Light.Length; i0++)
                {
                    WriteA3DCOffset(ref Data.Light[i0].Position     , ReturnToOffset);
                    WriteA3DCOffset(ref Data.Light[i0].SpotDirection, ReturnToOffset);
                }

                for (i0 = 0; i0 < Data.MObjectHRC.Length; i0++)
                {
                    if (Data.MObjectHRC[i0].Instance != null)
                        for (i1 = 0; i1 < Data.MObjectHRC[i0].Instance.Length; i1++)
                            WriteA3DCOffset(ref Data.MObjectHRC[i0].Instance[i1].MT, ReturnToOffset);

                    WriteA3DCOffset(ref Data.MObjectHRC[i0].MT, ReturnToOffset);

                    if (Data.MObjectHRC[i0].Node != null)
                        for (i1 = 0; i1 < Data.MObjectHRC[i0].Node.Length; i1++)
                            WriteA3DCOffset(ref Data.MObjectHRC[i0].Node[i1].MT, ReturnToOffset);
                }

                for (i0 = 0; i0 < Data.Object.Length; i0++)
                    WriteA3DCOffset(ref Data.Object[i0].MT, ReturnToOffset);

                for (i0 = 0; i0 < Data.ObjectHRC.Length; i0++)
                    if (Data.ObjectHRC[i0].Node != null)
                        for (i1 = 0; i1 < Data.ObjectHRC[i0].Node.Length; i1++)
                            WriteA3DCOffset(ref Data.ObjectHRC[i0].Node[i1].MT, ReturnToOffset);

                if (!ReturnToOffset)
                {
                    for (i0 = 0; i0 < Data.Ambient.Length; i0++)
                    {
                        WriteA3DC(ref Data.Ambient[i0].   LightDiffuse);
                        WriteA3DC(ref Data.Ambient[i0].RimLightDiffuse);
                    }

                    if (Data.Camera.Auxiliary.Boolean)
                    {
                        WriteA3DC(ref Data.Camera.Auxiliary.AutoExposure);
                        WriteA3DC(ref Data.Camera.Auxiliary.Exposure    );
                        WriteA3DC(ref Data.Camera.Auxiliary.Gamma       );
                        WriteA3DC(ref Data.Camera.Auxiliary.GammaRate   );
                        WriteA3DC(ref Data.Camera.Auxiliary.Saturate    );
                    }

                    for (i0 = 0; i0 < Data.Camera.Root.Length; i0++)
                    {
                        WriteA3DC(ref Data.Camera.Root[i0].Interest             );
                        WriteA3DC(ref Data.Camera.Root[i0].          MT         );
                        WriteA3DC(ref Data.Camera.Root[i0].ViewPoint.MT         );
                        WriteA3DC(ref Data.Camera.Root[i0].ViewPoint.FOV        );
                        WriteA3DC(ref Data.Camera.Root[i0].ViewPoint.FocalLength);
                        WriteA3DC(ref Data.Camera.Root[i0].ViewPoint.Roll       );
                    }

                    for (i0 = 0; i0 < Data.Chara.Length; i0++)
                        WriteA3DC(ref Data.Chara[i0]);

                    for (i0 = 0; i0 < Data.Curve.Length; i0++)
                        WriteA3DC(ref Data.Curve[i0].CV);

                    if (Data.DOF.Name != null && Data.Header.Format == Main.Format.FT)
                        WriteA3DC(ref Data.DOF.MT);

                    for (i0 = 0; i0 < Data.Fog.Length; i0++)
                    {
                        WriteA3DC(ref Data.Fog[i0].Density);
                        WriteA3DC(ref Data.Fog[i0].Diffuse);
                        WriteA3DC(ref Data.Fog[i0].End    );
                        WriteA3DC(ref Data.Fog[i0].Start  );
                    }

                    for (i0 = 0; i0 < Data.Light.Length; i0++)
                    {
                        WriteA3DC(ref Data.Light[i0].Ambient      );
                        WriteA3DC(ref Data.Light[i0].Diffuse      );
                        WriteA3DC(ref Data.Light[i0].Incandescence);
                        WriteA3DC(ref Data.Light[i0].Specular     );
                        WriteA3DC(ref Data.Light[i0].Position     );
                        WriteA3DC(ref Data.Light[i0].SpotDirection);
                    }

                    for (i0 = 0; i0 < Data.MObjectHRC.Length; i0++)
                    {
                        if (Data.MObjectHRC[i0].Instance != null)
                            for (i1 = 0; i1 < Data.MObjectHRC[i0].Instance.Length; i1++)
                                WriteA3DC(ref Data.MObjectHRC[i0].Instance[i1].MT);

                        WriteA3DC(ref Data.MObjectHRC[i0].MT);

                        if (Data.MObjectHRC[i0].Node != null)
                            for (i1 = 0; i1 < Data.MObjectHRC[i0].Node.Length; i1++)
                                WriteA3DC(ref Data.MObjectHRC[i0].Node[i1].MT);
                    }

                    if (Data.Header.Format == Main.Format.X)
                        for (i0 = 0; i0 < Data.MaterialList.Length; i0++)
                        {
                            WriteA3DC(ref Data.MaterialList[SO0[i0]].BlendColor   );
                            WriteA3DC(ref Data.MaterialList[SO0[i0]].GlowIntensity);
                            WriteA3DC(ref Data.MaterialList[SO0[i0]].Incandescence);
                        }

                    for (i0 = 0; i0 < Data.Object.Length; i0++)
                    {
                        WriteA3DC(ref Data.Object[i0].MT);
                        if (Data.Object[i0].TT != null)
                            for (i1 = 0; i1 < Data.Object[i0].TT.Length; i1++)
                            {
                                WriteA3DC(ref Data.Object[i0].TT[i1].C);
                                WriteA3DC(ref Data.Object[i0].TT[i1].O);
                                WriteA3DC(ref Data.Object[i0].TT[i1].R);
                                WriteA3DC(ref Data.Object[i0].TT[i1].Ro);
                                WriteA3DC(ref Data.Object[i0].TT[i1].RF);
                                WriteA3DC(ref Data.Object[i0].TT[i1].TF);
                            }
                    }

                    for (i0 = 0; i0 < Data.ObjectHRC.Length; i0++)
                        if (Data.ObjectHRC[i0].Node != null)
                            for (i1 = 0; i1 < Data.ObjectHRC[i0].Node.Length; i1++)
                                WriteA3DC(ref Data.ObjectHRC[i0].Node[i1].MT);

                    for (i0 = 0; i0 < Data.Point.Length; i0++)
                        WriteA3DC(ref Data.Point[i0]);

                    if (Data.PostProcess.Boolean)
                    {
                        WriteA3DC(ref Data.PostProcess.Ambient  );
                        WriteA3DC(ref Data.PostProcess.Diffuse  );
                        WriteA3DC(ref Data.PostProcess.Specular );
                        WriteA3DC(ref Data.PostProcess.LensFlare);
                        WriteA3DC(ref Data.PostProcess.LensGhost);
                        WriteA3DC(ref Data.PostProcess.LensShaft);
                    }
                }

                if (!ReturnToOffset)
                    IO.Align(0x10, true);
            }

            byte[] A3DCData = IO.ToArray(true);

            IO = new KKtIO(new MemoryStream());
            A3DAWriter(file);
            byte[] A3DAData = IO.ToArray(true);

            Data.Head = new Header();
            IO = KKtIO.OpenWriter(file + ".a3da");
            IO.Position = Offset;

            Data.Head.StringOffset = IO.Position - A3DCStart;
            IO.Write(A3DAData);
            Data.Head.StringLength = IO.Position - A3DCStart - Data.Head.StringOffset;
            IO.Align(0x20, true);

            Data.Head.BinaryOffset = IO.Position - A3DCStart;
            IO.Write(A3DCData);
            Data.Head.BinaryLength = IO.Position - A3DCStart - Data.Head.BinaryOffset;
            IO.Align(0x10, true);

            int A3DCEnd = IO.Position;

            if (Data._.CompressF16 != 0)
            {
                IO.Align(0x10);
                A3DCEnd = IO.Position;
                IO.WriteEOFC(0);
                IO.Position = 0x04;
                IO.Write(A3DCEnd - A3DCStart);
                IO.Position = 0x14;
                IO.Write(A3DCEnd - A3DCStart);
            }

            IO.Position = A3DCStart;
            IO.Write("#A3D", "C__________");
            IO.Write(0x2000);
            IO.Write(0x00);
            IO.WriteEndian(0x20, true);
            IO.Write(0x10000200);
            IO.Write(0x50);
            IO.WriteEndian(Data.Head.StringOffset, true);
            IO.WriteEndian(Data.Head.StringLength, true);
            IO.WriteEndian(0x01, true);
            IO.Write(0x4C42);
            IO.WriteEndian(Data.Head.BinaryOffset, true);
            IO.WriteEndian(Data.Head.BinaryLength, true);
            IO.WriteEndian(0x20, true);
            if (Data.Header.Format != Main.Format.F)
            {
                IO.Position = A3DCEnd;
                IO.WriteEOFC(0);
            }

            IO.Close();
        }

        private void WriteA3DC(ref ModelTransform MT)
        { WriteA3DC(ref MT.Scale, false); WriteA3DC(ref MT.Rot, true);
            WriteA3DC(ref MT.Trans, false); WriteA3DC(ref MT.Visibility, false); }

        private void WriteA3DC(ref RGBAKey RGBA)
        { if (RGBA.Boolean) { WriteA3DC(ref RGBA.R); WriteA3DC(ref RGBA.G);
                WriteA3DC(ref RGBA.B); WriteA3DC(ref RGBA.A); } }

        private void WriteA3DC(ref Vector3Key Key, bool CompressF16)
        { WriteA3DC(ref Key.X, CompressF16); WriteA3DC(ref Key.Y,
            CompressF16); WriteA3DC(ref Key.Z, CompressF16); }
        
        private void WriteA3DC(ref KeyUV UV)
        { WriteA3DC(ref UV.U); WriteA3DC(ref UV.V); }

        private void WriteA3DC(ref Key Key) =>
            WriteA3DC(ref Key, false);

        private void WriteA3DC(ref Key Key, bool CompressF16)
        {
            int i = 0;
            if (Key.Trans != null && Key.Boolean)
            {
                Key.BinOffset = IO.Position;
                IO.Write(Key.Type);
                IO.Write(0x00);
                IO.Write((float)Key.Max);
                IO.Write(Key.Trans.Length);
                for (i = 0; i < Key.Trans.Length; i++)
                {
                    if (CompressF16 && Data._.CompressF16 > 0)
                    { IO.Write((ushort)Key.Trans[i].Frame); IO.Write((Types.Half)Key.Trans[i].Value1); }
                    else
                    { IO.Write((float)Key.Trans[i].Frame); IO.Write((float)Key.Trans[i].Value1); }
                    if (CompressF16 && Data._.CompressF16 == 2)
                    { IO.Write((Types.Half)Key.Trans[i].Value2); IO.Write((Types.Half)Key.Trans[i].Value3); }
                    else
                    { IO.Write((float)Key.Trans[i].Value2); IO.Write((float)Key.Trans[i].Value3); }
                }
            }
            else if (Key.Boolean)
            {
                if (!UsedValues.Value.Contains(Key.Value) || !A3DCOpt)
                {
                    Key.BinOffset = IO.Position;
                    if (Key.Type != 0x00 || Key.Value != 0)
                    { IO.Write(0x01); IO.Write((float)Key.Value); }
                    else
                        IO.Write((long)0x00);
                    if (A3DCOpt)
                    { UsedValues.BinOffset.Add(Key.BinOffset); UsedValues.Value.Add(Key.Value); }
                    return;
                }

                for (i = 0; i < UsedValues.Value.Count; i++)
                    if (UsedValues.Value[i] == Key.Value)
                    {
                        Key.BinOffset = UsedValues.BinOffset[i];
                        return;
                    }
            }
        }

        private void WriteA3DCOffset(ref ModelTransform MT, bool ReturnToOffset)
        {
            if (ReturnToOffset)
            {
                IO.Position = (int)MT.BinOffset;
                WriteA3DCOffset(ref MT.Scale);
                WriteA3DCOffset(ref MT.Rot);
                WriteA3DCOffset(ref MT.Trans);
                IO.Write(MT.Visibility.BinOffset);
            }
            else
            {
                MT.BinOffset = IO.Position;
                for (byte i = 0; i < 10; i++)
                    IO.Write(0x00);
            }
            IO.Align(0x10);
        }

        private void WriteA3DCOffset(ref Vector3Key Key)
        {
            IO.Write(Key.X.BinOffset);
            IO.Write(Key.Y.BinOffset);
            IO.Write(Key.Z.BinOffset);
        }
  
        private int[] SortWriter(int Length)
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
        public int? CompressF16;
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
        public DOF DOF;
        public Fog[] Fog;
        public Curve[] Curve;
        public Event[] Event;
        public Light[] Light;
        public Header Head;
        public Camera Camera;
        public Object[] Object;
        public Ambient[] Ambient;
        public ObjectHRC[] ObjectHRC;
        public MObjectHRC[] MObjectHRC;
        public PlayControl PlayControl;
        public PostProcess PostProcess;
        public Main.Header Header;
        public MaterialList[] MaterialList;
        public ModelTransform[] Chara;
        public ModelTransform[] Point;

        public A3DAData()
        {
            _ = new _();
            DOF = new DOF();
            Fog = new Fog[0];
            Head = new Header();
            Chara = new ModelTransform[0];
            Curve = new Curve[0];
            Event = new Event[0];
            Light = new Light[0];
            Point = new ModelTransform[0];
            Camera = new Camera();
            Header = new Main.Header();
            Motion = new string[0];
            Object = new Object[0];
            Ambient = new Ambient[0];
            ObjectHRC = new ObjectHRC[0];
            ObjectList = new string[0];
            MObjectHRC = new MObjectHRC[0];
            PlayControl = new PlayControl();
            PostProcess = new PostProcess();
            MaterialList = new MaterialList[0];
            ObjectHRCList = new string[0];
            MObjectHRCList = new string[0];
        }
    }

    public class Ambient
    {
        public string  Name;
        public RGBAKey    LightDiffuse;
        public RGBAKey RimLightDiffuse;

        public Ambient()
        { Name = null; LightDiffuse = new RGBAKey(); RimLightDiffuse = new RGBAKey(); }
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

    public class DOF
    {
        public string Name;
        public ModelTransform MT;
        
        public DOF()
        { Name = null; MT = new ModelTransform(); }
    }

    public class Event
    {
        public int? Type;
        public double? End;
        public double? Begin;
        public double? ClipEnd;
        public double? ClipBegin;
        public double? TimeRefScale;
        public string Name;
        public string Param1;
        public string Ref;

        public Event()
        { Type = null; End = null; Begin = null; ClipEnd = null; ClipBegin = null;
            TimeRefScale = null; Name = null; Param1 = null; Ref = null; }
    }

    public class Fog
    {
        public int? Id;
        public Key End;
        public Key Start;
        public Key Density;
        public RGBAKey Diffuse;

        public Fog()
        { Id = null; End = new Key(); Start = new Key();
            Density = new Key(); Diffuse = new RGBAKey(); }
    }

    public class Header
    {
        public int? Count;
        public int? BinaryLength;
        public int? BinaryOffset;
        public int? HeaderOffset;
        public int? StringLength;
        public int? StringOffset;

        public Header()
        { Count = null; BinaryLength = null; BinaryOffset = null;
            HeaderOffset = null; StringLength = null; StringOffset = null; }
    }

    public class Key
    {
        public int? Type;
        public int? Length;
        public int? BinOffset;
        public bool Boolean;
        public double? Max;
        public double? Value;
        public double? EPTypePost;
        public double? EPTypePre;
        public RawD RawData;
        public Transform[] Trans;

        public Key() => NewKey();

        public void NewKey()
        { BinOffset = null; Boolean = false; EPTypePost = null; EPTypePre = null; Length = null;
            Max = null; RawData = new RawD(); Trans = null; Type = 0; Value = null; }

        public class RawD
        {
            public int? KeyType;
            public int? ValueListSize;
            public string ValueType;
            public string ValueListString;
            public string[] ValueList;

            public RawD()
            { KeyType = null; ValueListSize = null; ValueType = "float";
                ValueListString = null; ValueList = null; }
        }

        public struct Transform
        {
            public int    Type;
            public double Frame;
            public double Value1;
            public double Value2;
            public double Value3;
        }
    }

    public class Instance
    {
        public int? Shadow;
        public string Name;
        public string UIDName;
        public ModelTransform MT;

        public Instance()
        { Name = null; UIDName = null; MT = new ModelTransform(); }
    }

    public class KeyUV
    {
        public Key U;
        public Key V;

        public KeyUV()
        { U = new Key(); V = new Key(); }
    }

    public class Light
    {
        public int? Id;
        public string Name;
        public string Type;
        public RGBAKey Ambient;
        public RGBAKey Diffuse;
        public RGBAKey Specular;
        public RGBAKey Incandescence;
        public ModelTransform Position;
        public ModelTransform SpotDirection;

        public Light()
        { Id = null; Name = null; Type = null; Ambient = new RGBAKey();
            Diffuse = new RGBAKey(); Specular = new RGBAKey(); Incandescence = new RGBAKey();
            Position = new ModelTransform(); SpotDirection = new ModelTransform(); }
    }

    public class MaterialList
    {
        public string Name;
        public string HashName;
        public Key GlowIntensity;
        public RGBAKey BlendColor;
        public RGBAKey Incandescence;

        public MaterialList()
        { Name = null; HashName = null; GlowIntensity = new Key();
            BlendColor = new RGBAKey(); Incandescence = new RGBAKey(); }
    }

    public class MObjectHRC
    {
        public string Name;
        public Node[] Node;
        public Instance[] Instance;
        public ModelTransform MT;

        public MObjectHRC()
        { Name = null; Node = null; Instance = null; MT = new ModelTransform(); }
    }

    public class ModelTransform
    {
        public bool Writed;
        public int? BinOffset;
        public Key Visibility;
        public Vector3Key Rot;
        public Vector3Key Scale;
        public Vector3Key Trans;

        public ModelTransform() => NewModelTransform();

        public void NewModelTransform()
        { Rot = new Vector3Key(); Scale = new Vector3Key(); Trans = new Vector3Key();
            Visibility = new Key(); BinOffset = null; Writed = false; }
    }

    public class Node
    {
        public int? Parent;
        public string Name;
        public ModelTransform MT;

        public Node()
        { Parent = null; MT = new ModelTransform(); }
    }

    public class Object
    {
        public int? MorphOffset;
        public string Name;
        public string Morph;
        public string UIDName;
        public string ParentName;
        public ModelTransform MT;
        public TexturePattern[] TP; //TexPat
        public TextureTransform[] TT; //TexTrans

        public Object()
        { MorphOffset = null; Morph = null; Name = null; ParentName = null;
            UIDName = null; MT = new ModelTransform(); TP = null; TT = null; }
    }

    public class ObjectHRC
    {
        public double? Shadow;
        public string Name;
        public string UIDName;
        public Node[] Node;

        public ObjectHRC()
        { Shadow = null; Name = null; UIDName = null; Node = null; }
    }

    public class PlayControl
    {
        public double? Begin;
        public double? Div;
        public double? FPS;
        public double? Offset;
        public double? Size;

        public PlayControl()
        { Begin = null; Div = null; FPS = null; Offset = null; Size = null; }
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
        { Boolean = false; LensFlare = new Key(); LensGhost = new Key();
            LensShaft = new Key(); Ambient = new RGBAKey();
            Diffuse = new RGBAKey(); Specular = new RGBAKey(); }
    }

    public class RGBAKey
    {
        public bool Boolean;
        public Key R;
        public Key G;
        public Key B;
        public Key A;

        public RGBAKey()
        { Boolean = false; R = new Key(); G = new Key(); B = new Key(); A = new Key(); }
    }

    public class TexturePattern
    {
        public int? PatOffset;
        public string Pat;
        public string Name;

        public TexturePattern()
        { PatOffset = null; Pat = null; Name = null; }
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
        public List<int   ?> BinOffset;
        public List<double?> Value;

        public Values()
        { BinOffset = new List<int?>(); Value = new List<double?>(); }
    }

    public class Vector3Key
    {
        public Key X;
        public Key Y;
        public Key Z;

        public Vector3Key()
        { X = new Key(); Y = new Key(); Z = new Key(); }
    }

    public class ViewPoint
    {
        public double? Aspect;
        public double? FOVHorizontal;
        public double? CameraApertureH;
        public double? CameraApertureW;
        public Key FOV;
        public Key Roll;
        public Key FocalLength;
        public ModelTransform MT;

        public ViewPoint()
        { Aspect = null; CameraApertureH = null; CameraApertureW = null; FocalLength = new Key();
            FOV = new Key(); FOVHorizontal = null; MT = new ModelTransform(); Roll = new Key(); }
    }
}
