using System;
using System.Collections.Generic;
using System.Linq;
using Android.Content;
using Android.Content.Res;
using Android.Graphics;
using Android.Graphics.Drawables;
using Android.OS;
using Android.Runtime;
using Android.Text;
using Android.Text.Method;
using Android.Text.Style;
using Android.Text.Util;
using Android.Util;
using Android.Views;
using Android.Views.InputMethods;
using Android.Widget;
using com.android.ex.chips.Spans;
using ChipsSharp;
using Java.Lang;
using Math = Java.Lang.Math;
using Object = Java.Lang.Object;
using String = System.String;

namespace com.android.ex.chips
{
	[Register("com.android.ex.chips.ChipsEditTextView")]
	public partial class ChipsEditTextView : MultiAutoCompleteTextView,
		AdapterView.IOnItemClickListener,
		ActionMode.ICallback,
		//RecipientAlternatesAdapter.OnCheckedItemChangedListener,
		GestureDetector.IOnGestureListener,
		//AutoCompleteTextView.IOnDismissListener,
		View.IOnClickListener,
		TextView.IOnEditorActionListener
	{
		private const char CommitCharComma = ',';
		private const char CommitCharSemicolon = ';';
		private const char CommitCharSpace = ' ';

		//private static String SEPARATOR = new String(String.ValueOf(COMMIT_CHAR_COMMA) + String.ValueOf(COMMIT_CHAR_SPACE));

		//private static long DISMISS_DELAY = 300;

		// TODO: get correct number/ algorithm from with UX.
		// Visible for testing. /*package*/
		private const int ChipLimit = 2;

		//private static int MAX_CHIPS_PARSED = 50;

		private static int _selectedTextColor = -1;

		// Resources for displaying chips.
		private Drawable _chipBackground;

		private Drawable _chipDelete;

		private Drawable _invalidChipBackground;

		private Drawable _chipBackgroundPressed;

		private float _chipHeight;

		private float _chipFontSize;

		//private string _chipEntryErrorHint;

		//private string _chipOverLimitErrorHint;

		private float _lineSpacingExtra;

		private int _chipPadding;

		// Enumerator for avatar position. See attr.xml for more details. 0 for end, 1 for start.
		private int _avatarPosition;

		private const int AvatarPositionEnd = 0;

		private const int AvatarPositionStart = 1;

		// Enumerator for image span alignment. See attr.xml for more details. 0 for bottom, 1 for baseline.
		private SpanAlign _imageSpanAlignment;

		//private const int IMAGE_SPAN_ALIGNMENT_BOTTOM = 0;

		//private const int IMAGE_SPAN_ALIGNMENT_BASELINE = 1;

		private bool _disableDelete;

		private ITokenizer _tokenizer;

		private IValidator _validator; //TODO: need?

		private DrawableChipSpan _selectedChip;

		private Bitmap _defaultContactPhoto;

		public Bitmap NoAvatarPicture { get; protected set; }

		private ImageSpan _moreChip;

		private TextView _moreItem;

		private Handler _handler; //TODO: need for orientation change?

		private bool _noChips = false; //TODO: need append method?

		private readonly ListPopupWindow _alternatesPopup; //TODO: need?

		private readonly ListPopupWindow _addressPopup; //TODO: need?

		//private List<DrawableRecipientChip> mTemporaryRecipients; //TODO: need?

		private List<DrawableChipSpan> _removedSpans;

		private bool _shouldShrink = true;

		private readonly GestureDetector _gestureDetector;

		// Used with {@link #mAlternatesPopup}. Handles clicks to alternate addresses for a selected chip.
		private AdapterView.IOnItemClickListener _alternatesListener; //TODO: need?

		//private int _checkedItem; //TODO: probably don't need

		// Obtain the enclosing scroll view, if it exists, so that the view can be
		// scrolled to show the last line of chips content.
		private ScrollView _scrollView;  //TODO: need to handle scrolling

		private bool _triedGettingScrollView;

		private readonly Action _delayedShrink;

		private int _maxLines;

		private int _shrinkMaxLines = 1;

		private int _maxChipsAllowed = 99;

		private static int _excessTopPadding = -1;

		private int _actionBarHeight;

		private bool _attachedToWindow;

		private bool _dismissPopupOnClick = true;

		private ItemSelectedListener _itemSelectedListener;

		protected ChipsEditTextView(IntPtr javaReference, JniHandleOwnership transfer)
			: base(javaReference, transfer)
		{
		}

		public ChipsEditTextView(Context context, IAttributeSet attrs)
			: base(context, attrs)
		{
			TextChanged += OnTextChanged;
			AfterTextChanged += OnAfterTextChanged;

			_delayedShrink = Shrink;

			//// TODO: would be nice to show chips as an example here
			if (IsInEditMode)
			{
				return;
			}

			SetChipDimensions(context, attrs);
			if (_selectedTextColor == -1)
			{
				_selectedTextColor = context.Resources.GetColor(Android.Resource.Color.White);
			}

			_alternatesPopup = new ListPopupWindow(context);
			_addressPopup = new ListPopupWindow(context);
			
			InputType = InputType | InputTypes.TextFlagNoSuggestions;
			OnItemClickListener = this;
			CustomSelectionActionModeCallback = this;
			_gestureDetector = new GestureDetector(context, this);
			SetOnEditorActionListener(this);
		}

		protected override void OnDetachedFromWindow()
		{
			base.OnDetachedFromWindow();
			_attachedToWindow = false;
		}

		protected override void OnAttachedToWindow()
		{
			base.OnAttachedToWindow();
			_attachedToWindow = true;
		}

		public bool OnEditorAction(TextView v, ImeAction action, KeyEvent e)
		{
			if (action == ImeAction.Done)
			{
				//if (commitDefault())
				//{
				//	return true;
				//}
				if (_selectedChip != null)
				{
					ClearSelectedChip();
					return true;
				}
				else if (FocusNext())
				{
					return true;
				}
			}
			return false;
		}

		public override IInputConnection OnCreateInputConnection(EditorInfo outAttrs)
		{
			var connection = base.OnCreateInputConnection(outAttrs);
			//int imeActions = (int)outAttrs.ImeOptions& EditorInfo.IME_MASK_ACTION;
			int imeActions = (int)outAttrs.ImeOptions & (int)ImeAction.ImeMaskAction;

			if ((imeActions & (int)ImeAction.Done) != 0)
			{
				// clear the existing action
				outAttrs.ImeOptions ^= (ImeFlags)imeActions;
				// set the DONE action
				outAttrs.ImeOptions |= (ImeFlags)ImeAction.Done; // EditorInfo.IME_ACTION_DONE;
			}
			if ((outAttrs.ImeOptions & ImeFlags.NoEnterAction) != 0)
			{
				outAttrs.ImeOptions &= ~ImeFlags.NoEnterAction;
			}

			outAttrs.ActionId = (int)ImeAction.Done;
			//			outAttrs.ActionLabel = Context.GetString(Resource.String.done);
			//outAttrs.ActionLabel = new String(Context.GetString(Resource.String.done));
			outAttrs.ActionLabel = Context.GetString(Resource.String.done).ToJavaString();
			return connection;
		}

		protected override void OnSelectionChanged(int selStart, int selEnd)
		{
			// When selection changes, see if it is inside the chips area.
			// If so, move the cursor back after the chips again.
			DrawableChipSpan last = LastChip;
			if (last != null && selStart < Spannable.GetSpanEnd(last))
			{
				// Grab the last chip and set the cursor to after it.
				SetSelection(Math.Min(Spannable.GetSpanEnd(last) + 1, Text.Length));
			}
			base.OnSelectionChanged(selStart, selEnd);
		}

		public override IParcelable OnSaveInstanceState()
		{
			// If the user changes orientation while they are editing, just roll back the selection.
			ClearSelectedChip();

			if (!HasFocus)
			{
				Expand();
			}

			return base.OnSaveInstanceState();
		}

		public override void OnRestoreInstanceState(IParcelable state)
		{
			if (!TextUtils.IsEmpty(Text))
			{
				base.OnRestoreInstanceState(null);
			}
			else
			{
				base.OnRestoreInstanceState(state);
			}
		}

		/**
     * Convenience method: Append the specified text slice to the TextView's
     * display buffer, upgrading it to BufferType.EDITABLE if it was
     * not already editable. Commas are excluded as they are added automatically
     * by the view.
     */
		//public override void Append(ICharSequence text, int start, int end)
		//{
		//	// We don't care about watching text changes while appending.
		//	//if (mTextWatcher != null)
		//	//{
		//	//	RemoveTextChangedListener(mTextWatcher);
		//	//}
		//	AfterTextChanged -= OnAfterTextChanged;
		//	TextChanged -= OnTextChanged;

		//	base.Append(text, start, end);

		//	if (!TextUtils.IsEmpty(text) && TextUtils.GetTrimmedLength(text) > 0)
		//	{
		//		string displayString = text.ToString();

		//		if (!displayString.Trim().EndsWith(COMMIT_CHAR_COMMA.ToString()))
		//		{
		//			// We have no separator, so we should add it
		//			base.Append(SEPARATOR.ToString(), 0, SEPARATOR.Length());
		//			displayString += SEPARATOR;
		//		}

		//		if (!TextUtils.IsEmpty(displayString)
		//			&& TextUtils.GetTrimmedLength(displayString) > 0)
		//		{
		//			mPendingChipsCount++;
		//			mPendingChips.Add(new String(displayString));
		//		}
		//	}
		//	// Put a message on the queue to make sure we ALWAYS handle pending
		//	// chips.
		//	if (mPendingChipsCount > 0)
		//	{
		//		postHandlePendingChips();
		//	}

		//	TextChanged += OnTextChanged;
		//	AfterTextChanged += OnAfterTextChanged;
		//	//mHandler.Post(mAddTextWatcher);
		//}

		protected override void OnFocusChanged(bool gainFocus, FocusSearchDirection direction, Rect previouslyFocusedRect)
		{
			base.OnFocusChanged(gainFocus, direction, previouslyFocusedRect);
			if (!HasFocus)
			{
				Shrink();
			}
			else
			{
				Expand();
			}
		}

		private int GetExcessTopPadding()
		{
			if (_excessTopPadding == -1)
			{
				_excessTopPadding = (int)(_chipHeight + _lineSpacingExtra);
			}
			return _excessTopPadding;
		}

		//public <T extends ListAdapter & Filterable> void setAdapter(T adapter) {
		//	super.setAdapter(adapter);
		//	BaseRecipientAdapter baseAdapter = (BaseRecipientAdapter) adapter;
		//	baseAdapter.registerUpdateObserver(new BaseRecipientAdapter.EntriesUpdatedObserver() {

		//		public void onChanged(List<RecipientEntry> entries) {
		//			// Scroll the chips field to the top of the screen so
		//			// that the user can see as many results as possible.
		//			if (entries != null && entries.size() > 0) {
		//				scrollBottomIntoView();
		//			}
		//		}
		//	});
		//	baseAdapter.setDropdownChipLayouter(mDropdownChipLayouter);
		//}

		public override IListAdapter Adapter
		{
			get
			{
				return base.Adapter;
			}
			set
			{
				base.Adapter = value;
				//if (entries != null && entries.size() > 0) //TODO: need to do this
				//{
				//	scrollBottomIntoView();
				//}
			}
		}

		protected void ScrollBottomIntoView()
		{
			if (_scrollView != null && _shouldShrink)
			{
				int[] location = new int[2];
				GetLocationOnScreen(location);
				int height = Height;
				int currentPos = location[1] + height;
				// Desired position shows at least 1 line of chips below the action
				// bar. We add excess padding to make sure this is always below other
				// content.
				int desiredPos = (int)_chipHeight + _actionBarHeight + GetExcessTopPadding();
				if (currentPos > desiredPos)
				{
					_scrollView.ScrollBy(0, currentPos - desiredPos);
				}
			}
		}

		protected ScrollView GetScrollView()
		{
			return _scrollView;
		}


		public void performValidation()
		{
			// Do nothing. Chips handles its own validation.
		}

		private void Shrink()
		{
			if (_tokenizer == null)
			{
				return;
			}

			//long contactId = mSelectedChip != null ? mSelectedChip.getEntry().getContactId() : -1;

			// Clear error message before shrinking
			//SetError(null);

			//if (mSelectedChip != null && contactId != RecipientEntry.INVALID_CONTACT)
			//{
			//	clearSelectedChip();
			//}

			//if (mSelectedChip != null && contactId != RecipientEntry.INVALID_CONTACT
			//	&& (!isPhoneQuery() && contactId != RecipientEntry.GENERATED_CONTACT))
			//{

			//	clearSelectedChip();
			//}
			//else
			//{
			if (Width <= 0)
			{
				// We don't have the width yet which means the view hasn't been drawn yet
				// and there is no reason to attempt to commit chips yet.
				// This focus lost must be the result of an orientation change
				// or an initial rendering.
				// Re-post the shrink for later.
				_handler.RemoveCallbacks(_delayedShrink);
				_handler.Post(_delayedShrink);
				return;
			}
			// Reset any pending chips as they would have been handled
			// when the field lost focus.
			//if (mPendingChipsCount > 0)
			//{
			//	postHandlePendingChips();
			//}
			//else
			//{
			IEditable editable = EditableText;
			int end = SelectionEnd;
			int start = _tokenizer.FindTokenStart(editable, end);
			var chips = Spannable.GetSpans(start, end, Class.FromType(typeof(DrawableChipSpan))).Cast<DrawableChipSpan>().ToArray();
			if ((chips.Length == 0))
			{
				IEditable text = EditableText;
				int whatEnd = _tokenizer.FindTokenEnd(text, start);
				// This token was already tokenized, so skip past the ending token.
				if (whatEnd < text.Length() && text.CharAt(whatEnd) == ',')
				{
					whatEnd = MovePastTerminators(whatEnd);
				}

				// In the middle of chip; treat this as an edit
				// and commit the whole token if it is not only spaces
				bool isOverMaxNumberOfChips = AllChipSpans.Length >= _maxChipsAllowed;
				//if (!isOverMaxNumberOfChips && !editable.SubSequence(start, end).Trim().isEmpty())
				//if (!isOverMaxNumberOfChips && !string.IsNullOrEmpty(editable.SubSequence(start, end).Trim()))
				//{
				//	int selEnd = SelectionEnd;
				//	if (whatEnd != selEnd)
				//	{
				//		handleEdit(start, whatEnd);
				//	}
				//	else
				//	{
				//		commitChip(start, end, editable);
				//	}
				//}
			}
			else
			{
				editable.Delete(start, end);
			}
			//}

			//mHandler.Post(mAddTextWatcher);

			//}
			CreateMoreChip();
		}

		private void Expand()
		{
			if (_shouldShrink)
			{
				SetMaxLines(Integer.MaxValue);
			}
			RemoveMoreChip();
			SetCursorVisible(true);
			IEditable text = EditableText;
			SetSelection(text != null && text.Length() > 0 ? text.Length() : 0);
			// If there are any temporary chips, try replacing them now that the user
			// has expanded the field.
			//if (mTemporaryRecipients != null && mTemporaryRecipients.Count > 0)
			//{
			//	//new RecipientReplacementTask().Execute();
			//	mTemporaryRecipients = null;
			//}
		}

		//private ICharSequence EllipsizeText(ICharSequence text, TextPaint paint, float maxWidth)
		private string EllipsizeText(string text, TextPaint paint, float maxWidth)
		{
			paint.TextSize = _chipFontSize;
			//if (maxWidth <= 0 && Log.isLoggable(TAG, Log.DEBUG))
			//{
			//	Log.d(TAG, "Max width is negative: " + maxWidth);
			//}

			return TextUtils.EllipsizeFormatted(text.ToJavaString(), paint, maxWidth, TextUtils.TruncateAt.End).ToString();
		}

		/**
     * Calculate the bottom of the line the chip will be located on using:
     * 1) which line the chip appears on
     * 2) the height of a chip
     * 3) padding built into the edit text view
     */
		private int CalculateOffsetFromBottom(int line)
		{
			// Line offsets start at zero.
			int actualLine = LineCount - (line + 1);
			return -((actualLine * ((int)_chipHeight) + PaddingBottom) + PaddingTop)
				   + DropDownVerticalOffset;
		}

		/**
     * Get the max amount of space a chip can take up. The formula takes into
     * account the width of the EditTextView, any view padding, and padding
     * that will be added to the chip.
     */
		private float CalculateAvailableWidth()
		{
			return Width - PaddingLeft - PaddingRight - (_chipPadding * 2);
		}

		private void SetChipDimensions(Context context, IAttributeSet attrs)
		{
			TypedArray a = context.ObtainStyledAttributes(attrs, Resource.Styleable.RecipientEditTextView, 0, 0);
			Resources r = Context.Resources;

			_chipBackground = a.GetDrawable(Resource.Styleable.RecipientEditTextView_chipBackground);
			if (_chipBackground == null)
			{
				_chipBackground = r.GetDrawable(Resource.Drawable.chip_background);
			}
			_chipBackgroundPressed = a.GetDrawable(Resource.Styleable.RecipientEditTextView_chipBackgroundPressed);
			if (_chipBackgroundPressed == null)
			{
				_chipBackgroundPressed = r.GetDrawable(Resource.Drawable.chip_background_selected);
			}
			_chipDelete = a.GetDrawable(Resource.Styleable.RecipientEditTextView_chipDelete);
			if (_chipDelete == null)
			{
				_chipDelete = r.GetDrawable(Resource.Drawable.chip_delete);
			}
			_chipPadding = a.GetDimensionPixelSize(Resource.Styleable.RecipientEditTextView_chipPadding, -1);
			if (_chipPadding == -1)
			{
				_chipPadding = (int)r.GetDimension(Resource.Dimension.chip_padding);
			}

			_defaultContactPhoto = BitmapFactory.DecodeResource(r, Resource.Drawable.ic_contact_picture);

			NoAvatarPicture = BitmapFactory.DecodeResource(r, Resource.Drawable.no_avatar_picture);

			_moreItem = (TextView)LayoutInflater.From(Context).Inflate(Resource.Layout.more_item, null);

			_chipHeight = a.GetDimensionPixelSize(Resource.Styleable.RecipientEditTextView_chipHeight, -1);
			if (_chipHeight == -1)
			{
				_chipHeight = r.GetDimension(Resource.Dimension.chip_height);
			}
			_chipFontSize = a.GetDimensionPixelSize(Resource.Styleable.RecipientEditTextView_chipFontSize, -1);
			if (_chipFontSize == -1)
			{
				_chipFontSize = r.GetDimension(Resource.Dimension.chip_text_size);
			}

			//mChipEntryErrorHint = new String(a.GetString(Resource.Styleable.RecipientEditTextView_chipEntryErrorHint));
			////Log.e(TAG, "" + mChipEntryErrorHint);
			//if (mChipEntryErrorHint == null || mChipEntryErrorHint.IsEmpty)
			//{
			//	mChipEntryErrorHint = new String(context.GetString(Resource.String.error_invalid_chips));
			//}

			//mChipOverLimitErrorHint = new String(a.GetString(Resource.Styleable.RecipientEditTextView_chipOverLimitErrorHint));
			//if (mChipOverLimitErrorHint == null || mChipOverLimitErrorHint.IsEmpty)
			//{
			//	mChipOverLimitErrorHint = new String(context.GetString(Resource.String.error_over_chips_limit));
			//}

			_invalidChipBackground = a.GetDrawable(Resource.Styleable.RecipientEditTextView_invalidChipBackground);
			if (_invalidChipBackground == null)
			{
				_invalidChipBackground = r.GetDrawable(Resource.Drawable.chip_background_invalid);
			}

			_avatarPosition = a.GetInt(Resource.Styleable.RecipientEditTextView_avatarPosition, 1);
			_imageSpanAlignment = (SpanAlign)a.GetInt(Resource.Styleable.RecipientEditTextView_imageSpanAlignment, 0);
			_disableDelete = a.GetBoolean(Resource.Styleable.RecipientEditTextView_disableDelete, false);

			_lineSpacingExtra = r.GetDimension(Resource.Dimension.line_spacing_extra);
			_maxLines = r.GetInteger(Resource.Integer.chips_max_lines);
			TypedValue tv = new TypedValue();
			if (context.Theme.ResolveAttribute(Android.Resource.Attribute.ActionBarSize, tv, true))
			{
				_actionBarHeight = TypedValue.ComplexToDimensionPixelSize(tv.Data, Resources.DisplayMetrics);
			}

			a.Recycle();
		}

		private void SetMoreItem(TextView moreItem)
		{
			_moreItem = moreItem;
		}


		//private void setChipBackground(Drawable chipBackground)
		//{
		//	ChipBackground = chipBackground;
		//}

		//private Drawable getChipBackground(IChipEntry contact)
		//{
		//	//return contact.isValid() ? mChipBackground : mInvalidChipBackground;
		//	return ChipBackground;
		//}

		//protected void setChipHeight(int height)
		//{
		//	_chipHeight = height;
		//}

		//public float getChipHeight()
		//{
		//	return _chipHeight;
		//}

		//public void setMaxNumberOfChipsAllowed(int numberOfChipsAllowed)
		//{
		//	mMaxChipsAllowed = numberOfChipsAllowed;
		//}

		//public int getMaxNumberOfChipsAllowed()
		//{
		//	return mMaxChipsAllowed;
		//}

		//public int getShrinkMaxLines()
		//{
		//	return mShrinkMaxLines;
		//}

		//public void setShrinkMaxLines(int shrinkMaxLines)
		//{
		//	mShrinkMaxLines = shrinkMaxLines;
		//}

		/**
     * Set whether to shrink the recipients field such that at most
     * one line of recipients chips are shown when the field loses
     * focus. By default, the number of displayed recipients will be
     * limited and a "more" chip will be shown when focus is lost.
     * @param shrink
     */
		public void SetOnFocusListShrinkRecipients(bool shrink)
		{
			_shouldShrink = shrink;
		}

		protected override void OnSizeChanged(int w, int h, int oldw, int oldh)
		{
			base.OnSizeChanged(w, h, oldw, oldh);
			//super.onSizeChanged(width, height, oldw, oldh);
			if (Width != 0 && Height != 0)
			{
				//if (mPendingChipsCount > 0)
				//{
				//	postHandlePendingChips();
				//}
				//else
				//{
				CheckChipWidths();
				//}
			}
			// Try to find the scroll view parent, if it exists.
			if (_scrollView == null && !_triedGettingScrollView)
			{
				IViewParent parent = Parent;
				while (parent != null && !(parent is ScrollView))
				{
					parent = parent.Parent;
				}
				if (parent != null)
				{
					_scrollView = (ScrollView)parent;
				}
				_triedGettingScrollView = true;
			}
		}

		//private int getViewWidth()
		//{
		//	return Width;
		//}

		// VisibleForTesting
		protected IChipEntry CreateTokenizedEntry(String token)
		{
			if (TextUtils.IsEmpty(token))
			{
				return null;
			}

			//if (isPhoneQuery() && isPhoneNumber(token))
			//{
			//	return RecipientEntry.constructFakePhoneEntry(token, true);
			//}

			Rfc822Token[] tokens = Rfc822Tokenizer.Tokenize(token);
			String display;
			bool isValid = IsValid(token);
			if (isValid && tokens != null && tokens.Length > 0)
			{
				// If we can get a name from tokenizing, then generate an entry from
				// this.
				//display = new String(tokens[0].Name);
				display = tokens[0].Name;
				if (!TextUtils.IsEmpty(display))
				{
					//return RecipientEntry.constructGeneratedEntry(display, new String(tokens[0].Address), isValid);
				}
				else
				{
					//display = new String(tokens[0].Address);
					display = tokens[0].Address;
					if (!TextUtils.IsEmpty(display))
					{
						//return RecipientEntry.constructFakeEntry(display, true);
					}
				}
			}
			// Unable to validate the token or to create a valid token from it.
			// Just create a chip the user can edit.
			String validatedToken = null;
			if (_validator != null && !isValid)
			{
				// Try fixing up the entry using the validator.
				var foo = new Java.Lang.String(token);
				//validatedToken = new String(mValidator.FixTextFormatted(token).ToString());
				validatedToken = _validator.FixTextFormatted(new Java.Lang.String(token)).ToString();
				if (!TextUtils.IsEmpty(validatedToken))
				{
					if (validatedToken.Contains(token))
					{
						// protect against the case of a validator with a null
						// domain,
						// which doesn't add a domain to the token
						Rfc822Token[] tokenized = Rfc822Tokenizer.Tokenize(validatedToken);
						if (tokenized.Length > 0)
						{
							//validatedToken = new String(tokenized[0].Address);
							validatedToken = tokenized[0].Address;
							isValid = true;
						}
					}
					else
					{
						// We ran into a case where the token was invalid and
						// removed
						// by the validator. In this case, just use the original
						// token
						// and let the user sort out the error chip.
						validatedToken = null;
						isValid = false;
					}
				}
			}
			// Otherwise, fallback to just creating an editable email address chip.
			//return RecipientEntry.constructFakeEntry(!TextUtils.IsEmpty(validatedToken) ? validatedToken : token, isValid);
			return null;
		}

		private bool IsValid(String text)
		{
			return _validator == null || _validator.IsValid(text);
		}

		//private static String tokenizeAddress(String destination)
		//{
		//	Rfc822Token[] tokens = Rfc822Tokenizer.Tokenize(destination);
		//	if (tokens != null && tokens.Length > 0)
		//	{
		//		return new String(tokens[0].Address);
		//	}
		//	return destination;
		//}

		public override void SetTokenizer(ITokenizer t)
		{
			_tokenizer = t;
			base.SetTokenizer(t);
		}

		public override IValidator Validator { get; set; }

		/**
     * We cannot use the default mechanism for replaceText. Instead,
     * we override onItemClickListener so we can get all the associated
     * contact information including display text, address, and id.
     */
		protected void replaceText(ICharSequence text)
		{
			return;
		}

		/**
     * Dismiss any selected chips when the back key is pressed.
     */
		public override bool OnKeyPreIme(Keycode keyCode, KeyEvent @event)
		{
			if (keyCode == Keycode.Back && _selectedChip != null)
			{
				ClearSelectedChip();
				return true;
			}
			return base.OnKeyPreIme(keyCode, @event);
		}

		/**
     * Monitor key presses in this view to see if the user types
     * any commit keys, which consist of ENTER, TAB, or DPAD_CENTER.
     * If the user has entered text that has contact matches and types
     * a commit key, create a chip from the topmost matching contact.
     * If the user has entered text that has no contact matches and types
     * a commit key, then create a chip from the text they have entered.
     */

		//    @Override
		public override bool OnKeyUp(Keycode keyCode, KeyEvent @event)
		{
			switch (keyCode)
			{
				case Keycode.Tab:
					if (@event.HasNoModifiers)
					{
						if (_selectedChip != null)
						{
							ClearSelectedChip();
						}
						else
						{
							//commitDefault();
						}
					}
					break;
			}
			return base.OnKeyUp(keyCode, @event);
		}

		private bool FocusNext()
		{
			View next = FocusSearch(FocusSearchDirection.Down);
			if (next != null)
			{
				next.RequestFocus();
				return true;
			}
			return false;
		}

		/**
     * Create a chip from the default selection. If the popup is showing, the
     * default is the first item in the popup suggestions list. Otherwise, it is
     * whatever the user had typed in. End represents where the the tokenizer
     * should search for a token to turn into a chip.
     * @return If a chip was created from a real contact.
     */
		//private bool commitDefault()
		//{
		//	//// If there is no tokenizer, don't try to commit.
		//	//if (_tokenizer == null)
		//	//{
		//	//	return false;
		//	//}
		//	//IEditable editable = EditableText;
		//	//int end = SelectionEnd;
		//	//int start = _tokenizer.FindTokenStart(editable, end);

		//	//if (shouldCreateChip(start, end))
		//	//{
		//	//	int whatEnd = _tokenizer.FindTokenEnd(Text, start);
		//	//	// In the middle of chip; treat this as an edit
		//	//	// and commit the whole token.
		//	//	whatEnd = movePastTerminators(whatEnd);
		//	//	if (whatEnd != SelectionEnd)
		//	//	{
		//	//		handleEdit(start, whatEnd);
		//	//		return true;
		//	//	}
		//	//	return commitChip(start, end, editable);
		//	//}
		//	return false;
		//}

		private void CommitByCharacter()
		{
			// We can't possibly commit by character if we can't tokenize.
			if (_tokenizer == null)
			{
				return;
			}

			RemoveCommitCharBeforeCreatingChip();

			IEditable editable = EditableText;
			int end = SelectionEnd;
			int start = _tokenizer.FindTokenStart(editable, end);

			if (ShouldCreateChip(start, end))
			{
				// TODO: Validate that there are no bugs when checking if the user can add a chip here
				if (!EnoughRoomForAdditionalChip)
				{
					//SetError(mChipOverLimitErrorHint);
				}
				else
				{
					//SetError(null);
					CommitChip(start, end, editable);
				}
			}
			else
			{
				//SetError(mChipEntryErrorHint);
			}
			SetSelection(Text.Length);
		}

		protected bool CommitChip(int start, int end, IEditable editable)
		{
			// Check if there is not already too many chips
			if (AllChipSpans.Length >= _maxChipsAllowed)
			{
				return false;
			}

			//IListAdapter adapter = getAdapter();
			//IListAdapter adapter = null;

			// TODO: is it worth disabling the autocompletion if it is a phone query ? This is
			// potentially annoying when user types numbers (the prefix might be the same for many
			// people), but when entering the name, this is definitely a nice feature.
			// TODO: Can be disabled when there are no letters in the text.
			bool tempSCPhoneQuery = true;
			//if (adapter != null && adapter.Count > 0 && EnoughToFilter() && end == SelectionEnd && (!isPhoneQuery() || tempSCPhoneQuery))
			if (Adapter != null && Adapter.Count > 0 && EnoughToFilter() && end == SelectionEnd)
			{
				// Choose the first entry.
				SubmitItemAtPosition(0);
				DismissDropDown();
				return true;
			}
			else
			{
				// TODO: this commented line is a test. It seems to work fine so far.
				//int tokenEnd = mTokenizer.findTokenEnd(editable, start);
				int tokenEnd = end;
				if (editable.Length() > tokenEnd + 1)
				{
					char charAt = editable.CharAt(tokenEnd + 1);
					if (charAt == CommitCharComma || charAt == CommitCharSemicolon)
					{
						tokenEnd++;
					}
				}

				//String text = new String(editable.ToString().Substring(start, tokenEnd).Trim());
				//			String text = new String(editable.ToString().SubString(start, tokenEnd).Trim());
				String text = editable.ToString().JavaSubstring(start, tokenEnd).Trim();

				ClearComposingText();
				if (text != null && text.Length > 0)
				{
					IChipEntry entry = CreateTokenizedEntry(text);
					if (entry != null)
					{
						QwertyKeyListener.MarkAsReplaced(editable, start, end, text);
						ICharSequence chipText = CreateChip(entry, false);
						if (chipText != null && start > -1 && end > -1)
						{
							editable.Replace(start, end, chipText);
						}
					}
					// Only dismiss the dropdown if it is related to the text we
					// just committed.
					// For paste, it may not be as there are possibly multiple
					// tokens being added.
					if (end == SelectionEnd)
					{
						DismissDropDown();
					}
					SanitizeBetween();
					return true;
				}
			}
			return false;
		}

		// Visible for testing.
		/* package */

		private void SanitizeBetween()
		{
			// Don't sanitize while we are waiting for content to chipify.
			//if (mPendingChipsCount > 0)
			//{
			//	return;
			//}
			// Find the last chip.
			//DrawableChipSpan[] chipSpans = getSortedVisibleRecipients();
			DrawableChipSpan[] chipSpans = AllChipSpans;
			if (chipSpans != null && chipSpans.Length > 0)
			{
				DrawableChipSpan last = chipSpans[chipSpans.Length - 1];
				DrawableChipSpan beforeLast = null;
				if (chipSpans.Length > 1)
				{
					beforeLast = chipSpans[chipSpans.Length - 2];
				}
				int startLooking = 0;
				int end = Spannable.GetSpanStart((Object)last);
				if (beforeLast != null)
				{
					startLooking = Spannable.GetSpanEnd((Object)beforeLast);
					IEditable text = EditableText;
					if (startLooking == -1 || startLooking > text.Length() - 1)
					{
						// There is nothing after this chip.
						return;
					}
					if (text.CharAt(startLooking) == ' ')
					{
						startLooking++;
					}
				}
				if (startLooking >= 0 && end >= 0 && startLooking < end)
				{
					EditableText.Delete(startLooking, end);
				}
			}
		}

		private bool ShouldCreateChip(int start, int end)
		{
			return !_noChips && HasFocus && EnoughToFilter() && !AlreadyHasChip(start, end) && !NotEnoughCharactersWhenTrimmed(start, end);
		}

		private bool NotEnoughCharactersWhenTrimmed(int start, int end)
		{
			return Text.JavaSubstring(start, end).Trim().Length < Threshold;
		}

		private bool AlreadyHasChip(int start, int end)
		{
			if (_noChips)
			{
				return true;
			}
			var chips = Spannable.GetSpans(start, end, Class.FromType(typeof(DrawableChipSpan))).Cast<DrawableChipSpan>().ToArray();
			if ((chips.Length == 0))
			{
				return false;
			}
			return true;
		}

		//private void handleEdit(int start, int end)
		//{
		//	if (start == -1 || end == -1)
		//	{
		//		// This chip no longer exists in the field.
		//		DismissDropDown();
		//		return;
		//	}
		//	// This is in the middle of a chip, so select out the whole chip
		//	// and commit it.
		//	IEditable editable = EditableText;
		//	SetSelection(end);
		//	String text = new String(Text.ToString().Substring(start, end));
		//	if (!TextUtils.IsEmpty(text))
		//	{
		//		RecipientEntry entry = RecipientEntry.constructFakeEntry(text, IsValid(text));
		//		QwertyKeyListener.MarkAsReplaced(editable, start, end, text.ToString());
		//		ICharSequence chipText = createChip(entry, false);
		//		int selEnd = SelectionEnd;
		//		if (chipText != null && start > -1 && selEnd > -1)
		//		{
		//			editable.Replace(start, selEnd, chipText);
		//		}
		//	}
		//	DismissDropDown();
		//}

		/**
     * If there is a selected chip, delegate the key events
     * to the selected chip.
     */
		public override bool OnKeyDown(Keycode keyCode, KeyEvent @event)
		{
			if (_selectedChip != null && keyCode == Keycode.Del)
			{
				if (_alternatesPopup != null && _alternatesPopup.IsShowing)
				{
					_alternatesPopup.Dismiss();
				}
				RemoveChip(_selectedChip);
				return true;
			}

			switch (keyCode)
			{
				case Keycode.Enter:
				case Keycode.DpadCenter:
					if (@event.HasNoModifiers)
					{
						//if (commitDefault())
						//{
						//	return true;
						//}
						if (_selectedChip != null)
						{
							ClearSelectedChip();
							return true;
						}
						else if (FocusNext())
						{
							return true;
						}
					}
					break;
			}

			return base.OnKeyDown(keyCode, @event);
		}

		//internal ISpannable Spannable()
		//{
		//	return (ISpannable)TextFormatted;
		//}

		

		/**
     * Instead of filtering on the entire contents of the edit box,
     * this subclass method filters on the range from
     * {@link Tokenizer#findTokenStart} to {@link #getSelectionEnd}
     * if the length of that range meets or exceeds {@link #getThreshold}
     * and makes sure that the range is not already a Chip.
     */
		//    @Override
		protected override void PerformFiltering(ICharSequence text, int keyCode)
		{
			//	// Do not filter if the user cannot add additional chips
			if (AllChipSpans.Length >= _maxChipsAllowed)
			{
				return;
			}

			bool isCompletedToken = IsCompletedToken(text);
			if (EnoughToFilter() && !isCompletedToken)
			{
				int end = SelectionEnd;
				int start = _tokenizer.FindTokenStart(text, end);

				// If it does not contain at least two non-blank chars, does not filter
				if (NotEnoughCharactersWhenTrimmed(start, end))
				{
					return;
				}

				// If this is a RecipientChip, don't filter
				// on its contents.
				ISpannable span = Spannable;
				var chips = span.GetSpans(start, end, Class.FromType(typeof(DrawableChipSpan))).Cast<DrawableChipSpan>().ToArray();
				if (chips.Length > 0)
				{
					DismissDropDown();
					return;
				}
			}
			else if (isCompletedToken)
			{
				DismissDropDown();
				return;
			}

			base.PerformFiltering(text, keyCode);
		}


		private bool IsCompletedToken(ICharSequence text)
		{
			if (TextUtils.IsEmpty(text))
			{
				return false;
			}
			// Check to see if this is a completed token before filtering.
			int end = text.Length();
			int start = _tokenizer.FindTokenStart(text, end);
			//String token = new String(text.ToString().Substring(start, end).Trim());
			//String token = new String(text.ToString().SubString(start, end).Trim());
			String token = text.ToString().JavaSubstring(start, end).Trim();
			if (!TextUtils.IsEmpty(token))
			{
				//char atEnd = token.CharAt(token.Length() - 1);
				char atEnd = token.ElementAt(token.Length - 1);
				return atEnd == CommitCharComma || atEnd == CommitCharSemicolon;
			}
			return false;
		}

		public bool IgnoreTouchEvents { get; set; }

		/**
	* Monitor touch events in the RecipientEditTextView.
	* If the view does not have focus, any tap on the view
	* will just focus the view. If the view has focus, determine
	* if the touch target is a recipient chip. If it is and the chip
	* is not selected, select it and clear any other selected chips.
	* If it isn't, then select that chip.
	*/
		public override bool OnTouchEvent(MotionEvent @event)
		{
			if (!IsFocused)
			{
				// Ignore any chip taps until this view is focused.
				return base.OnTouchEvent(@event);
			}

			if (IgnoreTouchEvents)
				return true;

			bool handled = base.OnTouchEvent(@event);
			MotionEventActions action = @event.Action;
			bool chipWasSelected = false;
			if (_selectedChip == null)
			{
				_gestureDetector.OnTouchEvent(@event);
			}
			//if (mCopyAddress == null && action ==   MotionEventActions.Up) {
			if (action == MotionEventActions.Up)
			{
				float x = @event.GetX();
				float y = @event.GetY();
				int offset = putOffsetInRange(x, y);
				DrawableChipSpan currentChipSpan = FindChip(offset);
				if (currentChipSpan != null)
				{
					if (action == MotionEventActions.Up)
					{
						if (_selectedChip != null && _selectedChip != currentChipSpan)
						{
							ClearSelectedChip();
							_selectedChip = SelectChip(currentChipSpan);
						}
						else if (_selectedChip == null)
						{
							SetSelection(Text.Length);
							//commitDefault();
							_selectedChip = SelectChip(currentChipSpan);
						}
						else
						{
							onClick(_selectedChip, offset, x, y);
						}
					}
					chipWasSelected = true;
					handled = true;
				}
				else if (_selectedChip != null && ShouldShowEditableText(_selectedChip))
				{
					chipWasSelected = true;
				}
			}
			if (action == MotionEventActions.Up && !chipWasSelected)
			{
				ClearSelectedChip();
			}
			return handled;
		}

		private void ScrollLineIntoView(int line)
		{
			if (_scrollView != null)
			{
				_scrollView.SmoothScrollBy(0, CalculateOffsetFromBottom(line));
			}
		}

		
		private IListAdapter CreateSingleAddressAdapter(DrawableChipSpan currentChipSpan)
		{
			return new SingleRecipientArrayAdapter(Context, 0);
			//return new SingleRecipientArrayAdapter(getContext(), currentChip.getEntry(),
			//									   mDropdownChipLayouter);
			return null;
		}

		
		private int putOffsetInRange(float x, float y)
		{
			int offset;

			if (Build.VERSION.SdkInt >= BuildVersionCodes.IceCreamSandwich)
			{
				offset = GetOffsetForPosition(x, y);
			}
			else
			{
				offset = SupportGetOffsetForPosition(x, y);
			}

			return putOffsetInRange(offset);
		}

		// TODO: This algorithm will need a lot of tweaking after more people have used
		// the chips ui. This attempts to be "forgiving" to fat finger touches by favoring
		// what comes before the finger.
		private int putOffsetInRange(int o)
		{
			int offset = o;
			IEditable text = EditableText;
			int length = text.Length();
			// Remove whitespace from end to find "real end"
			int realLength = length;
			for (int i = length - 1; i >= 0; i--)
			{
				if (text.CharAt(i) == ' ')
				{
					realLength--;
				}
				else
				{
					break;
				}
			}

			// If the offset is beyond or at the end of the text,
			// leave it alone.
			if (offset >= realLength)
			{
				return offset;
			}
			IEditable editable = EditableText;
			while (offset >= 0 && FindText(editable, offset) == -1 && FindChip(offset) == null)
			{
				// Keep walking backward!
				offset--;
			}
			return offset;
		}

		private static int FindText(IEditable text, int offset)
		{
			if (text.CharAt(offset) != ' ')
			{
				return offset;
			}
			return -1;
		}

		/**
     * When an item in the suggestions list has been clicked, create a chip from the
     * contact information of the selected item.
     */
		public virtual void OnItemClick(AdapterView parent, View view, int position, long id)
		{
			if (position < 0)
			{
				return;
			}
			SubmitItemAtPosition(position);

			if (_itemSelectedListener != null)
			{
				_itemSelectedListener.onItemSelected();
			}
		}

		private void SubmitItemAtPosition(int position)
		{
			//var entry = createValidatedEntry((ChipEntry)Adapter.GetItem(position));
			Object e= Adapter.GetItem(position);
			
			var entry = ((IChipEntry)Adapter.GetItem(position));
			SubmitItem(entry);
			//submitItem(new String("SomeUserName"), new String("Number"));
		}

		protected void SubmitItem(IChipEntry entry)
		{
			if (entry == null || AllChipSpans.Length >= _maxChipsAllowed)
			{
				//SetError(mChipOverLimitErrorHint);
				return;
			}
			ClearComposingText();

			int end = SelectionEnd;
			int start = _tokenizer.FindTokenStart(Text, end);

			IEditable editable = EditableText;
			QwertyKeyListener.MarkAsReplaced(editable, start, end, editable.SubSequence(start, end));
			ICharSequence chip = CreateChip(entry, false);
			if (chip != null && start >= 0 && end >= 0)
			{
				editable.Replace(start, end, chip);
			}
			SanitizeBetween();
		}

		//private ChipEntry createValidatedEntry(ChipEntry item)
		//{
		//	if (item == null)
		//	{
		//		return null;
		//	}
		//	RecipientEntry entry;
		//	// If the display name and the address are the same, or if this is a
		//	// valid contact, but the destination is invalid, then make this a fake
		//	// recipient that is editable.
		//	String destination = item.getDestination();
		//	if (!isPhoneQuery() && item.getContactId() == RecipientEntry.GENERATED_CONTACT)
		//	{
		//		entry = RecipientEntry.constructGeneratedEntry(item.getDisplayName(),
		//													   destination, item.isValid());
		//	}
		//	else if (RecipientEntry.isCreatedRecipient(item.getContactId())
		//			 && (TextUtils.IsEmpty(item.getDisplayName())
		//				 || TextUtils.Equals(item.getDisplayName(), destination)
		//				 || (mValidator != null && !mValidator.IsValid(destination))))
		//	{
		//		entry = RecipientEntry.constructFakeEntry(destination, item.isValid());
		//	}
		//	else
		//	{
		//		entry = item;
		//	}
		//	return entry;
		//}

		//@Override
		public bool OnActionItemClicked(ActionMode mode, IMenuItem item)
		{
			return false;
			//throw new Settings.System.NotImplementedException();
		}

		//@Override
		public void OnDestroyActionMode(ActionMode mode)
		{
		}

		//@Override
		public bool OnPrepareActionMode(ActionMode mode, IMenu menu)
		{
			return false;
		}

		public bool OnCreateActionMode(ActionMode mode, IMenu menu)
		{
			return false;
		}

		private int CountTokens(IEditable text)
		{
			int tokenCount = 0;
			int start = 0;
			while (start < text.Length())
			{
				start = MovePastTerminators(_tokenizer.FindTokenEnd(text, start));
				tokenCount++;
				if (start >= text.Length())
				{
					break;
				}
			}
			return tokenCount;
		}

	protected bool ShouldShowEditableText(DrawableChipSpan currentChipSpan)
		{
			return false;
			//long contactId = currentChip.getContactId();
			//return contactId == RecipientEntry.INVALID_CONTACT || (!isPhoneQuery() && contactId == RecipientEntry.GENERATED_CONTACT);
		}

		private void ShowAddress(DrawableChipSpan currentChipSpan, ListPopupWindow popup,
								 int width)
		{
			if (!_attachedToWindow)
			{
				return;
			}
			int line = Layout.GetLineForOffset(GetChipStart(currentChipSpan));
			int bottom = CalculateOffsetFromBottom(line);
			// Align the alternates popup with the left side of the View,
			// regardless of the position of the chip tapped.
			popup.Width = width;
			popup.AnchorView = this;
			popup.VerticalOffset = bottom;
			popup.SetAdapter(CreateSingleAddressAdapter(currentChipSpan));
			//	popup.SetOnItemClickListener(new AdapterView.IOnItemClickListener()
			//	{

			//	public void onItemClick(AdapterView<?> parent,
			//		View view,
			//		int position,
			//		long id) {
			//									 unselectChip(currentChip);
			//									 popup.dismiss();
			//	}
			//}
			//)
			//	;
			popup.Show();
			ListView listView = popup.ListView;
			listView.ChoiceMode = ChoiceMode.Single;
			listView.SetItemChecked(0, true);
		}

		

		/**
     * Return whether a touch event was inside the delete target of
     * a selected chip. It is in the delete target if:
     * 1) the x and y points of the event are within the
     * delete asset.
     * 2) the point tapped would have caused a cursor to appear
     * right after the selected chip.
     * @return bool
     */

		private bool IsInDelete(DrawableChipSpan chipSpan, int offset, float x, float y)
		{
			// Figure out the bounds of this chip and whether or not
			// the user clicked in the X portion.
			// TODO: Should x and y be used, or removed?
			if (_disableDelete)
			{
				return false;
			}

			return chipSpan.isSelected() &&
				   ((_avatarPosition == AvatarPositionEnd && offset == GetChipEnd(chipSpan)) ||
					(_avatarPosition != AvatarPositionEnd && offset == GetChipStart(chipSpan)));
		}

	
		
		/**
     * Handle click events for a chip. When a selected chip receives a click
     * event, see if that event was in the delete icon. If so, delete it.
     * Otherwise, unselect the chip.
     */

		public void onClick(DrawableChipSpan chipSpan, int offset, float x, float y)
		{
			if (chipSpan.isSelected())
			{
				if (IsInDelete(chipSpan, offset, x, y))
				{
					RemoveChip(chipSpan);
				}
				else
				{
					ClearSelectedChip();
				}
			}
		}

		private void OnTextChanged(object sender, TextChangedEventArgs e)
		{
			var s = e.Text;
			var before = e.BeforeCount;
			var count = e.AfterCount;

			// The user deleted some text OR some text was replaced; check to
			// see if the insertion point is on a space
			// following a chip.

			if (before - count == 1)
			{
				// If the item deleted is a space, and the thing before the
				// space is a chip, delete the entire span.
				int selStart = SelectionStart;
				var repl = Spannable.GetSpans(selStart, selStart, Class.FromType(typeof(DrawableChipSpan))).Cast<DrawableChipSpan>().ToArray();

				if (repl.Length > 0)
				{
					// There is a chip there! Just remove it.
					IEditable editable = EditableText;

					// Add the separator token.
					int tokenStart = 0;
					if (selStart == editable.Length())
					{
						tokenStart = _tokenizer.FindTokenStart(editable, selStart);
					}
					else if (selStart != 0)
					{
						tokenStart = _tokenizer.FindTokenStart(editable, selStart - 1);
					}
					int tokenEnd = _tokenizer.FindTokenEnd(editable, tokenStart);

					// If start and end are set on the token instead of the recipient,
					// look for another starting point
					if (tokenEnd != 0 && tokenStart == tokenEnd)
					{
						tokenStart = _tokenizer.FindTokenStart(editable, tokenEnd - 1);
					}

					// Increments the tokenEnd to include the commit character.
					tokenEnd = tokenEnd + 1;
					if (tokenEnd > editable.Length())
					{
						tokenEnd = editable.Length();
					}
					editable.Delete(tokenStart, tokenEnd);
					Spannable.RemoveSpan(repl[0]);
				}
			}
			else if (count > before)
			{
				//if (mSelectedChip != null && isGeneratedContact(mSelectedChip))
				//if (mSelectedChip != null)
				//{
				//	if (lastCharacterIsCommitCharacter(s))
				//	{
				//		commitByCharacter();
				//		return;
				//	}
				//}
			}
		}

		private void OnAfterTextChanged(object sender, AfterTextChangedEventArgs e)
		{
			var s = e.Editable;
			// If the text has been set to null or empty, make sure we remove
			// all the spans we applied.
			if (TextUtils.IsEmpty(s))
			{
				// Remove all the chips spans.
				ISpannable spannable = Spannable;
				var chips = spannable.GetSpans(0, Text.Length, Class.FromType(typeof(DrawableChipSpan))).Cast<DrawableChipSpan>().ToArray();

				foreach (var chip in chips)
				{
					spannable.RemoveSpan(chip);
				}

				if (_moreChip != null)
				{
					spannable.RemoveSpan(_moreChip);
				}
				ClearSelectedChip();
				return;
			}
			// Get whether there are any recipients pending addition to the
			// view. If there are, don't do anything in the text watcher.
			//if (chipsPending())
			//{
			//	return;
			//}
			// If the user is editing a chip, don't clear it.
			//if (mSelectedChip != null)
			//{
			//	if (!isGeneratedContact(mSelectedChip))
			//	{
			//		SetCursorVisible(true);
			//		SetSelection(Text.Length);
			//		clearSelectedChip();
			//	}
			//	else
			//	{
			//		return;
			//	}
			//}
			int length = s.Length();
			// Make sure there is content there to parse and that it is
			// not just the commit character.
			if (length > 1)
			{
				if (LastCharacterIsCommitCharacter(s))
				{
					CommitByCharacter();
					return;
				}
				char last;
				int end = SelectionEnd == 0 ? 0 : SelectionEnd - 1;
				int len = Length() - 1;
				if (end != len)
				{
					last = s.CharAt(end);
				}
				else
				{
					last = s.CharAt(len);
				}
				if (last == CommitCharSpace)
				{
					//if (!isPhoneQuery())
					//{
					// Check if this is a valid email address. If it is,
					// commit it.
					int tokenStart = _tokenizer.FindTokenStart(Text, SelectionEnd);
					var sub = Text.JavaSubstring(tokenStart, _tokenizer.FindTokenEnd(Text, tokenStart));

					if (!TextUtils.IsEmpty(sub) && _validator != null && _validator.IsValid(sub))
					{
						CommitByCharacter();
					}
					//}
				}
			}
		}

		public bool LastCharacterIsCommitCharacter(ICharSequence s)
		{
			char last;
			int end = SelectionEnd == 0 ? 0 : SelectionEnd - 1;
			last = s.CharAt(end);
			return last == CommitCharComma || last == CommitCharSemicolon;
		}

		/**
     * Removes the commit char that triggers the creation of the token before using the
     * enoughToFilter method.
     */

		private void RemoveCommitCharBeforeCreatingChip()
		{
			//RemoveTextChangedListener(mTextWatcher);
			AfterTextChanged -= OnAfterTextChanged;
			TextChanged -= OnTextChanged;

			IEditable s = EditableText;
			char last;
			int end = SelectionEnd == 0 ? 0 : SelectionEnd - 1;
			last = s.CharAt(end);
			if (last == CommitCharComma || last == CommitCharSemicolon)
			{
				s.Delete(end, end + 1);
			}

			TextChanged += OnTextChanged;
			AfterTextChanged += OnAfterTextChanged;
			//mHandler.Post(mAddTextWatcher);
		}

		private int MovePastTerminators(int tokenEnd)
		{
			if (tokenEnd >= Length())
			{
				return tokenEnd;
			}
			char atEnd = Text.ElementAt(tokenEnd);
			if (atEnd == CommitCharComma || atEnd == CommitCharSemicolon)
			{
				tokenEnd++;
			}
			// This token had not only an end token character, but also a space
			// separating it from the next token.
			if (tokenEnd < Length() && Text.ElementAt(tokenEnd) == ' ')
			{
				tokenEnd++;
			}
			return tokenEnd;
		}

		public bool OnDown(MotionEvent e)
		{
			return false;
		}

		public bool OnFling(MotionEvent e1, MotionEvent e2, float velocityX, float velocityY)
		{
			return false;
		}

		public void OnLongPress(MotionEvent @event)
		{
		}

		public bool OnScroll(MotionEvent e1, MotionEvent e2, float distanceX, float distanceY)
		{
			return false;
		}

		public void OnShowPress(MotionEvent e)
		{
		}

		public bool OnSingleTapUp(MotionEvent e)
		{
			return false;
		}

		// The following methods are used to provide some functionality on older versions of Android
		// These methods were copied out of JB MR2's TextView
		/////////////////////////////////////////////////
		private int SupportGetOffsetForPosition(float x, float y)
		{
			if (Layout == null)
				return -1;

			int line = SupportGetLineAtCoordinate(y);
			int offset = SupportGetOffsetAtCoordinate(line, x);
			return offset;
		}

		private float SupportConvertToLocalHorizontalCoordinate(float x)
		{
			x -= TotalPaddingLeft;
			// Clamp the position to inside of the view.
			x = Math.Max(0.0f, x);
			x = Math.Min(Width - TotalPaddingRight - 1, x);
			x += ScrollX;
			return x;
		}

		private int SupportGetLineAtCoordinate(float y)
		{
			y -= TotalPaddingLeft;
			// Clamp the position to inside of the view.
			y = Math.Max(0.0f, y);
			y = Math.Min(Height - TotalPaddingBottom - 1, y);
			y += ScrollY;
			return Layout.GetLineForVertical((int)y);
		}

		private int SupportGetOffsetAtCoordinate(int line, float x)
		{
			x = SupportConvertToLocalHorizontalCoordinate(x);
			return Layout.GetOffsetForHorizontal(line, x);
		}

		public void OnClick(View v)
		{
			// Copy this to the clipboard.
			//ClipboardManager clipboard =  (ClipboardManager) Context.GetSystemService(Context.ClipboardService);
			//clipboard.setPrimaryClip(ClipData.newPlainText("", mCopyAddress));
			//mCopyDialog.dismiss();
		}

		public override void DismissDropDown()
		{
			if (_dismissPopupOnClick)
			{
				base.DismissDropDown();
			}
		}

		private void DismissDropDownOnItemSelected(bool dismiss)
		{
			_dismissPopupOnClick = dismiss;
		}

		private void SetPostSelectedAction(ItemSelectedListener listener)
		{
			_itemSelectedListener = listener;
		}
	}
}
