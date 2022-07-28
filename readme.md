
## Overview
Calling web services by WS-Trust using `WsFederationBinding` or `Ws2007FederationBinding`
each time requested security token from security token service although the issued token is still valid.

This library allow cache security token in security token cache to prevent unnecessary calls to security token service.

By default `appliesTo` value in request security token request is equals webservice actual address. 
In this case if the address may change, is better to use a unique web service identifier.

This library allows you to set a `appliesTo` calling a security token service.

## .NET Support
- .NET Framework 4.0 + WIF3.5 (Microsoft.IdentityModel)
- .NET Framework 4.5

## Configure security token caching
### Configure by ChannelFactory
The following line of code registers memory SecurityTokenCache.

```C#
var factory = new ChannelFactory<IHello>("Hello")
factory.ConfigureChannelFactory(new MemPrincipalSecurityTokenCache(), new Uri(audienceURI))
```

## How to specify `appliesTo` calling a security token service
### Add extra configuration to binding
You must add an `appliesTo` element to the `tokenRequestParameters` element in the configuration file.

**Be careful - the element must have a namespace that corresponds to the binding type.**

```xml
<binding name="ws2007FederationNoSct">
  <security mode="TransportWithMessageCredential">
    <message establishSecurityContext="false">
      ...
      <tokenRequestParameters>
        <wsp:AppliesTo xmlns:wsp="http://schemas.xmlsoap.org/ws/2004/09/policy">
          <EndpointReference xmlns="http://www.w3.org/2005/08/addressing">
            <Address>https://realm</Address>
          </EndpointReference>
        </wsp:AppliesTo>
      </tokenRequestParameters>
    </message>
  </security>
</binding>
```

### Add endpointBehaviour to client endpoint

```xml
  <system.serviceModel>
    <extensions>
      <behaviorExtensions>
        <add name="appliesTo" type="Abc.IdentityModel.Configuration.ClientAppliesToElement, Abc.IdentityModel" />
      </behaviorExtensions>
    </extensions>
    <client>
      <endpoint address="https://host/Service/wsFederationNoSct"
          binding="ws2007FederationHttpBinding" bindingConfiguration="ws2007FederationNoSct"
          contract="*" name="Service" behaviorConfiguration="client" />
    </client>
    <behaviors>
      <endpointBehaviors>
        <behavior name="client">
          <appliesTo address="https://realm"/>
        </behavior>
      </endpointBehaviors>
     </behaviors>
  </system.serviceModel>
```

### Configure by ChannelFactory
```C#
var factory = new ChannelFactory<IHello>("Hello")
factory.ConfigureChannelFactory(null, new Uri("https://realm"))
```