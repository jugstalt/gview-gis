﻿using System;

/// <summary>
/// The <c>gView.Framework</c> provides all interfaces to develope
/// with and for gView
/// </summary>
namespace gView.Framework.Core.Common
{
    public interface IDebugging
    {
        Exception LastException { get; set; }
    }
}
