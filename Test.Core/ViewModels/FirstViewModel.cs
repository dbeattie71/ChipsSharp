using System.Collections.Generic;
using System.Linq;
using ChipsSharp;
using Cirrious.CrossCore.Platform;
using Cirrious.MvvmCross.ViewModels;
using EnjinMobile.Api;
using EnjinMobile.Api.Models.Messages;
using EnjinMobile.Api.Services;

namespace Test.Core.ViewModels
{
	public class ContactEx : Contact, IChipEntry
	{
		public string getDisplayName()
		{
			return Name;
		}

		public string getDestination()
		{
			return Name2;
		}

		public string ImageUrl { get { return Image; } }
	}

	public class FirstViewModel : MvxViewModel
	{
		private readonly IMessagesService _messagesService;
		private string _currentTextHint;
		private List<IChipEntry> _selectedContacts;
		//private string _articles;
		private List<ContactEx> _suggestedContacts;
		//private List<Contact> _recipientEntries;
		//private List<Contact> _selectedContacts;
		//private object _selectedObject;

		public FirstViewModel(IEnjinApiFacade enjinApiFacade)
		{
			_messagesService = enjinApiFacade.CreateMessagesService();

			Commands = new MvxCommandCollectionBuilder().BuildCollectionFor(this);

			SelectedContacts = new List<IChipEntry>();

		}

		public IMvxCommandCollection Commands { get; private set; }

		//public string Articles
		//{
		//	get { return _articles; }
		//	set
		//	{
		//		_articles = value;
		//		RaisePropertyChanged();
		//	}
		//}

		public List<ContactEx> SuggestedContacts
		{
			get { return _suggestedContacts; }
			set
			{
				_suggestedContacts = value;
				RaisePropertyChanged();
			}
		}

		public List<IChipEntry> SelectedContacts
		{
			get { return _selectedContacts; }
			set
			{
				_selectedContacts = value;
				RaisePropertyChanged();
			}
		}

		//public List<Contact> RecipientEntries
		//{
		//	get { return _recipientEntries; }
		//	set
		//	{
		//		_recipientEntries = value;
		//		RaisePropertyChanged();
		//	}
		//}

		//public object SelectedObject
		//{
		//	get { return _selectedObject; }
		//	private set
		//	{
		//		_selectedObject = value;
		//		RaisePropertyChanged();
		//	}
		//}

		//public List<Contact> SelectedContacts
		//{
		//	get { return _selectedContacts; }
		//	set
		//	{
		//		_selectedContacts = value;
		//		RaisePropertyChanged();
		//	}
		//}

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

		public override void Start()
		{
		}

		public void TestCommand()
		{
			SelectedContacts.Add(new ContactEx { Name = "test" });
			RaisePropertyChanged(() => SelectedContacts);
			
		}

		private async void Foo(string currentTextHint)
		{
			var contacts = await _messagesService.SearchContacts(currentTextHint);
			if (contacts.Any())
			{
				SuggestedContacts = contacts.Select(c => new ContactEx
				{
					Id = c.Id,
					Image = c.Image,
					Name = c.Name,
					Name2 = c.Name2,
					Type = c.Type
				}).ToList();
			}
			else
			{
				SetSuggestionsEmpty();
			}
		}

		private void SetSuggestionsEmpty()
		{
			var contacts = new List<ContactEx>();
			SuggestedContacts = contacts;
		}

		public void Start2()
		{
			//SelectedContacts.Add(new Contact { Name = "test" });
			//RaisePropertyChanged(() => SelectedContacts);
		}
	}
}