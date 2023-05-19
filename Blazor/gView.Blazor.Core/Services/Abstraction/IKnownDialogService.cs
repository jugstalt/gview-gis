using gView.Framework.Blazor;
using System;

namespace gView.Blazor.Core.Services.Abstraction;

public interface IKnownDialogService
{
    KnownDialogs Dialog { get; }
    Type RazorType { get; }
    string Title { get; }
}
