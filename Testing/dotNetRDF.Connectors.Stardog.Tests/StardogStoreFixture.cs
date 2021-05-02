﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using VDS.RDF.Storage.Management;
using VDS.RDF.Storage.Management.Provisioning.Stardog;
using Xunit;

namespace VDS.RDF.Storage
{
    public class StardogStoreFixture : IDisposable
    {
        public StardogConnector Connector { get; }

        public StardogStoreFixture()
        {
            Skip.IfNot(TestConfigManager.GetSettingAsBoolean(TestConfigManager.UseStardog), "Test Config marks Stardog as unavailable, test cannot be run");
            var connector = new StardogConnector(TestConfigManager.GetSetting(TestConfigManager.StardogServer),
                TestConfigManager.GetSetting(TestConfigManager.StardogDatabase),
                TestConfigManager.GetSetting(TestConfigManager.StardogUser),
                TestConfigManager.GetSetting(TestConfigManager.StardogPassword));
            var testStore = TestConfigManager.GetSetting(TestConfigManager.StardogDatabase);
            if (!connector.ParentServer.ListStores().Contains(testStore))
            {
                connector.ParentServer.CreateStore(new StardogMemTemplate(testStore));
            }

            Connector = connector;
        }

        public StardogServer GetServer()
        {
            Skip.IfNot(TestConfigManager.GetSettingAsBoolean(TestConfigManager.UseStardog), "Test Config marks Stardog as unavailable, test cannot be run");
            return new StardogServer(TestConfigManager.GetSetting(TestConfigManager.StardogServer),
                TestConfigManager.GetSetting(TestConfigManager.StardogUser),
                TestConfigManager.GetSetting(TestConfigManager.StardogPassword));
        }


        public void Dispose()
        {

        }
    }
}
