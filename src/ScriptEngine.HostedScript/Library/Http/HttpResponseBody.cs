/*----------------------------------------------------------
This Source Code Form is subject to the terms of the 
Mozilla Public License, v.2.0. If a copy of the MPL 
was not distributed with this file, You can obtain one 
at http://mozilla.org/MPL/2.0/.
----------------------------------------------------------*/
using System;
using System.IO;
using System.Net;
using System.IO.Compression;

namespace ScriptEngine.HostedScript.Library.Http
{
    class HttpResponseBody : IDisposable
    {
        private const int INMEMORY_BODY_LIMIT = 1024 * 1024 * 5; // 5 Mb
        private const int UNDEFINED_LENGTH = -1;
        private const int CHUNK_SIZE = 0x8000;

        string _backingFileName;
        bool _backFileIsTemp = false;
        byte[] _inMemBody;

        private readonly bool _autoDecompress; 
        private long _contentSize = 0;

        public HttpResponseBody(HttpWebResponse response, string dumpToFile)
        {
            _autoDecompress = string.Equals(response.ContentEncoding, "gzip", StringComparison.OrdinalIgnoreCase);
            _contentSize = _autoDecompress ? -1 : response.ContentLength;

            if (String.IsNullOrEmpty(dumpToFile))
            {
                InitInMemoryResponse(response);
            }
            else
            {
                InitFileBackedResponse(response, dumpToFile);
            }
        }

        private void InitInMemoryResponse(HttpWebResponse response)
        {
            if(_contentSize > INMEMORY_BODY_LIMIT)
            {
                var filename = Path.GetTempFileName();
                _backFileIsTemp = true;
                InitFileBackedResponse(response, filename);
            }
            else
            {
                if(_contentSize == UNDEFINED_LENGTH)
                {
                    ReadToStream(response);
                }
                else
                {
                    ReadToArray(response);
                }
            }
        }

        public bool AutoDecompress => _autoDecompress;

        public long ContentSize => _contentSize < 0 ? 0 : _contentSize;

        public Stream OpenReadStream()
        {
            if (_backingFileName != null)
            {
                return new FileStream(_backingFileName, FileMode.Open, FileAccess.Read);
            }
            else if (_inMemBody != null)
            {
                return new MemoryStream(_inMemBody);
            }
            else
                throw new InvalidOperationException("No response body");
        }

        private Stream GetResponseStream(HttpWebResponse response)
        {
            if (_autoDecompress)
                return new GZipStream(response.GetResponseStream(), CompressionMode.Decompress);
            return response.GetResponseStream();
        }
        
        private void ReadToStream(HttpWebResponse response)
        {
            using (var responseStream = GetResponseStream(response))
            using(var ms = new MemoryStream())
            {
                bool memStreamIsAlive = true;

                int readTotal = 0;
                byte[] buffer = new byte[CHUNK_SIZE];
                while (true)
                {
                    var bytesRead = responseStream.Read(buffer, 0, CHUNK_SIZE);
                    if (bytesRead == 0)
                        break;

                    ms.Write(buffer, 0, bytesRead);

                    readTotal += bytesRead;

                    if(readTotal > INMEMORY_BODY_LIMIT)
                    {
                        var filename = Path.GetTempFileName();
                        _backFileIsTemp = true;
                        _backingFileName = filename;
                        
                        ms.Position = 0;
                        using (var file = new FileStream(filename, FileMode.Create))
                        {
                            StreamToStreamCopy(ms, file);
                            ms.Dispose();
                            memStreamIsAlive = false;
                            StreamToStreamCopy(responseStream, file);
                        }

                        break;

                    }
                }

                if(memStreamIsAlive)
                {
                    _inMemBody = new byte[ms.Length];
                    ms.Position = 0;
                    ms.Read(_inMemBody, 0, _inMemBody.Length);
                }

                if (_autoDecompress)
                    _contentSize = readTotal;
            }
        }

        private void ReadToArray(HttpWebResponse response)
        {
            System.Diagnostics.Debug.Assert(_contentSize <= INMEMORY_BODY_LIMIT);

            using (var stream = GetResponseStream(response))
            {
                var mustRead = (int)_contentSize;
                _inMemBody = new byte[mustRead];
                int offset = 0;

                while (mustRead > 0)
                {
                    int portion = Math.Min(CHUNK_SIZE, (int)mustRead);
                    var read = stream.Read(_inMemBody, offset, portion);

                    if (read == 0)
                        break;

                    mustRead -= read;
                    offset += read;
                }
            }
        }

        private void InitFileBackedResponse(HttpWebResponse response, string backingFileName)
        {
            _backingFileName = backingFileName;
            using(var responseStream = GetResponseStream(response))
            {
                using(var file = new FileStream(backingFileName, FileMode.Create))
                {
                    StreamToStreamCopy(responseStream, file);
                }
            }
        }

        private static void StreamToStreamCopy(Stream responseStream, Stream acceptor)
        {
            byte[] buffer = new byte[CHUNK_SIZE];
            while (true)
            {
                var bytesRead = responseStream.Read(buffer, 0, CHUNK_SIZE);
                if (bytesRead == 0)
                    break;

                acceptor.Write(buffer, 0, bytesRead);
            }
        }

        private void Dispose(bool manualDispose)
        {
            if(manualDispose)
            {
                GC.SuppressFinalize(this);
                _inMemBody = null;
            }

            KillTemporaryFile();
        }

        private void KillTemporaryFile()
        {
            if(_backFileIsTemp && _backingFileName != null)
            {
                if(File.Exists(_backingFileName))
                {
                    try
                    {
                        File.Delete(_backingFileName);
                    }
                    catch
                    {
                        // нипавезло :(
                    }
                }
            }
        }

        public void Dispose()
        {
            Dispose(true);
        }

        ~HttpResponseBody()
        {
            Dispose(false);
        }
    }
}
