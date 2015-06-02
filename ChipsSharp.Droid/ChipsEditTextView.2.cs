using System.Collections.Generic;
using System.Linq;
using Android.Graphics;
using Android.Graphics.Drawables;
using Android.OS;
using Android.Text;
using Android.Text.Method;
using Android.Text.Util;
using Android.Views;
using Android.Widget;
using com.android.ex.chips.Spans;
using ChipsSharp;
using Java.Lang;
using Math = Java.Lang.Math;
using String = System.String;

namespace com.android.ex.chips
{
	public partial class ChipsEditTextView
	{
		private ISpannable Spannable
		{
			get { return (ISpannable) TextFormatted; }
		}

		private DrawableChipSpan[] VisibleChipSpans
		{
			get
			{
				return Spannable.GetSpans(0, Text.Length, Class.FromType(typeof (DrawableChipSpan)))
					.Cast<DrawableChipSpan>()
					.ToArray();
			}
		}

		private DrawableChipSpan[] AllChipSpans
		{
			get
			{
				var chipSpans = VisibleChipSpans;

				var recipientsList = new List<DrawableChipSpan>(chipSpans.ToList());

				if (_removedSpans != null)
					recipientsList.AddRange(_removedSpans);

				return recipientsList.ToArray();
			}
		}

		private DrawableChipSpan LastChip
		{
			get
			{
				DrawableChipSpan last = null;
				var chipSpans = AllChipSpans;
				if (chipSpans != null && chipSpans.Length > 0)
				{
					last = chipSpans[chipSpans.Length - 1];
				}
				return last;
			}
		}

		private bool EnoughRoomForAdditionalChip
		{
			get
			{
				return AllChipSpans.Count() < _maxChipsAllowed;
			}
		}

		private int GetChipStart(DrawableChipSpan chipSpan)
		{
			return Spannable.GetSpanStart(chipSpan);
		}

		private int GetChipEnd(DrawableChipSpan chipSpan)
		{
			return Spannable.GetSpanEnd(chipSpan);
		}

		private DrawableChipSpan FindChip(int offset)
		{
			var chipSpans = VisibleChipSpans;
			
			for (var i = 0; i < chipSpans.Length; i++)
			{
				var chipSpan = chipSpans[i];
				var start = GetChipStart(chipSpan);
				var end = GetChipEnd(chipSpan);
				if (offset >= start && offset <= end)
				{
					return chipSpan;
				}
			}
			return null;
		}

		protected List<IChipEntry> GetChipEntries()
		{
			var chipEntries = new List<IChipEntry>();
			var chipSpans = AllChipSpans;
			foreach (var drawableChipSpan in chipSpans)
			{
				IChipEntry chipEntry = drawableChipSpan.getEntry();
				chipEntries.Add(chipEntry);
			}
			return chipEntries;
		}

		private DrawableChipSpan SelectChip(DrawableChipSpan currentChipSpan)
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

			int start = GetChipStart(currentChipSpan);
			int end = GetChipEnd(currentChipSpan);
			Spannable.RemoveSpan(currentChipSpan);
			DrawableChipSpan newChipSpan;
			try
			{
				//if (_noChips)
				//{
				//	return null;
				//}
				newChipSpan = ConstructChipSpan(currentChipSpan.getEntry(), true, false);
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
			if (ShouldShowEditableText(newChipSpan))
			{
				ScrollLineIntoView(Layout.GetLineForOffset(GetChipStart(newChipSpan)));
			}
			ShowAddress(newChipSpan, _addressPopup, Width);
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

		private void UnselectChip(DrawableChipSpan chipSpan)
		{
			int start = GetChipStart(chipSpan);
			int end = GetChipEnd(chipSpan);
			IEditable editable = EditableText;
			_selectedChip = null;
			if (start == -1 || end == -1)
			{
				//Log.w(TAG, "The chip doesn't exist or may be a chip a user was editing");
				SetSelection(editable.Length());
				//commitDefault();
			}
			else
			{
				Spannable.RemoveSpan(chipSpan);
				QwertyKeyListener.MarkAsReplaced(editable, start, end, "");
				editable.RemoveSpan(chipSpan);
				try
				{
					if (!_noChips)
					{
						editable.SetSpan(ConstructChipSpan(chipSpan.getEntry(), false, false), start, end, SpanTypes.ExclusiveExclusive);
					}
				}
				catch (NullPointerException e)
				{
					//Log.e(TAG, e.getMessage(), e);
				}
			}
			SetCursorVisible(true);
			SetSelection(editable.Length());
			if (_alternatesPopup != null && _alternatesPopup.IsShowing)
			{
				_alternatesPopup.Dismiss();
			}
		}

		private void ReplaceChip(DrawableChipSpan chipSpan, IChipEntry entry)
		{
			bool wasSelected = chipSpan == _selectedChip;
			if (wasSelected)
			{
				_selectedChip = null;
			}
			int start = GetChipStart(chipSpan);
			int end = GetChipEnd(chipSpan);
			Spannable.RemoveSpan(chipSpan);
			IEditable editable = EditableText;
			ICharSequence chipText = CreateChip(entry, false);
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
				ClearSelectedChip();
			}
		}

