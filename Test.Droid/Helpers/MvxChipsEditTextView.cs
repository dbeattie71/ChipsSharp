using System;
using System.Collections;
using System.Collections.Generic;
using Android.Content;
using Android.Runtime;
using Android.Util;
using Android.Views;
using Android.Widget;
using com.android.ex.chips;
using Cirrious.MvvmCross.Binding.Attributes;
using Cirrious.MvvmCross.Binding.Droid.Views;

namespace Test.Droid.Helpers
{
	[Register("test.droid.helpers.MvxChipsEditTextView")]
	public class MvxChipsEditTextView
		: ChipsEditTextView
	{
		//private object _selectedObject;
		//private List<ChipEntry> _selectedChipEntries;

		public MvxChipsEditTextView(Context context, IAttributeSet attrs)
			: this(context, attrs, new MvxFilteringAdapterEx(context))
		{
			SetTokenizer(new MultiAutoCompleteTextView.CommaTokenizer());

			// note - we shouldn't realy need both of these... but we do

			//ItemClick += OnItemClick;
			//ItemSelected += OnItemSelected;
		}

		public MvxChipsEditTextView(Context context, IAttributeSet attrs,
										MvxFilteringAdapterEx adapter)
			: base(context, attrs)
		{
			var itemTemplateId = MvxAttributeHelpers.ReadListItemTemplateId(context, attrs);
			adapter.ItemTemplateId = itemTemplateId;
			Adapter = adapter;
			//ItemClick += OnItemClick;

		}

		protected MvxChipsEditTextView(IntPtr javaReference, JniHandleOwnership transfer)
			: base(javaReference, transfer)
		{
		}

		public new MvxFilteringAdapterEx Adapter
		{
			get { return base.Adapter as MvxFilteringAdapterEx; }
			set
			{
				var existing = Adapter;
				if (existing == value)
					return;

				if (existing != null)
					existing.PartialTextChanged -= AdapterOnPartialTextChanged;

				if (existing != null && value != null)
				{
					value.ItemsSource = existing.ItemsSource;
					value.ItemTemplateId = existing.ItemTemplateId;
				}

				if (value != null)
					value.PartialTextChanged += AdapterOnPartialTextChanged;

				base.Adapter = value;
			}
		}

		[MvxSetToNullAfterBinding]
		public IEnumerable ItemsSource
		{
			get { return Adapter.ItemsSource; }
			set
			{
				Adapter.ItemsSource = value;
			}
		}

		public int ItemTemplateId
		{
			get { return Adapter.ItemTemplateId; }
			set { Adapter.ItemTemplateId = value; }
		}

		public string PartialText
		{
			get { return Adapter.PartialText; }
		}

		//public object SelectedObject
		//{
		//	get { return _selectedObject; }
		//	private set
		//	{
		//		if (_selectedObject == value)
		//			return;

		//		_selectedObject = value;
		//		FireChanged(SelectedObjectChanged);
		//	}
		//}

		//[MvxSetToNullAfterBinding]
		//public List<ChipEntry> SelectedChipEntries
		//{
		//	get { return _selectedChipEntries; }
		//	set
		//	{
		//		_selectedChipEntries = value;

		//		//var r =RecipientEntry.ConstructFakeEntry("test", true);
		//		//SubmitItem(r);
		//		foreach (var selectedContact in _selectedChipEntries)
		//		{
		//			//SubmitItem(selectedContact);
		//			//SubmitItem("test1", "test1");
		//			//SubmitItem("test2", "test2");

		//		}

		//	}
		//}

		//private void OnItemClick(object sender, AdapterView.ItemClickEventArgs itemClickEventArgs)
		//{
		//	OnItemClick(itemClickEventArgs.Position);
		//}

		//private void OnItemSelected(object sender, AdapterView.ItemSelectedEventArgs itemSelectedEventArgs)
		//{
		//	OnItemSelected(itemSelectedEventArgs.Position);
		//}

		//public override void OnItemClick(AdapterView p0, View p1, int p2, long p3)
		//{
		//	//var selectedObject = Adapter.GetRawItem(p2);
		//	//SelectedObject = selectedObject;

		//	base.OnItemClick(p0, p1, p2, p3);
		//}

		//protected virtual void OnItemClick(int position)
		//{
		//	var selectedObject = Adapter.GetRawItem(position);
		//	SelectedObject = selectedObject;
		//}

		//protected virtual void OnItemSelected(int position)
		//{
		//	var selectedObject = Adapter.GetRawItem(position);
		//	//SelectedObject = selectedObject;
		//}

		private void AdapterOnPartialTextChanged(object sender, EventArgs eventArgs)
		{
			FireChanged(PartialTextChanged);
		}

		//public event EventHandler SelectedObjectChanged;

		public event EventHandler PartialTextChanged;

		private void FireChanged(EventHandler eventHandler)
		{
			var handler = eventHandler;
			if (handler != null)
			{
				handler(this, EventArgs.Empty);
			}
		}
	}
}