using System;
using System.Collections.Generic;
using System.Linq;
using Foundation;
using UIKit;

namespace BeatBoxers.iOS
{
	[Register ("AppDelegate")]
	public partial class AppDelegate : global::Xamarin.Forms.Platform.iOS.FormsApplicationDelegate
	{
		public override bool FinishedLaunching (UIApplication app, NSDictionary options)
		{
			global::Xamarin.Forms.Forms.Init ();

			LoadApplication (new App (new Bluetooth(), new AudioIO(), new AudioResource()));

			UIApplication.SharedApplication.IdleTimerDisabled = true;

			return base.FinishedLaunching (app, options);
		}
	}
}

