//
// THIS CODE AND INFORMATION IS PROVIDED "AS IS" WITHOUT WARRANTY OF
// ANY KIND, EITHER EXPRESSED OR IMPLIED, INCLUDING BUT NOT LIMITED TO
// THE IMPLIED WARRANTIES OF MERCHANTABILITY AND/OR FITNESS FOR A
// PARTICULAR PURPOSE.
//
// Copyright (c) Microsoft Corporation. All rights reserved.
//
//
#pragma once

#include <credentialprovider.h>
#include <windows.h>
#include <strsafe.h>

#include "Reader.h"
#include "NFCCredential.h"
#include "helpers.h"

class reader;
class nfc_credential;

class nfc_credential_provider final : public ICredentialProvider
{
public:
	// IUnknown
	STDMETHOD_(ULONG, AddRef)()
	{
		return _cRef++;
	}

	STDMETHOD_(ULONG, Release)()
	{
		LONG cRef = _cRef--;
		if (!cRef)
		{
			delete this;
		}
		return cRef;
	}

	STDMETHOD (QueryInterface)(REFIID riid, void** ppv)
	{
		HRESULT hr;
		if (IID_IUnknown == riid || 
			IID_ICredentialProvider == riid)
		{
			*ppv = this;
			static_cast<IUnknown*>(*ppv)->AddRef();
			hr = S_OK;
		}
		else
		{
			*ppv = nullptr;
			hr = E_NOINTERFACE;
		}
		return hr;
	}

public:
	IFACEMETHODIMP SetUsageScenario(CREDENTIAL_PROVIDER_USAGE_SCENARIO cpus, DWORD dwFlags) override;
	IFACEMETHODIMP SetSerialization(const CREDENTIAL_PROVIDER_CREDENTIAL_SERIALIZATION* pcpcs) override;

	IFACEMETHODIMP Advise(__in ICredentialProviderEvents* pcpe, UINT_PTR upAdviseContext) override;
	IFACEMETHODIMP UnAdvise() override;

	IFACEMETHODIMP GetFieldDescriptorCount(__out DWORD* pdwCount) override;
	IFACEMETHODIMP GetFieldDescriptorAt(DWORD dwIndex,  __deref_out CREDENTIAL_PROVIDER_FIELD_DESCRIPTOR** ppcpfd) override;

	IFACEMETHODIMP GetCredentialCount(
		__out DWORD* pdwCount,
		__out DWORD* pdwDefault,
		__out BOOL* pbAutoLogonWithDefault) override;
	IFACEMETHODIMP GetCredentialAt(
		DWORD dwIndex, 
		__out ICredentialProviderCredential** ppcpc) override;

	friend HRESULT ZeroKeyCredentialProvider_CreateInstance(REFIID riid, __deref_out void** ppv);

public:
	void OnNFCStatusChanged();

protected:
	nfc_credential_provider();
	__override ~nfc_credential_provider();

private:
	LONG              _cRef;
	nfc_credential*							_pCredential;          // Our "connected" credential.
	BOOL									_defaultProvider = CREDENTIAL_PROVIDER_NO_DEFAULT;
	CREDENTIAL_PROVIDER_USAGE_SCENARIO      _cpus;
	ICredentialProviderEvents*				_credentialProviderEvents = nullptr;
	UINT_PTR								_adviseContext = -1;
	reader*									_reader;
};