using System.IO;
using System.Net;
using System.Security.Cryptography.X509Certificates;
using System.Xml;

namespace Pangea.Util
{
    public static class Soap
    {
        public static HttpWebRequest CreateWebRequest(string url, string action)
        {
            HttpWebRequest webRequest = (HttpWebRequest)WebRequest.Create(url);
            webRequest.Headers.Add("SOAPAction", action);
            webRequest.ContentType = "text/xml;charset=\"utf-8\"";
            webRequest.Accept = "text/xml";
            webRequest.Method = "POST";
            return webRequest;
        }

        public static XmlDocument CreateSoapEnvelope(string xml)
        {
            XmlDocument soapEnvelopeDocument = new XmlDocument();
            soapEnvelopeDocument.LoadXml(xml);
            return soapEnvelopeDocument;
        }

        public static void InsertSoapEnvelopeIntoWebRequest(XmlDocument soapEnvelopeXml, HttpWebRequest webRequest)
        {
            using (Stream stream = webRequest.GetRequestStream())
            {
                soapEnvelopeXml.Save(stream);
            }
        }

        //Gambi
        public static bool CustomValidation(object sender,X509Certificate cert,X509Chain chain, System.Net.Security.SslPolicyErrors error)
        {
            return true;
        }
    }
}
