// Original: https://github.com/blueskythlikesclouds/MikuMikuLibrary

using System;
using System.IO;
using System.Xml;
using System.Linq;
using System.Text;
using System.Numerics;
using System.Collections.Generic;

namespace PD_Tool.DB
{
    class BoneData
    {
        public static int format = 0;
        public static BONE Data;

        public static void BONReader(string file, string ext)
        {
            Data = new BONE { Header = new System.Header() };
            System.reader = new FileStream(file + ext, FileMode.Open, FileAccess.ReadWrite, FileShare.ReadWrite);

            System.format = System.Format.F;
            Data.Header.Signature = System.ReadInt32();
            if (Data.Header.Signature == 0x454E4F42)
            {
                Data.Header = System.HeaderReader();
                Data.POF0 = System.AddPOF0(Data.Header);
            }
            if (Data.Header.Signature != 0x09102720)
                Program.Exit();

            Data.Skeleton = new List<SkeletonEntry>();
            System.IsX = false;
            Data.Skeleton.Capacity = System.ReadInt32(true);
            System.GetOffset(ref Data.POF0);
            Data.SkeletonsOffset = System.ReadUInt32(true);
            System.GetOffset(ref Data.POF0);
            Data.SkeletonNamesOffset = System.ReadUInt32(true);
            if (Data.SkeletonNamesOffset == 0)
            {
                System.IsX = true;
                System.format = System.Format.X;
                Data.POF0.POF0Offsets = new List<long>();
                System.reader.Seek(Data.Header.Lenght, 0);
                Data.Header.Signature = System.ReadInt32();
                Data.Skeleton.Capacity = System.ReadInt32();
                System.GetOffset(ref Data.POF0);
                Data.SkeletonsOffset = System.ReadInt64();
                System.GetOffset(ref Data.POF0);
                Data.SkeletonNamesOffset = System.ReadInt64();
            }
            Data.SkeletonEntryOffset = new List<long>();
            if (System.IsX)
                System.reader.Seek(Data.SkeletonsOffset + Data.Header.Lenght, 0);
            else
                System.reader.Seek(Data.SkeletonsOffset, 0);
            for (int i0 = 0; i0 < Data.Skeleton.Capacity; i0++)
            {
                System.GetOffset(ref Data.POF0);
                if (System.IsX)
                    Data.SkeletonEntryOffset.Add(System.ReadInt64());
                else
                    Data.SkeletonEntryOffset.Add(System.ReadUInt32(true));
            }

            for (int i0 = 0; i0 < Data.Skeleton.Capacity; i0++)
            {
                System.reader.Seek(Data.SkeletonEntryOffset[i0] +
                    (System.IsX ? Data.Header.Lenght : 0), 0);

                SkeletonEntry Skeleton;
                if (System.IsX)
                {
                    Skeleton = new SkeletonEntry
                    {
                        BoneNames1 = new List<string>(),
                        BoneNames2 = new List<string>(),
                        Bones = new List<BoneEntry>(),
                        ParentIndices = new List<short>(),
                        Positions = new List<Vector3>(),
                        Skeleton = new Skeleton()
                    };
                    System.GetOffset(ref Data.POF0);
                    Skeleton.Skeleton.BonesOffset = System.ReadInt64();
                    Skeleton.Skeleton.PositionCount = System.ReadInt64();
                    System.GetOffset(ref Data.POF0);
                    Skeleton.Skeleton.PositionsOffset = System.ReadInt64();
                    System.GetOffset(ref Data.POF0);
                    Skeleton.Skeleton.Field02Offset = System.ReadInt64();
                    Skeleton.Skeleton.BoneName1Count = System.ReadInt64();
                    System.GetOffset(ref Data.POF0);
                    Skeleton.Skeleton.BoneNames1Offset = System.ReadInt64();
                    Skeleton.Skeleton.BoneName2Count = System.ReadInt64();
                    System.GetOffset(ref Data.POF0);
                    Skeleton.Skeleton.BoneNames2Offset = System.ReadInt64();
                    System.GetOffset(ref Data.POF0);
                    Skeleton.Skeleton.ParentIndicesOffset = System.ReadInt64();
                }
                else
                {
                    Skeleton = new SkeletonEntry
                    {
                        BoneNames1 = new List<string>(),
                        BoneNames2 = new List<string>(),
                        Bones = new List<BoneEntry>(),
                        ParentIndices = new List<short>(),
                        Positions = new List<Vector3>(),
                        Skeleton = new Skeleton ()
                    };
                    System.GetOffset(ref Data.POF0);
                    Skeleton.Skeleton.BonesOffset = System.ReadUInt32(true);
                    Skeleton.Skeleton.PositionCount = System.ReadUInt32(true);
                    System.GetOffset(ref Data.POF0);
                    Skeleton.Skeleton.PositionsOffset = System.ReadUInt32(true);
                    System.GetOffset(ref Data.POF0);
                    Skeleton.Skeleton.Field02Offset = System.ReadUInt32(true);
                    Skeleton.Skeleton.BoneName1Count = System.ReadUInt32(true);
                    System.GetOffset(ref Data.POF0);
                    Skeleton.Skeleton.BoneNames1Offset = System.ReadUInt32(true);
                    Skeleton.Skeleton.BoneName2Count = System.ReadUInt32(true);
                    System.GetOffset(ref Data.POF0);
                    Skeleton.Skeleton.BoneNames2Offset = System.ReadUInt32(true);
                    System.GetOffset(ref Data.POF0);
                    Skeleton.Skeleton.ParentIndicesOffset = System.ReadUInt32(true);
                }

                System.reader.Seek(Data.SkeletonNamesOffset +
                    (System.IsX ? i0 * 8 : i0 * 4) + (System.IsX ? Data.Header.Lenght : 0), 0);
                Skeleton.Name = System.NullTerminated(true, ref Data.POF0);

                System.reader.Seek(Skeleton.Skeleton.BonesOffset +
                    (System.IsX ? Data.Header.Lenght : 0), 0);
                while (true)
                {
                    BoneEntry BoneEntry = new BoneEntry
                    {
                        Field00 = (Field00)System.ReadByte(),
                        HasParent = System.ReadBoolean(),
                        ParentNameIndex = System.ReadByte(),
                        Field01 = System.ReadByte(),
                        PairNameIndex = System.ReadByte(),
                        Field02 = System.ReadByte()
                    };
                    System.ReadInt16(true);
                    BoneEntry.Name = System.NullTerminated(true, ref Data.POF0);

                    if (BoneEntry.Name == "End")
                        break;

                    Skeleton.Bones.Add(BoneEntry);
                }

                System.reader.Seek(Skeleton.Skeleton.
                    PositionsOffset + (System.IsX ? Data.Header.Lenght : 0), 0);
                for (int i = 0; i < Skeleton.Skeleton.PositionCount; i++)
                {
                    uint X = System.ReadUInt32(true);
                    uint Y = System.ReadUInt32(true);
                    uint Z = System.ReadUInt32(true);
                    Skeleton.Positions.Add(new Vector3((float)System.ToDouble(X),
                        (float)System.ToDouble(Y), (float)System.ToDouble(Z)));
                }

                System.reader.Seek(Skeleton.Skeleton.
                    Field02Offset + (System.IsX ? Data.Header.Lenght : 0), 0);
                if (System.IsX)
                    Skeleton.Field02 = System.ReadInt64();
                else
                    Skeleton.Field02 = System.ReadInt32(true);

                System.reader.Seek(Skeleton.Skeleton.
                    BoneNames1Offset + (System.IsX ? Data.Header.Lenght : 0), 0);
                for (int i = 0; i < Skeleton.Skeleton.BoneName1Count; i++)
                    Skeleton.BoneNames1.Add(System.NullTerminated(true, ref Data.POF0));

                System.reader.Seek(Skeleton.Skeleton.
                    BoneNames2Offset + (System.IsX ? Data.Header.Lenght : 0), 0);
                for (int i = 0; i < Skeleton.Skeleton.BoneName2Count; i++)
                    Skeleton.BoneNames2.Add(System.NullTerminated(true, ref Data.POF0));

                System.reader.Seek(Skeleton.Skeleton.
                    ParentIndicesOffset + (System.IsX ? Data.Header.Lenght : 0), 0);
                for (int i = 0; i < Skeleton.Skeleton.BoneName2Count; i++)
                    Skeleton.ParentIndices.Add(System.ReadInt16(true));

                Data.Skeleton.Add(Skeleton);
            }
            if ((uint)System.format > 1 && (uint)System.format < 5)
            {
                System.reader.Seek(Data.POF0.Offset, 0);
                System.POF0Reader(ref Data.POF0);
            }
            System.reader.Close();
        }

