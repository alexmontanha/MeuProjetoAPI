# Entity Framework Core

O Entity Framework Core é um ORM (Object-Relational Mapping) que permite aos desenvolvedores interagir com bancos de dados relacionais usando objetos e consultas LINQ. Ele é uma ferramenta poderosa para simplificar o acesso a dados em aplicações .NET.

## Passo a Passo para Incluir o Entity Framework no Projeto C# Web API

### 1. **Criar o projeto Web API**
Se você ainda não possui um projeto Web API, crie um utilizando o CLI do .NET:

```bash
dotnet new webapi -n MeuProjetoAPI
cd MeuProjetoAPI
```

### 2. **Instalar o pacote do Entity Framework Core**
Agora, é necessário instalar o pacote do Entity Framework Core. Esse é o ORM que irá facilitar a interação com o banco de dados.

```bash
dotnet add package Microsoft.EntityFrameworkCore
```

### 3. **Instalar o provedor de banco de dados**
O Entity Framework Core é independente de banco de dados, então você precisa instalar o provedor específico para o banco de dados que vai utilizar. Por exemplo, para usar o SQL Server, instale o seguinte pacote:

```bash
dotnet add package Microsoft.EntityFrameworkCore.SqlServer
```

Se você estiver usando outro banco de dados, os pacotes seriam diferentes. Por exemplo:

- **SQLite**: `dotnet add package Microsoft.EntityFrameworkCore.Sqlite`
- **MySQL**: `dotnet add package Pomelo.EntityFrameworkCore.MySql`

### 4. **Instalar as ferramentas do Entity Framework Core CLI**
Para usar os comandos de migração e outros recursos do EF CLI, instale o pacote de ferramentas:

```bash
dotnet add package Microsoft.EntityFrameworkCore.Tools
```

### 5. **Criar o modelo de dados (Entities)**
Agora, crie uma pasta `Models` no projeto e adicione uma classe que representará sua entidade de banco de dados. Por exemplo, um modelo `Produto`:

```csharp
// Models/Produto.cs
namespace MeuProjetoAPI.Models
{
    public class Produto
    {
        public int Id { get; set; }
        public string Nome { get; set; }
        public decimal Preco { get; set; }
    }
}
```

### 6. **Criar o DbContext**
Em seguida, crie uma classe que herda de `DbContext`, que será responsável por gerenciar as entidades e o banco de dados. Crie uma nova pasta chamada `Data` e adicione o `AppDbContext`.

```csharp
// Data/AppDbContext.cs
using Microsoft.EntityFrameworkCore;
using MeuProjetoAPI.Models;

namespace MeuProjetoAPI.Data
{
    public class AppDbContext : DbContext
    {
        public AppDbContext(DbContextOptions<AppDbContext> options) : base(options) { }

        public DbSet<Produto> Produtos { get; set; }
    }
}
```

### 7. **Configurar o DbContext no `Startup` (ou `Program.cs`)**
Adicione o `DbContext` ao container de injeção de dependência e configure a string de conexão no `Program.cs` (ou no `Startup.cs` se estiver utilizando versões anteriores).

No arquivo `appsettings.json`, configure a string de conexão do banco de dados. Para SQL Server, ficaria assim:

```json
{
  "ConnectionStrings": {
    "DefaultConnection": "Server=(localdb)\\mssqllocaldb;Database=MeuBancoDeDados;Trusted_Connection=True;"
  }
}
```

Em `Program.cs`, adicione a configuração do `DbContext`:

```csharp
// Program.cs
using Microsoft.EntityFrameworkCore;
using MeuProjetoAPI.Data;

var builder = WebApplication.CreateBuilder(args);

// Adiciona o DbContext com a string de conexão
builder.Services.AddDbContext<AppDbContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection")));

var app = builder.Build();

// Demais configurações...
app.Run();
```

### 8. **Adicionar Migrations e Atualizar o Banco de Dados**
Agora que o Entity Framework está configurado, você pode criar uma migration para gerar o banco de dados. Primeiro, adicione a migration:

```bash
dotnet ef migrations add InitialCreate
```

Isso criará uma migration chamada `InitialCreate`, que irá configurar o banco de dados com base no modelo definido no `DbContext`.

Em seguida, aplique a migration para criar o banco de dados:

```bash
dotnet ef database update
```

### 9. **Criar o Controller**
Agora, crie um controller para expor a entidade `Produto` na API. Adicione um `ProdutoController` na pasta `Controllers`.

```csharp
// Controllers/ProdutoController.cs
using Microsoft.AspNetCore.Mvc;
using MeuProjetoAPI.Data;
using MeuProjetoAPI.Models;
using Microsoft.EntityFrameworkCore;

namespace MeuProjetoAPI.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class ProdutoController : ControllerBase
    {
        private readonly AppDbContext _context;

        public ProdutoController(AppDbContext context)
        {
            _context = context;
        }

        [HttpGet]
        public async Task<ActionResult<IEnumerable<Produto>>> GetProdutos()
        {
            return await _context.Produtos.ToListAsync();
        }

        [HttpGet("{id}")]
        public async Task<ActionResult<Produto>> GetProduto(int id)
        {
            var produto = await _context.Produtos.FindAsync(id);

            if (produto == null)
            {
                return NotFound();
            }

            return produto;
        }

        [HttpPost]
        public async Task<ActionResult<Produto>> PostProduto(Produto produto)
        {
            _context.Produtos.Add(produto);
            await _context.SaveChangesAsync();

            return CreatedAtAction(nameof(GetProduto), new { id = produto.Id }, produto);
        }

        [HttpPut("{id}")]
        public async Task<IActionResult> PutProduto(int id, Produto produto)
        {
            if (id != produto.Id)
            {
                return BadRequest();
            }

            _context.Entry(produto).State = EntityState.Modified;

            try
            {
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!_context.Produtos.Any(e => e.Id == id))
                {
                    return NotFound();
                }
                else
                {
                    throw;
                }
            }

            return NoContent();
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteProduto(int id)
        {
            var produto = await _context.Produtos.FindAsync(id);
            if (produto == null)
            {
                return NotFound();
            }

            _context.Produtos.Remove(produto);
            await _context.SaveChangesAsync();

            return NoContent();
        }
    }
}
```

#### 10. **Executar o Projeto**
Agora, basta rodar o projeto:

```bash
dotnet run
```

Sua API estará disponível e conectada ao banco de dados via Entity Framework. Você pode testar as rotas do `ProdutoController` através de um cliente HTTP como o Postman ou o próprio Swagger, que está configurado por padrão.

