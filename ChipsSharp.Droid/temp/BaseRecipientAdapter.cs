using System.Collections.Generic;
using System.Linq;
using Android.Accounts;
using Android.App;
using Android.Content;
using Android.Content.Res;
using Android.Database;
using Android.Net;
using Android.OS;
using Android.Provider;
using Android.Runtime;
using Android.Text;
using Android.Text.Util;
using Android.Util;
using Android.Views;
using Android.Widget;
using CSharpTest.Net.Collections;
using Java.Lang;

namespace com.dbeattie
{
	public class BaseRecipientAdapter : BaseAdapter, IFilterable, AccountSpecifier
	{
		 private static string TAG = "BaseRecipientAdapter";

    private static  bool DEBUG = true;

    /**
     * The preferred number of results to be retrieved. This number may be
     * exceeded if there are several directories configured, because we will use
     * the same limit for all directories.
     */
    private static  int DEFAULT_PREFERRED_MAX_RESULT_COUNT = 10;

    /**
     * The number of extra entries requested to allow for duplicates. Duplicates
     * are removed from the overall result.
     */
    static  int ALLOWANCE_FOR_DUPLICATES = 5;

    // This is ContactsContract.PRIMARY_ACCOUNT_NAME. Available from ICS as hidden
    static  string PRIMARY_ACCOUNT_NAME = "name_for_primary_account";
    // This is ContactsContract.PRIMARY_ACCOUNT_TYPE. Available from ICS as hidden
    static  string PRIMARY_ACCOUNT_TYPE = "type_for_primary_account";

    /** The number of photos cached in this Adapter. */
    private static  int PHOTO_CACHE_SIZE = 200;

    /**
     * The "Waiting for more contacts" message will be displayed if search is not complete
     * within this many milliseconds.
     */
    private static  int MESSAGE_SEARCH_PENDING_DELAY = 1000;
    /** Used to prepare "Waiting for more contacts" message. */
    private static  int MESSAGE_SEARCH_PENDING = 1;

    public static  int QUERY_TYPE_EMAIL = 0;
    public static  int QUERY_TYPE_PHONE = 1;

    private  Queries.Query mQuery;
    private  int mQueryType;

    private bool showMobileOnly = true;

    /**
     * Model object for a {@link Directory} row.
     */
    public  class DirectorySearchParams {
        public  long directoryId;
        public  string directoryType;
        public  string displayName;
        public  string accountName;
        public  string accountType;
        public  string constraint;
        public  DirectoryFilter filter;
    }

		private static class PhotoQuery {
        public static  string[] PROJECTION = {
            ContactsContract.CommonDataKinds.Photo.PhotoColumnId
        };
	//private static class PhotoQuery {
	//	public static  String[] PROJECTION = {
	//		ContactsContract.CommonDataKinds.Photo.PHOTO
	//	};

        public static  int PHOTO = 0;
    }

    protected static class DirectoryListQuery {

        public static  Uri URI =
                Uri.WithAppendedPath(ContactsContract.AuthorityUri, "directories");
        public static string[] PROJECTION = {
          ContactsContract.Directory.InterfaceConsts.Id,              // 0
            ContactsContract.Directory.AccountName,     // 1
            ContactsContract.Directory.AccountType,     // 2
            ContactsContract.Directory.DisplayName,     // 3
            ContactsContract.Directory.PackageName,     // 4
            ContactsContract.Directory.TypeResourceId, // 5
        };

        public static  int ID = 0;
        public static  int ACCOUNT_NAME = 1;
        public static  int ACCOUNT_TYPE = 2;
        public static  int DISPLAY_NAME = 3;
        public static  int PACKAGE_NAME = 4;
        public static  int TYPE_RESOURCE_ID = 5;
    }

    /** Used to temporarily hold results in Cursor objects. */
   protected  class TemporaryEntry {
        public  string displayName;
        public  string destination;
        public  int destinationType;
        public  string destinationLabel;
        public  long contactId;
        public  long? directoryId;
        public  long dataId;
        public  string thumbnailUriString;
        public  int displayNameSource;
        public  string lookupKey;

        public TemporaryEntry(
                string displayName,
                string destination,
                int destinationType,
                string destinationLabel,
                long contactId,
                long? directoryId,
                long dataId,
                string thumbnailUriString,
                int displayNameSource,
                string lookupKey) {
            this.displayName = displayName;
            this.destination = destination;
            this.destinationType = destinationType;
            this.destinationLabel = destinationLabel;
            this.contactId = contactId;
            this.directoryId = directoryId;
            this.dataId = dataId;
            this.thumbnailUriString = thumbnailUriString;
            this.displayNameSource = displayNameSource;
            this.lookupKey = lookupKey;
        }

