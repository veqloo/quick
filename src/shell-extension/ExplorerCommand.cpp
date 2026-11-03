#include "ExplorerCommand.h"
#include <shlwapi.h>
#include <cwchar>

// GUIDs
// {65863AF6-B22D-4C52-BB0F-610D11268E04}
extern "C" const GUID CLSID_VeqloQuickCommand =
    { 0x65863af6, 0xb22d, 0x4c52, { 0xbb, 0xf, 0x61, 0xd, 0x11, 0x26, 0x8e, 0x4 } };

// {1ADC95E5-04A9-45AD-9CE9-470680AA2E5A}
extern "C" const GUID CLSID_VeqloQuickConvertCommand =
    { 0x1adc95e5, 0x4a9, 0x45ad, { 0x9c, 0xe9, 0x47, 0x6, 0x80, 0xaa, 0x2e, 0x5a } };

// {2BDD86F6-15BA-46BE-ADFA-581791BB3F6B}
extern "C" const GUID CLSID_VeqloQuickCompressCommand =
    { 0x2bdd86f6, 0x15ba, 0x46be, { 0xad, 0xfa, 0x58, 0x17, 0x91, 0xbb, 0x3f, 0x6b } };

// {3CEE9707-26CB-47CF-BE0B-692802CC407C}
extern "C" const GUID CLSID_VeqloQuickResizeCommand =
    { 0x3cee9707, 0x26cb, 0x47cf, { 0xbe, 0xb, 0x69, 0x28, 0x2, 0xcc, 0x40, 0x7c } };

// {4DFF0818-37DC-48DF-BF1C-7A3913DD518D}
extern "C" const GUID CLSID_VeqloQuickInspectCommand =
    { 0x4dff0818, 0x37dc, 0x48df, { 0xbf, 0x1c, 0x7a, 0x39, 0x13, 0xdd, 0x51, 0x8d } };

extern HMODULE g_hModule;
extern long g_serverLocks;

static HRESULT DuplicateShellString(const wchar_t* source, LPWSTR* dest)
{
    if (!dest) return E_POINTER;
    *dest = nullptr;
    return SHStrDupW(source, dest);
}

static std::wstring GetExecutablePath()
{
    wchar_t moduleDir[MAX_PATH] = { 0 };
    GetModuleFileNameW(g_hModule, moduleDir, ARRAYSIZE(moduleDir));
    PathRemoveFileSpecW(moduleDir);

    wchar_t exePath[MAX_PATH] = { 0 };
    PathCombineW(exePath, moduleDir, L"VeqloQuick.exe");
    return std::wstring(exePath);
}

bool LaunchVeqloQuickAction(const std::wstring& verb, IShellItemArray* psiItemArray)
{
    if (!psiItemArray) return false;

    DWORD itemCount = 0;
    HRESULT hr = psiItemArray->GetCount(&itemCount);
    if (FAILED(hr) || itemCount == 0) return false;

    std::wstring exePath = GetExecutablePath();
    std::wstring commandLine = L"\"" + exePath + L"\" " + verb;

    for (DWORD i = 0; i < itemCount; i++)
    {
        IShellItem* pItem = nullptr;
        if (SUCCEEDED(psiItemArray->GetItemAt(i, &pItem)) && pItem)
        {
            LPWSTR pszPath = nullptr;
            if (SUCCEEDED(pItem->GetDisplayName(SIGDN_FILESYSPATH, &pszPath)) && pszPath)
            {
                commandLine += L" --input \"" + std::wstring(pszPath) + L"\"";
                CoTaskMemFree(pszPath);
            }
            pItem->Release();
        }
    }

    STARTUPINFOW si = { sizeof(si) };
    PROCESS_INFORMATION pi = { 0 };

    std::vector<wchar_t> cmdBuffer(commandLine.begin(), commandLine.end());
    cmdBuffer.push_back(L'\0');

    BOOL success = CreateProcessW(
        nullptr,
        cmdBuffer.data(),
        nullptr,
        nullptr,
        FALSE,
        0,
        nullptr,
        nullptr,
        &si,
        &pi
    );

    if (success)
    {
        CloseHandle(pi.hProcess);
        CloseHandle(pi.hThread);
        return true;
    }
    return false;
}

