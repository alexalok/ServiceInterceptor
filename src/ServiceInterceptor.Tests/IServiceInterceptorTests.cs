using System;
using System.Linq;
using System.ServiceModel;
using NUnit.Framework;

namespace ServiceInterceptor.Tests
{
    [TestFixture]
    public class ServiceInterceptorImplementedOnServiceTests
    {
        [Test]
        public void IServiceInterceptor_Methods_On_The_Service_Should_Be_Called()
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
                Assert.True( HelloWorldService.BeforeCallWasCalled );

                Assert.True( HelloWorldService.AfterCallWasCalled );
            }
        }

        [Test]
        public void Throwing_An_Exception_In_BeforeCall_Should_Abort_The_Call_To_The_Intended_Service_Method()
        {
            // Arrange
            var baseAddress = new Uri( "http://localhost/HelloWorldService/" );

            using ( var host = new ServiceHost( typeof( HelloWorldServiceThatThrowsAnException ), baseAddress ) )
            {
                host.Open();

                var helloWorldService = ChannelFactory<IHelloWorldService>.CreateChannel(
                    new BasicHttpBinding(), new EndpointAddress( baseAddress ) );

                // Act
                TestDelegate action = () => helloWorldService.SayHello( "Bob" );

                // Assert
                Assert.Throws<FaultException>( action );

                Assert.False( HelloWorldServiceThatThrowsAnException.SayHelloWasCalled );
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
            public static bool BeforeCallWasCalled;
            public static bool AfterCallWasCalled;

            public HelloWorldService()
            {
                BeforeCallWasCalled = false;
                AfterCallWasCalled = false;
            }

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

        [ServiceInterceptor]
        public class HelloWorldServiceThatThrowsAnException : IHelloWorldService, IServiceInterceptor
        {
            public static bool SayHelloWasCalled;

            public HelloWorldServiceThatThrowsAnException()
            {
                SayHelloWasCalled = false;
            }

            public string SayHello( string name )
            {
                SayHelloWasCalled = true;

                return string.Format( "Hello, {0}", name );
            }

            #region Implementation of IServiceInterceptor

            /// <summary>
            ///     Called after the request has been received and deserialized in a WCF message object,
            ///     but before the operation method has been invoked.
            /// </summary>
            /// <param name="operationName">The name of the operation.</param>
            /// <param name="inputs">The objects passed to the method by the client.</param>
            /// <returns>
            ///     The correlation state that is returned as the correlationState parameter in AfterCall.
            ///     Return null if you do not intend to use correlation state.
            /// </returns>
            public object BeforeCall( string operationName, object[] inputs )
            {
                // If the user didn't send the super duper secret password then throw an exception.
                if ( inputs.Any( x => x.ToString().Contains( "New England Clam Chowder" ) ) == false )
                {
                    throw new InvalidPasswordException();
                }

                return null;
            }

            /// <summary>
            ///     Called after the operation has been invoked, but before the response is to be serialized.
            /// </summary>
            /// <param name="operationName">The name of the invoked operation.</param>
            /// <param name="outputs">Any output objects.</param>
            /// <param name="returnValue">The return value of the operation.</param>
            /// <param name="correlationState">Any correlation state returned from the BeforeCall method, or null.</param>
            public void AfterCall( string operationName, object[] outputs, object returnValue, object correlationState )
            {
            }

            #endregion Implementation of IServiceInterceptor

            public class InvalidPasswordException : FaultException
            {
                public const string ErrorMessage = "Missing/Invalid super duper secret password!";

                public InvalidPasswordException()
                    : base( ErrorMessage )
                {
                }
            }
        }

        #endregion Nested Types
    }
}