﻿using gView.Cmd.Core.Abstraction;
using gView.Cmd.Core.Builders;
using gView.Framework.Data;
using System;
using System.Collections.Generic;
using System.Text;

namespace gView.Cmd.Core.Extensions;
static public class CommandParameterDescriptionExtensions
{
    static public ICommandPararmeterBuilder GetBuilder(this ICommandParameterDescription parameterDescription)
    {
        ICommandPararmeterBuilder parameterBuilder = parameterDescription.ParameterType switch
        {
            Type t when t == typeof(IFeatureClass) => new FeatureClassParameterBuilder(parameterDescription.Name),
            _ => throw new Exception($"There is no parameter builder for type {parameterDescription.ParameterType} available")
        };

        return parameterBuilder;
    }
}