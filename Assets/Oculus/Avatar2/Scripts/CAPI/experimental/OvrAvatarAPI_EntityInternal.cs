using System;
using System.Runtime.InteropServices;

namespace Oculus.Avatar2.External
{
#pragma warning disable CA1401 // P/Invokes should not be visible
#pragma warning disable IDE1006 // Naming Styles
    public partial class InternalCAPI
    {
        public enum ovrAvatar2EndEffector : Int32
        {
            LeftArm = 0,
            RightArm = 1,
            LeftLeg = 2,
            RightLeg = 3,
        }

        [DllImport(LibFile, CallingConvention = CallingConvention.Cdecl)]
        public static extern CAPI.ovrAvatar2Result ovrAvatar2Entity_SetEndEffectorTargetTransform(
            CAPI.ovrAvatar2EntityId entityId,
            ovrAvatar2EndEffector endEffector,
            in CAPI.ovrAvatar2Transform targetTransform);
    }

#pragma warning restore IDE1006 // Naming Styles
#pragma warning restore CA1401 // P/Invokes should not be visible
}
