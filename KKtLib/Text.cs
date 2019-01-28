using System;
using System.Text;

namespace KKtLib
{
    public class Text
    {
        public static string ToASCII(byte[] Array) => Encoding.ASCII.GetString(Array);
        public static string ToUTF8 (byte[] Array) => Encoding.UTF8 .GetString(Array);
        public static byte[] ToASCII(string Data ) => Encoding.ASCII.GetBytes (Data );
        public static byte[] ToUTF8 (string Data ) => Encoding.UTF8 .GetBytes (Data );
        public static byte[] ToASCII(char[] Data ) => Encoding.ASCII.GetBytes (Data );
        public static byte[] ToUTF8 (char[] Data ) => Encoding.UTF8 .GetBytes (Data );
        
        /// <param name="lang">lang = 0 or anything other - English; lang = 1 - Russian</param>
        public static void SprTool(int code, byte lang)
        {
            string text = "";
            switch (lang)
            {
                case 1:
                    switch (code)
                    {
                        case 10:
                            text += ("Выберите тип компрессии входного файла:\n");
                            text += ("    1) DXT1 (BC1);\n    2) DXT5 (BC3);\n    3) ATI2 (BC5).");
                            break;
                        case 20:
                            text += ("Названия входных файлов должны быть такими:");
                            break;
                        case 30:
                            text += ("img[id]_mip[id]_[ширина]_[высота].bin\n");
                            text += ("    Прим.: img01_mip1_1024_256.bin - Хранит каналы Y и A\n");
                            text += ("    Прим.: img01_mip2_512_128.bin - Хранит каналы Cb и Cr.");
                            break;
                        case 31:
                            text += ("img[id]_[ширина]_[высота].bin\n");
                            text += ("    Прим.: img01_1024_256.bin");
                            break;
                        case 32:
                            text += ("img[id]_calculated_final.bmp и img[id]_calculated_final.png\n");
                            text += ("    Прим.: img01_calculated_final.bmp и img01_calculated_final.png");
                            break;
                        case 40:
                            text += ("Введите img[id]:");
                            break;
                        case 41:
                            text += ("Введите [ширину]:");
                            break;
                        case 42:
                            text += ("Введите [высоту]:");
                            break;
                        case 50:
                            text += ("Входная текстура не существует!!\n");
                            text += ("Программа завершит свою работу...");
                            break;
                        case 60:
                            text += ("Входная текстура каналов YA не существует!!\n");
                            text += ("Программа завершит свою работу...");
                            break;
                        case 61:
                            text += ("Входная текстура каналов CbCr не существует!!\n");
                            text += ("Программа завершит свою работу...");
                            break;
                        case 62:
                            text += ("Чтение входных YA и CbCr текстур...");
                            break;
                        case 63:
                            text += ("Сохранение YA и CbCr текстур в .bmp ...");
                            break;
                        case 64:
                            text += ("Теперь пытаемся просчитать оригинальное изображение...");
                            break;
                        case 65:
                            text += ("Сохранение просчитанного изображения в .bmp ...");
                            break;
                        case 70:
                            text += ("Не забудьте проверить просчитанный файл" +
                                "\n    и файлы буфера (если они были созданы)!!");
                            break;
                        case 80:
                            text += ("Имеется несовпадение между шириной и высотой" +
                                "\n    и размером входной текстуры!");
                            break;
                        case 81:
                            text += ("Имеется несовпадение между шириной и высотой" +
                                "\n    и размером входной YA текстуры!");
                            break;
                        case 82:
                            text += ("Имеется несовпадение между шириной и высотой" +
                                "\n    и размером входной CbCr текстуры!");
                            break;
                    }
                    break;
                default:
                    switch (code)
                    {
                        case 10:
                            text += ("Choose input Compression format of Texture:\n");
                            text += ("    1) DXT1 (BC1);\n    2) DXT5 (BC3);\n    3) ATI2 (BC5).");
                            break;
                        case 20:
                            text += ("Naming of input files should be right this:");
                            break;
                        case 30:
                            text += ("img[id]_mip[id]_[width]_[height].bin\n");
                            text += ("    Ex.: img01_mip1_1024_256.bin - Stores Y and A channels.\n");
                            text += ("    Ex.: img01_mip2_512_128.bin - Stores Cb and Cr channes.");
                            break;
                        case 31:
                            text += ("img[id]_[width]_[height].bin");
                            text += ("    Ex.: img01_1024_256.bin");
                            break;
                        case 32:
                            text += ("img[id]_calculated_final.bmp and img[id]_calculated_final.png\n");
                            text += ("    Ex.: img01_calculated_final.bmp and img01_calculated_final.png");
                            break;
                        case 40:
                            text += ("Enter img[id]:");
                            break;
                        case 41:
                            text += ("Enter [width]:");
                            break;
                        case 42:
                            text += ("Enter [height]:");
                            break;
                        case 50:
                            text += ("Input texture doesn't exist!!");
                            break;
                        case 60:
                            text += ("Input YA texture doesn't exist!!");
                            break;
                        case 61:
                            text += ("Input CbCr texture doesn't exist!!");
                            break;
                        case 62:
                            text += ("Reading input YA and CbCr textures...");
                            break;
                        case 63:
                            text += ("Saving YA and CbCr textures to a bitmap...");
                            break;
                        case 64:
                            text += ("Now attempting to calculate the original image...");
                            break;
                        case 65:
                            text += ("Saving calculated image to a bitmap...");
                            break;
                        case 70:
                            text += ("Remember to inspect the output and " +
                                "dump files (if them was created)!!");
                            break;
                        case 80:
                            text += ("There is a discrepancy between the width and height" +
                                "\n    and the size of the input texture!");
                            break;
                        case 81:
                            text += ("There is a discrepancy between the width and height" +
                                "\n    and the size of the input YA texture!");
                            break;
                        case 82:
                            text += ("There is a discrepancy between the width and height" +
                                "\n    and the size of the input CbCr texture!");
                            break;
                    }
                    break;
            }
            Console.WriteLine(text);
        }
    }
}