		private void RemoveChip(DrawableChipSpan chipSpan)
		{
			ISpannable spannable = Spannable;
			int spanStart = spannable.GetSpanStart((Object)chipSpan);
			int spanEnd = spannable.GetSpanEnd((Object)chipSpan);
			IEditable text = EditableText;
			int toDelete = spanEnd;
			bool wasSelected = chipSpan == _selectedChip;
			// Clear that there is a selected chip before updating any text.
			if (wasSelected)
			{
				_selectedChip = null;
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
				ClearSelectedChip();
			}
		}

		private void ClearSelectedChip()
		{
			if (_selectedChip != null)
			{
				UnselectChip(_selectedChip);
				_selectedChip = null;
			}
			SetCursorVisible(true);
		}

		private void ClearChipSpans()
		{
			// Clear the text field
			Text = "";

			// Clear chips that might have been stored
			if (_removedSpans != null)
			{
				_removedSpans.Clear();
			}
			//mTemporaryRecipients = null;
			_selectedChip = null;
		}

		private ICharSequence CreateChip(IChipEntry entry, bool pressed)
		{
			String displayText = CreateAddressText(entry);

			if (TextUtils.IsEmpty(displayText))
			{
				return null;
			}
			SpannableString chipText;
			// Always leave a blank space at the end of a chip.
			int textLength = displayText.Length - 1;
			chipText = new SpannableString(displayText);
			if (!_noChips)
			{
				try
				{
					DrawableChipSpan chipSpan = ConstructChipSpan(entry, pressed, false /* leave space for contact icon */);
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

		private DrawableChipSpan ConstructChipSpan(IChipEntry contact, bool pressed, bool leaveIconSpace)
		{
			if (_chipBackground == null)
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
				tmpBitmap = CreateSelectedChip(contact, paint);
			}
			else
			{
				tmpBitmap = CreateUnselectedChip(contact, paint, leaveIconSpace);
			}

			// Get the inset of the chip
			int chipInset = (int)(Resources.GetDimension(Resource.Dimension.line_spacing_extra) / 2);

			// Pass the full text, un-ellipsized, to the chip.
			Drawable result = new BitmapDrawable(Resources, tmpBitmap);
			result = new InsetDrawable(result, 0, chipInset, 0, chipInset);
			result.SetBounds(0, 0, tmpBitmap.Width, tmpBitmap.Height + 2 * chipInset);

			//DrawableRecipientChip recipientChip = new VisibleRecipientChip(result, contact, getImageSpanAlignment());
			DrawableChipSpan chipSpan = new VisibleChipSpan(result, contact, _imageSpanAlignment);
			// Return text to the original size.
			paint.TextSize = defaultSize;
			paint.Color = new Color(defaultColor);
			return chipSpan;
		}

		private Bitmap CreateSelectedChip(IChipEntry contact, TextPaint paint)
		{
			paint.Color = new Color(_selectedTextColor);
			Bitmap photo;
			if (_disableDelete)
			{
				// Show the avatar instead if we don't want to delete
				photo = GetAvatarIcon(contact);
			}
			else
			{
				photo = ((BitmapDrawable)_chipDelete).Bitmap;
			}
			return CreateChipBitmap(contact, paint, photo, _chipBackgroundPressed);
		}

		private Bitmap CreateUnselectedChip(IChipEntry contact, TextPaint paint,
											bool leaveBlankIconSpacer)
		{
			Drawable background = _chipBackground;
			Bitmap photo = GetAvatarIcon(contact);
			paint.Color = Context.Resources.GetColor(Android.Resource.Color.Black);
			return CreateChipBitmap(contact, paint, photo, background);
		}

		//TODO: fix
		protected virtual Bitmap GetAvatarIcon(IChipEntry contact)
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
			//		return _defaultContactPhoto;
			//	}
			//}

			return NoAvatarPicture;
		}

		private Bitmap CreateChipBitmap(IChipEntry contact, TextPaint paint, Bitmap icon, Drawable background)
		{
			if (background == null)
			{
				//Log.w(TAG, "Unable to draw a background for the chips as it was never set");
				return Bitmap.CreateBitmap(
					(int)_chipHeight * 2, (int)_chipHeight, Bitmap.Config.Argb8888);
			}

			Rect backgroundPadding = new Rect();
			background.GetPadding(backgroundPadding);

			// Ellipsize the text so that it takes AT MOST the entire width of the
			// autocomplete text entry area. Make sure to leave space for padding
			// on the sides.
			int height = (int)_chipHeight + Resources.GetDimensionPixelSize(Resource.Dimension.extra_chip_height);

			// Compute the space needed by the more chip before ellipsizing
			//String moreText = new String(String.Format(mMoreItem.Text, mMaxChipsAllowed));
			String moreText = String.Format(_moreItem.Text, _maxChipsAllowed);
			TextPaint morePaint = new TextPaint(Paint);
			morePaint.TextSize = _moreItem.TextSize;
			int moreChipWidth = (int)morePaint.MeasureText(moreText) + _moreItem.PaddingLeft + _moreItem.PaddingRight;

			// Since the icon is a square, it's width is equal to the maximum height it can be inside
			// the chip.
			int iconWidth = height - backgroundPadding.Top - backgroundPadding.Bottom;
			float[] widths = new float[1];
			paint.GetTextWidths(" ", widths);

			var ellipsizedText = EllipsizeText(CreateChipDisplayText(contact), paint,
											   CalculateAvailableWidth() - iconWidth - widths[0] -
											   backgroundPadding.Left - backgroundPadding.Right -
											   moreChipWidth);
			int textWidth = (int)paint.MeasureText(ellipsizedText, 0, ellipsizedText.Length);

			// Make sure there is a minimum chip width so the user can ALWAYS
			// tap a chip without difficulty.
			int width = Math.Max(iconWidth * 2, textWidth + (_chipPadding * 2) + iconWidth
											  + backgroundPadding.Left + backgroundPadding.Right);

			// Create the background of the chip.
			Bitmap tmpBitmap = Bitmap.CreateBitmap(width, height, Bitmap.Config.Argb8888);
			Canvas canvas = new Canvas(tmpBitmap);

			// Draw the background drawable
			background.SetBounds(height / 2, 0, width, height);
			background.Draw(canvas);
			// Draw the text vertically aligned
			var x = ShouldPositionAvatarOnRight()
				? _chipPadding + backgroundPadding.Left
				: width - backgroundPadding.Right - _chipPadding - textWidth;
			var y = GetTextYOffset(ellipsizedText, paint, height);
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
				DrawIconOnCanvas(icon, canvas, paint, src, dst);
			}
			return tmpBitmap;
		}

