#define STEAMWORKS_CLIENT_INTERFACES

#include <iostream>
#include <cstringt.h>
#include <atlstr.h>

#include <Steamworks.h>
#include <SFML/Network.hpp>

#undef FAILED
#define FAILED(x) (x != sf::Socket::Done)

typedef bool (*Steam_BGetCallbackFn)( HSteamPipe hSteamPipe, CallbackMsg_t *pCallbackMsg );
typedef void (*Steam_FreeLastCallbackFn)( HSteamPipe hSteamPipe );
static Steam_BGetCallbackFn BGetCallback = NULL;
static Steam_FreeLastCallbackFn FreeLastCallback = NULL;

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

using namespace std;

enum ChatType
{
    Chat_Private,
    Chat_Group
};

enum MessageType
{
    Message_Interact,
    Message_Tell
};

static sf::IpAddress ipAndSuch = sf::IpAddress::LocalHost;
static int port = 5000;

string RunBotCommand(const CSteamID& id, MessageType mType, const string& message, const string& username, ChatType cType);
string GetCallbackType(unsigned int callbackNum);

int main(int argc, char** argv)
{
	if (argc > 1)
	{
		ipAndSuch = argv[1];

		if (argc > 2)
		{
			port = atoi(argv[2]);
		}
	}

	CSteamAPILoader* loader   = new CSteamAPILoader();
	CreateInterfaceFn factory = loader->Load();

	BGetCallback          = static_cast<Steam_BGetCallbackFn>    (loader->GetSteamClientModule()->GetSymbol("Steam_BGetCallback"));
	FreeLastCallback      = static_cast<Steam_FreeLastCallbackFn>(loader->GetSteamClientModule()->GetSymbol("Steam_FreeLastCallback"));
	IClientEngine *client = static_cast<IClientEngine *>         (factory(CLIENTENGINE_INTERFACE_VERSION, NULL));

	HSteamPipe pipe = client->CreateSteamPipe();
	HSteamUser user = client->ConnectToGlobalUser(pipe);

	IClientFriends *clientfriends = static_cast<IClientFriends*>(client->GetIClientFriends(user, pipe, CLIENTFRIENDS_INTERFACE_VERSION));
	IClientUser *clientuser       = static_cast<IClientUser*>   (client->GetIClientUser(user, pipe, CLIENTUSER_INTERFACE_VERSION));

	cout << "Now connected to steam and awaiting callbacks!" << endl;

	while(true)
	{
		CallbackMsg_t callBack;
		if (BGetCallback( pipe, &callBack ) )
		{
			switch (callBack.m_iCallback)
			{
			case ChatRoomMsg_t::k_iCallback:
			case 810: // Chat room callback
                {
				    ChatRoomMsg_t Info = *(ChatRoomMsg_t *)callBack.m_pubParam;

				    if (Info.m_ulSteamIDUser == clientuser->GetSteamID())
					    break;
				
				    CSteamID userID;
				    string userName = clientfriends->GetFriendPersonaName(Info.m_ulSteamIDUser);
				    EChatEntryType entry = k_EChatEntryTypeInvalid;

                    char* temp = new char[4096];
				    clientfriends->GetChatRoomEntry(Info.m_ulSteamIDChat, Info.m_iChatID, &userID, temp, 4096, &entry);
                    std::string message = temp;
                    delete[] temp;

				    if (entry != k_EChatEntryTypeChatMsg && entry != k_EChatEntryTypeEmote)
						    break;

				    cout << "[Group] " << userName << " said something!" << endl;
				
                    bool foundOny = (ci_find_substr<string>(message, "ony") != -1);

					if (!foundOny && entry == k_EChatEntryTypeChatMsg)
					{
						RunBotCommand(Info.m_ulSteamIDUser, Message_Tell, message, userName, Chat_Group);
					}
                    
                    if (!foundOny)
                        break;
				
				    string response = RunBotCommand(Info.m_ulSteamIDUser, Message_Interact, message, userName, Chat_Group);

				    if(response.length() == 0)
                    {
                        cout << " !!Bot didn't respond!!" << endl;
					    break;
                    }
                    else if (response.front() == '!')
                    {
                        cout << " !" << response << "!" << endl;
                        break;
                    }
                    
				    EChatEntryType toSend = k_EChatEntryTypeChatMsg;
				    if (response.front() == '*' && response.back() == '*')
				    {
					    toSend = k_EChatEntryTypeEmote;
					    response.erase(response.begin());
					    response.erase(response.end()-1);
				    }

				    clientfriends->SendChatMsg(Info.m_ulSteamIDChat, toSend, response.c_str(), response.length()+1);

				    break;
                }

		    case 805:
			case FriendChatMsg_t::k_iCallback:
                {
				    FriendChatMsg_t Info = *(FriendChatMsg_t *)callBack.m_pubParam;

				    if (Info.m_ulSender == clientuser->GetSteamID())
					    break;

				    EChatEntryType entry = k_EChatEntryTypeInvalid;

                    char* temp = new char[4096];
				    clientfriends->GetFriendMessage(Info.m_ulReceiver, Info.m_iChatID, temp, 4096, &entry);
                    std::string message = temp;
                    delete[] temp;

				    if (entry != k_EChatEntryTypeChatMsg)
					    break;

                    string userName = clientfriends->GetFriendPersonaName(Info.m_ulSender);

				    cout << "[Friend] " << userName << " said something!" << endl;

				    string response = RunBotCommand(Info.m_ulSender, Message_Interact, message, userName, Chat_Private);

                    if(response.length() == 0)
                    {
                        cout << " !!Bot didn't respond!!" << endl;
					    break;
                    }
                    else if (response.front() == '!')
                    {
                        cout << " !" << response << "!" << endl;
                        break;
                    }

				    clientfriends->SendMsgToFriend(Info.m_ulSender, k_EChatEntryTypeChatMsg, response.c_str(), response.length()+1);
				    break;
                }

			default:
                {
#ifdef _DEBUG
                    cout << "Uncaught callback " << callBack.m_iCallback << " (" << GetCallbackType(callBack.m_iCallback) << ")" << endl;
#endif
				    break;
                }
			}

			FreeLastCallback(pipe);
		}

		sf::Sleep(10);
	}

	return 0;
}

