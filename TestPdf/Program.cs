using System;
using System.Collections.Generic;
using System.Text;
using System.IO;

namespace TestPdf
{
    class Program
    {
        static void Main(string[] args)
        {
            //using (FileStream fs = File.OpenRead("d:\\Tabs Pro - User Manual 1.3.pdf")) {
            //    BinaryReader br = new BinaryReader(fs);
            //    br.
            //    int iStart = FindStringInStream(fs, "");
            //}

            //using (PdfReader pdfReader = new PdfReader(File.OpenRead("d:\\Tabs Pro - User Manual 1.3.pdf"))) {
            using (PdfReader pdfReader = new PdfReader(File.OpenRead("d:\\PDF reference.pdf"))) {
                PdfObject pdfObj = pdfReader.ReadObject();
                File.WriteAllText("d:\\test-parse-pdf.txt", "");
                while (pdfObj != null) {
                    //Console.WriteLine(pdfObj.ToString());
                    File.AppendAllText("d:\\test-parse-pdf.txt", pdfObj.ToString());
                    pdfObj = pdfReader.ReadObject();
                }

                //for (int i = 0; i < 40; i++) {
                //    pdfReader.ReadObject();
                //    Console.WriteLine();
                //    Console.WriteLine();
                //    Console.WriteLine();
                //}

                //for (int i = 0; i < 2; i++) {
                //    pdfReader.ReadObject();
                //    Console.WriteLine();
                //}
            }
            return;
            using (StreamReader sr = new StreamReader(File.OpenRead("d:\\Tabs Pro - User Manual 1.3.pdf"))) {

                int iAbsIndex = 0;

                // skip PDF header
                string line = sr.ReadLine();
                iAbsIndex = line.Length;
                while (line[0] == '%') {
                    line = sr.ReadLine();
                }

                // read one object at a time
                bool moreObjects = true;
                while (moreObjects) {

                    sr.BaseStream.Flush();
                    long iObjStart = sr.BaseStream.Position;
                    while (line != "endobj") line = sr.ReadLine();
                    sr.BaseStream.Flush();
                    long iObjEnd = sr.BaseStream.Position - 6; // "endobj".Length;

                    byte[] buf = new byte[iObjEnd - iObjStart];
                    sr.BaseStream.Position = iObjStart;
                    sr.BaseStream.Read(buf, 0, (int)(iObjEnd - iObjStart));
                    Console.WriteLine();

                    moreObjects = false;
                }
            }
            return;

            using (StreamReader sr = new StreamReader(File.OpenRead("d:\\aaa.pdf"))) {
                
                
                Decode(sr.BaseStream);
                return;
                while (sr.Peek() >= 0) {
                    string line = sr.ReadLine();
                    if (line.IndexOf("<</Filter/FlateDecode/") == 0) {
                        line = sr.ReadLine(); // stream
                        
                        using (MemoryStream ms = new MemoryStream()) {
                            StreamWriter sw = new StreamWriter(ms);
                            while ((line = sr.ReadLine()) != "endstream") {
                                sw.Write(line);
                            }
                            sw.Flush();
                            ms.Flush();
                            Decode(ms);
                        }
                    }
                }
            }
        }

        static int FindStringInStream(Stream stream, string s)
        {
            return - 1;
        }

        static void Decode(Stream s)
        {
            // skip first two bytes
            Console.WriteLine(s.Length);
            s.ReadByte();
            s.ReadByte();

            using (System.IO.Compression.DeflateStream ds = new System.IO.Compression.DeflateStream(s, System.IO.Compression.CompressionMode.Decompress)) {
                ds.Flush();
                byte []b = new byte[100];
                int a = ds.Read(b, 0, 100);
                Console.WriteLine(a);
                string r = new StreamReader(ds).ReadToEnd();
                Console.WriteLine(r);
            }
        }
    }
}
