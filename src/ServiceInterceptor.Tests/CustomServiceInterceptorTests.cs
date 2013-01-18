using System;
using System.ServiceModel;
using NUnit.Framework;

namespace ServiceInterceptor.Tests
{
    [TestFixture]
    public class CustomServiceInterceptorTests
    {
        [Test]
        public void The_CustomServiceInterceptor_Methods_Should_Be_Called()
        {
            // Arrange
            var baseAddress = new Uri( "http://localhost/HelloWorldService/" );

            using ( var host = new ServiceHost( typeof( HelloWorldServiceWithCustomServiceInterceptor ), baseAddress ) )
            {
                host.Open();

                var helloWorldService = ChannelFactory<IHelloWorldService>.CreateChannel(
                    new BasicHttpBinding(), new EndpointAddress( baseAddress ) );

                // Act
                helloWorldService.SayHello( "Bob" );

                // Assert
                Assert.True( CustomServiceInterceptor.BeforeCallWasCalled );

                Assert.True( CustomServiceInterceptor.AfterCallWasCalled );
            }
        }

        #region Nested Types

        [ServiceContract]
        public interface IHelloWorldService
        {
            [OperationContract]
            string SayHello( string name );
        }

        [ServiceInterceptor( typeof( CustomServiceInterceptor ) )]
        public class HelloWorldServiceWithCustomServiceInterceptor : IHelloWorldService
        {
            public string SayHello( string name )
            {
                return string.Format( "Hello, {0}", name );
            }
        }

        public class CustomServiceInterceptor : IServiceInterceptor
        {
            public static bool BeforeCallWasCalled;
            public static bool AfterCallWasCalled;

            public CustomServiceInterceptor()
            {
                BeforeCallWasCalled = false;
                AfterCallWasCalled = false;
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
                BeforeCallWasCalled = true;

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
                AfterCallWasCalled = true;
            }

            #endregion Implementation of IServiceInterceptor
        }

        #endregion Nested Types
    }
}