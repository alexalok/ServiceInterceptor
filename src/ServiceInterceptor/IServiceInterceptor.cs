namespace ServiceInterceptor
{
    /// <summary>
    /// When used in conjunction with the <see cref="ServiceInterceptorAttribute"/>, this interfaces methods
    /// will be called before and after each service method call.
    /// </summary>
    public interface IServiceInterceptor
    {
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
        object BeforeCall( string operationName, object[] inputs );

        /// <summary>
        /// Called after the operation has been invoked, but before the response is to be serialized.
        /// </summary>
        /// <param name="operationName">The name of the invoked operation.</param>
        /// <param name="outputs">Any output objects.</param>
        /// <param name="returnValue">The return value of the operation.</param>
        /// <param name="correlationState">Any correlation state returned from the BeforeCall method, or null.</param>
        void AfterCall( string operationName, object[] outputs, object returnValue, object correlationState );
    }
}