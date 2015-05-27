using System;
using System.Threading;
using Android.App;
using Android.Content;
using Android.Runtime;
using Android.Widget;
using Cirrious.CrossCore.Droid;
using Cirrious.CrossCore.Platform;
using Java.Lang;
using Object = Java.Lang.Object;
using String = Java.Lang.String;

namespace Test.Droid.Helpers
{
	public class MvxFilteringAdapterEx
		: MvxAdapterEx, IFilterable
	{
		private readonly ManualResetEvent _dataChangedEvent = new ManualResetEvent(false);
		private MvxReplaceableJavaContainer _javaContainer;
		private string _partialText;

		public MvxFilteringAdapterEx(Context context)
			: base(context)
		{
			ReturnSingleObjectFromGetItem = true;
			Filter = new MyFilter(this);
		}

		protected MvxFilteringAdapterEx(IntPtr javaReference, JniHandleOwnership transfer)
			: base(javaReference, transfer)
		{
		}

		public string PartialText
		{
			get { return _partialText; }
			private set
			{
				_partialText = value;
				FireConstraintChanged();
			}
		}

		public bool ReturnSingleObjectFromGetItem { get; set; }

		#region Implementation of IFilterable

		public Filter Filter { get; set; }

		#endregion

		private int SetConstraintAndWaitForDataChange(string newConstraint)
		{
			MvxTrace.Trace("Wait starting for {0}", newConstraint);
			_dataChangedEvent.Reset();
			PartialText = newConstraint;
			_dataChangedEvent.WaitOne();
			MvxTrace.Trace("Wait finished with {1} items for {0}", newConstraint, Count);
			return Count;
		}

		public event EventHandler PartialTextChanged;

		private void FireConstraintChanged()
		{
			var activity = Context as Activity;

			if (activity == null)
				return;

			activity.RunOnUiThread(() =>
			{
				var handler = PartialTextChanged;
				if (handler != null)
					handler(this, EventArgs.Empty);
			});
		}

		public override void NotifyDataSetChanged()
		{
			_dataChangedEvent.Set();
			base.NotifyDataSetChanged();
		}

		public override Object GetItem(int position)
		{
			// for autocomplete views we need to return something other than null here
			// - see @JonPryor's answer in http://stackoverflow.com/questions/13842864/why-does-the-gref-go-too-high-when-i-put-a-mvxbindablespinner-in-a-mvxbindableli/13995199#comment19319057_13995199
			// - and see problem report in https://github.com/slodge/MvvmCross/issues/145
			// - obviously this solution is not good for general Java code!
			if (ReturnSingleObjectFromGetItem)
			{
				if (_javaContainer == null)
					_javaContainer = new MvxReplaceableJavaContainer();
				_javaContainer.Object = GetRawItem(position);
				return _javaContainer;
			}

			return base.GetItem(position);
		}

		private class MyFilter : Filter
		{
			private readonly MvxFilteringAdapterEx _owner;

			public MyFilter(MvxFilteringAdapterEx owner)
			{
				_owner = owner;
			}

			#region Overrides of Filter

			protected override FilterResults PerformFiltering(ICharSequence constraint)
			{
				var stringConstraint = constraint == null ? string.Empty : constraint.ToString();

				var count = _owner.SetConstraintAndWaitForDataChange(stringConstraint);

				return new FilterResults
				{
					Count = count
				};
			}

			protected override void PublishResults(ICharSequence constraint, FilterResults results)
			{
				// force a refresh
				_owner.NotifyDataSetInvalidated();
			}

			public override ICharSequence ConvertResultToStringFormatted(Object resultValue)
			{
				var ourContainer = resultValue as MvxJavaContainer;
				if (ourContainer == null)
				{
					return base.ConvertResultToStringFormatted(resultValue);
				}

				return new String(ourContainer.Object.ToString());
			}

			#endregion
		}
	}
}