        public static void BINWriter(string file)
        {
            System.writer = new FileStream(file + ".bin", FileMode.Create, FileAccess.ReadWrite, FileShare.ReadWrite);
            System.writer.SetLength(0);

            System.Write(0x09102720);
            System.Write(Data.Skeleton.Count);
            Data.SkeletonsOffset = System.writer.Position;
            System.Write(0x24);

            Data.SkeletonEntryOffset = new List<long>();
            Skeleton[] SkeletonWrite = new Skeleton[Data.Skeleton.Count];
            Bone UsedBones = new Bone
            {
                Names = new List<string>(),
                Offsets = new List<long>()
            };
            for (int i = 0; i < 6; i++)
                System.Write(0);
            for (int i = 0; i < Data.Skeleton.Count; i++)
                System.Write(0);
            uint[] NamesOffsets = new uint[Data.Skeleton.Count];
            for (int i = 0; i < Data.Skeleton.Count; i++)
            {
                NamesOffsets[i] = (uint)System.writer.Position;
                System.Write(Encoding.ASCII.GetBytes(Data.Skeleton[i].Name + '\0'));
            }
            System.Align(4);
            uint NamesOffset = (uint)System.writer.Position;
            System.writer.Seek(0x0C, 0);
            System.Write(NamesOffset);
            System.writer.Seek(NamesOffset, 0);
            for (int i = 0; i < Data.Skeleton.Count; i++)
                System.Write(NamesOffsets[i]);
            System.Align(16);
            
            for (int i = 0; i < Data.Skeleton.Count; i++)
            {
                SkeletonWrite[i].BoneNameOffsets = BonesNamesadder(ref UsedBones,
                    Data.Skeleton[i].Bones.Count, Data.Skeleton[i].Bones);
                if (UsedBones.Names.Contains("End"))
                {
                    for (int i2 = 0; i2 < UsedBones.Names.Count; i2++)
                        if (UsedBones.Names[i2] == "End")
                        {
                            SkeletonWrite[i].BoneNameOffsets[Data.Skeleton[i].
                                Bones.Count] = (uint)UsedBones.Offsets[i2];
                            break;
                        }
                }
                else
                {
                    SkeletonWrite[i].BoneNameOffsets[Data.Skeleton[i].Bones.Count] =
                        (uint)System.writer.Position;
                    System.Write(Encoding.ASCII.GetBytes("End\0"));
                    UsedBones.Names.Add("End");
                    UsedBones.Offsets.Add(SkeletonWrite[i].
                        BoneNameOffsets[Data.Skeleton[i].Bones.Count]);
                }
                System.Align(4);

                uint BonesOffset = (uint)System.writer.Position;
                for (int i1 = 0; i1 < Data.Skeleton[i].Bones.Count; i1++)
                {
                    System.Write((byte)Data.Skeleton[i].Bones[i1].Field00);
                    System.Write(Data.Skeleton[i].Bones[i1].HasParent);
                    System.Write(Data.Skeleton[i].Bones[i1].ParentNameIndex);
                    System.Write(Data.Skeleton[i].Bones[i1].Field01);
                    System.Write(Data.Skeleton[i].Bones[i1].PairNameIndex);
                    System.Write(Data.Skeleton[i].Bones[i1].Field02);
                    System.Align(4);
                    System.Write((uint)SkeletonWrite[i].BoneNameOffsets[i1]);
                }
                System.Write(0xFF);
                System.Write(0xFF);
                System.Write((uint)SkeletonWrite[i].BoneNameOffsets[Data.Skeleton[i].Bones.Count]);

                uint PositionsOffset = (uint)System.writer.Position;
                for (int i1 = 0; i1 < Data.Skeleton[i].Positions.Count; i1++)
                {
                    System.Write(Data.Skeleton[i].Positions[i1].X);
                    System.Write(Data.Skeleton[i].Positions[i1].Y);
                    System.Write(Data.Skeleton[i].Positions[i1].Z);
                }
                System.Align(16);

                uint Field02Offset = (uint)System.writer.Position;
                System.Write((uint)Data.Skeleton[i].Field02);


                SkeletonWrite[i].BoneName1Offsets = BonesNamesadder(ref UsedBones,
                    Data.Skeleton[i].BoneNames1.Count, Data.Skeleton[i].BoneNames1);
                System.Align(4);

                uint BoneNames1Offset = (uint)System.writer.Position;
                for (int i1 = 0; i1 < Data.Skeleton[i].BoneNames1.Count; i1++)
                    System.Write((uint)SkeletonWrite[i].BoneName1Offsets[i1]);

                SkeletonWrite[i].BoneName2Offsets = BonesNamesadder(ref UsedBones,
                    Data.Skeleton[i].BoneNames2.Count, Data.Skeleton[i].BoneNames2);
                System.Align(4);

                uint BoneNames2Offset = (uint)System.writer.Position;
                for (int i1 = 0; i1 < Data.Skeleton[i].BoneNames2.Count; i1++)
                    System.Write((uint)SkeletonWrite[i].BoneName2Offsets[i1]);

                uint ParentIndicesOffset = (uint)System.writer.Position;
                for (int i1 = 0; i1 < Data.Skeleton[i].ParentIndices.Count; i1++)
                    System.Write(Data.Skeleton[i].ParentIndices[i1]);
                System.Align(4);

                Data.SkeletonEntryOffset.Add(System.writer.Position);
                System.Write(BonesOffset);
                System.Write(Data.Skeleton[i].Positions.Count);
                System.Write(PositionsOffset);
                System.Write(Field02Offset);
                System.Write(Data.Skeleton[i].BoneNames1.Count);
                System.Write(BoneNames1Offset);
                System.Write(Data.Skeleton[i].BoneNames2.Count);
                System.Write(BoneNames2Offset);
                System.Write(ParentIndicesOffset);
                System.Write((long)0);
                System.Write((long)0);
                System.Write((long)0);
                System.Write((long)0);
            }
            
            System.writer.Seek(0x24, 0);
            for (int i = 0; i < Data.Skeleton.Count; i++)
                System.Write((uint)Data.SkeletonEntryOffset[i]);
            System.writer.Seek(0, SeekOrigin.End);
            System.Align(64);
            System.writer.Seek(-4, SeekOrigin.Current);
            System.Write(0);

            SkeletonWrite = null;
            System.writer.Close();
        }

