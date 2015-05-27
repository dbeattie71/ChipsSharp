using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using Android.App;
using Android.Content;
using Android.Graphics;
using Android.OS;
using Android.Runtime;
using Android.Text;
using Android.Text.Util;
using Android.Views;
using Android.Widget;
using Android.Net;
using Uri = Android.Net.Uri;

namespace com.dbeattie
{
	public class DropdownChipLayouter
	{
		  /**
     * The type of adapter that is requesting a chip layout.
     */
    public enum AdapterType {
        BASE_RECIPIENT,
        RECIPIENT_ALTERNATES,
        SINGLE_RECIPIENT
    }

    private  LayoutInflater mInflater;
    private  Context mContext;
    private Queries.Query mQuery;

    public DropdownChipLayouter(LayoutInflater inflater, Context context) {
        mInflater = inflater;
        mContext = context;
    }

    public void setQuery(Queries.Query query) {
        mQuery = query;
    }


    /**
     * Layouts and binds recipient information to the view. If convertView is null, inflates a new
     * view with getItemLaytout().
     *
     * @param convertView The view to bind information to.
     * @param parent The parent to bind the view to if we inflate a new view.
     * @param entry The recipient entry to get information from.
     * @param position The position in the list.
     * @param type The adapter type that is requesting the bind.
     * @param constraint The constraint typed in the auto complete view.
     *
     * @return A view ready to be shown in the drop down list.
     */
    public View bindView(View convertView, ViewGroup parent, RecipientEntry entry, int position,
        AdapterType type, String constraint) {
        // Default to show all the information
        String displayName = entry.getDisplayName();
        String destination = entry.getDestination();
        bool showImage = true;
        string destinationType = getDestinationType(entry);

         View itemView = reuseOrInflateView(convertView, parent, type);

         ViewHolder viewHolder = new ViewHolder(itemView);

        // Hide some information depending on the entry type and adapter type
        switch (type) {
            case AdapterType.BASE_RECIPIENT:
                if (TextUtils.IsEmpty(displayName) || TextUtils.Equals(displayName, destination)) {
                    displayName = destination;

                    // We only show the destination for secondary entries, so clear it only for the
                    // first level.
                    if (entry.isFirstLevel()) {
                        destination = null;
                    }
                }

                if (!entry.isFirstLevel()) {
                    displayName = null;
                    showImage = false;
                }
                break;
            case AdapterType.RECIPIENT_ALTERNATES:
                if (position != 0) {
                    displayName = null;
                    showImage = false;
                }
                break;
            case AdapterType.SINGLE_RECIPIENT:
                destination = Rfc822Tokenizer.Tokenize(entry.getDestination())[0].Address;
                destinationType = null;
		        break;
        }

        if (displayName == null && !showImage) {
            viewHolder.destinationView.SetPadding(mContext.Resources.GetDimensionPixelSize(Resource.Dimension.padding_no_picture), 0, 0, 0);
        } else {
            viewHolder.destinationView.SetPadding(0, 0, 0, 0);
        }

        // Bind the information to the view
        bindTextToView(displayName, viewHolder.displayNameView);
        bindTextToView(destination, viewHolder.destinationView);
        bindTextToView("(" + destinationType + ")", viewHolder.destinationTypeView);
        bindIconToView(showImage, entry, viewHolder.imageView, type);

        return itemView;
    }

    /**
     * Returns a new view with {@link #getItemLayoutResId()}.
     */
    public View newView() {
        return mInflater.Inflate(getItemLayoutResId(), null);
    }

    /**
     * Returns the same view, or inflates a new one if the given view was null.
     */
    protected View reuseOrInflateView(View convertView, ViewGroup parent, AdapterType type) {
        int itemLayout = getItemLayoutResId();
        switch (type) {
            case AdapterType.BASE_RECIPIENT:
            case AdapterType.RECIPIENT_ALTERNATES:
                break;
            case AdapterType.SINGLE_RECIPIENT:
                itemLayout = getAlternateItemLayoutResId();
                break;
        }
        return convertView != null ? convertView : mInflater.Inflate(itemLayout, parent, false);
    }

    /**
     * Binds the text to the given text view. If the text was null, hides the text view.
     */
    protected void bindTextToView(string text, TextView view) {
        if (view == null) {
            return;
        }

        if (text != null) {
            view.Text=text;
           view.Visibility = ViewStates.Visible;
        } else {
            view.Visibility = ViewStates.Gone;
        }
    }

