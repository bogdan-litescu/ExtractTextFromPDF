using System;
using System.Collections.Generic;
using System.Text;
using System.IO;

namespace TestPdf
{
    public enum ePdfObjectType
    {
        Unknown,
        FlateDecode
    }

    public class PdfObject
    {
        public PdfObject()
        {
        }

        public PdfObject(byte[] data)
        {
            int offset = 0;

            // skip indirect object definition line
            ReadLine(data, ref offset);

            // check object type (2nd line)
            string objType = ReadLine(data, ref offset);
            if (objType.IndexOf("<</Filter/FlateDecode/") != -1) {

                if (objType.IndexOf("stream") == -1) {
                    // skip stream line 
                    ReadLine(data, ref offset);
                }

                // decode stream
                MemoryStream ms = new MemoryStream(data, offset + 2, data.Length - offset - 22); // 22 is the endstream and endobj lines
                //Console.WriteLine(new StreamReader(ms).ReadToEnd());
                Decode(ms);
            }
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

        void Decode(Stream s)
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
                string r = new StreamReader(ds).ReadToEnd();
                Console.WriteLine(r);
            }
        }
    }
}