        public TemporaryEntry(ICursor cursor, long? directoryId) {
            this.displayName = cursor.GetString(Queries.Query.NAME);
            this.destination = cursor.GetString(Queries.Query.DESTINATION);
            this.destinationType = cursor.GetInt(Queries.Query.DESTINATION_TYPE);
            this.destinationLabel = cursor.GetString(Queries.Query.DESTINATION_LABEL);
            this.contactId = cursor.GetLong(Queries.Query.CONTACT_ID);
            this.directoryId = directoryId;
            this.dataId = cursor.GetLong(Queries.Query.DATA_ID);
            this.thumbnailUriString = cursor.GetString(Queries.Query.PHOTO_THUMBNAIL_URI);
            this.displayNameSource = cursor.GetInt(Queries.Query.DISPLAY_NAME_SOURCE);
            this.lookupKey = cursor.GetString(Queries.Query.LOOKUP_KEY);
        }
    }

    /**
     * Used to pass results from {@link DefaultFilter#performFiltering(CharSequence)} to
     * {@link DefaultFilter#publishResults(CharSequence, android.widget.Filter.FilterResults)}
     */
    private class DefaultFilterResult {
        public  List<RecipientEntry> entries;
        public  LurchTable<Long, List<RecipientEntry>> entryMap;
        public  List<RecipientEntry> nonAggregatedEntries;
        public  HashSet<String> existingDestinations;
        public  List<DirectorySearchParams> paramsList;

        public DefaultFilterResult(List<RecipientEntry> entries,
                LurchTable<Long, List<RecipientEntry>> entryMap,
                List<RecipientEntry> nonAggregatedEntries,
                HashSet<String> existingDestinations,
                List<DirectorySearchParams> paramsList) {
            this.entries = entries;
            this.entryMap = entryMap;
            this.nonAggregatedEntries = nonAggregatedEntries;
            this.existingDestinations = existingDestinations;
            this.paramsList = paramsList;
        }
    }

    /**
     * An asynchronous filter used for loading two data sets: email rows from the local
     * contact provider and the list of {@link Directory}'s.
     */
    private class DefaultFilter : Filter {

		 protected override FilterResults PerformFiltering(ICharSequence charSequence)
		 {
			 var constraint = charSequence.ToString();
			// if (DEBUG) {
			//	Log.d(TAG, "start filtering. constraint: " + constraint + ", thread:"
			//			+ Thread.currentThread());
			//}

            if (constraint == null) {
                constraint = "Choose Contacts:";
            }

            FilterResults results = new FilterResults();
            ICursor defaultDirectoryCursor = null;
            ICursor directoryCursor = null;
            bool limitResults = true;

            if (constraint == "Choose Contacts:") {
                limitResults = false;
                constraint = " ";
            }

            try {
                defaultDirectoryCursor = doQuery(constraint, limitResults ? mPreferredMaxResultCount : -1,
                        null /* directoryId */);
	          
                if (defaultDirectoryCursor == null) {
					//if (DEBUG) {
					//	Log.w(TAG, "null cursor returned for default Email filter query.");
					//}
                } else {
                    // These variables will become mEntries, mEntryMap, mNonAggregatedEntries, and
                    // mExistingDestinations. Here we shouldn't use those member variables directly
                    // since this method is run outside the UI thread.
	                var entryMap = new LurchTable<Long, List<RecipientEntry>>(LurchTableOrder.Insertion, 10);
                    var nonAggregatedEntries = new List<RecipientEntry>();
                     var existingDestinations = new HashSet<String>();

                    while (defaultDirectoryCursor.MoveToNext()) {
                        // Note: At this point each entry doesn't contain any photo
                        // (thus getPhotoBytes() returns null).
                        putOneEntry(new TemporaryEntry(defaultDirectoryCursor, null /* directoryId */), true, entryMap, nonAggregatedEntries, existingDestinations);
                    }

                    // We'll copy this result to mEntry in publicResults() (run in the UX thread).
                     List<RecipientEntry> entries = constructEntryList(entryMap, nonAggregatedEntries);

                    // After having local results, check the size of results. If the results are
                    // not enough, we search remote directories, which will take longer time.
                     int limit = mPreferredMaxResultCount - existingDestinations.Count;
                     List<DirectorySearchParams> paramsList;
                    if (limit > 0 && limitResults) {
						//if (DEBUG) {
						//	Log.d(TAG, "More entries should be needed (current: "
						//			+ existingDestinations.size()
						//			+ ", remaining limit: " + limit + ") ");
						//}
	                    directoryCursor = mContentResolver.Query(DirectoryListQuery.URI, DirectoryListQuery.PROJECTION,
	                                                             null, null, null);
                        paramsList = setupOtherDirectories(mContext, directoryCursor, mAccount);
                    } else {
                        // We don't need to search other directories.
                        paramsList = null;
                    }

                    results.Values = new DefaultFilterResult(entries, entryMap, nonAggregatedEntries, existingDestinations, paramsList);
                    results.Count = 1;
                }
            } finally {
                if (defaultDirectoryCursor != null) {
                    defaultDirectoryCursor.Close();
                }
                if (directoryCursor != null) {
                    directoryCursor.Close();
                }
            }
            return results;
	    }

