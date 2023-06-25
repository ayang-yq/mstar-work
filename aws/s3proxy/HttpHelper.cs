using System;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Text;

namespace MorningstarAWD.Common.Amazon
{
    public class HttpHelper
    {
        /// <summary>
        /// Makes a http request to the specified endpoint
        /// </summary>
        /// <param name="endpointUri"></param>
        /// <param name="httpMethod"></param>
        /// <param name="headers"></param>
        /// <param name="requestBody"></param>
        public static byte[] GetReponseData(AWDEnvironment env,
                                            Uri endpointUri,
                                            string httpMethod,
                                            IDictionary<string, string> headers,
                                            Tuple<long, long> rangeHeader,
                                            string requestBody)
        {
            byte[] res = null;
            var request = ConstructWebRequest(endpointUri, httpMethod, headers, rangeHeader);

            if (!string.IsNullOrEmpty(requestBody))
            {
                byte[] data = Encoding.UTF8.GetBytes(requestBody);
                using (var stream = request.GetRequestStream())
                {
                    stream.Write(data, 0, data.Length);
                }
            }

            using (var response = (HttpWebResponse)request.GetResponse())
            {
                using (Stream os = response.GetResponseStream())
                {
                    res = StreamUtil.ReadAll(os);
                }
            }

            return res;
        }

        public static byte[] GetReponseData(AWDEnvironment env,
                                            Uri endpointUri,
                                            string httpMethod,
                                            IDictionary<string, string> headers,
                                            byte[] data)
        {
            byte[] res = null;
            var request = ConstructWebRequest(endpointUri, httpMethod, headers, null);

            if (data != null && data.Length > 0)
            {
                using (var stream = request.GetRequestStream())
                {
                    stream.Write(data, 0, data.Length);
                }
            }

            using (var response = (HttpWebResponse)request.GetResponse())
            {
                using (Stream os = response.GetResponseStream())
                {
                    res = StreamUtil.ReadAll(os);
                }
            }

            return res;
        }

        /// <summary>
        /// Construct a HttpWebRequest onto the specified endpoint and populate
        /// the headers.
        /// </summary>
        /// <param name="endpointUri">The endpoint to call</param>
        /// <param name="httpMethod">GET, PUT etc</param>
        /// <param name="headers">The set of headers to apply to the request</param>
        /// <returns>Initialized HttpWebRequest instance</returns>
        public static HttpWebRequest ConstructWebRequest(Uri endpointUri,
                                                         string httpMethod,
                                                         IDictionary<string, string> headers,
                                                         Tuple<long, long> rangeHeader)
        {
            var request = (HttpWebRequest)WebRequest.Create(endpointUri);
            request.Method = httpMethod;

            foreach (var header in headers.Keys)
            {
                // not all headers can be set via the dictionary
                if (header.Equals("host", StringComparison.OrdinalIgnoreCase))
                    request.Host = headers[header];
                else if (header.Equals("content-length", StringComparison.OrdinalIgnoreCase))
                    request.ContentLength = long.Parse(headers[header]);
                else if (header.Equals("content-type", StringComparison.OrdinalIgnoreCase))
                    request.ContentType = headers[header];
                else
                    request.Headers.Add(header, headers[header]);
            }

            if (rangeHeader != null)
            {
                request.AddRange(rangeHeader.Item1, rangeHeader.Item2);
            }

            return request;
        }
    }
}