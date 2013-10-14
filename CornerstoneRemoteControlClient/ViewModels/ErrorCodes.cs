// Copyright © LECO Corporation 2013.  All Rights Reserved.

namespace CornerstoneRemoteControlClient.ViewModels
{
    /// <summary>
    /// Possible error codes that Cornerstone can send.
    /// </summary>
    public class ErrorCodes
    {
        public const int ErrorCodeNone = 0;
        public const int ErrorCodeUnknownCommand = 1;
        public const int ErrorCodeAnotherUserLoggedOn = 2;
        public const int ErrorCodeFailedLogon = 3;
        public const int ErrorCodeMalformedRequest = 4;
        public const int ErrorCodeLogonRequired = 5;
        public const int ErrorCodeException = 6;
        public const int ErrorCodeUnableToExecuteCommand = 7;
        public const int ErrorCodeCommandCurrentlyUnavailable = 8;
        public const int ErrorCodeUnknownCommandParameters = 9;
        public const int ErrorCodeMissingParameters = 10;
        public const int ErrorCodeRequestedItemNotFound = 11;
        public const int ErrorCodeGeneralError = 12;
        public const int ErrorCodeUserDoesNotHavePermissionToExecuteCommand = 13;
        public const int ErrorCodeUnableToDeleteItemIsReferenceByOtherItems = 14;
        public const int ErrorCodeFieldIsNotEditable = 15;
    }
}
