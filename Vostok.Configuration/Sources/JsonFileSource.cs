﻿using System;
using System.IO;

namespace Vostok.Configuration.Sources
{
    /// <inheritdoc />
    /// <summary>
    /// Json converter to RawSettings tree from file
    /// </summary>
    public class JsonFileSource : BaseFileSource
    {
        /// <summary>
        /// Creating json converter
        /// </summary>
        /// <param name="filePath">File name with settings</param>
        /// <param name="observePeriod">Observe period in ms (min 100, default 10000)</param>
        /// <param name="callBack">Callback action on exception</param>
        public JsonFileSource(string filePath, TimeSpan observePeriod = default, Action<Exception> callBack = null)
            : base(filePath,
                () =>
                {
                    using (var source = new JsonStringSource(File.ReadAllText(filePath)))
                        return source.Get();
                },
                observePeriod,
                callBack)
        { }
    }
}