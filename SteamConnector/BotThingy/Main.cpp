#define STEAMWORKS_CLIENT_INTERFACES
#include <iostream>
#include <cstringt.h>
#include <atlstr.h>

#include <Steamworks.h>

#include <SFML/Network.hpp>

#define CLIENT_TYPE 1

typedef bool (*Steam_BGetCallbackFn)( HSteamPipe hSteamPipe, CallbackMsg_t *pCallbackMsg );
typedef void (*Steam_FreeLastCallbackFn)( HSteamPipe hSteamPipe );
static Steam_BGetCallbackFn BGetCallback = NULL;
static Steam_FreeLastCallbackFn FreeLastCallback = NULL;

static std::string ipAndSuch;
static int port;

// templated version of my_equal so it could work with both char and wchar_t
template<typename charT>
struct my_equal {
    my_equal( const std::locale& loc ) : loc_(loc) {}
    bool operator()(charT ch1, charT ch2) {
        return std::toupper(ch1, loc_) == std::toupper(ch2, loc_);
    }
private:
    const std::locale& loc_;
};

// find substring (case insensitive)
template<typename T>
int ci_find_substr( const T& str1, const T& str2, const std::locale& loc = std::locale() )
{
    T::const_iterator it = std::search( str1.begin(), str1.end(), 
        str2.begin(), str2.end(), my_equal<T::value_type>(loc) );
    if ( it != str1.end() ) return it - str1.begin();
    else return -1; // not found
}


std::string AskBot(CSteamID id, const char* toAsk,  const char* _username,const char* _chattype)
{
	try{
	sf::TcpSocket socket;

	socket.Connect(sf::IpAddress(ipAndSuch), port);

	CStringW test = CStringW("interactsteam\n");
	test.Append( CA2W(id.Render()) );
	test.Append( CA2W("\n") );
	test.Append( CA2W(_username));
	test.Append( CA2W("\n") );
	test.Append( CA2W(_chattype) );
	test.Append( CA2W("\n") );
	test.Append( CA2W( toAsk )  );
	char* test2 = new char[test.GetLength()*2];

	_swab((char*)test.GetString(), test2, test.GetLength()*2);

	sf::Socket::Status sockSt;

	sockSt = socket.Send(test2, test.GetLength()*2);

	char* chars = new char[2048];
	size_t size;

	sockSt = socket.Receive(chars, (size_t)2048, size);

	wchar_t* char2 = new wchar_t[size];
	_swab(chars, (char*)char2, size);
	CStringW asdf(char2);
	std::string Dcolon = std::string(CW2A(asdf));
	Dcolon.resize(size/2);
	delete chars;
	delete char2;
	delete test2;

	socket.Disconnect();
	return Dcolon;
	}
	catch(...)
	{
	return std::string("Error while asking bot");
	}
}

void TellBot(CSteamID id, const char* ToTell, const char* _username,const char* _chattype)
{
	try{
	sf::TcpSocket socket;

	socket.Connect(sf::IpAddress(ipAndSuch), port);

	CStringW test = CStringW("learn\n");
	test.Append( CA2W(id.Render()) );
	test.Append( CA2W("\n") );
	test.Append( CA2W(_username));
	test.Append( CA2W("\n") );
	test.Append( CA2W(_chattype) );
	test.Append( CA2W("\n") );
	test.Append( CA2W( ToTell ) );
	
	char* test2 = new char[test.GetLength()*2];

	_swab((char*)test.GetString(), test2, test.GetLength()*2);

	sf::Socket::Status sockSt;

	sockSt = socket.Send(test2, test.GetLength()*2);

	char* chars = new char[2048];
	size_t size;


	delete chars;
	delete test2;

	socket.Disconnect();
	return;
	}
	catch(...)
	{
	printf("Error while telling bot");
	}
}


