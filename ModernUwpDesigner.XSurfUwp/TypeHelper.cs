using System;
using Windows.UI.Xaml.Markup;

namespace XSurfUwp;

internal static class TypeHelper
{
	private class MarkupExtensionAccessor
	{
		public Type GetDesignInstanceType()
		{
			return typeof(DesignInstance);
		}

		public Type GetMarkupExtensionType()
		{
			return typeof(MarkupExtension);
		}
	}

	public static Type GetDesignInstanceType()
	{
		try
		{
			return new MarkupExtensionAccessor().GetDesignInstanceType();
		}
		catch (TypeLoadException)
		{

		}

		return null;
	}

	public static Type GetMarkupExtensionType()
	{
		try
		{
			return new MarkupExtensionAccessor().GetMarkupExtensionType();
		}
		catch (TypeLoadException)
		{

		}

		return null;
	}
}
