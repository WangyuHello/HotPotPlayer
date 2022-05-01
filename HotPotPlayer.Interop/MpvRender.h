#pragma once
#include "MpvRender.g.h"

namespace winrt::HotPotPlayer_Interop::implementation
{
    struct MpvRender : MpvRenderT<MpvRender>
    {
        MpvRender() = default;

        int32_t Render(winrt::Microsoft::Graphics::Canvas::CanvasDrawingSession const& ds);
    };
}
namespace winrt::HotPotPlayer_Interop::factory_implementation
{
    struct MpvRender : MpvRenderT<MpvRender, implementation::MpvRender>
    {
    };
}
