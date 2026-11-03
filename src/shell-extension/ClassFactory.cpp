#include "ClassFactory.h"
#include "ExplorerCommand.h"

extern long g_serverLocks;

ClassFactory::ClassFactory(REFCLSID rclsid) : m_refCount(1), m_clsid(rclsid)
{
    InterlockedIncrement(&g_serverLocks);
}

ClassFactory::~ClassFactory()
{
    InterlockedDecrement(&g_serverLocks);
}

IFACEMETHODIMP ClassFactory::QueryInterface(REFIID riid, void** ppv)
{
    if (!ppv) return E_POINTER;
    *ppv = nullptr;

    if (IsEqualIID(riid, IID_IUnknown) || IsEqualIID(riid, IID_IClassFactory))
    {
        *ppv = static_cast<IClassFactory*>(this);
        AddRef();
        return S_OK;
    }
    return E_NOINTERFACE;
}

IFACEMETHODIMP_(ULONG) ClassFactory::AddRef()
{
    return InterlockedIncrement(&m_refCount);
}

IFACEMETHODIMP_(ULONG) ClassFactory::Release()
{
    ULONG count = InterlockedDecrement(&m_refCount);
    if (count == 0)
    {
        delete this;
    }
    return count;
}

IFACEMETHODIMP ClassFactory::CreateInstance(IUnknown* pUnkOuter, REFIID riid, void** ppv)
{
    if (!ppv) return E_POINTER;
    *ppv = nullptr;

    if (pUnkOuter != nullptr)
    {
        return CLASS_E_NOAGGREGATION;
    }

    if (IsEqualCLSID(m_clsid, CLSID_VeqloQuickCommand))
    {
        VeqloQuickCommand* pCommand = new (std::nothrow) VeqloQuickCommand();
        if (!pCommand) return E_OUTOFMEMORY;
        HRESULT hr = pCommand->QueryInterface(riid, ppv);
        pCommand->Release();
        return hr;
    }
    else if (IsEqualCLSID(m_clsid, CLSID_VeqloQuickConvertCommand))
    {
        VeqloQuickConvertCommand* pCommand = new (std::nothrow) VeqloQuickConvertCommand();
        if (!pCommand) return E_OUTOFMEMORY;
        HRESULT hr = pCommand->QueryInterface(riid, ppv);
        pCommand->Release();
        return hr;
    }
    else if (IsEqualCLSID(m_clsid, CLSID_VeqloQuickCompressCommand))
    {
        VeqloQuickCompressCommand* pCommand = new (std::nothrow) VeqloQuickCompressCommand();
        if (!pCommand) return E_OUTOFMEMORY;
        HRESULT hr = pCommand->QueryInterface(riid, ppv);
        pCommand->Release();
        return hr;
    }
    else if (IsEqualCLSID(m_clsid, CLSID_VeqloQuickResizeCommand))
    {
        VeqloQuickResizeCommand* pCommand = new (std::nothrow) VeqloQuickResizeCommand();
        if (!pCommand) return E_OUTOFMEMORY;
        HRESULT hr = pCommand->QueryInterface(riid, ppv);
        pCommand->Release();
        return hr;
    }
    else if (IsEqualCLSID(m_clsid, CLSID_VeqloQuickInspectCommand))
    {
        VeqloQuickInspectCommand* pCommand = new (std::nothrow) VeqloQuickInspectCommand();
        if (!pCommand) return E_OUTOFMEMORY;
        HRESULT hr = pCommand->QueryInterface(riid, ppv);
        pCommand->Release();
        return hr;
    }

    return CLASS_E_CLASSNOTAVAILABLE;
}

IFACEMETHODIMP ClassFactory::LockServer(BOOL fLock)
{
    if (fLock)
    {
        InterlockedIncrement(&g_serverLocks);
    }
    else
    {
        InterlockedDecrement(&g_serverLocks);
    }
    return S_OK;
}
