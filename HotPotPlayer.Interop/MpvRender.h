#pragma once
#include "MpvRender.g.h"
#include "winrt/Microsoft.Graphics.Canvas.h"
#include "winrt/Windows.Foundation.Numerics.h"
#include "winrt/Windows.UI.h"

namespace winrt::HotPotPlayer_Interop::implementation
{
    struct MpvRender : MpvRenderT<MpvRender>
    {
        MpvRender() = default;

        int32_t Render(int32_t ds);
    };
}
namespace winrt::HotPotPlayer_Interop::factory_implementation
{
    struct MpvRender : MpvRenderT<MpvRender, implementation::MpvRender>
    {
    };
}