    /**
     * Binds the avatar icon to the image view. If we don't want to show the image, hides the
     * image view.
     */
    protected void bindIconToView(bool showImage, RecipientEntry entry, ImageView view,
        AdapterType type) {
        if (view == null) {
            return;
        }

        if (showImage) {
            switch (type) {
                case AdapterType.BASE_RECIPIENT:
                    byte[] photoBytes = entry.getPhotoBytes();
                    if (photoBytes != null && photoBytes.Length > 0) {
                         Bitmap photo = ChipsUtil.getClip(BitmapFactory.DecodeByteArray(photoBytes, 0,
                            photoBytes.Length));
                        view.SetImageBitmap(photo);
                    } else
					{
						//BaseRecipientAdapter.tryFetchPhoto(entry, mContext.getContentResolver(), null, true, -1);
						//view.SetImageResource(getDefaultPhotoResId());
                    }
                    break;
                case AdapterType.RECIPIENT_ALTERNATES:
                    Uri thumbnailUri = entry.getPhotoThumbnailUri();
                    if (thumbnailUri != null) {
                        // TODO: see if this needs to be done outside the main thread
                        // as it may be too slow to get immediately.
                        view.SetImageURI(thumbnailUri);
                    } else {
                        view.SetImageResource(getDefaultPhotoResId());
                    }
                    break;
                case AdapterType.SINGLE_RECIPIENT:
                default:
                    break;
            }
            view.Visibility = ViewStates.Visible;
        } else
        {
	        view.Visibility = ViewStates.Gone;
        }
    }

    protected string getDestinationType(RecipientEntry entry) {
        return mQuery.getTypeLabel(mContext.Resources, entry.getDestinationType(),
            entry.getDestinationLabel()).ToString().ToUpper();
    }

    /**
     * Returns a layout id for each item inside auto-complete list.
     *
     * Each View must contain two TextViews (for display name and destination) and one ImageView
     * (for photo). Ids for those should be available via {@link #getDisplayNameResId()},
     * {@link #getDestinationResId()}, and {@link #getPhotoResId()}.
     */
    protected int getItemLayoutResId() {
        return Resource.Layout.chips_recipient_dropdown_item;
    }

    /**
     * Returns a layout id for each item inside alternate auto-complete list.
     *
     * Each View must contain two TextViews (for display name and destination) and one ImageView
     * (for photo). Ids for those should be available via {@link #getDisplayNameResId()},
     * {@link #getDestinationResId()}, and {@link #getPhotoResId()}.
     */
    protected int getAlternateItemLayoutResId() {
        return Resource.Layout.chips_alternate_item;
    }

    /**
     * Returns a resource ID representing an image which should be shown when ther's no relevant
     * photo is available.
     */
    protected int getDefaultPhotoResId() {
        return Resource.Drawable.ic_contact_picture;
    }

    /**
     * Returns an id for TextView in an item View for showing a display name. By default
     * {@link android.R.id#title} is returned.
     */
    protected static int getDisplayNameResId() {
        return  Android.Resource.Id.Title;
    }

    /**
     * Returns an id for TextView in an item View for showing a destination
     * (an email address or a phone number).
     * By default {@link android.R.id#text1} is returned.
     */
    protected static int getDestinationResId() {
        return  Android.Resource.Id.Text1;
    }

    /**
     * Returns an id for TextView in an item View for showing the type of the destination.
     * By default {@link android.R.id#text2} is returned.
     */
    protected static int getDestinationTypeResId() {
        return  Android.Resource.Id.Text2;
    }

    /**
     * Returns an id for ImageView in an item View for showing photo image for a person. In default
     * {@link android.R.id#icon} is returned.
     */
    protected static int getPhotoResId()
    {
	    return Android.Resource.Id.Icon;
    }

    /**
     * A holder class the view. Uses the getters in DropdownChipLayouter to find the id of the
     * corresponding views.
     */
    protected class ViewHolder {
        public  TextView displayNameView;
        public  TextView destinationView;
        public  TextView destinationTypeView;
        public  ImageView imageView;

        public ViewHolder(View view) {
            displayNameView = (TextView) view.FindViewById(getDisplayNameResId());
            destinationView = (TextView) view.FindViewById(getDestinationResId());
            destinationTypeView = (TextView) view.FindViewById(getDestinationTypeResId());
            imageView = (ImageView) view.FindViewById(getPhotoResId());
        }
    }
	}
}