		private float GetTextYOffset(String text, TextPaint paint, int height)
		{
			Rect bounds = new Rect();
			paint.GetTextBounds(text, 0, text.Length, bounds);
			int textHeight = bounds.Bottom - bounds.Top;
			return height - ((height - textHeight) / 2) - (int)paint.Descent() / 2;
		}

		private void DrawIconOnCanvas(Bitmap icon, Canvas canvas, Paint paint, RectF src, RectF dst)
		{
			Matrix matrix = new Matrix();
			matrix.SetRectToRect(src, dst, Matrix.ScaleToFit.Fill);
			canvas.DrawBitmap(icon, matrix, paint);
		}

		private bool ShouldPositionAvatarOnRight()
		{
			bool isRtl = Build.VERSION.SdkInt >= BuildVersionCodes.JellyBeanMr1 && LayoutDirection == LayoutDirection.Rtl;
			bool assignedPosition = _avatarPosition == AvatarPositionEnd;
			// If in Rtl mode, the position should be flipped.
			return isRtl ? !assignedPosition : assignedPosition;
		}

		private string CreateAddressText(IChipEntry entry)
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
			if (_tokenizer != null && !TextUtils.IsEmpty(trimmedDisplayText) && index < trimmedDisplayText.Length - 1)
			{
				return _tokenizer.TerminateToken(trimmedDisplayText);
			}
			else
			{
				return trimmedDisplayText;
			}
		}

		private string CreateChipDisplayText(IChipEntry entry)
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


