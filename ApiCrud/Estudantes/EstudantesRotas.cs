using ApiCrud.Data;
using Microsoft.EntityFrameworkCore;

namespace ApiCrud.Estudantes;

public static class EstudantesRotas
{
    public static void AddRotasEstudantes(this WebApplication app)
    {
        var rotasEstudantes = app.MapGroup("estudantes");
        //app.MapGet("estudantes", () => new Estudante("João Vítor"));
        
        // ============>>>>>  Criar Post
        rotasEstudantes.MapPost("", async (AddEstudanteRequest request, AppDbContext context, CancellationToken ct) =>
        {
            var jaExiste = await context.Estudantes.AnyAsync(estudante => estudante.Nome == request.Nome, ct);
            
            if (jaExiste)
                return Results.Conflict("Já existe um estudante.");
            var novoEstudante = new Estudante(request.Nome);
            
            await context.Estudantes.AddAsync(novoEstudante, ct);
            await context.SaveChangesAsync(ct);
            
            var estudanteRetorno = new EstudanteDto(novoEstudante.Id, novoEstudante.Nome);
            
            return Results.Ok(estudanteRetorno);
        });
        
        // ============>>>>>  Retornar todos os estudantes cadastrados
        rotasEstudantes.MapGet("", async (AppDbContext context, CancellationToken ct) =>
        {
            //Guid estudanteId = Guid.Parse( "b012c217-47a8-49fa-8228-3ceb4b1fb207" );

            var estudantes = await context
            .Estudantes
            //.Where(estudante => estudante.Id == estudanteId )
            .Where(estudante => estudante.Ativo)
            .Select(estudante => new EstudanteDto(estudante.Id, estudante.Nome))
            .ToListAsync(ct);

            return estudantes;
        });
        
        // ============>>>>>  Atualizar nomes estudante
        rotasEstudantes.MapPut("{id:guid}", async (Guid id,updateEstudante request, AppDbContext context, CancellationToken ct) =>
        {
            var estudante = await context.Estudantes.SingleOrDefaultAsync(estudante => estudante.Id == id, ct);

            if (estudante == null)
            {
                return Results.NotFound();
            }
            
            estudante.AtualizarNome(request.Nome);
            await context.SaveChangesAsync(ct);
            
            return Results.Ok(new EstudanteDto(estudante.Id, estudante.Nome));
        });
        
        // ============>>>>>  Deletar nomes estudante (soft delete)
        rotasEstudantes.MapDelete("{id:guid}", async (Guid id, AppDbContext context, CancellationToken ct) =>
        {
            var estudante = await context.Estudantes
                .SingleOrDefaultAsync(estudante => estudante.Id == id, ct);
            
            if(estudante == null)
                return Results.NotFound();
            
            estudante.Desativar();
            
            await context.SaveChangesAsync(ct);
            return Results.Ok(estudante.Ativo);
        });
        
        // ============>>>>>  Deletar nomes estudante (real delete)
        rotasEstudantes.MapDelete("realmenteApagarEstudante{id:guid}",
            async (Guid id, AppDbContext context, CancellationToken ct) =>
            {
                var estudante = await context.Estudantes
                    .SingleOrDefaultAsync(estudante => estudante.Id == id, ct);
                
                if(estudante == null)
                    return Results.NotFound();
                
                context.Estudantes.Remove(estudante);
                await context.SaveChangesAsync(ct);

                return Results.Ok($"Estudante {estudante.Nome} foi removido.");
            });
    }
}