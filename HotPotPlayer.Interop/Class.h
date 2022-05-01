#pragma once

#include "Class.g.h"

namespace winrt::HotPotPlayer_Interop::implementation
{
    struct Class : ClassT<Class>
    {
        Class() = default;

        int32_t MyProperty();
        void MyProperty(int32_t value);
    };
}

namespace winrt::HotPotPlayer_Interop::factory_implementation
{
    struct Class : ClassT<Class, implementation::Class>
    {
    };
}
