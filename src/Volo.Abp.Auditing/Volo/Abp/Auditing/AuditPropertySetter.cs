﻿using Volo.Abp.DependencyInjection;
using Volo.Abp.MultiTenancy;
using Volo.Abp.Timing;
using Volo.Abp.Users;

namespace Volo.Abp.Auditing
{
    public class AuditPropertySetter : IAuditPropertySetter, ITransientDependency
    {
        protected ICurrentUser CurrentUser { get; }
        protected ICurrentTenant CurrentTenant { get; }
        protected IClock Clock { get; }

        public AuditPropertySetter(
            ICurrentUser currentUser, 
            ICurrentTenant currentTenant, 
            IClock clock)
        {
            CurrentUser = currentUser;
            CurrentTenant = currentTenant;
            Clock = clock;
        }

        public void SetCreationProperties(object targetObject)
        {
            SetCreationTime(targetObject);
            SetCreatorId(targetObject);
        }

        public void SetModificationProperties(object targetObject)
        {
            SetLastModificationTime(targetObject);
            SetLastModifierId(targetObject);
        }

        public void SetDeletionProperties(object targetObject)
        {
            SetDeletionTime(targetObject);
            SetDeleterId(targetObject);
        }

        private void SetCreationTime(object targetObject)
        {
            if (!(targetObject is IHasCreationTime objectWithCreationTime))
            {
                return;
            }

            if (objectWithCreationTime.CreationTime == default)
            {
                objectWithCreationTime.CreationTime = Clock.Now;
            }
        }

        private void SetCreatorId(object targetObject)
        {
            if (!(targetObject is ICreationAudited creationAuditedObject))
            {
                return;
            }

            if (creationAuditedObject.CreatorId != null)
            {
                return;
            }

            if (!CurrentUser.Id.HasValue)
            {
                return;
            }

            if (targetObject is IMultiTenant multiTenantEntity)
            {
                if (multiTenantEntity.TenantId != CurrentUser.TenantId)
                {
                    return;
                }
            }

            /* TODO: The code below is from old ABP, not implemented yet
            if (tenantId.HasValue && MultiTenancyHelper.IsHostEntity(entity))
            {
                //Tenant user created a host entity
                return;
            }
             */

            creationAuditedObject.CreatorId = CurrentUser.Id;
        }
        
        private void SetLastModificationTime(object targetObject)
        {
            if (targetObject is IHasModificationTime objectWithModificationTime)
            {
                objectWithModificationTime.LastModificationTime = Clock.Now;
            }
        }

        private void SetLastModifierId(object targetObject)
        {
            if (!(targetObject is IModificationAudited modificationAuditedObject))
            {
                return;
            }

            if (!CurrentUser.Id.HasValue)
            {
                modificationAuditedObject.LastModifierId = null;
                return;
            }

            if (modificationAuditedObject is IMultiTenant multiTenantEntity)
            {
                if (multiTenantEntity.TenantId != CurrentUser.TenantId)
                {
                    modificationAuditedObject.LastModifierId = null;
                    return;
                }
            }

            /* TODO: The code below is from old ABP, not implemented yet
            if (tenantId.HasValue && MultiTenancyHelper.IsHostEntity(entity))
            {
                //Tenant user modified a host entity
                modificationAuditedObject.LastModifierId = null;
                return;
            }
             */

            modificationAuditedObject.LastModifierId = CurrentUser.Id;
        }

        private void SetDeletionTime(object targetObject)
        {
            if (targetObject is IHasDeletionTime objectWithDeletionTime)
            {
                if (objectWithDeletionTime.DeletionTime == null)
                {
                    objectWithDeletionTime.DeletionTime = Clock.Now;
                }
            }
        }

        private void SetDeleterId(object targetObject)
        {
            if (!(targetObject is IDeletionAudited deletionAuditedObject))
            {
                return;
            }

            if (deletionAuditedObject.DeleterId != null)
            {
                return;
            }

            if (!CurrentUser.Id.HasValue)
            {
                deletionAuditedObject.DeleterId = null;
                return;
            }

            if (deletionAuditedObject is IMultiTenant multiTenantEntity)
            {
                if (multiTenantEntity.TenantId != CurrentUser.TenantId)
                {
                    deletionAuditedObject.DeleterId = null;
                    return;
                }
            }

            deletionAuditedObject.DeleterId = CurrentUser.Id;
        }
    }
}
