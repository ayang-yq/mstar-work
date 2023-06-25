using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;

namespace MorningstarAWD.Common.Amazon
{
    public class AWS4SignerForAuthorizationHeader : AWS4SignerBase
    {
        public string ComputeSignature(IDictionary<string, string> headers,
                                       string queryParameters,
                                       string bodyHash,
                                       string awsAccessKey,
                                       string awsSecretKey)
        {
            var requestDateTime = DateTime.UtcNow;
            var dateTimeStamp = requestDateTime.ToString(ISO8601BasicFormat, CultureInfo.InvariantCulture);

            headers.Add(X_Amz_Date, dateTimeStamp);

            var hostHeader = EndpointUri.Host;
            if (!EndpointUri.IsDefaultPort)
                hostHeader += ":" + EndpointUri.Port;
            headers.Add("Host", hostHeader);

            var canonicalizedHeaderNames = CanonicalizeHeaderNames(headers);
            var canonicalizedHeaders = CanonicalizeHeaders(headers);
            var canonicalizedQueryParameters = CanonicalizeQueryParameters(queryParameters);

            var canonicalRequest = CanonicalizeRequest(EndpointUri,
                                                           HttpMethod,
                                                           canonicalizedQueryParameters,
                                                           canonicalizedHeaderNames,
                                                           canonicalizedHeaders,
                                                           bodyHash);

            var canonicalRequestHashBytes = CanonicalRequestHashAlgorithm.ComputeHash(Encoding.UTF8.GetBytes(canonicalRequest));

            var stringToSign = new StringBuilder();
            var dateStamp = requestDateTime.ToString(DateStringFormat, CultureInfo.InvariantCulture);
            var scope = string.Format("{0}/{1}/{2}/{3}", dateStamp, Region, Service, TERMINATOR);

            stringToSign.AppendFormat("{0}-{1}\n{2}\n{3}\n", SCHEME, ALGORITHM, dateTimeStamp, scope);
            stringToSign.Append(ToHexString(canonicalRequestHashBytes, true));

            var kha = KeyedHashAlgorithm.Create(HMACSHA256);
            kha.Key = DeriveSigningKey(HMACSHA256, awsSecretKey, Region, dateStamp, Service);

            var signature = kha.ComputeHash(Encoding.UTF8.GetBytes(stringToSign.ToString()));
            var signatureString = ToHexString(signature, true);

            var authString = new StringBuilder();
            authString.AppendFormat("{0}-{1} ", SCHEME, ALGORITHM);
            authString.AppendFormat("Credential={0}/{1}, ", awsAccessKey, scope);
            authString.AppendFormat("SignedHeaders={0}, ", canonicalizedHeaderNames);
            authString.AppendFormat("Signature={0}", signatureString);

            var authorization = authString.ToString();

            return authorization;
        }
    }
}