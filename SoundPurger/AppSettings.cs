﻿using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SoundPurger
{
    static class AppSettings
    {
        public static string RootFolder { get; set; }
        public static FileInfo[] FilesToRemove { get; set; }
    }
}
