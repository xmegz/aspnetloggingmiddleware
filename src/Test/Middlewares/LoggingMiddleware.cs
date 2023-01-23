using Microsoft.AspNetCore.Http;
using Microsoft.IO;
using System;
using System.Diagnostics;
using System.IO;
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


        private async Task LogRequest(HttpContext context)
        {
            var sb = new StringBuilder(256);
            var request = context.Request;

            sb.Append("req :");
            sb.Append(request.Scheme);
            sb.Append(" ");
            sb.Append(request.Host);
            sb.Append(request.Path);
            sb.Append(request.QueryString);
            sb.Append(" ");
            await sb.AppendRequestBody(context, 64 * 1024);

            Console.WriteLine(sb.ToString());
        }

        private async Task LogResponse(HttpContext context, Stopwatch stopwatch)
        {
            var sb = new StringBuilder(256);
            var request = context.Request;

            sb.Append("resp :");
            sb.Append(request.Scheme);
            sb.Append(" ");
            sb.Append(request.Host);
            sb.Append(request.Path);
            sb.Append(request.QueryString);
            sb.Append(" ");
            await sb.AppendResponseBody(context, 64 * 1024);

            stopwatch.Stop();
            sb.Append(" Time: ");
            sb.Append(stopwatch.ElapsedMilliseconds);

            Console.WriteLine(sb.ToString());
        }

        public async Task InvokeAsync(HttpContext context)
        {
            Stopwatch stopwath = Stopwatch.StartNew();

            await this.LogRequest(context);

            //await _next(context);

            
            var originalBodyStream = context.Response.Body;

            using (var responseBody = this._recyclableMemoryStreamManager.GetStream())
            {
                context.Response.Body = responseBody;

                await _next(context);

                //responseBody.Seek(0, SeekOrigin.Begin);
                await this.LogResponse(context, stopwath);
                //responseBody.Seek(0, SeekOrigin.Begin);

                await responseBody.CopyToAsync(originalBodyStream);
            }
            

        }
    }
}