		  protected override void PublishResults(ICharSequence constraint, FilterResults results)
	    {
		   // If a user types a string very quickly and database is slow, "constraint" refers to
            // an older text which shows inconsistent results for users obsolete (b/4998713).
            // TODO: Fix it.
            mCurrentConstraint = constraint;

            clearTempEntries();

            if (results.Values != null) {
                DefaultFilterResult defaultFilterResult = (DefaultFilterResult) results.Values;
                mEntryMap = defaultFilterResult.entryMap;
                mNonAggregatedEntries = defaultFilterResult.nonAggregatedEntries;
                mExistingDestinations = defaultFilterResult.existingDestinations;

                // If there are no local results, in the new result set, cache off what had been
                // shown to the user for use until the first directory result is returned
                if (defaultFilterResult.entries.Count == 0 &&
                        defaultFilterResult.paramsList != null) {
                    cacheCurrentEntries();
                }

                updateEntries(defaultFilterResult.entries);

                // We need to search other remote directories, doing other Filter requests.
                if (defaultFilterResult.paramsList != null) {
                     int limit = mPreferredMaxResultCount -
                            defaultFilterResult.existingDestinations.Count();
                    startSearchOtherDirectories(constraint, defaultFilterResult.paramsList, limit);
                }
            }
	    }

	    public override ICharSequence ConvertResultToStringFormatted(Object resultValue)
	    {
		  	 var entry = (RecipientEntry)resultValue;
			 var displayName = entry.getDisplayName();
			 var emailAddress = entry.getDestination();
			if (TextUtils.IsEmpty(displayName) || TextUtils.Equals(displayName, emailAddress)) {
				 return emailAddress;
			} else
			{
				return new Java.Lang.String(new Rfc822Token(displayName, emailAddress, null).ToString());
			}
	    }
    }

    /**
     * An asynchronous filter that performs search in a particular directory.
     */
	public class DirectoryFilter : Filter {
        private  DirectorySearchParams mParams;
        private int mLimit;

        public DirectoryFilter(DirectorySearchParams paramsx) {
            mParams = paramsx;
        }

        //public synchronized void setLimit(int limit) {
		public void setLimit(int limit) {
            this.mLimit = limit;
        }

        //public synchronized int getLimit() {
		public int getLimit() {
            return this.mLimit;
        }

protected override FilterResults PerformFiltering(ICharSequence constraint)
		{
				//if (DEBUG) {
			//	Log.d(TAG, "DirectoryFilter#performFiltering. directoryId: " + mParams.directoryId
			//			+ ", constraint: " + constraint + ", thread: " + Thread.currentThread());
			//}
			 FilterResults results = new FilterResults();
			results.Values = null;
			results.Count = 0;

			if (!TextUtils.IsEmpty(constraint)) {
				 List<TemporaryEntry> tempEntries = new List<TemporaryEntry>();

				ICursor cursor = null;
				try {
					// We don't want to pass this Cursor object to UI thread (b/5017608).
					// Assuming the result should contain fairly small results (at most ~10),
					// We just copy everything to local structure.
					cursor = doQuery(constraint, getLimit(), mParams.directoryId);

					if (cursor != null) {
						while (cursor.MoveToNext()) {
							tempEntries.Add(new TemporaryEntry(cursor, mParams.directoryId));
						}
					}
				} finally {
					if (cursor != null) {
						cursor.Close();
					}
				}
				if (!tempEntries.isEmpty()) {
					results.Values = tempEntries;
					results.Count = 1;
				}
			}

			//if (DEBUG) {
			//	Log.v(TAG, "finished loading directory \"" + mParams.displayName + "\"" +
			//			" with query " + constraint);
			//}

			return results;
		}

