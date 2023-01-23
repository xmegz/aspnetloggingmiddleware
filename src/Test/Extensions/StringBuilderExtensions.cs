using System.Buffers;
using System.Text;
using System;
using System.Diagnostics;

namespace AspNetLoggingMiddleware
{
    public static class StringBuilderExtensions
    {
        public static void AddReadOnlySequenceUtf8String(this StringBuilder self, in ReadOnlySequence<byte> readOnlySequence)
        {
            int byteCount = (int)readOnlySequence.Length;
            System.Diagnostics.Debug.WriteLine($"byteCount:{byteCount}");

            Span<byte> byteSpan = (byteCount < 64*1024) ? stackalloc byte[byteCount] : new byte[byteCount];

            readOnlySequence.CopyTo(byteSpan);

            var decoder = Encoding.UTF8.GetDecoder();
            int charCount = decoder.GetCharCount(byteSpan, true);

            System.Diagnostics.Debug.WriteLine($"charCount:{charCount}");

            Span<char> charSpan = (byteCount < 64 * 1024) ? stackalloc char[charCount] : new char[charCount];

            decoder.GetChars(byteSpan, charSpan, false);

            self.Append(charSpan);
        }
    }
}
