using System;
using System.Collections.Generic;
using System.Text;
using System.IO;
using System.Text.RegularExpressions;

namespace TestPdf
{

    public class PdfFlateDecodeObject: PdfObject
    {
        string _Contents = "";

        public PdfFlateDecodeObject(string objHeader, byte[] data)
        {
            int offset = 0;

            if (objHeader.IndexOf("stream") != objHeader.Length - "stream".Length) {
                // skip stream line 
                ReadLine(data, ref offset);
            }

            // decode stream
            MemoryStream ms = new MemoryStream(data, offset + 2, data.Length - offset - 6); // 22 is the endstream and endobj lines
            //Console.WriteLine(new StreamReader(ms).ReadToEnd());
            Console.WriteLine(objHeader);
            string content = Decode(ms);

            if (content.IndexOf("BT") != -1) {
            StringBuilder sbContent = new StringBuilder();
                foreach (Match m in Regex.Matches(content, "BT[^\\[]+\\[([^\\]]+)\\]", RegexOptions.Singleline | RegexOptions.Multiline)) {
                    if (m.Groups.Count > 0 && m.Groups[1].Value.IndexOf('(') != -1) {
                        string c = Regex.Replace(m.Groups[1].Value, "[^\\)]*\\(([^\\)]+)\\)[^\\(]*", "$1");
                        c = Regex.Replace(c, "<[A-Fa-f0-9]+>", "");
                        //foreach (Match mHexa in Regex.Matches(c, "<[A-Fa-f0-9]+>")) {
                        //    // convert it back to characters
                        //    c = c.Replace(mHexa.Value, HexToChars(mHexa.Value.Trim('<', '>')));
                        //}
                        //sbContent.Append(m.Groups[1].Value);
                        //sbContent.Append("\n");
                        sbContent.Append(c);
                        sbContent.Append("\n");
                    }
                }
                
                _Contents = sbContent.ToString();
            }
        }

        string HexToChars(string hex)
        {
            return ""; // TODO
            if (hex.Length % 2 == 1) {
                hex += "0";
            }

            int charSize = 2;
            if (hex.IndexOf("00") == 0)
                charSize = 4; // unicode

            StringBuilder sbRes = new StringBuilder();
            for (int i = 0; i < hex.Length; i += charSize) {
                sbRes.Append((char)Convert.ToInt32(hex.Substring(i, charSize), 16));
            }
            return sbRes.ToString();
        }

        string ReadLine(byte[] data, ref int offset)
        {
            string line = "";

            // skip all characters until new line or carriage return
            while (data[offset] != '\r' && data[offset] != '\n') {
                line += (char)data[offset];
                offset++;
            }

            // also skip \r and \n characters
            if (data.Length != offset && data[offset] == '\r')
                offset++;

            if (data.Length != offset && data[offset] == '\n')
                offset++;

            return line;
        }

        string Decode(Stream s)
        {
            // skip first two bytes
            //Console.WriteLine(s.Length);
            //s.ReadByte();
            //s.ReadByte();

            using (System.IO.Compression.DeflateStream ds = new System.IO.Compression.DeflateStream(s, System.IO.Compression.CompressionMode.Decompress)) {
                //ds.Flush();
                //byte[] b = new byte[100];
//                int a = ds.Read(b, 0, 100);
                //Console.WriteLine(a);
                return new StreamReader(ds).ReadToEnd();
                //Console.WriteLine(r);
            }
        }

        public override string ToString()
        {
            return _Contents + "\n\n";
        }
    }
}