		private void SanitizeEnd()
		{
			// Don't sanitize while we are waiting for pending chips to complete.
			//if (mPendingChipsCount > 0)
			//{
			//	return;
			//}
			// Find the last chip; eliminate any commit characters after it.
			//DrawableChipSpan[] chipSpans = getSortedVisibleRecipients();
			DrawableChipSpan[] chipSpans = AllChipSpans;
			ISpannable spannable = Spannable;
			if (chipSpans != null && chipSpans.Length > 0)
			{
				int end;
				_moreChip = GetMoreChip();
				if (_moreChip != null)
				{
					end = spannable.GetSpanEnd(_moreChip);
				}
				else
				{
					end = Spannable.GetSpanEnd(LastChip);
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

		private void CheckChipWidths()
		{
			// Check the widths of the associated chips.
			//DrawableChipSpan[] chipSpans = getSortedVisibleRecipients();
			DrawableChipSpan[] chipSpans = AllChipSpans;
			if (chipSpans != null)
			{
				Rect bounds;
				foreach (var chip in chipSpans)
				{
					bounds = chip.getBounds();
					if (Width > 0 && bounds.Right - bounds.Left > Width - PaddingLeft - PaddingRight)
					{
						// Need to redraw that chip.
						ReplaceChip(chip, chip.getEntry());
					}
				}

			}
		}
	}


	/**
     * Returns a list containing the sorted visible recipients.
     * @return Array of DrawableRecipientChip containing the sorted visible recipients
     */
	//public DrawableChipSpan[] getSortedVisibleRecipients()
	//{
	//	var recips = getSpannable().GetSpans(0, Text.Length, Class.FromType(typeof(DrawableChipSpan)))
	//		.Cast<DrawableChipSpan>();
	//	List<DrawableChipSpan> recipientsList = recips.ToList();
	//	//ISpannable spannable = getSpannable();
	//	//	Collections.Sort(recipientsList, new Comparator<DrawableRecipientChip>()
	//	//	{


	//	//	public int compare(DrawableRecipientChip first,
	//	//		DrawableRecipientChip second) {
	//	//										 int firstStart = spannable.getSpanStart(first);
	//	//										 int secondStart = spannable.getSpanStart(second);
	//	//										 if (firstStart < secondStart) {
	//	//										 return -1;
	//	//	}
	//	//else
	//	//	if (firstStart > secondStart)
	//	//	{
	//	//		return 1;
	//	//	}
	//	//	else
	//	//	{
	//	//		return 0;
	//	//	}
	//	//}
	//	//}
	//	//)
	//	//	;
	//	//return recipientsList.toArray(new DrawableRecipientChip[recipientsList.size()]);
	//	return recipientsList.ToArray();
	//}

	/**
 * Returns a list containing all sorted recipients, even the hidden ones when the field
 * is shrinked.
 * @return Array of DrawableRecipientChip containing all sorted recipients
 */
	// TODO: check if everything is working properly
	//public DrawableChipSpan[] getSortedRecipients()
	//{
	//	//DrawableChipSpan[] chipSpans = getSortedVisibleRecipients();
	//	DrawableChipSpan[] chipSpans = getChipSpans();
	//	//List<DrawableRecipientChip> recipientsList = new List<>(Arrays.asList(recipients));
	//	List<DrawableChipSpan> recipientsList = chipSpans.ToList();

	//	// Recreate each removed span.
	//	if (mRemovedSpans != null && mRemovedSpans.Count > 0)
	//	{
	//		// Start the search for tokens after the last currently visible
	//		// chip.
	//		int end = getSpannable().GetSpanEnd(chipSpans[chipSpans.Length - 1]);
	//		IEditable editable = EditableText;

	//		foreach (var chip in mRemovedSpans)
	//		{
	//			int chipStart;
	//			String token;
	//			// Need to find the location of the chip, again.
	//			token = chip.getOriginalText();
	//			// As we find the matching recipient for the remove spans,
	//			// reduce the size of the string we need to search.
	//			// That way, if there are duplicates, we always find the correct
	//			// recipient.
	//			chipStart = editable.ToString().IndexOf(token, end);
	//			end = Math.Min(editable.Length(), chipStart + token.Length);
	//			// Only set the span if we found a matching token.
	//			if (chipStart != -1)
	//			{
	//				recipientsList.Add(chip);
	//			}
	//		}
	//	}
	//	//return recipientsList.toArray(new DrawableRecipientChip[recipientsList.size()]);
	//	return recipientsList.ToArray();
	//}

}