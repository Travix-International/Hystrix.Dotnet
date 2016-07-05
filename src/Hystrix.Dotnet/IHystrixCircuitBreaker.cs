namespace Hystrix.Dotnet
{
    public interface IHystrixCircuitBreaker
    {
        /// <summary>
        /// Indicates whether the circuit is open or not, without affecting the state
        /// </summary>
        bool CircuitIsOpen { get; }

        /// <summary>
        /// Checks whether a request is allowed and changes state of the circuit breaker if there's a reason to do so
        /// </summary>
        /// <returns></returns>
        bool AllowRequest();

        /// <summary>
        /// Opens the circuit
        /// </summary>
        void OpenCircuit();

        /// <summary>
        /// Closes the circuit
        /// </summary>
        void CloseCircuit();
    }
}