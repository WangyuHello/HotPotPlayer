using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;
using WinRT;

namespace HotPotPlayer.Interop
{
    public static class InteropHelper
    {
//        public static Guid GetIID<T>()
//        {
//#if NET5_0_OR_GREATER
//            if (TryGetDefaultInterfaceTypeForRuntimeClassType(typeof(T), out Type defaultInterfaceType))
//                return GuidGenerator.GetIID(defaultInterfaceType);
//            return GuidGenerator.GetIID(typeof(T));
//#else
//            return typeof(T).GetInterface("I" + typeof(T).Name)?.GUID ?? typeof(T).GUID;
//#endif
//        }

#if NET5_0_OR_GREATER
        /// <summary>
        /// <see href="https://github.com/microsoft/CsWinRT/blob/master/src/WinRT.Runtime/Projections.cs#L378-L397"/>
        /// </summary>
        public static bool TryGetDefaultInterfaceTypeForRuntimeClassType(Type runtimeClass, out Type? defaultInterface)
        {
            runtimeClass = runtimeClass.GetRuntimeClassCCWType() ?? runtimeClass;
            defaultInterface = null;

            ProjectedRuntimeClassAttribute? attr = runtimeClass.GetCustomAttribute<ProjectedRuntimeClassAttribute>();
            if (attr is null)
                return false;

            if (attr.DefaultInterfaceProperty != null)
                defaultInterface = runtimeClass?.GetProperty(attr.DefaultInterfaceProperty, BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.DeclaredOnly).PropertyType;
            else
                defaultInterface = attr.DefaultInterface;

            return true;
        }
#endif

        public static TInteropInterface GetActivationFactory<TClass, TInteropInterface>() => GetActivationFactory<TInteropInterface>(typeof(TClass));

        public static T GetActivationFactory<T>(Type classType)
        {
            try
            {
                // ToDo: Improve this (performance)!
                var method = classType.GetMethod("As", BindingFlags.Static | BindingFlags.Public);
                return method.MakeGenericMethod(new[] { typeof(T) }).Invoke(null, null).As<T>();
            }
            catch (Exception)
            {
                throw new PlatformNotSupportedException("Please use the built-in net5.0 interops!" + Environment.NewLine + "https://docs.microsoft.com/en-us/windows/apps/desktop/modernize/winrt-com-interop-csharp#available-interop-classes");
            }
        }

        public static T CastWinRTObject<T>(object value)
        {
#if NET5_0_OR_GREATER
            return MarshalInterface<T>.FromAbi(Marshal.GetIUnknownForObject(value));
#else
            return (T)value;
#endif
        }

        /// <summary>
        /// <c>true</c> if <see cref="HasPackageIdentity"/> and <see cref="IsAppContainer"/>
        /// </summary>
        /// <returns></returns>
        public static bool IsUWP() => HasPackageIdentity() && IsAppContainer();

        /// <summary>
        /// <c>true</c> if <see cref="HasPackageIdentity"/> and not <see cref="IsAppContainer"/>
        /// </summary>
        /// <returns></returns>
        public static bool IsPackagedWin32() => HasPackageIdentity() && !IsAppContainer();

        /// <summary>
        /// <c>true</c> if not <see cref="HasPackageIdentity"/> and not <see cref="IsAppContainer"/>
        /// </summary>
        /// <returns></returns>
        public static bool IsUnpackagedWin32() => !HasPackageIdentity() && !IsAppContainer();

        public static bool HasPackageIdentity()
        {
            int length = 0;
            GetCurrentPackageFullName(ref length, null);
            StringBuilder sb = new StringBuilder(length);
            int hResult = GetCurrentPackageFullName(ref length, sb);
            if (hResult == 0)
                return true;
            if (hResult == APPMODEL_ERROR_NO_PACKAGE)
                return false;
            throw new Win32Exception(hResult);
        }

        private const int APPMODEL_ERROR_NO_PACKAGE = 15700;

        [DllImport("kernel32.dll", CharSet = CharSet.Unicode, SetLastError = true)]
        private extern static int GetCurrentPackageFullName(ref int packageFullNameLength, StringBuilder packageFullName);

        public static bool IsAppContainer()
        {
            uint result = 0;
            uint size = sizeof(uint);
            if (!GetTokenInformation((IntPtr)CurrentProcessPseudoToken, TokenIsAppContainer, ref result, ref size, out size))
                throw new Win32Exception(Marshal.GetLastWin32Error());
            return Convert.ToBoolean(result);
        }

        private const int TokenIsAppContainer = 29;

        private const int CurrentProcessPseudoToken = -4;

        [DllImport("Advapi32.dll", SetLastError = true)]
        private extern static bool GetTokenInformation(IntPtr TokenHandle, uint TokenInformationClass, ref uint TokenInformation, ref uint TokenInformationLength, out uint ReturnLength);
    }
}
