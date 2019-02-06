using System;
using System.IO;
using System.Collections.Generic;
using KKtIO = KKtLib.IO;
using MP = KKtLib.MsgPack.MsgPack;
using MPIO = KKtLib.MsgPack.IO;
using MPTypes = KKtLib.MsgPack.MsgPack.Types;

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
        private MP MsgPack;
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
                Data.PlayControl.Div = Main.ToDouble(value);
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
                Key.RawData.Boolean = true;
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
            if (!Key.RawData.Boolean)
            {
                for (i = 0; i < Key.Trans.Length; i++)
                {
                    SOi = SO[i];
                    IO.Write(Temp + "key." + SOi + ".data=");
                    if (Key.Trans[SOi].Type > 0) IO.Write("(");
                    IO.Write(Main.ToString(Key.Trans[SOi].Frame, 8));
                    if (Key.Trans[SOi].Type > 0) { IO.Write("," + Main.ToString(Key.Trans[SOi].Value1));
                        if (Key.Trans[SOi].Type > 1) { IO.Write("," + Main.ToString(Key.Trans[SOi].Value2));
                            if (Key.Trans[SOi].Type > 2) IO.Write("," + Main.ToString(Key.Trans[SOi].Value3)); }
                        IO.Write(")"); }
                    IO.Write("\n");
                    IO.Write(Temp + "key." + SOi + ".type=", Key.Trans[SOi].Type);
                }
                IO.Write(Temp + "key.length=", Key.Length);
            }
            if (Key.Max != null)
                IO.Write(Temp + "max=", Key.Max);
            if (Key.RawData.Boolean)
            {
                for (i = 0; i < Key.Trans.Length; i++)
                    if (Key.RawData.KeyType < Key.Trans[i].Type || Key.RawData.KeyType == null)
                        Key.RawData.KeyType = Key.Trans[i].Type;
                    else if (Key.RawData.KeyType == 3) break;
                Key.RawData.ValueListSize = Key.Trans.Length * (Key.RawData.KeyType + 1);
                IO.Write(Temp + "raw_data.value_list=");
                for (i = 0; i < Key.Trans.Length; i++)
                {
                    IO.Write(Main.ToString(Key.Trans[i].Frame, 8));
                    if (Key.RawData.KeyType > 0) { IO.Write("," + Main.ToString(Key.Trans[i].Value1));
                        if (Key.RawData.KeyType > 1) { IO.Write("," + Main.ToString(Key.Trans[i].Value2));
                            if (Key.RawData.KeyType > 2) IO.Write("," + Main.ToString(Key.Trans[i].Value3)); } }
                    if (i < Key.Trans.Length - 1) IO.Write(',');
                }
                IO.Write('\n');
                IO.Write(Temp + "raw_data.value_list_size=", Key.RawData.ValueListSize);
                IO.Write(Temp + "raw_data.value_type=", Key.RawData.ValueType);
                IO.Write(Temp + "raw_data_key_type=", Key.RawData.KeyType);
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
                int a = (int)((long)int.Parse(Data._.ConverterVersion) * BitConverter.
                    ToInt32(Text.ToUTF8(Data._.FileName), 0) * int.Parse(Data._.PropertyVersion));

                IO.Write(Text.ToASCII("A3DA"));
                IO.Write(0x00);
                IO.Write(0x40);
                IO.Write(0x10000000);
                IO.Write((long)0x00);
                IO.Write((long)0x00);
                IO.Write(a);
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

        public void MsgPackReader(string file)
        {
            int n = 0;
            int i19 = 0;
            int i18 = 0;
            MPIO IO = new MPIO(KKtIO.OpenReader(file + ".mp"));
            MsgPack = (MP)IO.Read();
            IO.Close();
            IO = null;
            if (MsgPack.Element("A3D", out MP A3D))
            {
                Enum.TryParse(A3D.ReadString("Format"), out Data.Header.Format);
                MP Temp = new MP();

                if (A3D.Element("_", out Temp))
                {
                    Data._.CompressF16 = Temp.ReadNInt32("CompressF16");
                    Data._.ConverterVersion = Temp.ReadString("ConverterVersion");
                    Data._.FileName = Temp.ReadString("FileName");
                    Data._.PropertyVersion = Temp.ReadString("PropertyVersion");
                }

                if (A3D.Element("Ambient", out Temp, typeof(object[])))
                {
                    Data.Ambient = new Ambient[((object[])Temp.Object).Length];
                    MP Ambient2 = new MP();

                    for (n = 0; n < Data.Ambient.Length; n++)
                        if (Temp[n].GetType() == typeof(MP))
                        {
                            Data.Ambient[n] = new Ambient();
                            Ambient2 = (MP)Temp[n];
                            Data.Ambient[n].Name = Ambient2.ReadString("Name");
                            ReadMP(ref Data.Ambient[n].LightDiffuse, Ambient2, "   LightDiffuse");
                            ReadMP(ref Data.Ambient[n].RimLightDiffuse, Ambient2, "RimLightDiffuse");
                        }
                }

                if (A3D.Element("Camera", out Temp))
                {
                    if (Temp.Element("Auxiliary", out MP Auxiliary))
                    {
                        Data.Camera.Auxiliary.Boolean = true;
                        ReadMP(ref Data.Camera.Auxiliary.AutoExposure, Auxiliary, "AutoExposure");
                        ReadMP(ref Data.Camera.Auxiliary.Exposure, Auxiliary, "Exposure");
                        ReadMP(ref Data.Camera.Auxiliary.Gamma, Auxiliary, "Gamma");
                        ReadMP(ref Data.Camera.Auxiliary.GammaRate, Auxiliary, "GammaRate");
                        ReadMP(ref Data.Camera.Auxiliary.Saturate, Auxiliary, "Saturate");
                    }

                    if (Temp.Element("Root", out MP Root, typeof(object[])))
                    {
                        Data.Camera.Root = new CameraRoot[((object[])Root.Object).Length];
                        MP _Root2 = new MP();
                        for (n = 0; n < Data.Camera.Root.Length; n++)
                            if (Root[n].GetType() == typeof(MP))
                            {
                                Data.Camera.Root[n] = new CameraRoot();
                                _Root2 = (MP)Root[n];
                                ReadMP(ref Data.Camera.Root[n].MT, _Root2);
                                ReadMP(ref Data.Camera.Root[n].Interest, _Root2, "Interest");
                                if (_Root2.Element("ViewPoint", out MP ViewPoint))
                                {
                                    Data.Camera.Root[n].ViewPoint.Aspect = ViewPoint.ReadNDouble("Aspect");
                                    Data.Camera.Root[n].ViewPoint.CameraApertureH = ViewPoint.ReadNDouble("CameraApertureH");
                                    Data.Camera.Root[n].ViewPoint.CameraApertureW = ViewPoint.ReadNDouble("CameraApertureW");
                                    Data.Camera.Root[n].ViewPoint.FOVHorizontal = ViewPoint.ReadNDouble("FOVHorizontal");
                                    ReadMP(ref Data.Camera.Root[n].ViewPoint.FocalLength, ViewPoint, "FocalLength");
                                    ReadMP(ref Data.Camera.Root[n].ViewPoint.FOV, ViewPoint, "FOV");
                                    ReadMP(ref Data.Camera.Root[n].ViewPoint.Roll, ViewPoint, "Roll");
                                    ReadMP(ref Data.Camera.Root[n].ViewPoint.MT, ViewPoint);
                                }
                            }
                    }
                }

                if (A3D.Element("Chara", out Temp, typeof(object[])))
                {
                    Data.Chara = new ModelTransform[((object[])Temp.Object).Length];
                    for (n = 0; n < Data.Chara.Length; n++)
                        if (Temp[n].GetType() == typeof(MP))
                        {
                            Data.Chara[n] = new ModelTransform();
                            ReadMP(ref Data.Chara[n], (MP)Temp[n]);
                        }
                }

                if (A3D.Element("Curve", out Temp, typeof(object[])))
                {
                    Data.Curve = new Curve[((object[])Temp.Object).Length];
                    MP Curve2 = new MP();
                    for (n = 0; n < Data.Curve.Length; n++)
                        if (Temp[n].GetType() == typeof(MP))
                        {
                            Data.Curve[n] = new Curve();
                            Curve2 = (MP)Temp[n];
                            Data.Curve[n].Name = Curve2.ReadString("Name");
                            ReadMP(ref Data.Curve[n].CV, Curve2, "CV");
                        }
                }

                if (A3D.Element("DOF", out Temp, typeof(object[])))
                {
                    Data.DOF.Name = Temp.ReadString("Name");
                    ReadMP(ref Data.DOF.MT, Temp);
                }

                if (A3D.Element("Event", out Temp, typeof(object[])))
                {
                    Data.Event = new Event[((object[])Temp.Object).Length];
                    MP Event2 = new MP();
                    for (n = 0; n < Data.Event.Length; n++)
                        if (Temp[n].GetType() == typeof(MP))
                        {
                            Data.Event[n] = new Event();
                            Event2 = (MP)Temp[n];
                            Data.Event[n].Begin = Event2.ReadNDouble("Begin");
                            Data.Event[n].ClipBegin = Event2.ReadNDouble("ClipBegin");
                            Data.Event[n].ClipEnd = Event2.ReadNDouble("ClipEnd");
                            Data.Event[n].End = Event2.ReadNDouble("End");
                            Data.Event[n].Name = Event2.ReadString("Name");
                            Data.Event[n].Param1 = Event2.ReadString("Param1");
                            Data.Event[n].Ref = Event2.ReadString("Ref");
                            Data.Event[n].TimeRefScale = Event2.ReadNDouble("TimeRefScale");
                            Data.Event[n].Type = Event2.ReadNInt32("Type");
                        }
                }

                if (A3D.Element("Fog", out Temp, typeof(object[])))
                {
                    Data.Fog = new Fog[((object[])Temp.Object).Length];
                    MP Fog2 = new MP();
                    for (n = 0; n < Data.Fog.Length; n++)
                        if (Temp[n].GetType() == typeof(MP))
                        {
                            Data.Fog[n] = new Fog();
                            Fog2 = (MP)Temp[n];
                            Data.Fog[n].Id = Fog2.ReadNInt32("Id");
                            ReadMP(ref Data.Fog[n].Density, Fog2, "Density");
                            ReadMP(ref Data.Fog[n].Diffuse, Fog2, "Diffuse");
                            ReadMP(ref Data.Fog[n].End, Fog2, "End");
                            ReadMP(ref Data.Fog[n].Start, Fog2, "Start");
                        }
                }

                if (A3D.Element("Light", out Temp, typeof(object[])))
                {
                    Data.Light = new Light[((object[])Temp.Object).Length];
                    MP Light2 = new MP();
                    for (n = 0; n < Data.Light.Length; n++)
                        if (Temp[n].GetType() == typeof(MP))
                        {
                            Data.Light[n] = new Light();
                            Light2 = (MP)Temp[n];
                            Data.Light[n].Id = Light2.ReadNInt32("Id");
                            Data.Light[n].Name = Light2.ReadString("Name");
                            Data.Light[n].Type = Light2.ReadString("Type");
                            ReadMP(ref Data.Light[n].Ambient, Light2, "Ambient");
                            ReadMP(ref Data.Light[n].Diffuse, Light2, "Diffuse");
                            ReadMP(ref Data.Light[n].Incandescence, Light2, "Incandescence");
                            ReadMP(ref Data.Light[n].Position, Light2, "Position");
                            ReadMP(ref Data.Light[n].Specular, Light2, "Specular");
                            ReadMP(ref Data.Light[n].SpotDirection, Light2, "SpotDirection");
                        }
                }

                if (A3D.Element("MaterialList", out Temp, typeof(object[])))
                {
                    Data.MaterialList = new MaterialList[((object[])Temp.Object).Length];
                    MP MaterialList2 = new MP();
                    for (n = 0; n < Data.MaterialList.Length; n++)
                        if (Temp[n].GetType() == typeof(MP))
                        {
                            Data.MaterialList[n] = new MaterialList();
                            MaterialList2 = (MP)Temp[n];
                            Data.MaterialList[n].HashName = MaterialList2.ReadString("HashName");
                            Data.MaterialList[n].Name = MaterialList2.ReadString("Name");
                            ReadMP(ref Data.MaterialList[n].BlendColor, MaterialList2, "BlendColor");
                            ReadMP(ref Data.MaterialList[n].GlowIntensity, MaterialList2, "GlowIntensity");
                            ReadMP(ref Data.MaterialList[n].Incandescence, MaterialList2, "Incandescence");
                        }
                }

                if (A3D.Element("MObjectHRC", out Temp, typeof(object[])))
                {
                    Data.MObjectHRC = new MObjectHRC[((object[])Temp.Object).Length];
                    MP _Node4 = new MP();
                    MP _Instance3 = new MP();
                    MP MObjectHRC2 = new MP();
                    for (i19 = 0; i19 < Data.MObjectHRC.Length; i19++)
                    {
                        if (Temp[i19].GetType() == typeof(MP))
                        {
                            Data.MObjectHRC[i19] = new MObjectHRC();
                            MObjectHRC2 = (MP)Temp[i19];
                            Data.MObjectHRC[i19].Name = MObjectHRC2.ReadString("Name");
                            ReadMP(ref Data.MObjectHRC[i19].MT, MObjectHRC2);

                            if (MObjectHRC2.Element("Instance", out MP Instance, typeof(object[])))
                            {
                                Data.MObjectHRC[i19].Instance = new Instance[((object[])Instance.Object).Length];
                                for (i18 = 0; i18 < Data.MObjectHRC[i19].Instance.Length; i18++)
                                {
                                    if (Instance[i18].GetType() == typeof(MP))
                                    {
                                        Data.MObjectHRC[i19].Instance[i18] = new Instance();
                                        _Instance3 = (MP)Instance[i18];
                                        Data.MObjectHRC[i19].Instance[i18].Name = _Instance3.ReadString("Name");
                                        Data.MObjectHRC[i19].Instance[i18].Shadow = _Instance3.ReadNInt32("Shadow");
                                        Data.MObjectHRC[i19].Instance[i18].UIDName = _Instance3.ReadString("UIDName");
                                        ReadMP(ref Data.MObjectHRC[i19].Instance[i18].MT, _Instance3);
                                    }
                                }
                            }

                            if (MObjectHRC2.Element("Node", out MP Node2, typeof(object[])))
                            {
                                Data.MObjectHRC[i19].Node = new Node[((object[])Node2.Object).Length];
                                for (i18 = 0; i18 < Data.MObjectHRC[i19].Node.Length; i18++)
                                {
                                    if (Node2[i18].GetType() == typeof(MP))
                                    {
                                        Data.MObjectHRC[i19].Node[i18] = new Node();
                                        _Node4 = (MP)Node2[i18];
                                        Data.MObjectHRC[i19].Node[i18].Name = _Node4.ReadString("Name");
                                        Data.MObjectHRC[i19].Node[i18].Parent = _Node4.ReadNInt32("Parent");
                                        ReadMP(ref Data.MObjectHRC[i19].Node[i18].MT, _Node4);
                                    }
                                }
                            }
                        }
                    }
                }

                if (A3D.Element("MObjectHRCList", out Temp, typeof(object[])))
                {
                    Data.MObjectHRCList = new string[((object[])Temp.Object).Length];
                    for (n = 0; n < Data.MObjectHRCList.Length; n++)
                        if (Temp[n].GetType() == typeof(MP))
                            Data.MObjectHRCList[n] = ((MP)Temp[n]).ReadString();
                }

                if (A3D.Element("Motion", out Temp, typeof(object[])))
                {
                    Data.Motion = new string[((object[])Temp.Object).Length];
                    for (n = 0; n < Data.Motion.Length; n++)
                        if (Temp[n].GetType() == typeof(MP))
                            Data.Motion[n] = ((MP)Temp[n]).ReadString();
                }

                if (A3D.Element("Object", out Temp, typeof(object[])))
                {
                    Data.Object = new KKtLib.A3DA.Object[((object[])Temp.Object).Length];
                    MP _TP2 = new MP();
                    MP _TT2 = new MP();
                    MP Object2 = new MP();
                    for (i19 = 0; i19 < Data.Object.Length; i19++)
                        if (Temp[i19].GetType() == typeof(MP))
                        {
                            Data.Object[i19] = new Object();
                            Object2 = (MP)Temp[i19];
                            Data.Object[i19].Morph = Object2.ReadString("Morph");
                            Data.Object[i19].MorphOffset = Object2.ReadNInt32("MorphOffset");
                            Data.Object[i19].Name = Object2.ReadString("Name");
                            Data.Object[i19].ParentName = Object2.ReadString("ParentName");
                            Data.Object[i19].UIDName = Object2.ReadString("UIDName");
                            ReadMP(ref Data.Object[i19].MT, Object2);

                            if (Object2.Element("TP", out MP TP, typeof(object[])))
                            {
                                Data.Object[i19].TP = new TexturePattern[((object[])TP.Object).Length];
                                for (i18 = 0; i18 < Data.Object[i19].TP.Length; i18++)
                                {
                                    if (TP[i18].GetType() == typeof(MP))
                                    {
                                        Data.Object[i19].TP[i18] = new TexturePattern();
                                        _TP2 = (MP)TP[i18];
                                        Data.Object[i19].TP[i18].Name = _TP2.ReadString("Name");
                                        Data.Object[i19].TP[i18].Pat = _TP2.ReadString("Pat");
                                        Data.Object[i19].TP[i18].PatOffset = _TP2.ReadNInt32("PatOffset");
                                    }
                                }
                            }

                            if (Object2.Element("TT", out MP TT, typeof(object[])))
                            {
                                Data.Object[i19].TT = new TextureTransform[((object[])TT.Object).Length];
                                for (i18 = 0; i18 < Data.Object[i19].TT.Length; i18++)
                                {
                                    if (TT[i18].GetType() == typeof(MP))
                                    {
                                        Data.Object[i19].TT[i18] = new TextureTransform();
                                        _TT2 = (MP)TT[i18];
                                        Data.Object[i19].TT[i18].Name = _TT2.ReadString("Name");
                                        ReadMP(ref Data.Object[i19].TT[i18].C, _TT2, "C");
                                        ReadMP(ref Data.Object[i19].TT[i18].O, _TT2, "O");
                                        ReadMP(ref Data.Object[i19].TT[i18].R, _TT2, "R");
                                        ReadMP(ref Data.Object[i19].TT[i18].RF, _TT2, "RF");
                                        ReadMP(ref Data.Object[i19].TT[i18].Ro, _TT2, "Ro");
                                        ReadMP(ref Data.Object[i19].TT[i18].TF, _TT2, "TF");
                                    }
                                }
                            }
                        }
                }

                if (A3D.Element("ObjectHRC", out Temp, typeof(object[])))
                {
                    Data.ObjectHRC = new ObjectHRC[((object[])Temp.Object).Length];
                    MP _Node2 = new MP();
                    MP _Instance = new MP();
                    MP ObjectHRC2 = new MP();
                    for (i19 = 0; i19 < Data.ObjectHRC.Length; i19++)
                        if (Temp[i19].GetType() == typeof(MP))
                        {
                            Data.ObjectHRC[i19] = new ObjectHRC();
                            ObjectHRC2 = (MP)Temp[i19];
                            Data.ObjectHRC[i19].Name = ObjectHRC2.ReadString("Name");
                            Data.ObjectHRC[i19].Shadow = ObjectHRC2.ReadNDouble("Shadow");
                            Data.ObjectHRC[i19].UIDName = ObjectHRC2.ReadString("UIDName");
                            if (ObjectHRC2.Element("Node", out MP Node, typeof(object[])))
                            {
                                Data.ObjectHRC[i19].Node = new Node[((object[])Node.Object).Length];
                                for (i18 = 0; i18 < Data.ObjectHRC[i19].Node.Length; i18++)
                                {
                                    if (Node[i18].GetType() == typeof(MP))
                                    {
                                        Data.ObjectHRC[i19].Node[i18] = new Node();
                                        _Node2 = (MP)Node[i18];
                                        Data.ObjectHRC[i19].Node[i18].Name = _Node2.ReadString("Name");
                                        Data.ObjectHRC[i19].Node[i18].Parent = _Node2.ReadInt32("Parent");
                                        ReadMP(ref Data.ObjectHRC[i19].Node[i18].MT, _Node2);
                                    }
                                }
                            }
                        }
                }

                if (A3D.Element("ObjectHRCList", out Temp, typeof(object[])))
                {
                    Data.ObjectHRCList = new string[((object[])Temp.Object).Length];
                    for (n = 0; n < Data.ObjectHRCList.Length; n++)
                        if (Temp[n].GetType() == typeof(MP))
                            Data.ObjectHRCList[n] = ((MP)Temp[n]).ReadString();
                }

                if (A3D.Element("ObjectList", out Temp, typeof(object[])))
                {
                    Data.ObjectList = new string[((object[])Temp.Object).Length];
                    for (n = 0; n < Data.ObjectList.Length; n++)
                        if (Temp[n].GetType() == typeof(MP))
                            Data.ObjectList[n] = ((MP)Temp[n]).ReadString();
                }

                if (A3D.Element("PlayControl", out Temp))
                {
                    Data.PlayControl.Begin = Temp.ReadNDouble("Begin");
                    Data.PlayControl.Div = Temp.ReadNDouble("Div");
                    Data.PlayControl.FPS = Temp.ReadNDouble("FPS");
                    Data.PlayControl.Offset = Temp.ReadNDouble("Offset");
                    Data.PlayControl.Size = Temp.ReadNDouble("Size");
                }

                if (A3D.Element("Point", out Temp, typeof(object[])))
                {
                    Data.Point = new ModelTransform[((object[])Temp.Object).Length];
                    for (n = 0; n < Data.Point.Length; n++)
                        if (Temp[n].GetType() == typeof(MP))
                            ReadMP(ref Data.Point[n], (MP)Temp[n]);
                }

                if (A3D.Element("PostProcess", out Temp))
                {
                    Data.PostProcess.Boolean = true;
                    ReadMP(ref Data.PostProcess.Ambient, Temp, "Ambient");
                    ReadMP(ref Data.PostProcess.Diffuse, Temp, "Diffuse");
                    ReadMP(ref Data.PostProcess.LensFlare, Temp, "LensFlare");
                    ReadMP(ref Data.PostProcess.LensGhost, Temp, "LensGhost");
                    ReadMP(ref Data.PostProcess.LensShaft, Temp, "LensShaft");
                    ReadMP(ref Data.PostProcess.Specular, Temp, "Specular");
                }
                Temp = null;
            }
            MsgPack = null;
        }

        private void ReadMP(ref ModelTransform MT, MP k, string name)
        { if (k.Element(name, out MP Name))
                ReadMP(ref MT, Name); }

        private void ReadMP(ref ModelTransform MT, MP k)
        {
            if (k != null)
            {
                MT = new ModelTransform();
                ReadMP(ref MT.Rot, k, "Rot");
                ReadMP(ref MT.Scale, k, "Scale");
                ReadMP(ref MT.Trans, k, "Trans");
                ReadMP(ref MT.Visibility, k, "Visibility");
            }
        }

        private void ReadMP(ref RGBAKey RGBA, MP k, string name)
        { if (k.Element(name, out MP Name))
                ReadMP(ref RGBA, Name); }

        private void ReadMP(ref RGBAKey RGBA, MP k)
        {
            if (k != null)
            {
                RGBA = new RGBAKey
                {
                    Boolean = true
                };
                ReadMP(ref RGBA.R, k, "R");
                ReadMP(ref RGBA.G, k, "G");
                ReadMP(ref RGBA.B, k, "B");
                ReadMP(ref RGBA.A, k, "A");
            }
        }

        private void ReadMP(ref Vector3Key Key, MP k, string name)
        { if (k.Element(name, out MP Name))
                ReadMP(ref Key, Name); }

        private void ReadMP(ref Vector3Key Key, MP k)
        {
            Key = new Vector3Key();
            ReadMP(ref Key.X, k, "X");
            ReadMP(ref Key.Y, k, "Y");
            ReadMP(ref Key.Z, k, "Z");
        }

        private void ReadMP(ref KeyUV UV, MP k, string name)
        { if (k.Element(name, out MP Name))
                ReadMP(ref UV, Name); }

        private void ReadMP(ref KeyUV UV, MP k)
        {
            UV = new KeyUV();
            ReadMP(ref UV.U, k, "U");
            ReadMP(ref UV.V, k, "V");
        }

        private void ReadMP(ref Key Key, MP k, string name)
        { if (k.Element(name, out MP Name))
                ReadMP(ref Key, Name); }

        private void ReadMP(ref Key Key, MP k)
        {
            if (k != null)
            {
                Key = new Key
                {
                    Boolean = true,
                    EPTypePost = k.ReadNDouble("Post"),
                    EPTypePre = k.ReadNDouble("Pre"),
                    Max = k.ReadNDouble("M"),
                    Type = k.ReadNInt32("T"),
                    Value = k.ReadNDouble("V")
                };
                Key.RawData.Boolean = k.ReadBoolean("RD");
                int? type = Key.Type;
                int num = 0;
                int num2;
                if (!(type.GetValueOrDefault() == num & type.HasValue))
                {
                    type = Key.Type;
                    num = 1;
                    num2 = ((!(type.GetValueOrDefault() == num & type.HasValue)) ? 1 : 0);
                }
                else
                    num2 = 0;
                if (num2 != 0)
                {
                    if (k.Element("Trans", out MP Trans, typeof(object[])))
                    {
                        Key.Length = ((object[])Trans.Object).Length;
                        Key.Trans = new Key.Transform[Key.Length.Value];
                        MP _Trans2 = new MP();
                        int i = 0;
                        while (true)
                        {
                            int num3 = i;
                            type = Key.Length;
                            if (num3 < type.GetValueOrDefault() & type.HasValue)
                            {
                                Key.Trans[i] = default(Key.Transform);
                                if (Trans[i].GetType() == typeof(MP))
                                {
                                    _Trans2 = (MP)Trans[i];
                                    if (_Trans2.Object.GetType() == typeof(object[]))
                                    {
                                        Key.Trans[i].Type = ((object[])_Trans2.Object).Length - 1;
                                        if (_Trans2[0].GetType() == typeof(MP))
                                        {
                                            Key.Trans[i].Frame = ((MP)_Trans2[0]).ReadDouble();
                                            if (Key.Trans[i].Type >= 1 && _Trans2[1].GetType() == typeof(MP))
                                            { Key.Trans[i].Value1 = ((MP)_Trans2[1]).ReadDouble();
                                                if (Key.Trans[i].Type >= 2 && _Trans2[2].GetType() == typeof(MP))
                                                { Key.Trans[i].Value2 = ((MP)_Trans2[2]).ReadDouble();
                                                    if (Key.Trans[i].Type >= 3 && _Trans2[3].GetType() == typeof(MP))
                                                        Key.Trans[i].Value3 = ((MP)_Trans2[3]).ReadDouble();
                                                }
                                            }
                                        }
                                    }
                                }
                                i++;
                                continue;
                            }
                            break;
                        }
                    }
                }
                else
                {
                    type = Key.Type;
                    num = 0;
                    if (type.GetValueOrDefault() == num & type.HasValue)
                    {
                        Key.Value = 0.0;
                    }
                }
            }
        }

        public void MsgPackWriter(string file)
        {
            int l = 0;
            int i18 = 0;
            int i17 = 0;

            MsgPack = new MP(MPTypes.FixMap);
            MP A3D = new MP("A3D");
            A3D.Add("Format", Data.Header.Format.ToString());

            MP _ = new MP("_");
            _.Add("CompressF16", Data._.CompressF16);
            _.Add("ConverterVersion", Data._.ConverterVersion);
            _.Add("FileName", Data._.FileName);
            _.Add("PropertyVersion", Data._.PropertyVersion);
            A3D.Add(_);

            if (Data.Ambient.Length != 0)
            {
                MP Ambient = new MP("Ambient", Data.Ambient.Length);
                for (l = 0; l < Data.Ambient.Length; l++)
                {
                    MP _Ambient = new MP();
                    _Ambient.Add(WriteMP(ref Data.Ambient[l].LightDiffuse, "LightDiffuse"));
                    _Ambient.Add("Name", Data.Ambient[l].Name);
                    _Ambient.Add(WriteMP(ref Data.Ambient[l].RimLightDiffuse, "RimLightDiffuse"));
                    Ambient[l] = _Ambient;
                }
                A3D.Add(Ambient);
            }

            if (Data.Camera.Auxiliary.Boolean || Data.Camera.Root.Length != 0)
            {
                MP Camera = new MP("Camera");
                if (Data.Camera.Auxiliary.Boolean)
                {
                    MP Auxiliary = new MP("Auxiliary");
                    Auxiliary.Add(WriteMP(ref Data.Camera.Auxiliary.AutoExposure, "AutoExposure"));
                    Auxiliary.Add(WriteMP(ref Data.Camera.Auxiliary.Exposure, "Exposure"));
                    Auxiliary.Add(WriteMP(ref Data.Camera.Auxiliary.Gamma, "Gamma"));
                    Auxiliary.Add(WriteMP(ref Data.Camera.Auxiliary.GammaRate, "GammaRate"));
                    Auxiliary.Add(WriteMP(ref Data.Camera.Auxiliary.Saturate, "Saturate"));
                    Camera.Add(Auxiliary);
                }

                if (Data.Camera.Root.Length != 0)
                {
                    MP Root = new MP("Root", Data.Camera.Root.Length);
                    for (l = 0; l < Data.Camera.Root.Length; l++)
                    {
                        MP _Root = new MP();
                        WriteMP(ref Data.Camera.Root[l].MT, ref _Root);
                        _Root.Add(WriteMP(ref Data.Camera.Root[l].Interest, "Interest"));
                        MP VP = new MP("ViewPoint");
                        if (Data.Camera.Root[l].ViewPoint.Aspect.HasValue)
                            VP.Add("Aspect", Data.Camera.Root[l].ViewPoint.Aspect);
                        if (Data.Camera.Root[l].ViewPoint.CameraApertureH.HasValue)
                            VP.Add("CameraApertureH", Data.Camera.Root[l].ViewPoint.CameraApertureH);
                        if (Data.Camera.Root[l].ViewPoint.CameraApertureW.HasValue)
                            VP.Add("CameraApertureW", Data.Camera.Root[l].ViewPoint.CameraApertureW);
                        VP.Add(WriteMP(ref Data.Camera.Root[l].ViewPoint.FocalLength, "FocalLength"));
                        VP.Add(WriteMP(ref Data.Camera.Root[l].ViewPoint.FOV, "FOV"));
                            VP.Add("FOVHorizontal", Data.Camera.Root[l].ViewPoint.FOVHorizontal);
                        VP.Add(WriteMP(ref Data.Camera.Root[l].ViewPoint.Roll, "Roll"));
                        WriteMP(ref Data.Camera.Root[l].ViewPoint.MT, ref VP);
                        _Root.Add(VP);
                        Root[l] = _Root;
                    }
                    Camera.Add(Root);
                }
                A3D.Add(Camera);
            }

            if (Data.Chara.Length != 0)
            {
                MP Chara = new MP("Chara", Data.Chara.Length);
                for (l = 0; l < Data.Curve.Length; l++)
                {
                    MP _Chara = new MP();
                    WriteMP(ref Data.Chara[l], ref _Chara);
                    Chara[l] = _Chara;
                }
                A3D.Add(Chara);
            }

            if (Data.Curve.Length != 0)
            {
                MP Curve = new MP("Curve", Data.Curve.Length);
                for (l = 0; l < Data.Curve.Length; l++)
                {
                    MP _Curve = new MP();
                    _Curve.Add("Name", Data.Curve[l].Name);
                    _Curve.Add(WriteMP(ref Data.Curve[l].CV, "CV"));
                    Curve[l] = _Curve;
                }
                A3D.Add(Curve);
            }

            if (Data.DOF.Name != null)
            {
                MP DOF = new MP("DOF");
                DOF.Add("Name", Data.DOF.Name);
                DOF.Add(WriteMP(ref Data.DOF.MT, "MT"));
                A3D.Add(DOF);
            }

            if (Data.Event.Length != 0)
            {
                MP Event = new MP("Events", Data.Event.Length);
                for (l = 0; l < Data.Event.Length; l++)
                {
                    MP _Event = new MP();
                    _Event.Add("Begin", Data.Event[l].Begin);
                    _Event.Add("ClipBegin", Data.Event[l].ClipBegin);
                    _Event.Add("ClipEnd", Data.Event[l].ClipEnd);
                    _Event.Add("End", Data.Event[l].End);
                    _Event.Add("Name", Data.Event[l].Name);
                    _Event.Add("Param1", Data.Event[l].Param1);
                    _Event.Add("Ref", Data.Event[l].Ref);
                    _Event.Add("TimeRefScale", Data.Event[l].TimeRefScale);
                    _Event.Add("Type", Data.Event[l].Type);
                    Event[l] = _Event;
                }
                A3D.Add(Event);
            }

            if (Data.Fog.Length != 0)
            {
                MP Fog = new MP("Fog", Data.Fog.Length);
                for (l = 0; l < Data.Fog.Length; l++)
                {
                    MP _Fog = new MP();
                    _Fog.Add(WriteMP(ref Data.Fog[l].Density, "Density"));
                    _Fog.Add(WriteMP(ref Data.Fog[l].Diffuse, "Diffuse"));
                    _Fog.Add(WriteMP(ref Data.Fog[l].End, "End"));
                    _Fog.Add("Id", Data.Fog[l].Id);
                    _Fog.Add(WriteMP(ref Data.Fog[l].Start, "Start"));
                    Fog[l] = _Fog;
                }
                A3D.Add(Fog);
            }

            if (Data.Light.Length != 0)
            {
                MP Light = new MP("Light", Data.Light.Length);
                for (l = 0; l < Data.Light.Length; l++)
                {
                    MP _Light = new MP();
                    _Light.Add(WriteMP(ref Data.Light[l].Ambient, "Ambient"));
                    _Light.Add(WriteMP(ref Data.Light[l].Diffuse, "Diffuse"));
                    _Light.Add("Id", Data.Light[l].Id);
                    _Light.Add(WriteMP(ref Data.Light[l].Incandescence, "Incandescence"));
                    _Light.Add("Name", Data.Light[l].Name);
                    _Light.Add(WriteMP(ref Data.Light[l].Position, "Position"));
                    _Light.Add(WriteMP(ref Data.Light[l].Specular, "Specular"));
                    _Light.Add(WriteMP(ref Data.Light[l].SpotDirection, "SpotDirection"));
                    _Light.Add("Type", Data.Light[l].Type);
                    Light[l] = _Light;
                }
                A3D.Add(Light);
            }

            if (Data.MObjectHRC.Length != 0)
            {
                MP MObjectHRC = new MP("MObjectHRC", Data.MObjectHRC.Length);
                for (i18 = 0; i18 < Data.MObjectHRC.Length; i18++)
                {
                    MP _MObjectHRC = new MP();
                    _MObjectHRC.Add("Name", Data.MObjectHRC[i18].Name);

                    if (Data.MObjectHRC[i18].Instance != null)
                    {
                        MP Instance = new MP("Instance", Data.MObjectHRC[i18].Instance.Length);
                        for (i17 = 0; i17 < Data.MObjectHRC[i18].Instance.Length; i17++)
                        {
                            MP _Instance = new MP();
                            _Instance.Add("Name", Data.MObjectHRC[i18].Instance[i17].Name);
                            _Instance.Add("Shadow", Data.MObjectHRC[i18].Instance[i17].Shadow);
                            _Instance.Add("UIDName", Data.MObjectHRC[i18].Instance[i17].UIDName);
                            WriteMP(ref Data.MObjectHRC[i18].Instance[i17].MT, ref _Instance);
                            Instance[i17] = _Instance;
                        }
                        _MObjectHRC.Add(Instance);
                    }

                    if (Data.MObjectHRC[i18].Node != null)
                    {
                        MP Node2 = new MP("Node", Data.MObjectHRC[i18].Node.Length);
                        for (i17 = 0; i17 < Data.MObjectHRC[i18].Node.Length; i17++)
                        {
                            MP _Node2 = new MP();
                            _Node2.Add("Name", Data.MObjectHRC[i18].Node[i17].Name);
                            _Node2.Add("Parent", Data.MObjectHRC[i18].Node[i17].Parent);
                            WriteMP(ref Data.MObjectHRC[i18].Node[i17].MT, ref _Node2);
                            Node2[i17] = _Node2;
                        }
                        _MObjectHRC.Add(Node2);
                    }

                    WriteMP(ref Data.MObjectHRC[i18].MT, ref _MObjectHRC);
                    MObjectHRC[i18] = _MObjectHRC;
                }
                A3D.Add(MObjectHRC);
            }

            if (Data.MObjectHRCList.Length != 0)
            {
                MP MObjectHRCList = new MP("MObjectHRCList", Data.MObjectHRCList.Length);
                for (l = 0; l < Data.MObjectHRCList.Length; l++)
                    MObjectHRCList[l] = Data.MObjectHRCList[l];
                A3D.Add(MObjectHRCList);
            }

            if (Data.MaterialList.Length != 0)
            {
                MP MaterialList = new MP("MaterialList");
                l = 0;
                while (l < Data.MaterialList.Length)
                {
                    MP _Material = new MP("Material");
                    _Material.Add("HashName", Data.MaterialList[l].HashName);
                    _Material.Add("Name", Data.MaterialList[l].Name);
                    _Material.Add(WriteMP(ref Data.MaterialList[l].BlendColor, "BlendColor"));
                    _Material.Add(WriteMP(ref Data.MaterialList[l].GlowIntensity, "GlowIntensity"));
                    _Material.Add(WriteMP(ref Data.MaterialList[l].Incandescence, "Incandescence"));
                    MaterialList[l] = _Material;
                    i18++;
                }
                A3D.Add(MaterialList);
            }

            if (Data.Motion.Length != 0)
            {
                MP Motion = new MP("Motion", Data.Motion.Length);
                for (l = 0; l < Data.Motion.Length; l++)
                    Motion[l] = Data.Motion[l];
                A3D.Add(Motion);
            }

            if (Data.Object.Length != 0)
            {
                MP Object = new MP("Object", Data.Object.Length);
                for (i18 = 0; i18 < Data.Object.Length; i18++)
                {
                    MP _Object = new MP();
                    _Object.Add("Morph", Data.Object[i18].Morph);
                    _Object.Add("MorphOffset", Data.Object[i18].MorphOffset);
                    _Object.Add("Name", Data.Object[i18].Name);
                    _Object.Add("ParentName", Data.Object[i18].ParentName);
                    _Object.Add("UIDName", Data.Object[i18].UIDName);
                    WriteMP(ref Data.Object[i18].MT, ref _Object);
                    if (Data.Object[i18].TP != null)
                    {
                        MP TP = new MP("TP", Data.Object[i18].TP.Length);
                        for (i17 = 0; i17 < Data.Object[i18].TP.Length; i17++)
                        {
                            MP _TP = new MP();
                            _TP.Add("Name", Data.Object[i18].TP[i17].Name);
                            _TP.Add("Pat", Data.Object[i18].TP[i17].Pat);
                            _TP.Add("PatOffset", Data.Object[i18].TP[i17].PatOffset);
                            TP[i17] = _TP;
                        }
                        _Object.Add(TP);
                    }
                    if (Data.Object[i18].TT != null)
                    {
                        MP TT = new MP("TT", Data.Object[i18].TT.Length);
                        for (i17 = 0; i17 < Data.Object[i18].TT.Length; i17++)
                        {
                            MP _TT = new MP();
                            _TT.Add("Name", Data.Object[i18].TT[i17].Name);
                            _TT.Add(WriteMP(ref Data.Object[i18].TT[i17].C, "C"));
                            _TT.Add(WriteMP(ref Data.Object[i18].TT[i17].O, "O"));
                            _TT.Add(WriteMP(ref Data.Object[i18].TT[i17].R, "R"));
                            _TT.Add(WriteMP(ref Data.Object[i18].TT[i17].Ro, "Ro"));
                            _TT.Add(WriteMP(ref Data.Object[i18].TT[i17].RF, "RF"));
                            _TT.Add(WriteMP(ref Data.Object[i18].TT[i17].TF, "TF"));
                            TT[i17] = _TT;
                        }
                        _Object.Add(TT);
                    }
                    Object[i18] = _Object;
                }
                A3D.Add(Object);
            }

            if (Data.ObjectHRC.Length != 0)
            {
                MP ObjectHRC = new MP("ObjectHRC", Data.ObjectHRC.Length);
                for (i18 = 0; i18 < Data.ObjectHRC.Length; i18++)
                {
                    MP _ObjectHRC = new MP();
                    _ObjectHRC.Add("Name", Data.ObjectHRC[i18].Name);
                    _ObjectHRC.Add("Shadow", Data.ObjectHRC[i18].Shadow);
                    _ObjectHRC.Add("UIDName", Data.ObjectHRC[i18].UIDName);
                    if (Data.ObjectHRC[i18].Node != null)
                    {
                        MP Node = new MP("Node", Data.ObjectHRC[i18].Node.Length);
                        for (i17 = 0; i17 < Data.ObjectHRC[i18].Node.Length; i17++)
                        {
                            MP _Node = new MP();
                            _Node.Add("Name", Data.ObjectHRC[i18].Node[i17].Name);
                            _Node.Add("Parent", Data.ObjectHRC[i18].Node[i17].Parent);
                            WriteMP(ref Data.ObjectHRC[i18].Node[i17].MT, ref _Node);
                            Node[i17] = _Node;
                        }
                        _ObjectHRC.Add(Node);
                    }
                    ObjectHRC[i18] = _ObjectHRC;
                }
                A3D.Add(ObjectHRC);
            }

            if (Data.ObjectHRCList.Length != 0)
            {
                MP ObjectHRCList = new MP("ObjectHRCList", Data.ObjectHRCList.Length);
                for (l = 0; l < Data.ObjectHRCList.Length; l++)
                    ObjectHRCList[l] = Data.ObjectHRCList[l];
                A3D.Add(ObjectHRCList);
            }

            if (Data.ObjectList.Length != 0)
            {
                MP ObjectList = new MP("ObjectList", Data.ObjectList.Length);
                for (l = 0; l < Data.ObjectList.Length; l++)
                    ObjectList[l] = Data.ObjectList[l];
                A3D.Add(ObjectList);
            }

            MP PlayControl = new MP("PlayControl");
            if (Data.PlayControl.Begin.HasValue)
                PlayControl.Add("Begin", Data.PlayControl.Begin);
            if (Data.PlayControl.Div.HasValue)
                PlayControl.Add("Div", Data.PlayControl.Div);
            if (Data.PlayControl.FPS.HasValue)
                PlayControl.Add("FPS", Data.PlayControl.FPS);
            if (Data.PlayControl.Offset.HasValue)
                PlayControl.Add("Offset", Data.PlayControl.Offset);
            if (Data.PlayControl.Size.HasValue)
                PlayControl.Add("Size", Data.PlayControl.Size);
            A3D.Add(PlayControl);

            if (Data.Point.Length != 0)
            {
                MP Point = new MP("Point", Data.Point.Length);
                for (l = 0; l < Data.Point.Length; l++)
                {
                    MP _Point = new MP();
                    WriteMP(ref Data.Point[l], ref _Point);
                    Point[l] = _Point;
                }
                A3D.Add(Point);
            }

            if (Data.PostProcess.Boolean)
            {
                MP PostProcess = new MP("PostProcess");
                PostProcess.Add(WriteMP(ref Data.PostProcess.Ambient, "Ambient"));
                PostProcess.Add(WriteMP(ref Data.PostProcess.Diffuse, "Diffuse"));
                PostProcess.Add(WriteMP(ref Data.PostProcess.LensFlare, "LensFlare"));
                PostProcess.Add(WriteMP(ref Data.PostProcess.LensGhost, "LensGhost"));
                PostProcess.Add(WriteMP(ref Data.PostProcess.LensShaft, "LensShaft"));
                PostProcess.Add(WriteMP(ref Data.PostProcess.Specular, "Specular"));
                A3D.Add(PostProcess);
            }

            MsgPack.Add(A3D);

            MPIO IO = new MPIO(KKtIO.OpenWriter(file + ".mp", true));
            IO.Write(MsgPack, true);
            MsgPack = null;
        }

        private MP WriteMP(ref ModelTransform MT, string name)
        {
            MP MTs = new MP(name);
            WriteMP(ref MT, ref MTs);
            return MTs;
        }

        private void WriteMP(ref ModelTransform MT, ref MP MTs)
        {
            MTs.Add(WriteMP(ref MT.Rot, "Rot"));
            MTs.Add(WriteMP(ref MT.Scale, "Scale"));
            MTs.Add(WriteMP(ref MT.Trans, "Trans"));
            MTs.Add(WriteMP(ref MT.Visibility, "Visibility"));
        }

        private MP WriteMP(ref RGBAKey RGBA, string name)
        {
            if (!RGBA.Boolean)
                return null;
            MP RGBAs = new MP(name);
            RGBAs.Add(WriteMP(ref RGBA.R, "R"));
            RGBAs.Add(WriteMP(ref RGBA.G, "G"));
            RGBAs.Add(WriteMP(ref RGBA.B, "B"));
            RGBAs.Add(WriteMP(ref RGBA.A, "A"));
            return RGBAs;
        }

        private MP WriteMP(ref Vector3Key Key, string name)
        {
            MP Keys = new MP(name);
            Keys.Add(WriteMP(ref Key.X, "X"));
            Keys.Add(WriteMP(ref Key.Y, "Y"));
            Keys.Add(WriteMP(ref Key.Z, "Z"));
            return Keys;
        }

        private MP WriteMP(ref KeyUV UV, string name)
        {
            if (!UV.U.Boolean && !UV.V.Boolean)
                return null;
            MP UVs = new MP(name);
            UVs.Add(WriteMP(ref UV.U, "U"));
            UVs.Add(WriteMP(ref UV.V, "V"));
            return UVs;
        }

        private MP WriteMP(ref Key Key, string name)
        {
            int num2;
            if (Key.Boolean)
            {
                int? type = Key.Type;
                int num = 0;
                num2 = ((type.GetValueOrDefault() < num & type.HasValue) ? 1 : 0);
            }
            else
                num2 = 1;
            if (num2 != 0)
                return null;
            MP Keys = new MP(name);
            Keys.Add("T", Key.Type);
            if (Key.Trans != null)
            {
                if (Key.EPTypePost.HasValue)
                    Keys.Add("Post", Key.EPTypePost);
                if (Key.EPTypePre.HasValue)
                    Keys.Add("Pre", Key.EPTypePre);
                if (Key.Max.HasValue)
                    Keys.Add("M", Key.Max);
                if (Key.RawData.Boolean)
                    Keys.Add("RD", Key.RawData.Boolean);
                if (Key.Length.HasValue)
                {
                    MP Trans = new MP("Trans", Key.Trans.Length);
                    for (int i = 0; i < Key.Trans.Length; i++)
                    {
                        MP K = new MP(Key.Trans[i].Type + 1);
                        K[0] = Key.Trans[i].Frame;
                        if (Key.Trans[i].Type > 0)
                        {
                            K[1] = Key.Trans[i].Value1;
                            if (Key.Trans[i].Type > 1)
                            {
                                K[2] = Key.Trans[i].Value2;
                                if (Key.Trans[i].Type > 2)
                                    K[3] = Key.Trans[i].Value3;
                            }
                        }
                        Trans[i] = K;
                    }
                    Keys.Add(Trans);
                }
            }
            else
            {
                double? nullable = Key.Value;
                double num3 = 0.0;
                int num4;
                if (!(nullable.GetValueOrDefault() == num3 & nullable.HasValue))
                {
                    int? type = Key.Type;
                    int num = 0;
                    num4 = ((type.GetValueOrDefault() > num & type.HasValue) ? 1 : 0);
                }
                else
                    num4 = 0;
                if (num4 != 0)
                    Keys.Add("V", Key.Value);
            }
            return Keys;
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
            public bool Boolean;
            public int? KeyType;
            public int? ValueListSize;
            public string ValueType;
            public string ValueListString;
            public string[] ValueList;

            public RawD()
            { Boolean = false; KeyType = null; ValueListSize = null; ValueType = "float";
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
