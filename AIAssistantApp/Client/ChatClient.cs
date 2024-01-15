using AIAssistantApp.Model;
using Microsoft.JSInterop;
using System.Text.Json;

namespace AIAssistantApp.Client;

public class ChatClient
{
    private readonly HttpClient _httpClient;
    private readonly IJSRuntime _jsRuntime;

    public ChatClient(HttpClient httpClient, IJSRuntime jsRuntime)
    {
        _httpClient = httpClient;
        _jsRuntime = jsRuntime;
    }

    public async Task<string> SendMessageToBackendAsync(string sessionId, string message)
    {
        var chatRequest = new ChatRequest { SessionId = sessionId, Message = message };
        try
        {
            await LogAsync($"Sending message to backend: {message}");
            var response = await _httpClient.PostAsJsonAsync("chat/process-message", chatRequest);

            if (response.IsSuccessStatusCode)
            {
                var chatResponse = await response.Content.ReadFromJsonAsync<ChatResponse>();
                await LogAsync($"Received response: {JsonSerializer.Serialize(chatResponse)}");
                return chatResponse.Message;
            }
            else
            {
                await LogAsync($"Failed to send message. Status code: {response.StatusCode}");
                return "Sorry, something went wrong.";
            }
        }
        catch (HttpRequestException ex)
        {
            await LogAsync($"HTTP request exception: {ex.Message}");
            return "A communication error occurred.";
        }
        catch (Exception ex)
        {
            await LogAsync($"Exception: {ex.Message}");
            return "An unexpected error occurred.";
        }
    }

    private async Task LogAsync(string message)
    {
        // This method logs to the browser console.
        await _jsRuntime.InvokeVoidAsync("console.log", message);
    }
}