using EnjinMobile.Api.Models.User;
using EnjinMobile.Api.Services;

namespace Test.Core.Helpers
{
	public class TestSessionPersistence : ISessionPersistence
	{
		public UserSession LoadSession()
		{
			var userSession = new UserSession();
			userSession.SessionId = "f43kjv5tdqcja1es7hivp6cqe3";
			return userSession;
		}

		public void SaveSession(UserSession userSession)
		{
		}

		public void ClearSession()
		{
		}
	}
}