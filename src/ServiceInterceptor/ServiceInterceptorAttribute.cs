/*  ServiceInterceptor v1.0.0.0 ( https://github.com/randyburden/ServiceInterceptor )

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

    Description:
    
    The ServiceInterceptor is a generic, extendable, and derivable attribute that can be placed on any WCF 
    service or method to gain access to the many hooks/interception points that WCF provides.
*/

using System;
using System.Collections.ObjectModel;
using System.ServiceModel;
using System.ServiceModel.Channels;
using System.ServiceModel.Description;
using System.ServiceModel.Dispatcher;

namespace ServiceInterceptor
{
    /// <summary>
    /// Provides numerous service interceptors/hooks for a WCF service or method.
    /// </summary>
    /// <remarks>
    /// Extensibility:
    /// All methods are overridable in a derived class easily allowing new custom attributes to be created.
    /// 
    /// Features:
    /// If the service endpoint that uses this interceptor implements the IServiceInterceptor interface, then
    /// the interfaces methods will automatically be called before and after each service method call.
    /// </remarks>
    [AttributeUsage( AttributeTargets.Class | AttributeTargets.Method, AllowMultiple = false, Inherited = true )]
    public class ServiceInterceptorAttribute : Attribute, IServiceBehavior, IEndpointBehavior, IOperationBehavior, IParameterInspector
    {
        #region Private Variables

        private readonly Type _iServiceInterceptorType;
        private object _iServiceInterceptorInstance;

        #endregion Private Variables

        #region Constructors

        public ServiceInterceptorAttribute()
        {
        }

        /// <summary>
        /// Initializes with an IServiceInterceptor which will be invoked before and after each
        /// service method call.
        /// </summary>
        /// <param name="iServiceInterceptor">A type that implements IServiceInterceptor</param>
        public ServiceInterceptorAttribute( Type iServiceInterceptor )
        {
            // Enforce the type to implement IServiceInterceptor
            if ( !typeof( IServiceInterceptor ).IsAssignableFrom( iServiceInterceptor ) )
                throw new ArgumentException( string.Format( "Wrong type: {0} must implement IServiceInterceptor", iServiceInterceptor ), "iServiceInterceptor" );

            _iServiceInterceptorType = iServiceInterceptor;
        }

        #endregion Constructors

        #region Implementation of IServiceBehavior

        /// <summary>
        /// Provides the ability to inspect the service host and the service description to confirm that the service can run successfully.
        /// </summary>
        /// <param name="serviceDescription">The service description.</param><param name="serviceHostBase">The service host that is currently being constructed.</param>
        public virtual void Validate( ServiceDescription serviceDescription, ServiceHostBase serviceHostBase )
        {
        }

        /// <summary>
        /// Provides the ability to pass custom data to binding elements to support the contract implementation.
        /// </summary>
        /// <param name="serviceDescription">The service description of the service.</param><param name="serviceHostBase">The host of the service.</param><param name="endpoints">The service endpoints.</param><param name="bindingParameters">Custom objects to which binding elements have access.</param>
        public virtual void AddBindingParameters( ServiceDescription serviceDescription, ServiceHostBase serviceHostBase, Collection<ServiceEndpoint> endpoints, BindingParameterCollection bindingParameters )
        {
        }

        /// <summary>
        /// Provides the ability to change run-time property values or insert custom extension objects such as error handlers, message or parameter interceptors, security extensions, and other custom extension objects.
        /// </summary>
        /// <param name="serviceDescription">The service description.</param><param name="serviceHostBase">The host that is currently being built.</param>
        public virtual void ApplyDispatchBehavior( ServiceDescription serviceDescription, ServiceHostBase serviceHostBase )
        {
            foreach ( var endpoint in serviceDescription.Endpoints )
            {
                endpoint.Behaviors.Add( this );

                foreach ( var operation in endpoint.Contract.Operations )
                {
                    if ( !operation.Behaviors.Contains( GetType() ) )
                    {
                        operation.Behaviors.Add( this );
                    }
                }
            }
        }

        #endregion Implementation of IServiceBehavior

        #region Implementation of IEndpointBehavior

        /// <summary>
        /// Implement to confirm that the endpoint meets some intended criteria.
        /// </summary>
        /// <param name="endpoint">The endpoint to validate.</param>
        public virtual void Validate( ServiceEndpoint endpoint )
        {
        }

        /// <summary>
        /// Implement to pass data at runtime to bindings to support custom behavior.
        /// </summary>
        /// <param name="endpoint">The endpoint to modify.</param><param name="bindingParameters">The objects that binding elements require to support the behavior.</param>
        public virtual void AddBindingParameters( ServiceEndpoint endpoint, BindingParameterCollection bindingParameters )
        {
        }

