#include <windows.h>
#include "ClassFactory.h"
#include "ExplorerCommand.h"

HMODULE g_hModule = nullptr;
long g_serverLocks = 0;

BOOL APIENTRY DllMain(HMODULE hModule, DWORD ul_reason_for_call, LPVOID)
{
    switch (ul_reason_for_call)
    {
    case DLL_PROCESS_ATTACH:
        g_hModule = hModule;
        DisableThreadLibraryCalls(hModule);
        break;
    case DLL_PROCESS_DETACH:
        break;
    }
    return TRUE;
}

STDAPI DllGetClassObject(REFCLSID rclsid, REFIID riid, void** ppv)
{
    if (!ppv) return E_POINTER;
    *ppv = nullptr;

    if (IsEqualCLSID(rclsid, CLSID_VeqloQuickCommand) ||
        IsEqualCLSID(rclsid, CLSID_VeqloQuickConvertCommand) ||
        IsEqualCLSID(rclsid, CLSID_VeqloQuickCompressCommand) ||
        IsEqualCLSID(rclsid, CLSID_VeqloQuickResizeCommand) ||
        IsEqualCLSID(rclsid, CLSID_VeqloQuickInspectCommand))
    {
        ClassFactory* pFactory = new (std::nothrow) ClassFactory(rclsid);
        if (!pFactory) return E_OUTOFMEMORY;

        HRESULT hr = pFactory->QueryInterface(riid, ppv);
        pFactory->Release();
        return hr;
    }

    return CLASS_E_CLASSNOTAVAILABLE;
}

STDAPI DllCanUnloadNow(void)
{
    return (g_serverLocks == 0) ? S_OK : S_FALSE;
}
