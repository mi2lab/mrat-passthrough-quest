using System;

namespace Oculus.Avatar2
{
    public partial class OvrAvatarEntity : UnityEngine.MonoBehaviour
    {
        private CAPI.ovrAvatar2EntityId CreateNativeEntity(in CAPI.ovrAvatar2EntityCreateInfo info)
        {
            return CAPI.OvrAvatar2Entity_Create(in info, this);
        }

        private bool DestroyNativeEntity()
        {
            if (entityId == CAPI.ovrAvatar2EntityId.Invalid)
            {
                OvrAvatarLog.LogWarning("Attempted to destroy entity with invalid ID", logScope, this);
                return false;
            }
            if (!CAPI.OvrAvatar2Entity_Destroy(entityId, this))
            {
                OvrAvatarLog.LogError($"Failed to destroy entity on gameObject:`{name}`", logScope, this);
                return false;
            }
            OvrAvatarLog.LogVerbose("Successfully destroyed native entity", logScope, this);
            entityId = CAPI.ovrAvatar2EntityId.Invalid;
            return true;
        }
    }

    public partial class CAPI
    {
        private const string lifecycleScope = "lifecycle";

        /// Destroy an entity, releasing all related memory
        /// \param entity to destroy
        /// \return result code
        ///
        internal static ovrAvatar2EntityId OvrAvatar2Entity_Create(in ovrAvatar2EntityCreateInfo info, OvrAvatarEntity context)
        {
            if (ovrAvatar2Entity_Create(in info, out var entityId)
                .EnsureSuccess("ovrAvatar2Entity_Create", lifecycleScope, context))
            {
                return entityId;
            }
            return CAPI.ovrAvatar2EntityId.Invalid;
        }

        /// Destroy an entity, releasing all related memory
        /// \param entity to destroy
        /// \return result code
        ///
        internal static bool OvrAvatar2Entity_Destroy(ovrAvatar2EntityId entityId, OvrAvatarEntity context)
        {
            OvrAvatarLog.Assert(entityId != ovrAvatar2EntityId.Invalid, entityLogScope, context);
            return ovrAvatar2Entity_Destroy(entityId)
                .EnsureSuccess("ovrAvatar2Entity_Destroy", lifecycleScope, context);
        }
    }
}
