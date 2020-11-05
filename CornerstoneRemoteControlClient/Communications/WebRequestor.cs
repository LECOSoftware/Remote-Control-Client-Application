// Copyright © LECO Corporation 2016.  All Rights Reserved.

using System;
using System.Collections.Generic;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Linq;

namespace CornerstoneRemoteControlClient.Communications
{
    /// <summary>
    /// Interace describing communication functionality with cloud server.
    /// </summary>
    public interface IWebRequestor
    {
        Task<XDocument> MakeRequest(string uri, string postContent);
        string CreateUri(string page, Dictionary<string, string> parameters, string server = "");
        string CreateUri(string page, string id, Dictionary<string, string> parameters = null, string server = "");
        string CreateUri(string page, string id, string data, Dictionary<string, string> parameters = null, string server = "");

        Guid GetInstrumentIdFromPayload(XElement payloadRoot);
    }

    /// <summary>
    /// This class is responsible for make requests to the Cornerstone Azure cloud server.
    /// </summary>
    public class WebRequestor : IWebRequestor
    {
        #region Constructor

        /// <summary>
        /// Constructor
        /// </summary>
        public WebRequestor() { }

        #endregion

        #region Public Methods

        /// <summary>
        /// Creates a URL describing a request to the Azure cloud server. This method is used when making a
        /// request to a cloud server page that does not forward the request onto a Cornerstone instrument,
        /// but rather fulfills the request on the server.
        /// </summary>
        /// <param name="page">Name of page on cloud server.</param>
        /// <param name="parameters">Collection of query string parameters.</param>
        /// <param name="server">Server address.</param>
        /// <returns>URL string formatted with the server address, page and query string parameters.</returns>
        public string CreateUri(string page, Dictionary<string, string> parameters, string server = "")
        {
            string paramsString = string.Empty;
            bool first = true;
            string paramSymbol = "?";

            if (parameters != null)
            {
                foreach (var param in parameters)
                {
                    paramsString += string.Format("{0}{1}={2}", paramSymbol, param.Key, param.Value);

                    if (first)
                    {
                        first = false;
                        paramSymbol = "&";
                    }
                }
            }

            if (string.IsNullOrEmpty(server))
            {
                server = "remote.lecosoftware.com";
            }

            //remove any slash characters that user may have entered.
            server = server.Replace("/", "");

            return string.Format("https://{0}/{1}{2}", server, page, paramsString);
        }

        /// <summary>
        /// Creates a URL describing a request to the Azure cloud server. This method is used to make a request for
        /// data from a specific Cornerstone instrument, represented by the 'id' parameter. The cloud server will
        /// forward this request on to the intended Cornerstone instrument and then return the results when ready.
        /// </summary>
        /// <param name="page">Name of page on cloud server.</param>
        /// <param name="id">Unique instrument identifier, indicating which instrument this message is intended for.</param>
        /// <param name="parameters">Collection of query string parameters.</param>
        /// <param name="server">Server address.</param>
        /// <returns>URL string formatted with the server address, page and query string parameters.</returns>
        /// <returns></returns>
        public string CreateUri(string page, string id, Dictionary<string, string> parameters = null, string server = "")
        {
            string paramsString = string.Empty;
            bool first = true;
            string paramSymbol = "?";

            if (parameters == null)
            {
                parameters = new Dictionary<string, string>();
            }

            if (!parameters.ContainsKey("Id"))
                parameters.Add("Id", id);

            foreach (var param in parameters)
            {
                paramsString += string.Format("{0}{1}={2}", paramSymbol, param.Key, param.Value);

                if (first)
                {
                    first = false;
                    paramSymbol = "&";
                }
            }

            if (string.IsNullOrEmpty(server))
            {
                server = "remote.lecosoftware.com";
            }

            //remove any slash characters that user may have entered.
            server = server.Replace("/", "");

            return string.Format("https://{0}/{1}{2}", server, page, paramsString);
        }

        /// <summary>
        /// Creates a URL describing a request to the Azure cloud server. This method is also used to make a request for
        /// data from a specific Cornerstone instrument, similar to the method above. The difference is that this method includes
        /// the type of data requested (represented by the 'data' parameter) in the query string. This is the format used by
        /// earlier versions of Cornerstone server and instrument software. Newer versions will include this data indicator
        /// in the posted data portion of the request.
        /// </summary>
        /// <param name="page">Name of page on cloud server.</param>
        /// <param name="id"></param>
        /// <param name="data"></param>
        /// <param name="parameters">Collection of query string parameters.</param>
        /// <param name="server">Server address.</param>
        /// <returns>URL string formatted with the server address, page and query string parameters.</returns>
        public string CreateUri(string page, string id, string data, Dictionary<string, string> parameters = null, string server = "")
        {
            string paramsString = string.Empty;
            bool first = true;
            string paramSymbol = "?";

            if (parameters == null)
            {
                parameters = new Dictionary<string, string>();
            }

            if (!parameters.ContainsKey("Id"))
                parameters.Add("Id", id);
            if (!parameters.ContainsKey("data"))
                parameters.Add("data", data);

            foreach (var param in parameters)
            {
                paramsString += string.Format("{0}{1}={2}", paramSymbol, param.Key, param.Value);

                if (first)
                {
                    first = false;
                    paramSymbol = "&";
                }
            }

            if (string.IsNullOrEmpty(server))
            {
                server = "remote.lecosoftware.com";
            }

            //remove any slash characters that user may have entered.
            server = server.Replace("/", "");

            return string.Format("https://{0}/{1}{2}", server, page, paramsString);
        }