		protected override void PublishResults(ICharSequence constraint, FilterResults results)
		{
			// if (DEBUG) {
			//	Log.d(TAG, "DirectoryFilter#publishResult. constraint: " + constraint
			//			+ ", mCurrentConstraint: " + mCurrentConstraint);
			//}
            mDelayedMessageHandler.removeDelayedLoadMessage();
            // Check if the received result matches the current constraint
            // If not - the user must have continued typing after the request was issued, which
            // means several member variables (like mRemainingDirectoryLoad) are already
            // overwritten so shouldn't be touched here anymore.
            if (TextUtils.Equals(constraint, mCurrentConstraint)) {
                if (results.Count > 0) {
                   // @SuppressWarnings("unchecked")
                     List<TemporaryEntry> tempEntries =
                            (List<TemporaryEntry>) results.Values;

					//for (TemporaryEntry tempEntry : tempEntries) {
					//	putOneEntry(tempEntry, mParams.directoryId == Directory.DEFAULT,
					//			mEntryMap, mNonAggregatedEntries, mExistingDestinations);
					//}

	                foreach (var tempEntry in tempEntries)
	                {
		               putOneEntry(tempEntry, mParams.directoryId == ContactsContract.Directory.Default,
                                mEntryMap, mNonAggregatedEntries, mExistingDestinations);
	                }
                }

                // If there are remaining directories, set up delayed message again.
                mRemainingDirectoryCount--;
                if (mRemainingDirectoryCount > 0) {
					//if (DEBUG) {
					//	Log.d(TAG, "Resend delayed load message. Current mRemainingDirectoryLoad: "
					//			+ mRemainingDirectoryCount);
					//}
                    mDelayedMessageHandler.sendDelayedLoadMessage();
                }

                // If this directory result has some items, or there are no more directories that
                // we are waiting for, clear the temp results
                if (results.Count > 0 || mRemainingDirectoryCount == 0) {
                    // Clear the temp entries
                    clearTempEntries();
                }
            }

            // Show the list again without "waiting" message.
            updateEntries(constructEntryList(mEntryMap, mNonAggregatedEntries));
		}
	}

    private static Context mContext;
    private static ContentResolver mContentResolver;
    private  LayoutInflater mInflater;
    private static Account mAccount;
    private static int mPreferredMaxResultCount;
    private DropdownChipLayouter mDropdownChipLayouter;

    /**
     * {@link #mEntries} is responsible for showing every result for this Adapter. To
     * construct it, we use {@link #mEntryMap}, {@link #mNonAggregatedEntries}, and
     * {@link #mExistingDestinations}.
     *
     * First, each destination (an email address or a phone number) with a valid contactId is
     * inserted into {@link #mEntryMap} and grouped by the contactId. Destinations without valid
     * contactId (possible if they aren't in local storage) are stored in
     * {@link #mNonAggregatedEntries}.
     * Duplicates are removed using {@link #mExistingDestinations}.
     *
     * After having all results from Cursor objects, all destinations in mEntryMap are copied to
     * {@link #mEntries}. If the number of destinations is not enough (i.e. less than
     * {@link #mPreferredMaxResultCount}), destinations in mNonAggregatedEntries are also used.
     *
     * These variables are only used in UI thread, thus should not be touched in
     * performFiltering() methods.
     */
    private static LurchTable<Long, List<RecipientEntry>> mEntryMap;
    private static List<RecipientEntry> mNonAggregatedEntries;
    private static HashSet<String> mExistingDestinations;
    /** Note: use {@link #updateEntries(List)} to update this variable. */
    private List<RecipientEntry> mEntries;
    private List<RecipientEntry> mTempEntries;

    /** The number of directories this adapter is waiting for results. */
    private static int mRemainingDirectoryCount;

    /**
     * Used to ignore asynchronous queries with a different constraint, which may happen when
     * users type characters quickly.
     */
    private static ICharSequence mCurrentConstraint;

    private static LruCache<Uri, byte[]> mPhotoCacheMap;

    /**
     * Handler specific for maintaining "Waiting for more contacts" message, which will be shown
     * when:
     * - there are directories to be searched
     * - results from directories are slow to come
     */
    private  class DelayedMessageHandler : Handler {
        //@Override
        public void handleMessage(Message msg) {
            if (mRemainingDirectoryCount > 0) {
                updateEntries(constructEntryList(mEntryMap, mNonAggregatedEntries));
            }
        }

        public void sendDelayedLoadMessage() {
            sendMessageDelayed(obtainMessage(MESSAGE_SEARCH_PENDING, 0, 0, null),
                    MESSAGE_SEARCH_PENDING_DELAY);
        }

        public void removeDelayedLoadMessage() {
            removeMessages(MESSAGE_SEARCH_PENDING);
        }
    }

