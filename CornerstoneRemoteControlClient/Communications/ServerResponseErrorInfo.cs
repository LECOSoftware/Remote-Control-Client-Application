// Copyright © LECO Corporation 2016.  All Rights Reserved.

using System.Xml.Linq;

namespace CornerstoneRemoteControlClient.Communications
{
    /// <summary>
    /// Simple class that holds the status code and description for
    /// a request to the cloud server.
    /// </summary>
    public class ServerResponseErrorInfo
    {
        public string StatusDescription { get; set; }
        public int StatusCode { get; set; }
    }

    /// <summary>
    /// This class is responsible for parsing a response from the cloud
    /// server with the intent of determining the status code and description.
    /// </summary>
    public static class ServerResponseErrorParser
    {
        #region Public Data

        public const int ErrorCodeTimeout = 1;
        public const int ErrorCodeUnknownInstrument = 5;
        public const int ErrorCodeException = 6;
        public const int ErrorUnableToExecuteCommand = 7;
        public const int ErrorCodeFailedUserValidation = -1;
        public const int ErrorUnknownError = -2;

        #endregion

        /// <summary>
        /// Determines if the response XML data contains error information.
        /// </summary>
        /// <param name="document">XML data.</param>
        /// <param name="expectedRootName">The expected name of the root element of the XML document.</param>
        /// <returns>Instance of response error info if an error parsed. If no error, then returns null.</returns>
        public static ServerResponseErrorInfo ParseServerError(XDocument document, string expectedRootName)
        {
            if (document != null)
            {
                var rootElement = document.Root;
                if (rootElement != null)
                {
                    //Check if the document's root element is what we expect it to be.
                    if (rootElement.Name.ToString() == expectedRootName)
                    {
                        //The document's root element is indeed what we expected, now look
                        //for the status code.
                        int statusCode = GetStatusCode(rootElement);
                        if (statusCode == 0)
                            return null; //normal status code, no error
                        else
                        {
                            //This document has error information. Get the status description.
                            var statusDescription = string.Empty;
                            var statusDescriptionElement = rootElement.Element("StatusDescription");
                            if (statusDescriptionElement != null)
                            {
                                statusDescription = statusDescriptionElement.Value;
                            }

                            //Create the error information and return.
                            var errorInfo = new ServerResponseErrorInfo() { StatusCode = statusCode, StatusDescription = statusDescription };
                            return errorInfo;
                        }
                    }
                    else
                    {
                        //The document's root element is not what we expect. Every path in here should return an error info instance. 
                        int statusCode = GetStatusCode(rootElement);
                        if (statusCode == 0)
                        {
                            //Even though the status code indicates no error, this may be because the status code
                            //does not exist.
                            var rootName = rootElement.Name.ToString().ToUpperInvariant();

                            //User validation is a special case we need to check for.
                            if (rootName == "USERVALIDATION")
                            {
                                var passedValidation = true;
                                bool.TryParse(rootElement.Value, out passedValidation);
                                if (!passedValidation)
                                {
                                    return new ServerResponseErrorInfo() { StatusCode = ErrorCodeFailedUserValidation };
                                }
                            }
                            else
                            {
                                return new ServerResponseErrorInfo() { StatusCode = ErrorUnknownError, StatusDescription = rootElement.Value };
                            }
                        }
                        else
                        {
                            //Get the status description.
                            var statusDescription = string.Empty;
                            var statusDescriptionElement = rootElement.Element("StatusDescription");
                            if (statusDescriptionElement != null)
                            {
                                statusDescription = statusDescriptionElement.Value;
                            }

                            //Create the error information and return.
                            var errorInfo = new ServerResponseErrorInfo() { StatusCode = statusCode, StatusDescription = statusDescription };
                            return errorInfo;
                        }
                    }
                }
            }

            //Return null, indicating no error.
            return null;
        }

        /// <summary>
        /// Given a status code, returns an appropriate error message.
        /// </summary>
        /// <param name="errorInfo">Error info instance.</param>
        /// <returns>Error message.</returns>
        public static string GetAppropriateErrorMessage(ServerResponseErrorInfo errorInfo)
        {
            if (errorInfo != null)
            {
                string errorMessage;

                switch (errorInfo.StatusCode)
                {
                    case ErrorCodeException:
                        errorMessage = errorInfo.StatusDescription;
                        break;
                    case ErrorCodeTimeout:
                        errorMessage = "A timeout occurred waiting for instrument response. Please check internet connection.";
                        break;
                    case ErrorCodeUnknownInstrument:
                        errorMessage = "The instrument is not currently online.";
                        break;
                    case ErrorCodeFailedUserValidation:
                        errorMessage = "The instrument has indicated that the supplied credentials are invalid. Ensure password is correct.";
                        break;
                    case ErrorUnableToExecuteCommand:
                        errorMessage = "Unable to execute remote command. Please check Cornerstone instrument remote query expiration.";
                        break;
                    case ErrorUnknownError:
                        errorMessage = string.Format("An unknown error has occurred attempting communicate with server. Status code: {0}. {1}", errorInfo.StatusCode, errorInfo.StatusDescription);
                        break;
                    default:
                        errorMessage = string.Format("An unknown error has occurred attempting communicate with server. Status code: {0}.", errorInfo.StatusCode);
                        break;
                }

                return errorMessage;
            }
            return string.Empty;
        }

        /// <summary>
        /// Determines the status code for the specified XML data.
        /// </summary>
        /// <param name="element">XML data.</param>
        /// <returns>Int value representing a status code.</returns>
        private static int GetStatusCode(XElement element)
        {
            var statusCode = 0;

            if (element != null)
            {
                var statusCodeElement = element.Element("StatusCode");
                if (statusCodeElement != null)
                {
                    int.TryParse(statusCodeElement.Value, out statusCode);
                }
                else
                {
                    var errorCodeElement = element.Element("ErrorCode");
                    if (errorCodeElement != null)
                    {
                        int.TryParse(errorCodeElement.Value, out statusCode);
                    }
                }
            }

            return statusCode;
        }
    }
}