int main(int argc, char** argv)
{
	ipAndSuch = "localhost";
	port = 5000;

	if (argc > 1)
	{
		ipAndSuch = argv[1];

		if (argc > 2)
		{
			char* end;
			port = strtol(argv[2], &end, 10);
		}
	}

	CSteamAPILoader* loader = new CSteamAPILoader();
	CreateInterfaceFn factory = loader->Load();

	BGetCallback = (Steam_BGetCallbackFn)loader->GetSteamClientModule()->GetSymbol("Steam_BGetCallback");
	FreeLastCallback = (Steam_FreeLastCallbackFn)loader->GetSteamClientModule()->GetSymbol("Steam_FreeLastCallback");

#if CLIENT_TYPE == 0
	ISteamClient012 *client = (ISteamClient012 *)factory( STEAMCLIENT_INTERFACE_VERSION_012 , NULL );
#else
	IClientEngine *client = (IClientEngine *)factory( CLIENTENGINE_INTERFACE_VERSION, NULL );
#endif
	HSteamPipe pipe = client->CreateSteamPipe();
	HSteamUser user = client->ConnectToGlobalUser(pipe);

#if CLIENT_TYPE == 0
	ISteamFriends010 *clientfriends = (ISteamFriends010 *)client->GetISteamFriends( user, pipe, STEAMFRIENDS_INTERFACE_VERSION_010 );
	ISteamUser016 *clientuser = (ISteamUser016 *)client->GetISteamUser( user, pipe, STEAMUSER_INTERFACE_VERSION_016 );
#else
	IClientFriends *clientfriends = (IClientFriends *)client->GetIClientFriends( user, pipe, CLIENTFRIENDS_INTERFACE_VERSION );
	IClientUser *clientuser = (IClientUser *)client->GetIClientUser( user, pipe, CLIENTUSER_INTERFACE_VERSION );
#endif

	printf("Now connected to steam and running!\n");

	char* dest;
	CSteamID userID;
	EChatEntryType entry;
	EChatEntryType toSend;
	const char* userName;

	ChatRoomMsg_t *pRInfo;
	FriendChatMsg_t *pFInfo;

	std::string Dcolon;

	while(true)
	{
		CallbackMsg_t callBack;
		if (BGetCallback( pipe, &callBack ) )
		{
			dest = new char[4096];
			memset(dest, 0, 4096);

			switch (callBack.m_iCallback)
			{
			
			case ChatRoomMsg_t::k_iCallback:
			case 810: // Chat room callback
				pRInfo = (ChatRoomMsg_t *)callBack.m_pubParam;

				if (pRInfo->m_ulSteamIDUser == clientuser->GetSteamID())
					break;
				
				userID = CSteamID();
				userName = clientfriends->GetFriendPersonaName(pRInfo->m_ulSteamIDUser);
				entry = k_EChatEntryTypeInvalid;

#if CLIENT_TYPE == 0
				clientfriends->GetClanChatMessage(pRInfo->m_ulSteamIDChat, pRInfo->m_iChatID, (void*)dest, 4096, &entry, &userID);
#else
				clientfriends->GetChatRoomEntry(pRInfo->m_ulSteamIDChat, pRInfo->m_iChatID, &userID, dest, 4096, &entry);
#endif

				if (entry != k_EChatEntryTypeChatMsg && entry != k_EChatEntryTypeEmote)
						break;

				printf("[Group] %s said %s\n", userName, dest);
				
					if (ci_find_substr(std::string(dest), std::string("onymity")) == -1 && ci_find_substr(std::string(dest), std::string("ony")) == -1  && entry != k_EChatEntryTypeEmote)
					{
						TellBot(pRInfo->m_ulSteamIDUser, dest, userName,"GROUPCHAT");
						break;
					}
					else if(ci_find_substr(std::string(dest), std::string("onymity")) == -1 && ci_find_substr(std::string(dest), std::string("ony")) == -1  && entry == k_EChatEntryTypeEmote)
					{break;}
				

				Dcolon = AskBot(pRInfo->m_ulSteamIDUser, dest, userName,"GROUPCHAT");

				if(Dcolon.length() == 0)
					break;

				toSend = k_EChatEntryTypeChatMsg;
				if (*Dcolon.begin() == '*' && *(Dcolon.end()-1) == '*')
				{
					toSend = k_EChatEntryTypeEmote;
					Dcolon.erase(Dcolon.begin());
					Dcolon.erase(Dcolon.end()-1);
				}

#if CLIENT_TYPE == 0
				clientfriends->SendClanChatMessage(pRInfo->m_ulSteamIDChat, Dcolon.c_str());
#else
				clientfriends->SendChatMsg(pRInfo->m_ulSteamIDChat, toSend, Dcolon.c_str(), Dcolon.length()+1);
#endif
				break;
				case 805:
			case FriendChatMsg_t::k_iCallback:
				pFInfo = (FriendChatMsg_t *)callBack.m_pubParam;

				if (pFInfo->m_ulSender == clientuser->GetSteamID())
					break;

				//userID = CSteamID();
				entry = k_EChatEntryTypeInvalid;

				clientfriends->GetFriendMessage(pFInfo->m_ulReceiver, pFInfo->m_iChatID, dest, 4096, &entry);

				if (entry != k_EChatEntryTypeChatMsg)
					break;

				printf("[Friend] %s said %s\n", clientfriends->GetFriendPersonaName(pFInfo->m_ulSender), dest);

				Dcolon = AskBot(pFInfo->m_ulSender, dest,clientfriends->GetFriendPersonaName(pFInfo->m_ulSender),"PM");
#if CLIENT_TYPE == 0
				clientfriends->ReplyToFriendMessage(pFInfo->m_ulSender, Dcolon.c_str());
#else
				clientfriends->SendMsgToFriend(pFInfo->m_ulSender, k_EChatEntryTypeChatMsg, Dcolon.c_str(), Dcolon.length()+1);
#endif
				break;

			default:
				int callbackNum = callBack.m_iCallback;

				char* callbackName;

				if (callbackNum > 2100)
					callbackName = "Client HTTP";
				else if (callbackNum > 2000)
					callbackName = "Game stats";
				else if (callbackNum > 1900)
					callbackName = "Steam 2 async";
				else if (callbackNum > 1800)
					callbackName = "Game server stats";
				else if (callbackNum > 1700)
					callbackName = "Game coordinator";
				else if (callbackNum > 1600)
					callbackName = "Client utils";
				else if (callbackNum > 1500)
					callbackName = "Game server items";
				else if (callbackNum > 1400)
					callbackName = "User items";
				else if (callbackNum > 1300)
					callbackName = "Remote storage";
				else if (callbackNum > 1200)
					callbackName = "Networking";
				else if (callbackNum > 1100)
					callbackName = "User stats";
				else if (callbackNum > 1000)
					callbackName = "Apps";
				else if (callbackNum > 900)
					callbackName = "Client user";
				else if (callbackNum > 800)
					callbackName = "Client friends";
				else if (callbackNum > 700)
					callbackName = "Utils";
				else if (callbackNum > 600)
					callbackName = "Content server";
				else if (callbackNum > 500)
					callbackName = "Matchmaking";
				else if (callbackNum > 400)
					callbackName = "Billing";
				else if (callbackNum > 300)
					callbackName = "Friends";
				else if (callbackNum > 200)
					callbackName = "Game server";
				else
					callbackName = "User";

				printf("Unused %s (%i) callback!\n", callbackName, callbackNum);

				break;
			}

			delete dest;

			FreeLastCallback(pipe);
		}

		sf::Sleep(10);
	}

	return 0;
}