        public static void BONWriterF2nd(string file, System.Format format)
        {
            uint Offset = 0;
            uint CurrentOffset = 0;
            System.format = format;
            System.writer = new FileStream(file + ".bon",  FileMode.Create, FileAccess.ReadWrite, FileShare.ReadWrite);
            System.writer.SetLength(0);
            Data.POF0 = new System.POF0
            {
                Offsets = new List<int>(),
                POF0Offsets = new List<long>()
            };

            System.Write(0x454E4F42);
            System.Write(0);
            System.Write(0x40);
            if ((uint)format == 3)
                System.Write(0x18000000);
            else
                System.Write(0x10000000);
            System.Write(0);
            System.Write(0);
            System.Write(0);
            System.Write(0);
            System.Write(0x8FD5D016);
            System.Write(0x00);
            System.Write((long)0x00);
            System.Write((long)0x00);
            System.Write((long)0x00);

            System.Write(0x09102720, true);
            System.Write(Data.Skeleton.Count, true);
            System.GetOffset(ref Data.POF0);
            Data.SkeletonsOffset = System.writer.Position;
            System.Write(0x50, true);

            System.GetOffset(ref Data.POF0);
            uint NamesOffset = (uint)(0x50 + 0x04 * Data.Skeleton.Count);
            System.Write(NamesOffset, true);

            Data.SkeletonEntryOffset = new List<long>();
            Skeleton[] SkeletonWrite = new Skeleton[Data.Skeleton.Count];
            Bone UsedBones = new Bone
            {
                Names = new List<string>(),
                Offsets = new List<long>()
            };
            for (int i = 0; i < Data.Skeleton.Count * 2; i++)
            {
                System.GetOffset(ref Data.POF0);
                System.Write(0);
            }
            for (int i = 0; i < Data.Skeleton.Count; i++)
            {
                /*UsedBones = new Bone
                {
                    Names = new List<string>(),
                    Offsets = new List<long>()
                };*/
                uint BonesOffset = (uint)System.GetOffset(ref Data.POF0);
                Data.SkeletonEntryOffset.Add(BonesOffset);
                System.Write(0);
                System.Write(Data.Skeleton[i].Positions.Count, true);
                uint PositionsOffset = (uint)System.GetOffset(ref Data.POF0);
                System.Write(0);
                uint Field02Offset = (uint)System.GetOffset(ref Data.POF0);
                System.Write(0);
                System.Write(Data.Skeleton[i].BoneNames1.Count, true);
                uint BoneNames1Offset = (uint)System.GetOffset(ref Data.POF0);
                System.Write(0);
                System.Write(Data.Skeleton[i].BoneNames2.Count, true);
                uint BoneNames2Offset = (uint)System.GetOffset(ref Data.POF0);
                System.Write(0);
                uint ParentIndicesOffset = (uint)System.GetOffset(ref Data.POF0);
                System.Write(0);
                System.Align(32);

                if (i % 2 == 0 && i != 0)
                    System.writer.Seek(8, SeekOrigin.Current);
                else if (i % 2 == 1)
                    System.writer.Seek(-8, SeekOrigin.Current);
                Offset = (uint)System.writer.Position;
                System.writer.Seek(BonesOffset, 0);
                System.Write(Offset, true);
                System.writer.Seek(Offset, 0);
                SkeletonWrite[i].BoneNamesOffsets = new List<long>();
                for (int i1 = 0; i1 < Data.Skeleton[i].Bones.Count; i1++)
                {
                    System.Write((byte)Data.Skeleton[i].Bones[i1].Field00);
                    System.Write(Data.Skeleton[i].Bones[i1].HasParent);
                    System.Write(Data.Skeleton[i].Bones[i1].ParentNameIndex);
                    System.Write(Data.Skeleton[i].Bones[i1].Field01);
                    System.Write(Data.Skeleton[i].Bones[i1].PairNameIndex);
                    System.Write(Data.Skeleton[i].Bones[i1].Field02);
                    System.Write((short)0);
                    SkeletonWrite[i].BoneNamesOffsets.Add((uint)System.GetOffset(ref Data.POF0));
                    System.Write(0);
                }
                System.Write(0xFF);
                System.Write(0xFF);
                SkeletonWrite[i].BoneNamesOffsets.Add((uint)System.GetOffset(ref Data.POF0));
                System.Write(0);
                System.Align(16);

                Offset = (uint)System.writer.Position;
                System.writer.Seek(PositionsOffset, 0);
                System.Write(Offset, true);
                System.writer.Seek(Offset, 0);
                for (int i1 = 0; i1 < Data.Skeleton[i].Positions.Count; i1++)
                {
                    System.Write(Data.Skeleton[i].Positions[i1].X, true);
                    System.Write(Data.Skeleton[i].Positions[i1].Y, true);
                    System.Write(Data.Skeleton[i].Positions[i1].Z, true);
                }
                System.Align(16);

                Offset = (uint)System.writer.Position;
                System.writer.Seek(Field02Offset, 0);
                System.Write(Offset, true);
                System.writer.Seek(Offset, 0);
                System.Write((uint)Data.Skeleton[i].Field02, true);

                Offset = (uint)System.writer.Position;
                System.writer.Seek(BoneNames1Offset, 0);
                System.Write(Offset, true);
                System.writer.Seek(Offset, 0);
                SkeletonWrite[i].BoneNames1Offsets = new List<long>();
                for (int i1 = 0; i1 < Data.Skeleton[i].BoneNames1.Count; i1++)
                {
                    SkeletonWrite[i].BoneNames1Offsets.Add((uint)System.GetOffset(ref Data.POF0));
                    System.Write(0);
                }

                Offset = (uint)System.writer.Position;
                System.writer.Seek(BoneNames2Offset, 0);
                System.Write(Offset, true);
                System.writer.Seek(Offset, 0);
                SkeletonWrite[i].BoneNames2Offsets = new List<long>();
                for (int i1 = 0; i1 < Data.Skeleton[i].BoneNames2.Count; i1++)
                {
                    SkeletonWrite[i].BoneNames2Offsets.Add((uint)System.GetOffset(ref Data.POF0));
                    System.Write(0);
                }

                Offset = (uint)System.writer.Position;
                System.writer.Seek(ParentIndicesOffset, 0);
                System.Write(Offset, true);
                System.writer.Seek(Offset, 0);
                for (int i1 = 0; i1 < Data.Skeleton[i].ParentIndices.Count; i1++)
                    System.Write(Data.Skeleton[i].ParentIndices[i1], true);
                System.Align(16);
            }
            
            for (int i = 0; i < Data.Skeleton.Count; i++)
            {
                SkeletonWrite[i].BoneNameOffsets = BonesNamesadder(ref UsedBones,
                    Data.Skeleton[i].Bones.Count, Data.Skeleton[i].Bones);
                if (UsedBones.Names.Contains("End"))
                {
                    for (int i2 = 0; i2 < UsedBones.Names.Count; i2++)
                        if (UsedBones.Names[i2] == "End")
                        {
                            SkeletonWrite[i].BoneNameOffsets[Data.Skeleton[i].
                                Bones.Count] = UsedBones.Offsets[i2];
                            break;
                        }
                }
                else
                {
                    SkeletonWrite[i].BoneNameOffsets[Data.Skeleton[i].
                        Bones.Count] = (uint)System.writer.Position;
                    System.Write(Encoding.ASCII.GetBytes("End\0"));
                    UsedBones.Names.Add("End");
                    UsedBones.Offsets.Add(SkeletonWrite[i].
                        BoneNameOffsets[Data.Skeleton[i].Bones.Count]);
                }

                SkeletonWrite[i].BoneName1Offsets = BonesNamesadder(ref UsedBones,
                    Data.Skeleton[i].BoneNames1.Count, Data.Skeleton[i].BoneNames1);
                SkeletonWrite[i].BoneName2Offsets = BonesNamesadder(ref UsedBones,
                    Data.Skeleton[i].BoneNames2.Count, Data.Skeleton[i].BoneNames2);
            }
            for (int i = 0; i < Data.Skeleton.Count; i++)
            {
                Offset = (uint)System.writer.Position;
                System.writer.Seek(NamesOffset + i * 0x04, 0);
                System.Write(Offset, true);
                System.writer.Seek(Offset, 0);
                System.Write(Encoding.ASCII.GetBytes(Data.Skeleton[i].Name + '\0'));
            }
            for (int i = 0; i < Data.Skeleton.Count; i++)
            {
                Offset = (uint)System.writer.Position;
                System.writer.Seek(0x50 + i * 0x04, 0);
                System.Write((uint)Data.SkeletonEntryOffset[i], true);
                System.writer.Seek(Offset, 0);
                Offset = (uint)System.writer.Position;
                for (int i1 = 0; i1 < Data.Skeleton[i].BoneNames1.Count; i1++)
                {
                    System.writer.Seek(SkeletonWrite[i].BoneNames1Offsets[i1], 0);
                    System.Write(SkeletonWrite[i].BoneName1Offsets[i1], true);
                }
                System.writer.Seek(Offset, 0);
                Offset = (uint)System.writer.Position;
                for (int i1 = 0; i1 < Data.Skeleton[i].BoneNames2.Count; i1++)
                {
                    System.writer.Seek(SkeletonWrite[i].BoneNames2Offsets[i1], 0);
                    System.Write(SkeletonWrite[i].BoneName2Offsets[i1], true);
                }
                System.writer.Seek(Offset, 0);
                Offset = (uint)System.writer.Position;
                for (int i1 = 0; i1 < Data.Skeleton[i].Bones.Count + 1; i1++)
                {
                    System.writer.Seek(SkeletonWrite[i].BoneNamesOffsets[i1], 0);
                    System.Write(SkeletonWrite[i].BoneNameOffsets[i1], true);
                }
                System.writer.Seek(Offset, 0);
            }
            SkeletonWrite = null;

            System.writer.Seek(System.writer.Length, 0);
            System.Align(16);
            Offset = (uint)System.writer.Position;
            Data.POF0.POF0Offsets = Data.POF0.POF0Offsets.Distinct().OrderBy(x => x).ToList();
            List<long> POF0Offsets1 = new List<long>();
            long CurrentPOF0Offset = 0;
            int POF0Lenght = 0;
            for (int i = 0; i < Data.POF0.POF0Offsets.Count; i++)
            {
                long POF0Offset = Data.POF0.POF0Offsets[i] - CurrentPOF0Offset;
                if (POF0Offset != 0)
                {
                    if (POF0Offset < 0xFF)
                        POF0Lenght += 1;
                    else if (POF0Offset < 0xFFFF)
                        POF0Lenght += 2;
                    else
                        POF0Lenght += 4;
                    POF0Offsets1.Add(POF0Offset);
                }
                CurrentPOF0Offset = Data.POF0.POF0Offsets[i];
            }
            Data.POF0.POF0Offsets = POF0Offsets1;

            POF0Lenght += 5;
            System.Write(Encoding.ASCII.GetBytes("POF0"));
            long POF0Lenghtaling = (POF0Lenght % 16 != 0) ? (POF0Lenght +
                16 - POF0Lenght % 16) : POF0Lenght;
            System.Write((uint)POF0Lenghtaling);
            System.Write(0x20);
            if (System.IsBE)
                System.Write(0x18000000);
            else
                System.Write(0x10000000);
            System.Write(0x00);
            System.Write((uint)POF0Lenghtaling);
            System.Write((long)0x00);
            System.Write(POF0Lenght);
            for (int i = 0; i < Data.POF0.POF0Offsets.Count; i++)
            {
                long POF0Offset = Data.POF0.POF0Offsets[i];
                if (POF0Offset < 0xFF)
                    System.Write((byte)(0x40 | (POF0Offset >> 2)));
                else if (POF0Offset < 0xFFFF)
                    System.Write((ushort)(0x8000 | (POF0Offset >> 2)), true, true);
                else
                    System.Write((uint)(0xC0000000 | (POF0Offset >> 2)), true, true);
            }
            System.Write(0);

            if (System.writer.Position % 16 != 0)
                System.writer.Seek(16 -
                    System.writer.Position % 16, SeekOrigin.Current);

            for (int i = 0; i < 2; i++)
            {
                System.EOFCWriter();
                if (i == 0)
                    CurrentOffset = (uint)System.writer.Length;
            }
            System.writer.Seek(0x04, 0);
            System.Write(CurrentOffset - 0x40);
            System.writer.Seek(0x14, 0);
            System.Write(Offset - 0x40);
            System.writer.Close();
        }

