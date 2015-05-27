using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using Android.App;
using Android.Content;
using Android.Content.Res;
using Android.Graphics;
using Android.Graphics.Drawables;
using Android.OS;
using Android.Provider;
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
using Java.Util.Regex;
using Javax.Xml.Validation;
using Exception = Java.Lang.Exception;
using LayoutDirection = Android.Views.LayoutDirection;
using Math = Java.Lang.Math;
using Object = Java.Lang.Object;
using String = System.String;
using Uri = Android.Net.Uri;

namespace com.android.ex.chips
{
	[Register("com.android.ex.chips.ChipsEditTextView")]
	public partial class ChipsEditTextView : MultiAutoCompleteTextView,
		AdapterView.IOnItemClickListener,
		ActionMode.ICallback,
		//RecipientAlternatesAdapter.OnCheckedItemChangedListener,
		GestureDetector.IOnGestureListener,
		AutoCompleteTextView.IOnDismissListener,
		View.IOnClickListener,
		TextView.IOnEditorActionListener
	{
		private static readonly char COMMIT_CHAR_COMMA = ',';

		private static readonly char COMMIT_CHAR_SEMICOLON = ';';

		private static readonly char COMMIT_CHAR_SPACE = ' ';

		//private static String SEPARATOR = new String(String.ValueOf(COMMIT_CHAR_COMMA) + String.ValueOf(COMMIT_CHAR_SPACE));

		//private static String TAG = new String("RecipientEditTextView");

		//private static int DISMISS = "dismiss".GetHashCode();

		private static long DISMISS_DELAY = 300;

		// TODO: get correct number/ algorithm from with UX.
		// Visible for testing. /*package*/
		private static int CHIP_LIMIT = 2;

		private static int MAX_CHIPS_PARSED = 50;

		private static int sSelectedTextColor = -1;

		// Resources for displaying chips.
		private Drawable mChipBackground = null;

		private Drawable mChipDelete = null;

		private Drawable mInvalidChipBackground;

		protected Drawable mChipBackgroundPressed;

		private float mChipHeight;

		private float mChipFontSize;

		private string mChipEntryErrorHint;

		private string mChipOverLimitErrorHint;

		private float mLineSpacingExtra;

		private int mChipPadding;

		// Enumerator for avatar position. See attr.xml for more details. 0 for end, 1 for start.
		private int mAvatarPosition;

		private static int AVATAR_POSITION_END = 0;

		private static int AVATAR_POSITION_START = 1;

		// Enumerator for image span alignment. See attr.xml for more details. 0 for bottom, 1 for baseline.
		private SpanAlign mImageSpanAlignment;

		private static int IMAGE_SPAN_ALIGNMENT_BOTTOM = 0;

		private static int IMAGE_SPAN_ALIGNMENT_BASELINE = 1;

		private bool mDisableDelete;

		private ITokenizer mTokenizer;

		private IValidator mValidator;

		internal DrawableChipSpan mSelectedChip;

		private Bitmap mDefaultContactPhoto, mNoAvatarPicture;

		internal ImageSpan mMoreChip;

		private TextView mMoreItem;

		// VisibleForTesting
		//private List<String> mPendingChips = new List<String>();

		private Handler mHandler;

		//private int mPendingChipsCount = 0;

		private bool mNoChips = false;

		private ListPopupWindow mAlternatesPopup;

		private ListPopupWindow mAddressPopup;

		// VisibleForTesting
		//private List<DrawableRecipientChip> mTemporaryRecipients;

		private List<DrawableChipSpan> mRemovedSpans;

		private bool mShouldShrink = true;

		// Chip copy fields.
		private GestureDetector mGestureDetector;

		//private Dialog mCopyDialog;

		//private String mCopyAddress;

		// Used with {@link #mAlternatesPopup}. Handles clicks to alternate addresses for a selected chip.
		private AdapterView.IOnItemClickListener mAlternatesListener;

		private int mCheckedItem;

		//private ITextWatcher mTextWatcher;

		// Obtain the enclosing scroll view, if it exists, so that the view can be
		// scrolled to show the last line of chips content.
		private ScrollView mScrollView;

		private bool mTriedGettingScrollView;

		//private bool mDragEnabled = false;

		// This pattern comes from android.util.Patterns. It has been tweaked to handle a "1" before
		// parens, so numbers such as "1 (425) 222-2342" match.
		//private static Java.Util.Regex.Pattern PHONE_PATTERN
		//	= Java.Util.Regex.Pattern.Compile( // sdd = space, dot, or dash
		//		"(\\+[0-9]+[\\- \\.]*)?" // +<digits><sdd>*
		//		+ "(1?[ ]*\\([0-9]+\\)[\\- \\.]*)?" // 1(<digits>)<sdd>*
		//		+ "([0-9][0-9\\- \\.][0-9\\- \\.]+[0-9])"); // <digit><digit|sdd>+<digit>

		//private System.Action mAddTextWatcher;
		//private  Runnable mAddTextWatcher = new Runnable() {

		//	public void run() {
		//		if (mTextWatcher == null) {
		//			mTextWatcher = new RecipientTextWatcher();
		//			addTextChangedListener(mTextWatcher);
		//		}
		//	}
		//};

		//private IndividualReplacementTask mIndividualReplacements;

		//private System.Action mHandlePendingChips;
		//private Runnable mHandlePendingChips = new Runnable() {


		//	public void run() {
		//		handlePendingChips();
		//	}

		//};

		private System.Action mDelayedShrink;
		//private Runnable mDelayedShrink = new Runnable() {


		//	public void run() {
		//		shrink();
		//	}

		//};

		private int mMaxLines;

		private int mShrinkMaxLines = 1;

		private int mMaxChipsAllowed = 99;

		private static int sExcessTopPadding = -1;

		private int mActionBarHeight;

		private bool mAttachedToWindow;

		//private DropdownChipLayouter mDropdownChipLayouter;

		private bool mDismissPopupOnClick = true;

		private ItemSelectedListener itemSelectedListener;


		//public RecipientEditTextView(Context context, IAttributeSet attrs) : this(context, attrs, new MvxFilteringAdapter(context))
		//{

		//}

		//public RecipientEditTextView(Context context, IAttributeSet attrs, MvxFilteringAdapter adapter)
		//	: base(context, attrs)
		//{

		//}

		protected ChipsEditTextView(IntPtr javaReference, JniHandleOwnership transfer)
			: base(javaReference, transfer)
		{
		}

		public ChipsEditTextView(Context context, IAttributeSet attrs)
			: base(context, attrs)
		{
			TextChanged += OnTextChanged;
			AfterTextChanged += OnAfterTextChanged;

			////mAddTextWatcher = () =>
			////{
			////	if (mTextWatcher == null)
			////	{
			////		mTextWatcher = new RecipientTextWatcher();
			////		AddTextChangedListener(mTextWatcher);
			////	}
			////};

			//mHandlePendingChips = handlePendingChips;
			mDelayedShrink = shrink;

			//// TODO: would be nice to show chips as an example here
			//if (IsInEditMode)
			//{
			//	return;
			//}

			setChipDimensions(context, attrs);
			if (sSelectedTextColor == -1)
			{
				sSelectedTextColor = context.Resources.GetColor(Android.Resource.Color.White);
			}

			mAlternatesPopup = new ListPopupWindow(context);
			mAddressPopup = new ListPopupWindow(context);
			//mCopyDialog = new Dialog(context);
			//mAlternatesListener = new OnItemClickListener() {

			//	public void onItemClick(AdapterView<?> adapterView,View view, int position,
			//			long rowId) {
			//		mAlternatesPopup.setOnItemClickListener(null);
			//		replaceChip(mSelectedChip, ((RecipientAlternatesAdapter) adapterView.getAdapter())
			//				.getRecipientEntry(position));
			//		Message delayed = Message.obtain(mHandler, DISMISS);
			//		delayed.obj = mAlternatesPopup;
			//		mHandler.sendMessageDelayed(delayed, DISMISS_DELAY);
			//		clearComposingText();
			//	}
			//};

			InputType = InputType | InputTypes.TextFlagNoSuggestions;
			//setInputType(getInputType() | InputType.TYPE_TEXT_FLAG_NO_SUGGESTIONS);
			OnItemClickListener = this;
			
			CustomSelectionActionModeCallback = this;
			//setCustomSelectionActionModeCallback(this);

			//mHandler = new Handler() {

			//	public void handleMessage(Message msg) {
			//		if (msg.what == DISMISS) {
			//			((ListPopupWindow) msg.obj).dismiss();
			//			return;
			//		}
			//		super.handleMessage(msg);
			//	}
			//};

			//mTextWatcher = new RecipientTextWatcher();
			//AddTextChangedListener(mTextWatcher);

			mGestureDetector = new GestureDetector(context, this);
			SetOnEditorActionListener(this);
			
			//setDropdownChipLayouter(new DropdownChipLayouter(LayoutInflater.from(context), context));
		}

		//protected void setDropdownChipLayouter(DropdownChipLayouter dropdownChipLayouter)
		//{
		//	mDropdownChipLayouter = dropdownChipLayouter;
		//}

		protected override void OnDetachedFromWindow()
		{
			base.OnDetachedFromWindow();
			mAttachedToWindow = false;
		}


		protected override void OnAttachedToWindow()
		{
			base.OnAttachedToWindow();
			mAttachedToWindow = true;
		}

		public bool OnEditorAction(TextView v, ImeAction action, KeyEvent e)
		{
			if (action == ImeAction.Done)
			{
				//if (commitDefault())
				//{
				//	return true;
				//}
				if (mSelectedChip != null)
				{
					clearSelectedChip();
					return true;
				}
				else if (focusNext())
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

		/*package*/
		private DrawableChipSpan getLastChip()
		{
			DrawableChipSpan last = null;
			DrawableChipSpan[] chipSpans = getSortedVisibleRecipients();
			if (chipSpans != null && chipSpans.Length > 0)
			{
				last = chipSpans[chipSpans.Length - 1];
			}
			return last;
		}

		protected override void OnSelectionChanged(int selStart, int selEnd)
		{
			// When selection changes, see if it is inside the chips area.
			// If so, move the cursor back after the chips again.
			DrawableChipSpan last = getLastChip();
			if (last != null && selStart < getSpannable().GetSpanEnd((Object)last))
			{
				// Grab the last chip and set the cursor to after it.
				SetSelection(Math.Min(getSpannable().GetSpanEnd((Object)last) + 1, Text.Length));
			}
			base.OnSelectionChanged(selStart, selEnd);
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

		public override IParcelable OnSaveInstanceState()
		{
			// If the user changes orientation while they are editing, just roll back the selection.
			clearSelectedChip();

			if (!HasFocus)
			{
				expand();
			}

			return base.OnSaveInstanceState();
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
				shrink();
			}
			else
			{
				expand();
			}
		}

		private int getExcessTopPadding()
		{
			if (sExcessTopPadding == -1)
			{
				sExcessTopPadding = (int)(mChipHeight + mLineSpacingExtra);
			}
			return sExcessTopPadding;
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
				//			if (entries != null && entries.size() > 0) {
				//				scrollBottomIntoView();
				//			}
			}
		}

		protected void scrollBottomIntoView()
		{
			if (mScrollView != null && mShouldShrink)
			{
				int[] location = new int[2];
				GetLocationOnScreen(location);
				int height = Height;
				int currentPos = location[1] + height;
				// Desired position shows at least 1 line of chips below the action
				// bar. We add excess padding to make sure this is always below other
				// content.
				int desiredPos = (int)mChipHeight + mActionBarHeight + getExcessTopPadding();
				if (currentPos > desiredPos)
				{
					mScrollView.ScrollBy(0, currentPos - desiredPos);
				}
			}
		}

		protected ScrollView getScrollView()
		{
			return mScrollView;
		}


		public void performValidation()
		{
			// Do nothing. Chips handles its own validation.
		}

		private void shrink()
		{
			if (mTokenizer == null)
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
				mHandler.RemoveCallbacks(mDelayedShrink);
				mHandler.Post(mDelayedShrink);
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
			int start = mTokenizer.FindTokenStart(editable, end);
			var chips = getSpannable().GetSpans(start, end, Class.FromType(typeof(DrawableChipSpan))).Cast<DrawableChipSpan>().ToArray();
			if ((chips.Length == 0))
			{
				IEditable text = EditableText;
				int whatEnd = mTokenizer.FindTokenEnd(text, start);
				// This token was already tokenized, so skip past the ending token.
				if (whatEnd < text.Length() && text.CharAt(whatEnd) == ',')
				{
					whatEnd = movePastTerminators(whatEnd);
				}

				// In the middle of chip; treat this as an edit
				// and commit the whole token if it is not only spaces
				bool isOverMaxNumberOfChips = getRecipients().Length >= mMaxChipsAllowed;
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
			createMoreChip();
		}

		private void expand()
		{
			if (mShouldShrink)
			{
				SetMaxLines(Integer.MaxValue);
			}
			removeMoreChip();
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
			paint.TextSize = mChipFontSize;
			//if (maxWidth <= 0 && Log.isLoggable(TAG, Log.DEBUG))
			//{
			//	Log.d(TAG, "Max width is negative: " + maxWidth);
			//}

			return TextUtils.EllipsizeFormatted(text.ToJavaString(), paint, maxWidth, TextUtils.TruncateAt.End).ToString();
		}

		/**
     * Creates a bitmap of the given contact on a selected chip.
     *
     * @param contact The recipient entry to pull data from.
     * @param paint The paint to use to draw the bitmap.
     */
		private Bitmap createSelectedChip(ChipEntry contact, TextPaint paint)
		{
			paint.Color = new Color(sSelectedTextColor);
			Bitmap photo;
			if (mDisableDelete)
			{
				// Show the avatar instead if we don't want to delete
				photo = getAvatarIcon(contact);
			}
			else
			{
				photo = ((BitmapDrawable)mChipDelete).Bitmap;
			}
			return createChipBitmap(contact, paint, photo, mChipBackgroundPressed);
		}

		/**
     * Creates a bitmap of the given contact on a selected chip.
     *
     * @param contact The recipient entry to pull data from.
     * @param paint The paint to use to draw the bitmap.
     */
		// TODO: Is leaveBlankIconSpacer obsolete now that we have left and right attributes?
		private Bitmap createUnselectedChip(ChipEntry contact, TextPaint paint,
											bool leaveBlankIconSpacer)
		{
			Drawable background = getChipBackground(contact);
			Bitmap photo = getAvatarIcon(contact);
			paint.Color = Context.Resources.GetColor(Android.Resource.Color.Black);
			return createChipBitmap(contact, paint, photo, background);
		}

		protected Bitmap createChipBitmap(ChipEntry contact, TextPaint paint, Bitmap icon,
										  Drawable background)
		{
			if (background == null)
			{
				//Log.w(TAG, "Unable to draw a background for the chips as it was never set");
				return Bitmap.CreateBitmap(
					(int)mChipHeight * 2, (int)mChipHeight, Bitmap.Config.Argb8888);
			}

			Rect backgroundPadding = new Rect();
			background.GetPadding(backgroundPadding);

			// Ellipsize the text so that it takes AT MOST the entire width of the
			// autocomplete text entry area. Make sure to leave space for padding
			// on the sides.
			int height = (int)mChipHeight + Resources.GetDimensionPixelSize(Resource.Dimension.extra_chip_height);

			// Compute the space needed by the more chip before ellipsizing
			//String moreText = new String(String.Format(mMoreItem.Text, mMaxChipsAllowed));
			String moreText = String.Format(mMoreItem.Text, mMaxChipsAllowed);
			TextPaint morePaint = new TextPaint(Paint);
			morePaint.TextSize = mMoreItem.TextSize;
			int moreChipWidth = (int)morePaint.MeasureText(moreText) + mMoreItem.PaddingLeft + mMoreItem.PaddingRight;

			// Since the icon is a square, it's width is equal to the maximum height it can be inside
			// the chip.
			int iconWidth = height - backgroundPadding.Top - backgroundPadding.Bottom;
			float[] widths = new float[1];
			paint.GetTextWidths(" ", widths);

			var ellipsizedText = EllipsizeText(createChipDisplayText(contact), paint,
											   calculateAvailableWidth() - iconWidth - widths[0] -
											   backgroundPadding.Left - backgroundPadding.Right -
											   moreChipWidth);
			int textWidth = (int)paint.MeasureText(ellipsizedText, 0, ellipsizedText.Length);

			// Make sure there is a minimum chip width so the user can ALWAYS
			// tap a chip without difficulty.
			int width = Math.Max(iconWidth * 2, textWidth + (mChipPadding * 2) + iconWidth
											  + backgroundPadding.Left + backgroundPadding.Right);

			// Create the background of the chip.
			Bitmap tmpBitmap = Bitmap.CreateBitmap(width, height, Bitmap.Config.Argb8888);
			Canvas canvas = new Canvas(tmpBitmap);

			// Draw the background drawable
			background.SetBounds(height / 2, 0, width, height);
			background.Draw(canvas);
			// Draw the text vertically aligned
			var x = shouldPositionAvatarOnRight()
				? mChipPadding + backgroundPadding.Left
				: width - backgroundPadding.Right - mChipPadding - textWidth;
			var y = getTextYOffset(ellipsizedText, paint, height);
			paint.Color = Color.ParseColor("#FF5C5C5C");
			paint.AntiAlias = true;
			canvas.DrawText(ellipsizedText,
			                0,
			                ellipsizedText.Length,
			                x,
			               y,
			                paint);

			if (icon != null)
			{
				// Draw the icon
				icon = ChipsUtil.getClip(icon);
				//int iconX = shouldPositionAvatarOnRight()
				//	? width - backgroundPadding.Right - iconWidth
				//	: backgroundPadding.Left;
				RectF src = new RectF(0, 0, icon.Width, icon.Height);
				RectF dst = new RectF(0, 0, height, height);
				drawIconOnCanvas(icon, canvas, paint, src, dst);
			}
			return tmpBitmap;
		}

		/**
     * Returns true if the avatar should be positioned at the right edge of the chip.
     * Takes into account both the set avatar position (start or end) as well as whether
     * the layout direction is LTR or RTL.
     */

		private bool shouldPositionAvatarOnRight()
		{
			bool isRtl = Build.VERSION.SdkInt >= Android.OS.BuildVersionCodes.JellyBeanMr1 && LayoutDirection == LayoutDirection.Rtl;
			bool assignedPosition = mAvatarPosition == AVATAR_POSITION_END;
			// If in Rtl mode, the position should be flipped.
			return isRtl ? !assignedPosition : assignedPosition;
		}

		/**
     * Returns the avatar icon to use for this recipient entry. Returns null if we don't want to
     * draw an icon for this recipient.
     */

		private Bitmap getAvatarIcon(ChipEntry contact)
		{
			// Don't draw photos for recipients that have been typed in OR generated on the fly.
			//long contactId = contact.getContactId();
			//bool drawPhotos = isPhoneQuery()
			//	? contactId != RecipientEntry.INVALID_CONTACT
			//	: (contactId != RecipientEntry.INVALID_CONTACT
			//	   && (contactId != RecipientEntry.GENERATED_CONTACT &&
			//		   !TextUtils.IsEmpty(contact.getDisplayName())));

			//if (drawPhotos)
			//{
			//	byte[] photoBytes = contact.getPhotoBytes();
			//	// There may not be a photo yet if anything but the first contact address
			//	// was selected.
			//	if (photoBytes == null && contact.getPhotoThumbnailUri() != null)
			//	{
			//		// TODO: cache this in the recipient entry?
			//		Adapter.FetchPhoto(contact, contact.getPhotoThumbnailUri(), Context.ContentResolver);
			//		photoBytes = contact.getPhotoBytes();
			//	}
			//	if (photoBytes != null)
			//	{
			//		return BitmapFactory.DecodeByteArray(photoBytes, 0, photoBytes.Length);
			//	}
			//	else
			//	{
			//		// TODO: can the scaled down default photo be cached?
			return mDefaultContactPhoto;
			//	}
			//}

			return mNoAvatarPicture;
		}

		/**
     * Get the background drawable for a RecipientChip.
     */
		// Visible for testing.
		/* package */

		private Drawable getChipBackground(ChipEntry contact)
		{
			//return contact.isValid() ? mChipBackground : mInvalidChipBackground;
			return mChipBackground;
		}

		/**
     * Given a height, returns a Y offset that will draw the text in the middle of the height.
     */

		protected float getTextYOffset(String text, TextPaint paint, int height)
		{
			Rect bounds = new Rect();
			paint.GetTextBounds(text, 0, text.Length, bounds);
			int textHeight = bounds.Bottom - bounds.Top;
			return height - ((height - textHeight) / 2) - (int)paint.Descent() / 2;
		}

		/**
     * Draws the icon onto the canvas given the source rectangle of the bitmap and the destination
     * rectangle of the canvas.
     */

		protected void drawIconOnCanvas(Bitmap icon, Canvas canvas, Paint paint, RectF src, RectF dst)
		{
			Matrix matrix = new Matrix();
			matrix.SetRectToRect(src, dst, Matrix.ScaleToFit.Fill);
			canvas.DrawBitmap(icon, matrix, paint);
		}

		//private DrawableRecipientChip constructChipSpan(RecipientEntry contact, bool pressed,bool leaveIconSpace) throws NullPointerException {
		private DrawableChipSpan constructChipSpan(ChipEntry contact, bool pressed, bool leaveIconSpace)
		{
			if (mChipBackground == null)
			{
				throw new NullPointerException(
					"Unable to render any chips as setChipDimensions was not called.");
			}

			TextPaint paint = Paint;
			float defaultSize = paint.TextSize;
			int defaultColor = paint.Color;

			Bitmap tmpBitmap;
			if (pressed)
			{
				tmpBitmap = createSelectedChip(contact, paint);
			}
			else
			{
				tmpBitmap = createUnselectedChip(contact, paint, leaveIconSpace);
			}

			// Get the inset of the chip
			int chipInset = (int)(Resources.GetDimension(Resource.Dimension.line_spacing_extra) / 2);

			// Pass the full text, un-ellipsized, to the chip.
			Drawable result = new BitmapDrawable(Resources, tmpBitmap);
			result = new InsetDrawable(result, 0, chipInset, 0, chipInset);
			result.SetBounds(0, 0, tmpBitmap.Width, tmpBitmap.Height + 2 * chipInset);

			//DrawableRecipientChip recipientChip = new VisibleRecipientChip(result, contact, getImageSpanAlignment());
			DrawableChipSpan chipSpan = new VisibleChipSpan(result, contact, mImageSpanAlignment);
			// Return text to the original size.
			paint.TextSize = defaultSize;
			paint.Color = new Color(defaultColor);
			return chipSpan;
		}

		//private SpanAlign getImageSpanAlignment()
		//{
		//	switch (mImageSpanAlignment)
		//	{
		//		case SpanAlign.Baseline:
		//			return SpanAlign.Baseline;
		//		case SpanAlign.Bottom:
		//			return SpanAlign.Bottom;
		//		default:
		//			return SpanAlign.Bottom;
		//	}
		//}

		/**
     * Calculate the bottom of the line the chip will be located on using:
     * 1) which line the chip appears on
     * 2) the height of a chip
     * 3) padding built into the edit text view
     */

		private int calculateOffsetFromBottom(int line)
		{
			// Line offsets start at zero.
			int actualLine = LineCount - (line + 1);
			return -((actualLine * ((int)mChipHeight) + PaddingBottom) + PaddingTop)
				   + DropDownVerticalOffset;
		}

		/**
     * Get the max amount of space a chip can take up. The formula takes into
     * account the width of the EditTextView, any view padding, and padding
     * that will be added to the chip.
     */

		private float calculateAvailableWidth()
		{
			return Width - PaddingLeft - PaddingRight - (mChipPadding * 2);
		}

		private void setChipDimensions(Context context, IAttributeSet attrs)
		{
			TypedArray a = context.ObtainStyledAttributes(attrs, Resource.Styleable.RecipientEditTextView, 0, 0);
			Resources r = Context.Resources;

			mChipBackground = a.GetDrawable(Resource.Styleable.RecipientEditTextView_chipBackground);
			if (mChipBackground == null)
			{
				mChipBackground = r.GetDrawable(Resource.Drawable.chip_background);
			}
			mChipBackgroundPressed = a.GetDrawable(Resource.Styleable.RecipientEditTextView_chipBackgroundPressed);
			if (mChipBackgroundPressed == null)
			{
				mChipBackgroundPressed = r.GetDrawable(Resource.Drawable.chip_background_selected);
			}
			mChipDelete = a.GetDrawable(Resource.Styleable.RecipientEditTextView_chipDelete);
			if (mChipDelete == null)
			{
				mChipDelete = r.GetDrawable(Resource.Drawable.chip_delete);
			}
			mChipPadding = a.GetDimensionPixelSize(Resource.Styleable.RecipientEditTextView_chipPadding, -1);
			if (mChipPadding == -1)
			{
				mChipPadding = (int)r.GetDimension(Resource.Dimension.chip_padding);
			}

			mDefaultContactPhoto = BitmapFactory.DecodeResource(r, Resource.Drawable.ic_contact_picture);

			mNoAvatarPicture = BitmapFactory.DecodeResource(r, Resource.Drawable.no_avatar_picture);

			mMoreItem = (TextView)LayoutInflater.From(Context).Inflate(Resource.Layout.more_item, null);

			mChipHeight = a.GetDimensionPixelSize(Resource.Styleable.RecipientEditTextView_chipHeight, -1);
			if (mChipHeight == -1)
			{
				mChipHeight = r.GetDimension(Resource.Dimension.chip_height);
			}
			mChipFontSize = a.GetDimensionPixelSize(Resource.Styleable.RecipientEditTextView_chipFontSize, -1);
			if (mChipFontSize == -1)
			{
				mChipFontSize = r.GetDimension(Resource.Dimension.chip_text_size);
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

			mInvalidChipBackground = a.GetDrawable(Resource.Styleable.RecipientEditTextView_invalidChipBackground);
			if (mInvalidChipBackground == null)
			{
				mInvalidChipBackground = r.GetDrawable(Resource.Drawable.chip_background_invalid);
			}

			mAvatarPosition = a.GetInt(Resource.Styleable.RecipientEditTextView_avatarPosition, 1);
			mImageSpanAlignment = (SpanAlign)a.GetInt(Resource.Styleable.RecipientEditTextView_imageSpanAlignment, 0);
			mDisableDelete = a.GetBoolean(Resource.Styleable.RecipientEditTextView_disableDelete, false);

			mLineSpacingExtra = r.GetDimension(Resource.Dimension.line_spacing_extra);
			mMaxLines = r.GetInteger(Resource.Integer.chips_max_lines);
			TypedValue tv = new TypedValue();
			if (context.Theme.ResolveAttribute(Android.Resource.Attribute.ActionBarSize, tv, true))
			{
				mActionBarHeight = TypedValue.ComplexToDimensionPixelSize(tv.Data, Resources.DisplayMetrics);
			}

			a.Recycle();
		}

		// Visible for testing.
		/* package */

		private void setMoreItem(TextView moreItem)
		{
			mMoreItem = moreItem;
		}


		// Visible for testing.
		/* package */

		private void setChipBackground(Drawable chipBackground)
		{
			mChipBackground = chipBackground;
		}

		// Visible for testing.
		/* package */

		protected void setChipHeight(int height)
		{
			mChipHeight = height;
		}

		public float getChipHeight()
		{
			return mChipHeight;
		}

		public void setMaxNumberOfChipsAllowed(int numberOfChipsAllowed)
		{
			mMaxChipsAllowed = numberOfChipsAllowed;
		}

		public int getMaxNumberOfChipsAllowed()
		{
			return mMaxChipsAllowed;
		}

		public int getShrinkMaxLines()
		{
			return mShrinkMaxLines;
		}

		public void setShrinkMaxLines(int shrinkMaxLines)
		{
			mShrinkMaxLines = shrinkMaxLines;
		}

		/**
     * Set whether to shrink the recipients field such that at most
     * one line of recipients chips are shown when the field loses
     * focus. By default, the number of displayed recipients will be
     * limited and a "more" chip will be shown when focus is lost.
     * @param shrink
     */
		public void setOnFocusListShrinkRecipients(bool shrink)
		{
			mShouldShrink = shrink;
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
				checkChipWidths();
				//}
			}
			// Try to find the scroll view parent, if it exists.
			if (mScrollView == null && !mTriedGettingScrollView)
			{
				IViewParent parent = Parent;
				while (parent != null && !(parent is ScrollView))
				{
					parent = parent.Parent;
				}
				if (parent != null)
				{
					mScrollView = (ScrollView)parent;
				}
				mTriedGettingScrollView = true;
			}
		}

		//private void postHandlePendingChips()
		//{
		//	mHandler.RemoveCallbacks(mHandlePendingChips);
		//	mHandler.Post(mHandlePendingChips);
		//}

		private void checkChipWidths()
		{
			// Check the widths of the associated chips.
			DrawableChipSpan[] chipSpans = getSortedVisibleRecipients();
			if (chipSpans != null)
			{
				Rect bounds;
				foreach (var chip in chipSpans)
				{
					bounds = chip.getBounds();
					if (Width > 0 && bounds.Right - bounds.Left > Width - PaddingLeft - PaddingRight)
					{
						// Need to redraw that chip.
						replaceChip(chip, chip.getEntry());
					}
				}

			}
		}

		// Visible for testing.
		/*package*/

		//private void handlePendingChips()
		//{
		//	if (getViewWidth() <= 0)
		//	{
		//		// The widget has not been sized yet.
		//		// This will be called as a result of onSizeChanged
		//		// at a later point.
		//		return;
		//	}
		//	//if (mPendingChipsCount <= 0)
		//	//{
		//	//	return;
		//	//}

		//	//synchronized(mPendingChips)
		//	//{
		//		IEditable editable = EditableText;
		//		// Tokenize!
		//		if (mPendingChipsCount <= MAX_CHIPS_PARSED)
		//		{
		//			for (int i = 0; i < mPendingChips.Count; i++)
		//			{
		//				//String current = mPendingChips.get(i);
		//				String current = mPendingChips.ElementAt(i);
		//				int tokenStart = editable.ToString().IndexOf(current.ToString());
		//				// Always leave a space at the end between tokens.
		//				int tokenEnd = tokenStart + current.Length() - 1;
		//				if (tokenStart >= 0)
		//				{
		//					// When we have a valid token, include it with the token
		//					// to the left.
		//					if (tokenEnd < editable.Length() - 2
		//						&& editable.CharAt(tokenEnd) == COMMIT_CHAR_COMMA)
		//					{
		//						tokenEnd++;
		//					}
		//					createReplacementChip(tokenStart, tokenEnd, editable, i < CHIP_LIMIT
		//																		  || !mShouldShrink);
		//				}
		//				mPendingChipsCount--;
		//			}
		//			sanitizeEnd();
		//		}
		//		else
		//		{
		//			mNoChips = true;
		//		}

		//		//if (mTemporaryRecipients != null && mTemporaryRecipients.Count > 0 && mTemporaryRecipients.Count <= RecipientAlternatesAdapter.MAX_LOOKUPS)
		//		if (mTemporaryRecipients != null && mTemporaryRecipients.Count > 0)// && mTemporaryRecipients.Count <= RecipientAlternatesAdapter.MAX_LOOKUPS)
		//		{
		//			if (HasFocus || mTemporaryRecipients.Count < CHIP_LIMIT)
		//			{
		//				//new RecipientReplacementTask().Execute();
		//				mTemporaryRecipients = null;
		//			}
		//			else
		//			{
		//				// Create the "more" chip
		//				//mIndividualReplacements = new IndividualReplacementTask();
		//				//mIndividualReplacements.Execute(new List<DrawableRecipientChip>(mTemporaryRecipients.GetRange(0, CHIP_LIMIT)));
		//				if (mTemporaryRecipients.Count > CHIP_LIMIT)
		//				{
		//					mTemporaryRecipients = new List<DrawableRecipientChip>(
		//						mTemporaryRecipients.GetRange(CHIP_LIMIT, mTemporaryRecipients.Count()));
		//				}
		//				else
		//				{
		//					mTemporaryRecipients = null;
		//				}
		//				createMoreChip();
		//			}
		//		}
		//		else
		//		{
		//			// There are too many recipients to look up, so just fall back
		//			// to showing addresses for all of them.
		//			mTemporaryRecipients = null;
		//			createMoreChip();
		//		}
		//		mPendingChipsCount = 0;
		//		mPendingChips.Clear();
		//	//}
		//}

		// Visible for testing.
		/*package*/

		private int getViewWidth()
		{
			return Width;
		}

		/**
     * Remove any characters after the last valid chip.
     */
		// Visible for testing.
		/*package*/

		private void sanitizeEnd()
		{
			// Don't sanitize while we are waiting for pending chips to complete.
			//if (mPendingChipsCount > 0)
			//{
			//	return;
			//}
			// Find the last chip; eliminate any commit characters after it.
			DrawableChipSpan[] chipSpans = getSortedVisibleRecipients();
			ISpannable spannable = getSpannable();
			if (chipSpans != null && chipSpans.Length > 0)
			{
				int end;
				mMoreChip = getMoreChip();
				if (mMoreChip != null)
				{
					end = spannable.GetSpanEnd(mMoreChip);
				}
				else
				{
					end = getSpannable().GetSpanEnd((Object)getLastChip());
				}
				IEditable editable = EditableText;
				int length = editable.Length();
				if (length > end)
				{
					// See what characters occur after that and eliminate them.
					//if (Log.isLoggable(TAG, Log.DEBUG))
					//{
					//	Log.d(TAG, "There were extra characters after the last tokenizable entry."
					//			   + editable);
					//}
					editable.Delete(end + 1, length);
				}
			}
		}

		/**
     * Create a chip that represents just the email address of a recipient. At some later
     * point, this chip will be attached to a real contact entry, if one exists.
     */
		//private void createReplacementChip(int tokenStart, int tokenEnd, IEditable editable,
		//								   bool visible)
		//{
		//	if (alreadyHasChip(tokenStart, tokenEnd))
		//	{
		//		// There is already a chip present at this location.
		//		// Don't recreate it.
		//		return;
		//	}
		//	String token = new String(editable.ToString().Substring(tokenStart, tokenEnd));
		//	String trimmedToken = new String(token.Trim());
		//	int commitCharIndex = trimmedToken.LastIndexOf(COMMIT_CHAR_COMMA);
		//	if (commitCharIndex != -1 && commitCharIndex == trimmedToken.Length() - 1)
		//	{
		//		token = new String(trimmedToken.Substring(0, trimmedToken.Length() - 1));
		//	}
		//	RecipientEntry entry = createTokenizedEntry(token);
		//	if (entry != null)
		//	{
		//		DrawableRecipientChip chip = null;
		//		try
		//		{
		//			if (!mNoChips)
		//			{
		//				/*
		//			 * leave space for the contact icon if this is not just an
		//			 * email address
		//			 */
		//				bool leaveSpace = TextUtils.IsEmpty(entry.getDisplayName())
		//								  || TextUtils.Equals(entry.getDisplayName(),
		//													  entry.getDestination());
		//				chip = visible
		//					? constructChipSpan(entry, false, leaveSpace)
		//					: new InvisibleRecipientChip(entry);
		//			}
		//		}
		//		catch (NullPointerException e)
		//		{
		//			//Log.e(TAG, e.getMessage(), e);
		//		}
		//		editable.SetSpan(chip, tokenStart, tokenEnd, SpanTypes.ExclusiveExclusive);
		//		// Add this chip to the list of entries "to replace"
		//		if (chip != null)
		//		{
		//			if (mTemporaryRecipients == null)
		//			{
		//				mTemporaryRecipients = new List<DrawableRecipientChip>();
		//			}
		//			chip.setOriginalText(token);
		//			mTemporaryRecipients.Add(chip);
		//		}
		//	}
		//}

		//private static bool isPhoneNumber(String number)
		//{
		//	// TODO: replace this function with libphonenumber's isPossibleNumber (see
		//	// PhoneNumberUtil). One complication is that it requires the sender's region which
		//	// comes from the CurrentCountryIso. For now, let's just do this simple match.
		//	if (TextUtils.IsEmpty(number))
		//	{
		//		return false;
		//	}

		//	Matcher match = PHONE_PATTERN.Matcher(number);
		//	return match.Matches();
		//}

		// VisibleForTesting
		protected ChipEntry createTokenizedEntry(String token)
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
			if (mValidator != null && !isValid)
			{
				// Try fixing up the entry using the validator.
				var foo = new Java.Lang.String(token);
				//validatedToken = new String(mValidator.FixTextFormatted(token).ToString());
				validatedToken = mValidator.FixTextFormatted(new Java.Lang.String(token)).ToString();
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
			return mValidator == null || mValidator.IsValid(text);
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


		//public void setTokenizer(Tokenizer tokenizer)
		//{
		//	mTokenizer = tokenizer;
		//	super.setTokenizer(mTokenizer);
		//}
		public override void SetTokenizer(ITokenizer t)
		{
			mTokenizer = t;
			base.SetTokenizer(t);
		}

		//public void setValidator(Validator validator)
		//{
		//	mValidator = validator;
		//	super.setValidator(validator);
		//}
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

		//public bool onKeyPreIme(int keyCode, KeyEvent @event)
		//{
		//	if (keyCode == KeyEvent.KEYCODE_BACK && mSelectedChip != null)
		//	{
		//		clearSelectedChip();
		//		return true;
		//	}
		//	return super.onKeyPreIme(keyCode, @event);
		//}
		public override bool OnKeyPreIme(Keycode keyCode, KeyEvent @event)
		{
			if (keyCode == Keycode.Back && mSelectedChip != null)
			{
				clearSelectedChip();
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
						if (mSelectedChip != null)
						{
							clearSelectedChip();
						}
						//else
						//{
						//	commitDefault();
						//}
					}
					break;
			}
			return base.OnKeyUp(keyCode, @event);
		}

		private bool focusNext()
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
		//	// If there is no tokenizer, don't try to commit.
		//	if (mTokenizer == null)
		//	{
		//		return false;
		//	}
		//	IEditable editable = EditableText;
		//	int end = SelectionEnd;
		//	int start = mTokenizer.FindTokenStart(editable, end);

		//	if (shouldCreateChip(start, end))
		//	{
		//		int whatEnd = mTokenizer.FindTokenEnd(Text, start);
		//		// In the middle of chip; treat this as an edit
		//		// and commit the whole token.
		//		whatEnd = movePastTerminators(whatEnd);
		//		if (whatEnd != SelectionEnd)
		//		{
		//			handleEdit(start, whatEnd);
		//			return true;
		//		}
		//		return commitChip(start, end, editable);
		//	}
		//	return false;
		//}

		private void commitByCharacter()
		{
			// We can't possibly commit by character if we can't tokenize.
			if (mTokenizer == null)
			{
				return;
			}

			removeCommitCharBeforeCreatingChip();

			IEditable editable = EditableText;
			int end = SelectionEnd;
			int start = mTokenizer.FindTokenStart(editable, end);

			if (shouldCreateChip(start, end))
			{
				// TODO: Validate that there are no bugs when checking if the user can add a chip here
				if (!enoughRoomForAdditionalChip())
				{
					//SetError(mChipOverLimitErrorHint);
				}
				else
				{
					//SetError(null);
					commitChip(start, end, editable);
				}
			}
			else
			{
				//SetError(mChipEntryErrorHint);
			}
			SetSelection(Text.Length);
		}

		protected bool commitChip(int start, int end, IEditable editable)
		{
			// Check if there is not already too many chips
			if (getRecipients().Length >= mMaxChipsAllowed)
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
				submitItemAtPosition(0);
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
				if (charAt == COMMIT_CHAR_COMMA || charAt == COMMIT_CHAR_SEMICOLON)
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
				ChipEntry entry = createTokenizedEntry(text);
				if (entry != null)
				{
					QwertyKeyListener.MarkAsReplaced(editable, start, end, text.ToString());
					ICharSequence chipText = createChip(entry, false);
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
				sanitizeBetween();
				return true;
			}
			}
			return false;
		}

		// Visible for testing.
		/* package */

		private void sanitizeBetween()
		{
			// Don't sanitize while we are waiting for content to chipify.
			//if (mPendingChipsCount > 0)
			//{
			//	return;
			//}
			// Find the last chip.
			DrawableChipSpan[] recips = getSortedVisibleRecipients();
			if (recips != null && recips.Length > 0)
			{
				DrawableChipSpan last = recips[recips.Length - 1];
				DrawableChipSpan beforeLast = null;
				if (recips.Length > 1)
				{
					beforeLast = recips[recips.Length - 2];
				}
				int startLooking = 0;
				int end = getSpannable().GetSpanStart((Object)last);
				if (beforeLast != null)
				{
					startLooking = getSpannable().GetSpanEnd((Object)beforeLast);
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

		private bool shouldCreateChip(int start, int end)
		{
			return !mNoChips && HasFocus && EnoughToFilter() && !alreadyHasChip(start, end) && !notEnoughCharactersWhenTrimmed(start, end);
		}

		private bool notEnoughCharactersWhenTrimmed(int start, int end)
		{
			return Text.JavaSubstring(start, end).Trim().Length < Threshold;
		}

		private bool alreadyHasChip(int start, int end)
		{
			if (mNoChips)
			{
				return true;
			}
			var chips = getSpannable().GetSpans(start, end, Class.FromType(typeof(DrawableChipSpan))).Cast<DrawableChipSpan>().ToArray();
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
			if (mSelectedChip != null && keyCode == Keycode.Del)
			{
				if (mAlternatesPopup != null && mAlternatesPopup.IsShowing)
				{
					mAlternatesPopup.Dismiss();
				}
				removeChip(mSelectedChip);
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
						if (mSelectedChip != null)
						{
							clearSelectedChip();
							return true;
						}
						else if (focusNext())
						{
							return true;
						}
					}
					break;
			}

			return base.OnKeyDown(keyCode, @event);
		}

		// Visible for testing.
		/* package */

		internal ISpannable getSpannable()
		{
			return (ISpannable)TextFormatted;
		}

		private int getChipStart(DrawableChipSpan chipSpan)
		{
			return getSpannable().GetSpanStart(chipSpan);
		}

		private int getChipEnd(DrawableChipSpan chipSpan)
		{
			return getSpannable().GetSpanEnd(chipSpan);
		}

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
			if (getRecipients().Length >= mMaxChipsAllowed)
			{
				return;
			}

			bool isCompletedToken = IsCompletedToken(text);
			if (EnoughToFilter() && !isCompletedToken)
			{
				int end = SelectionEnd;
				int start = mTokenizer.FindTokenStart(text, end);

				// If it does not contain at least two non-blank chars, does not filter
				if (notEnoughCharactersWhenTrimmed(start, end))
				{
					return;
				}

				// If this is a RecipientChip, don't filter
				// on its contents.
				ISpannable span = getSpannable();
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


		// Visible for testing.
		/*package*/
		private bool IsCompletedToken(ICharSequence text)
		{
			if (TextUtils.IsEmpty(text))
			{
				return false;
			}
			// Check to see if this is a completed token before filtering.
			int end = text.Length();
			int start = mTokenizer.FindTokenStart(text, end);
			//String token = new String(text.ToString().Substring(start, end).Trim());
			//String token = new String(text.ToString().SubString(start, end).Trim());
			String token = text.ToString().JavaSubstring(start, end).Trim();
			if (!TextUtils.IsEmpty(token))
			{
				//char atEnd = token.CharAt(token.Length() - 1);
				char atEnd = token.ElementAt(token.Length - 1);
				return atEnd == COMMIT_CHAR_COMMA || atEnd == COMMIT_CHAR_SEMICOLON;
			}
			return false;
		}

		internal void clearSelectedChip()
		{
			if (mSelectedChip != null)
			{
				unselectChip(mSelectedChip);
				mSelectedChip = null;
			}
			SetCursorVisible(true);
		}

		public bool IgnoreTouchEvents;
		private IListAdapter _adapter;

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
			if (mSelectedChip == null)
			{
				mGestureDetector.OnTouchEvent(@event);
			}
			//if (mCopyAddress == null && action ==   MotionEventActions.Up) {
			if (action == MotionEventActions.Up)
			{
				float x = @event.GetX();
				float y = @event.GetY();
				int offset = putOffsetInRange(x, y);
				DrawableChipSpan currentChipSpan = findChip(offset);
				if (currentChipSpan != null)
				{
					if (action == MotionEventActions.Up)
					{
						if (mSelectedChip != null && mSelectedChip != currentChipSpan)
						{
							clearSelectedChip();
							mSelectedChip = selectChip(currentChipSpan);
						}
						else if (mSelectedChip == null)
						{
							SetSelection(Text.Length);
							//commitDefault();
							mSelectedChip = selectChip(currentChipSpan);
						}
						else
						{
							onClick(mSelectedChip, offset, x, y);
						}
					}
					chipWasSelected = true;
					handled = true;
				}
				else if (mSelectedChip != null && shouldShowEditableText(mSelectedChip))
				{
					chipWasSelected = true;
				}
			}
			if (action == MotionEventActions.Up && !chipWasSelected)
			{
				clearSelectedChip();
			}
			return handled;
		}

		private void scrollLineIntoView(int line)
		{
			if (mScrollView != null)
			{
				mScrollView.SmoothScrollBy(0, calculateOffsetFromBottom(line));
			}
		}

		//private void showAlternates(DrawableRecipientChip currentChip,
		//							ListPopupWindow alternatesPopup, int width)
		//{
		//	// protected ListAdapter doInBackground(final Void... params) {
		//	//	return createAlternatesAdapter(currentChip);
		//	//}

		//	if (!mAttachedToWindow)
		//	{
		//		return;
		//	}

		//	int line = Layout.GetLineForOffset(getChipStart(currentChip));

		//	int bottom;
		//	if (line == LineCount - 1)
		//	{
		//		bottom = 0;
		//	}
		//	else
		//	{
		//		bottom = -(int) ((mChipHeight + (2*mLineSpacingExtra))*(Math.Abs(LineCount - 1 - line)));
		//	}

		//	// Align the alternates popup with the left side of the View,
		//	// regardless of the position of the chip tapped.
		//	alternatesPopup.Width = width;
		//	alternatesPopup.AnchorView = this;
		//	alternatesPopup.VerticalOffset = bottom;
		//	//alternatesPopup.SetAdapter(result);
		//	alternatesPopup.SetOnItemClickListener(mAlternatesListener);
		//	// Clear the checked item.

		//	mCheckedItem = -1;
		//	alternatesPopup.Show();
		//	ListView listView = alternatesPopup.ListView;
		//	listView.ChoiceMode = ChoiceMode.Single;
		//	// Checked item would be -1 if the adapter has not
		//	// loaded the view that should be checked yet. The
		//	// variable will be set correctly when onCheckedItemChanged
		//	// is called in a separate thread.
		//	if (mCheckedItem != -1)
		//	{
		//		listView.SetItemChecked(mCheckedItem, true);
		//		mCheckedItem = -1;
		//	}
		//}

		private IListAdapter createAlternatesAdapter(DrawableChipSpan chipSpan)
		{
			//return new RecipientAlternatesAdapter(getContext(), chip.getContactId(),
			//									  chip.getDirectoryId(), chip.getLookupKey(), chip.getDataId(),
			//									  getAdapter().getQueryType(), this, mDropdownChipLayouter);
			return null;
		}

		private IListAdapter createSingleAddressAdapter(DrawableChipSpan currentChipSpan)
		{
			return new SingleRecipientArrayAdapter(Context, 0);
			//return new SingleRecipientArrayAdapter(getContext(), currentChip.getEntry(),
			//									   mDropdownChipLayouter);
			return null;
		}

		//@Override
		//public void onCheckedItemChanged(int position)
		//{
		//	ListView listView = mAlternatesPopup.ListView;
		//	if (listView != null && listView.CheckedItemCount == 0)
		//	{
		//		listView.SetItemChecked(position, true);
		//	}
		//	mCheckedItem = position;
		//}

		private int putOffsetInRange(float x, float y)
		{
			int offset;

			if (Build.VERSION.SdkInt >= BuildVersionCodes.IceCreamSandwich)
			{
				offset = GetOffsetForPosition(x, y);
			}
			else
			{
				offset = supportGetOffsetForPosition(x, y);
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
			while (offset >= 0 && findText(editable, offset) == -1 && findChip(offset) == null)
			{
				// Keep walking backward!
				offset--;
			}
			return offset;
		}

		private static int findText(IEditable text, int offset)
		{
			if (text.CharAt(offset) != ' ')
			{
				return offset;
			}
			return -1;
		}

		private DrawableChipSpan findChip(int offset)
		{
			var chips = getSpannable().GetSpans(0, Text.Length, Class.FromType(typeof(DrawableChipSpan))).Cast<DrawableChipSpan>().ToArray();

			// Find the chip that contains this offset.
			for (int i = 0; i < chips.Length; i++)
			{
				DrawableChipSpan chipSpan = chips[i];
				int start = getChipStart(chipSpan);
				int end = getChipEnd(chipSpan);
				if (offset >= start && offset <= end)
				{
					return chipSpan;
				}
			}
			return null;
		}

		// Visible for testing.
		// Use this method to generate text to add to the list of addresses.
		/* package */

		private String createAddressText(ChipEntry entry)
		{
			
			String display = entry.getDisplayName();
			String address = entry.getDestination();
			if (TextUtils.IsEmpty(display) || TextUtils.Equals(display, address))
			{
				display = null;
			}

			String trimmedDisplayText;
			//if (isPhoneQuery() && isPhoneNumber(address))
			//{
			//	trimmedDisplayText = new String(address.Trim());
			//}
			//else
			//{
			if (address != null)
			{
				// Tokenize out the address in case the address already
				// contained the username as well.
				Rfc822Token[] tokenized = Rfc822Tokenizer.Tokenize(address);
				if (tokenized != null && tokenized.Length > 0)
				{
					//address = new String(tokenized[0].Address);
					address = tokenized[0].Address;
				}
			}
			Rfc822Token token = new Rfc822Token(display, address, null);
			//trimmedDisplayText = new String(token.ToString().Trim());
			trimmedDisplayText = token.ToString().Trim();
			//}

			int index = trimmedDisplayText.IndexOf(",");
			if (mTokenizer != null && !TextUtils.IsEmpty(trimmedDisplayText) && index < trimmedDisplayText.Length - 1)
			{
				return mTokenizer.TerminateToken(trimmedDisplayText);
			}
			else
			{
				return trimmedDisplayText;
			}
		}

		// Visible for testing.
		// Use this method to generate text to display in a chip.
		/*package*/

		private String createChipDisplayText(ChipEntry entry)
		{
			String display = entry.getDisplayName();
			String address = entry.getDestination();
			if (TextUtils.IsEmpty(display) || TextUtils.Equals(display, address))
			{
				display = null;
			}
			if (!TextUtils.IsEmpty(display))
			{
				return display;
			}
			else if (!TextUtils.IsEmpty(address))
			{
				return address;
			}
			else
			{
				return new Rfc822Token(display, address, null).ToString();
			}
		}

		private ICharSequence createChip(ChipEntry entry, bool pressed)
		{
			String displayText = createAddressText(entry);

			if (TextUtils.IsEmpty(displayText))
			{
				return null;
			}
			SpannableString chipText;
			// Always leave a blank space at the end of a chip.
			int textLength = displayText.Length - 1;
			chipText = new SpannableString(displayText);
			if (!mNoChips)
			{
				try
				{
					DrawableChipSpan chipSpan = constructChipSpan(entry, pressed, false /* leave space for contact icon */);
					chipText.SetSpan(chipSpan, 0, textLength, SpanTypes.ExclusiveExclusive);
					chipSpan.setOriginalText(chipText.ToString());
				}
				catch (NullPointerException e)
				{
					//Log.e(TAG, e.getMessage(), e);
					return null;
				}
			}
			return chipText;
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
			submitItemAtPosition(position);

			if (itemSelectedListener != null)
			{
				itemSelectedListener.onItemSelected();
			}
		}

		private void submitItemAtPosition(int position)
		{
			//var entry = createValidatedEntry((ChipEntry)Adapter.GetItem(position));
			//var entry = ((ChipEntry)Adapter.GetItem(position));
			//submitItem(entry);
			//submitItem(new String("SomeUserName"), new String("Number"));
		}

		//public void submitItem(string name, string number)
		//	public void submitItem(ChipEntry entry)
		//{
		//	//RecipientEntry entry = RecipientEntry.constructGeneratedEntry(name, number, true);
		//	//ChipEntry entry = new ChipEntry(name, number, null);
		//	submitItem(entry);
		//}

		//public void submitItem(String name, String number, Uri imageUri)
		//{
		//	RecipientEntry entry = RecipientEntry.constructGeneratedEntry(name, number, imageUri, true);
		//	submitItem(entry);
		//}

		//public void submitItem(String name, String number, Uri imageUri, byte[] photoBytes)
		//{
		//	RecipientEntry entry = RecipientEntry.constructGeneratedEntry(name, number, imageUri, photoBytes, true);
		//	submitItem(entry);
		//}

		protected void submitItem(ChipEntry entry)
		{
			if (entry == null || getRecipients().Length >= mMaxChipsAllowed)
			{
				//SetError(mChipOverLimitErrorHint);
				return;
			}
			ClearComposingText();

			int end = SelectionEnd;
			int start = mTokenizer.FindTokenStart(Text, end);

			IEditable editable = EditableText;
			QwertyKeyListener.MarkAsReplaced(editable, start, end, editable.SubSequence(start, end));
			ICharSequence chip = createChip(entry, false);
			if (chip != null && start >= 0 && end >= 0)
			{
				editable.Replace(start, end, chip);
			}
			sanitizeBetween();
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

		/** Returns a collection of contact Id for each chip inside this View. */
		/* package */

		//private Collection<Long> getContactIds()
		//{
		//	var result = new Collection<Long>();
		//	DrawableRecipientChip[] chips = getSortedVisibleRecipients();
		//	if (chips != null)
		//	{
		//		foreach (var chip in chips)
		//		{
		//			result.Add(new Long(chip.getContactId()));
		//		}

		//	}
		//	return result;
		//}


		/** Returns a collection of data Id for each chip inside this View. May be null. */
		/* package */

		//private Collection<Long> getDataIds()
		//{
		//	//var result = new HashSet<Long>();
		//	var result = new Collection<Long>();
		//	DrawableRecipientChip[] chips = getSortedVisibleRecipients();
		//	if (chips != null)
		//	{
		//		foreach (var chip in chips)
		//		{
		//			result.Add(new Long(chip.getDataId()));
		//		}


		//	}
		//	return result;
		//}

		public DrawableChipSpan[] getRecipients()
		{
			var recipients = getSpannable()
				.GetSpans(0, Text.Length, Class.FromType(typeof(DrawableChipSpan)))
				.Cast<DrawableChipSpan>()
				.ToArray();

			var recipientsList = new List<DrawableChipSpan>(recipients.ToList());

			if (mRemovedSpans != null)
				recipientsList.AddRange(mRemovedSpans);

			return recipientsList.ToArray();
		}

		/**
     * Returns a list containing the sorted visible recipients.
     * @return Array of DrawableRecipientChip containing the sorted visible recipients
     */

		public DrawableChipSpan[] getSortedVisibleRecipients()
		{
			var recips = getSpannable().GetSpans(0, Text.Length, Class.FromType(typeof(DrawableChipSpan))).Cast<DrawableChipSpan>();
			List<DrawableChipSpan> recipientsList = recips.ToList();
			//ISpannable spannable = getSpannable();
			//	Collections.Sort(recipientsList, new Comparator<DrawableRecipientChip>()
			//	{


			//	public int compare(DrawableRecipientChip first,
			//		DrawableRecipientChip second) {
			//										 int firstStart = spannable.getSpanStart(first);
			//										 int secondStart = spannable.getSpanStart(second);
			//										 if (firstStart < secondStart) {
			//										 return -1;
			//	}
			//else
			//	if (firstStart > secondStart)
			//	{
			//		return 1;
			//	}
			//	else
			//	{
			//		return 0;
			//	}
			//}
			//}
			//)
			//	;
			//return recipientsList.toArray(new DrawableRecipientChip[recipientsList.size()]);
			return recipientsList.ToArray();
		}

		/**
     * Returns a list containing all sorted recipients, even the hidden ones when the field
     * is shrinked.
     * @return Array of DrawableRecipientChip containing all sorted recipients
     */
		// TODO: check if everything is working properly
		public DrawableChipSpan[] getSortedRecipients()
		{
			DrawableChipSpan[] recipients = getSortedVisibleRecipients();
			//List<DrawableRecipientChip> recipientsList = new List<>(Arrays.asList(recipients));
			List<DrawableChipSpan> recipientsList = recipients.ToList();

			// Recreate each removed span.
			if (mRemovedSpans != null && mRemovedSpans.Count > 0)
			{
				// Start the search for tokens after the last currently visible
				// chip.
				int end = getSpannable().GetSpanEnd(recipients[recipients.Length - 1]);
				IEditable editable = EditableText;

				foreach (var chip in mRemovedSpans)
				{
					int chipStart;
					String token;
					// Need to find the location of the chip, again.
					token = chip.getOriginalText();
					// As we find the matching recipient for the remove spans,
					// reduce the size of the string we need to search.
					// That way, if there are duplicates, we always find the correct
					// recipient.
					chipStart = editable.ToString().IndexOf(token.ToString(), end);
					end = Math.Min(editable.Length(), chipStart + token.Length);
					// Only set the span if we found a matching token.
					if (chipStart != -1)
					{
						recipientsList.Add(chip);
					}
				}
			}
			//return recipientsList.toArray(new DrawableRecipientChip[recipientsList.size()]);
			return recipientsList.ToArray();
		}

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

		/**
     * No chips are selectable.
     */
		//@Override
		//public bool onCreateActionMode(ActionMode mode, Menu menu)
		//{
		//	return false;
		//}
		public bool OnCreateActionMode(ActionMode mode, IMenu menu)
		{
			return false;
		}

		// Visible for testing.
		/* package */

		private ImageSpan getMoreChip()
		{
			MoreImageSpan[] moreSpans = (MoreImageSpan[])getSpannable().GetSpans(0, Text.Length, Class.FromType(typeof(MoreImageSpan))).Cast<MoreImageSpan>();
			return moreSpans != null && moreSpans.Length > 0 ? moreSpans[0] : null;
		}

		private MoreImageSpan createMoreSpan(int count)
		{
			var moreText = Java.Lang.String.Format(mMoreItem.Text, count);

			TextPaint morePaint = new TextPaint(Paint);
			morePaint.TextSize = mMoreItem.TextSize;
			morePaint.Color = new Color(mMoreItem.CurrentTextColor);
			int width = (int)morePaint.MeasureText(moreText) + mMoreItem.PaddingLeft
						+ mMoreItem.PaddingRight;

			int height;
			int adjustedHeight;
			Layout layout = Layout;
			if (layout != null)
			{
				height = -layout.GetLineAscent(0);
				// The +1 takes into account the rounded int, that can make the text being cropped
				adjustedHeight = height - layout.GetLineDescent(0) + 1;
			}
			else
			{
				height = LineHeight;
				adjustedHeight = height;
			}

			Bitmap drawable = Bitmap.CreateBitmap(width, height, Bitmap.Config.Argb8888);
			Canvas canvas = new Canvas(drawable);
			canvas.DrawText(moreText, 0, moreText.Length, 0, adjustedHeight, morePaint);

			Drawable result = new BitmapDrawable(Resources, drawable);
			result.SetBounds(0, 0, width, height);
			return new MoreImageSpan(result);
		}

		// Visible for testing.
		/*package*/
		private void createMoreChipPlainText()
		{
			// Take the first <= CHIP_LIMIT addresses and get to the end of the second one.
			IEditable text = EditableText;
			int start = 0;
			int end = start;
			for (int i = 0; i < CHIP_LIMIT; i++)
			{
				end = movePastTerminators(mTokenizer.FindTokenEnd(text, start));
				start = end; // move to the next token and get its end.
			}
			// Now, count total addresses.
			int tokenCount = countTokens(text);
			MoreImageSpan moreSpan = createMoreSpan(tokenCount - CHIP_LIMIT);
			SpannableString chipText = new SpannableString(text.SubSequence(end, text.Length()));
			chipText.SetSpan(moreSpan, 0, chipText.Length(), SpanTypes.ExclusiveExclusive);
			text.Replace(end, text.Length(), chipText);
			mMoreChip = moreSpan;
		}

		// Visible for testing.
		/* package */
		private int countTokens(IEditable text)
		{
			int tokenCount = 0;
			int start = 0;
			while (start < text.Length())
			{
				start = movePastTerminators(mTokenizer.FindTokenEnd(text, start));
				tokenCount++;
				if (start >= text.Length())
				{
					break;
				}
			}
			return tokenCount;
		}

		/**
     * Create the more chip. The more chip is text that replaces any chips that
     * do not fit in the pre-defined available space when the
     * RecipientEditTextView loses focus.
     */
		// Visible for testing.
		/* package */
		private void createMoreChip()
		{
			if (mNoChips)
			{
				createMoreChipPlainText();
				return;
			}

			if (!mShouldShrink)
			{
				return;
			}
			var tempMore = getSpannable().GetSpans(0, Text.Length, Class.FromType(typeof(MoreImageSpan))).Cast<ImageSpan>().ToArray();
			if (tempMore.Length > 0)
			{
				getSpannable().RemoveSpan(tempMore[0]);
			}
			var recipients = getSortedVisibleRecipients();

			int fieldWidth = Width - PaddingLeft - PaddingRight;
			// Compute the width of a blank space because there should be one between each chip
			TextPaint fieldPaint = new TextPaint(Paint);
			fieldPaint.TextSize = TextSize;
			int blankSpaceWidth = (int)fieldPaint.MeasureText(" ");

			int totalChipLength = 0;
			int chipLimit = 0;
			for (int i = 0; i < recipients.Length; i++)
			{
				if (totalChipLength + recipients[i].getBounds().Right + blankSpaceWidth < (fieldWidth * getShrinkMaxLines()))
				{
					totalChipLength += recipients[i].getBounds().Right + blankSpaceWidth;
					chipLimit = i + 1;
				}
				else
				{
					break;
				}
			}

			if (recipients == null || recipients.Length <= chipLimit)
			{
				mMoreChip = null;
				return;
			}

			ISpannable spannable = getSpannable();
			int numRecipients = recipients.Length;
			int overage = numRecipients - chipLimit;

			// Now checks if the moreSpan is not too big for the available space
			String moreText = String.Format(mMoreItem.Text, overage);
			TextPaint morePaint = new TextPaint(Paint);
			morePaint.TextSize = mMoreItem.TextSize;
			int moreChipWidth = (int)morePaint.MeasureText(moreText) + mMoreItem.PaddingLeft + mMoreItem.PaddingRight;

			MoreImageSpan moreSpan;
			while (totalChipLength + moreChipWidth >= (fieldWidth * getShrinkMaxLines()))
			{
				totalChipLength -= recipients[chipLimit - 1].getBounds().Right;
				chipLimit--;
				overage++;
				moreText = String.Format(mMoreItem.Text, overage);
				moreChipWidth = (int)morePaint.MeasureText(moreText) + mMoreItem.PaddingLeft
								+ mMoreItem.PaddingRight;
			}
			moreSpan = createMoreSpan(overage);

			mRemovedSpans = new List<DrawableChipSpan>();
			int totalReplaceStart = 0;
			int totalReplaceEnd = 0;
			IEditable text = EditableText;

			for (int i = numRecipients - overage; i < numRecipients; i++)
			{
				mRemovedSpans.Add(recipients[i]);
				if (i == numRecipients - overage)
				{
					totalReplaceStart = spannable.GetSpanStart(recipients[i]);
				}
				if (i == recipients.Length - 1)
				{
					totalReplaceEnd = spannable.GetSpanEnd(recipients[i]);
				}
				//if (mTemporaryRecipients == null || !mTemporaryRecipients.Contains(recipients[i]))
				//{
				//	var r1 = recipients[i].getDisplay();
				//	int spanStart = spannable.GetSpanStart(recipients[i]);
				//	int spanEnd = spannable.GetSpanEnd(recipients[i]);
				//	var bar = new String(text.ToString().Substring(spanStart, spanEnd));
				//	recipients[i].setOriginalText(bar);
				//}
				spannable.RemoveSpan(recipients[i]);
			}
			if (totalReplaceEnd < text.Length())
			{
				totalReplaceEnd = text.Length();
			}
			int end = Math.Max(totalReplaceStart, totalReplaceEnd);
			int start = Math.Min(totalReplaceStart, totalReplaceEnd);
			SpannableString chipText = new SpannableString(text.SubSequence(start, end));
			chipText.SetSpan(moreSpan, 0, chipText.Length(), SpanTypes.ExclusiveExclusive);
			text.Replace(start, end, chipText);
			mMoreChip = moreSpan;
			// If adding the +more chip goes over the limit, resize accordingly.


			//if (!isPhoneQuery() && LineCount > mMaxLines)
			//{
			//	setMaxLines(getLineCount());
			//}
		}

		/**
     * Replace the more chip, if it exists, with all of the recipient chips it had
     * replaced when the RecipientEditTextView gains focus.
     */
		public void removeMoreChip()
		{
			if (mMoreChip != null)
			{
				ISpannable span = getSpannable();
				span.RemoveSpan(mMoreChip);
				mMoreChip = null;
				// Re-add the spans that were removed.
				if (mRemovedSpans != null && mRemovedSpans.Count > 0)
				{
					// Recreate each removed span.
					DrawableChipSpan[] recipients = getSortedVisibleRecipients();
					// Start the search for tokens after the last currently visible
					// chip.
					if (recipients == null || recipients.Length == 0)
					{
						return;
					}
					int end = span.GetSpanEnd((Object)recipients[recipients.Length - 1]);
					IEditable editable = EditableText;

					foreach (var chip in mRemovedSpans)
					{
						int chipStart;
						int chipEnd;
						string token;
						// Need to find the location of the chip, again.
						token = (String)chip.getOriginalText();
						// As we find the matching recipient for the remove spans,
						// reduce the size of the string we need to search.
						// That way, if there are duplicates, we always find the correct
						// recipient.
						//chipStart = editable.ToString().indexOf(token, end);
						chipStart = editable.ToString().IndexOf(token, end);
						end = chipEnd = Math.Min(editable.Length(), chipStart + token.Length);
						// Only set the span if we found a matching token.
						if (chipStart != -1)
						{
							editable.SetSpan(chip, chipStart, chipEnd, SpanTypes.ExclusiveExclusive);
						}
					}

					mRemovedSpans.Clear();
				}
			}
		}

		/**
     * Show specified chip as selected. If the RecipientChip is just an email address,
     * selecting the chip will take the contents of the chip and place it at
     * the end of the RecipientEditTextView for inline editing. If the
     * RecipientChip is a complete contact, then selecting the chip
     * will change the background color of the chip, show the delete icon,
     * and a popup window with the address in use highlighted and any other
     * alternate addresses for the contact.
     * @param currentChip Chip to select.
     * @return A RecipientChip in the selected state or null if the chip
     * just contained an email address.
     */

		private DrawableChipSpan selectChip(DrawableChipSpan currentChipSpan)
		{
			IEditable editable = EditableText;
			//if (shouldShowEditableText(currentChip))
			//{
			//	ICharSequence text = currentChip.getValue();
			//	//IEditable editable = EditableText;
			//	ISpannable spannable = getSpannable();
			//	int spanStart = spannable.GetSpanStart(currentChip);
			//	int spanEnd = spannable.GetSpanEnd(currentChip);
			//	spannable.RemoveSpan(currentChip);
			//	editable.Delete(spanStart, spanEnd);
			//	SetCursorVisible(true);
			//	SetSelection(editable.Length());
			//	editable.Append(text);
			//	return constructChipSpan(RecipientEntry.constructFakeEntry((String) text, IsValid(new String(text.ToString()))),
			//		true, false);
			//}

			int start = getChipStart(currentChipSpan);
			int end = getChipEnd(currentChipSpan);
			getSpannable().RemoveSpan(currentChipSpan);
			DrawableChipSpan newChipSpan;
			try
			{
				if (mNoChips)
				{
					return null;
				}
				newChipSpan = constructChipSpan(currentChipSpan.getEntry(), true, false);
			}
			catch (NullPointerException e)
			{
				//Log.e(TAG, e.getMessage(), e);
				return null;
			}
			//IEditable editable = EditableText;
			QwertyKeyListener.MarkAsReplaced(editable, start, end, "");
			if (start == -1 || end == -1)
			{
				//Log.d(TAG, "The chip being selected no longer exists but should.");
			}
			else
			{
				editable.SetSpan(newChipSpan, start, end, SpanTypes.ExclusiveExclusive);
			}
			newChipSpan.setSelected(true);
			if (shouldShowEditableText(newChipSpan))
			{
				scrollLineIntoView(Layout.GetLineForOffset(getChipStart(newChipSpan)));
			}
			showAddress(newChipSpan, mAddressPopup, Width);
			SetCursorVisible(false);
			return newChipSpan;

			//else if (currentChip.getContactId() == RecipientEntry.GENERATED_CONTACT)
			//{
			//	int start = getChipStart(currentChip);
			//	int end = getChipEnd(currentChip);
			//	getSpannable().RemoveSpan(currentChip);
			//	DrawableRecipientChip newChip;
			//	try
			//	{
			//		if (mNoChips)
			//		{
			//			return null;
			//		}
			//		newChip = constructChipSpan(currentChip.getEntry(), true, false);
			//	}
			//	catch (NullPointerException e)
			//	{
			//		//Log.e(TAG, e.getMessage(), e);
			//		return null;
			//	}
			//	IEditable editable = EditableText;
			//	QwertyKeyListener.MarkAsReplaced(editable, start, end, "");
			//	if (start == -1 || end == -1)
			//	{
			//		//Log.d(TAG, "The chip being selected no longer exists but should.");
			//	}
			//	else
			//	{
			//		editable.SetSpan(newChip, start, end, SpanTypes.ExclusiveExclusive);
			//	}
			//	newChip.setSelected(true);
			//	if (shouldShowEditableText(newChip))
			//	{
			//		scrollLineIntoView(Layout.GetLineForOffset(getChipStart(newChip)));
			//	}
			//	showAddress(newChip, mAddressPopup, Width);
			//	SetCursorVisible(false);
			//	return newChip;
			//}
			//else
			//{
			//	int start = getChipStart(currentChip);
			//	int end = getChipEnd(currentChip);
			//	getSpannable().RemoveSpan(currentChip);
			//	DrawableRecipientChip newChip;
			//	try
			//	{
			//		newChip = constructChipSpan(currentChip.getEntry(), true, false);
			//	}
			//	catch (NullPointerException e)
			//	{
			//		//Log.e(TAG, e.getMessage(), e);
			//		return null;
			//	}
			//	IEditable editable = EditableText;
			//	QwertyKeyListener.MarkAsReplaced(editable, start, end, "");
			//	if (start == -1 || end == -1)
			//	{
			//		//Log.d(TAG, "The chip being selected no longer exists but should.");
			//	}
			//	else
			//	{
			//		editable.SetSpan(newChip, start, end, SpanTypes.ExclusiveExclusive);
			//	}
			//	newChip.setSelected(true);
			//	if (shouldShowEditableText(newChip))
			//	{
			//		scrollLineIntoView(Layout.GetLineForOffset(getChipStart(newChip)));
			//	}
			//	showAlternates(newChip, mAlternatesPopup, Width);
			//	SetCursorVisible(false);
			//	return newChip;
			//}
		}

		protected bool shouldShowEditableText(DrawableChipSpan currentChipSpan)
		{
			return false;
			//long contactId = currentChip.getContactId();
			//return contactId == RecipientEntry.INVALID_CONTACT || (!isPhoneQuery() && contactId == RecipientEntry.GENERATED_CONTACT);
		}

		private void showAddress(DrawableChipSpan currentChipSpan, ListPopupWindow popup,
								 int width)
		{
			if (!mAttachedToWindow)
			{
				return;
			}
			int line = Layout.GetLineForOffset(getChipStart(currentChipSpan));
			int bottom = calculateOffsetFromBottom(line);
			// Align the alternates popup with the left side of the View,
			// regardless of the position of the chip tapped.
			popup.Width = width;
			popup.AnchorView = this;
			popup.VerticalOffset = bottom;
			popup.SetAdapter(createSingleAddressAdapter(currentChipSpan));
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
     * Remove selection from this chip. Unselecting a RecipientChip will render
     * the chip without a delete icon and with an unfocused background. This is
     * called when the RecipientChip no longer has focus.
     */

		private void unselectChip(DrawableChipSpan chipSpan)
		{
			int start = getChipStart(chipSpan);
			int end = getChipEnd(chipSpan);
			IEditable editable = EditableText;
			mSelectedChip = null;
			if (start == -1 || end == -1)
			{
				//Log.w(TAG, "The chip doesn't exist or may be a chip a user was editing");
				SetSelection(editable.Length());
				//commitDefault();
			}
			else
			{
				getSpannable().RemoveSpan(chipSpan);
				QwertyKeyListener.MarkAsReplaced(editable, start, end, "");
				editable.RemoveSpan(chipSpan);
				try
				{
					if (!mNoChips)
					{
						editable.SetSpan(constructChipSpan(chipSpan.getEntry(), false, false), start, end, SpanTypes.ExclusiveExclusive);
					}
				}
				catch (NullPointerException e)
				{
					//Log.e(TAG, e.getMessage(), e);
				}
			}
			SetCursorVisible(true);
			SetSelection(editable.Length());
			if (mAlternatesPopup != null && mAlternatesPopup.IsShowing)
			{
				mAlternatesPopup.Dismiss();
			}
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

		private bool isInDelete(DrawableChipSpan chipSpan, int offset, float x, float y)
		{
			// Figure out the bounds of this chip and whether or not
			// the user clicked in the X portion.
			// TODO: Should x and y be used, or removed?
			if (mDisableDelete)
			{
				return false;
			}

			return chipSpan.isSelected() &&
				   ((mAvatarPosition == AVATAR_POSITION_END && offset == getChipEnd(chipSpan)) ||
					(mAvatarPosition != AVATAR_POSITION_END && offset == getChipStart(chipSpan)));
		}

		/**
     * Remove the chip and any text associated with it from the RecipientEditTextView.
     */
		// Visible for testing.
		/* package */

		protected void removeChip(DrawableChipSpan chipSpan)
		{
			ISpannable spannable = getSpannable();
			int spanStart = spannable.GetSpanStart((Object)chipSpan);
			int spanEnd = spannable.GetSpanEnd((Object)chipSpan);
			IEditable text = EditableText;
			int toDelete = spanEnd;
			bool wasSelected = chipSpan == mSelectedChip;
			// Clear that there is a selected chip before updating any text.
			if (wasSelected)
			{
				mSelectedChip = null;
			}
			// Always remove trailing spaces when removing a chip.
			while (toDelete >= 0 && toDelete < text.Length() && text.CharAt(toDelete) == ' ')
			{
				toDelete++;
			}
			spannable.RemoveSpan(chipSpan);
			if (spanStart >= 0 && toDelete > 0)
			{
				text.Delete(spanStart, toDelete);
			}
			if (wasSelected)
			{
				clearSelectedChip();
			}
		}

		/**
     * Replace this currently selected chip with a new chip
     * that uses the contact data provided.
     */
		// Visible for testing.
		/*package*/

		private void replaceChip(DrawableChipSpan chipSpan, ChipEntry entry)
		{
			bool wasSelected = chipSpan == mSelectedChip;
			if (wasSelected)
			{
				mSelectedChip = null;
			}
			int start = getChipStart(chipSpan);
			int end = getChipEnd(chipSpan);
			getSpannable().RemoveSpan(chipSpan);
			IEditable editable = EditableText;
			ICharSequence chipText = createChip(entry, false);
			if (chipText != null)
			{
				if (start == -1 || end == -1)
				{
					//Log.e(TAG, "The chip to replace does not exist but should.");
					editable.Insert(0, chipText);
				}
				else
				{
					if (!TextUtils.IsEmpty(chipText))
					{
						// There may be a space to replace with this chip's new
						// associated space. Check for it
						int toReplace = end;
						while (toReplace >= 0 && toReplace < editable.Length()
							   && editable.CharAt(toReplace) == ' ')
						{
							toReplace++;
						}
						editable.Replace(start, toReplace, chipText);
					}
				}
			}
			SetCursorVisible(true);
			if (wasSelected)
			{
				clearSelectedChip();
			}
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
				if (isInDelete(chipSpan, offset, x, y))
				{
					removeChip(chipSpan);
				}
				else
				{
					clearSelectedChip();
				}
			}
		}

		//internal bool chipsPending()
		//{
		//	return mPendingChipsCount > 0 || (mRemovedSpans != null && mRemovedSpans.Count > 0);
		//}

		//public override void RemoveTextChangedListener(ITextWatcher watcher)
		//{
		//	mTextWatcher = null;
		//	base.RemoveTextChangedListener(watcher);
		//}

		//RecipientTextWatcher
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
				var repl = getSpannable().GetSpans(selStart, selStart, Class.FromType(typeof(DrawableChipSpan))).Cast<DrawableChipSpan>().ToArray();

				if (repl.Length > 0)
				{
					// There is a chip there! Just remove it.
					IEditable editable = EditableText;

					// Add the separator token.
					int tokenStart = 0;
					if (selStart == editable.Length())
					{
						tokenStart = mTokenizer.FindTokenStart(editable, selStart);
					}
					else if (selStart != 0)
					{
						tokenStart = mTokenizer.FindTokenStart(editable, selStart - 1);
					}
					int tokenEnd = mTokenizer.FindTokenEnd(editable, tokenStart);

					// If start and end are set on the token instead of the recipient,
					// look for another starting point
					if (tokenEnd != 0 && tokenStart == tokenEnd)
					{
						tokenStart = mTokenizer.FindTokenStart(editable, tokenEnd - 1);
					}

					// Increments the tokenEnd to include the commit character.
					tokenEnd = tokenEnd + 1;
					if (tokenEnd > editable.Length())
					{
						tokenEnd = editable.Length();
					}
					editable.Delete(tokenStart, tokenEnd);
					getSpannable().RemoveSpan(repl[0]);
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
				ISpannable spannable = getSpannable();
				var chips = spannable.GetSpans(0, Text.Length, Class.FromType(typeof(DrawableChipSpan))).Cast<DrawableChipSpan>().ToArray();

				foreach (var chip in chips)
				{
					spannable.RemoveSpan(chip);
				}

				if (mMoreChip != null)
				{
					spannable.RemoveSpan(mMoreChip);
				}
				clearSelectedChip();
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
				if (lastCharacterIsCommitCharacter(s))
				{
					commitByCharacter();
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
				if (last == COMMIT_CHAR_SPACE)
				{
					//if (!isPhoneQuery())
					//{
					// Check if this is a valid email address. If it is,
					// commit it.
					int tokenStart = mTokenizer.FindTokenStart(Text, SelectionEnd);
					var sub = Text.JavaSubstring(tokenStart, mTokenizer.FindTokenEnd(Text, tokenStart));

					if (!TextUtils.IsEmpty(sub) && mValidator != null && mValidator.IsValid(sub))
					{
						commitByCharacter();
					}
					//}
				}
			}
		}

		public bool enoughRoomForAdditionalChip()
		{
			return getRecipients().Count() < mMaxChipsAllowed;
		}

		public bool lastCharacterIsCommitCharacter(ICharSequence s)
		{
			char last;
			int end = SelectionEnd == 0 ? 0 : SelectionEnd - 1;
			last = s.CharAt(end);
			return last == COMMIT_CHAR_COMMA || last == COMMIT_CHAR_SEMICOLON;
		}

		/**
     * Removes the commit char that triggers the creation of the token before using the
     * enoughToFilter method.
     */

		private void removeCommitCharBeforeCreatingChip()
		{
			//RemoveTextChangedListener(mTextWatcher);
			AfterTextChanged -= OnAfterTextChanged;
			TextChanged -= OnTextChanged;

			IEditable s = EditableText;
			char last;
			int end = SelectionEnd == 0 ? 0 : SelectionEnd - 1;
			last = s.CharAt(end);
			if (last == COMMIT_CHAR_COMMA || last == COMMIT_CHAR_SEMICOLON)
			{
				s.Delete(end, end + 1);
			}

			TextChanged += OnTextChanged;
			AfterTextChanged += OnAfterTextChanged;
			//mHandler.Post(mAddTextWatcher);
		}

		//public bool isGeneratedContact(DrawableRecipientChip chip)
		//{
		//	long contactId = chip.getContactId();
		//	return contactId == RecipientEntry.INVALID_CONTACT || (!isPhoneQuery() && contactId == RecipientEntry.GENERATED_CONTACT);
		//}

		/**
     * Handles pasting a {@link ClipData} to this {@link RecipientEditTextView}.
     */

		//private void handlePasteClip(ClipData clip)
		//{
		//	removeTextChangedListener(mTextWatcher);

		//	if (clip != null && clip.getDescription().hasMimeType(ClipDescription.MIMETYPE_TEXT_PLAIN))
		//	{
		//		for (int i = 0; i < clip.getItemCount(); i++)
		//		{
		//			CharSequence paste = clip.getItemAt(i).getText();
		//			if (paste != null)
		//			{
		//				int start = getSelectionStart();
		//				int end = getSelectionEnd();
		//				Editable editable = getText();
		//				if (start >= 0 && end >= 0 && start != end)
		//				{
		//					editable.append(paste, start, end);
		//				}
		//				else
		//				{
		//					editable.insert(end, paste);
		//				}
		//				handlePasteAndReplace();
		//			}
		//		}
		//	}

		//	mHandler.Post(mAddTextWatcher);
		//}

		//@Override
		//public bool onTextContextMenuItem(int id)
		//{
		//	if (id == android.R.id.paste)
		//	{
		//		ClipboardManager clipboard = (ClipboardManager) getContext().getSystemService(
		//			Context.CLIPBOARD_SERVICE);
		//		handlePasteClip(clipboard.getPrimaryClip());
		//		return true;
		//	}
		//	return super.onTextContextMenuItem(id);
		//}
		public override bool OnTextContextMenuItem(int id)
		{
			//if (id == Android.Resource.Id.Paste)
			//{
			//	ClipboardManager clipboard = (ClipboardManager) getContext().getSystemService(Context.CLIPBOARD_SERVICE);
			//	handlePasteClip(clipboard.getPrimaryClip());
			//	return true;
			//}
			return base.OnTextContextMenuItem(id);
		}

		//private void handlePasteAndReplace()
		//{
		//	List<DrawableRecipientChip> created = handlePaste();
		//	if (created != null && created.size() > 0)
		//	{
		//		// Perform reverse lookups on the pasted contacts.
		//		IndividualReplacementTask replace = new IndividualReplacementTask();
		//		replace.execute(created);
		//	}
		//}

		// Visible for testing.
		/* package */
		//private List<DrawableRecipientChip> handlePaste()
		//{
		//	String text = getText().toString();
		//	int originalTokenStart = mTokenizer.findTokenStart(text, getSelectionEnd());
		//	String lastAddress = text.substring(originalTokenStart);
		//	int tokenStart = originalTokenStart;
		//	int prevTokenStart = 0;
		//	DrawableRecipientChip findChip = null;
		//	List<DrawableRecipientChip> created = new List<DrawableRecipientChip>();
		//	if (tokenStart != 0)
		//	{
		//		// There are things before this!
		//		while (tokenStart != 0 && findChip == null && tokenStart != prevTokenStart)
		//		{
		//			prevTokenStart = tokenStart;
		//			tokenStart = mTokenizer.findTokenStart(text, tokenStart);
		//			findChip = findChip(tokenStart);
		//			if (tokenStart == originalTokenStart && findChip == null)
		//			{
		//				break;
		//			}
		//		}
		//		if (tokenStart != originalTokenStart)
		//		{
		//			if (findChip != null)
		//			{
		//				tokenStart = prevTokenStart;
		//			}
		//			int tokenEnd;
		//			DrawableRecipientChip createdChip;
		//			while (tokenStart < originalTokenStart)
		//			{
		//				tokenEnd = movePastTerminators(mTokenizer.findTokenEnd(getText().toString(),
		//																	   tokenStart));
		//				commitChip(tokenStart, tokenEnd, getText());
		//				createdChip = findChip(tokenStart);
		//				if (createdChip == null)
		//				{
		//					break;
		//				}
		//				// +1 for the space at the end.
		//				tokenStart = getSpannable().getSpanEnd(createdChip) + 1;
		//				created.add(createdChip);
		//			}
		//		}
		//	}
		//	// Take a look at the last token. If the token has been completed with a
		//	// commit character, create a chip.
		//	if (isCompletedToken(lastAddress))
		//	{
		//		Editable editable = getText();
		//		tokenStart = editable.toString().indexOf(lastAddress, originalTokenStart);
		//		commitChip(tokenStart, editable.length(), editable);
		//		created.add(findChip(tokenStart));
		//	}
		//	return created;
		//}

		// Visible for testing.
		/* package */

		private int movePastTerminators(int tokenEnd)
		{
			if (tokenEnd >= Length())
			{
				return tokenEnd;
			}
			char atEnd = Text.ElementAt(tokenEnd);
			if (atEnd == COMMIT_CHAR_COMMA || atEnd == COMMIT_CHAR_SEMICOLON)
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

		//private class RecipientReplacementTask extends AsyncTask<Void, Void, Void> {

		//private class IndividualReplacementTask

		//@Override
		public bool OnDown(MotionEvent e)
		{
			return false;
		}

		//@Override
		public bool OnFling(MotionEvent e1, MotionEvent e2, float velocityX, float velocityY)
		{
			// Do nothing.
			return false;
		}

		//@Override
		public void OnLongPress(MotionEvent @event)
		{
			//if (mSelectedChip != null)
			//{
			//	return;
			//}
			//float x = @event.getX();
			//float y = @event.getY();
			//int offset = putOffsetInRange(x, y);
			//DrawableRecipientChip currentChip = findChip(offset);
			//if (currentChip != null)
			//{
			//	if (mDragEnabled)
			//	{
			//		// Start drag-and-drop for the selected chip.
			//		startDrag(currentChip);
			//	}
			//	else
			//	{
			//		// Copy the selected chip email address.
			//		showCopyDialog(currentChip.getEntry().getDestination());
			//	}
			//}
		}

		// The following methods are used to provide some functionality on older versions of Android
		// These methods were copied out of JB MR2's TextView
		/////////////////////////////////////////////////
		private int supportGetOffsetForPosition(float x, float y)
		{
			if (Layout == null)
				return -1;

			int line = supportGetLineAtCoordinate(y);
			int offset = supportGetOffsetAtCoordinate(line, x);
			return offset;
		}

		private float supportConvertToLocalHorizontalCoordinate(float x)
		{
			x -= TotalPaddingLeft;
			// Clamp the position to inside of the view.
			x = Math.Max(0.0f, x);
			x = Math.Min(Width - TotalPaddingRight - 1, x);
			x += ScrollX;
			return x;
		}

		private int supportGetLineAtCoordinate(float y)
		{
			y -= TotalPaddingLeft;
			// Clamp the position to inside of the view.
			y = Math.Max(0.0f, y);
			y = Math.Min(Height - TotalPaddingBottom - 1, y);
			y += ScrollY;
			return Layout.GetLineForVertical((int)y);
		}

		private int supportGetOffsetAtCoordinate(int line, float x)
		{
			x = supportConvertToLocalHorizontalCoordinate(x);
			return Layout.GetOffsetForHorizontal(line, x);
		}

		/////////////////////////////////////////////////

		/**
     * Enables drag-and-drop for chips.
     */

		//public void enableDrag()
		//{
		//	mDragEnabled = true;
		//}

		/**
     * Starts drag-and-drop for the selected chip.
     */

		//private void startDrag(DrawableRecipientChip currentChip)
		//{
		//	String address = currentChip.getEntry().getDestination();
		//	ClipData data = ClipData.newPlainText(address, address + COMMIT_CHAR_COMMA);

		//	// Start drag mode.
		//	startDrag(data, new RecipientChipShadow(currentChip), null, 0);

		//	// Remove the current chip, so drag-and-drop will result in a move.
		//	// TODO (phamm): consider readd this chip if it's dropped outside a target.
		//	removeChip(currentChip);
		//}

		/**
     * Handles drag event.
     */
		//public bool onDragEvent(DragEvent @event)
		//{
		//	switch (@event.getAction())
		//	{
		//		case DragEvent.ACTION_DRAG_STARTED:
		//			// Only handle plain text drag and drop.
		//			return @event.getClipDescription().hasMimeType(ClipDescription.MIMETYPE_TEXT_PLAIN);
		//		case DragEvent.ACTION_DRAG_ENTERED:
		//			requestFocus();
		//			return true;
		//		case DragEvent.ACTION_DROP:
		//			handlePasteClip(@event.getClipData());
		//			return true;
		//	}
		//	return false;
		//}

		//private final class RecipientChipShadow extends DragShadowBuilder {

		//private void showCopyDialog(String address)
		//{
		//	if (!mAttachedToWindow)
		//	{
		//		return;
		//	}
		//	mCopyAddress = address;
		//	mCopyDialog.setTitle(address);
		//	mCopyDialog.setContentView(R.layout.copy_chip_dialog_layout);
		//	mCopyDialog.setCancelable(true);
		//	mCopyDialog.setCanceledOnTouchOutside(true);
		//	Button button = (Button) mCopyDialog.findViewById(android.R.id.button1);
		//	button.setOnClickListener(this);
		//	int btnTitleId;
		//	if (isPhoneQuery())
		//	{
		//		btnTitleId = R.
		//		string.copy_number;
		//	}
		//	else
		//	{
		//		btnTitleId = R.
		//		string.copy_email;
		//	}
		//	String buttonTitle = getContext().getResources().getString(btnTitleId);
		//	button.setText(buttonTitle);
		//	mCopyDialog.setOnDismissListener(this);
		//	mCopyDialog.show();
		//}

		//@Override
		public bool OnScroll(MotionEvent e1, MotionEvent e2, float distanceX, float distanceY)
		{
			// Do nothing.
			return false;
		}

		//@Override
		public void OnShowPress(MotionEvent e)
		{
			// Do nothing.
		}

		//@Override
		public bool OnSingleTapUp(MotionEvent e)
		{
			// Do nothing.
			return false;
		}

		//@Override
		//public void OnDismiss(DialogInterface dialog)
		//{

		//}
		public void OnDismiss()
		{
			//mCopyAddress = null;
		}


		//@Override
		public void OnClick(View v)
		{
			// Copy this to the clipboard.
			//ClipboardManager clipboard =  (ClipboardManager) Context.GetSystemService(Context.ClipboardService);
			//clipboard.setPrimaryClip(ClipData.newPlainText("", mCopyAddress));
			//mCopyDialog.dismiss();
		}

		//protected bool isPhoneQuery()
		//{
		//	return false;
		//	//return Adapter != null && Adapter.getQueryType() == BaseRecipientAdapter.QUERY_TYPE_PHONE;
		//}


		//public BaseRecipientAdapter getAdapter()
		//{
		//	return (BaseRecipientAdapter) super.getAdapter();
		//}

		public void clearRecipients()
		{
			// Clear the text field
			Text = "";

			// Clear chips that might have been stored
			if (mRemovedSpans != null)
			{
				mRemovedSpans.Clear();
			}
			//mTemporaryRecipients = null;
			mSelectedChip = null;

		}

		public override void DismissDropDown()
		{
			if (mDismissPopupOnClick)
			{
				base.DismissDropDown();
			}
		}

		public void dismissDropDownOnItemSelected(bool dismiss)
		{
			mDismissPopupOnClick = dismiss;
		}

		public void setPostSelectedAction(ItemSelectedListener listener)
		{
			itemSelectedListener = listener;
		}

		
	}
}
