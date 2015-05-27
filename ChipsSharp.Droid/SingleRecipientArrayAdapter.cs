using System;
using Android.Content;
using Android.Runtime;
using Android.Views;
using Android.Widget;
using Java.Lang;
using Object = System.Object;
using String = System.String;

namespace com.android.ex.chips
{
	public class SingleRecipientArrayAdapter : ArrayAdapter
	{
		private static readonly string[] Data = {"one"};
		private readonly LayoutInflater _layoutInflater;

		protected SingleRecipientArrayAdapter(IntPtr javaReference, JniHandleOwnership transfer)
			: base(javaReference, transfer)
		{
		}

		public SingleRecipientArrayAdapter(Context context, int resource)
			: base(context, Resource.Layout.chips_alternate_item, Data)
		{
			_layoutInflater = LayoutInflater.FromContext(context);
		}

		public override View GetView(int position, View convertView, ViewGroup parent)
		{
			var view = _layoutInflater.Inflate(Resource.Layout.chips_alternate_item, parent, false);
			return view;
		}
	}

	public class SingleRecipientArrayAdapter2 : ArrayAdapter
	{
		private static readonly string[] Data = { "one" };
		private readonly LayoutInflater _layoutInflater;

		protected SingleRecipientArrayAdapter2(IntPtr javaReference, JniHandleOwnership transfer)
			: base(javaReference, transfer)
		{
		}

		public SingleRecipientArrayAdapter2(Context context, int resource)
			: base(context, Resource.Layout.chips_alternate_item, Data)
		{
			_layoutInflater = LayoutInflater.FromContext(context);
			
		}

		public override Filter Filter
		{
			get
			{
				return new MyFilter(this);
			}
		}

		public override View GetView(int position, View convertView, ViewGroup parent)
		{
			var view = _layoutInflater.Inflate(Resource.Layout.chips_recipient_dropdown_item, parent, false);
			return view;
		}
	}

	public class MyFilter : Filter
	{
		private readonly SingleRecipientArrayAdapter2 _owner;

		public MyFilter(SingleRecipientArrayAdapter2 owner)
		{
			_owner = owner;
		}

		protected override FilterResults PerformFiltering(ICharSequence constraint)
		{
			return new FilterResults
			{
				Count = 1
			};
		}

		protected override void PublishResults(ICharSequence constraint, FilterResults results)
		{
			_owner.NotifyDataSetInvalidated();
		}
	}
}