        public static void BONWriterX(string file)
        {
            long Offset = 0;
            long CurrentOffset = 0;
            System.writer = new FileStream(file + ".bon", FileMode.Create, FileAccess.ReadWrite, FileShare.ReadWrite);
            System.writer.SetLength(0);
            Data.POF0 = new System.POF0
            {
                Offsets = new List<int>(),
                POF0Offsets = new List<long>()
            };

            System.Write(0x454E4F42);
            System.Write(0);
            System.Write(0x20);
            System.Write(0x10000000);
            System.Write(0);
            System.Write(0);
            System.Write(0);
            System.Write(0);
            System.Write(0x09102720);
            System.Write(Data.Skeleton.Count);
            System.GetOffset(ref Data.POF0);
            Data.SkeletonsOffset = System.writer.Position;
            System.Write((long)0x20);

            System.GetOffset(ref Data.POF0);
            //
            long NamesOffset = 0x40 + 0x08 * Data.Skeleton.Count;
            NamesOffset += 16 - NamesOffset % 16;
            System.Write(NamesOffset - 0x20);
            System.Align(16);

            Data.SkeletonEntryOffset = new List<long>();
            Skeleton[] SkeletonWrite = new Skeleton[Data.Skeleton.Count];
            Bone UsedBones = new Bone
            {
                Names = new List<string>(),
                Offsets = new List<long>()
            };
            for (int i0 = 0; i0 < 2; i0++)
            {
                for (int i = 0; i < Data.Skeleton.Count; i++)
                {
                    System.GetOffset(ref Data.POF0);
                    System.Write((long)0);
                }
                System.Align(16);
            }
            for (int i = 0; i < Data.Skeleton.Count; i++)
            {
                SkeletonWrite[i].BonesOffset = System.GetOffset(ref Data.POF0);
                Data.SkeletonEntryOffset.Add(SkeletonWrite[i].BonesOffset);
                System.Write((long)0);
                System.Write((long)Data.Skeleton[i].Positions.Count);
                SkeletonWrite[i].PositionsOffset = System.GetOffset(ref Data.POF0);
                System.Write((long)0);
                SkeletonWrite[i].Field02Offset = System.GetOffset(ref Data.POF0);
                System.Write((long)0);
                System.Write((long)Data.Skeleton[i].BoneNames1.Count);
                SkeletonWrite[i].BoneNames1Offset = System.GetOffset(ref Data.POF0);
                System.Write((long)0);
                System.Write((long)Data.Skeleton[i].BoneNames2.Count);
                SkeletonWrite[i].BoneNames2Offset = System.GetOffset(ref Data.POF0);
                System.Write((long)0);
                SkeletonWrite[i].ParentIndicesOffset = System.GetOffset(ref Data.POF0);
                System.Write((long)0);
                System.Align(16);
                System.Write((long)0);
                System.Write((long)0);
                System.Write((long)0);
                System.Write((long)0);
            }
            for (int i = 0; i < Data.Skeleton.Count; i++)
            {
                /*UsedBones = new Bone
                {
                    Names = new List<string>(),
                    Offsets = new List<long>()
                };*/
                Offset = System.writer.Position;
                System.writer.Seek(SkeletonWrite[i].BonesOffset, 0);
                System.Write(Offset - 0x20);
                System.writer.Seek(Offset, 0);
                SkeletonWrite[i].BoneNamesOffsets = new List<long>();
                for (int i1 = 0; i1 < Data.Skeleton[i].Bones.Count; i1++)
                {
                    System.Write((byte)Data.Skeleton[i].Bones[i1].Field00);
                    System.Write(Data.Skeleton[i].Bones[i1].HasParent);
                    System.Write(Data.Skeleton[i].Bones[i1].ParentNameIndex);
                    System.Write(Data.Skeleton[i].Bones[i1].Field01);
                    System.Write(Data.Skeleton[i].Bones[i1].PairNameIndex);
                    System.Write(Data.Skeleton[i].Bones[i1].Field02);
                    System.Write((short)0);
                    SkeletonWrite[i].BoneNamesOffsets.Add(System.GetOffset(ref Data.POF0));
                    System.Write((long)0);
                }
                System.Write(0xFF);
                System.Write(0xFF);
                SkeletonWrite[i].BoneNamesOffsets.Add(System.GetOffset(ref Data.POF0));
                System.Write((long)0);
                System.Align(16);

                Offset = System.writer.Position;
                System.writer.Seek(SkeletonWrite[i].PositionsOffset, 0);
                System.Write(Offset - 0x20);
                System.writer.Seek(Offset, 0);
                for (int i1 = 0; i1 < Data.Skeleton[i].Positions.Count; i1++)
                {
                    System.Write(Data.Skeleton[i].Positions[i1].X);
                    System.Write(Data.Skeleton[i].Positions[i1].Y);
                    System.Write(Data.Skeleton[i].Positions[i1].Z);
                }
                System.Align(16);

                Offset = System.writer.Position;
                System.writer.Seek(SkeletonWrite[i].Field02Offset, 0);
                System.Write(Offset - 0x20);
                System.writer.Seek(Offset, 0);
                System.Write(Data.Skeleton[i].Field02);
                System.Align(16);

                Offset = System.writer.Position;
                System.writer.Seek(SkeletonWrite[i].BoneNames1Offset, 0);
                System.Write(Offset - 0x20);
                System.writer.Seek(Offset, 0);
                SkeletonWrite[i].BoneNames1Offsets = new List<long>();
                for (int i1 = 0; i1 < Data.Skeleton[i].BoneNames1.Count; i1++)
                {
                    SkeletonWrite[i].BoneNames1Offsets.Add(System.GetOffset(ref Data.POF0));
                    System.Write((long)0);
                }
                System.Align(16);

                Offset = System.writer.Position;
                System.writer.Seek(SkeletonWrite[i].BoneNames2Offset, 0);
                System.Write(Offset - 0x20);
                System.writer.Seek(Offset, 0);
                SkeletonWrite[i].BoneNames2Offsets = new List<long>();
                for (int i1 = 0; i1 < Data.Skeleton[i].BoneNames2.Count; i1++)
                {
                    SkeletonWrite[i].BoneNames2Offsets.Add(System.GetOffset(ref Data.POF0));
                    System.Write((long)0);
                }
                System.Align(16);

                Offset = System.writer.Position;
                System.writer.Seek(SkeletonWrite[i].ParentIndicesOffset, 0);
                System.Write(Offset - 0x20);
                System.writer.Seek(Offset, 0);
                for (int i1 = 0; i1 < Data.Skeleton[i].ParentIndices.Count; i1++)
                    System.Write(Data.Skeleton[i].ParentIndices[i1]);
                System.Align(16);
            }

            for (int i = 0; i < Data.Skeleton.Count; i++)
            {
                Offset = System.writer.Position;
                System.writer.Seek(NamesOffset + i * 0x08, 0);
                System.Write(Offset - 0x20);
                System.writer.Seek(Offset, 0);
                System.Write(Encoding.ASCII.GetBytes(Data.Skeleton[i].Name + '\0'));
            }
            for (int i = 0; i < Data.Skeleton.Count; i++)
            {
                SkeletonWrite[i].BoneNameOffsets = BonesNamesadder(ref UsedBones,
                    Data.Skeleton[i].Bones.Count, Data.Skeleton[i].Bones);
                if (UsedBones.Names.Contains("End"))
                {
                    for (int i2 = 0; i2 < UsedBones.Names.Count; i2++)
                        if (UsedBones.Names[i2] == "End")
                        {
                            SkeletonWrite[i].BoneNameOffsets[Data.
                                Skeleton[i].Bones.Count] = UsedBones.Offsets[i2];
                            break;
                        }
                }
                else
                {
                    SkeletonWrite[i].BoneNameOffsets[Data.
                        Skeleton[i].Bones.Count] = System.writer.Position;
                    System.Write(Encoding.ASCII.GetBytes("End\0"));
                    UsedBones.Names.Add("End");
                    UsedBones.Offsets.Add(SkeletonWrite[i].
                        BoneNameOffsets[Data.Skeleton[i].Bones.Count]);
                }

                SkeletonWrite[i].BoneName1Offsets = BonesNamesadder(ref UsedBones,
                    Data.Skeleton[i].BoneNames1.Count, Data.Skeleton[i].BoneNames1);
                SkeletonWrite[i].BoneName2Offsets = BonesNamesadder(ref UsedBones,
                    Data.Skeleton[i].BoneNames2.Count, Data.Skeleton[i].BoneNames2);
            }
            for (int i = 0; i < Data.Skeleton.Count; i++)
            {
                Offset = System.writer.Position;
                System.writer.Seek(0x40 + i * 0x08, 0);
                System.Write(Data.SkeletonEntryOffset[i] - 0x20);
                System.writer.Seek(Offset, 0);
                Offset = System.writer.Position;
                for (int i1 = 0; i1 < Data.Skeleton[i].BoneNames1.Count; i1++)
                {
                    System.writer.Seek(SkeletonWrite[i].BoneNames1Offsets[i1], 0);
                    System.Write(SkeletonWrite[i].BoneName1Offsets[i1] - 0x20);
                }
                System.writer.Seek(Offset, 0);
                Offset = System.writer.Position;
                for (int i1 = 0; i1 < Data.Skeleton[i].BoneNames2.Count; i1++)
                {
                    System.writer.Seek(SkeletonWrite[i].BoneNames2Offsets[i1], 0);
                    System.Write(SkeletonWrite[i].BoneName2Offsets[i1] - 0x20);
                }
                System.writer.Seek(Offset, 0);
                Offset = System.writer.Position;
                for (int i1 = 0; i1 < Data.Skeleton[i].Bones.Count + 1; i1++)
                {
                    System.writer.Seek(SkeletonWrite[i].BoneNamesOffsets[i1], 0);
                    System.Write(SkeletonWrite[i].BoneNameOffsets[i1] - 0x20);
                }
                System.writer.Seek(Offset, 0);
            }
            SkeletonWrite = null;

            System.writer.Seek(System.writer.Length, 0);
            System.Align(16);
            Offset = System.writer.Position;

            Data.POF0.POF0Offsets = Data.POF0.POF0Offsets.Distinct().OrderBy(x => x).ToList();
            List<long> POF0Offsets1 = new List<long>();
            long CurrentPOF0Offset = 0;
            int POF0Lenght = 0;
            for (int i = 0; i < Data.POF0.POF0Offsets.Count; i++)
            {
                long POF0Offset = Data.POF0.POF0Offsets[i] - CurrentPOF0Offset;
                if (POF0Offset != 0)
                {
                    if (POF0Offset < 0x1FF)
                        POF0Lenght += 1;
                    else if (POF0Offset < 0x1FFFF)
                        POF0Lenght += 2;
                    else
                        POF0Lenght += 4;
                    POF0Offsets1.Add(POF0Offset);
                }
                CurrentPOF0Offset = Data.POF0.POF0Offsets[i];
            }
            Data.POF0.POF0Offsets = POF0Offsets1;

            POF0Lenght += 4;
            System.Write(Encoding.ASCII.GetBytes("POF1"));

            long POF0Lenghtaling = (POF0Lenght % 16 != 0) ? (POF0Lenght +
                16 - POF0Lenght % 16) : POF0Lenght;
            System.Write((uint)POF0Lenghtaling);
            System.Write(0x20);
            if (System.IsBE)
                System.Write(0x18000000);
            else
                System.Write(0x10000000);
            System.Write(0x00);
            System.Write((uint)POF0Lenghtaling);
            System.Write((long)0x00);
            System.Write((uint)POF0Lenght);
            for (int i = 0; i < Data.POF0.POF0Offsets.Count; i++)
            {
                long POF0Offset = Data.POF0.POF0Offsets[i];
                if (POF0Offset < 0x1FF)
                    System.Write((byte)(0x40 | (POF0Offset >> 3)));
                else if (POF0Offset < 0x1FFFF)
                    System.Write((ushort)(0x8000 | (POF0Offset >> 3)), true, true);
                else
                    System.Write((uint)(0xC0000000 | (POF0Offset >> 3)), true, true);
            }
            System.Align(16);
            for (int i = 0; i < 2; i++)
            {
                System.EOFCWriter();
                if (i == 0)
                    CurrentOffset = System.writer.Length;
            }
            System.writer.Seek(0x04, 0);
            System.Write((uint)(CurrentOffset - 0x20));
            System.writer.Seek(0x14, 0);
            System.Write((uint)(Offset - 0x20));
            System.writer.Close();
        }