    private static DelayedMessageHandler mDelayedMessageHandler = new DelayedMessageHandler();

    private EntriesUpdatedObserver mEntriesUpdatedObserver;

    /**
     * Constructor for email queries.
     */
    public BaseRecipientAdapter(Context context):this(context, DEFAULT_PREFERRED_MAX_RESULT_COUNT, QUERY_TYPE_EMAIL) {
    }

    public BaseRecipientAdapter(Context context, int preferredMaxResultCount) :this(context, preferredMaxResultCount, QUERY_TYPE_EMAIL){
    }

    public BaseRecipientAdapter(int queryMode, Context context):this(context, DEFAULT_PREFERRED_MAX_RESULT_COUNT, queryMode) {
    }

    public BaseRecipientAdapter(int queryMode, Context context, int preferredMaxResultCount):this(context, preferredMaxResultCount, queryMode) {
    }

    public BaseRecipientAdapter(Context context, int preferredMaxResultCount, int queryMode) {
        mContext = context;
        mContentResolver = context.ContentResolver;
        mInflater = LayoutInflater.From(context);
        mPreferredMaxResultCount = preferredMaxResultCount;
        if (mPhotoCacheMap == null) {
            mPhotoCacheMap = new LruCache(PHOTO_CACHE_SIZE);
        }
        mQueryType = queryMode;

        if (queryMode == QUERY_TYPE_EMAIL) {
            mQuery = Queries.EMAIL;
        } else if (queryMode == QUERY_TYPE_PHONE) {
            mQuery = Queries.PHONE;
        } else {
            mQuery = Queries.EMAIL;
           // Log.e(TAG, "Unsupported query type: " + queryMode);
        }
    }

    public Context getContext() {
        return mContext;
    }

    public int getQueryType() {
        return mQueryType;
    }

    public void setDropdownChipLayouter(DropdownChipLayouter dropdownChipLayouter) {
        mDropdownChipLayouter = dropdownChipLayouter;
        mDropdownChipLayouter.setQuery(mQuery);
    }

    public DropdownChipLayouter getDropdownChipLayouter() {
        return mDropdownChipLayouter;
    }

    /**
     * Set the account when known. Causes the search to prioritize contacts from that account.
     */
    //@Override
    public void setAccount(Account account) {
        mAccount = account;
    }

    /** Will be called from {@link AutoCompleteTextView} to prepare auto-complete list. */
    //@Override
    public Filter getFilter() {
        return new DefaultFilter();
    }

    /**
     * An extesion to {@link RecipientAlternatesAdapter#getMatchingRecipients} that allows
     * additional sources of contacts to be considered as matching recipients.
     * @param addresses A set of addresses to be matched
     * @return A list of matches or null if none found
     */
    public Map<String, RecipientEntry> getMatchingRecipients(Set<String> addresses) {
        return null;
    }

    public static List<DirectorySearchParams> setupOtherDirectories(Context context,
            ICursor directoryCursor, Account account) {
         var packageManager = context.PackageManager;
         List<DirectorySearchParams> paramsList = new List<DirectorySearchParams>();
        DirectorySearchParams preferredDirectory = null;
        while (directoryCursor.MoveToNext()) {
             long id = directoryCursor.GetLong(DirectoryListQuery.ID);

            // Skip the local invisible directory, because the default directory already includes
            // all local results.
            if (id == Directory.LOCAL_INVISIBLE) {
                continue;
            }

             DirectorySearchParams paramsx = new DirectorySearchParams();
             String packageName = directoryCursor.GetString(DirectoryListQuery.PACKAGE_NAME);
             int resourceId = directoryCursor.GetInt(DirectoryListQuery.TYPE_RESOURCE_ID);
            paramsx.directoryId = id;
            paramsx.displayName = directoryCursor.GetString(DirectoryListQuery.DISPLAY_NAME);
            paramsx.accountName = directoryCursor.GetString(DirectoryListQuery.ACCOUNT_NAME);
            paramsx.accountType = directoryCursor.GetString(DirectoryListQuery.ACCOUNT_TYPE);
            if (packageName != null && resourceId != 0) {
                try {
                     Resources resources = packageManager.GetResourcesForApplication(packageName);
                    paramsx.directoryType = resources.GetString(resourceId);
                    if (paramsx.directoryType == null) {
						//Log.e(TAG, "Cannot resolve directory name: "
						//		+ resourceId + "@" + packageName);
                    }
                } catch (NameNotFoundException e) {
					//Log.e(TAG, "Cannot resolve directory name: "
					//		+ resourceId + "@" + packageName, e);
                }
            }

            // If an account has been provided and we found a directory that
            // corresponds to that account, place that directory second, directly
            // underneath the local contacts.
            if (account != null && account.Name.Equals(paramsx.accountName) &&
                    account.Type.Equals(paramsx.accountType)) {
                preferredDirectory = paramsx;
            } else {
                paramsList.Add(paramsx);
            }
        }

        if (preferredDirectory != null) {
            paramsList.Add(1, preferredDirectory);
        }

        return paramsList;
    }

