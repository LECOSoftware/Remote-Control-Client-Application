// Copyright © LECO Corporation 2016.  All Rights Reserved.

using System;
using System.Security.Cryptography;
using System.Text;

namespace CornerstoneRemoteControlClient.Helpers
{
    public interface IHashCreator
    {
        string CreateHash(string data);
    }

    public class HashCreator : IHashCreator
    {
        public string CreateHash(string data)
        {
            var hash = SHA512.Create();
            return Convert.ToBase64String(hash.ComputeHash(Encoding.Unicode.GetBytes(data)));
        }
    }
}