        static long[] BonesNamesadder(ref Bone UsedBones, int Count, List<string> BoneNames)
        {
            long[] BoneNamesOffset = new long[Count];
            for (int i1 = 0; i1 < Count; i1++)
            {
                if (UsedBones.Names.Contains(BoneNames[i1]))
                {
                    for (int i2 = 0; i2 < UsedBones.Names.Count; i2++)
                        if (UsedBones.Names[i2] == BoneNames[i1])
                        {
                            BoneNamesOffset[i1] = UsedBones.Offsets[i2];
                            break;
                        }
                }
                else
                {
                    BoneNamesOffset[i1] = System.writer.Position;
                    System.Write(Encoding.ASCII.GetBytes(BoneNames[i1]));
                    System.Write('\0');
                    UsedBones.Names.Add(BoneNames[i1]);
                    UsedBones.Offsets.Add(BoneNamesOffset[i1]);
                }
            }
            return BoneNamesOffset;
        }

        static long[] BonesNamesadder(ref Bone UsedBones, int Count, List<BoneEntry> Bones)
        {
            long[] BoneNamesOffset = new long[Count + 1];
            for (int i1 = 0; i1 < Count; i1++)
            {
                if (UsedBones.Names.Contains(Bones[i1].Name))
                {
                    for (int i2 = 0; i2 < UsedBones.Names.Count; i2++)
                        if (UsedBones.Names[i2] == Bones[i1].Name)
                        {
                            BoneNamesOffset[i1] = UsedBones.Offsets[i2];
                            break;
                        }
                }
                else
                {
                    BoneNamesOffset[i1] = System.writer.Position;
                    System.Write(Encoding.ASCII.GetBytes(Bones[i1].Name + '\0'));
                    UsedBones.Names.Add(Bones[i1].Name);
                    UsedBones.Offsets.Add(BoneNamesOffset[i1]);
                }
            }
            return BoneNamesOffset;
        }