    /**
     * Starts search in other directories using {@link Filter}. Results will be handled in
     * {@link DirectoryFilter}.
     */
    protected void startSearchOtherDirectories(CharSequence constraint, List<DirectorySearchParams> paramsList, int limit) {
         int count = paramsList.Count;
        // Note: skipping the default partition (index 0), which has already been loaded
        for (int i = 1; i < count; i++) {
             DirectorySearchParams paramsx = paramsList.get(i);
            paramsx.constraint = constraint;
            if (paramsx.filter == null) {
                paramsx.filter = new DirectoryFilter(paramsx);
            }
            paramsx.filter.setLimit(limit);
            paramsx.filter.filter(constraint);
        }

        // Directory search started. We may show "waiting" message if directory results are slow
        // enough.
        mRemainingDirectoryCount = count - 1;
        mDelayedMessageHandler.sendDelayedLoadMessage();
    }

    private static void putOneEntry(TemporaryEntry entry, bool isAggregatedEntry,
            LurchTable<Long, List<RecipientEntry>> entryMap,
            List<RecipientEntry> nonAggregatedEntries,
            HashSet<String> existingDestinations) {
        if (existingDestinations.Contains(entry.destination)) {
            return;
        }

        existingDestinations.Add(entry.destination);

        if (!isAggregatedEntry) {
            nonAggregatedEntries.Add(RecipientEntry.constructTopLevelEntry(
                    entry.displayName,
                    entry.displayNameSource,
                    entry.destination, entry.destinationType, entry.destinationLabel,
                    entry.contactId, entry.directoryId, entry.dataId, entry.thumbnailUriString,
                    true, entry.lookupKey));
        } else if (entryMap.containsKey(entry.contactId)) {
            // We already have a section for the person.
             List<RecipientEntry> entryList = entryMap.get(entry.contactId);
            entryList.Add(RecipientEntry.constructSecondLevelEntry(
                    entry.displayName,
                    entry.displayNameSource,
                    entry.destination, entry.destinationType, entry.destinationLabel,
                    entry.contactId, entry.directoryId, entry.dataId, entry.thumbnailUriString,
                    true, entry.lookupKey));
        } else {
             List<RecipientEntry> entryList = new List<RecipientEntry>();
            entryList.Add(RecipientEntry.constructTopLevelEntry(
                    entry.displayName,
                    entry.displayNameSource,
                    entry.destination, entry.destinationType, entry.destinationLabel,
                    entry.contactId, entry.directoryId, entry.dataId, entry.thumbnailUriString,
                    true, entry.lookupKey));
            entryMap.put(entry.contactId, entryList);
        }
    }

    /**
     * Constructs an actual list for this Adapter using {@link #mEntryMap}. Also tries to
     * fetch a cached photo for each contact entry (other than separators), or request another
     * thread to get one from directories.
     */
    private static List<RecipientEntry> constructEntryList(LurchTable<Long, List<RecipientEntry>> entryMap, List<RecipientEntry> nonAggregatedEntries) {
         var entries = new List<RecipientEntry>();
        int validEntryCount = 0;

	    foreach (var mapEntry in entryMap)
	    {
		     var entryList = mapEntry.Value;
		    var size = entryList.Count;

			for (int i = 0; i < size; i++)
			{
				var entry = entryList.ElementAt(i);
                entries.Add(entry);
                tryFetchPhoto(entry, mContentResolver, this, false, i);
                validEntryCount++;
            }
//            if (validEntryCount > mPreferredMaxResultCount) {
//                break;
//            }
	    }

        if (validEntryCount <= mPreferredMaxResultCount) {
            for (int i = 0; i < nonAggregatedEntries.Count; i++) {
                RecipientEntry entry = nonAggregatedEntries.ElementAt(i);
//                if (validEntryCount > mPreferredMaxResultCount) {
//                    break;
//                }
                entries.Add(entry);
                tryFetchPhoto(entry, mContentResolver, this, false, i);

                validEntryCount++;
            }
        }

        return entries;
    }


    public interface EntriesUpdatedObserver {
        public void onChanged(List<RecipientEntry> entries);
    }

    public void registerUpdateObserver(EntriesUpdatedObserver observer) {
        mEntriesUpdatedObserver = observer;
    }

