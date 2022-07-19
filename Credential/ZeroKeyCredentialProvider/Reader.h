//
// THIS CODE AND INFORMATION IS PROVIDED "AS IS" WITHOUT WARRANTY OF
// ANY KIND, EITHER EXPRESSED OR IMPLIED, INCLUDING BUT NOT LIMITED TO
// THE IMPLIED WARRANTIES OF MERCHANTABILITY AND/OR FITNESS FOR A
// PARTICULAR PURPOSE.
//
// Copyright (c) 2006 Microsoft Corporation. All rights reserved.
//
// CCommandWindow provides a way to emulate external "connect" and "disconnect" 
// events, which are invoked via toggle button on a window. The window is launched
// and managed on a separate thread, which is necessary to ensure it gets pumped.
//

#pragma once

#include <windows.h>
#include <thread>
#include "NFCCredentialProvider.h"
#include "guid.h"
#include <string>

#pragma comment(lib, "Ws2_32.lib")

class reader
{
public:
	reader(void);
	~reader(void);
	HRESULT initialize(nfc_credential_provider *pProvider);
	void stop();
	void start();

	bool has_login();
	void clear_login();
	HRESULT get_login(
		CREDENTIAL_PROVIDER_GET_SERIALIZATION_RESPONSE* pcpgsr,
		CREDENTIAL_PROVIDER_CREDENTIAL_SERIALIZATION* pcpcs,
		PWSTR* ppwszOptionalStatusText,
		CREDENTIAL_PROVIDER_STATUS_ICON* pcpsiOptionalStatusIcon,
		CREDENTIAL_PROVIDER_USAGE_SCENARIO cpus
		);

private:
	void check_nfc();

	std::thread					reader_thread_;
	bool						check_loop_ = false;
	bool						kerbros_credential_retrieved_ = false;
	SOCKET						soc_{};
	bool						service_found_ = false;
	WSADATA						wsa_data_{};
	nfc_credential_provider     *p_provider_;				// Pointer to our owner.
	HINSTANCE                   h_inst_;					// Current instance
	std::string					username_;
	std::string					password_;
	std::string					domain_;
};
