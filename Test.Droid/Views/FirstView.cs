using Android.App;
using Android.OS;
using com.android.ex.chips;
using Cirrious.MvvmCross.Droid.Views;

namespace Test.Droid.Views
{
    [Activity(Label = "View for FirstViewModel")]
    public class FirstView : MvxActivity
    {
        protected override void OnCreate(Bundle bundle)
        {
            base.OnCreate(bundle);
            SetContentView(Resource.Layout.first_view);

			var phoneRetv = FindViewById<ChipsEditTextView>(Resource.Id.phone_retv);
	        phoneRetv.Threshold = 3;
        }
    }
}