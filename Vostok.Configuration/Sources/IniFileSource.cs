﻿using System;

namespace Vostok.Configuration.Sources
{
    /// <inheritdoc />
    /// <summary>
    /// Ini converter to RawSettings tree from file
    /// </summary>
    public class IniFileSource : BaseFileSource<IniStringSource>
    {
        /// <summary>
        /// Creating ini converter
        /// </summary>
        /// <param name="filePath">File name with settings</param>
        /// <param name="observePeriod">Observe period in ms (min 100, default 10000)</param>
        /// <param name="callBack">Callback on exception</param>
        public IniFileSource(string filePath, TimeSpan observePeriod = default, Action<Exception> callBack = null)
            :base(filePath, observePeriod, callBack)
        { }
    }
}