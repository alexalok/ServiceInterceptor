using System;
using System.ServiceModel;
using System.Text;
using NUnit.Framework;

namespace ServiceInterceptor.Tests
{
    [TestFixture]
    public class MethodLevelAttributeTests
    {
        [Test]
        public void No_Logging_Should_Be_Performed_For_The_Method_That_Is_Not_Decorated_With_The_Attribute()
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
                helloWorldService.SayHelloAgain( "Bob" );
                helloWorldService.SayGoodbye( "Bob" );

                string log = ServiceMethodLogger.LoggerCache.ToString();

                // Assert
                Assert.That( log.Contains( "SayHello was called." ) );

                Assert.False( log.Contains( "SayHelloAgain was called." ) );

                Assert.That( log.Contains( "SayGoodbye was called." ) );
            }
        }

        #region Nested Types

        [ServiceContract]
        public interface IHelloWorldService
        {
            [OperationContract]
            string SayHello( string name );

            [OperationContract]
            string SayHelloAgain( string name );

            [OperationContract]
            string SayGoodbye( string name );
        }

        /// <summary>
        /// Example of a custom interceptor that logs calls to service methods.
        /// </summary>
        public class ServiceMethodLogger : ServiceInterceptorAttribute
        {
            public static StringBuilder LoggerCache = new StringBuilder();

            public override object BeforeCall( string operationName, object[] inputs )
            {
                string message = string.Format( "{0} was called.", operationName );

                LoggerCache.AppendLine( message );

                return base.BeforeCall( operationName, inputs );
            }
        }
        
        public class HelloWorldService : IHelloWorldService
        {
            [ServiceMethodLogger]
            public string SayHello( string name )
            {
                return string.Format( "Hello, {0}", name );
            }

            public string SayHelloAgain( string name )
            {
                return string.Format( "Hello again, {0}", name );
            }

            [ServiceMethodLogger]
            public string SayGoodbye( string name )
            {
                return string.Format( "Goodbye, {0}", name );
            }
        }

        #endregion Nested Types
    }
}