// Copyright � 2019-2020 Alex Kukhtin. All rights reserved.

#include "pch.h"
#include "posterm.h"
#include "equipmentbase.h"
#include "acqterminalimpl.h"
#include "acqt_Null.h"
#include "stringtools.h"


// virtual 
void AcqTerminal_Null::Open(const wchar_t* port, const wchar_t* log)
{
	TraceINFO(L"TESTACQUIRING [%s]. Open({port:'%s', log:'%s'})", _id.c_str(), port, log);
}

// virtual 
bool AcqTerminal_Null::Payment(long amount)
{
	TraceINFO(L"TESTACQUIRING [%s]. Payment({amount:'%ld'})", _id.c_str(), amount);
	::Sleep(3000);
	_response.Add(L"result", L"decline"); // action result
	_response.Add(L"receipt", L"43");
	_response.Add(L"card_pan", L"431403**********");
	_response.Add(L"date_time", L"20200303193504");
	_response.Add(L"merchant_id", L"030010000000006");
	_response.Add(L"status", L"�I������� � ������I!");
	_response.Add(L"status_code", "1");
	//_response.Add(L"decline_reason", L"� ����������� �������� ��� ��������� ������.");
	TraceINFO(L"\t Response:(%s)", _response.ToString().c_str());
	return true;
}
