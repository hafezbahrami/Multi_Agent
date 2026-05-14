using System.Dynamic;
using System.Runtime.InteropServices;

namespace MQ.Office.MCP.Server.Tests.Helpers;

/// <summary>
/// DynamicObject whose property/method access can be customised — used to
/// simulate edge cases (null ActiveDocument, COM exceptions, etc.) that
/// <see cref="ExpandoObject"/> cannot easily express.
/// </summary>
internal sealed class FakeCom : DynamicObject
{
    private readonly Dictionary<string, object?> _props = new(StringComparer.Ordinal);
    private readonly Dictionary<string, Func<object?>> _getters = new(StringComparer.Ordinal);

    public FakeCom Set(string name, object? value) { _props[name] = value; return this; }
    public FakeCom SetGetter(string name, Func<object?> getter) { _getters[name] = getter; return this; }

    public override bool TryGetMember(GetMemberBinder binder, out object? result)
    {
        if (_getters.TryGetValue(binder.Name, out var fn))
        {
            result = fn();
            return true;
        }
        return _props.TryGetValue(binder.Name, out result);
    }

    public override bool TrySetMember(SetMemberBinder binder, object? value)
    {
        _props[binder.Name] = value;
        return true;
    }

    public override bool TryInvokeMember(InvokeMemberBinder binder, object?[]? args, out object? result)
    {
        if (_props.TryGetValue(binder.Name, out var value) && value is Delegate d)
        {
            result = d.DynamicInvoke(args);
            return true;
        }
        result = null;
        return true;
    }

    public static COMException MakeComException(int hresult = unchecked((int)0x80004005)) =>
        new("Simulated COM error", hresult);
}