        /// <summary>
        /// Implements a modification or extension of the service across an endpoint.
        /// </summary>
        /// <param name="endpoint">The endpoint that exposes the contract.</param><param name="endpointDispatcher">The endpoint dispatcher to be modified or extended.</param>
        public virtual void ApplyDispatchBehavior( ServiceEndpoint endpoint, EndpointDispatcher endpointDispatcher )
        {
        }

        /// <summary>
        /// Implements a modification or extension of the client across an endpoint.
        /// </summary>
        /// <param name="endpoint">The endpoint that is to be customized.</param><param name="clientRuntime">The client runtime to be customized.</param>
        public virtual void ApplyClientBehavior( ServiceEndpoint endpoint, ClientRuntime clientRuntime )
        {
        }

        #endregion Implementation of IEndpointBehavior

        #region Implementation of IOperationBehavior

        /// <summary>
        /// Implement to confirm that the operation meets some intended criteria.
        /// </summary>
        /// <param name="operationDescription">The operation being examined. Use for examination only. If the operation description is modified, the results are undefined.</param>
        public virtual void Validate( OperationDescription operationDescription )
        {
        }

        /// <summary>
        /// Implements a modification or extension of the service across an operation.
        /// </summary>
        /// <param name="operationDescription">The operation being examined. Use for examination only. If the operation description is modified, the results are undefined.</param><param name="dispatchOperation">The run-time object that exposes customization properties for the operation described by <paramref name="operationDescription"/>.</param>
        public virtual void ApplyDispatchBehavior( OperationDescription operationDescription, DispatchOperation dispatchOperation )
        {
            dispatchOperation.ParameterInspectors.Add( this );
        }

        /// <summary>
        /// Implements a modification or extension of the client across an operation.
        /// </summary>
        /// <param name="operationDescription">The operation being examined. Use for examination only. If the operation description is modified, the results are undefined.</param><param name="clientOperation">The run-time object that exposes customization properties for the operation described by <paramref name="operationDescription"/>.</param>
        public virtual void ApplyClientBehavior( OperationDescription operationDescription, ClientOperation clientOperation )
        {
        }

        /// <summary>
        /// Implement to pass data at runtime to bindings to support custom behavior.
        /// </summary>
        /// <param name="operationDescription">The operation being examined. Use for examination only. If the operation description is modified, the results are undefined.</param><param name="bindingParameters">The collection of objects that binding elements require to support the behavior.</param>
        public virtual void AddBindingParameters( OperationDescription operationDescription, BindingParameterCollection bindingParameters )
        {
        }

        #endregion Implementation of IOperationBehavior

        #region Implementation of IParameterInspector

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
        public virtual object BeforeCall( string operationName, object[] inputs )
        {
            object obj = null;

            var interceptor = GetServiceInterceptor();

            if ( interceptor != null )
            {
                obj = interceptor.BeforeCall( operationName, inputs );
            }

            return obj;
        }

        /// <summary>
        /// Called after the operation has been invoked, but before the response is to be serialized.
        /// </summary>
        /// <param name="operationName">The name of the invoked operation.</param>
        /// <param name="outputs">Any output objects.</param>
        /// <param name="returnValue">The return value of the operation.</param>
        /// <param name="correlationState">Any correlation state returned from the BeforeCall method, or null.</param>
        public virtual void AfterCall( string operationName, object[] outputs, object returnValue, object correlationState )
        {
            var interceptor = GetServiceInterceptor();

            if ( interceptor != null )
            {
                interceptor.AfterCall( operationName, outputs, returnValue, correlationState );
            }
        }

        #endregion Implementation of IParameterInspector

        #region Public Methods

        /// <summary>
        /// Attempts to get an IServiceInterceptor.
        /// </summary>
        /// <remarks>
        /// This method attempts to locate an IServiceInterceptor in this order:
        /// 1. Return the IServiceInterceptor passed in through the constructor if not null.
        /// 2. Return the current service instance if it implements the IServiceInterceptor interface.
        /// 3. Returns null.
        /// </remarks>
        /// <returns>An IServiceInterceptor</returns>
        public virtual IServiceInterceptor GetServiceInterceptor()
        {
            if ( _iServiceInterceptorInstance != null )
            {
                return _iServiceInterceptorInstance as IServiceInterceptor;
            }

            if ( _iServiceInterceptorType != null )
            {
                var instance = Activator.CreateInstance( _iServiceInterceptorType );

                var serviceInterceptor = instance as IServiceInterceptor;

                if ( serviceInterceptor != null )
                {
                    _iServiceInterceptorInstance = serviceInterceptor;

                    return serviceInterceptor;
                }
            }

            var context = OperationContext.Current;

            if ( context != null )
            {
                var serviceInstance = context.InstanceContext.GetServiceInstance();

                if ( serviceInstance != null )
                {
                    var interceptor = serviceInstance as IServiceInterceptor;

                    if ( interceptor != null )
                    {
                        return interceptor;
                    }
                }
            }

            return null;
        }

        #endregion Public Methods
    }
}