        public static void XMLReader(string file, string ext, int format)
        {
            Data = new BONE
            {
                Header = new System.Header()
                {
                    Lenght = format == 4 ? 20 :
                (format == 2 | format == 3 ? 40 : 0)
                },
                Skeleton = new List<SkeletonEntry>()
            };
            if (format == 2)
                System.IsBE = true;
            if (format == 4)
                System.IsX = true;
            System.doc = new XmlDocument();
            System.doc.Load(file + ext);
            XmlElement BoneDatabase = System.doc.DocumentElement;
            if (BoneDatabase.Name == "BoneDatabase")
                foreach (XmlNode Skeletons in BoneDatabase.ChildNodes)
                {
                    if (Skeletons.Name == "Skeletons")
                        foreach (XmlNode SkeletonEntry in Skeletons.ChildNodes)
                            if (SkeletonEntry.Name == "SkeletonEntry")
                                Data.Skeleton.Add(SkeletonEntryXMLReader(SkeletonEntry));

                    if (Skeletons.Name == "SkeletonEntry")
                    {
                        System.XMLCompact = true;
                        Data.Skeleton.Add(SkeletonEntryXMLReader(Skeletons));
                        System.XMLCompact = false;
                    }
                }
        }

        static SkeletonEntry SkeletonEntryXMLReader(XmlNode SkeletonEntry)
        {
            SkeletonEntry Skeleton = new SkeletonEntry
            {
                BoneNames1 = new List<string>(),
                BoneNames2 = new List<string>(),
                Bones = new List<BoneEntry>(),
                ParentIndices = new List<short>(),
                Positions = new List<Vector3>()
            };

            foreach (XmlAttribute Entry in SkeletonEntry.Attributes)
            {
                if (Entry.Name == "Field02")
                    Skeleton.Field02 = long.Parse(Entry.Value);
                if (Entry.Name == "Name")
                    Skeleton.Name = Entry.Value;
            }

            foreach (XmlNode Entry in SkeletonEntry)
            {
                if (Entry.Name == "BoneNames1")
                    for (int i = 0; i < Entry.ChildNodes.Count; i++)
                        Skeleton.BoneNames1.Add(Entry.ChildNodes[i].InnerText);
                if (Entry.Name == "BoneNames2")
                    for (int i = 0; i < Entry.ChildNodes.Count; i++)
                        Skeleton.BoneNames2.Add(Entry.ChildNodes[i].InnerText);
                if (Entry.Name == "Field02")
                    Skeleton.Field02 = long.Parse(Entry.InnerText);
                if (Entry.Name == "Name")
                    Skeleton.Name = Entry.InnerText;
                if (Entry.Name == "Bones")
                    foreach (XmlNode BoneEntry in Entry.ChildNodes)
                        if (BoneEntry.Name == "BoneEntry")
                            Skeleton.Bones.Add(BoneEntryXMLReader(BoneEntry));
                if (Entry.Name == "ParentIndices")
                    for (int i = 0; i < Entry.ChildNodes.Count; i++)
                        Skeleton.ParentIndices.Add(short.Parse(Entry.ChildNodes[i].InnerText));
                if (Entry.Name == "Positions")
                    foreach (XmlNode Position in Entry.ChildNodes)
                        if (Position.Name == "Vector3")
                        {
                            float X = 0;
                            float Y = 0;
                            float Z = 0;
                            for (int i = 0; (System.XMLCompact ? i < Position.Attributes.
                                Count : i < Position.ChildNodes.Count); i++)
                            {
                                XmlNode Pos = Position.ChildNodes[i];
                                if (System.XMLCompact)
                                    Pos = Position.Attributes[i];
                                string InnerText = Pos.InnerText;
                                if (System.XMLCompact)
                                    InnerText = Pos.Value;
                                if (Pos.Name == "X")
                                    X = float.Parse(InnerText);
                                if (Pos.Name == "Y")
                                    Y = float.Parse(InnerText);
                                if (Pos.Name == "Z")
                                    Z = float.Parse(InnerText);
                            }
                            Skeleton.Positions.Add(new Vector3(X, Y, Z));
                        }
            }
            return Skeleton;
        }

