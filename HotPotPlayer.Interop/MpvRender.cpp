#include "pch.h"
#include "MpvRender.h"
#include "MpvRender.g.cpp"

namespace winrt::HotPotPlayer_Interop::implementation
{
    using namespace winrt;
    using namespace Microsoft::Graphics::Canvas;
    using namespace Windows::Foundation::Numerics;
    using namespace Windows::UI;

    int32_t MpvRender::Render(CanvasDrawingSession const& ds)
    {
        float2 center = { 50,50 };
        Color c = Colors::AliceBlue();
        ds.DrawCircle(center, 20, c);
        return 0;
    }
}