// --------------------------------------------------------------------------
// VeqloQuickCommand (Parent Item: "Veqlo Quick")
// --------------------------------------------------------------------------
VeqloQuickCommand::VeqloQuickCommand() : m_refCount(1)
{
    InterlockedIncrement(&g_serverLocks);
}

VeqloQuickCommand::~VeqloQuickCommand()
{
    InterlockedDecrement(&g_serverLocks);
}

IFACEMETHODIMP VeqloQuickCommand::QueryInterface(REFIID riid, void** ppv)
{
    if (!ppv) return E_POINTER;
    *ppv = nullptr;

    if (IsEqualIID(riid, IID_IUnknown) || IsEqualIID(riid, IID_IExplorerCommand))
    {
        *ppv = static_cast<IExplorerCommand*>(this);
        AddRef();
        return S_OK;
    }
    return E_NOINTERFACE;
}

IFACEMETHODIMP_(ULONG) VeqloQuickCommand::AddRef()
{
    return InterlockedIncrement(&m_refCount);
}

IFACEMETHODIMP_(ULONG) VeqloQuickCommand::Release()
{
    ULONG count = InterlockedDecrement(&m_refCount);
    if (count == 0)
    {
        delete this;
    }
    return count;
}

IFACEMETHODIMP VeqloQuickCommand::GetTitle(IShellItemArray*, LPWSTR* ppszName)
{
    return DuplicateShellString(L"Veqlo Quick", ppszName);
}

IFACEMETHODIMP VeqloQuickCommand::GetIcon(IShellItemArray*, LPWSTR* ppszIcon)
{
    wchar_t modulePath[MAX_PATH] = { 0 };
    if (GetModuleFileNameW(g_hModule, modulePath, ARRAYSIZE(modulePath)))
    {
        wchar_t iconSpec[MAX_PATH + 8] = { 0 };
        swprintf_s(iconSpec, ARRAYSIZE(iconSpec), L"%s,-101", modulePath);
        return DuplicateShellString(iconSpec, ppszIcon);
    }
    return E_NOTIMPL;
}

IFACEMETHODIMP VeqloQuickCommand::GetToolTip(IShellItemArray*, LPWSTR* ppszInfoTip)
{
    return DuplicateShellString(L"Veqlo Quick Image Tools", ppszInfoTip);
}

IFACEMETHODIMP VeqloQuickCommand::GetCanonicalName(GUID* pguidCommandName)
{
    if (!pguidCommandName) return E_POINTER;
    *pguidCommandName = CLSID_VeqloQuickCommand;
    return S_OK;
}

IFACEMETHODIMP VeqloQuickCommand::GetState(IShellItemArray*, BOOL, EXPCMDSTATE* pCmdState)
{
    if (!pCmdState) return E_POINTER;
    *pCmdState = ECS_ENABLED;
    return S_OK;
}

IFACEMETHODIMP VeqloQuickCommand::Invoke(IShellItemArray*, IBindCtx*)
{
    return S_OK;
}

IFACEMETHODIMP VeqloQuickCommand::GetFlags(EXPCMDFLAGS* pFlags)
{
    if (!pFlags) return E_POINTER;
    *pFlags = ECF_HASSUBCOMMANDS;
    return S_OK;
}

IFACEMETHODIMP VeqloQuickCommand::EnumSubCommands(IEnumExplorerCommand** ppEnum)
{
    if (!ppEnum) return E_POINTER;
    *ppEnum = nullptr;

    VeqloQuickEnumCommands* pEnum = new (std::nothrow) VeqloQuickEnumCommands();
    if (!pEnum) return E_OUTOFMEMORY;

    *ppEnum = static_cast<IEnumExplorerCommand*>(pEnum);
    return S_OK;
}

// --------------------------------------------------------------------------
// VeqloQuickConvertCommand (Subcommand: "Convert")
// --------------------------------------------------------------------------
VeqloQuickConvertCommand::VeqloQuickConvertCommand() : m_refCount(1)
{
    InterlockedIncrement(&g_serverLocks);
}

