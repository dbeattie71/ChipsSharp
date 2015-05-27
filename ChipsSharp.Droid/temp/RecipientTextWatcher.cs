using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using Android.App;
using Android.Content;
using Android.OS;
using Android.Runtime;
using Android.Text;
using Android.Views;
using Android.Widget;
using Java.Lang;

namespace com.dbeattie
{
	public class RecipientTextWatcher : ITextWatcher
	{
		public void Dispose()
		{
			throw new NotImplementedException();
		}

		public IntPtr Handle { get; private set; }

		public void AfterTextChanged(IEditable s)
		{
			throw new NotImplementedException();
		}

		public void BeforeTextChanged(ICharSequence s, int start, int count, int after)
		{
			throw new NotImplementedException();
		}

		public void OnTextChanged(ICharSequence s, int start, int before, int count)
		{
			throw new NotImplementedException();
		}
	}
}