    /** Resets {@link #mEntries} and notify the event to its parent ListView. */
    private static void updateEntries(List<RecipientEntry> newEntries) {
        mEntries = newEntries;
        mEntriesUpdatedObserver.onChanged(newEntries);
        notifyDataSetChanged();
    }

    private static void cacheCurrentEntries() {
        mTempEntries = mEntries;
    }

    private static void clearTempEntries() {
        mTempEntries = null;
    }

    protected List<RecipientEntry> getEntries() {
        return mTempEntries != null ? mTempEntries : mEntries;
    }

    public static void tryFetchPhoto( RecipientEntry entry, ContentResolver mContentResolver, BaseAdapter adapter, bool forceLoad, int position) {
        if (forceLoad || position <= 20) {
             Uri photoThumbnailUri = entry.getPhotoThumbnailUri();
            if (photoThumbnailUri != null) {
                 byte[] photoBytes = mPhotoCacheMap.get(photoThumbnailUri);
                if (photoBytes != null) {
                    entry.setPhotoBytes(photoBytes);
                    // notifyDataSetChanged() should be called by a caller.
                } else {
                    if (DEBUG) {
                        Log.d(TAG, "No photo cache for " + entry.getDisplayName()
                                + ". Fetch one asynchronously");
                    }
                    fetchPhotoAsync(entry, photoThumbnailUri, adapter, mContentResolver);
                }
            }
        }
    }

    // For reading photos for directory contacts, this is the chunksize for
    // copying from the inputstream to the output stream.
    private static  int BUFFER_SIZE = 1024*16;

    private static void fetchPhotoAsync( RecipientEntry entry,  Uri photoThumbnailUri,  BaseAdapter adapter,  ContentResolver mContentResolver) {
         AsyncTask<Void, Void, byte[]> photoLoadTask = new AsyncTask<Void, Void, byte[]>() {
            //@Override
            protected byte[] doInBackground(Void... params) {
                // First try running a query. Images for local contacts are
                // loaded by sending a query to the ContactsProvider.
                 Cursor photoCursor = mContentResolver.query(
                        photoThumbnailUri, PhotoQuery.PROJECTION, null, null, null);
                if (photoCursor != null) {
                    try {
                        if (photoCursor.moveToFirst()) {
                            return photoCursor.getBlob(PhotoQuery.PHOTO);
                        }
                    } ly {
                        photoCursor.close();
                    }
                } else {
                    // If the query fails, try streaming the URI directly.
                    // For remote directory images, this URI resolves to the
                    // directory provider and the images are loaded by sending
                    // an openFile call to the provider.
                    try {
                        InputStream is = mContentResolver.openInputStream(
                                photoThumbnailUri);
                        if (is != null) {
                            byte[] buffer = new byte[BUFFER_SIZE];
                            ByteArrayOutputStream baos = new ByteArrayOutputStream();
                            try {
                                int size;
                                while ((size = is.read(buffer)) != -1) {
                                    baos.write(buffer, 0, size);
                                }
                            } finally {
                                is.close();
                            }
                            return baos.toByteArray();
                        }
                    } catch (IOException ex) {
                        // ignore
                    }
                }
                return null;
            }

            //@Override
            protected void onPostExecute( byte[] photoBytes) {
                entry.setPhotoBytes(photoBytes);
                if (photoBytes != null) {
                    mPhotoCacheMap.put(photoThumbnailUri, photoBytes);
                    if (adapter != null)
                        adapter.notifyDataSetChanged();
                }
            }
        };
        photoLoadTask.executeOnExecutor(AsyncTask.SERIAL_EXECUTOR);
    }

