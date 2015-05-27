using System.Collections.Generic;
using Android.OS;
using Android.Util;
using com.dbeattie.recipientchip;
using Java.Lang;

namespace com.dbeattie
{
	internal class RecipientReplacementTask : AsyncTask<Void, Void, Void>
	{
		private readonly RecipientEditTextView _editTextView;

		public RecipientReplacementTask(RecipientEditTextView editTextView)
		{
			_editTextView = editTextView;
		}

		private DrawableRecipientChip createFreeChip(RecipientEntry entry)
		{
			try
			{
				if (_editTextView.mNoChips)
				{
					return null;
				}
				return _editTextView.constructChipSpan(entry, false,
						false /*leave space for contact icon */);
			}
			catch (NullPointerException e)
			{
				//Log.e(TAG, e.getMessage(), e);
				return null;
			}
		}

		protected override void OnPreExecute()
		{
			// Ensure everything is in chip-form already, so we don't have text that slowly gets replaced
             List<DrawableRecipientChip> originalRecipients = new List<DrawableRecipientChip>();
             DrawableRecipientChip[] existingChips = _editTextView.getSortedVisibleRecipients();
            for (int i = 0; i < existingChips.length; i++) {
                originalRecipients.add(existingChips[i]);
            }
            if (mRemovedSpans != null) {
                originalRecipients.addAll(mRemovedSpans);
            }

             List<DrawableRecipientChip> replacements =
                    new ArrayList<DrawableRecipientChip>(originalRecipients.size());

            for (final DrawableRecipientChip chip : originalRecipients) {
                if (RecipientEntry.isCreatedRecipient(chip.getEntry().getContactId())
                        && getSpannable().getSpanStart(chip) != -1) {
                    replacements.add(createFreeChip(chip.getEntry()));
                } else {
                    replacements.add(null);
                }
            }

            processReplacements(originalRecipients, replacements);
		}

		protected override Void RunInBackground(params Void[] @params)
		{
			
		}
	}
}