        static BoneEntry BoneEntryXMLReader(XmlNode BoneEntry)
        {
            BoneEntry Bone = new BoneEntry
            {
                Field00 = 0,
                Field01 = 0,
                Field02 = 0,
                HasParent = false,
                Name = "",
                PairNameIndex = 0,
                ParentNameIndex = 0
            };
            for (int i = 0; (System.XMLCompact ? i < BoneEntry.Attributes.
                Count : i < BoneEntry.ChildNodes.Count); i++)
            {
                XmlNode ChildBoneEntry = BoneEntry.ChildNodes[i];
                if (System.XMLCompact)
                    ChildBoneEntry = BoneEntry.Attributes[i];
                string InnerText = ChildBoneEntry.InnerText;
                if (System.XMLCompact)
                    InnerText = ChildBoneEntry.Value;
                if (ChildBoneEntry.Name == "Field00")
                {
                    int Field00 = 0;
                    if (!System.XMLCompact)
                    {
                        if (InnerText.Contains("Flag04"))
                            Field00 |= 4;
                        if (InnerText.Contains("Flag02"))
                            Field00 |= 2;
                        if (InnerText.Contains("Flag01"))
                            Field00 |= 1;
                    }
                    else
                        Field00 = byte.Parse(ChildBoneEntry.Value);
                    Bone.Field00 = (Field00)Field00;
                }
                else if (ChildBoneEntry.Name == "Field01")
                    Bone.Field01 = byte.Parse(InnerText);
                else if (ChildBoneEntry.Name == "Field02")
                    Bone.Field02 = byte.Parse(InnerText);
                else if (ChildBoneEntry.Name == "HasParent")
                    Bone.HasParent = bool.Parse(InnerText);
                else if (ChildBoneEntry.Name == "Name")
                    Bone.Name = InnerText;
                else if (ChildBoneEntry.Name == "PairNameIndex")
                    Bone.PairNameIndex = byte.Parse(InnerText);
                else if (ChildBoneEntry.Name == "ParentNameIndex")
                    Bone.ParentNameIndex = byte.Parse(InnerText);
            }
            return Bone;
        }

