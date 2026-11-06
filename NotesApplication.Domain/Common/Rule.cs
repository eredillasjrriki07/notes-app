namespace NotesApplication.Domain.Common;

public static class Rule
{
    public static void Require(bool condition, string userMessage)
    {
        if (!condition) throw new DomainRuleException(userMessage);
    }
}