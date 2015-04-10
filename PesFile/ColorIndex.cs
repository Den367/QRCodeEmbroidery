using System.Drawing;

namespace EmbroideryFile
{
    /// <summary>
    /// Color numbers for PES file embroidery format
    /// </summary>
    public static class ColorIndex
    {
       
        public static Color ColorByIndex(int index)
        {
            Color retval;
            switch (index)
            {
                case 1:
                    retval = Color.FromArgb(14, 31, 124);
                    break;
                case 2:
                    retval = Color.FromArgb(10, 85, 163);
                   
                    break;
                case 3:
                    retval = Color.FromArgb(48, 135, 119);
                    break;
                case 4:
                    retval = Color.FromArgb(75, 107, 175);
                    break;
                case 5:
                    retval = Color.FromArgb(237, 23, 31);
                    break;
                case 6:
                    retval = Color.FromArgb(209, 92, 0);
                    break;
                case 7:
                    retval = Color.FromArgb(145, 54, 151);
                    break;
                case 8:
                    retval = Color.FromArgb(228, 154, 203);
                    break;
                case 9:
                    retval = Color.FromArgb(145, 95, 172);
                    break;
                case 10:
                    retval = Color.FromArgb(157, 214, 125);
                    break;
                case 11:
                    retval = Color.FromArgb(232, 169, 0);
                    break;
                case 12:
                    retval = Color.FromArgb(254, 186, 53);
                    break;
                case 13:
                    retval = Color.FromArgb(255, 255, 0);
                    break;
                case 14:
                    retval = Color.FromArgb(112, 188, 31);
                    break;
                case 15:
                    retval = Color.FromArgb(186, 152, 0);
                    break;
                case 16:
                    retval = Color.FromArgb(168, 168, 168);
                    break;
                case 17:
                    retval = Color.FromArgb(123, 111, 0);
                    break;
                case 18:
                    retval = Color.FromArgb(255, 255, 179);
                    break;
                case 19:
                    retval = Color.FromArgb(79, 85, 86);
                    break;
                case 20:
                    retval = Color.FromArgb(0, 0, 0);
                    break;
                case 21:
                    retval = Color.FromArgb(11, 61, 145);
                    break;
                case 22:
                    retval = Color.FromArgb(119, 1, 118);
                    break;
                case 23:
                    retval = Color.FromArgb(41, 49, 51);
                    break;
                case 24:
                    retval = Color.FromArgb(42, 19, 1);
                    break;
                case 25:
                    retval = Color.FromArgb(246, 74, 138);
                    break;
                case 26:
                    retval = Color.FromArgb(178, 118, 36);
                    break;
                case 27:
                    retval = Color.FromArgb(252, 187, 196);
                    break;
                case 28:
                    retval = Color.FromArgb(254, 55, 15);
                    break;
                case 29:
                    retval = Color.FromArgb(240, 240, 240);
                    break;
                case 30:
                    retval = Color.FromArgb(106, 28, 138);
                    break;
                case 31:
                    retval = Color.FromArgb(168, 221, 196);
                    break;
                case 32:
                    retval = Color.FromArgb(37, 132, 187);
                    break;
                case 33:
                    retval = Color.FromArgb(254, 179, 67);
                    break;
                case 34:
                    retval = Color.FromArgb(255, 240, 141);
                    break;
                case 35:
                    retval = Color.FromArgb(208, 166, 96);
                    break;
                case 36:
                    retval = Color.FromArgb(209, 84, 0);
                    break;
                case 37:
                    retval = Color.FromArgb(102, 186, 73);
                    break;
                case 38:
                    retval = Color.FromArgb(19, 74, 70);
                    break;
                case 39:
                    retval = Color.FromArgb(135, 135, 135);
                    break;
                case 40:
                    retval = Color.FromArgb(216, 202, 198);
                    break;
                case 41:
                    retval = Color.FromArgb(67, 86, 7);
                    break;
                case 42:
                    retval = Color.FromArgb(254, 227, 197);
                    break;
                case 43:
                    retval = Color.FromArgb(249, 147, 188);
                    break;
                case 44:
                    retval = Color.FromArgb(0, 56, 34);
                    break;
                case 45:
                    retval = Color.FromArgb(178, 175, 212);
                    break;
                case 46:
                    retval = Color.FromArgb(104, 106, 176);
                    break;
                case 47:
                    retval = Color.FromArgb(239, 227, 185);
                    break;
                case 48:
                    retval = Color.FromArgb(247, 56, 102);
                    break;
                case 49:
                    retval = Color.FromArgb(181, 76, 100);
                    break;
                case 50:
                    retval = Color.FromArgb(19, 43, 26);
                    break;
                case 51:
                    retval = Color.FromArgb(199, 1, 85);
                    break;
                case 52:
                    retval = Color.FromArgb(254, 158, 50);
                    break;
                case 53:
                    retval = Color.FromArgb(168, 222, 235);
                    break;
                case 54:
                    retval = Color.FromArgb(0, 103, 26);
                    break;
                case 55:
                    retval = Color.FromArgb(78, 41, 144);
                    break;
                case 56:
                    retval = Color.FromArgb(47, 126, 32);
                    break;
                case 57:
                    retval = Color.FromArgb(253, 217, 222);
                    break;
                case 58:
                    retval = Color.FromArgb(255, 217, 17);
                    break;
                case 59:
                    retval = Color.FromArgb(9, 91, 166);
                    break;
                case 60:
                    retval = Color.FromArgb(240, 249, 112);
                    break;
                case 61:
                    retval = Color.FromArgb(227, 243, 91);
                    break;
                case 62:
                    retval = Color.FromArgb(255, 200, 100);
                    break;
                case 63:
                    retval = Color.FromArgb(255, 200, 150);
                    break;
                case 64:
                    retval = Color.FromArgb(255, 200, 200);
                    break;
                default:
                    retval = Color.White;
                   
                    break;
            }
            return retval;
        }
    }
}
