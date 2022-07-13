#include "Reader.h"
#include <strsafe.h>
#include "sha1.h"
#include <codecvt>
#include "log.h"


#include <locale>
#include <fstream>
#include <cstdlib>
//#pragma comment ( lib, "cryptlib" )
//#include "..\External\include\sha.h"
//#include "..\External\include\aes.h"

#pragma warning(disable : 4996)

reader::reader(void)
{
	MAZ_LOG(LogMessageType::Information, "Reader::Constructor");

	h_inst_ = NULL;
	p_provider_ = NULL;
}

reader::~reader(void)
{
	MAZ_LOG(LogMessageType::Information, "Reader::Destructor");
	stop();

	// make sure to release any reference we have to the provider.
	if (p_provider_ != NULL)
	{
		p_provider_->Release();
		p_provider_ = NULL;
	}
}

void reader::stop()
{
	MAZ_LOG(LogMessageType::Information, "Reader::Stop");

	// end thread
	check_loop_ = false;
	closesocket(soc_);
	MAZ_LOG(LogMessageType::Information, "Reader::Stop Socket closed");
	WSACleanup();
	if (reader_thread_.joinable())
		reader_thread_.join();
	MAZ_LOG(LogMessageType::Information, "Reader::Stop thread joined");
}

void reader::start()
{
	MAZ_LOG(LogMessageType::Information, "Reader::Start");

	if (check_loop_ && reader_thread_.joinable())
		return; // already running

	// start listening thread for reader events
	check_loop_ = true;

	int result;
	result = WSAStartup(MAKEWORD(2, 2), &wsa_data_);
	if (result != 0) {
		MAZ_LOG(LogMessageType::Information, "Reader::Start WSAStartup failed");
		printf("WSAStartup failed with error: %d\n", result);
		//return 1;
		return; //failed
	}
	soc_ = socket(AF_INET, SOCK_STREAM, IPPROTO_TCP);
	if (soc_ == INVALID_SOCKET)
	{
		MAZ_LOG(LogMessageType::Information, "Reader::Start Invalid socket");

		//std::cout << "socket is bullshit and didnt start" << std::endl;
		WSACleanup();
		//return 1;
		return;
	}

	service_found_ = true;
	MAZ_LOG(LogMessageType::Information, "Reader::Start Service found");

	// this is where we'd start the thread to check for a valid ring
	reader_thread_ = std::thread(&reader::check_nfc, this);
}

// Performs the work required to spin off our message so we can listen for events.
HRESULT reader::initialize(nfc_credential_provider *pProvider)
{
	MAZ_LOG(LogMessageType::Information, "Reader::Initialize");

	HRESULT hr = S_OK;

	// Be sure to add a release any existing provider we might have, then add a reference
	// to the provider we're working with now.
	if (p_provider_ != NULL)
	{
		p_provider_->Release();
	}
	p_provider_ = pProvider;
	p_provider_->AddRef();

	start();

	return hr;
}

void reader::check_nfc()
{
	MAZ_LOG(LogMessageType::Information, "Reader::CheckNFC");
	
	struct sockaddr_in destination{};
	destination.sin_family = AF_INET;
	destination.sin_port = htons(28416);
	destination.sin_addr.s_addr = inet_addr("127.0.0.1");
	while (check_loop_)
	{
		MAZ_LOG(LogMessageType::Information, std::to_string((int)soc_));

		int result = connect(soc_, (struct sockaddr *)&destination, sizeof(destination));
		MAZ_LOG(LogMessageType::Information, std::string("Reader::CheckNFC result ").append(std::to_string(result)));
		for (int i = 0; i < 5; i++)
		{
			if (result != 0 && check_loop_)
			{
				result = connect(soc_, (struct sockaddr *)&destination, sizeof(destination));
				MAZ_LOG(LogMessageType::Information, std::string("Reader::CheckNFC result ").append(std::to_string(result)));
			}
			else
				break;
			std::this_thread::sleep_for(std::chrono::seconds(1));
			MAZ_LOG(LogMessageType::Information, "Reader::CheckNFC connect failed");
		}
		char buffer[1000];
		int newData = 0;
		if (soc_ != INVALID_SOCKET)
		{

			try
			{
				newData = recv(soc_, buffer, 1000, 0); 
				if (newData > 0)
				{
					username_ = std::string(buffer, newData);
					if (username_ == " ")
					{
						username_ = "";
						continue;
					}
					newData = recv(soc_, buffer, 1000, 0);
					if (newData > 0)
					{
						password_ = std::string(buffer, newData);
						newData = recv(soc_, buffer, 1000, 0);
						domain_ = std::string(buffer, newData);
						kerbros_credential_retrieved_ = true;
						// did we get both parts of the credential?
						// fire "CredentialsChanged" event

						if (p_provider_ != NULL)
							p_provider_->OnNFCStatusChanged();
					}
					else
					{
						MAZ_LOG(LogMessageType::Information, "Reader::CheckNFC no data for password");
					}
				}
				else
				{
					MAZ_LOG(LogMessageType::Information, "Reader::CheckNFC no data for username");
				}
			}
			catch (...)
			{
				//FILE* f = fopen("C:\\credex.txt", "w");
				//fprintf(f, "%i", e);
				//fclose(f);
			}
		}
	}
}