        public static void XMLWriter(string file, int format)
        {
            System.doc = new XmlDocument();
            if (File.Exists(file + ".xml"))
                File.Delete(file + ".xml");
            System.doc.InsertBefore(System.doc.CreateXmlDeclaration("1.0",
                "utf-8", null), System.doc.DocumentElement);
            XmlElement BoneDatabase = System.doc.CreateElement("BoneDatabase");
            XmlElement Skeletons = System.doc.CreateElement("Skeletons");
            for (int i0 = 0; i0 < Data.Skeleton.Count; i0++)
            {
                System.XMLCompact = format == 14;
                XmlElement SkeletonEntry = System.doc.CreateElement("SkeletonEntry");
                System.XMLWriter(ref SkeletonEntry, Data.Skeleton[i0].Name, "Name");

                System.XMLWriter(ref SkeletonEntry, Data.Skeleton[i0].
                    Field02.ToString().Replace(", ", " "), "Field02");

                XmlElement Bones = System.doc.CreateElement("Bones");
                for (int i = 0; i < Data.Skeleton[i0].Bones.Count; i++)
                {
                    BoneEntry Bone = Data.Skeleton[i0].Bones[i];
                    XmlElement BoneEntry = System.doc.CreateElement("BoneEntry");
                    if (!System.XMLCompact)
                    {
                        if (Bone.Field00 != 0)
                        {
                            string Field00 = "";
                            if ((((byte)Bone.Field00 >> 0) & 0x1) == 1)
                                Field00 += "Flag01 ";
                            if ((((byte)Bone.Field00 >> 1) & 0x1) == 1)
                                Field00 += "Flag02 ";
                            if ((((byte)Bone.Field00 >> 2) & 0x1) == 1)
                                Field00 += "Flag04 ";
                            System.XMLWriter(ref BoneEntry, Field00.Remove(Field00.Length - 1), "Field00");
                        }
                        else
                            System.XMLWriter(ref BoneEntry, "", "Field00");
                    }
                    else
                        System.XMLWriter(ref BoneEntry, ((byte)Bone.Field00).ToString(), "Field00");
                    System.XMLWriter(ref BoneEntry, Bone.HasParent.ToString().ToLower(), "HasParent");
                    System.XMLWriter(ref BoneEntry, Bone.ParentNameIndex.ToString(), "ParentNameIndex");
                    System.XMLWriter(ref BoneEntry, Bone.Field01.ToString(), "Field01");
                    System.XMLWriter(ref BoneEntry, Bone.PairNameIndex.ToString(), "PairNameIndex");
                    System.XMLWriter(ref BoneEntry, Bone.Field02.ToString(), "Field02");
                    System.XMLWriter(ref BoneEntry, Bone.Name, "Name");
                    Bones.AppendChild(BoneEntry);
                }
                SkeletonEntry.AppendChild(Bones);

                XmlElement Positions = System.doc.CreateElement("Positions");
                for (int i = 0; i < Data.Skeleton[i0].Positions.Count; i++)
                {
                    Vector3 Position = Data.Skeleton[i0].Positions[i];
                    XmlElement Vector3 = System.doc.CreateElement("Vector3");
                    System.XMLWriter(ref Vector3, Position.X.ToString(), "X");
                    System.XMLWriter(ref Vector3, Position.Y.ToString(), "Y");
                    System.XMLWriter(ref Vector3, Position.Z.ToString(), "Z");
                    Positions.AppendChild(Vector3);
                }
                SkeletonEntry.AppendChild(Positions);

                System.XMLCompact = false;
                XmlElement BoneNames1 = System.doc.CreateElement("BoneNames1");
                foreach (string BoneName1 in Data.Skeleton[i0].BoneNames1)
                    System.XMLWriter(ref BoneNames1, BoneName1, "string");
                SkeletonEntry.AppendChild(BoneNames1);

                XmlElement BoneNames2 = System.doc.CreateElement("BoneNames2");
                foreach (string BoneName2 in Data.Skeleton[i0].BoneNames2)
                    System.XMLWriter(ref BoneNames2, BoneName2, "string");
                SkeletonEntry.AppendChild(BoneNames2);

                XmlElement ParentIndices = System.doc.CreateElement("ParentIndices");
                foreach (short ParentIndice in Data.Skeleton[i0].ParentIndices)
                    System.XMLWriter(ref ParentIndices, ParentIndice.ToString(), "short");
                SkeletonEntry.AppendChild(ParentIndices);

                if (format == 15)
                    Skeletons.AppendChild(SkeletonEntry);
                else
                    BoneDatabase.AppendChild(SkeletonEntry);
            }
            if (format == 15)
                BoneDatabase.AppendChild(Skeletons);
            System.doc.AppendChild(BoneDatabase);

            XmlWriterSettings settings = new XmlWriterSettings
            {
                Encoding = Encoding.UTF8,
                NewLineChars = "\n",
                Indent = true,
                IndentChars = " "
            };
            XmlWriter writer = XmlWriter.Create(file + ".xml", settings);
            System.doc.Save(writer);
            writer.Close();
        }

        public struct Bone
        {
            public List<string> Names;
            public List<long> Offsets;
        }

        public struct BONE
        {
            public long SkeletonsOffset;
            public long SkeletonNamesOffset;
            public System.POF0 POF0;
            public System.Header Header;
            public List<long> SkeletonEntryOffset;
            public List<SkeletonEntry> Skeleton;
        }

        public struct BoneEntry
        {
            public bool HasParent;
            public byte Field01;
            public byte Field02;
            public byte PairNameIndex;
            public byte ParentNameIndex;
            public string Name;
            public Field00 Field00;
        }

        public struct Skeleton
        {
            public long BonesOffset;
            public long PositionCount;
            public long Field02Offset;
            public long BoneName1Count;
            public long BoneName2Count;
            public long PositionsOffset;
            public long BoneNames1Offset;
            public long BoneNames2Offset;
            public long ParentIndicesOffset;
            public long[] BoneNameOffsets;
            public long[] BoneName1Offsets;
            public long[] BoneName2Offsets;
            public List<long> BoneNamesOffsets;
            public List<long> BoneNames1Offsets;
            public List<long> BoneNames2Offsets;
        }

        public struct SkeletonEntry
        {
            public long Field02;
            public string Name;
            public List<short> ParentIndices;
            public List<string> BoneNames1;
            public List<string> BoneNames2;
            public List<Vector3> Positions;
            public Skeleton Skeleton;
            public List<BoneEntry> Bones;
        }
        
        public enum Field00
        {
            Flag01 = 0b001,
            Flag02 = 0b010,
            Flag04 = 0b100,
        }
    }
}