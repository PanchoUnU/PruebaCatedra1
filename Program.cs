using System.Reflection.Metadata.Ecma335;
using ebooks_dotnet8_api;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
var builder = WebApplication.CreateBuilder(args);
builder.Services.AddDbContext<DataContext>(opt => opt.UseInMemoryDatabase("ebooks"));
builder.Services.AddDatabaseDeveloperPageExceptionFilter();
var app = builder.Build();

var ebooks = app.MapGroup("api/ebook");

// TODO: Add more routes
ebooks.MapPost("/",CreateEBookAsync);
ebooks.MapGet("/",GetLibros);
ebooks.MapPut("/{id}",PutLibro );
ebooks.MapPut("/{id}/change-availability",PutLibroDisponibilidad);
ebooks.MapPut("/{id}/increment-stock", PutLibroStock);
ebooks.MapPost("/purchase",PostCompraLibro);
ebooks.MapDelete("/{id}",DeleteLibro);

app.Run();

// TODO: Add more methods
async Task<IResult> CreateEBookAsync(EBook eBook,DataContext context)
{
    var Book = context.EBooks.Where(u=> u.Author == eBook.Author || u.Title ==eBook.Title).First();
    if(Book is not null)
    {
        return Results.BadRequest("El libro ya existe.");
    }
    eBook.Stock = 0;
    eBook.IsAvailable = true;
    context.EBooks.Add(eBook);
    await context.SaveChangesAsync();
    return Results.Created();
    
}
IResult GetLibros([FromQuery]string autor,[FromQuery]string Format, [FromQuery] string Genre , DataContext context)
{
    if (autor == null && Format == null && Genre== "")
    {
        var libros = context.EBooks.Where(u => u.IsAvailable == true).ToList();
        return Results.Ok(libros);
    }
    if (autor != "")
    {
        var libros = context.EBooks.Where(u => u.IsAvailable == true && u.Author == autor).ToListAsync();
        return Results.Ok(libros);

    }
    if (Format != null)
    {
        var libros = context.EBooks.Where(u => u.IsAvailable == true && u.Format ==Format).ToListAsync();
        return Results.Ok(libros);
    }
    if (Genre == null)
    {
        var libros = context.EBooks.Where(u => u.IsAvailable == true && u.Genre == Genre).ToListAsync();
        return Results.Ok(libros);
    }
    return Results.NotFound("No se encontraron libros.");
}
async Task<IResult> PutLibro(DataContext context, string Author,string Format,string Genre,int Price,string Title,int id)
{
    var libro = context.EBooks.Find(id);
    if(libro is null)
    {
        return Results.BadRequest("No se encontro el libro.");
    }
    libro.Author = Author;
    libro.Format = Format;
    libro.Genre = Genre;
    libro.Price = Price;
    libro.Title = Title;
    await context.SaveChangesAsync();
    return Results.NoContent();
}
async Task<IResult> PutLibroDisponibilidad(DataContext context, int id)
{
    var libro = context.EBooks.Find(id);
    if(libro is null)
    {
        return Results.BadRequest("No se encontro el libro.");
    }
    libro.IsAvailable = !libro.IsAvailable;

    await context.SaveChangesAsync();
    return Results.NoContent();

}
async Task<IResult> PutLibroStock(DataContext context, int id,[FromQuery] int stock)
{
    var libro = context.EBooks.Find(id);
    if(libro is null)
    {
        return Results.BadRequest("No se encontro el libro.");
    }
    if(stock >0)
    {
        libro.Stock = stock;
        await context.SaveChangesAsync();
        return Results.NoContent();
    }
    return Results.BadRequest("El stock es menor a 0");
    
}
async Task<IResult> PostCompraLibro(DataContext context, [FromQuery]int paga, [FromQuery]int cantidad, int id)
{
    var libro = context.EBooks.Find(id);
    if(libro is null)
    {
        return Results.NotFound("El libro no fue encontrado.");
    }
    if(paga <= 0 || cantidad <=0)
    {
        return Results.BadRequest("No se pudo realizar la compra.");

    }
    if(libro.Stock< cantidad || libro.Price*cantidad> paga)
    {
        return Results.BadRequest("la cantidad o la paga no son apropiados.");
    }
    libro.Stock -= cantidad;
    await context.SaveChangesAsync();
    return Results.NoContent();

}
async Task<IResult> DeleteLibro(DataContext context, int id)
{
    var libro = context.EBooks.Find(id);
    if(libro is null)
    {
        return Results.NotFound("El libro no fue encontrado.");
    }
    context.Remove(libro);
    await context.SaveChangesAsync();
    return Results.NoContent();
}