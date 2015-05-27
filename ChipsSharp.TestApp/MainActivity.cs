using System;
using Android.App;
using Android.OS;
using Android.Widget;
using com.android.ex.chips;
using String = Java.Lang.String;

namespace com.dbeattie
{
	[Activity(Label = "ChipsSharp.TestApp", MainLauncher = true, Icon = "@drawable/icon")]
	public class MainActivity : Activity
	{
		private ChipsEditTextView _phoneRetv;

		protected override void OnCreate(Bundle bundle)
		{
			base.OnCreate(bundle);

			// Set our view from the "main" layout resource
			SetContentView(Resource.Layout.Main);

			_phoneRetv = FindViewById<ChipsEditTextView>(Resource.Id.phone_retv);
			////phoneRetv.ShrinkMaxLines = 3;
			////phoneRetv.SetLines(3);
			////phoneRetv.SetMaxLines(3);
			_phoneRetv.SetTokenizer(new MultiAutoCompleteTextView.CommaTokenizer());
			_phoneRetv.Adapter = new SingleRecipientArrayAdapter2(this,0);

			var button = FindViewById<Button>(Resource.Id.test_button);
			button.Click += ButtonOnClick;

		}

		private int _foo = 0;
		private void ButtonOnClick(object sender, EventArgs eventArgs)
		{
			//_phoneRetv.submitItem("SomeUserName" + _foo,"Number");
			_foo++;
		}
	}

	
}