VeqloQuickConvertCommand::~VeqloQuickConvertCommand()
{
    InterlockedDecrement(&g_serverLocks);
}

IFACEMETHODIMP VeqloQuickConvertCommand::QueryInterface(REFIID riid, void** ppv)
{
    if (!ppv) return E_POINTER;
    *ppv = nullptr;

    if (IsEqualIID(riid, IID_IUnknown) || IsEqualIID(riid, IID_IExplorerCommand))
    {
        *ppv = static_cast<IExplorerCommand*>(this);
        AddRef();
        return S_OK;
    }
    return E_NOINTERFACE;
}

IFACEMETHODIMP_(ULONG) VeqloQuickConvertCommand::AddRef()
{
    return InterlockedIncrement(&m_refCount);
}

IFACEMETHODIMP_(ULONG) VeqloQuickConvertCommand::Release()
{
    ULONG count = InterlockedDecrement(&m_refCount);
    if (count == 0)
    {
        delete this;
    }
    return count;
}

IFACEMETHODIMP VeqloQuickConvertCommand::GetTitle(IShellItemArray*, LPWSTR* ppszName)
{
    return DuplicateShellString(L"Convert", ppszName);
}

IFACEMETHODIMP VeqloQuickConvertCommand::GetIcon(IShellItemArray*, LPWSTR* ppszIcon)
{
    return E_NOTIMPL;
}

IFACEMETHODIMP VeqloQuickConvertCommand::GetToolTip(IShellItemArray*, LPWSTR* ppszInfoTip)
{
    return DuplicateShellString(L"Convert image to JPG, PNG, WebP, BMP, TIFF, ICO", ppszInfoTip);
}

IFACEMETHODIMP VeqloQuickConvertCommand::GetCanonicalName(GUID* pguidCommandName)
{
    if (!pguidCommandName) return E_POINTER;
    *pguidCommandName = CLSID_VeqloQuickConvertCommand;
    return S_OK;
}

IFACEMETHODIMP VeqloQuickConvertCommand::GetState(IShellItemArray*, BOOL, EXPCMDSTATE* pCmdState)
{
    if (!pCmdState) return E_POINTER;
    *pCmdState = ECS_ENABLED;
    return S_OK;
}

IFACEMETHODIMP VeqloQuickConvertCommand::GetFlags(EXPCMDFLAGS* pFlags)
{
    if (!pFlags) return E_POINTER;
    *pFlags = ECF_DEFAULT;
    return S_OK;
}

IFACEMETHODIMP VeqloQuickConvertCommand::EnumSubCommands(IEnumExplorerCommand** ppEnum)
{
    if (!ppEnum) return E_POINTER;
    *ppEnum = nullptr;
    return E_NOTIMPL;
}

IFACEMETHODIMP VeqloQuickConvertCommand::Invoke(IShellItemArray* psiItemArray, IBindCtx*)
{
    if (LaunchVeqloQuickAction(L"convert", psiItemArray))
        return S_OK;
    return E_FAIL;
}

// --------------------------------------------------------------------------
// VeqloQuickCompressCommand (Subcommand: "Compress")
// --------------------------------------------------------------------------
VeqloQuickCompressCommand::VeqloQuickCompressCommand() : m_refCount(1)
{
    InterlockedIncrement(&g_serverLocks);
}

VeqloQuickCompressCommand::~VeqloQuickCompressCommand()
{
    InterlockedDecrement(&g_serverLocks);
}

IFACEMETHODIMP VeqloQuickCompressCommand::QueryInterface(REFIID riid, void** ppv)
{
    if (!ppv) return E_POINTER;
    *ppv = nullptr;

    if (IsEqualIID(riid, IID_IUnknown) || IsEqualIID(riid, IID_IExplorerCommand))
    {
        *ppv = static_cast<IExplorerCommand*>(this);
        AddRef();
        return S_OK;
    }
    return E_NOINTERFACE;
}

