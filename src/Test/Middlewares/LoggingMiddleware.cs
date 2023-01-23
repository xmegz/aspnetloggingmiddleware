using Microsoft.AspNetCore.Http;
using Microsoft.IO;
using System;
using System.IO;
using System.IO.Pipelines;
using System.Text;
using System.Threading.Tasks;

namespace AspNetLoggingMiddleware.Middlewares
{
    public sealed class LoggingMiddleware
    {
        private readonly RequestDelegate _next;
        private readonly RecyclableMemoryStreamManager _recyclableMemoryStreamManager;

        public LoggingMiddleware(RequestDelegate next)
        {
            this._next = next;
            this._recyclableMemoryStreamManager = new RecyclableMemoryStreamManager();
        }

        private async Task _appendRequestBody(HttpContext context, StringBuilder sb)
        {
            var request = context.Request;

            if ((request.Body.CanRead) && (request.ContentLength > 0))
            {
                //Body buffered and context.Request.Body stream changed at this point
                context.Request.EnableBuffering();

                try
                {
                    while (true)
                    {
                        var reader = context.Request.BodyReader;

                        ReadResult readResult = await reader.ReadAsync();
                        var buffer = readResult.Buffer;

                        if (readResult.IsCanceled)
                            break;

                        if ((readResult.IsCompleted) && (buffer.Length > 0))
                            sb.AddReadOnlySequenceUtf8String(buffer);

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

        private async Task _logRequest(HttpContext context)
        {
            var sb = new StringBuilder(256);
            var request = context.Request;

            sb.Append("req: ");
            sb.Append(request.Scheme);
            sb.Append(" ");
            sb.Append(request.Host);
            sb.Append(request.Path);
            sb.Append(request.QueryString);
            sb.Append(" ");
            await _appendRequestBody(context, sb);

            Console.WriteLine(sb.ToString());
        }

        private async Task<string> _getResponseBody(HttpContext context)
        {
            var response = context.Response;
            var body = response?.Body;

            string bodyAsString = null;

            if ((body.CanRead) && (body.Length > 0) && (body.Length < (100 * 1024)))
            {
                bodyAsString = await new StreamReader(body, Encoding.UTF8).ReadToEndAsync();
            }

            return bodyAsString;
        }

        private async Task _logResponse(HttpContext context)
        {
            var request = context.Request;
            var body = await this._getResponseBody(context);

            Console.WriteLine($"resp: {request.Scheme} {request.Host}{request.Path} {request.QueryString} {body?.Length} {body}");
        }

        public async Task InvokeAsync(HttpContext context)
        {
            await this._logRequest(context);

            await _next(context);

            /*
            var originalBodyStream = context.Response.Body;

            //using (var responseBody = new MemoryStream())
            using (var responseBody = this._recyclableMemoryStreamManager.GetStream())
            {
                context.Response.Body = responseBody;

                await _next(context);

                responseBody.Seek(0, SeekOrigin.Begin);
                await this._logResponse(context);
                responseBody.Seek(0, SeekOrigin.Begin);

                await responseBody.CopyToAsync(originalBodyStream);
            }
            */
        }
    }
}