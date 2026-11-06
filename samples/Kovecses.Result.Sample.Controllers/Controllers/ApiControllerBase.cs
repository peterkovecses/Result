namespace Kovecses.Result.Sample.Controllers.Controllers;

public abstract class ApiControllerBase(IMediator mediator) : ControllerBase
{
    protected IMediator Mediator { get; } = mediator;
}
