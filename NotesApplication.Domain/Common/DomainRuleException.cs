namespace NotesApplication.Domain.Common;

public class DomainRuleException(string userMessage) : Exception(userMessage);
