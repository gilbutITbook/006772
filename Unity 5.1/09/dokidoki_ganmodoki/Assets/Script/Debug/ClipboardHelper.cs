// ↓여기서 가져왔어요.
// http://gamesonytablet.blogspot.jp/2012/09/unity_13.html
//

using UnityEngine;
using System;
using System.Reflection;

public class ClipboardHelper
{
	private static PropertyInfo m_systemCopyBufferProperty = null;
	private static PropertyInfo GetSystemCopyBufferProperty()
	{
		if (m_systemCopyBufferProperty == null)
		{
			Type T = typeof(GUIUtility);
			m_systemCopyBufferProperty = T.GetProperty("systemCopyBuffer", BindingFlags.Static | BindingFlags.NonPublic);
			if (m_systemCopyBufferProperty == null)
				throw new Exception("Can't access internal member 'GUIUtility.systemCopyBuffer' it may have been removed / renamed");
		}
		return m_systemCopyBufferProperty;
	}
	public static string clipBoard
	{
		get 
		{
			PropertyInfo P = GetSystemCopyBufferProperty();
			return (string)P.GetValue(null,null);
		}
		set
		{
			PropertyInfo P = GetSystemCopyBufferProperty();
			P.SetValue(null,value,null);
		}
	}
}