using System;
using System.Diagnostics;
using System.ServiceModel;
using System.Text;
using NUnit.Framework;

namespace ServiceInterceptor.Tests
{
    [TestFixture]
    public class DerivedAttributeTests
    {
        #region Setup

        private readonly TestTraceListener _listener = new TestTraceListener();

        [TestFixtureSetUp]
        public void TestFixtureSetUp()
        {
            Trace.Listeners.Add( _listener );
        }

        [TestFixtureTearDown]
        public void TestFixtureTearDown()
        {
            Trace.Listeners.Remove( _listener );
        }

        #endregion Setup

        [Test]
        public void The_Service_Should_Still_Be_Callable()
        {
            // Arrange
            var baseAddress = new Uri( "http://localhost/HelloWorldService/" );

            using ( var host = new ServiceHost( typeof( HelloWorldService ), baseAddress ) )
            {
                host.Open();

                var helloWorldService = ChannelFactory<IHelloWorldService>.CreateChannel(
                    new BasicHttpBinding(), new EndpointAddress( baseAddress ) );

                // Act
                var response = helloWorldService.SayHello( "Bob" );

                // Assert
                Assert.That( response == "Hello, Bob" );
            }
        }

        [Test]
        public void The_Derived_Attribute_Should_Have_Written_To_The_Debug_Trace_Listeners()
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

                var loggedOutput = _listener.Cache.ToString();

                // Assert
                Assert.That( loggedOutput.Contains( "HelloWorldService.SayHello was called." ) );
            }
        }

        #region Nested Types

        public class TestTraceListener : TraceListener
        {
            public StringBuilder Cache = new StringBuilder();

            #region Overrides of TraceListener

            public override void Write( string message )
            {
                Cache.Append( message );
            }

            public override void WriteLine( string message )
            {
                Cache.AppendLine( message );
            }

            #endregion Overrides of TraceListener
        }

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

            public override void ApplyDispatchBehavior( System.ServiceModel.Description.ServiceDescription serviceDescription, ServiceHostBase serviceHostBase )
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

        #endregion Nested Types
    }
}