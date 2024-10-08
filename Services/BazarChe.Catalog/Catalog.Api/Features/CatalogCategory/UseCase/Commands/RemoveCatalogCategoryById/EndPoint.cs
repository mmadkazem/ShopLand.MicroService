namespace Catalog.Api.Features.CatalogCategory.UseCase.Commands.RemoveCatalogCategoryById;

public sealed class EndPoint : ICarterModule
{
    public void AddRoutes(IEndpointRouteBuilder app)
    {
        app.MapGroup(FeatureManger.Prefix)
            .WithTags(FeatureManger.EndPointTagName)
            .MapDelete("{id:int:required}", async (ISender _sender, int id, CancellationToken token) =>
            {
                return await _sender.Send(new RemoveCatalogCategoryByIdCommandRequest(id), token);
            });
    }
}