bool reader::has_login()
{
	MAZ_LOG(LogMessageType::Information, "Reader::HasLogin");

	return kerbros_credential_retrieved_;
}

void reader::clear_login()
{
	MAZ_LOG(LogMessageType::Information, "Reader::ClearLogin");

	username_ = "";
	password_ = "";
	domain_ = "";
	kerbros_credential_retrieved_ = false;
}

std::wstring StringToWString(const std::string& s)
{
	MAZ_LOG(LogMessageType::Information, "Reader::StringToWString");

	std::wstring temp(s.length(), L' ');
	std::copy(s.begin(), s.end(), temp.begin());
	return temp;
}

HRESULT reader::get_login(
	CREDENTIAL_PROVIDER_GET_SERIALIZATION_RESPONSE* pcpgsr,
	CREDENTIAL_PROVIDER_CREDENTIAL_SERIALIZATION* pcpcs,
	PWSTR* ppwszOptionalStatusText,
	CREDENTIAL_PROVIDER_STATUS_ICON* pcpsiOptionalStatusIcon,
	CREDENTIAL_PROVIDER_USAGE_SCENARIO cpus
	)
{
	MAZ_LOG(LogMessageType::Information, "Reader::GetLogin");

	if (!kerbros_credential_retrieved_)
		return E_FAIL;

	if (username_ == "" || password_ == "") // might help?
		return E_FAIL;

	std::wstring un = StringToWString(username_);
	std::wstring pw = StringToWString(password_);
	std::wstring dm = StringToWString(domain_);

	username_ = "";
	password_ = "";
	domain_ = "";

	kerbros_credential_retrieved_ = false;

	HRESULT hr = S_OK;
	WCHAR wsz[MAX_COMPUTERNAME_LENGTH + 1];
	DWORD cch = ARRAYSIZE(wsz);
	if (dm.length() == 0)
	{
		if (!GetComputerNameW(wsz, &cch))
		{
			DWORD dwErr = GetLastError();
			hr = HRESULT_FROM_WIN32(dwErr);
			return hr;
		}
	}
	else
	{
		std::wcscpy(&wsz[0], dm.c_str());
	}
	PWSTR pwzProtectedPassword;
	// cpus
	hr = ProtectIfNecessaryAndCopyPassword(&pw[0], CPUS_LOGON, &pwzProtectedPassword);

	//FILE* f = fopen("C:\\cred4.txt", "w+");
	//fwprintf(f, L"%s\n", pwzProtectedPassword);
	//fclose(f);

	if (SUCCEEDED(hr))
	{
		KERB_INTERACTIVE_UNLOCK_LOGON kiul;

		// Initialize kiul with weak references to our credential.
		hr = KerbInteractiveUnlockLogonInit(wsz, &un[0], pwzProtectedPassword, cpus, &kiul);
		//hr = KerbInteractiveUnlockLogonInit(wsz, &un[0], &(pass)[0], cpus, &kiul);

		if (SUCCEEDED(hr))
		{
			// We use KERB_INTERACTIVE_UNLOCK_LOGON in both unlock and logon scenarios.  It contains a
			// KERB_INTERACTIVE_LOGON to hold the creds plus a LUID that is filled in for us by Winlogon
			// as necessary.
			hr = KerbInteractiveUnlockLogonPack(kiul, &pcpcs->rgbSerialization, &pcpcs->cbSerialization);

			if (SUCCEEDED(hr))
			{
				ULONG ulAuthPackage;
				hr = RetrieveNegotiateAuthPackage(&ulAuthPackage);
				if (SUCCEEDED(hr))
				{
					pcpcs->ulAuthenticationPackage = ulAuthPackage;
					pcpcs->clsidCredentialProvider = CLSID_NFCLOCProvider;

					// At this point the credential has created the serialized credential used for logon
					// By setting this to CPGSR_RETURN_CREDENTIAL_FINISHED we are letting logonUI know
					// that we have all the information we need and it should attempt to submit the 
					// serialized credential.
					*pcpgsr = CPGSR_RETURN_CREDENTIAL_FINISHED;
				}
			}
		}

		CoTaskMemFree(pwzProtectedPassword);
	}
	return hr;
}