std::string RunBotCommand(const CSteamID& id, MessageType mType, const string& message, const string& username, ChatType cType)
{
	try
    {
	    sf::TcpSocket socket;

        sf::Socket::Status status = socket.Connect(ipAndSuch, port);
        if (FAILED(status))
            return "!Failed to connect to bot!";

        char* utf8string = new char[2048];
        sprintf(utf8string, "%s\n%s\n%s\n%s\n%s\n", (mType == Message_Interact ? "interactsteam" : "tell"), id.Render(), username.c_str(), (cType == Chat_Private ? "PM" : "GROUPCHAT"), message.c_str());

        char* utf16string = 0;
        int utf16stringLen = 0;
        {
	        CStringW temp = CStringW(CA2W(utf8string));
            utf16stringLen = temp.GetLength()*2;
	        utf16string = new char[utf16stringLen];

	        _swab((char*)temp.GetString(), utf16string, utf16stringLen);
        }

        delete[] utf8string;

	    status = socket.Send(utf16string, utf16stringLen);
        delete[] utf16string;

        if (FAILED(status))
        {
            return "!Failed to send message to bot!";
        }

	    utf16string = new char[2048];
	    size_t size;

	    status = socket.Receive(utf16string, (size_t)2048, size);
        if (FAILED(status))
        {
            delete[] utf16string;

            return "!Failed to receive response from bost!";
        }

        std::string retString;
        {
	        wchar_t* temp = new wchar_t[size];
	        _swab(utf16string, (char*)temp, size);

	        CStringW asdf(temp);
	        retString = std::string(CW2A(asdf));
            delete[] temp;

	        retString.resize(size/2);
        }

	    socket.Disconnect();
	    return retString;
	}
	catch (const std::exception& e)
	{
        std::string error = "!Something went wrong: ";
        error.append(e.what());
        error.append("!");
	    return error;
	}
}

string GetCallbackType(unsigned int callbackNum)
{
    if (callbackNum > 2100)
	    return "Client HTTP";
    else if (callbackNum > 2000)
	    return "Game stats";
    else if (callbackNum > 1900)
	    return "Steam 2 async";
    else if (callbackNum > 1800)
	    return "Game server stats";
    else if (callbackNum > 1700)
	    return "Game coordinator";
    else if (callbackNum > 1600)
	    return "Client utils";
    else if (callbackNum > 1500)
	    return "Game server items";
    else if (callbackNum > 1400)
	    return "User items";
    else if (callbackNum > 1300)
	    return "Remote storage";
    else if (callbackNum > 1200)
	    return "Networking";
    else if (callbackNum > 1100)
	    return "User stats";
    else if (callbackNum > 1000)
	    return "Apps";
    else if (callbackNum > 900)
	    return "Client user";
    else if (callbackNum > 800)
	    return "Client friends";
    else if (callbackNum > 700)
	    return "Utils";
    else if (callbackNum > 600)
	    return "Content server";
    else if (callbackNum > 500)
	    return "Matchmaking";
    else if (callbackNum > 400)
	    return "Billing";
    else if (callbackNum > 300)
	    return "Friends";
    else if (callbackNum > 200)
	    return "Game server";
    else
	    return "User";
}