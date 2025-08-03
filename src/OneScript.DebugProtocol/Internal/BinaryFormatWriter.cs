/*----------------------------------------------------------
This Source Code Form is subject to the terms of the
Mozilla Public License, v.2.0. If a copy of the MPL
was not distributed with this file, You can obtain one
at http://mozilla.org/MPL/2.0/.
----------------------------------------------------------*/

using System;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;

namespace OneScript.DebugProtocol.Internal
{
    internal class BinaryFormatWriter : IDisposable
    {
        private readonly BinaryWriter _writer;
        
        public BinaryFormatWriter(Stream target)
        {
            _writer = new BinaryWriter(target, Encoding.UTF8, leaveOpen: true);
        }

        public void WriteHeader(int rootId, int headerId)
        {
            WriteRecordType(0);
            _writer.Write(rootId);
            _writer.Write(headerId);
            _writer.Write(1);
            _writer.Write(0);
        }

        public void WriteStringRecord(int objectId, string data)
        {
            WriteRecordType(6);
            _writer.Write(objectId);
            
            WriteStringValue(data);
        }

        private void WriteStringValue(string data)
        {
            var bytes = Encoding.UTF8.GetBytes(data);
            if (bytes.Length > 127)
                throw new ArgumentOutOfRangeException(nameof(data), "Length encoding not supported for strings more than 127 bytes");

            _writer.Write((byte)bytes.Length);
            _writer.Write(bytes, 0, bytes.Length);
        }

        public void WriteLibrary(Assembly assembly, int libraryId)
        {
            WriteRecordType(12);
            _writer.Write(libraryId);
            WriteStringValue(assembly.GetName().Name);
        }

        public void WriteClassWithNoFields(Type type, int classId, int libraryId)
        {
            WriteRecordType(5);
            
            _writer.Write(classId);
            WriteStringValue(type.FullName);
            _writer.Write(0); // properties count
            
            _writer.Write(libraryId);
        }

        private void WriteObjectArray(int objectId, object[] arr)
        {
            if (arr.Length != 0)
                throw new ArgumentException("Only empty arrays mapping supported");
            
            WriteRecordType(16);
            _writer.Write(objectId);
            _writer.Write(0);
        }

        private void WriteNull()
        {
            WriteRecordType(10);
        }

        public void WriteEnd()
        {
            WriteRecordType(11);
        }

        public void Close()
        {
            Dispose();
        }

        private void WriteRecordType(byte type)
        {
            _writer.Write(type);
        }

        public void Dispose()
        {
            _writer.Flush();
            _writer.Dispose();
        }
    }
}