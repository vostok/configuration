﻿using System;

namespace Vostok.Configuration.Sources
{
    /// <inheritdoc />
    /// <summary>
    /// Json converter to RawSettings tree from file
    /// </summary>
    public class JsonFileSource : BaseFileSource<JsonStringSource>
    {
        /// <summary>
        /// Creating json converter
        /// </summary>
        /// <param name="filePath">File name with settings</param>
        /// <param name="observePeriod">Observe period in ms (min 100, default 10000)</param>
        public JsonFileSource(string filePath, TimeSpan observePeriod = default)
            : base(filePath, observePeriod)
        { }
    }
}