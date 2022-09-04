using System;
using UnityEngine;

/// <summary>Provides a drop-down of every Tag.</summary>
[AttributeUsage(AttributeTargets.Field, AllowMultiple = false, Inherited = false)]
public class TagAttribute : PropertyAttribute { }
