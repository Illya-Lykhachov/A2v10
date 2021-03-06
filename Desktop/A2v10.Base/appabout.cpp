// Copyright © 2008-2017 Alex Kukhtin. All rights reserved.

#include "stdafx.h"
#include "resource.h"

#include "..\include\appdefs.h"
#include "..\include\appstructs.h"
#include "..\include\guiext.h"
#include "..\include\theme.h"
//#include "..\include\thhelp.h"
#include "..\include\appdata.h"

#include "appabout.h"

#include "..\include\versions.h"

#ifdef _DEBUG
#define new DEBUG_NEW
#endif

#define MASK_COLOR    (RGB(128, 0, 128))

#define IDC_NAME		IDC_TEXT1
#define IDC_BANNER		IDC_TEXT2
#define IDC_VERSION		IDC_PROMPT3
#define IDC_COPYRIGHT	IDC_PROMPT2
#define IDC_OTHERCOPYRIGHTS IDC_PROMPT5


CAppAboutDialog::CAppAboutDialog(CWnd* pParent /*=NULL*/)
	: CDialogEx(CAppAboutDialog::IDD, pParent)
{

}

CAppAboutDialog::~CAppAboutDialog()
{
}

void CAppAboutDialog::DoDataExchange(CDataExchange* pDX)
{
	__super::DoDataExchange(pDX);
}


BEGIN_MESSAGE_MAP(CAppAboutDialog, CDialogEx)
	ON_WM_PAINT()
END_MESSAGE_MAP()


// virtual
BOOL CAppAboutDialog::OnInitDialog()
{
	__super::OnInitDialog();

	// CThemeHelper::EnableThemeDialogTexture(GetSafeHwnd(), ETDT_ENABLETAB);

	CWnd* pWnd = GetDlgItem(IDC_BANNER);
	ATLASSERT(pWnd && pWnd->GetSafeHwnd());
	if (pWnd)
		pWnd->SetFont(CTheme::GetUIFont(CTheme::FontSmall));
	pWnd = GetDlgItem(IDC_NAME);
	ATLASSERT(pWnd && pWnd->GetSafeHwnd());
	if (pWnd)
		pWnd->SetFont(CTheme::GetUIFont(CTheme::FontBold));

	CModuleVersion ver;

	WCHAR szFullPath[_MAX_PATH + 1];
	WCHAR szShortFileName[_MAX_PATH + 1];
	::GetModuleFileName(::AfxGetInstanceHandle(), szFullPath, sizeof(szFullPath));
	_wcslwr_s(szFullPath, MAX_PATH);
	_tcscpy_s(szShortFileName, _MAX_PATH, szFullPath);
	::PathStripPath(szShortFileName);
	if (ver.GetFileVersionInfo(szFullPath)) {
		int i0 = (int) HIWORD(ver.dwFileVersionMS);
		int i1 = (int) LOWORD(ver.dwFileVersionMS);
		int b0 = (int) HIWORD(ver.dwFileVersionLS);
		int b1 = (int) LOWORD(ver.dwFileVersionLS);
		// i0.i1.b0.b1
		CString vs;
		CString bs;
		vs.Format(L"%d.%d", i0, i1);
		bs.Format(L"%d", b0);
		CString x;
		GetDlgItemText(IDC_VERSION, x);
		ATLASSERT(x.Find(L"%1") != -1);
		ATLASSERT(x.Find(L"%2") != -1);
		CString s;
		s.FormatMessage(x, (LPCWSTR) vs, (LPCWSTR) bs);
		SetDlgItemText(IDC_VERSION, s);
		SetDlgItemText(IDC_NAME, ver.GetValue(L"FileDescription"));
	}
	LPCWSTR szLang = CAppData::GetCurrentUILangCode();
	CString szLib;
	szLib.Format(L"A2v10.Locale.%s.dll", szLang);
	CString fp(szFullPath);
	fp.Replace(szShortFileName, szLib);
	if (szLib && ver.GetFileVersionInfo(fp)) {
		SetDlgItemText(IDC_COPYRIGHT, ver.GetValue(L"LegalCopyright"));
	}

	CString msCopyright(EMPTYSTR);

	return TRUE;
}

// CAppAboutDialog message handlers

// afx_msg
void CAppAboutDialog::OnPaint()
{
	CPaintDC dc(this); // device context for painting
	CWnd* pWnd = GetDlgItem(IDC_PROMPT1);
	if (!pWnd)
		return;
	CRect rc;
	pWnd->GetClientRect(&rc);
	pWnd->MapWindowPoints(this, &rc);
	if (m_imageList.GetSafeHandle() == NULL) {
		CBitmap bmp;
		bmp.LoadBitmap(IDIL_48X48);
		m_imageList.Create(48, 48, ILC_COLOR24 | ILC_MASK, 0, 0);
		m_imageList.Add(&bmp, MASK_COLOR);
	}
	int iImage = 0;
	m_imageList.Draw(&dc, iImage, rc.TopLeft(), ILD_NORMAL);
}

