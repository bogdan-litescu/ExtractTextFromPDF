using System;
using System.Collections.Generic;
using System.Text;
using System.IO;

namespace TestPdf
{
    public class PdfReader : IDisposable
    {
        Stream _Stream;
        byte[] _Buffer;
        int _CurrentIndex = 0;
        int _BufferSize;
        public const int BufferMaxSize = 1024;


        public PdfReader(Stream s)
        {
            _Stream = s;
            _Buffer = new byte[BufferMaxSize];

            ReadBuffer();
        }

        ~PdfReader()
        {
            if (_Stream != null)
                _Stream.Dispose();
        }

        public void Dispose()
        {
            if (_Stream != null)
                _Stream.Dispose();
        }

        public void Close()
        {
            if (_Stream != null)
                _Stream.Close();
        }


        public PdfObject ReadObject()
        {
            try {
                SkipNonObjects();

                // next line tells info about object
                string objectInfo = ReadLine(true);

                if (objectInfo.IndexOf("/Filter/FlateDecode/Length") == -1 || objectInfo.IndexOf("/Subtype/") > 0 || objectInfo.IndexOf("/Type/") > 0) {
                    return new DummyPdfObject();
                }

                long iObjectStart = _Stream.Position - _BufferSize + _CurrentIndex;
                string line;
                while ((line = ReadLine(true)) != "endobj") { }

                long iObjectEnd = _Stream.Position - _BufferSize + _CurrentIndex - 7/* "endobj".Length */;

                byte[] objBuf = new byte[iObjectEnd - iObjectStart];

                long streamPos = _Stream.Position;
                _Stream.Seek(iObjectStart, SeekOrigin.Begin);
                _Stream.Read(objBuf, 0, (int)(iObjectEnd - iObjectStart));

                // prepare for next object
                _Stream.Position = streamPos;

                //PdfObject pdfObj = new PdfObject(objBuf);
                // TODO: somefactory pattern here
                if (objectInfo.IndexOf("Filter/FlateDecode") != -1) {
                    return new PdfFlateDecodeObject(objectInfo, objBuf);
                } else {
                    return new DummyPdfObject();
                }

            } catch (EndOfStreamException) {
                return null;
            }
        }

        void SkipNonObjects()
        {
            string line = ReadLine(false);
            while (line.IndexOf(" obj\r") == -1) {
                line = ReadLine(false);
            }
        }

        void ReadBuffer()
        {
            _BufferSize = _Stream.Read(_Buffer, 0, BufferMaxSize);
            if (_BufferSize == 0) // end of stream
                throw new EndOfStreamException();
            _CurrentIndex = 0;
        }

        void AdvanceIndex()
        {
            _CurrentIndex++;
            if (_CurrentIndex >= _BufferSize) {
                ReadBuffer();
            }
        }

        void SkipLine()
        {
            // skip all characters until new line or carriage return
            while (_Buffer[_CurrentIndex] != '\r' && _Buffer[_CurrentIndex] != '\n')
                AdvanceIndex();

            // also skip \r and \n characters
            if (_Buffer[_CurrentIndex] == '\r')
                AdvanceIndex();

            if (_Buffer[_CurrentIndex] == '\n')
                AdvanceIndex();
        }

        string ReadLine(bool bTrimNewLine)
        {
            string line = "";

            // skip all characters until new line or carriage return
            while (_Buffer[_CurrentIndex] != '\r' && _Buffer[_CurrentIndex] != '\n') {
                line += (char) _Buffer[_CurrentIndex];
                AdvanceIndex();
            }

            // also skip \r and \n characters
            if (_Buffer[_CurrentIndex] == '\r') {
                if (!bTrimNewLine)
                    line += (char)_Buffer[_CurrentIndex];
                AdvanceIndex();
            }

            if (_Buffer[_CurrentIndex] == '\n') {
                if (!bTrimNewLine)
                    line += (char)_Buffer[_CurrentIndex];
                AdvanceIndex();
            }

            return line;
        }
    }
}
