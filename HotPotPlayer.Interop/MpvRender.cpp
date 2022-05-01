#include "pch.h"
#include "MpvRender.h"
#include "MpvRender.g.cpp"

namespace winrt::HotPotPlayer_Interop::implementation
{
    int32_t MpvRender::Render(int32_t ds)
    {
        return ds + 10;
    }
}
