using System.Collections.Generic;
using System.Linq;
using Cirrious.CrossCore.Platform;
using Cirrious.MvvmCross.ViewModels;
using EnjinMobile.Api;
using EnjinMobile.Api.Models.Messages;
using EnjinMobile.Api.Services;

namespace Test.Core.ViewModels
{
	
	public class FirstViewModel : MvxViewModel
	{
		private readonly IMessagesService _messagesService;
		private string _articles;
		private List<Contact> _contacts;
		private string _currentTextHint;
		private List<Contact> _recipientEntries;
		private List<Contact> _selectedContacts;
		private object _selectedObject;

		public FirstViewModel(IEnjinApiFacade enjinApiFacade)
		{
			_messagesService = enjinApiFacade.CreateMessagesService();

			Commands = new MvxCommandCollectionBuilder().BuildCollectionFor(this);

			SelectedContacts = new List<Contact>();

		}

		public override void Start()
		{

		}

		public IMvxCommandCollection Commands { get; private set; }

		public string Articles
		{
			get { return _articles; }
			set
			{
				_articles = value;
				RaisePropertyChanged();
			}
		}

		public List<Contact> Contacts
		{
			get { return _contacts; }
			set
			{
				_contacts = value;
				RaisePropertyChanged();
			}
		}

		public List<Contact> RecipientEntries
		{
			get { return _recipientEntries; }
			set
			{
				_recipientEntries = value;
				RaisePropertyChanged();
			}
		}

		public object SelectedObject
		{
			get { return _selectedObject; }
			private set
			{
				_selectedObject = value;
				RaisePropertyChanged();
			}
		}

		public List<Contact> SelectedContacts
		{
			get { return _selectedContacts; }
			set
			{
				_selectedContacts = value;
				RaisePropertyChanged();
			}
		}

		public string CurrentTextHint
		{
			get { return _currentTextHint; }
			set
			{
				MvxTrace.Trace("Partial Text Value Sent {0}", value);
				//Setting _currentTextHint to null if an empty string gets passed here
				//is extremely important.
				if (value == "")
				{
					_currentTextHint = null;
					SetSuggestionsEmpty();
					return;
				}
				_currentTextHint = value;

				if (_currentTextHint.Trim().Length < 3)
				{
					SetSuggestionsEmpty();
					return;
				}

				Foo(_currentTextHint);
			}
		}

		public void TestCommand()
		{
			SelectedContacts.Add(new Contact { Name = "test" });
			RaisePropertyChanged(() => SelectedContacts);
		}

		private async void Foo(string currentTextHint)
		{
			var contacts = await _messagesService.SearchContacts(currentTextHint);
			if (contacts.Any())
			{
				Contacts = contacts;
			}
			else
			{
				SetSuggestionsEmpty();
			}
		}

		private void SetSuggestionsEmpty()
		{
			var contacts = new List<Contact>();
			Contacts = contacts;
		}

		public void Start2()
		{
			SelectedContacts.Add(new Contact { Name = "test" });
			RaisePropertyChanged(() => SelectedContacts);
		}
	}
}