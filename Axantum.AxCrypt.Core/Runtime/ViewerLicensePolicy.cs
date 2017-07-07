﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Axantum.AxCrypt.Core.Runtime
{
    public class ViewerLicensePolicy : LicensePolicy
    {
        protected static readonly HashSet<LicenseCapability> ViewerCapabilitySet = new HashSet<LicenseCapability>(new LicenseCapability[]
        {
                    LicenseCapability.StandardEncryption,
                    LicenseCapability.AccountKeyBackup,
                    LicenseCapability.CommunitySupport,
        });

        protected override LicenseCapabilities FreeCapabilities
        {
            get
            {
                return new LicenseCapabilities(ViewerCapabilitySet);
            }
        }
    }
}