    protected static void fetchPhoto( RecipientEntry entry,  Uri photoThumbnailUri,  ContentResolver mContentResolver) {
        byte[] photoBytes = mPhotoCacheMap.get(photoThumbnailUri);
        if (photoBytes != null) {
            entry.setPhotoBytes(photoBytes);
            return;
        }
        final Cursor photoCursor = mContentResolver.query(photoThumbnailUri, PhotoQuery.PROJECTION,
                null, null, null);
        if (photoCursor != null) {
            try {
                if (photoCursor.moveToFirst()) {
                    photoBytes = photoCursor.getBlob(PhotoQuery.PHOTO);
                    entry.setPhotoBytes(photoBytes);
                    mPhotoCacheMap.put(photoThumbnailUri, photoBytes);
                }
            } finally {
                photoCursor.close();
            }
        } else {
            InputStream inputStream = null;
            ByteArrayOutputStream outputStream = null;
            try {
                inputStream = mContentResolver.openInputStream(photoThumbnailUri);
                 Bitmap bitmap = BitmapFactory.decodeStream(inputStream);

                if (bitmap != null) {
                    outputStream = new ByteArrayOutputStream();
                    bitmap.compress(Bitmap.CompressFormat.PNG, 100, outputStream);
                    photoBytes = outputStream.toByteArray();

                    entry.setPhotoBytes(photoBytes);
                    mPhotoCacheMap.put(photoThumbnailUri, photoBytes);
                }
            } catch ( FileNotFoundException e) {
                Log.w(TAG, "Error opening InputStream for photo", e);
            } finally {
                try {
                    if (inputStream != null) {
                        inputStream.close();
                    }
                } catch (IOException e) {
                    Log.e(TAG, "Error closing photo input stream", e);
                }
                try {
                    if (outputStream != null) {
                        outputStream.close();
                    }
                } catch (IOException e) {
                    Log.e(TAG, "Error closing photo output stream", e);
                }
            }
        }
    }

		private static ICursor doQuery(string constraint, int limit, Long directoryId)
		{
			var builder = mQuery.getContentFilterUri().BuildUpon();
			builder.AppendPath(constraint);
			builder.AppendQueryParameter(ContactsContract.LimitParamKey,
			                             String.ValueOf(limit + ALLOWANCE_FOR_DUPLICATES));

			if (directoryId != null)
			{
				builder.AppendQueryParameter(ContactsContract.DirectoryParamKey,
				                             String.ValueOf(directoryId));
			}
			if (mAccount != null)
			{
				builder.AppendQueryParameter(PRIMARY_ACCOUNT_NAME, mAccount.Name);
				builder.AppendQueryParameter(PRIMARY_ACCOUNT_TYPE, mAccount.Type);
			}
			var where = (showMobileOnly && mQueryType == QUERY_TYPE_PHONE)
				? ContactsContract.CommonDataKinds.Phone.InterfaceConsts.Type + "=" + (int) PhoneDataKind.Mobile
				: null;
			//String where = (showMobileOnly && mQueryType == QUERY_TYPE_PHONE) ?
			//		ContactsContract.CommonDataKinds.Phone.InterfaceConsts.Type + "=" + ContactsContract.CommonDataKinds.Phone.TYPE_MOBILE : null;

			//var start = JavaSystem.CurrentTimeMillis();
			var cursor = mContentResolver.Query(
				limit == -1 ? mQuery.getContentUri() : builder.Build(), mQuery.getProjection(),
				@where, null,
				limit == -1 ? ContactsContract.Contacts.InterfaceConsts.DisplayName + " ASC" : null);
			//var end = JavaSystem.CurrentTimeMillis();
			//if (DEBUG) {
			//	Log.d(TAG, "Time for autocomplete (query: " + constraint
			//			+ ", directoryId: " + directoryId + ", num_of_results: "
			//			+ (cursor != null ? cursor.getCount() : "null") + "): "
			//			+ (end - start) + " ms");
			//}
			return cursor;
		}

    // TODO: This won't be used at all. We should find better way to quit the thread..
    /*public void close() {
        mEntries = null;
        mPhotoCacheMap.evictAll();
        if (!sPhotoHandlerThread.quit()) {
            Log.w(TAG, "Failed to quit photo handler thread, ignoring it.");
        }
    }*/

    //@Override
    public int getCount() {
         List<RecipientEntry> entries = getEntries();
        return entries != null ? entries.size() : 0;
    }

    //@Override
    public RecipientEntry getItem(int position) {
        return getEntries().get(position);
    }

    //@Override
    public long getItemId(int position) {
        return position;
    }

    //@Override
    public int getViewTypeCount() {
        return RecipientEntry.ENTRY_TYPE_SIZE;
    }

    //@Override
    public int getItemViewType(int position) {
        return getEntries().get(position).getEntryType();
    }

    //@Override
    public bool isEnabled(int position) {
        return getEntries().get(position).isSelectable();
    }

    //@Override
    public View getView(int position, View convertView, ViewGroup parent) {
         RecipientEntry entry = getEntries().get(position);

         String constraint = mCurrentConstraint == null ? null :
                mCurrentConstraint.toString();

        return mDropdownChipLayouter.bindView(convertView, parent, entry, position,
                AdapterType.BASE_RECIPIENT, constraint);
    }

    public Account getAccount() {
        return mAccount;
    }

    public bool isShowMobileOnly() {
        return showMobileOnly;
    }

    public void setShowMobileOnly(bool showMobileOnly) {
        this.showMobileOnly = showMobileOnly;
    }
	}
}