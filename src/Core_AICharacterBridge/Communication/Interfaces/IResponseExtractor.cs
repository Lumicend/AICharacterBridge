namespace AICharacterBridge.Core.Communication.Interfaces
{
    /// <summary>
    /// Interface for extracting message content from raw AI client responses.
    /// </summary>
    public interface IResponseExtractor
    {
        /// <summary>
        /// Gets the name of the extractor.
        /// </summary>
        string GetName();

        /// <summary>
        /// Extracts the message content from the raw response.
        /// </summary>
        string ExtractMessage(string rawResponse);
    }
}