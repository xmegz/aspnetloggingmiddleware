using System.Buffers;
using System.Text;
using System;
using System.Diagnostics;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using System.IO.Pipelines;
using System.IO;
using System.Linq;
using Microsoft.IO;
using System.Collections.Generic;

namespace AspNetLoggingMiddleware
{
    public static class StringBuilderExtensions
    {
        public static void AddReadOnlySequenceUtf8String(this StringBuilder self, in ReadOnlySequence<byte> readOnlySequence)
        {
            int byteCount = (int)readOnlySequence.Length;

            System.Diagnostics.Debug.WriteLine($"byteCount:{byteCount}");

            Span<byte> byteSpan = (byteCount <= (65 * 1024)) ? stackalloc byte[byteCount] : new byte[byteCount];
            readOnlySequence.CopyTo(byteSpan);

            var decoder = Encoding.UTF8.GetDecoder();
            int charCount = decoder.GetCharCount(byteSpan, true);

            System.Diagnostics.Debug.WriteLine($"charCount:{charCount}");

            Span<char> charSpan = (byteCount <= (65 * 1024)) ? stackalloc char[charCount] : new char[charCount];

            decoder.GetChars(byteSpan, charSpan, false);
         
            self.Append(charSpan);          
        }


        public static void AddByteArrayAsUtf8String(this StringBuilder self, byte[] bytes)
        {
            int byteCount = bytes.Length;

            System.Diagnostics.Debug.WriteLine($"byteCount:{byteCount}");

            var decoder = Encoding.UTF8.GetDecoder();
            int charCount = decoder.GetCharCount(bytes, true);

            System.Diagnostics.Debug.WriteLine($"charCount:{charCount}");

            Span<char> charSpan = (byteCount <= (65 * 1024)) ? stackalloc char[charCount] : new char[charCount];

            decoder.GetChars(bytes, charSpan, false);
   
            self.Append(charSpan);
        }


        public static async Task AppendRequestBody(this StringBuilder self, HttpContext context, int limit)
        {
            var request = context.Request;

            if ((request.Body.CanRead) && (request.ContentLength.HasValue) && (request.ContentLength.Value > 0))
            {
                //Body buffered and context.Request.Body stream changed at this point
                context.Request.EnableBuffering();

                try
                {
                    while (true)
                    {
                        var reader = context.Request.BodyReader;

                        // First chunk
                        ReadResult readResult = await reader.ReadAsync();
                        var buffer = readResult.Buffer;

                        if (readResult.IsCanceled)
                            break;

                        if ((readResult.IsCompleted) && (buffer.Length > 0))
                        {
                            var limitedBuffer = (buffer.Length > limit) ? buffer.Slice(0, limit) : buffer;
                   
                            self.AppendLine();
                            self.AddReadOnlySequenceUtf8String(limitedBuffer);
                            
                            if (buffer.Length != limitedBuffer.Length)                            
                                self.AppendLine("--- LIMITED ----");
                            else
                                self.AppendLine();

                            self.Append("Length: ");
                            self.Append(buffer.Length);
                        }
                            

                        reader.AdvanceTo(buffer.Start, buffer.End);

                        if (readResult.IsCompleted)
                            break;

                        if (context.RequestAborted.IsCancellationRequested)
                            break;
                    }
                }
                finally
                {
                    request.Body.Position = 0;
                }
            }
        }

        /*
        var rb = body as RecyclableMemoryStream;
        rb.GetReadOnlySequence();
                     var limitedBuffer = (buffer.Length > limit) ? buffer.Slice(0, limit) : buffer;
        */


        public static async Task AppendResponseBody(this StringBuilder self, HttpContext context, int limit)
        {
            var response = context.Response;
            var body = response?.Body;


            if ((body.CanRead) && (body.Length > 0))
            {
                body.Seek(0, SeekOrigin.Begin);

                var rb = body as RecyclableMemoryStream;
                var buffer = rb.GetReadOnlySequence();
                var limitedBuffer = (buffer.Length > limit) ? buffer.Slice(0, limit) : buffer;

               
                self.AppendLine();
                self.AddReadOnlySequenceUtf8String(limitedBuffer);

                if (buffer.Length != limitedBuffer.Length)
                    self.AppendLine("--- LIMITED ----");
                else
                    self.AppendLine();

                self.Append("Length: ");
                self.Append(buffer.Length);


                body.Seek(0, SeekOrigin.Begin);

            }
        }
    














    public static async Task AppendResponseBodyOrig(this StringBuilder self, HttpContext context, int limit)
        {
            var response = context.Response;
            var body = response?.Body;

            string bodyAsString = null;

            if ((body.CanRead) && (body.Length > 0))
            {
                var bufferLength = (body.Length > limit) ? limit : body.Length;
                var pool = ArrayPool<byte>.Shared;

                byte[] buffer = pool.Rent((int)bufferLength);

                try
                {
                   


                    body.Seek(0, SeekOrigin.Begin);

                    await body.ReadAsync(buffer, 0, (int)bufferLength);

                    self.Append("Length: ");
                    self.Append(body.Length);
                    self.AppendLine();
                    self.AddByteArrayAsUtf8String(buffer);

                    if (body.Length != buffer.Length)
                        self.AppendLine("--- LIMITED ----");
                }
                finally
                {
                    pool.Return(buffer);

                    body.Seek(0, SeekOrigin.Begin);
                }
            }

            self.Append(bodyAsString);
        }

    }
}
