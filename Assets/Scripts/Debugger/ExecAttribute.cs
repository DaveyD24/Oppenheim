using System;
using UnityEngine;

[AttributeUsage(AttributeTargets.Method)]
public class ExecAttribute : Attribute
{
	public string Description;

	public ExecAttribute() { Description = string.Empty; }

	public ExecAttribute(string Desc)
	{
		Description = Desc;
	}
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
