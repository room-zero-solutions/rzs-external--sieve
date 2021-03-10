namespace RzsSieveUnitTests.Abstractions.Entity
{
    public interface IComment: IBaseEntity
    {
        string Text { get; set; }
    }
}
