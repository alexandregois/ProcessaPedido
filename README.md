# Processa Pedido

Descrição do Projeto:<br><br>
O ProcessaPedido é uma API RESTful para gerenciamento de entregas/logística. <br><br>
O projeto segue boas práticas de arquitetura, processamento assíncrono, integração com fila (RabbitMQ), persistência em banco de dados (SQLite/EF Core) e testes robustos.<br><br>

Principais Funcionalidades:<br>
- API RESTful para criação e consulta de entregas.<br>
- Processamento assíncrono de status de entrega via fila (RabbitMQ/MassTransit).<br>
- Banco de dados SQLite para facilitar o setup local e testes.<br>
- Swagger para documentação e testes dos endpoints.<br>
- Testes de integração cobrindo o fluxo completo.<br>
- Arquitetura em camadas: Api, Application, Domain, Infrastructure, Tests - Visando uma maior separação das responsabilidades e para facilitar manutenções futuras.<br><br>

Arquitetura:<br>
- ProcessaPedido.Api: Camada de apresentação (controllers, autenticação, DI, Swagger).<br>
- ProcessaPedido.Application: Mensagens e contratos de aplicação.<br>
- ProcessaPedido.Domain: Entidades e enums de domínio.<br>
- ProcessaPedido.Infrastructure: Persistência (EF Core/SQLite) e mensageria (MassTransit/RabbitMQ).<br>
- ProcessaPedido.Tests: Testes de integração com WebApplicationFactory.<br><br>

Tecnologias Utilizadas:<br>
- .NET 8<br>
- MassTransit (mensageria) - framework open source .NET que encapsula várias funções, facilitando o desenvolvedor ao usar filas.<br>
- RabbitMQ (fila de mensagens)<br>
- Entity Framework Core (ORM)<br>
- SQLite (banco de dados local)<br>
- Swagger (estrutura e documentação)<br>
- xUnit (testes)<br>
- Docker Desktop (para RabbitMQ)<br><br>

Como Rodar o Projeto:<br>
1. Pré-requisitos:<br>
.NET 8 SDK<br>
Docker Desktop para Windows<br>
Git (opcional, para clonar o repositório)<br><br>
2. Subindo o RabbitMQ com Docker<br>
O projeto já inclui um arquivo docker-compose.yml, na raiz da solução, para facilitar o setup do RabbitMQ.<br><br>
No terminal, execute:<br>
docker-compose up -d<br><br>
Isso irá:
Baixar a imagem oficial do RabbitMQ com painel de administração.
Subir o serviço na porta padrão (5672) e painel web em `http://localhost:15672` (usuário/senha: guest/guest).<br><br>
3. Rodando a API<br>
No diretório do projeto, execute:<br><br>
dotnet build
dotnet run --project ProcessaPedido.Api<br><br>
A API estará disponível em http://localhost:5272 (ou porta configurada).<br><br>
4. Banco de Dados SQLite<br><br>
O projeto utiliza SQLite localmente, criando o arquivo entregas.db automaticamente na primeira execução.
Não é necessário instalar nada extra para o banco.
O uso do SQLite facilita o setup e permite rodar a aplicação e os testes sem dependências externas de banco.<br><br>
Se o banco não existir inicialmente:<br>
Intalar o EF Core Cli globalmente.<br>
dotnet tool install --global dotnet-ef<br><br>
Depois execute o comando para o Migration e geração das tabelas.<br>
dotnet ef database update --project ProcessaPedido.Infrastructure --startup-project ProcessaPedido.Api<br><br>


5. Acessando o Swagger<br><br>
Acesse http://localhost:5272/swagger para explorar e testar os endpoints da API.<br><br>

Testes:<br><br>
Para rodar os testes de integração:<br>
dotnet test<br><br>
Os testes cobrem o fluxo completo de criação e processamento de entregas, incluindo o uso da fila (RabbitMQ real ou in-memory, conforme configuração).<br><br>

Sobre o docker-compose.yml<br><br>
O arquivo docker-compose.yml está na raiz do projeto e define o serviço do RabbitMQ:<br><br>

version: '3.9'

services:<br>
  rabbitmq:<br>
    image: rabbitmq:3-management<br>
    container_name: fila-rabbitmq<br>
    ports:<br>
      - "5672:5672"<br>
      - "15672:15672"<br>
    environment:<br>
      RABBITMQ_DEFAULT_USER: guest<br>
      RABBITMQ_DEFAULT_PASS: guest<br>
    volumes:<br>
      - rabbitmq_data:/var/lib/rabbitmq<br>
<br>
volumes:<br>
  rabbitmq_data:<br><br><br>

Informações:<br>
5672: Porta de comunicação da aplicação com o RabbitMQ.<br>
15672: Painel web de administração.<br>
Usuário/senha: guest/guest (padrão).<br><br><br>
Persistência:<br>
Volume nomeado para manter dados mesmo após reiniciar o container.<br>
<br><br>
Observações e Dicas:<br>
O projeto está pronto para rodar tanto com fila em memória (para testes rápidos) quanto com RabbitMQ real (produção/integrado).<br><br>
Para alternar entre os modos, basta ajustar a configuração no Program.cs. - código comentado.<br><br>
O SQLite foi escolhido para facilitar o desenvolvimento e testes locais, sem necessidade de instalar um SGBD.<br><br>
O painel do RabbitMQ pode ser acessado em `http://localhost:15672` para monitorar filas, mensagens e consumidores.
