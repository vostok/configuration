using System;
// ReSharper disable CheckNamespace

namespace JetBrains.Annotations
{
    [AttributeUsage(
        AttributeTargets.Method | AttributeTargets.Parameter | AttributeTargets.Property |
        AttributeTargets.Delegate | AttributeTargets.Field | AttributeTargets.Event |
        AttributeTargets.Class | AttributeTargets.Interface | AttributeTargets.GenericParameter)]
    internal sealed class CanBeNullAttribute : Attribute { }
}