IFACEMETHODIMP_(ULONG) VeqloQuickCompressCommand::AddRef()
{
    return InterlockedIncrement(&m_refCount);
}

IFACEMETHODIMP_(ULONG) VeqloQuickCompressCommand::Release()
{
    ULONG count = InterlockedDecrement(&m_refCount);
    if (count == 0)
    {
        delete this;
    }
    return count;
}

IFACEMETHODIMP VeqloQuickCompressCommand::GetTitle(IShellItemArray*, LPWSTR* ppszName)
{
    return DuplicateShellString(L"Compress", ppszName);
}

IFACEMETHODIMP VeqloQuickCompressCommand::GetIcon(IShellItemArray*, LPWSTR* ppszIcon)
{
    return E_NOTIMPL;
}

IFACEMETHODIMP VeqloQuickCompressCommand::GetToolTip(IShellItemArray*, LPWSTR* ppszInfoTip)
{
    return DuplicateShellString(L"Reduce file size with real re-encoding", ppszInfoTip);
}

IFACEMETHODIMP VeqloQuickCompressCommand::GetCanonicalName(GUID* pguidCommandName)
{
    if (!pguidCommandName) return E_POINTER;
    *pguidCommandName = CLSID_VeqloQuickCompressCommand;
    return S_OK;
}

IFACEMETHODIMP VeqloQuickCompressCommand::GetState(IShellItemArray*, BOOL, EXPCMDSTATE* pCmdState)
{
    if (!pCmdState) return E_POINTER;
    *pCmdState = ECS_ENABLED;
    return S_OK;
}

IFACEMETHODIMP VeqloQuickCompressCommand::GetFlags(EXPCMDFLAGS* pFlags)
{
    if (!pFlags) return E_POINTER;
    *pFlags = ECF_DEFAULT;
    return S_OK;
}

IFACEMETHODIMP VeqloQuickCompressCommand::EnumSubCommands(IEnumExplorerCommand** ppEnum)
{
    if (!ppEnum) return E_POINTER;
    *ppEnum = nullptr;
    return E_NOTIMPL;
}

IFACEMETHODIMP VeqloQuickCompressCommand::Invoke(IShellItemArray* psiItemArray, IBindCtx*)
{
    if (LaunchVeqloQuickAction(L"compress", psiItemArray))
        return S_OK;
    return E_FAIL;
}

// --------------------------------------------------------------------------
// VeqloQuickResizeCommand (Subcommand: "Resize")
// --------------------------------------------------------------------------
VeqloQuickResizeCommand::VeqloQuickResizeCommand() : m_refCount(1)
{
    InterlockedIncrement(&g_serverLocks);
}

VeqloQuickResizeCommand::~VeqloQuickResizeCommand()
{
    InterlockedDecrement(&g_serverLocks);
}

IFACEMETHODIMP VeqloQuickResizeCommand::QueryInterface(REFIID riid, void** ppv)
{
    if (!ppv) return E_POINTER;
    *ppv = nullptr;

    if (IsEqualIID(riid, IID_IUnknown) || IsEqualIID(riid, IID_IExplorerCommand))
    {
        *ppv = static_cast<IExplorerCommand*>(this);
        AddRef();
        return S_OK;
    }
    return E_NOINTERFACE;
}

IFACEMETHODIMP_(ULONG) VeqloQuickResizeCommand::AddRef()
{
    return InterlockedIncrement(&m_refCount);
}

IFACEMETHODIMP_(ULONG) VeqloQuickResizeCommand::Release()
{
    ULONG count = InterlockedDecrement(&m_refCount);
    if (count == 0)
    {
        delete this;
    }
    return count;
}

IFACEMETHODIMP VeqloQuickResizeCommand::GetTitle(IShellItemArray*, LPWSTR* ppszName)
{
    return DuplicateShellString(L"Resize", ppszName);
}

IFACEMETHODIMP VeqloQuickResizeCommand::GetIcon(IShellItemArray*, LPWSTR* ppszIcon)
{
    return E_NOTIMPL;
}

