namespace NTokenizers.ShowCase.ToHtml.AiChat;

public record RenderRequestDto(string Content);
public record ChatMessageDto(string Role, string Content);
public record ChatRequestDto(List<ChatMessageDto> Messages);
