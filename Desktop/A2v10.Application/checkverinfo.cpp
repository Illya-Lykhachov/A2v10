// Copyright � 2019 Alex Kukhtin. All rights reserved.

#include "stdafx.h"
#include "checkverinfo.h"

// virtual 
void CVersionModule::SetStringValue(const wchar_t* szName, const wchar_t* szValue)
{
	if (wcscmp(szName, L"module") == 0)
		m_module = szValue;
}

// virtual 
void CVersionModule::SetNumberValue(const wchar_t* szName, const wchar_t* szValue)
{
	if (wcscmp(szName, L"version") == 0)
		m_version = _wtoi(szValue);
	else if (wcscmp(szName, L"installed") == 0)
		m_installed = _wtoi(szValue);
}

CString CVersionModule::InstalledVersion() const
{
	return CConvert::Long2String(m_installed);
}

CString CVersionModule::RequiredVersion() const
{
	return CConvert::Long2String(m_version);
}


// CVersionModules 
///////////////////////
// virtual 
CVersionModules::~CVersionModules()
{
	Clear();
}

void CVersionModules::Clear()
{
	for (int i = 0; i < GetCount(); i++)
		delete ElementAt(i);
	RemoveAll();
}

// virtual 
JsonTarget* CVersionModules::CreateObject(const wchar_t* szName)
{
	CVersionModule*  pM = new CVersionModule();
	Add(pM);
	return pM;
}


bool CVersionModules::Parse(const wchar_t* json)
{
	Clear();
	try {
		JsonParser parser;
		parser.SetTarget(this);
		parser.Parse(json);
		return true;
	}
	catch (JsonException& je) {
		AfxMessageBox(je.GetMessage());
		return false;
	}
}