IFACEMETHODIMP VeqloQuickResizeCommand::GetToolTip(IShellItemArray*, LPWSTR* ppszInfoTip)
{
    return DuplicateShellString(L"Resize image dimensions with high fidelity", ppszInfoTip);
}

IFACEMETHODIMP VeqloQuickResizeCommand::GetCanonicalName(GUID* pguidCommandName)
{
    if (!pguidCommandName) return E_POINTER;
    *pguidCommandName = CLSID_VeqloQuickResizeCommand;
    return S_OK;
}

IFACEMETHODIMP VeqloQuickResizeCommand::GetState(IShellItemArray*, BOOL, EXPCMDSTATE* pCmdState)
{
    if (!pCmdState) return E_POINTER;
    *pCmdState = ECS_ENABLED;
    return S_OK;
}

IFACEMETHODIMP VeqloQuickResizeCommand::GetFlags(EXPCMDFLAGS* pFlags)
{
    if (!pFlags) return E_POINTER;
    *pFlags = ECF_DEFAULT;
    return S_OK;
}

IFACEMETHODIMP VeqloQuickResizeCommand::EnumSubCommands(IEnumExplorerCommand** ppEnum)
{
    if (!ppEnum) return E_POINTER;
    *ppEnum = nullptr;
    return E_NOTIMPL;
}

IFACEMETHODIMP VeqloQuickResizeCommand::Invoke(IShellItemArray* psiItemArray, IBindCtx*)
{
    if (LaunchVeqloQuickAction(L"resize", psiItemArray))
        return S_OK;
    return E_FAIL;
}

// --------------------------------------------------------------------------
// VeqloQuickInspectCommand ("Inspect details" Sub-command)
// --------------------------------------------------------------------------
VeqloQuickInspectCommand::VeqloQuickInspectCommand() : m_refCount(1)
{
    InterlockedIncrement(&g_serverLocks);
}

VeqloQuickInspectCommand::~VeqloQuickInspectCommand()
{
    InterlockedDecrement(&g_serverLocks);
}

IFACEMETHODIMP VeqloQuickInspectCommand::QueryInterface(REFIID riid, void** ppv)
{
    if (!ppv) return E_POINTER;
    *ppv = nullptr;

    if (IsEqualIID(riid, IID_IUnknown) || IsEqualIID(riid, IID_IExplorerCommand))
    {
        *ppv = static_cast<IExplorerCommand*>(this);
        AddRef();
        return S_OK;
    }
    return E_NOINTERFACE;
}

IFACEMETHODIMP_(ULONG) VeqloQuickInspectCommand::AddRef()
{
    return InterlockedIncrement(&m_refCount);
}

IFACEMETHODIMP_(ULONG) VeqloQuickInspectCommand::Release()
{
    ULONG count = InterlockedDecrement(&m_refCount);
    if (count == 0)
    {
        delete this;
    }
    return count;
}

IFACEMETHODIMP VeqloQuickInspectCommand::GetTitle(IShellItemArray*, LPWSTR* ppszName)
{
    return DuplicateShellString(L"Inspect details", ppszName);
}

IFACEMETHODIMP VeqloQuickInspectCommand::GetIcon(IShellItemArray*, LPWSTR* ppszIcon)
{
    std::wstring iconSpec = GetExecutablePath() + L",0";
    return DuplicateShellString(iconSpec.c_str(), ppszIcon);
}

IFACEMETHODIMP VeqloQuickInspectCommand::GetToolTip(IShellItemArray*, LPWSTR* ppszInfoTip)
{
    return DuplicateShellString(L"Inspect image specifications, EXIF tags, and clean metadata", ppszInfoTip);
}

IFACEMETHODIMP VeqloQuickInspectCommand::GetCanonicalName(GUID* pguidCommandName)
{
    if (!pguidCommandName) return E_POINTER;
    *pguidCommandName = CLSID_VeqloQuickInspectCommand;
    return S_OK;
}

IFACEMETHODIMP VeqloQuickInspectCommand::GetState(IShellItemArray*, BOOL, EXPCMDSTATE* pCmdState)
{
    if (!pCmdState) return E_POINTER;
    *pCmdState = ECS_ENABLED;
    return S_OK;
}