        /// <summary>
        /// Sends an HTTP request to the specified URL, including with the request the data in the 'postContent' parameter.
        /// </summary>
        /// <param name="uri">Page to which request is sent.</param>
        /// <param name="postContent">POST data to attach to the request.</param>
        /// <returns>Return data in and XML document.</returns>
        public async Task<XDocument> MakeRequest(string uri, string postContent)
        {
            XDocument xDoc = new XDocument();

            try
            {
                //create the web request
                var httpWebRequest = (HttpWebRequest)WebRequest.Create(uri);

                //if we have post data, write it to the request stream
                if (!string.IsNullOrWhiteSpace(postContent))
                {
                    byte[] data = Encoding.UTF8.GetBytes(postContent);
                    httpWebRequest.Method = "post";
                    httpWebRequest.ContentType = "text/xml";

                    using (var reqStream = await httpWebRequest.GetRequestStreamAsync())
                    {
                        reqStream.Write(data, 0, data.Length);
                    }
                }

                using (var webResponse = await httpWebRequest.GetResponseAsync())
                {
                    using (var stream = webResponse.GetResponseStream())
                    {
                        if (stream != null)
                        {
                            xDoc = XDocument.Load(stream);
                        }
                    }
                }
            }
            catch (Exception /*e*/)
            {
                //var isConnected = await IsConnected();
                //if (isConnected)
                //    xDoc = CreateExceptionElement(e);
                //else
                    xDoc = CreateNoNetworkElement();
            }

            return xDoc;
        }

        /// <summary>
        /// Extracts the instrument identifier from the XML data provided. This XML has
        /// been return by the cloud server in response to a request for instrument data.
        /// </summary>
        /// <param name="payloadRoot">Instrument data as XML</param>
        /// <returns>Instrument identifier. If the instrument identifier can not be found in the XML
        /// data, the empty GUID is returned.</returns>
        public Guid GetInstrumentIdFromPayload(XElement payloadRoot)
        {
            Guid instrumentId = Guid.Empty;

            if (payloadRoot != null)
            {
                var instrumentIdAttribute = payloadRoot.Attribute("InstrumentID");
                if (instrumentIdAttribute != null)
                {
                    Guid.TryParse(instrumentIdAttribute.Value, out instrumentId);
                }
            }

            return instrumentId;
        }

        #endregion

        #region Private Methods

        /// <summary>
        /// Creates an exception XML document containing the provided message.
        /// </summary>
        /// <param name="message">Exception message.</param>
        /// <returns>XML document with exception message.</returns>
        private XDocument CreateExceptionElement(string message)
        {
            XDocument xDoc = new XDocument();

            if (!string.IsNullOrWhiteSpace(message))
            {
                var exceptionElement = new XElement("Exception") { Value = message };
                xDoc.Add(exceptionElement);
            }
            return xDoc;
        }

        /// <summary>
        /// Creates an exception XML document containing the provided exception.
        /// </summary>
        /// <param name="exception">Exception.</param>
        /// <returns>XML document containing the exception's message.</returns>
        private XDocument CreateExceptionElement(Exception exception)
        {
            XDocument xDoc = new XDocument();

            if (exception != null)
            {
                var exceptionElement = new XElement("Exception") { Value = exception.Message };
                xDoc.Add(exceptionElement);
            }
            return xDoc;
        }

        /// <summary>
        /// Creates an XML document representing the state where there is no network available. This
        /// is done by setting the StatusCode and StatusDescription elements which are expected to be
        /// present in the parsing code.
        /// </summary>
        /// <returns>XML document with status code and description for no network availability.</returns>
        private XDocument CreateNoNetworkElement()
        {
            XDocument xDoc = new XDocument();

            var rootElement = new XElement("root");
            xDoc.Add(rootElement);
            var statusCodeElement = new XElement("StatusCode") { Value = ServerResponseErrorParser.ErrorCodeException.ToString() };
            rootElement.Add(statusCodeElement);
            var statusDescriptionElement = new XElement("StatusDescription") { Value = "A network connection is not available." };
            rootElement.Add(statusDescriptionElement);

            return xDoc;
        }

        #endregion
    }
}
