<?xml version="1.0" encoding="utf-8"?>
<serviceModel xmlns:xsi="http://www.w3.org/2001/XMLSchema-instance" xmlns:xsd="http://www.w3.org/2001/XMLSchema" name="KrolWebCrawler" generation="1" functional="0" release="0" Id="2180516b-b021-408b-9014-7fd877f52fc5" dslVersion="1.2.0.0" xmlns="http://schemas.microsoft.com/dsltools/RDSM">
  <groups>
    <group name="KrolWebCrawlerGroup" generation="1" functional="0" release="0">
      <componentports>
        <inPort name="CrawlerWebRole:Endpoint1" protocol="http">
          <inToChannel>
            <lBChannelMoniker name="/KrolWebCrawler/KrolWebCrawlerGroup/LB:CrawlerWebRole:Endpoint1" />
          </inToChannel>
        </inPort>
      </componentports>
      <settings>
        <aCS name="CrawlerWebRole:Microsoft.WindowsAzure.Plugins.Diagnostics.ConnectionString" defaultValue="">
          <maps>
            <mapMoniker name="/KrolWebCrawler/KrolWebCrawlerGroup/MapCrawlerWebRole:Microsoft.WindowsAzure.Plugins.Diagnostics.ConnectionString" />
          </maps>
        </aCS>
        <aCS name="CrawlerWebRoleInstances" defaultValue="[1,1,1]">
          <maps>
            <mapMoniker name="/KrolWebCrawler/KrolWebCrawlerGroup/MapCrawlerWebRoleInstances" />
          </maps>
        </aCS>
        <aCS name="CrawlerWorkerRole:Microsoft.WindowsAzure.Plugins.Diagnostics.ConnectionString" defaultValue="">
          <maps>
            <mapMoniker name="/KrolWebCrawler/KrolWebCrawlerGroup/MapCrawlerWorkerRole:Microsoft.WindowsAzure.Plugins.Diagnostics.ConnectionString" />
          </maps>
        </aCS>
        <aCS name="CrawlerWorkerRoleInstances" defaultValue="[1,1,1]">
          <maps>
            <mapMoniker name="/KrolWebCrawler/KrolWebCrawlerGroup/MapCrawlerWorkerRoleInstances" />
          </maps>
        </aCS>
      </settings>
      <channels>
        <lBChannel name="LB:CrawlerWebRole:Endpoint1">
          <toPorts>
            <inPortMoniker name="/KrolWebCrawler/KrolWebCrawlerGroup/CrawlerWebRole/Endpoint1" />
          </toPorts>
        </lBChannel>
      </channels>
      <maps>
        <map name="MapCrawlerWebRole:Microsoft.WindowsAzure.Plugins.Diagnostics.ConnectionString" kind="Identity">
          <setting>
            <aCSMoniker name="/KrolWebCrawler/KrolWebCrawlerGroup/CrawlerWebRole/Microsoft.WindowsAzure.Plugins.Diagnostics.ConnectionString" />
          </setting>
        </map>
        <map name="MapCrawlerWebRoleInstances" kind="Identity">
          <setting>
            <sCSPolicyIDMoniker name="/KrolWebCrawler/KrolWebCrawlerGroup/CrawlerWebRoleInstances" />
          </setting>
        </map>
        <map name="MapCrawlerWorkerRole:Microsoft.WindowsAzure.Plugins.Diagnostics.ConnectionString" kind="Identity">
          <setting>
            <aCSMoniker name="/KrolWebCrawler/KrolWebCrawlerGroup/CrawlerWorkerRole/Microsoft.WindowsAzure.Plugins.Diagnostics.ConnectionString" />
          </setting>
        </map>
        <map name="MapCrawlerWorkerRoleInstances" kind="Identity">
          <setting>
            <sCSPolicyIDMoniker name="/KrolWebCrawler/KrolWebCrawlerGroup/CrawlerWorkerRoleInstances" />
          </setting>
        </map>
      </maps>
      <components>
        <groupHascomponents>
          <role name="CrawlerWebRole" generation="1" functional="0" release="0" software="C:\Users\rhenvar\Source\Repos\KrolWebCrawler\KrolWebCrawler\csx\Release\roles\CrawlerWebRole" entryPoint="base\x64\WaHostBootstrapper.exe" parameters="base\x64\WaIISHost.exe " memIndex="-1" hostingEnvironment="frontendadmin" hostingEnvironmentVersion="2">
            <componentports>
              <inPort name="Endpoint1" protocol="http" portRanges="80" />
            </componentports>
            <settings>
              <aCS name="Microsoft.WindowsAzure.Plugins.Diagnostics.ConnectionString" defaultValue="" />
              <aCS name="__ModelData" defaultValue="&lt;m role=&quot;CrawlerWebRole&quot; xmlns=&quot;urn:azure:m:v1&quot;&gt;&lt;r name=&quot;CrawlerWebRole&quot;&gt;&lt;e name=&quot;Endpoint1&quot; /&gt;&lt;/r&gt;&lt;r name=&quot;CrawlerWorkerRole&quot; /&gt;&lt;/m&gt;" />
            </settings>
            <resourcereferences>
              <resourceReference name="DiagnosticStore" defaultAmount="[4096,4096,4096]" defaultSticky="true" kind="Directory" />
              <resourceReference name="EventStore" defaultAmount="[1000,1000,1000]" defaultSticky="false" kind="LogStore" />
            </resourcereferences>
          </role>
          <sCSPolicy>
            <sCSPolicyIDMoniker name="/KrolWebCrawler/KrolWebCrawlerGroup/CrawlerWebRoleInstances" />
            <sCSPolicyUpdateDomainMoniker name="/KrolWebCrawler/KrolWebCrawlerGroup/CrawlerWebRoleUpgradeDomains" />
            <sCSPolicyFaultDomainMoniker name="/KrolWebCrawler/KrolWebCrawlerGroup/CrawlerWebRoleFaultDomains" />
          </sCSPolicy>
        </groupHascomponents>
        <groupHascomponents>
          <role name="CrawlerWorkerRole" generation="1" functional="0" release="0" software="C:\Users\rhenvar\Source\Repos\KrolWebCrawler\KrolWebCrawler\csx\Release\roles\CrawlerWorkerRole" entryPoint="base\x64\WaHostBootstrapper.exe" parameters="base\x64\WaWorkerHost.exe " memIndex="-1" hostingEnvironment="consoleroleadmin" hostingEnvironmentVersion="2">
            <settings>
              <aCS name="Microsoft.WindowsAzure.Plugins.Diagnostics.ConnectionString" defaultValue="" />
              <aCS name="__ModelData" defaultValue="&lt;m role=&quot;CrawlerWorkerRole&quot; xmlns=&quot;urn:azure:m:v1&quot;&gt;&lt;r name=&quot;CrawlerWebRole&quot;&gt;&lt;e name=&quot;Endpoint1&quot; /&gt;&lt;/r&gt;&lt;r name=&quot;CrawlerWorkerRole&quot; /&gt;&lt;/m&gt;" />
            </settings>
            <resourcereferences>
              <resourceReference name="DiagnosticStore" defaultAmount="[4096,4096,4096]" defaultSticky="true" kind="Directory" />
              <resourceReference name="EventStore" defaultAmount="[1000,1000,1000]" defaultSticky="false" kind="LogStore" />
            </resourcereferences>
          </role>
          <sCSPolicy>
            <sCSPolicyIDMoniker name="/KrolWebCrawler/KrolWebCrawlerGroup/CrawlerWorkerRoleInstances" />
            <sCSPolicyUpdateDomainMoniker name="/KrolWebCrawler/KrolWebCrawlerGroup/CrawlerWorkerRoleUpgradeDomains" />
            <sCSPolicyFaultDomainMoniker name="/KrolWebCrawler/KrolWebCrawlerGroup/CrawlerWorkerRoleFaultDomains" />
          </sCSPolicy>
        </groupHascomponents>
      </components>
      <sCSPolicy>
        <sCSPolicyUpdateDomain name="CrawlerWebRoleUpgradeDomains" defaultPolicy="[5,5,5]" />
        <sCSPolicyUpdateDomain name="CrawlerWorkerRoleUpgradeDomains" defaultPolicy="[5,5,5]" />
        <sCSPolicyFaultDomain name="CrawlerWebRoleFaultDomains" defaultPolicy="[2,2,2]" />
        <sCSPolicyFaultDomain name="CrawlerWorkerRoleFaultDomains" defaultPolicy="[2,2,2]" />
        <sCSPolicyID name="CrawlerWebRoleInstances" defaultPolicy="[1,1,1]" />
        <sCSPolicyID name="CrawlerWorkerRoleInstances" defaultPolicy="[1,1,1]" />
      </sCSPolicy>
    </group>
  </groups>
  <implements>
    <implementation Id="13250c35-de05-49be-ba4f-33195a93199f" ref="Microsoft.RedDog.Contract\ServiceContract\KrolWebCrawlerContract@ServiceDefinition">
      <interfacereferences>
        <interfaceReference Id="3a517dbf-814f-4b0b-a55c-29dda43b4fb1" ref="Microsoft.RedDog.Contract\Interface\CrawlerWebRole:Endpoint1@ServiceDefinition">
          <inPort>
            <inPortMoniker name="/KrolWebCrawler/KrolWebCrawlerGroup/CrawlerWebRole:Endpoint1" />
          </inPort>
        </interfaceReference>
      </interfacereferences>
    </implementation>
  </implements>
</serviceModel>