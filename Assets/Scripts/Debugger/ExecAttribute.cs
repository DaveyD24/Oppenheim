using System;
using UnityEngine;

[AttributeUsage(AttributeTargets.Method)]
public class ExecAttribute : Attribute
{
	public string Description;
	public bool bExecOnStart;
	internal bool bHideInConsole;
	internal object[] ExecParams;

	public ExecAttribute() { Description = string.Empty; }

	public ExecAttribute(string Desc) { Description = Desc; }

	public ExecAttribute(string Desc, bool bOnStart, params object[] Params) : this(Desc) { bExecOnStart = bOnStart; ExecParams = Params; }

	public ExecAttribute(bool bIsTestExec) : this(string.Empty, true) { bHideInConsole = bIsTestExec; }
}

public struct MethodExec<T1, T2>
{
	public T1 Method;
	public T2 Exec;
	public MethodExec(T1 M, T2 E)
	{
		Method = M;
		Exec = E;
	}
}
