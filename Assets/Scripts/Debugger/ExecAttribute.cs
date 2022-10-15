using System;
using UnityEngine;

[AttributeUsage(AttributeTargets.Method)]
public class ExecAttribute : Attribute
{
	public Type Behaviour;
	public string Description;

	public ExecAttribute() { }

	public ExecAttribute(Type Behaviour)
	{
		this.Behaviour = Behaviour;
		Description = string.Empty;
	}

	public ExecAttribute(Type Behaviour, string Desc) : this(Behaviour)
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
