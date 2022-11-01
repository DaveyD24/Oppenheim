using System;
using System.Linq;
using System.Text;
using System.Reflection;
using System.Collections.Generic;
using UnityEngine;
using IN = UnityEngine.Input;

public class Console : MonoBehaviour
{
	Dictionary<string, MethodExec<MethodInfo, ExecAttribute>> Funcs;

	bool bShowConsole = false;
	string Input;
	string PreviousInput;

	void Awake()
	{
		Funcs = new Dictionary<string, MethodExec<MethodInfo, ExecAttribute>>();

		IEnumerable<MethodInfo> Methods = typeof(Console).Assembly.GetTypes()
			.SelectMany(Type => Type.GetMethods());

		foreach (MethodInfo Method in Methods)
		{
			ExecAttribute Command = (ExecAttribute)Attribute.GetCustomAttribute(Method, typeof(ExecAttribute));

			if (Command == null)
				continue;

			Funcs.Add(Method.Name, new MethodExec<MethodInfo, ExecAttribute>(Method, Command));

#if UNITY_EDITOR
			if (Command.bExecOnStart)
			{
				Exec(Method.Name, Command.ExecParams);
			}
#endif

			if (Command.bHideInConsole)
			{
				Funcs.Remove(Method.Name);
			}
		}
	}

	void Update()
	{
		if (IN.GetKeyDown(KeyCode.BackQuote))
		{
			bShowConsole = !bShowConsole;
		}
	}

	void BuildExec()
	{
		if (!string.IsNullOrEmpty(Input))
		{
			string[] Split = Input.Split(' ');
			object[] Args = new object[Split.Length - 1];
			string Func = Split[0];

			for (int o = 0, s = 1; s < Split.Length; ++s, ++o)
				Args[o] = Split[s];

			Exec(Func, Args);

			PreviousInput = Input;

		}

		Input = "";
	}

	public void Exec(string MethodName, params object[] Params)
	{
		if (Funcs.ContainsKey(MethodName))
		{
			try
			{
				MethodExec<MethodInfo, ExecAttribute> Func = Funcs[MethodName];

				object[] Parameters = new object[Params.Length];
				ParameterInfo[] MethodParams = Func.Method.GetParameters();

				for (int i = 0; i < Params.Length; ++i)
					Parameters[i] = Convert.ChangeType(Params[i], MethodParams[i].ParameterType);

				if (Func.Method.IsStatic)
				{
					Func.Method.Invoke(null, Parameters);
				}
				else
				{
					Func.Method.Invoke(Convert.ChangeType(FindObjectOfType(Func.Method.DeclaringType), Func.Method.DeclaringType), Parameters);
				}
			}
			catch (Exception)
			{
				StringBuilder ErrorBuilder = new StringBuilder();

				for (int i = 0; i < Params.Length; ++i)
				{
					ErrorBuilder.Append(Params[i].ToString());
					if (i != Params.Length - 1)
						ErrorBuilder.Append(", ");
				}

				Debug.LogError($"Failed to execute {MethodName} ({ErrorBuilder})");
			}
		}
		else
		{
			Debug.LogError($"Unknown Command: {MethodName}");
		}
	}

	Vector2 Scroll;

	void OnGUI()
	{
		if (!bShowConsole)
		{
			return;
		}

		if (Event.current.Equals(Event.KeyboardEvent("Return")))
		{
			BuildExec();
			return;
		}

		if (Event.current.Equals(Event.KeyboardEvent("Backquote")))
		{
			bShowConsole = false;
			return;
		}

		if (Event.current.Equals(Event.KeyboardEvent("Up")))
		{
			Input = PreviousInput;
		}

		float Y = 0f;

		if (Funcs.Count > 0)
		{
			float FuncsHeight = Screen.height * .75f;

			GUI.Box(new Rect(0, Y, Screen.width, FuncsHeight), "");
			Rect ExecList = new Rect(0, 0, Screen.width - 30, 20 * Funcs.Count);

			GUI.backgroundColor = Color.white;
			Scroll = GUI.BeginScrollView(new Rect(0, Y + 5, Screen.width, FuncsHeight), Scroll, ExecList);

			int i = 0;
#pragma warning disable UNT0018 // System.Reflection features in performance critical messages. MethodInfo is already cached in Funcs by Start().
			foreach (KeyValuePair<string, MethodExec<MethodInfo, ExecAttribute>> Func in Funcs)
			{
				StringBuilder ParamsBuilder = new StringBuilder();
				foreach (ParameterInfo Param in Func.Value.Method.GetParameters())
				{
					string[] ParamSplit = Param.ParameterType.Name.Split('.');
					ParamsBuilder.Append(ParamSplit[ParamSplit.Length - 1]).Append(" ").Append(Param.Name).Append(", ");
				}

				string Text = $"{Func.Value.Method.Name} ({ParamsBuilder.ToString().TrimEnd(',', ' ')}) - {Func.Value.Exec.Description}";

				Rect TextRect = new Rect(5, 20 * i++, ExecList.width - 100, 20);

				GUI.Label(TextRect, Text);
			}
#pragma warning restore UNT0018 // System.Reflection features in performance critical messages. MethodInfo is already cached in Funcs by Start().

			GUI.EndScrollView();

			Y += FuncsHeight;
		}

		GUI.Box(new Rect(0, Y, Screen.width, 30), "");
		GUI.backgroundColor = Color.white;

		GUI.SetNextControlName("Exec Text Field");
		Input = GUI.TextField(new Rect(10f, Y + 5f, Screen.width - 20f, 20f), Input);
		GUI.FocusControl("Exec Text Field");
	}
}
