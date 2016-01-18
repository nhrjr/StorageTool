using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace StorageTool.Resources
{
    static class Constants
    {
        public const int GetSizeConcurrencyLevel = 2;
        public const int DirSizeStringLength = 2;
        public const string ProfileInputSourceDefault = "Select your source folder.";
        public const string ProfileInputStorageDefault = "Select your storage folder.";
        public const string ProfileInputNameDefault = "Enter your profile name here.";
        public const string ProfileInputNameAlreadyExists = "This name exists already. Please retry";
        public const string CloseApplicationErrorString = "StorageTool is still copying,\n are you sure you wish to close?\n This will cancel all current move operations.";
    }
}
