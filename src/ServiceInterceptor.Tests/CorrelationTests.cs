using System;
using System.ServiceModel;
using NUnit.Framework;

namespace ServiceInterceptor.Tests
{
    [TestFixture]
    public class CorrelationTests
    {
        public static object BeforeCallCorrelationState = 444;
        public static object AfterCallCorrelationState;
        
        [Test]
        public void Test_Correlation_State()
        {
            // Arrange
            var baseAddress = new Uri( "http://localhost/HelloWorldService/" );

            using ( var host = new ServiceHost( typeof( HelloWorldService ), baseAddress ) )
            {
                host.Open();

                var helloWorldService = ChannelFactory<IHelloWorldService>.CreateChannel(
                    new BasicHttpBinding(), new EndpointAddress( baseAddress ) );

                // Act
                helloWorldService.SayHello( "Bob" );
                
                // Assert
                Assert.That( AfterCallCorrelationState == BeforeCallCorrelationState );
            }
        }

        #region Nested Types

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
                return BeforeCallCorrelationState;
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
                AfterCallCorrelationState = correlationState;
            }

            #endregion Implementation of IServiceInterceptor
        }

        #endregion Nested Types
    }
}