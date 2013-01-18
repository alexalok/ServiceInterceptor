ServiceInterceptor
==================

###Overview###

The ServiceInterceptor is a generic, extendable, and derivable attribute that 
can be placed on any WCF service or method to gain access to the many 
hooks/interception points that WCF provides.

In a nutshell, it is a wrapper around the various integration points built
into WCF ( The .NET Windows Communication Foundation framework for building 
service-oriented applications ).

Its primary usage is to be able to easily decorate a web service class with
the ServiceInterceptor attribute and inherit from the IServiceInterceptor
interface to gain access to the `BeforeCall()` and `AfterCall()` interception
methods but there are many other ways to use this handy attribute.

###Usage###

####IServiceInterceptor####

The following example uses the ServiceInterceptor attribute in conjunction with
the IServiceInterceptor interface to gain access to the before and after method
call hooks.

```csharp
[ServiceContract]
public interface IHelloWorldService
{
	[OperationContract]
	string SayHello( string name );
}

[ServiceInterceptor]
public class HelloWorldService : IHelloWorldService, IServiceInterceptor
{
	public string SayHello( string name )
	{
		return string.Format( "Hello, {0}", name );
	}

	#region Implementation of IServiceInterceptor

	/// <summary>
	/// Called after the request has been received and deserialized in a WCF message object,
	/// but before the operation method has been invoked.
	/// </summary>
	/// <param name="operationName">The name of the operation.</param>
	/// <param name="inputs">The objects passed to the method by the client.</param>
	/// <returns>
	/// The correlation state that is returned as the correlationState parameter in AfterCall. 
	/// Return null if you do not intend to use correlation state.
	/// </returns>
	public object BeforeCall( string operationName, object[] inputs )
	{
		// Do whatever you want before the service method has been called.
		
		// Examples: Log method calls, perform validation on the inputs, perform basic
		//           authentication/authorization, initialize a unit-of-work, etc.
		
		// You could throw a FaultException to abort the call to the service method
		// or you could modify the inputs to the service method, etc.
		
		// You can return an object which will be passed as the correlationState parameter
		// to the AfterCall() method or simply return null.
		return null;
	}

	/// <summary>
	/// Called after the operation has been invoked, but before the response is to be serialized.
	/// </summary>
	/// <param name="operationName">The name of the invoked operation.</param>
	/// <param name="outputs">Any output objects.</param>
	/// <param name="returnValue">The return value of the operation.</param>
	/// <param name="correlationState">Any correlation state returned from the BeforeCall method, or null.</param>
	public void AfterCall( string operationName, object[] outputs, object returnValue, object correlationState )
	{
		// Do whatever you want after the service method has been called.
	}

	#endregion Implementation of IServiceInterceptor
}
```

####Creating Custom Attributes####

The following example demonstrates how easy it is to create a custom attribute
by deriving from the ServiceInterceptorAttribute and overriding two of its methods
in order to implement a basic logging attribute that can be placed on the service
itself or on any method that needs the desired behavior.

```csharp

[ServiceContract]
public interface IHelloWorldService
{
	[OperationContract]
	string SayHello( string name );
}

[LogCallsToDebugger]
public class HelloWorldService : IHelloWorldService
{
	public string SayHello( string name )
	{
		return string.Format( "Hello, {0}", name );
	}
}

/// <summary>
/// Example of a custom interceptor that logs calls to the debug trace listeners.
/// </summary>
public class LogCallsToDebugger : ServiceInterceptorAttribute
{
	private Type _serviceType;

	public override void ApplyDispatchBehavior( ServiceDescription serviceDescription, ServiceHostBase serviceHostBase )
	{
		_serviceType = serviceDescription.ServiceType;

		base.ApplyDispatchBehavior( serviceDescription, serviceHostBase );
	}

	public override object BeforeCall( string operationName, object[] inputs )
	{
		string message = string.Format( "{0}.{1} was called.", _serviceType.Name, operationName );

		Debug.WriteLine( message );

		return base.BeforeCall( operationName, inputs );
	}
}

```


Please see the test project for more usage examples.

###License###

ServiceInterceptor v1.0.0.0 ( https://github.com/randyburden/ServiceInterceptor )

MIT License:

Copyright (c) 2013, Randy Burden ( http://randyburden.com )
All rights reserved.

Permission is hereby granted, free of charge, to any person obtaining a copy of this software and 
associated documentation files (the "Software"), to deal in the Software without restriction, including 
without limitation the rights to use, copy, modify, merge, publish, distribute, sublicense, and/or sell 
copies of the Software, and to permit persons to whom the Software is furnished to do so, subject to the 
following conditions:

The above copyright notice and this permission notice shall be included in all copies or substantial 
portions of the Software.

THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR IMPLIED, INCLUDING BUT NOT 
LIMITED TO THE WARRANTIES OF MERCHANTABILITY, FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN 
NO EVENT SHALL THE AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER LIABILITY, 
WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM, OUT OF OR IN CONNECTION WITH THE 
SOFTWARE OR THE USE OR OTHER DEALINGS IN THE SOFTWARE. 