IFACEMETHODIMP VeqloQuickInspectCommand::GetFlags(EXPCMDFLAGS* pFlags)
{
    if (!pFlags) return E_POINTER;
    *pFlags = ECF_DEFAULT;
    return S_OK;
}

IFACEMETHODIMP VeqloQuickInspectCommand::EnumSubCommands(IEnumExplorerCommand** ppEnum)
{
    if (!ppEnum) return E_POINTER;
    *ppEnum = nullptr;
    return E_NOTIMPL;
}

IFACEMETHODIMP VeqloQuickInspectCommand::Invoke(IShellItemArray* psiItemArray, IBindCtx*)
{
    if (LaunchVeqloQuickAction(L"inspect", psiItemArray))
        return S_OK;
    return E_FAIL;
}

// --------------------------------------------------------------------------
// VeqloQuickEnumCommands
// --------------------------------------------------------------------------
VeqloQuickEnumCommands::VeqloQuickEnumCommands() : m_refCount(1), m_currentIndex(0)
{
    InterlockedIncrement(&g_serverLocks);
    m_commands.push_back(new VeqloQuickConvertCommand());
    m_commands.push_back(new VeqloQuickCompressCommand());
    m_commands.push_back(new VeqloQuickResizeCommand());
    m_commands.push_back(new VeqloQuickInspectCommand());
}

VeqloQuickEnumCommands::~VeqloQuickEnumCommands()
{
    for (auto* cmd : m_commands)
    {
        if (cmd) cmd->Release();
    }
    m_commands.clear();
    InterlockedDecrement(&g_serverLocks);
}

IFACEMETHODIMP VeqloQuickEnumCommands::QueryInterface(REFIID riid, void** ppv)
{
    if (!ppv) return E_POINTER;
    *ppv = nullptr;

    if (IsEqualIID(riid, IID_IUnknown) || IsEqualIID(riid, IID_IEnumExplorerCommand))
    {
        *ppv = static_cast<IEnumExplorerCommand*>(this);
        AddRef();
        return S_OK;
    }
    return E_NOINTERFACE;
}

IFACEMETHODIMP_(ULONG) VeqloQuickEnumCommands::AddRef()
{
    return InterlockedIncrement(&m_refCount);
}

IFACEMETHODIMP_(ULONG) VeqloQuickEnumCommands::Release()
{
    ULONG count = InterlockedDecrement(&m_refCount);
    if (count == 0)
    {
        delete this;
    }
    return count;
}

IFACEMETHODIMP VeqloQuickEnumCommands::Next(ULONG celt, IExplorerCommand** pUICommand, ULONG* pceltFetched)
{
    if (!pUICommand) return E_POINTER;
    if (celt == 0) return S_OK;

    ULONG fetched = 0;
    while (m_currentIndex < m_commands.size() && fetched < celt)
    {
        pUICommand[fetched] = m_commands[m_currentIndex];
        pUICommand[fetched]->AddRef();
        m_currentIndex++;
        fetched++;
    }

    if (pceltFetched)
    {
        *pceltFetched = fetched;
    }

    return (fetched == celt) ? S_OK : S_FALSE;
}

IFACEMETHODIMP VeqloQuickEnumCommands::Skip(ULONG celt)
{
    m_currentIndex += celt;
    if (m_currentIndex > m_commands.size())
    {
        m_currentIndex = m_commands.size();
        return S_FALSE;
    }
    return S_OK;
}

IFACEMETHODIMP VeqloQuickEnumCommands::Reset()
{
    m_currentIndex = 0;
    return S_OK;
}

IFACEMETHODIMP VeqloQuickEnumCommands::Clone(IEnumExplorerCommand** ppEnum)
{
    if (!ppEnum) return E_POINTER;
    *ppEnum = nullptr;

    VeqloQuickEnumCommands* pClone = new (std::nothrow) VeqloQuickEnumCommands();
    if (!pClone) return E_OUTOFMEMORY;

    *ppEnum = static_cast<IEnumExplorerCommand*>(pClone);
    return S_OK;
}
