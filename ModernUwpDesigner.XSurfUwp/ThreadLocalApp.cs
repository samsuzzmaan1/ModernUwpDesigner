using Windows.Foundation;
using Windows.UI.Xaml;

namespace XSurfUwp;

public partial class ThreadLocalApp : DependencyObject
{
	public static DependencyProperty DeviceSizeProperty = DependencyProperty.Register("DeviceSize", typeof(Size), typeof(ThreadLocalApp), null);

	public Size DeviceSize
	{
		get
		{
			return (Size)GetValue(DeviceSizeProperty);
		}
		set
		{
			SetValue(DeviceSizeProperty, value);
		}
	}
}
