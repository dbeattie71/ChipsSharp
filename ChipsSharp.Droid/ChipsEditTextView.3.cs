using System.Collections.Generic;
using System.Linq;
using Android.Graphics;
using Android.Graphics.Drawables;
using Android.Text;
using Android.Text.Style;
using com.android.ex.chips.Spans;
using Java.Lang;
using Math = Java.Lang.Math;
using String = System.String;

namespace com.android.ex.chips
{
	public partial class ChipsEditTextView
	{
		private void CreateMoreChip()
		{
			if (_noChips)
			{
				CreateMoreChipPlainText();
				return;
			}

			if (!_shouldShrink)
			{
				return;
			}
			var tempMore = Spannable.GetSpans(0, Text.Length, Class.FromType(typeof(MoreImageSpan))).Cast<ImageSpan>().ToArray();
			if (tempMore.Length > 0)
			{
				Spannable.RemoveSpan(tempMore[0]);
			}
			//var recipients = getSortedVisibleRecipients();
			var chipSpans = AllChipSpans;

			int fieldWidth = Width - PaddingLeft - PaddingRight;
			// Compute the width of a blank space because there should be one between each chip
			TextPaint fieldPaint = new TextPaint(Paint);
			fieldPaint.TextSize = TextSize;
			int blankSpaceWidth = (int)fieldPaint.MeasureText(" ");

			int totalChipLength = 0;
			int chipLimit = 0;
			for (int i = 0; i < chipSpans.Length; i++)
			{
				if (totalChipLength + chipSpans[i].getBounds().Right + blankSpaceWidth < (fieldWidth * _shrinkMaxLines))
				{
					totalChipLength += chipSpans[i].getBounds().Right + blankSpaceWidth;
					chipLimit = i + 1;
				}
				else
				{
					break;
				}
			}

			if (chipSpans == null || chipSpans.Length <= chipLimit)
			{
				_moreChip = null;
				return;
			}

			ISpannable spannable = Spannable;
			int numRecipients = chipSpans.Length;
			int overage = numRecipients - chipLimit;

			// Now checks if the moreSpan is not too big for the available space
			String moreText = String.Format(_moreItem.Text, overage);
			TextPaint morePaint = new TextPaint(Paint);
			morePaint.TextSize = _moreItem.TextSize;
			int moreChipWidth = (int)morePaint.MeasureText(moreText) + _moreItem.PaddingLeft + _moreItem.PaddingRight;

			MoreImageSpan moreSpan;
			while (totalChipLength + moreChipWidth >= (fieldWidth * _shrinkMaxLines))
			{
				totalChipLength -= chipSpans[chipLimit - 1].getBounds().Right;
				chipLimit--;
				overage++;
				moreText = String.Format(_moreItem.Text, overage);
				moreChipWidth = (int)morePaint.MeasureText(moreText) + _moreItem.PaddingLeft
								+ _moreItem.PaddingRight;
			}
			moreSpan = CreateMoreSpan(overage);

			_removedSpans = new List<DrawableChipSpan>();
			int totalReplaceStart = 0;
			int totalReplaceEnd = 0;
			IEditable text = EditableText;

			for (int i = numRecipients - overage; i < numRecipients; i++)
			{
				_removedSpans.Add(chipSpans[i]);
				if (i == numRecipients - overage)
				{
					totalReplaceStart = spannable.GetSpanStart(chipSpans[i]);
				}
				if (i == chipSpans.Length - 1)
				{
					totalReplaceEnd = spannable.GetSpanEnd(chipSpans[i]);
				}
				//if (mTemporaryRecipients == null || !mTemporaryRecipients.Contains(recipients[i]))
				//{
				//	var r1 = recipients[i].getDisplay();
				//	int spanStart = spannable.GetSpanStart(recipients[i]);
				//	int spanEnd = spannable.GetSpanEnd(recipients[i]);
				//	var bar = new String(text.ToString().Substring(spanStart, spanEnd));
				//	recipients[i].setOriginalText(bar);
				//}
				spannable.RemoveSpan(chipSpans[i]);
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
			_moreChip = moreSpan;
			// If adding the +more chip goes over the limit, resize accordingly.


			//if (!isPhoneQuery() && LineCount > mMaxLines)
			//{
			//	setMaxLines(getLineCount());
			//}
		}

		private void CreateMoreChipPlainText()
		{
			// Take the first <= CHIP_LIMIT addresses and get to the end of the second one.
			IEditable text = EditableText;
			int start = 0;
			int end = start;
			for (int i = 0; i < ChipLimit; i++)
			{
				end = MovePastTerminators(_tokenizer.FindTokenEnd(text, start));
				start = end; // move to the next token and get its end.
			}
			// Now, count total addresses.
			int tokenCount = CountTokens(text);
			MoreImageSpan moreSpan = CreateMoreSpan(tokenCount - ChipLimit);
			SpannableString chipText = new SpannableString(text.SubSequence(end, text.Length()));
			chipText.SetSpan(moreSpan, 0, chipText.Length(), SpanTypes.ExclusiveExclusive);
			text.Replace(end, text.Length(), chipText);
			_moreChip = moreSpan;
		}

		private MoreImageSpan CreateMoreSpan(int count)
		{
			var moreText = Java.Lang.String.Format(_moreItem.Text, count);

			TextPaint morePaint = new TextPaint(Paint);
			morePaint.TextSize = _moreItem.TextSize;
			morePaint.Color = new Color(_moreItem.CurrentTextColor);
			int width = (int)morePaint.MeasureText(moreText) + _moreItem.PaddingLeft
						+ _moreItem.PaddingRight;

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

		public void RemoveMoreChip()
		{
			if (_moreChip != null)
			{
				ISpannable span = Spannable;
				span.RemoveSpan(_moreChip);
				_moreChip = null;
				// Re-add the spans that were removed.
				if (_removedSpans != null && _removedSpans.Count > 0)
				{
					// Recreate each removed span.
					//DrawableChipSpan[] recipients = getSortedVisibleRecipients();
					DrawableChipSpan[] chipSpans = AllChipSpans;

					// Start the search for tokens after the last currently visible
					// chip.
					if (chipSpans == null || chipSpans.Length == 0)
					{
						return;
					}
					int end = span.GetSpanEnd(chipSpans[chipSpans.Length - 1]);
					IEditable editable = EditableText;

					foreach (var chip in _removedSpans)
					{
						int chipStart;
						int chipEnd;
						string token;
						// Need to find the location of the chip, again.
						token = chip.getOriginalText();
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

					_removedSpans.Clear();
				}
			}
		}

		private ImageSpan GetMoreChip()
		{
			var moreSpans = Spannable.GetSpans(0, Text.Length, Class.FromType(typeof(MoreImageSpan))).Cast<MoreImageSpan>().ToArray();
			return moreSpans.Length > 0 ? moreSpans[0] : null;
		}
	}
}