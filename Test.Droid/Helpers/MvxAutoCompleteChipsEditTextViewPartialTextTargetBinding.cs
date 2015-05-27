using System;
using System.Reflection;
using Cirrious.CrossCore.Platform;
using Cirrious.MvvmCross.Binding;
using Cirrious.MvvmCross.Binding.Bindings.Target;

namespace Test.Droid.Helpers
{
	public class MvxAutoCompleteChipsEditTextViewPartialTextTargetBinding
		: MvxPropertyInfoTargetBinding<MvxChipsEditTextView>
	{
		private bool _subscribed;

		public MvxAutoCompleteChipsEditTextViewPartialTextTargetBinding(object target, PropertyInfo targetPropertyInfo)
			: base(target, targetPropertyInfo)
		{
			var autoComplete = View;
			if (autoComplete == null)
			{
				MvxBindingTrace.Trace(MvxTraceLevel.Error,
				                      "Error - autoComplete is null in MvxAutoCompleteTextViewPartialTextTargetBinding");
			}
		}

		public override MvxBindingMode DefaultMode
		{
			get { return MvxBindingMode.OneWayToSource; }
		}

		private void AutoCompleteOnPartialTextChanged(object sender, EventArgs eventArgs)
		{
			FireValueChanged(View.PartialText);
		}

		public override void SubscribeToEvents()
		{
			var autoComplete = View;
			if (autoComplete == null)
				return;

			_subscribed = true;
			autoComplete.PartialTextChanged += AutoCompleteOnPartialTextChanged;
		}

		protected override void Dispose(bool isDisposing)
		{
			base.Dispose(isDisposing);
			if (isDisposing)
			{
				var autoComplete = View;
				if (autoComplete != null && _subscribed)
				{
					autoComplete.PartialTextChanged -= AutoCompleteOnPartialTextChanged;
					_subscribed = false;
				}
			}
		}
	}
}