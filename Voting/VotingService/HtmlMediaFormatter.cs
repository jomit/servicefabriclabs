using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Formatting;
using System.Net.Http.Headers;
using System.Text;
using System.Threading.Tasks;

namespace VotingService
{
    public class HtmlMediaFormatter : BufferedMediaTypeFormatter
    {
        public HtmlMediaFormatter()
        {
            SupportedMediaTypes.Add(new MediaTypeHeaderValue("text/html"));
            SupportedEncodings.Add(new UTF8Encoding(encoderShouldEmitUTF8Identifier: false));
        }
        public override bool CanReadType(Type type) { return false; }
        public override bool CanWriteType(Type type) { return (typeof(string) == type) ? true : false; }
        public override void WriteToStream(Type type, object value, Stream writeStream, HttpContent content)
        {
            Encoding effectiveEncoding = SelectCharacterEncoding(content.Headers);
            using (var writer = new StreamWriter(writeStream, effectiveEncoding)) { writer.Write(value); }
        }
    }
}
