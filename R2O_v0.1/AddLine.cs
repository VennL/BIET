using System.IO;
using System.Text;

namespace R2O
{
    public class AddLine
    {
        public static void AddText(FileStream fs, string value)
        {
            byte[] info = new UTF8Encoding(true).GetBytes(value);
            fs.Write(info, 0, info.Length);

        }// A Method to Write Text into TXT File in Newline
        public static void AddTextLine(FileStream fs, string value)
        {
            byte[] info = new UTF8Encoding(true).GetBytes(value + "\n");
            fs.Write(info, 0, info.Length);
        }
    }
    
}
